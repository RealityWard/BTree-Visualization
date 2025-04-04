/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Base class for all the node objects used in the BTree and B+Tree.
*/
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;

namespace BTreeVisualizationNode
{
  /// <summary>
  /// Base class for all the node objects used in the BTree and B+Tree.
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  /// <typeparam name="N">Type of node.</typeparam>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent non-leaf node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to
  /// be externally viewed.</param>
  public abstract class Node<T, N>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)
  {
    /// <summary>
    /// Output Buffer for Status updates to be externally viewed.
    /// </summary>
    protected BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> _BufferBlock = bufferBlock;
    public BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> GetBufferBlock
    {
      get { return _BufferBlock; }
    }
    /// <summary>
    /// Determines the number of keys and children per node.
    /// </summary>
    protected readonly int _Degree = degree;
    public int Degree
    {
      get { return _Degree; }
    }
    /// <summary>
    /// Identifier to be unique per node for reference on gui side.
    /// </summary>
    protected long _ID = DateTime.Now.Ticks;
    /// <summary>
    /// Current count of key entries to this node. Marks the last index - 1 to perceive in _Keys[] and _Contents[].
    /// </summary>
    protected int _NumKeys = 0;
    /// <summary>
    /// Prints out this node and its children.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Hierachical Node ID</param>
    /// <returns>String in JSON syntax.</returns>
    public abstract string Traverse(string x);

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
  /// Skeleton for B-Tree nodes
  /// </summary>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to
  /// be externally viewed.</param>
  public abstract class BTreeNode<T>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock) : Node<T, BTreeNode<T>>(degree, bufferBlock)
  {
    /// <summary>
    /// Holds key entries for this node.
    /// </summary>
    protected int[] _Keys = new int[2 * degree - 1];
    /// <summary>
    /// Getter of _Keys
    /// </summary>
    public int[] Keys
    {
      get { return _Keys; }
    }
    /// <summary>
    /// Generic typed array parallel to the keys array.
    /// It holds the values associated with the corresponding key.
    /// </summary>
    protected T?[] _Contents = new T[2 * degree - 1];
    /// <summary>
    /// Getter for _Contents[]
    /// </summary>
    public T?[] Contents
    {
      get { return _Contents; }
    }
    /// <summary>
    /// Checks if this node is at max capacity.
    /// </summary>
    /// <returns>True if it is full.</returns>
    public bool IsFull()
    {
      return _NumKeys == 2 * _Degree - 1;
    }
    /// <summary>
    /// Checks if this node is below minimum capacity.
    /// </summary>
    /// <returns>True if it is below.</returns>
    public bool IsUnderflow()
    {
      return _NumKeys < _Degree - 1;
    }
    /// <summary>
    /// Delete an entry matching key from this node or child node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson, Date: 2024-02-18</remarks>
    /// <param name="key">Integer to search for and delete if found.</param>
    public abstract void DeleteKey(int key);
    /// <summary>
    /// Delete an entries within the range of key to endKey
    /// from this node. Recurs into the child in the range
    /// or forks to the children on the edges of the range.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Start of range, inclusive</param>
    /// <param name="endKey">End of range, exclusive</param>
    public abstract void DeleteKeysMain(int key, int endKey, long parentID);
    /// <summary>
    /// Runs at the point at which the range effects more than one node.
    /// This represents the fork of DeleteKeysMain.
    /// Deletes entries matching the range from this node
    /// and the sibiling specified.
    /// Recurs into the children on the edge of the range.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Start of range, inclusive</param>
    /// <param name="endKey">End of range, exclusive</param>
    public abstract void DeleteKeysSplit(int key, int endKey, BTreeNode<T> rightSibiling, long parentID);
    /// <summary>
    /// Runs on nodes on the left fork of DeleteKeysMain.
    /// Deletes everything from index and up.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="index">Index to start
    /// deleting from.</param>
    public abstract void DeleteKeysLeft(int index, long parentID);
    /// <summary>
    /// Runs on nodes on the right fork of DeleteKeysMain.
    /// Deletes everything up to index but not index.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="index">Index to start
    /// deleting from.</param>
    public abstract void DeleteKeysRight(int index, long parentID);
    /// <summary>
    /// Runs on nodes on the right fork of DeleteKeysMain.
    /// Runs MergeAt from the bottom affected nodes and up on this fork.
    /// </summary>
    public abstract void RestoreRight();
    /// <summary>
    /// Runs on nodes on the left fork of DeleteKeysMain.
    /// Runs MergeAt from the bottom affected nodes and up on this fork.
    /// </summary>
    public abstract void RestoreLeft();
    /// <summary>
    /// Removes the beginning entry of this node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    public abstract void LosesToLeft(int diff);
    /// <summary>
    /// Removes the last entry of this node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    public abstract void LosesToRight(int diff);
    /// <summary>
    /// Split this node into two.
    /// </summary>
    /// <returns>The new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).</returns>
    public abstract ((int, T), BTreeNode<T>) Split(long parentID);
    /// <summary>
    /// Append the entry between this node and its sibiling.
    /// Then append all the entries from the sibiling to this node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">Sibiling to right. (Sibiling's Keys should be
    /// greater than all the keys in the called node.)</param>
    public abstract void Merge(int dividerKey, T dividerData, BTreeNode<T> sibiling);
    /// <summary>
    /// This node appends its sibiling's left most entry to its own entries.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">Sibiling to right. (Sibiling's Keys
    /// should be greater than all the keys in the called node.)</param>
    public abstract void GainsFromRight(int diff, int dividerKey, T? dividerData, BTreeNode<T> sibiling);
    /// <summary>
    /// This node prepends its sibiling's left most entry to its own entries.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-22</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">Sibiling to left. (Sibiling's Keys should be
    /// smaller than all the keys in the called node.)</param>
    public abstract void GainsFromLeft(int diff, int dividerKey, T? dividerData, BTreeNode<T> sibiling);
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
    public abstract ((int, T?), BTreeNode<T>?) InsertKey(int key, T data, long parentID);
    /// <summary>
    /// Retreive the right most entry of the right most leaf node of this node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-23</remarks>
    /// <returns>The key and corresponding content from the right
    /// most leaf node below this node.</returns>
    public abstract (int, T?) ForfeitKey();
    /// <summary>
    /// Find a key in this node or in its children.
    /// </summary>
    /// <param name="key">Integer to find in _Keys[].</param>
    /// <returns>If found returns the index and this node else returns -1 and this node.</returns>
    public abstract (int key, T content)? SearchKey(int key);
    /// <summary>
    /// Searches for all keys equal to or greater than key and less than endKey.
    /// </summary>
    /// <param name="key">Lower bound inclusive.</param>
    /// <param name="endKey">Upper bound exclusive.</param>
    /// <returns>A list of key-content pairs from the matching range.</returns>
    public abstract List<(int key, T content)> SearchKeys(int key, int endKey);
    /// <summary>
    /// Sends NodeDeleted status to the frontend for this
    /// node and included children.
    /// </summary>
    /// <param name="id">ID of the parent node.</param>
    public abstract void DeleteNode(long id);

    /// <summary>
    /// Iterates over the _Keys array to find key. If found returns the index else returns -1.
    /// Log(n) search static method for use on all nodes with keys.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index else returns -1.</returns>
    static protected int Search(BTreeNode<T> node, int key)
    {
      if (node.NumKeys == 0)
        return -1;
      int firstIndex = 0;
      int lastIndex = node.NumKeys - 1;
      int midIndex = lastIndex / 2;
      while (!((midIndex == 0 || node.Keys[midIndex - 1] < key) && node.Keys[midIndex] >= key) && firstIndex <= lastIndex)
      {
        midIndex = (firstIndex + lastIndex) / 2;
        if (node.Keys[midIndex] < key)
        {
          firstIndex = midIndex + 1;
        }
        else
        {
          lastIndex = midIndex - 1;
        }
      }
      return node.Keys[midIndex] >= key ? midIndex : -1;
    }

    /// <summary>
    /// Creates a deep copy of the keys and contents arrays.
    /// </summary>
    /// <returns>The deep copies.</returns>
    public (int, int[], T?[]) CreateBufferVar()
    {
      int numKeys = NumKeys;
      int[] keys = new int[_Keys.Length];
      T?[] contents = new T[_Keys.Length];
      for (int i = 0; i < _Keys.Length; i++)
      {
        keys[i] = Keys[i];
        contents[i] = Contents[i];
      }
      return (numKeys, keys, contents);
    }
  }

  /// <summary>
  /// Thrown when a reference to a content object is null.
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
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
