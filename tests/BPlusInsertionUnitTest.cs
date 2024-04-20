using System.Text.RegularExpressions;
using BPlusTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace BPlustests{
  [TestFixture]
  public partial class BPlusInsertionUnitTests{
    private BPlusTree<Person> _Tree;
    private BPlusTree<Person> _Tree2;
    private BPlusTree<Person> _Tree3;

    [SetUp]
    public void Setup(){
        _Tree = new(3,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
        _Tree2 = new(5,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
        _Tree3 = new(6,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
    }
  
    /// <summary>
    /// Author: Andreas Kramer
    /// Testing insertion and searching for random keys
    /// </summary>
    [Test]
    public void InsertAndSearchRandomKeys(){
      Random random = new Random();
      int numberOfKeys = 1000;
      Dictionary<int, string> insertedKeys = new Dictionary<int, string>();

      for(int i = 0; i < numberOfKeys; i++){
        int key = random.Next(1, 10000); 
        string name = $"Person {i}";
        if (!insertedKeys.ContainsKey(key)){
          _Tree.Insert(key, new Person(name));
          _Tree2.Insert(key, new Person(name));
          _Tree3.Insert(key,new Person(name));
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
        var result2 = _Tree.Search(key);
        var result3 = _Tree.Search(key);
            
        Assert.That(result, Is.Not.Null, $"Key {key} should be found.");
        Assert.That(result.Name, Is.EqualTo(expectedName), $"Key {key} should return the correct person.");
        Assert.That(result2, Is.Not.Null, $"Key {key} should be found.");
        Assert.That(result2.Name, Is.EqualTo(expectedName), $"Key {key} should return the correct person.");
        Assert.That(result3, Is.Not.Null, $"Key {key} should be found.");
        Assert.That(result3.Name, Is.EqualTo(expectedName), $"Key {key} should return the correct person.");

      }
    }

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
  }
}