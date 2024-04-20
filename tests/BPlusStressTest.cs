using System.Text.RegularExpressions;
using BPlusTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace BPlustests{
  [TestFixture]
  public partial class BPlusStressTests{
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
    //Helper methods
    public int[] CreateRandomArray(int length){
      int[] array = new int[length];
      for(int i = 0; i < length;i++){
        Random rand = new Random();
        array[i] = rand.Next(0,100000);
      }
      return array;    
    }

    public void DeleteFirstHalf(BPlusTree<Person> bPlusTree, int[] array){
      for(int i = 0; i < array.Length / 2 ;i++){
        int key = array[i];
        bPlusTree.Delete(key);
      }
    }

    public void InsertFirstHalf(BPlusTree<Person> bPlusTree,int[] array){
      for(int i = 0; i < array.Length / 2 ;i++){
        int key = array[i];
        bPlusTree.Insert(key,new Person("Person " + key));
      }
    }

    public void DeleteSecondHalf(BPlusTree<Person> bPlusTree, int[] array){
      for(int i = array.Length / 2; i < array.Length ;i++){
        int key = array[i];
        bPlusTree.Delete(key);
      }
    }

    public void InsertSecondHalf(BPlusTree<Person> bPlusTree,int[] array){
      for(int i = array.Length / 2; i < array.Length ;i++){
        int key = array[i];
        bPlusTree.Insert(key,new Person("Person " + key));
      }

    }
    /// <summary>
    /// This tests the balancedness of the treeheight, insertes and deletes half of the array, re-initializes and keeps inserting
    /// and deleting, repeatedly
    /// </summary>
    /// <param name="length"></param>
    [TestCase(10000)]
    public void StressTestTreeHeight(int length){
      int[] array = CreateRandomArray(length);
      for(int i = 0; i < array.Length;i++){
        int key = array[i];
        _Tree.Insert(key,new Person("Person " + key));
      }
      int minHeight = _Tree.GetMinHeight();
      int maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");

      DeleteFirstHalf(_Tree, array);

      minHeight = _Tree.GetMinHeight();
      maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");

      InsertFirstHalf(_Tree, array);

      minHeight = _Tree.GetMinHeight();
      maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");

      DeleteSecondHalf(_Tree, array);

      minHeight = _Tree.GetMinHeight();
      maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");

      array = CreateRandomArray(length); //re-initialize random array

      InsertFirstHalf(_Tree, array);

      minHeight = _Tree.GetMinHeight();
      maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");

      DeleteFirstHalf(_Tree, array);

      minHeight = _Tree.GetMinHeight();
      maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");

      InsertSecondHalf(_Tree, array);

      minHeight = _Tree.GetMinHeight();
      maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");

      DeleteSecondHalf(_Tree, array);

      minHeight = _Tree.GetMinHeight();
      maxHeight = _Tree.GetMaxHeight();
      Assert.That(minHeight == maxHeight,$"Min-Height: {minHeight}; Max-Height: {maxHeight} (3)");
    }

  }
}