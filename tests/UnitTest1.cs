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
        Assert.That(_Tree.Search(1).Name,Is.EqualTo("Chad"));
        if(_Tree.Search(1).Name == "Chad"){
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
          Assert.That(_Tree.Search(i).Name, Is.EqualTo(i.ToString()), "Search turns up wrong content.");
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
      }
      Assert.That(Regex.Count(_Tree.Traverse(),"name"), Is.EqualTo(x), "Missing insertions");
    }

#pragma warning disable CA1861 // Avoid constant arrays as arguments
    [TestCase(1,new int[] {1,0},0,false)]
    [TestCase(2,new int[] {1,3},0,false)]
    [TestCase(3,new int[] {1,-1},0,false)]
    [TestCase(4,new int[] {1,-1,4,3,2},0,false)]
    [TestCase(10,new int[] {0,1,2,3,4,5,6,7,8,9},0,false)]
    [TestCase(10,new int[] {0,1,2,3,4,8,9},0,false)]
    [TestCase(10,new int[] {9,8,7,6,5,4,3,2,1,0},0,false)]
    [TestCase(50,new int[] {9,8,7,6,5,34,78,4,3,23,9,2,1,0},0,false)]
    [TestCase(100,new int[] {},100,false)]
    [TestCase(100,new int[] {},100,true)]
#pragma warning restore CA1861 // Avoid constant arrays as arguments
    public void DeletionBaseTest(int insertions, int[] deletions, int x, bool reversed){
      for(int i = 0; i < insertions; i++){
        _Tree.Insert(i,new Person(i.ToString()));
      }
      if(x!=0){
        deletions = CreateInOrderArrayFilled(x,reversed);
      }
      string before;
      for(int i = 0; i < deletions.Length; i++){
        before = _Tree.Traverse();
        #pragma warning disable CS8629 // Nullable value type may be null.
        if (_Tree.Search(deletions[i]) != null){
          _Tree.Delete(deletions[i]);
          Assert.That(_Tree.Search(deletions[i]), Is.Null, "Key " + i + " was not deleted.");
        }else{
          _Tree.Delete(deletions[i]);
          Assert.That(_Tree.Traverse(), Is.EqualTo(before), "Deleted a key that should not have been deleted");
        }
        #pragma warning restore CS8629 // Nullable value type may be null.
      }
    }

    private static int[] CreateInOrderArrayFilled(int x, bool reversed){
      int[] result = new int[x];
      if(reversed){
        for(int i = 0; i < x; i++){
          result[i] = x - i - 1;
        }
      }else{
        for(int i = 0; i < x; i++){
          result[i] = i;
        }
      }
      return result;
    }
  }
}