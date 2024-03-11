// Program.cs

using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using BPlusTreeVisualization;
using NodeData;
using ThreadCommunication;

class Program
{
  static void Main()
  {
      var outputBuffer = new BufferBlock<(
      Status status,
      long id,
      int numKeys,
      int[] keys,
      Person?[] contents,
      long altID,
      int altNumKeys,
      int[] altKeys,
      Person?[] altContents
      )>();

    BPlusTree<Person> bPlusTree = new(3,outputBuffer);

    bPlusTree.Insert(1,new Person("hello"));
    bPlusTree.Insert(2,new Person("hello"));
    bPlusTree.Insert(3,new Person("hello"));
    bPlusTree.Insert(4,new Person("hello"));
    bPlusTree.Insert(5,new Person("hello"));
    bPlusTree.Insert(6,new Person("hello"));
    bPlusTree.Insert(7,new Person("hello"));

    
    Console.WriteLine(bPlusTree.Traverse());
    
    Thread.CurrentThread.Name = "Main";

  }

}
