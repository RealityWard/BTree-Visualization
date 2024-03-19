/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace ThreadTesting
{
  /// <summary>
  /// Tests for threading properly ordered messages. 
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  [TestFixture(3)]
  public partial class ThreadTests(int degree)
  {
    private readonly int[][] _StatusPrecedence = [
      [01,-1,-1,-1,06,-1,-1,-1,-1,-1,-1,-1,-1,19,-1,-1,-1,23,-1],
      [02,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,02,03,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,04,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,05,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,05,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,07,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,07,08,13,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,11,-1,09,-1,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,10,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,12,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,-1,-1,13,14,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,08,-1,-1,17,-1,15,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,16,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,08,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,18,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,08,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,20,-1,-1,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,20,21,22,-1,-1],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,00],
      [-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,00]
    ];
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
    private BTree<Person>? _Tree;
    List<int> _InsertedKeys = [];
    List<string> _TraverseHistory = [];
    Dictionary<TreeCommand, int> _CommandCount = [];
    private readonly int _NumberOfKeys = 100000;
    private Task? _Producer;
    private Task? _Consumer;
    private int? _LastSetupKey;

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
      _TraverseHistory = [];
      _InsertedKeys = [];
      _CommandCount = [];
      foreach (TreeCommand treeCommand in Enum.GetValues(typeof(TreeCommand)))
      {
        _CommandCount.Add(treeCommand, 0);
      }
      _Tree = new(degree, _OutputBuffer);
      _Producer = TreeProduce();
      _Consumer = GuiConsume();
      Random random = new();
      int key = 0;
      for (int i = 0; i < _NumberOfKeys; i++)
      {
        do
        {
          key = random.Next(1, _NumberOfKeys * 10);
        } while (_InsertedKeys.Contains(key));
        _Tree.Insert(key, new Person(key.ToString()));
        _InsertedKeys.Add(key);
      }
      _LastSetupKey = key;
      _Tree.Search(key);
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
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          case TreeCommand.Tree:
            _Tree = new(_InputBufferHistory.Last().key, _OutputBuffer);
            break;
          case TreeCommand.Insert:
            _Tree.Insert(_InputBufferHistory.Last().key, _InputBufferHistory.Last().content);
            break;
          case TreeCommand.Delete:
            _Tree.Delete(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.Search:
            _Tree.Search(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.Traverse:
            Console.Write(_Tree.Traverse());
            break;
          case TreeCommand.Close:
            await _OutputBuffer.SendAsync((NodeStatus.Close, 0, -1, [], [], 0, -1, [], [])).ConfigureAwait(false);
            _InputBuffer.Complete();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            _InputBuffer.Complete();
            Console.Write("TreeCommand:{0} not recognized", _InputBufferHistory.Last().action);
            break;
        }
        _TraverseHistory.Add(_Tree.Traverse());
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        if (_CommandCount.ContainsKey(_InputBufferHistory.Last().action))
          _CommandCount[_InputBufferHistory.Last().action]++;
      }
    }

    /// <summary>
    /// Task creation for the fake consumer "GUI" object.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the GUI consumer.</returns>
    private async Task GuiConsume()
    {
      bool setupConsumed = false;
      while (await _OutputBuffer.OutputAvailableAsync() && !setupConsumed)
      {
        (NodeStatus status, long id, int numKeys, int[] keys
          , Person?[] contents, long altID, int altNumKeys, int[] altKeys
          , Person?[] altContents) = _OutputBuffer.Receive();
        setupConsumed = status == NodeStatus.Found;
      }
      while (await _OutputBuffer.OutputAvailableAsync())
      {
        _OutputBufferHistory.Add(_OutputBuffer.Receive());
        switch (_OutputBufferHistory.Last().status)
        {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          case NodeStatus.Insert:
            break;
          case NodeStatus.ISearching:
            break;
          case NodeStatus.Inserted:
            break;
          case NodeStatus.Split:
            break;
          case NodeStatus.Delete:
            break;
          case NodeStatus.DSearching:
            break;
          case NodeStatus.Deleted:
            break;
          case NodeStatus.FSearching:
            break;
          case NodeStatus.Forfeit:
            break;
          case NodeStatus.Merge:
            break;
          case NodeStatus.MergeParent:
            break;
          case NodeStatus.UnderFlow:
            break;
          case NodeStatus.Shift:
            break;
          case NodeStatus.Search:
            break;
          case NodeStatus.SSearching:
            break;
          case NodeStatus.Found:
            break;
          case NodeStatus.Close:
            _OutputBuffer.Complete();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            Console.Write("TreeCommand:{0} not recognized", _OutputBufferHistory.Last().status);
            break;
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
      }
    }

    /// <summary>
    /// Simply test insertion times
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [TestCase(10)]
    [TestCase(10)]
    [TestCase(10)]
    [TestCase(10)]
    public async Task PresedenceTesting(int x)
    {
      Random random = new();
      int key;
      List<(int, int)> mixKeys = [];
      for (int i = 0; i < _NumberOfKeys / 10; i++)
      {
        key = random.Next(0, 3);
        // Randomly choose insert, delete, or search
        if (key == 1)
        {
          do
          {
            key = random.Next(1, _NumberOfKeys * 10);
          } while (_InsertedKeys.Contains(key));
          await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1
            , new Person(key.ToString())));
          _InsertedKeys.Add(key);
          mixKeys.Add((1, key));
        }
        else if (key == 2)
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.Delete, key, -1
            , null));
          _InsertedKeys.Remove(key);
          mixKeys.Add((0, key));
        }
        else
        {
          do
          {
            key = random.Next(1, _NumberOfKeys * 10);
          } while (_InsertedKeys.Contains(key));
          await _InputBuffer.SendAsync((TreeCommand.Search, key, key + 10
            , null));
        }
      }
      await _InputBuffer.SendAsync((TreeCommand.Close, 0, -1, null));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _Producer.Wait();
      _Consumer.Wait();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      Assert.That(_InputBufferHistory.Count(), Is.EqualTo(x + 1), "Not all commands are getting through.");
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      // Console.Write(traversal);
      int traverseIndex = 0;
      string? treeTraverse = _TraverseHistory[traverseIndex];
      NodeStatus lastStatus = NodeStatus.Close;
      int k = -1;
      int state = 0;
      _OutputBufferHistory.ForEach((bufMessage) =>
      {
        if (bufMessage.status == NodeStatus.Insert)
        {
          treeTraverse = _TraverseHistory[++traverseIndex];
          k = bufMessage.keys[0];
        }
        state = _StatusPrecedence[state][(int)bufMessage.status] != -1 ?
          _StatusPrecedence[state][(int)bufMessage.status] : _StatusPrecedence[state][^1];
        if (state == -1)
        {
          Assert.Fail($"Status:{bufMessage.status}\n " +
            $"Last Status:{lastStatus}\nTraversal " +
            $"out of order:{treeTraverse}\nid:" +
            $"{bufMessage.id}\nkey:{k}\nkeys:" +
            $"{StringifyKeys(bufMessage.numKeys, bufMessage.keys)}");
        }
        lastStatus = bufMessage.status;
      });
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

    [TestCase(100, 1000)]
    public void RangeSearch(int x, int y)
    {

    }
  }
}