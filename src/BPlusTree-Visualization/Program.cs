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

    for(int i = 50; i<100;i++){
      bPlusTree.Insert(i,new Person("hello " + i));
      int j = i -50;
      bPlusTree.Insert(i-50,new Person("hello " + j));
      Person? search1 = bPlusTree.Search(i);
      if(search1 != null){
        Console.WriteLine("Found: " + search1.ToString());
      }
      else{
        Console.WriteLine("Not found");
      }
      Person? search3 = bPlusTree.Search(j);
  
      if(search3 != null){
        Console.WriteLine("Found: " + search3.ToString());
      }
      else{
        Console.WriteLine("Not found");
      }
    }

    Person? search = bPlusTree.Search(56);
  
    if(search != null){
      Console.WriteLine("Found: " + search.ToString());
    }
    else{
      Console.WriteLine("Not found");
    }
    Person? search2 = bPlusTree.Search(89);
    if(search2 != null){
      Console.WriteLine("Found: " + search2.ToString());
    }
    else{
      Console.WriteLine("Not found");
    }
  
    //Console.WriteLine(bPlusTree.Traverse());
    
    Thread.CurrentThread.Name = "Main";

  }

}
