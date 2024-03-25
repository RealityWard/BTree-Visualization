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
    Dictionary<NodeStatus, int>[] _StatusPrecedence = [];
    int[] _AcceptStates = [];
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
      int numOfStates = 53;
      _StatusPrecedence = new Dictionary<NodeStatus, int>[numOfStates];
      for (int i = 0; i < numOfStates; i++)
      {
        _StatusPrecedence[i] = [];
      }
      _StatusPrecedence[0].Add(NodeStatus.Insert, 1);
      _StatusPrecedence[0].Add(NodeStatus.Delete, 15);
      _StatusPrecedence[0].Add(NodeStatus.Search, 29);
      _StatusPrecedence[0].Add(NodeStatus.SearchRange, 32);
      _StatusPrecedence[0].Add(NodeStatus.DeleteRange, 35);
      _AcceptStates = [0, 53, 3, 7, 14, 17, 18, 19, 21, 31, 34, 37, 39, 44, 52];
      _StatusPrecedence[0].Add(NodeStatus.Close, 53);
      _StatusPrecedence[1].Add(NodeStatus.ISearching, 2);
      _StatusPrecedence[2].Add(NodeStatus.ISearching, 2);
      _StatusPrecedence[2].Add(NodeStatus.Inserted, 3);
      _StatusPrecedence[3].Add(NodeStatus.Split, 4);
      _StatusPrecedence[4].Add(NodeStatus.SplitResult, 5);
      _StatusPrecedence[5].Add(NodeStatus.SplitResult, 6);
      _StatusPrecedence[6].Add(NodeStatus.SplitInsert, 7);
      _StatusPrecedence[7].Add(NodeStatus.Split, 8);
      _StatusPrecedence[8].Add(NodeStatus.SplitResult, 9);
      _StatusPrecedence[9].Add(NodeStatus.SplitResult, 10);
      _StatusPrecedence[10].Add(NodeStatus.Shift, 11);
      _StatusPrecedence[11].Add(NodeStatus.Shift, 11);
      _StatusPrecedence[11].Add(NodeStatus.SplitInsert, 7);
      _StatusPrecedence[11].Add(NodeStatus.NewRoot, 12);
      _StatusPrecedence[12].Add(NodeStatus.Shift, 13);
      _StatusPrecedence[13].Add(NodeStatus.Shift, 14);
      _StatusPrecedence[15].Add(NodeStatus.DSearching, 16);
      _StatusPrecedence[16].Add(NodeStatus.DSearching, 16);
      _StatusPrecedence[16].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[16].Add(NodeStatus.FSearching, 23);
      _StatusPrecedence[17].Add(NodeStatus.UnderFlow, 18);
      _StatusPrecedence[17].Add(NodeStatus.Merge, 20);
      _StatusPrecedence[18].Add(NodeStatus.Shift, 19);
      _StatusPrecedence[20].Add(NodeStatus.MergeParent, 21);
      _StatusPrecedence[21].Add(NodeStatus.Merge, 20);
      _StatusPrecedence[21].Add(NodeStatus.UnderFlow, 22);
      _StatusPrecedence[22].Add(NodeStatus.Shift, 19);
      _StatusPrecedence[23].Add(NodeStatus.FSearching, 23);
      _StatusPrecedence[23].Add(NodeStatus.Forfeit, 24);
      _StatusPrecedence[24].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[24].Add(NodeStatus.UnderFlow, 25);
      _StatusPrecedence[24].Add(NodeStatus.Merge, 26);
      _StatusPrecedence[25].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[26].Add(NodeStatus.MergeParent, 27);
      _StatusPrecedence[27].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[27].Add(NodeStatus.Merge, 26);
      _StatusPrecedence[27].Add(NodeStatus.UnderFlow, 28);
      _StatusPrecedence[28].Add(NodeStatus.Shift, 25);
      _StatusPrecedence[29].Add(NodeStatus.SSearching, 30);
      _StatusPrecedence[30].Add(NodeStatus.SSearching, 30);
      _StatusPrecedence[30].Add(NodeStatus.Found, 31);
      _StatusPrecedence[32].Add(NodeStatus.SSearching, 33);
      _StatusPrecedence[33].Add(NodeStatus.SSearching, 33);
      _StatusPrecedence[33].Add(NodeStatus.FoundRange, 34);
      _StatusPrecedence[35].Add(NodeStatus.DSearching, 36);
      _StatusPrecedence[36].Add(NodeStatus.DSearching, 36);
      _StatusPrecedence[36].Add(NodeStatus.DeletedRange, 37);
      _StatusPrecedence[37].Add(NodeStatus.Merge, 38);
      _StatusPrecedence[38].Add(NodeStatus.MergeParent, 39);
      _StatusPrecedence[37].Add(NodeStatus.UnderFlow, 40);
      _StatusPrecedence[37].Add(NodeStatus.DSearching, 41);
      _StatusPrecedence[39].Add(NodeStatus.Merge, 38);
      _StatusPrecedence[39].Add(NodeStatus.UnderFlow, 40);
      _StatusPrecedence[40].Add(NodeStatus.Shift, 37);
      _StatusPrecedence[40].Add(NodeStatus.DSearching, 41);
      _StatusPrecedence[41].Add(NodeStatus.DSearching, 41);
      _StatusPrecedence[41].Add(NodeStatus.DeletedRange, 42);
      _StatusPrecedence[42].Add(NodeStatus.Merge, 43);
      _StatusPrecedence[42].Add(NodeStatus.UnderFlow, 45);
      _StatusPrecedence[42].Add(NodeStatus.Rebalanced, 46);
      _StatusPrecedence[42].Add(NodeStatus.NodeDeleted, 47);
      _StatusPrecedence[43].Add(NodeStatus.MergeParent, 44);
      _StatusPrecedence[44].Add(NodeStatus.Merge, 43);
      _StatusPrecedence[44].Add(NodeStatus.UnderFlow, 45);
      _StatusPrecedence[45].Add(NodeStatus.Shift, 42);
      _StatusPrecedence[45].Add(NodeStatus.Rebalanced, 46);
      _StatusPrecedence[45].Add(NodeStatus.NodeDeleted, 47);
      _StatusPrecedence[46].Add(NodeStatus.NodeDeleted, 47);
      _StatusPrecedence[46].Add(NodeStatus.DeletedRange, 48);
      _StatusPrecedence[47].Add(NodeStatus.DeletedRange, 48);
      _StatusPrecedence[48].Add(NodeStatus.Merge, 49);
      _StatusPrecedence[48].Add(NodeStatus.UnderFlow, 51);
      _StatusPrecedence[49].Add(NodeStatus.MergeParent, 50);
      _StatusPrecedence[50].Add(NodeStatus.Merge, 49);
      _StatusPrecedence[50].Add(NodeStatus.UnderFlow, 51);
      _StatusPrecedence[51].Add(NodeStatus.Shift, 48);
      _StatusPrecedence[51].Add(NodeStatus.DeletedRange, 52);
      _StatusPrecedence[52].Add(NodeStatus.DSearching, 41);
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
            // Console.Write("TreeCommand:{0} not recognized", _OutputBufferHistory.Last().status);
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
        if (key == 0)
        {
          for (int j = 0; j < 10; j++)
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
        }
        else if (key == 1)
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.Delete, key, -1
            , null));
          _InsertedKeys.Remove(key);
          mixKeys.Add((0, key));
        }
        else if (key == 2)
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.SearchRange, key, key + 10
            , null));
        }
        else
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.DeleteRange, key, key + 10
            , null));
          for (int l = 0; l < 10; l++)
          {
            _InsertedKeys.Remove(key);
            mixKeys.Add((0, key++));
          }
        }
      }
      await _InputBuffer.SendAsync((TreeCommand.Close, 0, -1, null));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _Producer.Wait();
      _Consumer.Wait();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      Assert.That(_InputBufferHistory, Has.Count.EqualTo(x + 1), "Not all commands are getting through.");
      NodeStatus lastStatus = NodeStatus.Close;
      int k = -1;
      int state = 0;
      _OutputBufferHistory.ForEach((bufMessage) =>
      {
        // if (bufMessage.status == NodeStatus.FoundRange)
        // {
        //   Console.WriteLine("Here:" + StringifyKeys(bufMessage.numKeys,bufMessage.keys));
        // }
        if (_StatusPrecedence[state].ContainsKey(bufMessage.status))
        {
          state = _StatusPrecedence[state][bufMessage.status];
        }
        else if (_AcceptStates.Contains(state) && _StatusPrecedence[0].ContainsKey(bufMessage.status))
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
  }
}