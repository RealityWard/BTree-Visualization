/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;
// using System.Text.RegularExpressions;

namespace tests{
  [TestFixture]
  public class TestsForTreeStart{
    private BTree<Person> _Tree;
    [SetUp]
    public void Setup(){
      _Tree = new(3);
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-03
    /// Just testing how NUnit works and testing initial insert and retrieval.
    /// </summary>
    // [Test]
    // public void BasicTest(){
    //   _Tree.Insert(1,new Person("Chad"));
    //   Assert.That(_Tree.Search(1), Is.Not.Null);
    //   if (_Tree.Search(1) != null){
    //     Assert.That(_Tree.Search(1).Name,Is.EqualTo("Chad"));
    //     if(_Tree.Search(1).Name == "Chad"){
    //       Assert.That(_Tree.Traverse(), Is.EqualTo("{\n\"dataEntry\":0\n\"key\":\"1\",\n\"person\":{\"name\":Chad}\n}"));
    //     }
    //   }
    //   Assert.That(_Tree.Search(0), Is.Null);
    // }

    [TestCase(1)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(10)]
    public void InsertionTest(int x){
      for(int i = 0; i < x; i++){
        _Tree.Insert(i,new Person(i.ToString()));
      }
      Console.WriteLine(_Tree.Traverse());
      for(int i = 0; i < x; i++){
        Assert.That(_Tree.Search(i), Is.Not.Null,"Search for key " + i + " was not found.");
        if (_Tree.Search(i) != null){
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
          Assert.That(_Tree.Search(i).Name, Is.EqualTo(i.ToString()));
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
      }
      Assert.That(Regex.Count(_Tree.Traverse(),"name"), Is.EqualTo(x));
    }
  }
}