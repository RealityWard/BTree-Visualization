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
      NodeStatus status,
      long id,
      int numKeys,
      int[] keys,
      Person?[] contents,
      long altID,
      int altNumKeys,
      int[] altKeys,
      Person?[] altContents
      )>();

    BPlusTree<Person> bPlusTree = new(4,outputBuffer);

    for(int i = 20; i<30;i++){
      bPlusTree.Insert(i,new Person("hello " + i));
      int j = i -10;
      bPlusTree.Insert(i-10,new Person("hello " + j));
    }

    Person? search = bPlusTree.Search(2);
  
    if(search != null){
      Console.WriteLine("Found: " + search.ToString());
    }
    else{
      Console.WriteLine("Not found");
    }
    Person? search2 = bPlusTree.Search(8);
    if(search2 != null){
      Console.WriteLine("Found: " + search2.ToString());
    }
    else{
      Console.WriteLine("Not found");
    }
  
    Console.WriteLine(bPlusTree.Traverse());
    
    Thread.CurrentThread.Name = "Main";

  }

}
