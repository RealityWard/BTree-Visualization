/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace RangeOperationTesting
{
  /// <summary>
  /// Tests for threading properly ordered messages. 
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  [TestFixture(3, 100000)]
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
    List<(int, Person)> _Entries = [];
    private readonly int _NumberOfKeys = numKeys;
    private Task? _Producer;
    private Task? _Consumer;
    private BTree<Person> _Tree;

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
      _Entries = [];
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
      Person person = new(key.ToString());
      for (int i = 0; i < _NumberOfKeys; i++)
      {
        do
        {
          key = random.Next(1, _NumberOfKeys * 1000);
          person = new(key.ToString());
        } while (_Entries.Contains((key, person)));
        await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1, person));
        _Entries.Add((key, person));
      }
      await _InputBuffer.SendAsync((TreeCommand.Close, 0, -1, null));
    }

    [TestCase(1)]
    public void BaseTest(int rangeSize)
    {
      Random random = new();
      int key, endKey, index, endIndex;
      List<(int key, Person content)> range = [];
      int numberOfKeysRemaining = _NumberOfKeys;
      _Entries.Sort((first, second) =>
      {
        return first.Item1.CompareTo(second.Item1);
      });
      while (numberOfKeysRemaining > 0)
      {
        index = random.Next(1, _Entries.Count - 1);
        key = _Entries[index].Item1;
        endKey = key + rangeSize;
        for (endIndex = index; endIndex < _Entries.Count && _Entries[endIndex].Item1 >= key && _Entries[endIndex].Item1 <= endKey; endIndex++)
        {
          range.Add(_Entries[endIndex]);
        }
        // Console.WriteLine(string.Join(',', range));
        // // Assert.That(_Tree.Search(key), Is.Not.Null);
        // Console.WriteLine($"After Count:{_Tree.Search(key, endKey).Count()}\n Range:{key} - {endKey}");
        Assert.That(_Tree.Search(key, endKey), Is.EqualTo(range));
        _Tree.DeleteRange(key, endKey);
        Assert.That(_Tree.Search(key, endKey), Is.Empty);
        _Entries.RemoveRange(index, range.Count);
        numberOfKeysRemaining -= range.Count;
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
  }
}