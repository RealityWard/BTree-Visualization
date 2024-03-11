/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;
using System.Text.Json;

namespace tests
{
  /// <summary>
  /// Tests everything to do with threading.
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  [TestFixture]
  public partial class ThreadTest
  {
    private BTree<Person> _Tree;
    private BufferBlock<(Status status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBuffer;
    private BufferBlock<(TreeCommand action, int key
      , Person? content)> _InputBuffer;
    [SetUp]
    public void Setup()
    {
      _OutputBuffer = new();
      _InputBuffer = new();
      _Tree = new(3, _OutputBuffer);

      Task producer = Task.Run(async () =>
      {
        Thread.CurrentThread.Name = "Producer";
        while (await _InputBuffer.OutputAvailableAsync())
        {
          (TreeCommand action, int key, Person? content) = _InputBuffer.Receive();
          switch (action)
          {
            case TreeCommand.Tree:
              _Tree = new(key, _OutputBuffer);
              break;
            case TreeCommand.Insert:
#pragma warning disable CS8604 // Possible null reference argument.
              _Tree.Insert(key, content);
#pragma warning restore CS8604 // Possible null reference argument.
              break;
            case TreeCommand.Delete:
              _Tree.Delete(key);
              break;
            case TreeCommand.Search:
              _Tree.Search(key);
              break;
            case TreeCommand.Traverse:
              Console.WriteLine(_Tree.Traverse());
              break;
            case TreeCommand.Close:
              _InputBuffer.Complete();
              break;
            default:// Will close buffer upon receiving a bad TreeCommand.
              _InputBuffer.Complete();
              Console.WriteLine("TreeCommand:{0} not recognized", action);
              break;
          }
        }
      });
    }

    /// <summary>
    /// Simply test whether messages are sent correctly.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [TestCase(1)]
    [TestCase(10)]
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(10000)]
    public void MessageWorks(int x)
    {
      BufferBlock<int> testInsertions = new();
      Task consumer = Task.Run(async () =>
      {
        string innerHistory = "innerH:";
        Thread.CurrentThread.Name = "Consumer";
        (Status status, long id, int numKeys, int[] keys
          , Person?[] contents, long altID, int altNumKeys, int[] altKeys
          , Person?[] altContents) nextBufferTuple = new();
        int nextBufferInt = 0;
        bool continueInnerWhile = true;
        while (await testInsertions.OutputAvailableAsync())
        {
          nextBufferInt = testInsertions.Receive();
          innerHistory += nextBufferInt + ",";
          while (await _OutputBuffer.OutputAvailableAsync() 
            && continueInnerWhile)
          {
            nextBufferTuple = _OutputBuffer.Receive();
            // Console.WriteLine("oBH:Status:" + nextBufferTuple.status + "NumKeys:" + nextBufferTuple.numKeys);
            if (nextBufferTuple.status == Status.Close)
            {
              _OutputBuffer.Complete();
            }
            continueInnerWhile = nextBufferTuple.status != Status.Inserted;
          }
          continueInnerWhile = true;
          // string keysArray = ":Keys:";
          // if(nextBufferTuple.keys != null)
          //   for(int i = 0; i < nextBufferTuple.numKeys; i++)
          //     keysArray += nextBufferTuple.keys[i] + ", ";
          // Console.WriteLine(innerHistory + "\nnBI:" + nextBufferInt + keysArray);
          Assert.That(nextBufferTuple.keys, Does.Contain(nextBufferInt)
            , $"Out of order\n{innerHistory}");
        }
        Console.WriteLine(_Tree.Traverse());
      });
      Task taskCreator = Task.Run(() => 
      {
        string history = "x:" + x + "\n";
        Random random = new();
        int[] uniqueKeys = new int[x];
        for (int i = 0; i < x; i++)
        {
          uniqueKeys[i] = random.Next(0, x * 100);
          _InputBuffer.Post((TreeCommand.Insert, uniqueKeys[i]
            , new Person(uniqueKeys[i].ToString())));
          history += uniqueKeys[i] + ",";
          testInsertions.Post(uniqueKeys[i]);
          // _InputBuffer.Post((TreeCommand.Traverse, 0, null));
        }
        Console.WriteLine("\nHistory:" + history);
        _InputBuffer.Complete();
        _OutputBuffer.Complete();
        testInsertions.Complete();
      });
    }
  }

}