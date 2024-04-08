using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;

namespace ThreadCommunication
{
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
    /// Sent once on an insert to a leaf node occurs thus incrementing the NumKeys attribute. 
    /// ID,NumKeys,Keys,Contents of altered node sent.
    /// In the case of duplicate key ID,-1,[],[].
    /// </summary>
    Inserted,
    /// <summary>
    /// Sent once on an insert to a non-leaf node occurs thus incrementing the NumKeys attribute. 
    /// ID,NumKeys,Keys,Contents of altered node sent.
    /// In the case of duplicate key ID,-1,[],[].
    /// </summary>
    SplitInsert,
    /// <summary>
    /// Sent once the root node splits, indicating a new root node. 
    /// ID,NumKeys,Keys,Contents of root sent.
    /// Followed by two shifts for both children.
    /// </summary>
    NewRoot,
    /// <summary>
    /// Sent once from the node split was called on. Just an ID to know what node is splitting.
    /// ID,-1,[],[]
    /// </summary>
    Split,
    /// <summary>
    /// Sent twice. Once for the node being split and once for the node created.
    /// Alt refers to the parent node.
    /// ID,NumKeys,Keys,Contents,AltID
    /// </summary>
    SplitResult,
    /// <summary>
    /// Initial response to Delete TreeCommand. Nothing else sent.
    /// </summary>
    Delete,
    /// <summary>
    /// Initial response to DeleteRange TreeCommand. Nothing else sent.
    /// </summary>
    DeleteRange,
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
    /// Sent each time "TBD"
    /// ID,NumKeys,Keys,Contents of altered node sent.
    /// ID,-1,[],[] in the case of not found.
    /// </summary>
    DeletedRange,
    /// <summary>
    /// Sent during DeleteRange when tail children of the range are rebalanced.
    /// </summary>
    Rebalanced,
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
    /// Initial response to SearchRange TreeCommand. Nothing else sent.
    /// </summary>
    SearchRange,
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
    /// Sent once at end of SearchKeys. In the case of
    /// some keys found the 
    /// Keys[] will contain the matching keys in the
    /// range from the node SearchKeys was called on.
    /// Contents will contain the corresponding contents in the range.
    /// Non-leaf nodes will send one for each key in the range.
    /// ID,Keys.Length,Keys,Contents
    /// ID,-1,[],[] in the case of nothing found
    /// in the node it was searching.
    /// </summary>
    FoundRange,
    /// <summary>
    /// Sent once the key values of the nonLeafNode are updated
    /// when the leafnodes in the bplustree get deleted, or a merge happens
    /// the key values for the non-leafnodes have to change
    /// sends the ID, Keys
    /// </summary>
    //UpdateKeyValues,
    /// <summary>
    /// Sent if the node had nothing left
    /// during a delete over a range of keys.
    /// Just the ID of the one being deleted.
    /// ID,-1,[],[]
    /// </summary>
    NodeDeleted,
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
    /// Delete a range of keys and the corresponding content
    /// within the tree.
    /// </summary>
    DeleteRange,
    /// <summary>
    /// Search for key within the tree.
    /// </summary>
    Search,
    /// <summary>
    /// Search for all keys and content
    /// within the tree that match the given range.
    /// </summary>
    SearchRange,
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
    List<int> _InsertedKeys = [];
    List<(int, int)> _MixKeys = [];
    int _NumberOfKeys = 1000000;

    var outputBuffer = new BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>();

    var inputBuffer = new BufferBlock<(TreeCommand action, int key, int endKey, Person? content)>();
    BTree<Person> _Tree = new(3, outputBuffer);//This is only defined out here for traversing after the threads are killed to prove it is working.
    // Producer
    Task producer = Task.Run(async () =>
    {
      Thread.CurrentThread.Name = "Producer";
      while (await inputBuffer.OutputAvailableAsync())
      {
        (TreeCommand action, int key, int endKey, Person? content) = inputBuffer.Receive();
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
          case TreeCommand.SearchRange:
            _Tree.Search(key, endKey);
            break;
          case TreeCommand.Traverse:
            Console.WriteLine(_Tree.Traverse());
            break;
          case TreeCommand.Close:
            inputBuffer.Complete();
            _Tree.Close();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            // Console.WriteLine("TreeCommand:{0} not recognized", action);
            break;
        }
      }
    });
    // Consumer
    Task consumer = Task.Run(async () =>
    {
      Console.WriteLine("Consumer");
      // int[] uniqueKeys = [0, 237, 321, 778, 709, 683, 250, 525, 352, 300, 980, 191, 40, 721, 281, 532, 747, 58, 767, 196, 831, 884, 393, 83, 84, 652, 807, 306, 287, 936, 634, 305, 540, 185, 152, 489, 108, 120, 394, 791, 19, 562, 537, 201, 186, 131, 527, 837, 769, 252, 344, 204, 709, 582, 166, 765, 463, 665, 112, 363, 986, 705, 950, 371, 924, 483, 580, 188, 643, 423, 387, 293, 93, 918, 85, 660, 135, 990, 768, 753, 894, 332, 902, 800, 195, 374, 18, 282, 369, 296, 76, 40, 940, 852, 983, 362, 941, 7, 725, 732, 647];
      // int[] uniqueKeys1 = [0, 237, 321, 778, 709, 683, 250, 525, 352, 300, 980, 40, 721, 281, 532, 747, 58, 767, 196, 831, 884, 393, 83, 84];
      // foreach (int uniqueKey in uniqueKeys)
      // {
      //   inputBuffer.Post((TreeCommand.Insert, uniqueKey, new Person(uniqueKey.ToString())));
      // }
      // inputBuffer.Post((TreeCommand.Traverse, -1, new Person((-1).ToString())));
      // inputBuffer.Post((TreeCommand.Close, -1, new Person((-1).ToString())));
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
          case NodeStatus.FoundRange:
            // Console.WriteLine(StringifyKeys(history.Last().numKeys,history.Last().keys));
            break;
          case NodeStatus.Close:
            outputBuffer.Complete();
            break;
          default:// Will close threads upon receiving a bad TreeCommand.
            // Console.WriteLine("TreeCommand:{0} not recognized", history.Last().status);
            break;
        }
      }
    });
    Task sideTask = Task.Run(async () =>
    {
      Random random = new();
      int key;
      for (int i = 0; i < _NumberOfKeys / 10 + 10; i++)
      {
        do
        {
          key = random.Next(1, _NumberOfKeys * 10);
        } while (_InsertedKeys.Contains(key));
        await inputBuffer.SendAsync((TreeCommand.Insert, key, -1
          , new Person(key.ToString())));
        _InsertedKeys.Add(key);
      }
      for (int i = 0; i < _NumberOfKeys / 10; i++)
      {
        key = random.Next(0, 3);
        if (key == 1)
        {
          do
          {
            key = random.Next(1, _NumberOfKeys * 10);
          } while (_InsertedKeys.Contains(key));
          await inputBuffer.SendAsync((TreeCommand.Insert, key, -1
            , new Person(key.ToString())));
          _InsertedKeys.Add(key);
          _MixKeys.Add((1, key));
        }
        else if (key == 2)
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await inputBuffer.SendAsync((TreeCommand.Delete, key, -1
            , null));
          _InsertedKeys.Remove(key);
          _MixKeys.Add((0, key));
        }
        else
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await inputBuffer.SendAsync((TreeCommand.SearchRange, key, key + 10
            , null));
        }
      }
      await inputBuffer.SendAsync((TreeCommand.Close, -1, -1, new Person((-1).ToString())));
    });
    Console.WriteLine("Which is first?");
    producer.Wait();
    consumer.Wait();
    Console.WriteLine("Done");
    // Console.WriteLine(_Tree.Traverse());
    int minHeight = _Tree.GetMinHeight();
    int maxHeight = _Tree.GetMaxHeight();
    Console.WriteLine(minHeight + " " + maxHeight);
  }

  /// <summary>
  /// Read out just the portion of the keys[] currently in use.
  /// </summary>
  /// <param name="numKeys">Index to stop at.</param>
  /// <param name="keys">Array of ints</param>
  /// <returns>String of the keys seperated by a ','</returns>
  private static string StringifyKeys(int numKeys, int[] keys)
  {
    string result = "";
    for (int i = 0; i < numKeys; i++)
      result += keys[i] + (i + 1 == numKeys ? "" : ",");
    return result;
  }
}
