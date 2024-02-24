// Program.cs

using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;

class Program
{
  static void Main()
  {
    BTree<Person> _Tree = new(3);
    for (int i = 0; i < 100; i++)
    {
      _Tree.Insert(i, new Person(i.ToString()));
    }

    /**
    string before1 = _Tree.Traverse();
    _Tree.Delete(71);
    string after1 = _Tree.Traverse();
    for(int i = 0; i < 100; i++){
      string checkDup = _Tree.Traverse();
      if(Regex.Count(checkDup,"\"" + i + "\"") > 1){
        Console.WriteLine(checkDup);
      }
    // }
    Random random = new Random();
    int[] uniqueKeys = new int[100];
    for (int i = 0; i < uniqueKeys.Length; i++)
    {
      uniqueKeys[i] = random.Next(1, 1000);
      Console.Write(uniqueKeys[i] + ",");
    }
    Console.WriteLine("----------------");
    foreach (int key in uniqueKeys)
    {
      _Tree.Insert(key, new Person(key.ToString()));
    }
    List<int> keysToDelete = new List<int>();
    List<int> allKeys = uniqueKeys.ToList();

    for (int i = 0; i < 100; i++)
    {
      random = new Random();
      int keyIndex = random.Next(allKeys.Count);
      int key = allKeys[keyIndex];
      Console.Write(key + ",");
      // string before = key + "here---------------------------------------------------------------------------------------------------------------"
      //    + "\n" +_Tree.Traverse();
      _Tree.Delete(key);
      // string after = "\n" +_Tree.Traverse();
      // if(_Tree.Search(key) != null){
      //   Console.WriteLine(before + after);
      // }
      keysToDelete.Add(key);
      allKeys.RemoveAt(keyIndex);
      string checkDup = _Tree.Traverse();
      if (Regex.Count(checkDup, "\"" + key + "\"") > 1)
      {
        // Console.WriteLine(checkDup);
      }
      foreach (int keyCheck in uniqueKeys)
      {
        checkDup = _Tree.Traverse();
        if (Regex.Count(checkDup, "\"" + keyCheck + "\"") > 1)
        {
          Console.WriteLine(checkDup);
        }
      }
    }
    */
    
    int[] uniqueKeys = [237,321,778,709,683,250,525,352,300,980,191,40,721,281,532,747,58,767,196,831,884,393,83,84,652,807,306,287,936,634,305,540,185,152,489,108,120,394,791,19,562,537,201,186,131,527,837,769,252,344,204,709,582,166,765,463,665,112,363,986,705,950,371,924,483,580,188,643,423,387,293,93,918,85,660,135,990,768,753,894,332,902,800,195,374,18,282,369,296,76,40,940,852,983,362,941,7,725,732,647];
    foreach (int key in uniqueKeys)
    {
      _Tree.Insert(key, new Person(key.ToString()));
    }

    int[] deleteKeys = [709,769,562,532,791,195,387,527,643,18,582,540,362,305,709,747,778,332,924,201,463,990,665,652,135,250,58,831,837,344,363,321,108,732,525,120,894,852,306,807,186,252,300,634,660,237,281,983,483,980,84,918,950,282,93,936,287,941,768,188,131,293,767,166,683,83,371,705,369,765,489,721,725,374,191,352,580,152,76,112,296,40,884,85,40,940,19,394,204,647,537,196,902,7,800,185,423,986,393,753];
    for (int i = 0; i < deleteKeys.Length; i++)
    {
      // string before = key + "here---------------------------------------------------------------------------------------------------------------"
      //    + "\n" +_Tree.Traverse();
      string checkDup = _Tree.Traverse();
      _Tree.Delete(deleteKeys[i]);
      for(int j = 0; j <= i; j++){
        if(_Tree.Search(deleteKeys[j]) != null)
        {
          Console.WriteLine(_Tree.Search(deleteKeys[j]));
        }
      }
      // string after = "\n" +_Tree.Traverse();
      // if(_Tree.Search(key) != null){
      //   Console.WriteLine(before + after);
      // }
      if (Regex.Count(checkDup, "\"" + deleteKeys[i] + "\"") > 1)
      {
        // Console.WriteLine(checkDup);
      }
      foreach (int keyCheck in uniqueKeys)
      {
        checkDup = _Tree.Traverse();
        if (Regex.Count(checkDup, "\"" + keyCheck + "\"") > 1)
        {
          Console.WriteLine(checkDup);
        }
      }
    }
  }

}
