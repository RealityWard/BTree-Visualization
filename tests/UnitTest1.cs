/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using BTreeVisualization;
using NodeData;


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
    [Test]
    public void Test1(){
      _Tree.Insert(1,new Person("Chad"));
      #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
      if (_Tree.Search(1) != null){
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        if(_Tree.Search(1).Name == "Chad"){
          Assert.Pass();
        }else{
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
          Assert.Fail("Missing Name from person data");
        }
      }else{
        Assert.Fail("Insert to leaf not working; Missing Data");
      }
      #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
    }
  }
}