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
      
      
      int[] deletions = [0,1,2,3,4,5,6,7,8,9,10,11];
      string before;
      for(int i = 0; i < deletions.Length; i++){
        before = _Tree.Traverse();
        Console.WriteLine(_Tree.Traverse());
        #pragma warning disable CS8629 // Nullable value type may be null.
        if (_Tree.Search(deletions[i]) != null){
          _Tree.Delete(deletions[i]);
          if(_Tree.Search(deletions[i]) != null)
            Console.WriteLine("issue with deleting key " + deletions[i]);
        }else{
          _Tree.Delete(deletions[i]);
          if(_Tree.Traverse() != before)
            Console.WriteLine("Deleted a key that should not have been deleted: " + i);
        }
        #pragma warning restore CS8629 // Nullable value type may be null.
        Console.WriteLine(_Tree.Traverse());
      }
    }
}
