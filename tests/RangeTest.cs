/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using BTreeVisualization;
using Newtonsoft.Json;
using NodeData;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

namespace RangeOperationTesting
{
  /// <summary>
  /// Tests for threading properly ordered messages. 
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  // [TestFixture(3, 1000)]
  // [TestFixture(5, 1000)]
  [TestFixture(3, 10000)]
  // [TestFixture(5, 100000)]
  // [TestFixture(50, 1000000)]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  public partial class RangeTests(int degree, int numKeys)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
  {
    private BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
    private BufferBlock<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
    private List<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBufferHistory = [];
    private List<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBufferHistory = [];
    List<int> keys = [];
    private readonly int _NumberOfKeys = numKeys;
    private Task? _Producer;
    private Task? _Consumer;
    private BTree<Person> _Tree;

    private List<int> _OrderOfDeletion = [];
    private List<int> _BeginningTreeState = [];
    private bool _UseConstant = false;
    private readonly string _Path = ".\\rangeTesting.txt";

    /// <summary>
    /// NUnit setup for this class. 
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [SetUp]
    public void Setup()
    {
      _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
      _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
      _OutputBufferHistory = [];
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _Producer.Wait();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    [TearDown]
    public void Outro()
    {
      _Tree.Close();
    }

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

    private static void GetConstantSetup(string _Path, List<int> insertion, List<int> deletion)
    {
      try
      {
        string file = File.ReadAllText(_Path);
        foreach (Match number in NumberListRegex().Matches(InsertionRegex().Match(file).Value))
        {
          insertion.Add(int.Parse(number.Groups[0].Value));
        }
        foreach (Match number in NumberListRegex().Matches(DeletionRegex().Match(file).Value))
        {
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
    /// <returns></returns>
    private async Task TreeSetup()
    {
      Random random = new();
      int key = 0;
      //* Alternative Constant setup
      if (_UseConstant)
      {
        for (int i = 0; i < _BeginningTreeState.Count; i++)
        {
          key = _BeginningTreeState[i];
          await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1, new(key.ToString())));
          keys.Add(key);
        }
      }
      else
      {
        for (int i = 0; i < _NumberOfKeys; i++)
        {
          do
          {
            key = random.Next(1, _NumberOfKeys * 100);
          } while (keys.Contains(key));
          await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1, new(key.ToString())));
          keys.Add(key);
        }
      }
      await _InputBuffer.SendAsync((TreeCommand.Close, 0, -1, null));
    }

    [TestCase(1000)]
    [TestCase(10000)]
    [TestCase(10000)]
    [TestCase(10000)]
    [TestCase(10000)]
    [TestCase(10000)]
    [TestCase(10000)]
    [TestCase(10000)]
    public void BaseTest(int rangeSize)
    {
      Random random = new();
      int key, endKey, index, endIndex;
      List<(int key, Person content)> range = [];
      List<int> history = [], keyHistory = [];
      string insertionOrder = string.Join(',', keys);
      keys.Sort();
      string deleteHstory = "";
      while ((_UseConstant && _OrderOfDeletion.Count > 0) || (!_UseConstant && keys.Count > 0))
      {
        if (_UseConstant)
        {
          index = keys.IndexOf(_OrderOfDeletion[0]);
          key = _OrderOfDeletion[0];
        }
        else
        {
          if (keys.Count - 1 == 0)
            index = 0;
          else
            index = random.Next(1, keys.Count - 1);
          key = keys[index];
        }
        keyHistory.Add(key);
        endKey = key + rangeSize;
        for (endIndex = index; endIndex < keys.Count && keys[endIndex] >= key && keys[endIndex] < endKey; endIndex++)
        {
          range.Add((keys[endIndex], new(keys[endIndex].ToString())));
          history.Add(keys[endIndex]);
          deleteHstory += (deleteHstory.Length > 0 ? "," : "") + keys[endIndex];
        }
        for (int f = 0; f < range.Count; f++)
        {
          keys.RemoveAll(x => x == range[f].key);
          _OrderOfDeletion.RemoveAll(x => x == range[f].key);
        }
        for (int k = 0; k < keys.Count; k++)
        {
          var entry = _Tree.Search(keys[k]);
          if (entry == null)
          {
            if (!_UseConstant)
              SetConstantSetup(_Path, insertionOrder, deleteHstory);
            Assert.Fail($"The delete cycles started with {string.Join(',', keyHistory)}\nSearch turned up bogus for {keys[k]}\n");
          }
        }
        _Tree.DeleteRange(key, endKey);
        Assert.That(_Tree.Search(key, endKey), Is.Empty, $"The delete cycles started with {string.Join(',', keyHistory)}\nSearch shouldnt exist for {key}\n{deleteHstory}\n\n{insertionOrder}");
        range.Clear();
      }
    }

    /// <summary>
    /// Read out just the portion of the keys[] currently in use.
    /// </summary>
    /// <param name="numKeys">Index to stop at.</param>
    /// <param name="keys">Array of ints</param>
    /// <returns>String of the keys seperated by a ','</returns>
    private static string StringifyKeys(int numKeys, int[] keys)
    {
      string result = "";
      for (int i = 0; i < numKeys; i++)
        result += keys[i] + (i + 1 == numKeys ? "" : ",");
      return result;
    }

    [GeneratedRegex("Deletion:.+?$")]
    private static partial Regex DeletionRegex();
    [GeneratedRegex("Insertion:.+?\n")]
    private static partial Regex InsertionRegex();
    [GeneratedRegex(@"\b\d+\b")]
    private static partial Regex NumberListRegex();
  }
}