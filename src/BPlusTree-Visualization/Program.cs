// Program.cs

using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;


/// <summary>
/// Author: Tristan Anderson
/// Used to indicate the various message types
/// being communicated to the display thread.
/// </summary>
public enum BPlusStatus
{
  /// <summary>
  /// Initial response to Insert Action. Nothing else sent.
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
  /// Initial response to Delete Action. Nothing else sent.
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
  /// Initial response to Search Action. Nothing else sent.
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
/// Author: Tristan Anderson
/// Used to indicate what action to perform on the tree thread.
/// </summary>
public enum Action
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

  }

}
