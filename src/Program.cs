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
      for(int i = 0; i < 7; i++){
        _Tree.Insert(i,new Person(i.ToString()));
      }
      Console.WriteLine(_Tree.Traverse());
    }
}
