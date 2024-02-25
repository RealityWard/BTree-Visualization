/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace tests{
  [TestFixture]
  public partial class TestsForTreeStart{
    private BTree<Person> _Tree;
    [SetUp]
    public void Setup(){
      _Tree = new(3,new BufferBlock<(Status status, long id, int[] keys, Person[] contents, long altID, int[] altKeys, Person[] altContents)>());
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

    // <summary>
    /// Author: Andreas Kramer
    /// Date: 2024-02-14
    /// Testing the search functionality via different scenarios
    /// </summary>

    [Test]
    public void SearchInEmptyTreeTestReturnsNull(){
      Assert.That(_Tree.Search(1), Is.Null, "Search in an empty tree should return null.");
    }

    [Test]
    public void SearchForExistingKeysReturnsCorrectData(){
      var testData = new List<(int key, string name)> { (1, "Alice"), (2, "Bob"), (3, "Charlie") };
      foreach (var (key, name) in testData){
          _Tree.Insert(key, new Person(name));
      }
    
      foreach (var (key, name) in testData){
          var result = _Tree.Search(key);
          Assert.That(result, Is.Not.Null, $"Key {key} should be found.");
          Assert.That(result.Name, Is.EqualTo(name), $"Key {key} should be associated with name {name}.");
      }
    }

    [Test]
    [TestCase(-1)]
    [TestCase(100)]
    [TestCase(50)]
    public void SearchForNonExistingKeysShouldReturnNull(int key){
        _Tree.Insert(1, new Person("Test 1"));
        _Tree.Insert(10, new Person("Test 10"));
        _Tree.Insert(20, new Person("Test 20"));
    
        Assert.That(_Tree.Search(key), Is.Null, $"Non-existing key {key} should not be found.");
    } 

    [Test]
    public void InsertAndSearchRandomKeys(){
      Random random = new Random();
      int numberOfKeys = 10;
      Dictionary<int, string> insertedKeys = new Dictionary<int, string>();

      for(int i = 0; i < numberOfKeys; i++){
          int key = random.Next(1, 10000); 
          string name = $"Person {i}";
         //making sure keys are unique
        if (!insertedKeys.ContainsKey(key)){
            _Tree.Insert(key, new Person(name));
            insertedKeys.Add(key, name);
        }
        else{
            i--;
        }
      }
      //each inserted key should be correctly searched for
      foreach(var entry in insertedKeys){
        int key = entry.Key;
        string expectedName = entry.Value;
        var result = _Tree.Search(key);
      
        Assert.That(result, Is.Not.Null, $"Key {key} should be found.");
        Assert.That(result.Name, Is.EqualTo(expectedName), $"Key {key} should return the correct person.");
      }
    }

    [Test]
    public void SearchBoundaryValues(){

      _Tree.Insert(1, new Person("Min Person"));
      for(int i = 2; i < 100; i++){
      _Tree.Insert(i, new Person("Min Person"));
      }
      _Tree.Insert(100, new Person("Max Person"));

      var minResult = _Tree.Search(1);
      var maxResult = _Tree.Search(100);

      Assert.That(minResult, Is.Not.Null, "Minimum key should be found.");
      Assert.That(maxResult, Is.Not.Null, "Maximum key should be found.");
    }
    [Test]
    public void ConcurrencySearchTest(){
      for(int i = 0; i < 100; i++){
        _Tree.Insert(i, new Person($"Person {i}"));
      }

      Parallel.For(0, 100, (i) => {
        var result = _Tree.Search(i);
        Assert.That(result, Is.Not.Null, $"Concurrent search for key {i} failed.");
      });
    }

    [TestCase(1000)]
    public void StressTestForSearchConsistency(int x){
      int numberOfKeys = x; // Or more, depending on performance
      for(int i = 0; i < numberOfKeys; i++){
          _Tree.Insert(i, new Person($"Person {i}"));
      }

      for(int i = 0; i < numberOfKeys; i++){
        var result = _Tree.Search(i);
        Assert.That(result, Is.Not.Null, $"Inserted key {i} should be found.");
        Assert.That(result.Name, Is.EqualTo($"Person {i}"), $"Key {i} should return the correct person.");
      }
    }

    //Delete Tests
    [Test]
    public void DeleteLeafNodeKey() {
      _Tree.Insert(10, new Person("Person 10"));
      _Tree.Insert(20, new Person("Person 20"));
      _Tree.Delete(20);

      Assert.That(_Tree.Search(20), Is.Null, "Key 20 should have been deleted from the leaf node.");
    }

    /// <summary>
    /// Primary Author: Andreas
    /// Secondary Author: Tristan for some edits to log history 2024-02-23
    /// </summary>
    /// <param name="numberOfEntries"></param>
    /// <param name="numberOfKeysToDelete"></param>
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(100,100)]
    [TestCase(1000,1000)]
    public void DeleteRandomKeysFromTree(int numberOfEntries, int numberOfKeysToDelete){
      Random random = new Random();
      string keysPrintOutInCaseOfError = "";
      int[] uniqueKeys = new int[numberOfEntries];
      for(int i = 0; i < uniqueKeys.Length; i++){
        uniqueKeys[i] = random.Next(1,10000);
        keysPrintOutInCaseOfError += uniqueKeys[i] + ",";
      }
      keysPrintOutInCaseOfError += "----------------";
      foreach (int key in uniqueKeys) {
          _Tree.Insert(key, new Person("Name"));
      }
      List<int> deletedKeys = DeleteRandomKeysFromTreeHelper(numberOfEntries, numberOfKeysToDelete, uniqueKeys);

      foreach(int key in deletedKeys){
        keysPrintOutInCaseOfError += key + ",";
        var result = _Tree.Search(key);
        Assert.That(result, Is.Null, $"Inserted key {key} should NOT be found. Below is the keys in order of entry and deletion.\n{keysPrintOutInCaseOfError}");
      }
    }

    private List<int> DeleteRandomKeysFromTreeHelper(int numberOfEntries , int numberOfKeysToDelete, int[] uniqueKeys){
      List<int> keysToDelete = new List<int>();
      List<int> allKeys = uniqueKeys.ToList();
      
      for (int i = 0; i < numberOfKeysToDelete; i++) {
        Random random = new Random();
          int keyIndex = random.Next(allKeys.Count);
          int key = allKeys[keyIndex];
          _Tree.Delete(key);
          keysToDelete.Add(key);
          allKeys.RemoveAt(keyIndex);
      }
      return keysToDelete;
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

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Tests the delete and merge method basics with several variations 
    /// including reverse, delete something not there, middle arbitrarily,
    /// up to 100 entries.
    /// </summary>
    /// <param name="insertions"></param>
    /// <param name="deletions"></param>
    /// <param name="x"></param>
    /// <param name="reversed"></param>
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
    [TestCase(100,new int[] {9,8,7,6,5,34,78,4,3,23,9,2,1,0},0,false)]
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

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Fills an array with a sequence from 0 to x or x to 0 if reversed.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="reversed"></param>
    /// <returns></returns>
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