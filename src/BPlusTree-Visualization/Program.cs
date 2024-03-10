// Program.cs

using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using BPlusTreeVisualization;
using NodeData;

class Program
{
  static void Main()
  {
        var outputBuffer = new BufferBlock<(
      Status status,
      long id,
      int numKeys,
      int[] keys,
      Person[] contents,
      long altID,
      int altNumKeys,
      int[] altKeys,
      Person[] altContents
      )>();

    BPlusTree<Person> bPlusTree = new(3,outputBuffer);

    bPlusTree.Insert(1,new Person("hello"));
    bPlusTree.Traverse();
    
    Thread.CurrentThread.Name = "Main";

  }

}
