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

    for(int i = 0; i<20;i++){
      bPlusTree.Insert(i,new Person("hello"));
    }

    Person? search = bPlusTree.Search(2);
    if(search != null){
      Console.WriteLine(search.ToString());
    }
    else{
      Console.WriteLine("Not found");
    }
    

    
    Console.WriteLine(bPlusTree.Traverse());
    
    Thread.CurrentThread.Name = "Main";

  }

}
