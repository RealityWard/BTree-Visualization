// Program.cs

using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;

class Program
{
    static void Main()
    {
      Console.WriteLine("Hello, world!");
      BTree<Person> _Tree = new(3);
      for(int i = 0; i < 100; i++){
        _Tree.Insert(i,new Person(i.ToString()));
      }

      Random random = new Random();
      int[] uniqueKeys = new int[100];
      for(int i = 0; i < uniqueKeys.Length; i++){
        uniqueKeys[i] = random.Next(1,1000);
      }
      
      foreach (int key in uniqueKeys) {
          _Tree.Insert(key, new Person("Name"));
      }
      List<int> keysToDelete = new List<int>();
      List<int> allKeys = uniqueKeys.ToList();
      
      for (int i = 0; i < 100; i++) {
        random = new Random();
          int keyIndex = random.Next(allKeys.Count);
          int key = allKeys[keyIndex];
          string before = key + "here---------------------------------------------------------------------------------------------------------------"
             + "\n" +_Tree.Traverse();
          _Tree.Delete(key);
          string after = "\n" +_Tree.Traverse();
          if(_Tree.Search(key) != null){
            Console.WriteLine(before + after);
          }
          keysToDelete.Add(key);
          allKeys.RemoveAt(keyIndex);
      }
    }

}
