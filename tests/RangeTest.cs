/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using BTreeVisualization;
using NodeData;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace RangeOperationTesting
{
  /// <summary>
  /// Tests for threading properly ordered messages. 
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  [TestFixture(3, 10000)]
  [TestFixture(5, 10000)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  public partial class RangeTests(int degree, int numKeys)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  {
    private BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
    private BufferBlock<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
    private List<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBufferHistory = [];
    List<int> keys = [];
    /// <summary>
    /// If not _UseConstant then fill the tree
    /// with a number of random keys equal to
    /// this.
    /// </summary>
    private readonly int _NumberOfKeys = numKeys;
    /// <summary>
    /// Used to track the Producer thread.
    /// </summary>
    private Task? _Producer;
#pragma warning disable IDE0052 // Remove unread private members
    /// <summary>
    /// Used to track the Consumer thread.
    /// </summary>
    private Task? _Consumer;
#pragma warning restore IDE0052 // Remove unread private members
    private BTree<Person> _Tree;
    /// <summary>
    /// Filled if _UseConstant
    /// </summary>
    private List<int> _OrderOfDeletion = [];
    /// <summary>
    /// Filled if _UseConstant
    /// </summary>
    private List<int> _BeginningTreeState = [];
    /// <summary>
    /// Set to true to load insertion and deletion
    /// order from file at _Path.
    /// Else the tests will randomly generate
    /// insertion and deletion order.
    /// </summary>
    private readonly bool _UseConstant = false;
    private readonly string _Path = ".\\rangeTesting.txt";

    /// <summary>
    /// NUnit setup for this class.
    /// Provides a clean slate between tests.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [SetUp]
    public void Setup()
    {
      _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
      _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
      _InputBufferHistory = [];
      keys = [];
      _OrderOfDeletion = [];
      _BeginningTreeState = [];
      if (_UseConstant)
      {
        GetConstantSetup(_Path, _BeginningTreeState, _OrderOfDeletion);
      }
      _Tree = new(degree, _OutputBuffer);
      _Producer = TreeProduce();
      _Consumer = GuiConsume();
      _ = TreeSetup();
      _Producer.Wait();
    }

    /// <summary>
    /// Just to be certain _Tree is reset before the next run.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [TearDown]
    public void Outro()
    {
      _Tree.Close();
    }

    /// <summary>
    /// Supposed to put the last used insertion order and deletion
    /// order into a text file at _Path. This by default will be
    /// placed into ".\BTree-Visualization\tests\bin\Debug\net8.0\"
    /// However, this is buggy as it seems to not like running from
    /// the NUnit test files.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="_Path">Location and name of file.</param>
    /// <param name="insertion">The string of insertions in order of insertion</param>
    /// <param name="deletion">The string of deletions in order of deletion</param>
    private static void SetConstantSetup(string _Path, string insertion, string deletion)
    {
      try
      {
        using (FileStream fs = File.OpenWrite(_Path))
        {
          // Putting some contents
          Byte[] info = new UTF8Encoding(true).GetBytes($"Insertion:{insertion}\nDeletion:{deletion}");
          fs.Write(info, 0, info.Length);
        }
      }
      catch { }
    }

    /// <summary>
    /// Supposed to get the last used insertion order and deletion
    /// order into a text file at _Path. This by default will be
    /// placed into ".\BTree-Visualization\tests\bin\Debug\net8.0\"
    /// It parses the file using Regex to fill the list objects
    /// that the rest of this files' tests use.
    /// insertion is filled from the line beginning with "Insertion:"
    /// deletion is filled from the line beginning with "Deletion:"
    /// both are followed by numbers seperated by ',' with
    /// no whitespace.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="_Path">Location and name of file.</param>
    /// <param name="insertion">The list of insertions in order of insertion</param>
    /// <param name="deletion">The list of deletions in order of deletion</param>
    private static void GetConstantSetup(string _Path, List<int> insertion, List<int> deletion)
    {
      try
      {
        string file = File.ReadAllText(_Path);
        foreach (Match number in NumberListRegex().Matches(InsertionRegex().Match(file).Value))
        {// Parses on the line beginning with "Insertion:"
          insertion.Add(int.Parse(number.Groups[0].Value));
        }
        foreach (Match number in NumberListRegex().Matches(DeletionRegex().Match(file).Value))
        {// Parses on the line beginning with "Deletion:"
          deletion.Add(int.Parse(number.Groups[0].Value));
        }
      }
      catch { }
    }

    /// <summary>
    /// Task creation for the tree object.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the tree.</returns>
    private async Task TreeProduce()
    {
      while (await _InputBuffer.OutputAvailableAsync())
      {
        _InputBufferHistory.Add(_InputBuffer.Receive());
        switch (_InputBufferHistory.Last().action)
        {
#pragma warning disable CS8604 // Possible null reference argument.
          case TreeCommand.Tree:
            _Tree = new(_InputBufferHistory.Last().key, _OutputBuffer);
            break;
          case TreeCommand.Insert:
            _Tree.Insert(_InputBufferHistory.Last().key, _InputBufferHistory.Last().content);
            break;
          case TreeCommand.Delete:
            _Tree.Delete(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.DeleteRange:
            _Tree.DeleteRange(_InputBufferHistory.Last().key, _InputBufferHistory.Last().endKey);
            break;
          case TreeCommand.Search:
            _Tree.Search(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.SearchRange:
            _Tree.Search(_InputBufferHistory.Last().key, _InputBufferHistory.Last().endKey);
            break;
          case TreeCommand.Traverse:
            Console.Write(_Tree.Traverse());
            break;
          case TreeCommand.Close:
            _InputBuffer.Complete();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            Console.Write("TreeCommand:{0} not recognized", _InputBufferHistory.Last().action);
            break;
        }
        _InputBufferHistory.Clear();
#pragma warning restore CS8604 // Possible null reference argument.
      }
    }

    /// <summary>
    /// Task creation for the fake consumer "GUI" object.
    /// Set to consume all the output from the tree object
    /// to prevent filling up the _OutputBuffer.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the GUI consumer.</returns>
    private async Task GuiConsume()
    {
      while (await _OutputBuffer.OutputAvailableAsync())
      {
        if (_OutputBuffer.Receive().status == NodeStatus.Close)
          _OutputBuffer.Complete();
      }
    }

    /// <summary>
    /// Part of setup to fill the tree object then close its thread.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>The task that fills the tree.</returns>
    private async Task TreeSetup()
    {
      Random random = new();
      int key;
      if (_UseConstant)
      {// Filled from file
        for (int i = 0; i < _BeginningTreeState.Count; i++)
        {
          key = _BeginningTreeState[i];
          await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1, new(key.ToString())));
          keys.Add(key);
        }
      }
      else
      {// Filled with random numbers
        for (int i = 0; i < _NumberOfKeys; i++)
        {
          do
          {// Make certain of unique keys
            key = random.Next(1, _NumberOfKeys * 100);
          } while (keys.Contains(key));
          await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1, new(key.ToString())));
          keys.Add(key);
        }
      }
      await _InputBuffer.SendAsync((TreeCommand.Close, 0, -1, null));
    }

    /// <summary>
    /// Runs DeleteRange on the tree a number of times.
    /// Warning this is a slow test in the realm of minutes.
    /// My system takes 7 minutes and it isn't a
    /// trash system. However, this is a very through test
    /// with random generation it can find almost any
    /// existing problem with DeleteRange.
    /// Each run uses a key from the tree that was
    /// previously inserted during setup. It sets the
    /// range by adding rangeSize to the key selected.
    /// After deletion, it tests to see if any of
    /// the range still exists. Then before each
    /// DeleteRange is called it checks every other
    /// entry not deleted yet is still searchable
    /// in the tree.
    /// Operates with random unless _UseConstant
    /// is true. If _UseConstant is true
    /// refer to GetConstantSetup().
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="rangeSize">Sets the key range
    /// per DeleteRange</param>
    /// <param name="countOfDeleteRanges">Number of
    /// times DeleteRange is run on the tree.</param>
    [TestCase(1000, 1000)]
    [TestCase(10000, 1000)]
    [TestCase(10000, 1000)]
    public void BaseRangeDeletionTest(int rangeSize, int countOfDeleteRanges)
    {
      Random random = new();
      int key, endKey, index, endIndex;
      List<(int key, Person content)> range = [];
      List<int> history = [], keyHistory = [];
      string insertionOrder = string.Join(',', keys);
      keys.Sort();
      string deleteHstory = "";
      while (((_UseConstant && _OrderOfDeletion.Count > 0) || (!_UseConstant && keys.Count > 0)) && countOfDeleteRanges >= 0)
      {
        // Limit the runs to save on time though this means
        // it won't run until the tree is empty.
        // If not limited the later iterations will delete fewer
        // and fewer keys until it is only being run for one entry at a time.
        countOfDeleteRanges--;
        if (_UseConstant)
        {// Constant mode
          index = keys.IndexOf(_OrderOfDeletion[0]);
          key = _OrderOfDeletion[0];
        }
        else
        {// Normal mode
          if (keys.Count - 1 == 0)
            index = 0;// Just in case
          else
            index = random.Next(1, keys.Count - 1);
          key = keys[index];
        }
        keyHistory.Add(key);
        endKey = key + rangeSize;
        for (endIndex = index; endIndex < keys.Count && keys[endIndex] >= key && keys[endIndex] < endKey; endIndex++)
        {// Track all keys being deleted from the tree for easier debugging when there is a crash or failed test.
          range.Add((keys[endIndex], new(keys[endIndex].ToString())));
          history.Add(keys[endIndex]);
          deleteHstory += (deleteHstory.Length > 0 ? "," : "") + keys[endIndex];
        }
        for (int k = 0; k < keys.Count; k++)
        {// Search for each entry individually before moving on to the
          // delete just in case something was wrong with insertion or
          // the last DeleteRange deleted something it shouldn't have
          var entry = _Tree.Search(keys[k]);
          if (entry == null)
          {
            if (!_UseConstant)// To make certain not to mess with current Constant setup.
              SetConstantSetup(_Path, insertionOrder, deleteHstory);
            // The below Console line is useful if you can't use the
            // debugger to copy out the lists to the file for _UseConstant mode.
            // Console.WriteLine($"Deleting: {key} - {endKey}" +
            // $"\nInsertion:{insertionOrder}\nDeletion:{deleteHstory}");
            Assert.Fail($"The delete cycles started with {string.Join(',', keyHistory)}\nSearch turned up bogus for {keys[k]}\n");
          }
        }
        for (int f = 0; f < range.Count; f++)
        {// Update list for the keys about to be deleted
          keys.RemoveAll(x => x == range[f].key);
          _OrderOfDeletion.RemoveAll(x => x == range[f].key);
        }
        _Tree.DeleteRange(key, endKey);
        Assert.That(_Tree.Search(key, endKey), Is.Empty, 
          $"The delete cycles started with {string.Join(',', keyHistory)}\n" +
          $"Search shouldnt exist for {key}\n{deleteHstory}\n\n{insertionOrder}");
        range.Clear();
      }
    }

    /// <summary>
    /// Just testing that it can delete every entry
    /// in a single run correctly.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [Test]
    public void DeleteAllAtOnce()
    {
      string insertionOrder = string.Join(',', keys);
      keys.Sort();
      _Tree.DeleteRange(keys[0], keys[^1] + 1);
      for (int k = 0; k < keys.Count; k++)
      {
        var entry = _Tree.Search(keys[k]);
        if (entry != null)
        {
          Assert.Fail($"Search found {keys[k]}\n");
        }
      }
      Assert.That(_Tree.Search(keys[0], keys[^1] + 1), Is.Empty, $"Insertion:{insertionOrder}");
    }

    /// <summary>
    /// Tests if it correctly deletes everything but the greatest entry.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [Test]
    public void DeleteAllExceptGreatestOne()
    {
      string insertionOrder = string.Join(',', keys);
      keys.Sort();
      _Tree.DeleteRange(keys[0], keys[^1]);
      for (int k = 0; k < keys.Count - 1; k++)
      {
        var entry = _Tree.Search(keys[k]);
        if (entry != null)
        {
          Assert.Fail($"Search found {keys[k]}\n");
        }
      }
      var singleEntry = _Tree.Search(keys[^1]);
      if (singleEntry == null)
      {
        Assert.Fail($"Search found {keys[^1]}\n");
      }
      Assert.That(_Tree.Search(keys[0], keys[^1]), Is.Empty, $"Insertion:{insertionOrder}");
    }

    /// <summary>
    /// Tests if it correctly deletes everything but the least entry.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [Test]
    public void DeleteAllExceptLeastOne()
    {
      string insertionOrder = string.Join(',', keys);
      keys.Sort();
      _Tree.DeleteRange(keys[0] + 1, keys[^1] + 1);
      for (int k = 1; k < keys.Count; k++)
      {
        var entry = _Tree.Search(keys[k]);
        if (entry != null)
        {
          Assert.Fail($"Search found {keys[k]}\n");
        }
      }
      var singleEntry = _Tree.Search(keys[0]);
      if (singleEntry == null)
      {
        Assert.Fail($"Search found {keys[0]}\n");
      }
      Assert.That(_Tree.Search(keys[0] + 1, keys[^1] + 1), Is.Empty, $"Insertion:{insertionOrder}");
    }

    /// <summary>
    /// Generating the Regex patterns for reading from the file
    /// if _UseConstant is true.
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex("Deletion:.+?$")]
    private static partial Regex DeletionRegex();
    [GeneratedRegex("Insertion:.+?\n")]
    private static partial Regex InsertionRegex();
    [GeneratedRegex(@"\b\d+\b")]
    private static partial Regex NumberListRegex();
  }
}