/**
Author: Andreas Kramer
Date: 2024-02-03
Desc: Main for B+Tree implementation, running code here + initial testing
*/
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

    //testing redistribution from right
    BPlusTree<Person> bPlusTree = new(5,outputBuffer);
     for(int i = 0; i < 5;i++){
      bPlusTree.Insert(i,new Person("hello " + i));

     }
     Console.WriteLine(bPlusTree.Traverse());
     bPlusTree.Delete(1);
     Console.WriteLine(bPlusTree.Traverse());

    //testing redistribution from left
    Console.WriteLine("testing redistribution from left");

    BPlusTree<Person> bPlusTree2 = new(5,outputBuffer);
    for(int i = 0; i < 2;i++){
      bPlusTree2.Insert(i,new Person("hello " + i));
    }
    for(int i = 4; i < 7;i++){
      bPlusTree2.Insert(i,new Person("hello " + i));
    }
    /*
    for(int i = 2; i < 4;i++){
      bPlusTree2.Insert(i,new Person("hello " + i));
    }
    */
    Console.WriteLine(bPlusTree2.Traverse());
    bPlusTree2.Delete(4);
    Console.WriteLine(bPlusTree2.Traverse());
    bPlusTree2.Delete(5);
    Console.WriteLine(bPlusTree2.Traverse());
    /*
    Console.WriteLine("testing merging with rightsibling");
    //testing merging
    bPlusTree2.Delete(6);
    bPlusTree2.Delete(2);
    bPlusTree2.Delete(1);
    Console.WriteLine(bPlusTree2.Traverse());

    /*
    BPlusTree<Person> bPlusTree3 = new(4,outputBuffer);
    for(int i = 0; i < 4;i++){
      bPlusTree3.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree3.Traverse());
    bPlusTree3.Delete(2);
    bPlusTree3.Delete(3);
    bPlusTree3.Delete(1);
    Console.WriteLine(bPlusTree3.Traverse());
    */


    
    

  










    //testing merging



    /*

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
    */
  
    //Console.WriteLine(bPlusTree.Traverse());
    
    Thread.CurrentThread.Name = "Main";

  }

}
