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
    
    //Testing redistribution of entries in leafnodes from right
    Console.WriteLine("Testing redistribution of leafnode-entries from right to left");
    BPlusTree<Person> bPlusTree1 = new(5,outputBuffer);
     for(int i = 0; i < 5;i++){
      bPlusTree1.Insert(i,new Person("hello " + i));

     }
     Console.WriteLine(bPlusTree1.Traverse());
     bPlusTree1.Delete(1);
     Console.WriteLine(bPlusTree1.Traverse());
    
    //testing redistribution from left
    Console.WriteLine("Testing redistribution of leafnode-entries from left to right");

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
    bPlusTree2.Delete(5);
    Console.WriteLine("After deletion:");
    Console.WriteLine(bPlusTree2.Traverse());

    //Merging with left (leafnode)
    Console.WriteLine("Testing merging with leftsibling (leafnode)");
    bPlusTree2.Delete(2);
    bPlusTree1.Delete(3);
    Console.WriteLine("After deletion:");
    Console.WriteLine(bPlusTree2.Traverse());

    
    //Merging with right (leafnode)
    for(int i = 2; i < 4;i++){
      bPlusTree2.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine("Testing merging with rightsibling (leafnode)");
    bPlusTree2.Delete(2);
    bPlusTree2.Delete(1);
    Console.WriteLine(bPlusTree2.Traverse());
    
    //Testing updated root value
    Console.WriteLine("Testing updated root value");
    BPlusTree<Person> bPlusTree3 = new(5,outputBuffer);
    for(int i = 0; i < 2;i++){
      bPlusTree3.Insert(i,new Person("hello " + i));
    }
    for(int i = 4; i < 7;i++){
      bPlusTree3.Insert(i,new Person("hello " + i));
    }
    
    for(int i = 2; i < 4;i++){
      bPlusTree2.Insert(i,new Person("hello " + i));
    }
    
    Console.WriteLine(bPlusTree3.Traverse());
    bPlusTree3.Delete(4);
    Console.WriteLine(bPlusTree3.Traverse());
    bPlusTree3.Delete(5);
    Console.WriteLine(bPlusTree3.Traverse());
    
    //Updated Root values
    Console.WriteLine("Testing updated root value");
    BPlusTree<Person> bPlusTree4 = new(5,outputBuffer);
    for(int i = 0; i < 2;i++){
      bPlusTree4.Insert(i,new Person("hello " + i));
    }
    for(int i = 4; i < 7;i++){
      bPlusTree4.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree4.Traverse());
    bPlusTree4.Delete(4);
    Console.WriteLine("After deletion:");
    Console.WriteLine(bPlusTree4.Traverse());

    //Deletion from root
    Console.WriteLine("Testing deleting from root");
    BPlusTree<Person> bPlusTree5 = new(5, outputBuffer);
    for(int i = 0; i < 5;i++){
      bPlusTree5.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree5.Traverse());
    bPlusTree5.Delete(1);
    bPlusTree5.Delete(2);
    bPlusTree5.Delete(3);
    bPlusTree5.Delete(4);
    Console.WriteLine("After deletion:");
    Console.WriteLine(bPlusTree5.Traverse());
    
    //Child-Redistribution left to right 
    Console.WriteLine("Testing deletion causing redistribution of child from left to right");
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

    //Child-Redistribution right to left
    Console.WriteLine("Testing deletion causing redistribution of child from right to left");
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
    Console.WriteLine("After redistribution:");
    Console.WriteLine(bPlusTree7.Traverse());

    //Merge with right sibling
    Console.WriteLine("Testing merging with right sibling");
    BPlusTree<Person> bPlusTree8 = new(7, outputBuffer);
    for(int i = 1; i < 26;i++){
      bPlusTree8.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree8.Traverse());
    bPlusTree8.Delete(7);
    Console.WriteLine("After merging:");
    Console.WriteLine(bPlusTree8.Traverse());

    //Merge with left sibling
    Console.WriteLine("Testing merging with left sibling");
    BPlusTree<Person> bPlusTree9 = new(7, outputBuffer);
    for(int i = 1; i < 26;i++){
      bPlusTree9.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree9.Traverse());
    bPlusTree9.Delete(7);
    Console.WriteLine("After merging:");
    Console.WriteLine(bPlusTree9.Traverse());

    //tests with different degrees and random values, arrays inserting, deleting half, etc...

    BPlusTree<Person> bPlusTree10 = new(7, outputBuffer);
    for(int i = 0; i < 50;i++){
      bPlusTree10.Insert(i,new Person("hello " + i));
    }
    Console.WriteLine(bPlusTree10.Traverse());
    for(int i = 15; i < 25;i++){
      bPlusTree10.Delete(i);
    }
    Console.WriteLine(bPlusTree10.Traverse());

    BPlusTree<Person> bPlusTree11 = new(4, outputBuffer);
    bPlusTree11.Insert(1,new Person("hello" + 1));
    for(int i = 0; i < 10;i++){
      bPlusTree11.Insert(i,new Person("hello " + i));
    }
    bPlusTree11.Insert(5, new Person("Testing"));
    for(int i = 5; i < 15;i++){
      bPlusTree11.Insert(i,new Person("hello " + i));
    }
    bPlusTree11.Insert(1,new Person("Andreas" + 1));
    Console.WriteLine(bPlusTree11.Traverse());
    bPlusTree11.Delete(1);
    bPlusTree11.Delete(5);
    Console.WriteLine(bPlusTree11.Traverse());

    Console.WriteLine(bPlusTree11.GetLeafNodesAsString());
    bPlusTree11.RangeQuery(2,8);


    






    




    
    

    



    



    


    
    

  










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
