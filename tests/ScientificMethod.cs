/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;
using System.Text.Json;

namespace Science
{
  /// <summary>
  /// Tests for CHEM 1100 Scientific Method Project 
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  [TestFixture]
  public partial class BenchMarkTests()
  {
    private BufferBlock<(Status status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
    private BufferBlock<(TreeCommand action, int key
      , Person? content)> _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
    private BTree<Person>? _Tree;
    Dictionary<int, string> _InsertedKeys = [];
    private readonly int _NumberOfKeys = 1000000;
    private Task? _Producer;

    [SetUp]
    public void Setup()
    {
      
    }

#pragma warning disable CA1861 // Avoid constant arrays as arguments
    [TestCase(3, new int[] { 1000, 10000, 100000 })]
    // [TestCase(30, new int[] { 1000, 10000, 100000 })]
    // [TestCase(300, new int[] { 1000, 10000, 100000 })]
#pragma warning restore CA1861 // Avoid constant arrays as arguments
    public void DegreeTest(int degree, int[] tests){
      _Tree = new(degree, _OutputBuffer);
      Task consumer = Task.Run(async () => 
      {
        while (await _OutputBuffer.OutputAvailableAsync())
        {
          _OutputBuffer.Receive();
        }
      });
      Random random = new();
      int key;
      for (int i = 0; i < _NumberOfKeys; i++)
      {
        do
        {
          key = random.Next(1, _NumberOfKeys * 10);
        } while (_InsertedKeys.ContainsKey(key));
        _Tree.Insert(key, new Person(key.ToString()));
        _InsertedKeys.Add(key, key.ToString());
      }
      _Producer = Task.Run(async () =>
      {
        Dictionary<TreeCommand,long> counter = [];
        foreach(TreeCommand i in (TreeCommand[])Enum.GetValues(typeof(TreeCommand))){
          counter.Add(i,0);
        }
        while (await _InputBuffer.OutputAvailableAsync())
        {
          (TreeCommand action, int key, Person? content) = _InputBuffer.Receive();
          counter[action] += 1;
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
              _OutputBuffer.Complete();
              _InputBuffer.Complete();
              break;
            default:// Will close buffer upon receiving a bad TreeCommand.
              _InputBuffer.Complete();
              Console.WriteLine("TreeCommand:{0} not recognized", action);
              break;
          }
        }
        foreach(TreeCommand i in (TreeCommand[])Enum.GetValues(typeof(TreeCommand))){
          Console.WriteLine(i.ToString() + ":" + counter[i]);
        }
      });
      foreach(int x in tests){
        InsertionTest(x);
        DeletionTest(x);
        SearchTest(x);
      }
    }

    /// <summary>
    /// Simply test insertion times
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Number of insertions</param>
    private void InsertionTest(int x)
    {
      long memBefore = GC.GetTotalMemory(true);
      var watch = System.Diagnostics.Stopwatch.StartNew();
      Random random = new();
      int key;
      for (int i = 0; i < x; i++)
      {
        do
        {
          key = random.Next(0, _NumberOfKeys * 10);
        } while (_InsertedKeys.ContainsKey(key));
        _InputBuffer.SendAsync((TreeCommand.Insert, key
          , new Person(key.ToString())));
        _InsertedKeys.Add(key, key.ToString());
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
      Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name},{_Tree._Degree},{x},{elapsedMs},{Math.Abs(memBefore - memAfter)},{keyCount},{nodeCount}");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    
    /// <summary>
    /// Simply test deletion times
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Number of deletions</param>
    private void DeletionTest(int x)
    {
      long memBefore = GC.GetTotalMemory(true);
      var watch = System.Diagnostics.Stopwatch.StartNew();
      int key;
      Random random = new();
      for (int i = 0; i < x; i++)
      {
        key = _InsertedKeys.ElementAt(random.Next(0, _InsertedKeys.Count)).Key;
        _InputBuffer.SendAsync((TreeCommand.Delete, key, null));
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
      Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name},{_Tree._Degree},{x},{elapsedMs},{Math.Abs(memBefore - memAfter)},{keyCount},{nodeCount}");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }

    
    /// <summary>
    /// Simply test deletion times
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Number of searches</param>
    private void SearchTest(int x)
    {
      long memBefore = GC.GetTotalMemory(true);
      var watch = System.Diagnostics.Stopwatch.StartNew();
      int key;
      Random random = new();
      for (int i = 0; i < x; i++)
      {
        key = _InsertedKeys.ElementAt(random.Next(0, _InsertedKeys.Count)).Key;
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
      Console.WriteLine($"{System.Reflection.MethodBase.GetCurrentMethod().Name},{_Tree._Degree},{x},{elapsedMs},{Math.Abs(memBefore - memAfter)},{keyCount},{nodeCount}");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
  }

}