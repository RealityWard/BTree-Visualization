/**
Primary Author: Tristan Anderson
Secondary Author: Andreas Kramer (for tree height methods)
Date: 2024-02-03
Desc: Maintains the entry point of the BTree data
structure and initializes root and new node creation in the beginning.
*/
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;

namespace BTreeVisualization
{
  public class BTree<T>
  {
    /// <summary>
    /// Entry point of the tree.
    /// </summary>
    private BTreeNode<T> _Root;
    /// <summary>
    /// Determines the number of keys and children per node.
    /// </summary>
    public readonly int _Degree;
    /// <summary>
    /// Tracks whether a key of zero is in use in the tree.
    /// </summary>
    private bool zeroKeyUsed;
    private BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> _BufferBlock;

    public BTree(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)
    {
      _Degree = degree;
      _Root = new LeafNode<T>(degree, bufferBlock);
      zeroKeyUsed = false;
      _BufferBlock = bufferBlock;
      _BufferBlock.SendAsync((NodeStatus.Inserted, _Root.ID, _Root.NumKeys, _Root.Keys, _Root.Contents, 0, -1, [], []));
    }

    /// <summary>
    /// Takes a node and the key and data to place into root.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="rNode">Value returned from InsertKey on _Root node.</param>
    private void Split(((int, T), BTreeNode<T>) rNode)
    {
      _Root = new NonLeafNode<T>(_Degree, [rNode.Item1.Item1], [rNode.Item1.Item2]
        , [_Root, rNode.Item2], _BufferBlock);
      _BufferBlock.SendAsync((NodeStatus.NewRoot, _Root.ID, _Root.NumKeys, _Root.Keys, _Root.Contents, 0, -1, [], []));
      _BufferBlock.SendAsync((NodeStatus.Shift, _Root.ID, -1, [], [], (((NonLeafNode<T>)_Root).Children[0]
        ?? throw new NullChildReferenceException($"Child of new root node at index:0")).ID, -1, [], []));
      _BufferBlock.SendAsync((NodeStatus.Shift, _Root.ID, -1, [], [], (((NonLeafNode<T>)_Root).Children[1]
        ?? throw new NullChildReferenceException($"Child of new root node at index:1")).ID, -1, [], []));
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
        /** Due to initializing all int[] entries as zero instead of null
          We must use a boolean to detect whether or not to 
          allow a zero key insertion*/
        if (key == 0)
          zeroKeyUsed = true;
        _BufferBlock.SendAsync((NodeStatus.Insert, 0, -1, [key], [data], 0, -1, [], []));
        ((int, T?), BTreeNode<T>?) result = _Root.InsertKey(key, data, 0);
        if (result.Item2 != null && result.Item1.Item2 != null)
        {
#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
          Split(result);
#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }
      }
      else
      {
        _BufferBlock.SendAsync((NodeStatus.Inserted, 0, -1, [], [], 0, -1, [], []));
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
      _BufferBlock.SendAsync((NodeStatus.Delete, 0, -1, [], [], 0, -1, [], []));
      if (key == 0 && zeroKeyUsed)
        zeroKeyUsed = false; // After deletion there will no longer be a zero key in use, thus must re-enable insertion of zero
      _Root.DeleteKey(key);
      if (_Root.NumKeys == 0 && _Root as NonLeafNode<T> != null)
      {
        long temp = _Root.ID;
        _Root = ((NonLeafNode<T>)_Root).Children[0]
          ?? throw new NullChildReferenceException(
            $"Child of child on root node");
        _BufferBlock.SendAsync((NodeStatus.Merge, _Root.ID, _Root.NumKeys, _Root.Keys, _Root.Contents, temp, -1, [], []));
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
      _BufferBlock.SendAsync((NodeStatus.Search, 0, -1, [], [], 0, -1, [], []));
      (int key, T content)? result = _Root.SearchKey(key);
      if(result == null)
      {
        return default;
      }
      else
      {
        return result.Value.content;
      }
    }

    /// <summary>
    /// Invokes the SearchKey overload 1 on the root node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Lower bound inclusive.</param>
    /// <param name="endKey">Upper bound exclusive.</param>
    /// <returns>A list of key-value pairs from the matching range in order of found.</returns>
    public List<(int key, T content)> Search(int key, int endKey)
    {
      _BufferBlock.SendAsync((NodeStatus.Search, 0, -1, [], [], 0, -1, [], []));
      List<(int key, T value)> result = _Root.SearchKeys(key, endKey);
      if (result.Count > 0)
      {
        int[] keys = new int[result.Count];
        T[] contents = new T[result.Count];
        for (int i = 0; i < result.Count; i++)
        {
          keys[i] = result[i].key;
          contents[i] = result[i].value;
        }
        _BufferBlock.SendAsync((NodeStatus.FoundRange, _Root.ID, result.Count, keys, contents, 0, -1, [], []));
      }
      else
      {
        _BufferBlock.SendAsync((NodeStatus.FoundRange, _Root.ID, -1, [], [], 0, -1, [], []));
      }
      return result;
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
      _Root = new LeafNode<T>(_Degree, _BufferBlock);
    }

    public void Close()
    {
      _BufferBlock.SendAsync((NodeStatus.Close, 0, -1, [], [], 0, -1, [], []));
    }

    /// <summary>
    /// Author: Andreas Kramer
    /// Calculates the Height of the B-Tree and returns it as an integer, assuming it is correctly balanced
    /// </summary>
    /// <returns></returns>

    public int GetTreeHeight()
    {
      if (_Root == null)
      {
        return 0;
      }
      int height = 1;
      BTreeNode<T> currentNode = _Root;
      while (currentNode is NonLeafNode<T> nonLeafNode && nonLeafNode.Children.Length > 0)
      {
        currentNode = nonLeafNode.Children[0] ?? throw new NullChildReferenceException(
          $"Child at index:0 within node:{nonLeafNode.ID}");
        height++;
      }
      return height;
    }

    /// <summary>
    /// Author: Andreas Kramer
    /// Calculates the Minimum Height of the B-Tree
    /// </summary>
    /// <returns> minimum height of the B-tree as an integer
    /// 
    public int GetMinHeight()
    {
      return GetMinHeight(_Root, 0);
    }
    static private int GetMinHeight(BTreeNode<T> node, int currentLevel)
    {
      if (node == null || node is LeafNode<T>)
      {
        return currentLevel + 1;
      }
      if (node is NonLeafNode<T> nonLeafNode)
      {
        int minHeight = int.MaxValue;
        foreach (var child in nonLeafNode.Children)
        {
          if (child != null)
          {
            int childHeight = GetMinHeight(child, currentLevel + 1);
            if (childHeight < minHeight)
            {
              minHeight = childHeight;
            }
          }
        }
        return minHeight;
      }
      return currentLevel;
    }

    /// <summary>
    /// Author: Andreas Kramer
    /// Calculates the Maximum Height of the B-Tree and returns it as an integer
    /// </summary>
    /// <returns></returns>
    /// 
    public int GetMaxHeight()
    {
      return GetMaxHeight(_Root, 0);
    }
    private int GetMaxHeight(BTreeNode<T> node, int currentLevel)
    {
      if (node == null || node is LeafNode<T>)
      {
        return currentLevel + 1;
      }
      if (node is NonLeafNode<T> nonLeafNode)
      {
        int maxHeight = currentLevel;
        foreach (var child in nonLeafNode.Children)
        {
          if (child != null)
          {
            int childHeight = GetMinHeight(child, currentLevel + 1);
            if (childHeight > maxHeight)
            {
              maxHeight = childHeight;
            }
          }
        }
        return maxHeight;
      }
      return currentLevel;
    }

    /// <summary>
    /// Author: Andreas Kramer
    /// Method to check if the leafNodes of the tree are all on the same level (checking for tree balance)
    /// Returns a boolean that indicates whether the BTree is balanced or not, only returns true if all subtrees are balanced
    /// </summary>
    /// <returns></returns>

    public bool IsBalanced()
    {
      if (_Root == null)
      {
        return true;
      }
      int leafLevel = -1;
      return CheckNodeBalance(_Root, 0, ref leafLevel);
    }

    static private bool CheckNodeBalance(BTreeNode<T> node, int currentLevel, ref int leafLevel)
    {
      if (node is LeafNode<T>)
      {
        if (leafLevel == -1)
        {
          leafLevel = currentLevel;
          return true;
        }
        return currentLevel == leafLevel;
      }

      if (node is NonLeafNode<T> nonLeafNode)
      {
        foreach (var child in nonLeafNode.Children)
        {
          if (child != null && !CheckNodeBalance(child, currentLevel + 1, ref leafLevel))
          {
            return false;
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Gets the total number of keys in this tree.
    /// </summary>
    /// <returns>Count of all keys.</returns>
    public long KeyCount()
    {
      if (_Root as NonLeafNode<T> != null)
        return NonLeafNode<T>.KeyCount((NonLeafNode<T>)_Root);
      else
        return _Root.NumKeys;
    }

    /// <summary>
    /// Gets the total number of nodes in this tree.
    /// </summary>
    /// <returns>Count of all keys.</returns>
    public int NodeCount()
    {
      if (_Root as NonLeafNode<T> != null)
        return NonLeafNode<T>.NodeCount((NonLeafNode<T>)_Root);
      else
        return 1;
    }
  }
}
