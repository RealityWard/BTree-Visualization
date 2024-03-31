/**
Author: Andreas Kramer
Date: 2024-02-03
Desc: Main for B+Tree implementation, running code here + initial testing
*/
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using BPlusTreeVisualization;
using Microsoft.VisualBasic.FileIO;
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
    Console.WriteLine("testing redistribution from right");
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
    
    for(int i = 2; i < 4;i++){
      bPlusTree2.Insert(i,new Person("hello " + i));
    }
    
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
    */

    /*
    Console.WriteLine("testing updated root value");

    BPlusTree<Person> bPlusTree3 = new(5,outputBuffer);
    for(int i = 0; i < 2;i++){
      bPlusTree3.Insert(i,new Person("hello " + i));
    }
    for(int i = 4; i < 7;i++){
      bPlusTree3.Insert(i,new Person("hello " + i));
    }
    /*
    for(int i = 2; i < 4;i++){
      bPlusTree2.Insert(i,new Person("hello " + i));
    }
    
    Console.WriteLine(bPlusTree3.Traverse());
    bPlusTree3.Delete(4);
    Console.WriteLine(bPlusTree3.Traverse());
    bPlusTree3.Delete(5);
    Console.WriteLine(bPlusTree3.Traverse());
    */
    
    Console.WriteLine("testing updated root value");
    BPlusTree<Person> bPlusTree4 = new(5,outputBuffer);
    for(int i = 0; i < 2;i++){
      bPlusTree4.Insert(i,new Person("hello " + i));
    }
    for(int i = 4; i < 7;i++){
      bPlusTree4.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree4.Traverse());
    bPlusTree4.Delete(4);
    Console.WriteLine(bPlusTree4.Traverse());

    Console.WriteLine("testing deleting from root");
    BPlusTree<Person> bPlusTree5 = new(5, outputBuffer);
    for(int i = 0; i < 5;i++){
      bPlusTree5.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree5.Traverse());
    bPlusTree5.Delete(1);
    bPlusTree5.Delete(2);
    bPlusTree5.Delete(3);
    bPlusTree5.Delete(4);
    Console.WriteLine(bPlusTree5.Traverse());

    Console.WriteLine("testing deletion causing distributing of child from left to right");
    BPlusTree<Person> bPlusTree6 = new(7, outputBuffer);
    for(int i = 0; i < 11;i++){
      bPlusTree6.Insert(i,new Person("hello " + i));
    }
    for(int i = 40; i < 54;i++){
      bPlusTree6.Insert(i,new Person("hello " + i));
    }
    for(int i = 11; i < 15;i++){
      bPlusTree6.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree6.Traverse());
    bPlusTree6.Delete(41);
    Console.WriteLine("Deleting");
    Console.WriteLine(bPlusTree6.Traverse());

    Console.WriteLine("testing deletion causing distributing of child from right to left");
    BPlusTree<Person> bPlusTree7 = new(7, outputBuffer);
    for(int i = 0; i < 14;i++){
      bPlusTree7.Insert(i,new Person("hello " + i));
    }
    for(int i = 40; i < 51;i++){
      bPlusTree7.Insert(i,new Person("hello " + i));
    }
    for(int i = 14; i < 18;i++){
      bPlusTree7.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree7.Traverse());
    bPlusTree7.Delete(11);
    Console.WriteLine("Deleting");
    Console.WriteLine(bPlusTree7.Traverse());

    




    
    

    



    



    


    
    

  










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
