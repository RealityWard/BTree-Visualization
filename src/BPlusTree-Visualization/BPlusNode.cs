/*
Author: Andreas Kramer (modified code from B-Tree implementation by Tristan Anderson)
Remark: contains code from BTree implementation (Tristan Anderson and others)
Date: 03/04/2024
Desc: Describes functionality for non-leaf nodes on the B+Tree. Recursive function iteration due to children nodes.
*/
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;


namespace BPlusTreeVisualization
{
  /// <summary>
  /// Base class for all the node objects used in the BTree and B+Tree.
  /// </summary>
  /// <remarks>Author: Tristan Anderson, Secondary Author: Andreas Kramer</remarks>
  /// <typeparam name="N">Type of node.</typeparam>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent non-leaf node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to
  /// be externally viewed.</param>
  public abstract class Node<N, T>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)
  {
    /// <summary>
    /// Output Buffer for Status updates to be externally viewed.
    /// </summary>
    protected BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> _BufferBlock = bufferBlock;
    /// <summary>
    /// Determines the number of keys and children per node.
    /// </summary>
    protected readonly int _Degree = degree;
    /// <summary>
    /// Identifier to be unique per node for reference on gui side.
    /// </summary>
    protected long _ID = DateTime.Now.Ticks;
    /// <summary>
    /// Current count of key entries to this node. Marks the last index - 1 to perceive in _Keys[]
    /// </summary>
    protected int _NumKeys = 0;
    /// <summary>
    /// Holds key entries for this node.
    /// </summary>
    protected int[] _Keys = new int[degree];

    /// <summary>
    /// Find a key in this node or in its children.
    /// </summary>
    /// <param name="key">Integer to find in _Keys[].</param>
    /// <returns>If found returns the index and this node else returns -1 and this node.</returns>
    public abstract (int, N) SearchKey(int key);
    
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
    /// */
    public abstract ((int, T?), N?) InsertKey(int key, T data, long parentID);

    /// <summary>
    /// Delete an entry matching key from this node or child node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson, Date: 2024-02-18</remarks>
    /// <param name="key">Integer to search for and delete if found.</param>
    
    public abstract void DeleteKey(int key, Stack<Tuple<BPlusNonLeafNode<T>,int>> pathStack);

    /// <summary>
    /// Prints out this node and its children.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Hierachical Node ID</param>
    /// <returns>String in JSON syntax.</returns>
    
    
    public abstract string Traverse(string x);
    /// <summary>
    /// Getter of _Keys
    /// </summary>
    public int[] Keys
    {
      get { return _Keys; }
    }
    /// <summary>
    /// Getter of _ID
    /// </summary>
    public long ID
    {
      get { return _ID; }
    }
    /// <summary>
    /// Getter of _NumKeys
    /// </summary>
    public int NumKeys
    {
      get { return _NumKeys; }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Creates a string made up of a number of spaces equal to the length of input minus four.
    /// Author: Tristan Anderson
    /// </summary>
    /// <param name="input">String with length required.</param>
    /// <returns>A string of spaces.</returns>
    public static string Spacer(string input)
    {
      return Spacer(input.Length);
    }

    /// <summary>
    /// Creates a string made up of a number of spaces equal to the length of input minus four.
    /// Author: Tristan Anderson
    /// </summary>
    /// <param name="input">Length of string.</param>
    /// <returns>A string of spaces.</returns>
    public static string Spacer(int input)
    {
      string result = "";
      for (int i = 0; i < input - 4; i++)
      {
        result += " ";
      }
      return result;
    }
  }

  /// <summary>
  /// Skeleton for B+Tree nodes
  /// </summary>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to
  /// be externally viewed.</param>
    public abstract class BPlusTreeNode<T>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, 
                T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock) 
                : Node<BPlusTreeNode<T>, T>(degree, bufferBlock)
    {
      
        
    }

    public class NullContentReferenceException : Exception
  {
    public NullContentReferenceException() : base() { }
    public NullContentReferenceException(string message) : base(message) { }
    public NullContentReferenceException(string message, Exception inner) : base(message, inner) { }
  }

  /// <summary>
  /// Thrown when a reference to a content object is null.
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  public class NullChildReferenceException : Exception
  {
    public NullChildReferenceException() : base() { }
    public NullChildReferenceException(string message) : base(message) { }
    public NullChildReferenceException(string message, Exception inner) : base(message, inner) { }
  }
}