using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;

namespace ThreadCommunication{
  /// <summary>
  /// Used to indicate the various message types
  /// being communicated to the display thread.
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  public enum NodeStatus
  {
    /// <summary>
    /// Initial response to Insert TreeCommand. Nothing else sent.
    /// </summary>
    Insert,
    /// <summary>
    /// Sent everytime InsertKey is called on a node. Only ID sent.
    /// </summary>
    ISearching,
    /// <summary>
    /// Sent once an insert to node occurs thus incrementing the NumKeys attribute. 
    /// ID,NumKeys,Keys,Contents of altered node sent.
    /// In the case of duplicate key ID,-1,[],[].
    /// </summary>
    Inserted,
    /// <summary>
    /// Sent once from the node split was called on. Alt refers to new sibiling node.
    /// ID,NumKeys,Keys,Contents,AltID,AltNumKeys,AltKeys,AltContents
    /// All values will be sent to update existing node and create sibiling node.
    /// </summary>
    Split,
    /// <summary>
    /// Initial response to Delete TreeCommand. Nothing else sent.
    /// </summary>
    Delete,
    /// <summary>
    /// Sent everytime DeleteKey is called on a node. Only ID sent.
    /// </summary>
    DSearching,
    /// <summary>
    /// Sent once a key was found or end of search.
    /// ID,NumKeys,Keys,Contents of altered node sent.
    /// ID,-1,[],[] in the case of not found.
    /// </summary>
    Deleted,
    /// <summary>
    /// Sent everytime ForfeitKey is called on a node. Only ID sent.
    /// </summary>
    FSearching,
    /// <summary>
    /// Sent once it retrieves a key from a leaf node.
    /// ID,NumKeys,Keys,Contents
    /// values will be sent to update existing node.
    /// </summary>
    Forfeit,
    /// <summary>
    /// Sent once from the node merge was called on. Alt refers the sibiling node being eaten.
    /// ID,NumKeys,Keys,Contents,AltID
    /// values will be sent to update existing node and delete sibiling node.
    /// </summary>
    Merge,
    /// <summary>
    /// Sent once after merging child nodes.
    /// ID,NumKeys,Keys,Contents
    /// values will be sent to update existing node.
    /// </summary>
    MergeParent,
    /// <summary>
    /// Sent when a full merge is not possible thus one sibiling takes a
    /// bite out of its sibiling. Alt refers the sibiling node being biten.
    /// ID,NumKeys,Keys,Contents,AltID,AltNumKeys,AltKeys,AltContents
    /// </summary>
    UnderFlow,
    /// <summary>
    /// During both split and merge children will need to update who they point to.
    /// Alt refers to child node.
    /// ID,-1,[],[],AltID
    /// </summary>
    Shift,
    /// <summary>
    /// Initial response to Search TreeCommand. Nothing else sent.
    /// </summary>
    Search,
    /// <summary>
    /// Sent everytime SearchKey is called on a node. Only ID sent.
    /// </summary>
    SSearching,
    /// <summary>
    /// Sent once a key was found or end of search.
    /// In case of found, NumKeys will be index of the key in the node.
    /// Keys will contain only the key searched for.
    /// Contents will contain only the content belonging to the key.
    /// ID,NumKeys,Keys,Contents
    /// ID,-1,[],[] in the case of not found.
    /// </summary>
    Found,
    /// <summary>
    /// Sent to close/complete the buffer and as a result
    /// terminate the thread using this buffer.
    /// </summary>
    Close
  }

  /// <summary>
  /// Used to indicate what action to perform on the tree thread.
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  public enum TreeCommand
  {
    /// <summary>
    /// Create new tree with the degree set from key attribute.
    /// </summary>
    Tree,
    /// <summary>
    /// Insert into tree object key with content.
    /// </summary>
    Insert,
    /// <summary>
    /// Delete key and the corresponding content
    /// within the tree.
    /// </summary>
    Delete,
    /// <summary>
    /// Search for key within the tree.
    /// </summary>
    Search,
    /// <summary>
    /// Console output the tree traversal.
    /// </summary>
    Traverse,
    /// <summary>
    /// Sent to close/complete the buffer and as a result
    /// terminate the thread using this buffer.
    /// </summary>
    Close
  }
}

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

    var inputBuffer = new BufferBlock<(
      TreeCommand action,
      int key,
      Person? content
      )>();
    BTree<Person> _Tree = new(3, outputBuffer);//This is only defined out here for traversing after the threads are killed to prove it is working.
    // Producer
    Task producer = Task.Run(async () =>
    {
      Thread.CurrentThread.Name = "Producer";
      while (await inputBuffer.OutputAvailableAsync())
      {
        (TreeCommand action, int key, Person? content) = inputBuffer.Receive();
        switch (action)
        {
          case TreeCommand.Tree:
            _Tree = new(key, outputBuffer);
            break;
          case TreeCommand.Insert:
            _Tree.Insert(key, content 
              ?? throw new NullContentReferenceException(
                "Insert on tree with null content."));
            break;
          case TreeCommand.Delete:
            _Tree.Delete(key);
            break;
          case TreeCommand.Search:
            _Tree.Search(key);
            break;
          case TreeCommand.Traverse:
            Console.WriteLine(_Tree.Traverse());
            break;
          case TreeCommand.Close:
            inputBuffer.Complete();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            inputBuffer.Complete();
            Console.WriteLine("TreeCommand:{0} not recognized", action);
            break;
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
        inputBuffer.Post((TreeCommand.Insert, key, new Person(key.ToString())));
      }
      inputBuffer.Post((TreeCommand.Traverse, -1, new Person((-1).ToString())));
      inputBuffer.Post((TreeCommand.Close, -1, new Person((-1).ToString())));
      List<(NodeStatus status,
        long id,
        int numKeys,
        int[] keys,
        Person?[] contents,
        long altID,
        int altNumKeys,
        int[] altKeys,
        Person?[] altContents)> history = [];
      while (await outputBuffer.OutputAvailableAsync())
      {
        history.Add(outputBuffer.Receive());
        switch (history.Last().status)
        {
          case NodeStatus.Close:
            inputBuffer.Post((TreeCommand.Close, -1, null));
            outputBuffer.Complete();
            break;
          default:// Will close threads upon receiving a bad TreeCommand.
            inputBuffer.Post((TreeCommand.Close, -1, null));
            outputBuffer.Complete();
            Console.WriteLine("TreeCommand:{0} not recognized", history.Last().status);
            break;
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
