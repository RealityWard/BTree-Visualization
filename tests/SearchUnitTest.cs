
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
    public partial class SearchTest{
        private BTree<Person> _Tree;
        [SetUp]
        public void Setup(){
        _Tree = new(3,new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>());
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
    }
}