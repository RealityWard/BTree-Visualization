/**
Desc: Implements the leaf nodes of a B-Tree. Non-recursive function
iteration due to no children.
*/
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using System.Text.RegularExpressions;
using BTreeVisualization;

namespace BPlusTreeVisualization
{
  /// <summary>
  /// Creates a leaf node for a B-Tree data structure with
  /// empty arrays of keys and contents.
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent non-leaf node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to
  /// be externally viewed.</param>
  public class LeafNode<T>(int degree, BufferBlock<(Status status,
      long id, int numKeys, int[] keys, T[] contents, long altID,
      int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock)
      : BTreeNode<T>(degree, bufferBlock)
  {
    /// <summary>
    /// Creates a leaf node for a B-Tree data structure
    /// with starting values of the passed arrays of
    /// keys and contents. Sets the NumKeys to the length of keys[].
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="degree">Same as parent non-leaf node</param>
    /// <param name="keys">Values to initialize in _Keys[]</param>
    /// <param name="contents">Values to initialize in _Contents[]</param>
    /// <param name="bufferBlock">Output Buffer for Status updates to be
    /// externally viewed.</param>
    public LeafNode(int degree, int[] keys, T[] contents,
        BufferBlock<(Status status, long id, int numKeys, int[] keys,
        T[] contents, long altID, int altNumKeys, int[] altKeys,
        T[] altContents)> bufferBlock) : this(degree, bufferBlock)
    {
      _NumKeys = keys.Length;
      for (int i = 0; i < keys.Length; i++)
      {
        _Keys[i] = keys[i];
        _Contents[i] = contents[i];
      }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Iterates over the _Keys array to find key. If found returns the index else returns -1.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index else returns -1.</returns>
    private int Search(int key)
    {
      for (int i = 0; i < _NumKeys; i++)
      {
        if (_Keys[i] == key)
        {
          _BufferBlock.Post((Status.Found, ID, i, [key], [Contents[i]], 0, -1, [], []));
          return i;
        }
      }
      _BufferBlock.Post((Status.Found, ID, -1, [], [], 0, -1, [], []));
      return -1;
    }

    /// <summary>
    /// Iterates over the _Keys array to find key. If found returns the
    /// index and this else returns -1 and this.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index and this node else returns -1 and
    /// this node.</returns>
    public override (int, BTreeNode<T>) SearchKey(int key)
    {
      _BufferBlock.Post((Status.SSearching, ID, -1, [], [], 0, -1, [], []));
      return (Search(key), this);
    }

    /// <summary>
    /// Evenly splits the _Contents[] and _Keys[] of this node giving
    /// up the greater half to a new node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>The new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).</returns>
    public override ((int, T), BTreeNode<T>) Split()
    {
      int[] newKeys = new int[_Degree - 1];
      T[] newContent = new T[_Degree - 1];
      for (int i = 0; i < _Degree - 1; i++)
      {
        newKeys[i] = _Keys[i + _Degree];
        newContent[i] = _Contents[i + _Degree];
      }
      _NumKeys = _Degree - 1;
      LeafNode<T> newNode = new(_Degree, newKeys, newContent, _BufferBlock);
      _BufferBlock.Post((Status.Split, ID, NumKeys, Keys, Contents, newNode.ID,
                          newNode.NumKeys, newNode.Keys, newNode.Contents));
      return ((_Keys[_NumKeys], _Contents[_NumKeys]), newNode);
    }

    /// <summary>
    /// Finds and places the new info in the 
    /// current node. 
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to be placed into _Keys[] of this node.</param>
    /// <param name="data">Coresponding data to be stored in _Contents[]
    /// of this node at the same index as key in _Keys[].</param>
    /// <returns>If this node reaches capacity it calls split and returns
    /// the new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).
    /// Otherwise it returns ((-1, null), null).</returns>
    public override ((int, T?), BTreeNode<T>?) InsertKey(int key, T data)
    {
      _BufferBlock.Post((Status.ISearching, ID, -1, [], [], 0, -1, [], []));
      int i = 0;
      while (i < _NumKeys && key > _Keys[i])
        i++;
      if (i == _NumKeys || key != _Keys[i] || key == 0)
      {
        for (int j = _NumKeys - 1; j >= i; j--)
        {
          _Keys[j + 1] = _Keys[j];
          _Contents[j + 1] = _Contents[j];
        }
        _Keys[i] = key;
        _Contents[i] = data;
        _NumKeys++;
        _BufferBlock.Post((Status.Inserted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
        if (IsFull())
        {
          return Split();
        }
      }
      else
      {
        _BufferBlock.Post((Status.Inserted, 0, -1, [], [], 0, -1, [], []));
      }
      return ((-1, default(T)), null);
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// If the entry exists it deletes it.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to search for and delete if found.</param>
    public override void DeleteKey(int key)
    {
      _BufferBlock.Post((Status.DSearching, ID, -1, [], [], 0, -1, [], []));
      int i = Search(key);
      if (i != -1)
      {
        for (; i < _NumKeys; i++)
        {
          _Keys[i] = _Keys[i + 1];
          _Contents[i] = _Contents[i + 1];
        }
        _NumKeys--;
        _BufferBlock.Post((Status.Deleted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
      }
      _BufferBlock.Post((Status.Deleted, ID, -1, [], [], 0, -1, [], []));
    }

    /// <summary>
    /// Returns its rigth most / last 
    /// key to the parent. Then it decrements the number of keys.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <returns>Tuple of Key and corresponding content.</returns>
    public override (int, T) ForfeitKey()
    {
      _NumKeys--;
      _BufferBlock.Post((Status.Forfeit, ID, NumKeys, Keys, Contents, 0, -1, [], []));
      return (_Keys[_NumKeys], _Contents[_NumKeys]);
    }

    /// <summary>
    /// Appends the given divider to its own arrays and grabs 
    /// all the entries from the sibiling adding those to its arrays as well.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">Sibiling to right. (Sibiling's Keys should be
    /// greater than all the keys in the called node.)</param>
    public override void Merge(int dividerKey, T dividerData, BTreeNode<T> sibiling)
    {
      _Keys[_NumKeys] = dividerKey;
      _Contents[_NumKeys] = dividerData;
      _NumKeys++;
      for (int i = 0; i < sibiling.NumKeys; i++)
      {
        _Keys[_NumKeys + i] = sibiling.Keys[i];
        _Contents[_NumKeys + i] = sibiling.Contents[i];
      }
      _NumKeys += sibiling.NumKeys;
      _BufferBlock.Post((Status.Merge, ID, NumKeys, Keys, Contents, sibiling.ID, -1, [], []));
    }

    /// <summary>
    /// Appends the given key and data of the sibiling.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">Sibiling to right. (Sibiling's Keys
    /// should be greater than all the keys in the called node.)</param>
    public override void GainsFromRight(int dividerKey, T dividerData, BTreeNode<T> sibiling)
    {
      _Keys[_NumKeys] = dividerKey;
      _Contents[_NumKeys] = dividerData;
      _NumKeys++;
    }

    /// <summary>
    /// Shifts the values in the arrays by one to the left overwriting 
    /// the first entries and decrements the _NumKeys var.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    public override void LosesToLeft()
    {
      for (int i = 0; i < _NumKeys - 1; i++)
      {
        _Keys[i] = _Keys[i + 1];
        _Contents[i] = _Contents[i + 1];
      }
      _NumKeys--;
    }

    /// <summary>
    /// Inserts at the beginning of this node arrays the 
    /// given key and data.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-22</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">Sibiling to left. (Sibiling's Keys should be
    /// smaller than all the keys in the called node.)</param>
    public override void GainsFromLeft(int dividerKey, T dividerData, BTreeNode<T> sibiling)
    {
      for (int i = _NumKeys - 1; i >= 0; i--)
      {
        _Keys[i + 1] = _Keys[i];
        _Contents[i + 1] = _Contents[i];
      }
      _NumKeys++;
      _Keys[0] = dividerKey;
      _Contents[0] = dividerData;
    }

    /// <summary>
    /// Decrements the _NumKeys var.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-22</remarks>
    public override void LosesToRight()
    {
      _NumKeys--;
    }

    /// <summary>
    /// Prints out the contents of the node in JSON format.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="x">Hierachical Node ID</param>
    /// <returns>String with the entirety of this node's
    /// keys and contents arrays formmatted in JSON syntax.</returns>
    public override string Traverse(string x)
    {
      string output = Spacer(x) + "{\n";
      output += Spacer(x) + "  \"leafnode\":\"" + x + "\",\n" 
        + Spacer(x) + "\"  ID\":" + _ID + ",\n"
        + Spacer(x) + "  \"keys\":[";
      for (int i = 0; i < _NumKeys; i++)
      {
        output += _Keys[i] + (i + 1 < _NumKeys ? "," : "");
      }
      output += "],\n" + Spacer(x) + "  \"contents\":[";
      for (int i = 0; i < _NumKeys; i++)
      {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        output += _Contents[i].ToString() + (i + 1 < _NumKeys ? "," : "");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      }
      return output + "]\n" + Spacer(x) + "}";
    }
  }
}