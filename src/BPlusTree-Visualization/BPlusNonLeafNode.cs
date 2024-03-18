/*
Author: Andreas Kramer
Date: 03/04/2024
Desc: Describes functionality for non-leaf nodes on the B+Tree. Recursive function iteration due to children nodes.
*/
using System.Threading.Tasks.Dataflow;
using System.Text.RegularExpressions;
using ThreadCommunication;


namespace BPlusTreeVisualization
{
  /// <summary>
  /// Creates a non-leaf node for a B-Tree data structure with
  /// empty arrays of keys, contents, and children.
  /// </summary>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent non-leaf node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to be externally viewed.</param>
  public class BPlusNonLeafNode<T>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock) : BPlusTreeNode<T>(degree, bufferBlock)
  {
    /// <summary>
    /// Array to track child nodes of this node. These can be either Leaf or Non-Leaf.
    /// </summary>
    private BPlusTreeNode<T>?[] _Children = new BPlusTreeNode<T>[degree + 1];
    /// <summary>
    /// Getter for _Children[]
    /// </summary>
    public BPlusTreeNode<T>?[] Children
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
    /// <param name="children">Child nodes to initialize in _Children[]</param>
    /// <param name="bufferBlock">Output Buffer for Status updates to be externally viewed.</param>
    public BPlusNonLeafNode(int degree, int[] keys, BPlusTreeNode<T>[] children, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock) : this(degree, bufferBlock)
    {
      _NumKeys = keys.Length;
      for (int i = 0; i < keys.Length; i++)
      {
        _Keys[i] = keys[i];
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
      int index = 0;
      while (index < _NumKeys && _Keys[index] <= key)
        {
          index++;
        }
      return index;
    }
    
    /// <summary>
    /// Iterates over the _Keys array to find key. If found returns the index and this else returns -1 and this.
    /// </summary>
    /// <remarks>Copied and modified from
    /// LeafNode.SearchKey()</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index and this node else returns -1 and this node.</returns>
 
    public override (int, BPlusTreeNode<T>) SearchKey(int key)
    {
      _BufferBlock.SendAsync((NodeStatus.SSearching, ID, -1, [], [], 0, -1, [], []));
      int index = Search(key);
  
      if(_Children[index] != null) {

        if(_Children[index] is BPlusLeafNode<T> leaf){
          (int, BPlusTreeNode<T>) result = leaf.SearchKey(key);
          return result;
        }else if(_Children[index] is BPlusNonLeafNode<T> NonLeaf){
          return NonLeaf.SearchKey(key);
        }
      }       
      return (-1, this);
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
    /// <param name="data">Corresponding data to be stored in _Contents[]
    /// of this node at the same index as key in _Keys[].</param>
    /// <returns>If this node reaches capacity it calls split and returns
    /// the new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).
    /// Otherwise it returns ((-1, null), null).</returns>
    public override ((int,T?), BPlusTreeNode<T>?) InsertKey(int key, T data)
    {
      _BufferBlock.SendAsync((NodeStatus.ISearching,ID,-1,[],[],0,-1,[],[]));
      ((int,T?),BPlusTreeNode<T>?) result;
      int i = 0;
      while(i < _NumKeys && key > _Keys[i]){
        i++;
      }
      if(i == _NumKeys || key != _Keys[i] || key == 0){
        result = (Children[i]?? throw new NullChildReferenceException(
          $"Child at index:{i} within node:{ID}")).InsertKey(key,data);
        if(result.Item2 != null){
          for (int j = _NumKeys - 1; j >= i; j--)
          {
            _Keys[j + 1] = _Keys[j];
            _Children[j + 2] = _Children[j + 1];
          }
          _Keys[i] = result.Item1.Item1;
          _Children[i + 1] = result.Item2;
          _NumKeys++;
          _BufferBlock.SendAsync((NodeStatus.Inserted, ID, NumKeys, Keys, [], 0, -1, [], []));
          if (IsFull())
          {
            return Split();
          }
        }
      }else{
          _BufferBlock.SendAsync((NodeStatus.Inserted,0,-1,[],[],0,-1,[],[]));
        }
      
      return ((-1, default(T)),null);
    }

    /// <summary>
    /// Splits _Keys[] of this node giving up the greater half to a new node.
    /// </summary>
    /// <returns>The new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).</returns>
    public ((int,T?), BPlusTreeNode<T>) Split()
    { 
      int[] newKeys = new int[_Degree];
      BPlusTreeNode<T>[] newChildren = new BPlusTreeNode<T>[_Degree + 1];
      int dividerIndex = _NumKeys / 2;
      (int,T?) dividerEntry = (_Keys[dividerIndex],default(T));
      int i = 1;
      for (; i < _NumKeys - dividerIndex; i++){
        newKeys[i - 1] = _Keys[i + dividerIndex];
        newChildren[i - 1] = _Children[i + dividerIndex]
          ?? throw new NullChildReferenceException(
            $"Child at index:{i + dividerIndex} within node:{ID}");
        _Children[i + dividerIndex] = default;
        _Keys[i + dividerIndex] = default;
        _BufferBlock.SendAsync((NodeStatus.Shift, newChildren[i-1].ID, -1, [], [], ID, -1, [], []));
      }
      newChildren[i -1] = _Children[i + dividerIndex]
        ?? throw new NullChildReferenceException(
          $"Child at index:{i + _Degree} within node:{ID}");
      _Children[i + dividerIndex] = default;
      _Keys[dividerIndex] = default;

      BPlusNonLeafNode<T> newNode = new(_Degree, newKeys, newChildren, _BufferBlock)
            {
            _NumKeys = _NumKeys - dividerIndex - 1
            };
      _NumKeys = dividerIndex;
      _BufferBlock.SendAsync((NodeStatus.Split, ID, NumKeys, Keys, [],
      newNode.ID, newNode.NumKeys, newNode.Keys, []));
      return (dividerEntry, newNode);
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
 /*
		public override void DeleteKey(int key)
    {
      _BufferBlock.SendAsync((Status.DSearching, ID, -1, [], [], 0, -1, [], []));
      int result = Search(key);
      if (result == -1)
      {
        // Search only goes through keys and thus if it did not 
        // find it at the last index it returns -1.
        (_Children[_NumKeys] ?? throw new NullChildReferenceException(
          $"Child at index:{_NumKeys} within node:{ID}")).DeleteKey(key);
        MergeAt(_NumKeys);
      }
      else if (_Keys[result] == key)
      {
        (_Keys[result], _Contents[result]) = (_Children[result]
          ?? throw new NullChildReferenceException(
            $"Child at index:{result} within node:{ID}")).ForfeitKey();
        _BufferBlock.SendAsync((Status.Deleted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
        MergeAt(result);
      }
      else
      {
        (_Children[result] ?? throw new NullChildReferenceException(
          $"Child at index:{result} within node:{ID}")).DeleteKey(key);
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
      _BufferBlock.SendAsync((Status.FSearching, ID, -1, [], [], 0, -1, [], []));
      (int, T) result =
        (_Children[_NumKeys] ?? throw new NullChildReferenceException(
          $"Child at index:{_NumKeys} within node:{ID}")).ForfeitKey();
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
    public override void Merge(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling)
    {
      _Keys[_NumKeys] = dividerKey;
      _Contents[_NumKeys] = dividerData;
      _NumKeys++;
      for (int i = 0; i < sibiling.NumKeys; i++)
      {
        _Keys[_NumKeys + i] = sibiling.Keys[i];
        _Contents[_NumKeys + i] = sibiling.Contents[i];
        _Children[_NumKeys + i] = ((BPlusNonLeafNode<T>)sibiling).Children[i];
      }
      _Children[_NumKeys + sibiling.NumKeys] = ((BPlusNonLeafNode<T>)sibiling).Children[sibiling.NumKeys];
      _NumKeys += sibiling.NumKeys;
      _BufferBlock.SendAsync((Status.Merge, ID, NumKeys, Keys, Contents, sibiling.ID, -1, [], []));
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
      if ((_Children[index] ?? throw new NullChildReferenceException(
          $"Child at index:{index} within node:{ID}")).IsUnderflow())
      {
        if (index == _NumKeys) { index--; }
        if (_Children[index] == null)
        {
          throw new NullChildReferenceException(
                    $"Child at index:{index} within node:{ID}");
        }
        else if (_Children[index + 1] == null)
        {
          throw new NullChildReferenceException(
                    $"Child at index:{index + 1} within node:{ID}");
        }
        else if (_Contents[index] == null)
        {
          throw new NullContentReferenceException(
                    $"Content at index:{index} within node:{ID}");
        }
        else
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          if (_Children[index + 1].NumKeys >= _Degree)
          {
#pragma warning disable CS8604 // Possible null reference argument.
            _Children[index].GainsFromRight(_Keys[index], _Contents[index], _Children[index + 1]);
#pragma warning restore CS8604 // Possible null reference argument.
            _Keys[index] = _Children[index + 1].Keys[0];
            _Contents[index] = _Children[index + 1].Contents[0];
            _Children[index + 1].LosesToLeft();
            _BufferBlock.SendAsync((Status.UnderFlow, Children[index].ID, Children[index].NumKeys,
              Children[index].Keys, Children[index].Contents, Children[index + 1].ID,
              Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents));
            if (_Children[index] as BPlusNonLeafNode<T> != null)
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
              _BufferBlock.SendAsync((Status.Shift, (((BPlusNonLeafNode<T>)Children[index])
                .Children[Children[index].NumKeys]
                  ?? throw new NullChildReferenceException(
                    $"Child at index:{Children[index].NumKeys} within node:{ID}")
                    ).ID, -1, [], [], _Children[index].ID, -1, [], []));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            }
          }
          else if (_Children[index].NumKeys >= _Degree)
          {
#pragma warning disable CS8604 // Possible null reference argument.
            _Children[index + 1].GainsFromLeft(_Keys[index], _Contents[index], _Children[index]);
#pragma warning restore CS8604 // Possible null reference argument.
            _Keys[index] = _Children[index].Keys[_Children[index].NumKeys - 1];
            _Contents[index] = _Children[index].Contents[_Children[index].NumKeys - 1];
            _Children[index].LosesToRight();
            _BufferBlock.SendAsync((Status.UnderFlow, Children[index + 1].ID,
              Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents,
              Children[index].ID, Children[index].NumKeys,
              Children[index].Keys, Children[index].Contents));
            if (_Children[index] as BPlusNonLeafNode<T> != null)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
              _BufferBlock.SendAsync((Status.Shift, ((BPlusNonLeafNode<T>)Children[index + 1])
                .Children[0].ID, -1, [], [], _Children[index + 1].ID, -1, [], []));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
          }
          else
          {
#pragma warning disable CS8604 // Possible null reference argument.
            _Children[index].Merge(_Keys[index], _Contents[index], _Children[index + 1]);
#pragma warning restore CS8604 // Possible null reference argument.
            for (; index < _NumKeys - 1;)
            {
              _Keys[index] = _Keys[index + 1];
              _Contents[index] = _Contents[index + 1];
              index++;
              _Children[index] = _Children[index + 1];
            }
            _Keys[index] = default;
            _Contents[index] = default;
            _Children[index + 1] = default;
            _NumKeys--;
            _BufferBlock.SendAsync((Status.MergeParent, ID, NumKeys, Keys, Contents, 0, -1, [], []));
          }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
    public override void GainsFromRight(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling)
    {
      _Keys[_NumKeys] = dividerKey;
      _Contents[_NumKeys] = dividerData;
      _Children[++_NumKeys] = ((BPlusNonLeafNode<T>)sibiling).Children[0];
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
      _Keys[_NumKeys] = default;
      _Contents[_NumKeys] = default;
      _Children[_NumKeys] = _Children[_NumKeys + 1];
      _Children[_NumKeys + 1] = default;
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
    public override void GainsFromLeft(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling)
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
      _Children[0] = ((BPlusNonLeafNode<T>)sibiling).Children[sibiling.NumKeys];
    }

    /// <summary>
    /// Decrements the _NumKeys var.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-22</remarks>
    public override void LosesToRight()
    {
      _Children[_NumKeys] = default;
      _NumKeys--;
      _Keys[_NumKeys] = default;
      _Contents[_NumKeys] = default;
    }

    /// <summary>
    /// Prints out the contents of the node in JSON format.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-13</remarks>
    /// <param name="x">Hierachical Node ID</param>
    /// <returns>String with the entirety of this node's keys and contents arrays formmatted in JSON syntax.</returns>
    */
		public override string Traverse(string x)
    {
      string output = Spacer(x) + "{\n";
      output += Spacer(x) + "  \"node\":\"" + x + "\",\n"
        + Spacer(x) + "\"  ID\":" + _ID + ",\n" + Spacer(x) + "  \"keys\":[";
      for (int i = 0; i < _NumKeys; i++)
      {
        output += _Keys[i] + (i + 1 < _NumKeys ? "," : "");
      }
      output += "],\n" + Spacer(x) + "  \"children\":[\n";
      for (int i = 0; i <= _NumKeys; i++)
      {
        output += (_Children[i] ?? throw new NullChildReferenceException(
          $"Child at index:{i} within node:{ID}")).Traverse(x + "." + i)
          + (i + 1 <= _NumKeys ? "," : "") + "\n";
      }
      return output + Spacer(x) + "  ]\n" + Spacer(x) + "}";
    }

    static public long KeyCount(BPlusNonLeafNode<T> node)
    {
      long count = 0;
      if (node.Children[0] as BPlusNonLeafNode<T> != null)
        for (int i = 0; i <= node.NumKeys && node.Children[i] != null; i++)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
          count += KeyCount((BPlusNonLeafNode<T>)node.Children[i] ?? throw new NullChildReferenceException(
            $"Child at index:{i} within node:{node.ID}"));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
      else
        for (int i = 0; i <= node.NumKeys && node.Children[i] != null; i++)
        {
          count += (node.Children[i] ?? throw new NullChildReferenceException(
            $"Child at index:{i} within node:{node.ID}")).NumKeys;
        }
      return count + node.NumKeys;
    }
    static public int NodeCount(BPlusNonLeafNode<T> node)
    {
      int count = 0;
      if (node.Children[0] as BPlusNonLeafNode<T> != null)
        for (int i = 0; i <= node.NumKeys && node.Children[i] != null; i++)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
          count += NodeCount((BPlusNonLeafNode<T>)node.Children[i] ?? throw new NullChildReferenceException(
            $"Child at index:{i} within node:{node.ID}"));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
      else
        for (int i = 0; i <= node.NumKeys && node.Children[i] != null; i++)
        {
          count++;
        }
      return count + 1;
    }
  }

}
