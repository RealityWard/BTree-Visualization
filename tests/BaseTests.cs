/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;
using System.Text.Json;

namespace tests{
  [TestFixture]
  public partial class BaseOperationsTest{
    private BTree<Person> _Tree;
    [SetUp]
    public void Setup(){
      _Tree = new(3, new BufferBlock<(Status status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
    }

    /// <summary>
    /// Author: Andreas Kramer
    /// Date: 2024-02-22
    /// Testing for Tree structure and properties
    /// </summary>

    [TestCase(10)]
    [TestCase(25)]
    [TestCase(250)]
    public void VerifyTreeHeightConsistency(int numberOfEntries){
      Random random = new Random();
      int[] uniqueKeys = new int[numberOfEntries];
      for(int i = 0; i < uniqueKeys.Length; i++){
        uniqueKeys[i] = random.Next(1,10000);
      }
      foreach (int key in uniqueKeys) {
          _Tree.Insert(key, new Person("Example"));
      }
      string output = _Tree.Traverse();
      int maxHeight = _Tree.GetMaxHeight();
      int minHeight = _Tree.GetMinHeight();
      Assert.That(maxHeight == minHeight, Is.True, $"Tree's minimum height is {minHeight}. Tree's maximum height is {maxHeight}. \n BeloW: {output}");
    }
    /// <summary>
    /// Author: Andreas Kramer
    /// The following tests are for testing the input filtering
    /// </summary>
    /// <param name="x"></param>
    [Test]
    public void InsertingDuplicateValues(){
      int[] a = {2, 2};
      _Tree.Insert(a[0], new Person("Example"));
      _Tree.Insert(a[1], new Person("Example"));
      _Tree.Delete(a[0]);
      Assert.That(_Tree.Search(a[0]),Is.Null, $"Key {a[0]} should NOT be found. Duplicate values should be ignored at insertion.");     
    }
    /// <summary>
    /// Date: 2024-02-13
    /// Just testing how NUnit works and testing initial insert and retrieval.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [Test]
    public void BasicTest(){
      _Tree.Insert(1,new Person("Chad"));
      Assert.That(_Tree.Search(1), Is.Not.Null);
      if (_Tree.Search(1) != null){
        Assert.That(_Tree.Search(1).Name,Is.EqualTo("Chad"));
        if(_Tree.Search(1).Name == "Chad"){
          Assert.That(Regex.Count(_Tree.Traverse(),"name"), Is.EqualTo(1));
        }
      }
      Assert.That(_Tree.Search(0), Is.Null);
    }

    [TestCase(100)]
    [TestCase(1000)]
    public void RandomOperationsTest(int numOps){
    //   for(int i = 0; i < numOps; i++){
    //     _Tree.Insert(i, new Person($"Person {i}"));
    //   }
      for(int i = 0; i < numOps; i++){
        Random rand = new Random();
        int op = rand.Next(0,3);
        int key = rand.Next(0,1000);
        
        _Tree.Insert(key, new Person($"Person {key}"));
        if (op == 0){
          var result = _Tree.Search(key);
          Assert.That(result, Is.Not.Null, $"Key {key} should be found.");
        }
        else if (op == 1){
         var  foundKey = _Tree.Search(key);
          if (foundKey != null){
            _Tree.Delete(key);
            var result = _Tree.Search(key);
            Assert.That(result, Is.Null, $"Key {key} should not be found.");
          }
        }
        else{
          var result = _Tree.Search(key);
          Assert.That(result, Is.Not.Null, $"Inserted key {key} should be found.");
          Assert.That(result.Name, Is.EqualTo($"Person {key}"), $"Key {key} should return the correct person.");
        
        }
    }
  }
  }

}