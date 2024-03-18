using System.Text.RegularExpressions;
using BPlusTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace BPlustests{
  [TestFixture]
  public partial class BPlusInsertionUnitTests{
    private BPlusTree<Person> _Tree;
    [SetUp]
    public void Setup(){
        _Tree = new(3,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
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
  }
}