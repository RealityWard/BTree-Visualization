/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;

namespace tests{
  [TestFixture]
  public partial class TestsForTreeStart{
    private BTree<Person> _Tree;
    [SetUp]
    public void Setup(){
      _Tree = new(3);
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
        Assert.That(_Tree.Search(1).name,Is.EqualTo("Chad"));
        if(_Tree.Search(1).name == "Chad"){
          Assert.That(Regex.Count(_Tree.Traverse(),"name"), Is.EqualTo(1));
        }
      }
      Assert.That(_Tree.Search(0), Is.Null);
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-13
    /// Working with insertion operations and using search and traverse to test correctness.
    /// </summary>
    /// <param name="x"></param>
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(10)]
    [TestCase(100)]
    public void InsertionBaseTest(int x){
      for(int i = 0; i < x; i++){
        _Tree.Insert(i,new Person(i.ToString()));
      }
      for(int i = 0; i < x; i++){
        Assert.That(_Tree.Search(i), Is.Not.Null, "Search for key " + i + " was not found.");
        if (_Tree.Search(i) != null){
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
          Assert.That(_Tree.Search(i).name, Is.EqualTo(i.ToString()), "Search turns up wrong content.");
          Assert.That(_Tree.Search(i).name, Is.EqualTo(i.ToString()), "Search turns up wrong content.");
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
      }
      Assert.That(Regex.Count(_Tree.Traverse(),"name"), Is.EqualTo(x), "Missing insertions");
      Assert.That(Regex.Count(_Tree.Traverse(),"name"), Is.EqualTo(x), "Missing insertions");
    }

    [GeneratedRegex("name")]
    private static partial Regex MyRegex();
  }
}