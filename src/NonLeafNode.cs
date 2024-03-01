/*
Author: Emily Elzinga and Tristan Anderson
Date: 2/07/2024
Desc: Describes functionality for non-leaf nodes on the BTree. Recursive function iteration due to children nodes.
*/
using System.Threading.Tasks.Dataflow;
using System.Text.RegularExpressions;


namespace BTreeVisualization
{
  /// <summary>
  /// Creates a non-leaf node for a B-Tree data structure with
  /// empty arrays of keys, contents, and children.
  /// </summary>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent non-leaf node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to be externally viewed.</param>
  public class NonLeafNode<T>(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock) : BTreeNode<T>(degree, bufferBlock)
  {
    /// <summary>
    /// Array to track child nodes of this node. These can be either Leaf or Non-Leaf.
    /// </summary>
    private BTreeNode<T>[] _Children = new BTreeNode<T>[2 * degree];
    /// <summary>
    /// Getter for _Children[]
    /// </summary>
    public BTreeNode<T>[] Children
    {
      get { return _Children; }
    }

    /// <summary>
    /// Creates a non-leaf node for a B-Tree data structure
    /// with starting values of the passed arrays of keys,
    /// contents, and children. Sets the NumKeys to the length of keys[].
    /// </summary>
    /// <param name="degree">Same as parent non-leaf node/tree</param>
    /// <param name="keys">Values to initialize in _Keys[]</param>
    /// <param name="data">Values to initialize in _Contents[]</param>
    /// <param name="children">Child nodes to initialize in _Children[]</param>
    /// <param name="bufferBlock">Output Buffer for Status updates to be externally viewed.</param>
    public NonLeafNode(int degree, int[] keys, T[] data, BTreeNode<T>[] children, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock) : this(degree, bufferBlock)
    {
      _NumKeys = keys.Length;
      for (int i = 0; i < keys.Length; i++)
      {
        _Keys[i] = keys[i];
        _Contents[i] = data[i];
        _Children[i] = children[i];
      }
      _Children[keys.Length] = children[keys.Length];
    }

    /// <summary>
    /// Iterates over the _Keys[] to find an entry == key.
    /// </summary>
    /// <remarks>Copied and modified from
    /// LeafNode.Search()</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index else returns -1.</returns>
    private int Search(int key)
    {
      //searches for correct key, finds it returns the node, else returns -1
      for (int i = 0; i < _NumKeys; i++)
      {
        if (_Keys[i] >= key)
        {
          return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Iterates over the _Keys array to find key. If found returns the index and this else returns -1 and this.
    /// </summary>
    /// <remarks>Copied and modified from
    /// LeafNode.SearchKey()</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index and this node else returns -1 and this node.</returns>
    public override (int, BTreeNode<T>) SearchKey(int key)
    {
      _BufferBlock.Post((Status.SSearching, ID, -1, [], [], 0, -1, [], []));
      int result = Search(key);
      if (result == -1)
      {
        return _Children[_NumKeys].SearchKey(key);
      }
      else if (_Keys[result] == key)
      {
        _BufferBlock.Post((Status.Found, ID, result, [key], [Contents[result]], 0, -1, [], []));
        return (result, this);
      }
      else
      {
        return _Children[result].SearchKey(key);
      }
    }

    /// <summary>
    /// Calls InsertKey on _Children[i] where i == _Keys[i] < key < _Keys[i+1].
    /// Then checks if a split occured. If so it inserts the new 
    /// ((dividing Key, Content), new Node) to itself.
    /// Afterwards calls split if full.
    /// </summary>
    /// <remarks>Copied and modified from
    /// LeafNode.InsertKey()</remarks>
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
      ((int, T?), BTreeNode<T>?) result;
      int i = 0;
      while (i < _NumKeys && key > _Keys[i]){
        i++;
      }
      if (i == _NumKeys || key != _Keys[i] || key == 0)
      {
        result = _Children[i].InsertKey(key, data);
        if (result.Item2 != null && result.Item1.Item2 != null)
        {
          for (int j = _NumKeys - 1; j >= i; j--)
          {
            _Keys[j + 1] = _Keys[j];
            _Contents[j + 1] = _Contents[j];
            _Children[j + 2] = _Children[j + 1];
          }
          _Keys[i] = result.Item1.Item1;
          _Contents[i] = result.Item1.Item2;
          _Children[i + 1] = result.Item2;
          _NumKeys++;
          _BufferBlock.Post((Status.Inserted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
          if (IsFull())
          {
            return Split();
          }
        }
      }
      else
      {
        _BufferBlock.Post((Status.Inserted, 0, -1, [], [], 0, -1, [], []));
      }
      return ((-1, default(T)), null);
    }

    /// <summary>
    /// Evenly splits the _Contents[] and _Keys[] of this node giving
    /// up the greater half to a new node.
    /// </summary>
    /// <remarks>Copied and modified from
    /// LeafNode.Split()</remarks>
    /// <returns>The new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).</returns>
    public override ((int, T), BTreeNode<T>) Split()
    {
      int[] newKeys = new int[_Degree - 1];
      T[] newContent = new T[_Degree - 1];
      BTreeNode<T>[] newChildren = new BTreeNode<T>[_Degree];
      int i = 0;
      for (; i < _Degree - 1; i++)
      {
        newKeys[i] = _Keys[i + _Degree];
        newContent[i] = _Contents[i + _Degree];
        newChildren[i] = _Children[i + _Degree];
        _BufferBlock.Post((Status.Shift, newChildren[i].ID, -1, [], [], ID, -1, [], []));
      }
      newChildren[i] = _Children[i + _Degree];
      _NumKeys = _Degree - 1;
      NonLeafNode<T> newNode = new(_Degree, newKeys, newContent, newChildren, _BufferBlock);
      _BufferBlock.Post((Status.Split, ID, NumKeys, Keys, Contents, newNode.ID, newNode.NumKeys, newNode.Keys, newNode.Contents));
      return ((_Keys[_NumKeys], _Contents[_NumKeys]), newNode);
    }

    /// <summary>
    /// Searches itself for key. If found it deletes it by overwriting it
    /// with the returned result from calling ForfeitKey() on the
    /// left child from the key being deleted. If not found it calls
    /// Search on _Children[i] where i == _Keys[i] < key < _Keys[i+1].
    /// Afterwards checks the child for underflow.
    /// </summary>
    /// <remarks>Author: Tristan Anderson, Date: 2024-02-18</remarks>
    /// <param name="key">Integer to search for and delete if found.</param>
		public override void DeleteKey(int key)
    {
      _BufferBlock.Post((Status.DSearching, ID, -1, [], [], 0, -1, [], []));
      int result = Search(key);
      if (result == -1)
      {
        // Search only goes through keys and thus if it did not 
        // find it at the last index it returns -1.
        _Children[_NumKeys].DeleteKey(key);
        MergeAt(_NumKeys);
      }
      else if (_Keys[result] == key)
      {
        (_Keys[result], _Contents[result]) = _Children[result].ForfeitKey();
        _BufferBlock.Post((Status.Deleted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
        MergeAt(result);
      }
      else
      {
        _Children[result].DeleteKey(key);
        MergeAt(result);
      }
    }

    /// <summary>
    /// Calls ForfeitKey() on last child for a replacement key
    /// for the parent node. Afterwards checks the child for underflow.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-23</remarks>
    /// <returns>The key and corresponding content from the right
    /// most leaf node below this node.</returns>
    public override (int, T) ForfeitKey()
    {
      _BufferBlock.Post((Status.FSearching, ID, -1, [], [], 0, -1, [], []));
      (int, T) result;
      result = _Children[_NumKeys].ForfeitKey();
      MergeAt(_NumKeys);
      return result;
    }

    /// <summary>
    /// Appends the given divider to itself and appends 
    /// all the entries from the sibiling to itself.
    /// </summary>
    /// <remarks>
    /// Author: Tristan Anderson,
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
        _Children[_NumKeys + i] = ((NonLeafNode<T>)sibiling).Children[i];
      }
      _Children[_NumKeys + sibiling.NumKeys] = ((NonLeafNode<T>)sibiling).Children[sibiling.NumKeys];
      _NumKeys += sibiling.NumKeys;
      _BufferBlock.Post((Status.Merge, ID, NumKeys, Keys, Contents, sibiling.ID, -1, [], []));
    }

    /// <summary>
    /// Checks the child at index for underflow. If so it then checks for _Degree 
    /// number of children in the right child of the key. _Degree or greater means 
    /// either overflow or split. 
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="index">Index of affected child node.</param>
    private void MergeAt(int index)
    {
      if (_Children[index].IsUnderflow())
      {
        if (index == _NumKeys) { index--; }
        if (_Children[index + 1].NumKeys >= _Degree)
        {
          _Children[index].GainsFromRight(_Keys[index], _Contents[index], _Children[index + 1]);
          _Keys[index] = _Children[index + 1].Keys[0];
          _Contents[index] = _Children[index + 1].Contents[0];
          _Children[index + 1].LosesToLeft();
          _BufferBlock.Post((Status.UnderFlow, Children[index].ID, Children[index].NumKeys,
            Children[index].Keys, Children[index].Contents, Children[index + 1].ID,
            Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents));
          if (_Children[index] as NonLeafNode<T> != null)
            _BufferBlock.Post((Status.Shift, ((NonLeafNode<T>)Children[index])
              .Children[Children[index].NumKeys].ID, -1, [], [], _Children[index].ID, -1, [], []));
        }
        else if (_Children[index].NumKeys >= _Degree)
        {
          _Children[index + 1].GainsFromLeft(_Keys[index], _Contents[index], _Children[index]);
          _Keys[index] = _Children[index].Keys[_Children[index].NumKeys - 1];
          _Contents[index] = _Children[index].Contents[_Children[index].NumKeys - 1];
          _Children[index].LosesToRight();
          _BufferBlock.Post((Status.UnderFlow, Children[index + 1].ID,
            Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents,
            Children[index].ID, Children[index].NumKeys,
            Children[index].Keys, Children[index].Contents));
          if (_Children[index] as NonLeafNode<T> != null)
            _BufferBlock.Post((Status.Shift, ((NonLeafNode<T>)Children[index + 1])
              .Children[0].ID, -1, [], [], _Children[index + 1].ID, -1, [], []));
        }
        else
        {
          _Children[index].Merge(_Keys[index], _Contents[index], _Children[index + 1]);
          for (; index < _NumKeys - 1;)
          {
            _Keys[index] = _Keys[index + 1];
            _Contents[index] = _Contents[index + 1];
            index++;
            _Children[index] = _Children[index + 1];
          }
          _NumKeys--;
          _BufferBlock.Post((Status.MergeParent, ID, NumKeys, Keys, Contents, 0, -1, [], []));
        }
      }
    }

    /// <summary>
    /// Tacks on the given key and data and grabs the first child of the sibiling.
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
      _Children[++_NumKeys] = ((NonLeafNode<T>)sibiling).Children[0];
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
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
        _Children[i] = _Children[i + 1];
      }
      _NumKeys--;
      _Children[_NumKeys] = _Children[_NumKeys + 1];
    }

    /// <summary>
    /// Inserts at the beginning of this node arrays the 
    /// given key and data and grabs the last child of the sibiling.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-22</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">Sibiling to left. (Sibiling's Keys should be
    /// smaller than all the keys in the called node.)</param>
    public override void GainsFromLeft(int dividerKey, T dividerData, BTreeNode<T> sibiling)
    {
      _Children[_NumKeys + 1] = _Children[_NumKeys];
      for (int i = _NumKeys; i > 0; i--)
      {
        _Keys[i] = _Keys[i - 1];
        _Contents[i] = _Contents[i - 1];
        _Children[i] = _Children[i - 1];
      }
      _NumKeys++;
      _Keys[0] = dividerKey;
      _Contents[0] = dividerData;
      _Children[0] = ((NonLeafNode<T>)sibiling).Children[sibiling.NumKeys];
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
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-13</remarks>
    /// <param name="x">Hierachical Node ID</param>
    /// <returns>String with the entirety of this node's keys and contents arrays formmatted in JSON syntax.</returns>
		public override string Traverse(string x)
    {
      string output = Spacer(x) + "{\n";
      output += Spacer(x) + "  \"node\":\"" + x + "\",\n" 
        + Spacer(x) + "\"  ID\":" + _ID + ",\n" + Spacer(x) + "  \"keys\":[";
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
      output += "],\n" + Spacer(x) + "  \"children\":[\n";
      for (int i = 0; i <= _NumKeys; i++)
      {
        output += _Children[i].Traverse(x + "." + i) + (i + 1 <= _NumKeys ? "," : "") + "\n";
      }
      return output + Spacer(x) + "  ]\n" + Spacer(x) + "}";
    }
  }
}
