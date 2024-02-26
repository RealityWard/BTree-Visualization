/**
Desc: Maintains the entry point of the BTree data 
structure and initializes root and new node creation in the beginning.
*/
using System.Threading.Tasks.Dataflow;
namespace BTreeVisualization
{
  public class BTree<T>(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock)
  {
    /// <summary>
    /// Entry point of the tree.
    /// </summary>
    private BTreeNode<T> _Root = new LeafNode<T>(degree, bufferBlock);
    /// <summary>
    /// Determines the number of keys and children per node.
    /// </summary>
    private readonly int _Degree = degree;
    /// <summary>
    /// Tracks whether a key of zero is in use in the tree.
    /// </summary>
    private bool zeroKeyUsed = false;

    /// <summary>
    /// Takes a node and the key and data to place into root.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="rNode">Value returned from InsertKey on _Root node.</param>
    private void Split(((int, T), BTreeNode<T>) rNode)
    {
      _Root = new NonLeafNode<T>(_Degree, [rNode.Item1.Item1], [rNode.Item1.Item2]
        , [_Root, rNode.Item2], bufferBlock);
    }

    /// <summary>
    /// Inserts data at root node and set root to a new 
    /// leaf node if root isn't yet created. It also 
    /// checks for a split afterwards.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to insert into the tree.</param>
    /// <param name="data">Coresponding data belonging to key.</param>
    public void Insert(int key, T data)
    {
      if ((key == 0 && !zeroKeyUsed) || key != 0)
      {
        if (key == 0) // Due to initializing all int[] entries as zero instead of null
          zeroKeyUsed = true; // We must use a boolean to detect whether or not to 
                              // allow a zero key insertion
        bufferBlock.Post((Status.Insert, 0, -1, [], [], 0, -1, [], []));
        ((int, T?), BTreeNode<T>?) result = _Root.InsertKey(key, data);
        if (result.Item2 != null && result.Item1.Item2 != null)
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
          Split(result);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }
      }
    }

    /// <summary>
    /// Invokes a delete through out the tree to find 
    /// an entry matching key and deletes it. If there 
    /// are duplicates only the first encountered is deleted. 
    /// If after the delete the root is reduced too 
    /// much it will grab its remaining child and make that the root.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="key">Integer to search for and delete if found.</param>
    public void Delete(int key)
    {
      if (key == 0 && zeroKeyUsed)
        zeroKeyUsed = false; // After deletion there will no longer be a zero key in use, thus must re-enable insertion of zero
      _Root.DeleteKey(key);
      if (_Root.NumKeys == 0 && _Root as NonLeafNode<T> != null)
      {
        _Root = ((NonLeafNode<T>)_Root).Children[0];
      }
    }

    /// <summary>
    /// Using searchKey on the nodes to return the data 
    /// correlated to the key.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Number to search for.</param>
    /// <returns>Data object stored under key.</returns>
    public T? Search(int key)
    {
      (int, BTreeNode<T>?) result = _Root.SearchKey(key);
      if (result.Item1 == -1 || result.Item2 == null)
      {
        return default;
      }
      else
      {
        return result.Item2.Contents[result.Item1];
      }
    }

    /// <summary>
    /// Invokes Traverse recursively through out the 
    /// tree to return a json print out of all nodes.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Entire tree object as a JSON string.</returns>
    public string Traverse()
    {
      return _Root.Traverse("Root");
    }

    public void Clear()
    {
      _Root = new LeafNode<T>(_Degree, bufferBlock);
    }
  }
}