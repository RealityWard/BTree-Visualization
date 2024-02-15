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
        // Prepopulate the tree with known keys
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
        
        // Ensure uniqueness of keys
        if (!insertedKeys.ContainsKey(key)){
            _Tree.Insert(key, new Person(name));
            insertedKeys.Add(key, name);
        }
        else{
            i--;
        }
      }
        // Verify that each inserted key can be correctly searched for
        foreach(var entry in insertedKeys){
          int key = entry.Key;
          string expectedName = entry.Value;
          var result = _Tree.Search(key);
        
          Assert.That(result, Is.Not.Null, $"Key {key} should be found.");
          Assert.That(result.Name, Is.EqualTo(expectedName), $"Key {key} should return the correct person.");
        }
    }

    [TestCase(100)]
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