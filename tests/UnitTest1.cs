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

    [TestCase(100,10)]
    public void DeleteRandomKeysFromTree(int numberOfEntries, int numberOfKeysToDelete){
      Random random = new Random();
      int[] uniqueKeys = new int[numberOfEntries];
      for(int i = 0; i < uniqueKeys.Length; i++){
        uniqueKeys[i] = random.Next(1,1000);
      }
      
      foreach (int key in uniqueKeys) {
          _Tree.Insert(key, new Person("Name"));
      }
      List<int> deletedKeys = DeleteRandomKeysFromTreeHelper(numberOfEntries, numberOfKeysToDelete, uniqueKeys);

      foreach(int key in deletedKeys){
        var result = _Tree.Search(key);
        Assert.That(result, Is.Null, $"Inserted key {key} should NOT be found.");
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
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(10)]
    //[TestCase(100)]
    public void InsertionTest(int x){
      for(int i = 0; i < x; i++){
        _Tree.Insert(i,new Person(i.ToString()));
      }
      for(int i = 0; i < x; i++){
        Assert.That(_Tree.Search(i), Is.Not.Null,"Search for key " + i + " was not found.");
        if (_Tree.Search(i) != null){
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
          Assert.That(_Tree.Search(i).Name, Is.EqualTo(i.ToString()),"Search turns up wrong content.");
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
      }
      Assert.That(MyRegex().Count(_Tree.Traverse()), Is.EqualTo(x));
    }

    [GeneratedRegex("name")]
    private static partial Regex MyRegex();
  }
}