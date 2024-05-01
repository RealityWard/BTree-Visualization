/*
Author: Andreas Kramer (modified code from B-Tree implementation by Tristan Anderson)
Remark: contains code from BTree implementation (Tristan Anderson and others)
Date: 03/04/2024
Desc: Describes functionality for non-leaf nodes on the B+Tree. Recursive function iteration due to children nodes.
*/
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;
using BTreeVisualizationNode;

namespace BPlusTreeVisualization
{
  
  /// <summary>
  /// Skeleton for B+Tree nodes
  /// </summary>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to
  /// be externally viewed.</param>
    public abstract class BPlusTreeNode<T>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, 
                T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock) 
                : Node<T, BPlusTreeNode<T>>(degree, bufferBlock)
    {
    /// <summary>
    /// Holds key entries for this node.
    /// </summary>
    protected int[] _Keys = new int[degree];
    /// <summary>
    /// Getter of _Keys
    /// </summary>
    public int[] Keys
    {
      get { return _Keys; }
    }
    /// <summary>
    /// Removes the beginning entry of this node.
    /// </summary>
    /// Date: 2024-03-18</remarks>
    public abstract void LosesToLeft();
    /// <summary>
    /// Removes the last entry of this node.
    /// </summary>
    /// Date: 2024-03-18</remarks>
    
    public abstract void LosesToRight();
    /// <summary>
    /// Checks if this node is at max capacity.
    /// </summary>
    /// <returns>True if it is full.</returns>
    /// 
    public bool IsFull()
    {
      return _NumKeys > _Degree - 1;
    }
    /// <summary>
    /// Checks if this node is below minimum capacity.
    /// </summary>
    /// <returns>True if it is below.</returns>
    
    public abstract bool IsUnderflow();
    /// <summary>
    /// Find a key in this node or in its children.
    /// </summary>
    /// <param name="key">Integer to find in _Keys[].</param>
    /// <returns>If found returns the index and this node else returns -1 and this node.</returns>
    public abstract (int, BPlusTreeNode<T>) SearchKey(int key);
    /// <summary>
    /// Insert new entry to this node or one of its children.
    /// Then recognize if a child split and adjust accordingly.
    /// </summary>
    /// <param name="key">Integer to be placed into _Keys[].</param>
    /// <param name="data">Coresponding data to be stored in _Contents[]
    /// at the same index as key in _Keys[].</param>
    /// <returns>If this node reaches capacity it calls split and returns
    /// the new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).
    /// Otherwise it returns ((-1, null), null).</returns>
    /// 
    public abstract ((int, T?), BPlusTreeNode<T>?) InsertKey(int key, T data, long parentID);
    /// <summary>
    /// Delete an entry matching key from this node or child node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson, Date: 2024-02-18</remarks>
    /// <param name="key">Integer to search for and delete if found.</param>
    
    public abstract void DeleteKey(int key, Stack<Tuple<BPlusNonLeafNode<T>,int>> pathStack);
    }
  }