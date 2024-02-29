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
      _Tree = new(3, new BufferBlock<(Status status, long id, int numKeys, int[] keys, Person[] contents, long altID, int altNumKeys, int[] altKeys, Person[] altContents)>());
    }
    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-13
    /// Just testing how NUnit works and testing initial insert and retrieval.
    /// </summary>
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