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
    Dictionary<NodeStatus,int>[] _StatusPrecedence = [];
    List<int> _AcceptStates = [];
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
    List<int> _InsertedKeys = [];
    // Dictionary<TreeCommand, int> _CommandCount = [];
    private readonly int _NumberOfKeys = 100000;
    private Task? _Producer;
    private Task? _Consumer;

    /// <summary>
    /// NUnit setup for this class. 
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [SetUp]
    public void Setup()
    {
      DefineStatusPrecedence();
      _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
      _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
      _OutputBufferHistory = [];
      _InputBufferHistory = [];
      _InsertedKeys = [];
      // _CommandCount = [];
      // foreach (TreeCommand treeCommand in Enum.GetValues(typeof(TreeCommand)))
      // {
      //   _CommandCount.Add(treeCommand, 0);
      // }
      _Producer = TreeProduce();
      _Consumer = GuiConsume();
    }

    /// <summary>
    /// Defines a structure for an NFA like processing of status messages.
    /// </summary>
    private void DefineStatusPrecedence()
    {
      int numOfStates = 24;
      _StatusPrecedence = new Dictionary<NodeStatus,int>[numOfStates];
      for(int i = 0; i < numOfStates; i++){
        _StatusPrecedence[i] = [];
      }
      _AcceptStates = [];
      // Node 0, Start
      int index = 0;
      _StatusPrecedence[index].Add(NodeStatus.Insert,1);
      _StatusPrecedence[index].Add(NodeStatus.Delete,6);
      _StatusPrecedence[index].Add(NodeStatus.Search,19);
      _StatusPrecedence[index].Add(NodeStatus.Close,23);
      // Node 1, Insert
      index++;
      _StatusPrecedence[index].Add(NodeStatus.ISearching,2);
      // Node 2, ISearching
      index++;
      _StatusPrecedence[index].Add(NodeStatus.ISearching,2);
      _StatusPrecedence[index].Add(NodeStatus.Inserted,3);
      // Node 3, Inserted, Accept
      index++;
      _AcceptStates.Add(index);
      _StatusPrecedence[index].Add(NodeStatus.Split,4);
      // Node 4, Split
      index++;
      _StatusPrecedence[index].Add(NodeStatus.Inserted,3);
      _StatusPrecedence[index].Add(NodeStatus.Shift,5);
      // Node 5, Shift, Accept
      index++;
      _AcceptStates.Add(index);
      _StatusPrecedence[index].Add(NodeStatus.Shift,5);
      _StatusPrecedence[index].Add(NodeStatus.Inserted,3);
      // Node 6, Delete
      index++;
      _StatusPrecedence[index].Add(NodeStatus.DSearching,7);
      // Node 7, DSearching
      index++;
      _StatusPrecedence[index].Add(NodeStatus.DSearching,7);
      _StatusPrecedence[index].Add(NodeStatus.Deleted,8);
      _StatusPrecedence[index].Add(NodeStatus.FSearching,9);
      // Node 8, Deleted, Accept
      index++;
      _AcceptStates.Add(index);
      _StatusPrecedence[index].Add(NodeStatus.UnderFlow,10);
      _StatusPrecedence[index].Add(NodeStatus.Merge,11);
      // Node 9, FSearching
      index++;
      _StatusPrecedence[index].Add(NodeStatus.FSearching,9);
      _StatusPrecedence[index].Add(NodeStatus.Forfeit,12);
      // Node 10, Deleted -> UnderFlow, Accept
      index++;
      _AcceptStates.Add(index);
      _StatusPrecedence[index].Add(NodeStatus.UnderFlow,10);
      _StatusPrecedence[index].Add(NodeStatus.Shift,17);
      // Node 11, Deleted -> Merge
      index++;
      _StatusPrecedence[index].Add(NodeStatus.MergeParent,18);
      // Node 12, Forfeit
      index++;
      _StatusPrecedence[index].Add(NodeStatus.Deleted,8);
      _StatusPrecedence[index].Add(NodeStatus.Merge,13);
      _StatusPrecedence[index].Add(NodeStatus.UnderFlow,15);
      // Node 13, Forfeit -> Merge
      index++;
      _StatusPrecedence[index].Add(NodeStatus.MergeParent,14);
      // Node 14, Forfeit -> Merge -> MergeParent
      index++;
      _StatusPrecedence[index].Add(NodeStatus.Deleted,8);
      _StatusPrecedence[index].Add(NodeStatus.Merge,13);
      _StatusPrecedence[index].Add(NodeStatus.UnderFlow,15);
      // Node 15, Forfeit -> UnderFlow
      index++;
      _StatusPrecedence[index].Add(NodeStatus.Deleted,8);
      _StatusPrecedence[index].Add(NodeStatus.Shift,16);
      // Node 16, Forfeit -> UnderFlow -> Shift
      index++;
      _StatusPrecedence[index].Add(NodeStatus.Deleted,8);
      // Node 17, Deleted -> UnderFlow -> Shift, Accept
      index++;
      _AcceptStates.Add(index);
      _StatusPrecedence[index].Add(NodeStatus.UnderFlow,10);
      // Node 18, Deleted -> Merge -> MergeParent, Accept
      index++;
      _AcceptStates.Add(index);
      _StatusPrecedence[index].Add(NodeStatus.Merge,11);
      _StatusPrecedence[index].Add(NodeStatus.UnderFlow,10);
      // Node 19, Search
      index++;
      _StatusPrecedence[index].Add(NodeStatus.SSearching,20);
      // Node 20, SSearching
      index++;
      _StatusPrecedence[index].Add(NodeStatus.SSearching,20);
      _StatusPrecedence[index].Add(NodeStatus.Found,21);
      _StatusPrecedence[index].Add(NodeStatus.FoundRange,22);
      // Node 21, Found, Accept
      index++;
      _AcceptStates.Add(index);
      // Node 22, FoundRange, Accept
      index++;
      _AcceptStates.Add(index);
      // Node 23, Close, Accept
      index++;
      _AcceptStates.Add(index);
    }

    /// <summary>
    /// Task creation for the tree object.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the tree.</returns>
    private async Task TreeProduce()
    {
      BTree<Person> _Tree = new(degree, _OutputBuffer);
      Random random = new();
      int key = 0;
      for (int i = 0; i < _NumberOfKeys; i++)
      {
        int j = 0;
        do
        {
          key = random.Next(1, _NumberOfKeys * 10);
        } while (_InsertedKeys.Contains(key) && ++j < _NumberOfKeys);
        _Tree.Insert(key, new Person(key.ToString()));
        _InsertedKeys.Add(key);
      }
      _Tree.Search(key);
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
          case TreeCommand.SearchRange:
            _Tree.Search(_InputBufferHistory.Last().key, _InputBufferHistory.Last().endKey);
            break;
          case TreeCommand.Traverse:
            Console.Write(_Tree.Traverse());
            break;
          case TreeCommand.Close:
            _InputBuffer.Complete();
            _Tree.Close();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            Console.Write("TreeCommand:{0} not recognized", _InputBufferHistory.Last().action);
            break;
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        // if (_CommandCount.ContainsKey(_InputBufferHistory.Last().action))
        //   _CommandCount[_InputBufferHistory.Last().action]++;
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
          case NodeStatus.FoundRange:
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
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    public async Task PresedenceTesting(int x)
    {
      Random random = new();
      int key;
      List<(int, int)> mixKeys = [];
      for (int i = 0; i < x; i++)
      {
        key = random.Next(0, 4);
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
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.SearchRange, key, key + 10
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
      NodeStatus lastStatus = NodeStatus.Close;
      int k = -1;
      int state = 0;
      _OutputBufferHistory.ForEach((bufMessage) =>
      {
        // if (bufMessage.status == NodeStatus.FoundRange)
        // {
        //   Console.WriteLine("Here:" + StringifyKeys(bufMessage.numKeys,bufMessage.keys));
        // }
        if(_StatusPrecedence[state].ContainsKey(bufMessage.status))
        {
          state = _StatusPrecedence[state][bufMessage.status];
        }
        else if(_AcceptStates.Contains(state) && _StatusPrecedence[0].ContainsKey(bufMessage.status))
        {
          state = _StatusPrecedence[0][bufMessage.status];
        }
        else
        {
          Assert.Fail($"State:{state}\n " +
            $"Status:{bufMessage.status}\n " +
            $"Last Status:{lastStatus}\nid: " +
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

    // [TestCase(100, 1000)]
    // public void RangeSearch(int x, int y)
    // {

    // }
  }
}