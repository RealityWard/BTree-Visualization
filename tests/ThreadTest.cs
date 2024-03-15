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
    private BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
    private BufferBlock<(TreeCommand action, int key
      , Person? content)> _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
    private List<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBufferHistory = [];
    private List<(TreeCommand action, int key
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
    public void InsertionTest(int x)
    {
      Random random = new();
      int key;
      for (int i = 0; i < x; i++)
      {
        do
        {
          key = random.Next(1, _NumberOfKeys * 10);
        } while (_InsertedKeys.Contains(key));
        if (!_InputBuffer.SendAsync((TreeCommand.Insert, key
          , new Person(key.ToString()))).Result)
          Assert.Fail("Rejected insert to the _InputBuffer");
        _InsertedKeys.Add(key);
      }
      if (!_InputBuffer.SendAsync((TreeCommand.Close, 0, null)).Result)
        Assert.Fail("Rejected close to the _InputBuffer");
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
      bool failed = false;
      _OutputBufferHistory.ForEach((bufMessage) => 
      {
        switch (bufMessage.status)
        {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          case NodeStatus.Insert:
            treeTraverse = _TraverseHistory[++traverseIndex];
            k = bufMessage.keys[0];
            break;
          case NodeStatus.ISearching:
            failed = !(lastStatus == NodeStatus.Insert || lastStatus == NodeStatus.ISearching);
            break;
          case NodeStatus.Inserted:
            failed = !(lastStatus == NodeStatus.ISearching || lastStatus == NodeStatus.Shift || lastStatus == NodeStatus.Split);
            break;
          case NodeStatus.Split:
            failed = lastStatus != NodeStatus.Inserted;
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
            failed = !(lastStatus == NodeStatus.Split || lastStatus == NodeStatus.Shift);
            break;
          case NodeStatus.Search:
            break;
          case NodeStatus.SSearching:
            break;
          case NodeStatus.Found:
            break;
          case NodeStatus.Close:
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            Assert.Fail($"TreeCommand:{bufMessage.status} not recognized");
            break;
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
        if(failed)
          Assert.Fail($"Last Status:{lastStatus}\nTraversal out of order:{treeTraverse}\nstatus:{bufMessage.status}\nid:{bufMessage.id}\nkey:{k}\nkeys:{StringifyKeys(bufMessage.numKeys,bufMessage.keys)}");
        lastStatus = bufMessage.status;
      });
    }

    private static string StringifyKeys(int numKeys, int[] keys){
      string result = "";
      for(int i = 0; i < numKeys; i++)
        result += keys[i] + (i + 1 == numKeys? "" : ",");
      return result;
    }

    /// <summary>
    /// Simply test deletion times
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Number of deletions</param>
    // [TestCase(1000)]
    // [TestCase(10000)]
    public void DeletionTest(int x)
    {
      int key;
      Random random = new();
      for (int i = 0; i < x; i++)
      {
        key = _InsertedKeys.ElementAt(random.Next(0, _InsertedKeys.Count));
        _InputBuffer.SendAsync((TreeCommand.Delete, key, null));
        _InsertedKeys.Remove(key);
      }
      _InputBuffer.SendAsync((TreeCommand.Close, 0, null));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _Producer.Wait();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      Assert.That(_CommandCount[TreeCommand.Delete], Is.EqualTo(x), "Not all commands are getting through.");
    }

    /// <summary>
    /// Simply test deletion times
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Number of searches</param>
    // [TestCase(1000)]
    // [TestCase(10000)]
    // [TestCase(100000)]
    public void SearchTest(int x)
    {
      long memBefore = GC.GetTotalMemory(true);
      var watch = System.Diagnostics.Stopwatch.StartNew();
      int key;
      Random random = new();
      for (int i = 0; i < x; i++)
      {
        key = _InsertedKeys.ElementAt(random.Next(0, _InsertedKeys.Count));
        _InputBuffer.SendAsync((TreeCommand.Search, key, null));
        _InsertedKeys.Remove(key);
      }
      _InputBuffer.SendAsync((TreeCommand.Close, 0, null));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _Producer.Wait();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      watch.Stop();
      long memAfter = GC.GetTotalMemory(true);
      long elapsedMs = watch.ElapsedMilliseconds;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      long keyCount = _Tree.KeyCount();
      int nodeCount = _Tree.NodeCount();
      Console.Write($"{System.Reflection.MethodBase.GetCurrentMethod().Name},{_Tree._Degree},{x},{elapsedMs},{Math.Abs(memBefore - memAfter)},{keyCount},{nodeCount}");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
  }
}