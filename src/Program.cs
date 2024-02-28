// Program.cs

using System.Text.RegularExpressions;
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

public enum Status
{
  Insert,
  ISearching,
  Inserted,
  Split,
  Delete,
  DSearching,
  Deleted,
  Merge,
  Search,
  SSearching,
  Found,
  Close
}

public enum Action
{
  Tree,
  Insert,
  Close
}

// namespace BTreeVisualization
// {
//   public static class Global
//   {     
//     public static BufferBlock<(
//       Status status,
//       long id,
//       int[] keys,
//       string[] contents,
//       long altID,
//       int[] altKeys,
//       string[] altContents
//       )> OUTPUT_BUFFER = new();

//   }
// }

class Program
{
  static void Main()
  {
    /* 
    BTree<Person> _Tree = new(3);
    for (int i = 0; i < 100; i++)
    {
      _Tree.Insert(i, new Person(i.ToString()));
    }
    */
    Thread.CurrentThread.Name = "Main";

    var outputBuffer = new BufferBlock<(
      Status status,
      long id,
      int[] keys,
      Person[] contents,
      long altID,
      int[] altKeys,
      Person[] altContents
      )>();

    var inputBuffer = new BufferBlock<(
      Action action,
      int key,
      Person content
      )>();
    BTree<Person> _Tree = new(3,outputBuffer);
    // Producer
    Task producer = Task.Run(async () =>
    {
      Console.WriteLine("Producer");
      while (await inputBuffer.OutputAvailableAsync())
      {
        (Action action, int key, Person content) = await inputBuffer.ReceiveAsync();
        if (Action.Tree == action)
        {
          _Tree = new(key,outputBuffer);
        }
        else if (Action.Insert == action)
        {
          // await outputBuffer.SendAsync((Status.Insert, 0, [key], [content.ToString()], 0, [], ["Working"]));
          _Tree.Insert(key, content);
          // outputBuffer.Complete();
        }else if(Action.Close == action){
          outputBuffer.Post((Status.Close,-1,[],[],-1,[],[]));
          outputBuffer.Complete();
        }
      }
    });
    // Consumer
    Task consumer = Task.Run(async () =>
    {
      Console.WriteLine("Consumer");
      int[] uniqueKeys = [0, 237, 321, 778, 709, 683, 250, 525, 352, 300, 980, 191, 40, 721, 281, 532, 747, 58, 767, 196, 831, 884, 393, 83, 84, 652, 807, 306, 287, 936, 634, 305, 540, 185, 152, 489, 108, 120, 394, 791, 19, 562, 537, 201, 186, 131, 527, 837, 769, 252, 344, 204, 709, 582, 166, 765, 463, 665, 112, 363, 986, 705, 950, 371, 924, 483, 580, 188, 643, 423, 387, 293, 93, 918, 85, 660, 135, 990, 768, 753, 894, 332, 902, 800, 195, 374, 18, 282, 369, 296, 76, 40, 940, 852, 983, 362, 941, 7, 725, 732, 647];
      int[] uniqueKeys1 = [0, 237, 321, 778, 709, 683, 250, 525, 352, 300, 980, 40, 721, 281, 532, 747, 58, 767, 196, 831, 884, 393, 83, 84];
      foreach (int key in uniqueKeys1)
      {
        inputBuffer.Post((Action.Insert, key, new Person(key.ToString())));
      }
      inputBuffer.Post((Action.Close, -1, new Person((-1).ToString())));
      while (await outputBuffer.OutputAvailableAsync())
      {
        (Status status,
        long id,
        int[] keys,
        Person[] contents,
        long altID,
        int[] altKeys,
        Person[] altContents) = await outputBuffer.ReceiveAsync();
        if(Status.Close != status){
          Console.WriteLine("Status Code: {0}\nID: {1}",status,id);
        }else{
          inputBuffer.Complete();
        }
      }
    });
    Console.WriteLine("Which is first?");
    producer.Wait();
    consumer.Wait();
    Console.WriteLine("Done");
    Console.WriteLine(_Tree.Traverse());
    int minHeight = _Tree.GetMinHeight();
    int maxHeight = _Tree.GetMaxHeight();
    Console.WriteLine(minHeight + " " + maxHeight);

    /* Deletion Testing
    int[] uniqueKeys = [237, 321, 778, 709, 683, 250, 525, 352, 300, 980, 191, 40, 721, 281, 532, 747, 58, 767, 196, 831, 884, 393, 83, 84, 652, 807, 306, 287, 936, 634, 305, 540, 185, 152, 489, 108, 120, 394, 791, 19, 562, 537, 201, 186, 131, 527, 837, 769, 252, 344, 204, 709, 582, 166, 765, 463, 665, 112, 363, 986, 705, 950, 371, 924, 483, 580, 188, 643, 423, 387, 293, 93, 918, 85, 660, 135, 990, 768, 753, 894, 332, 902, 800, 195, 374, 18, 282, 369, 296, 76, 40, 940, 852, 983, 362, 941, 7, 725, 732, 647];
    foreach (int key in uniqueKeys)
    {
      _Tree.Insert(key, new Person(key.ToString()));
    }

    int[] deleteKeys = [709, 769, 562, 532, 791, 195, 387, 527, 643, 18, 582, 540, 362, 305, 709, 747, 778, 332, 924, 201, 463, 990, 665, 652, 135, 250, 58, 831, 837, 344, 363, 321, 108, 732, 525, 120, 894, 852, 306, 807, 186, 252, 300, 634, 660, 237, 281, 983, 483, 980, 84, 918, 950, 282, 93, 936, 287, 941, 768, 188, 131, 293, 767, 166, 683, 83, 371, 705, 369, 765, 489, 721, 725, 374, 191, 352, 580, 152, 76, 112, 296, 40, 884, 85, 40, 940, 19, 394, 204, 647, 537, 196, 902, 7, 800, 185, 423, 986, 393, 753];
    for (int i = 0; i < deleteKeys.Length; i++)
    {
      // string before = key + "here---------------------------------------------------------------------------------------------------------------"
      //    + "\n" +_Tree.Traverse();
      string checkDup = _Tree.Traverse();
      _Tree.Delete(deleteKeys[i]);
      for (int j = 0; j <= i; j++)
      {
        if (_Tree.Search(deleteKeys[j]) != null)
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
     */
  }

}
