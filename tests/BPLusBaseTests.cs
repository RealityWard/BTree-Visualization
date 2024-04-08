using System.Text.RegularExpressions;
using BPlusTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

///Author: Andreas Kramer

namespace BPlustests{
  [TestFixture]
  public partial class BPlusBaseTests{

    private BPlusTree<Person> _Tree;
    private BPlusTree<Person> _Tree2;
    private BPlusTree<Person> _Tree3;
    private BPlusTree<Person> _Tree4;
    private BPlusTree<Person> _Tree5;
    
    [SetUp]
    public void Setup(){
        _Tree = new(3,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
        _Tree2 = new(4,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
        _Tree3 = new(5,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
        _Tree4 = new(6,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
        _Tree5 = new(7,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
    }
    public int[] CreateUniqueRandomArray(int length){
      int[] array = new int[length];
      for(int i = 0; i < length;i++){
        Random rand = new Random();
        array[i] = rand.Next(0,1000);
      }
      int[] uniqueArray = array.Distinct().ToArray();
      return uniqueArray;    
    }
    /// <summary>
    /// Creates an array with unique integers and inserts them, randomly deletes values from the tree and 
    /// checks if they actually got deleted, keeps calling search to check if the tree maintained its structure
    /// checks trees of degrees 3,4,5,6,7
    /// Author: Andreas Kramer (modified from RandomOperationsTest from Emily Elzinga)
    /// </summary>
    /// <param name="numOps"></param>
    [TestCase(1000)]
    public void RandomOperationsTest(int numOps){
      int[] array = CreateUniqueRandomArray(numOps);
      numOps = array.Length;
      List<int> deletedKeys = new List<int>();  //for debugging
      for(int i = 0; i < numOps;i++){
        Random rand = new Random();
        int op = rand.Next(0,3);
        int key = array[i];
        _Tree.Insert(key,new Person($"Person {key}"));
        _Tree2.Insert(key,new Person($"Person {key}"));
        _Tree3.Insert(key,new Person($"Person {key}"));
        _Tree4.Insert(key,new Person($"Person {key}"));
        _Tree5.Insert(key,new Person($"Person {key}"));
        if(op == 0){
          var result = _Tree.Search(key);
          Assert.That(result, Is.Not.Null, $"Key {key} should be found. (3)");
          var result2 = _Tree2.Search(key);
          Assert.That(result2, Is.Not.Null, $"Key {key} should be found. (4)");
          var result3 = _Tree3.Search(key);
          Assert.That(result3, Is.Not.Null, $"Key {key} should be found. (5)");
          var result4 = _Tree4.Search(key);
          Assert.That(result4, Is.Not.Null, $"Key {key} should be found. (6)");
          var result5 = _Tree5.Search(key);
          Assert.That(result5, Is.Not.Null, $"Key {key} should be found. (7)");

        }
        else if(op == 1){
          var foundkey = _Tree.Search(key);
          if(foundkey != null){
            _Tree.Delete(key);
            deletedKeys.Add(key);
            var result = _Tree.Search(key);
            Assert.That(result, Is.Null, $"Key {key} should not be found.");

          }
          var foundkey2 = _Tree2.Search(key);
          if(foundkey2 != null){
            _Tree2.Delete(key);
            deletedKeys.Add(key);
            var result = _Tree2.Search(key);
            Assert.That(result, Is.Null, $"Key {key} should not be found.");

          }
          var foundkey3 = _Tree3.Search(key);
          if(foundkey3 != null){
            _Tree3.Delete(key);
            deletedKeys.Add(key);
            var result = _Tree3.Search(key);
            Assert.That(result, Is.Null, $"Key {key} should not be found.");

          }
          var foundkey4 = _Tree.Search(key);
          if(foundkey4 != null){
            _Tree4.Delete(key);
            deletedKeys.Add(key);
            var result = _Tree4.Search(key);
            Assert.That(result, Is.Null, $"Key {key} should not be found.");

          }
          var foundkey5 = _Tree.Search(key);
          if(foundkey5 != null){
            _Tree4.Delete(key);
            deletedKeys.Add(key);
            var result = _Tree5.Search(key);
            Assert.That(result, Is.Null, $"Key {key} should not be found.");

          }
        }
        else{
          var result = _Tree.Search(key);            
          Assert.That(result, Is.Not.Null, $"Key {key} should be found. (3)");
          var result2 = _Tree2.Search(key);
          Assert.That(result2, Is.Not.Null, $"Key {key} should be found. (4)");
          var result3 = _Tree3.Search(key);
          Assert.That(result3, Is.Not.Null, $"Key {key} should be found. (5)");
          var result4 = _Tree4.Search(key);
          Assert.That(result4, Is.Not.Null, $"Key {key} should be found. (6)");
          var result5 = _Tree5.Search(key);
          Assert.That(result5, Is.Not.Null, $"Key {key} should be found. (7)");
        }
      }
    }
  }    
}
