/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;
using System.Text.Json;

namespace tests{
  [TestFixture]
  public partial class DeletionTests{
    private BTree<Person> _Tree;
    [SetUp]
    public void Setup(){
        _Tree = new(3,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
    }
        
    //Delete Tests
    /// <summary>
    /// Author: Andreas Kramer
    /// deletes a key from a leafnode and checks if it actually got deleted
    /// </summary>
    [Test]
    public void DeleteLeafNodeKey(){
        _Tree.Insert(10, new Person("Person 10"));
        _Tree.Insert(20, new Person("Person 20"));
        _Tree.Delete(20);
        Assert.That(_Tree.Search(20), Is.Null, "Key 20 should have been deleted from the leaf node.");
    }

    /// <summary>
    /// Primary Author: Andreas
    /// Secondary Author: Tristan for some edits to log history 2024-02-23
    /// This is a test for consistency of the tree, by inserting and deleting multiple random values
    /// we check the correct algorithm by searching for the keys that should have been deleted and searching for all the ones that are left
    /// this is also testing the tree structure and properties by checking the balancedness of the tree
    /// </summary>
    /// <param name="numberOfEntries"></param>
    /// <param name="numberOfKeysToDelete"></param>
    [TestCase(100,50)]
    [TestCase(100,100)]
    [TestCase(100,30)]
    [TestCase(100,100)]
    [TestCase(100,90)]
    [TestCase(100,80)]
    [TestCase(100,10)]
    [TestCase(100,70)]
    [TestCase(100,100)]
    [TestCase(100,40)]
    [TestCase(100,100)]
    [TestCase(1000,1000)]
    
    public void DeleteRandomKeysFromTree(int numberOfEntries, int numberOfKeysToDelete){
        Random random = new Random();
        string keysPrintOutInCaseOfError = "";
        string keysComparison = "";
        int[] randomKeys = new int[numberOfEntries];
        for (int i = 0; i < randomKeys.Length; i++)
        {
            randomKeys[i] = random.Next(1, 10000);
            keysPrintOutInCaseOfError += randomKeys[i] + ",";
        }
        int[] uniqueKeys = randomKeys.Distinct().ToArray(); 
        keysPrintOutInCaseOfError += "----------------";
        foreach (int key in uniqueKeys)
        {
            _Tree.Insert(key, new Person("Name"));
        }

        List<int> allKeys = uniqueKeys.ToList();
        int numberOfKeysToDeleteActual = allKeys.Count;
        List<int> deletedKeys = DeleteRandomKeysFromTreeHelper(numberOfEntries, numberOfKeysToDeleteActual, ref allKeys);

        bool balanced =_Tree.IsBalanced();
        Assert.That(balanced,Is.True, $"Tree is unbalanced.");

        foreach (int key in deletedKeys){
            keysPrintOutInCaseOfError += key + ",";
            var result = _Tree.Search(key);
            Assert.That(result, Is.Null, $"Inserted key {key} should NOT be found. Below are the keys in order of entry and deletion.\n{keysPrintOutInCaseOfError}");
        }
        foreach(int key in allKeys){
            keysComparison += key + ", ";
            var result = _Tree.Search(key);
            Assert.That(result, Is.Not.Null, $"Inserted key {key} should be found. Below are the keys in order of entry and deletion. \n{keysComparison}");
        }
        int maxHeight = _Tree.GetMaxHeight();
        int minHeight = _Tree.GetMinHeight();
        Assert.That(maxHeight == minHeight, Is.True, $"Tree's minimum height is {minHeight}. Tree's maximum height is {maxHeight}.");
        }
    private List<int> DeleteRandomKeysFromTreeHelper(int numberOfEntries , int numberOfKeysToDelete, ref List<int> allKeys){
        List<int> keysToDelete = new List<int>();
      
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