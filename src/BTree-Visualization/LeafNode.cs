/**
Desc: Implements the leaf nodes of a B-Tree. Non-recursive function
iteration due to no children.
*/
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;
using BTreeVisualizationNode;

namespace BTreeVisualization
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
  public class LeafNode<T>(int degree, BufferBlock<(NodeStatus status,
      long id, int numKeys, int[] keys, T?[] contents, long altID,
      int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)
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
        BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys,
        T?[] contents, long altID, int altNumKeys, int[] altKeys,
        T?[] altContents)> bufferBlock) : this(degree, bufferBlock)
    {
      _NumKeys = keys.Length;
      for (int i = 0; i < keys.Length; i++)
      {
        _Keys[i] = keys[i];
        _Contents[i] = contents[i];
      }
    }

    /// <summary>
    /// Iterates over the _Keys array to find key. If found returns the
    /// index and this else returns -1 and this.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index and this node else returns -1 and
    /// this node.</returns>
    public override (int key, T content)? SearchKey(int key)
    {
      _BufferBlock.SendAsync((NodeStatus.SSearching, ID, -1, [], [], 0, -1, [], []));
      int result = Search(this, key);
      if (result != -1 && _Keys[result] == key)
      {
        _BufferBlock.SendAsync((NodeStatus.Found, ID, result, [key], [Contents[result]], 0, -1, [], []));
        return (result, Contents[result] ?? throw new NullContentReferenceException(
            $"Content at index:{result} within node:{ID}"));
      }
      else
      {
        _BufferBlock.SendAsync((NodeStatus.Found, ID, -1, [], [], 0, -1, [], []));
        return default;
      }
    }

    /// <summary>
    /// Searches for all keys, wihtin this node, (equal to or greater than key) and less than endKey.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Lower bound inclusive.</param>
    /// <param name="endKey">Upper bound exclusive.</param>
    /// <returns>A list of key-content pairs from the matching range in order of found.</returns>
    /// <exception cref="NullContentReferenceException">In the case that one of the key-content
    /// pairs would have a null for a value.</exception>
    public override List<(int key, T content)> SearchKeys(int key, int endKey)
    {
      _BufferBlock.SendAsync((NodeStatus.SSearching, ID, -1, [], [], 0, -1, [], []));
      List<(int key, T value)> result = [];
      int index = Search(this, key);
      if (index != -1)
        for (; index < _NumKeys && _Keys[index] >= key && _Keys[index] < endKey; index++)
        {
          result.Add((Keys[index], Contents[index] ?? throw new NullContentReferenceException(
            $"Content at index:{index} within node:{ID}")));
        }
      if (result.Count > 0)
      {
        int[] keys = new int[result.Count];
        T[] contents = new T[result.Count];
        for (int i = 0; i < result.Count; i++)
        {
          keys[i] = result[i].key;
          contents[i] = result[i].value;
        }
        _BufferBlock.SendAsync((NodeStatus.FoundRange, ID, result.Count, keys, contents, 0, -1, [], []));
      }
      else
      {
        _BufferBlock.SendAsync((NodeStatus.FoundRange, ID, -1, [], [], 0, -1, [], []));
      }
      return result;
    }

    /// <summary>
    /// Evenly splits the _Contents[] and _Keys[] of this node giving
    /// up the greater half to a new node.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>The new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).</returns>
    public override ((int, T), BTreeNode<T>) Split(long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.Split, ID, -1, [], [], 0, -1, [], []));
      int[] newKeys = new int[_Degree - 1];
      T[] newContent = new T[_Degree - 1];
      for (int i = 0; i < _Degree - 1; i++)
      {
        newKeys[i] = _Keys[i + _Degree];
        newContent[i] = _Contents[i + _Degree]
          ?? throw new NullContentReferenceException(
            $"Content at index:{i + _Degree} within node:{ID}");
        _Keys[i + _Degree] = default;
        _Contents[i + _Degree] = default;
      }
      _NumKeys = _Degree - 1;
      LeafNode<T> newNode = new(_Degree, newKeys, newContent, _BufferBlock);
      (int, T) dividerEntry = (_Keys[_NumKeys], _Contents[_NumKeys]
        ?? throw new NullContentReferenceException(
          $"Content at index:{NumKeys} within node:{ID}"));
      _Keys[_NumKeys] = default;
      _Contents[_NumKeys] = default;
      (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.SplitResult, ID, bufferVar.NumKeys,
        bufferVar.Keys, bufferVar.Contents, parentID, -1, [], []));
      (int NumKeys, int[] Keys, T?[] Contents) newNodeBufferVar = newNode.CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.SplitResult, newNode.ID,
        newNodeBufferVar.NumKeys, newNodeBufferVar.Keys, newNodeBufferVar.Contents, parentID, -1, [], []));
      return (dividerEntry, newNode);
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
    public override ((int, T?), BTreeNode<T>?) InsertKey(int key, T data, long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.ISearching, ID, -1, [], [], 0, -1, [], []));
      int index = Search(this, key);
      if (index == -1)
        index = _NumKeys;
      for (int j = _NumKeys - 1; j >= index; j--)
      {
        _Keys[j + 1] = _Keys[j];
        _Contents[j + 1] = _Contents[j];
      }
      _Keys[index] = key;
      _Contents[index] = data;
      _NumKeys++;
      (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.Inserted, ID, bufferVar.NumKeys, bufferVar.Keys, bufferVar.Contents, 0, -1, [], []));
      if (IsFull())
      {
        return Split(parentID);
      }
      return ((-1, default(T)), null);
    }

    /// <summary>
    /// Sends NodeDeleted status to the frontend for this
    /// node.
    /// </summary>
    /// <param name="id">ID of the parent node.</param>
    public override void DeleteNode(long id)
    {
      _BufferBlock.SendAsync((NodeStatus.NodeDeleted,
        ID, -1, [], [], id, -1, [], []));
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// If the entry exists it deletes it.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Integer to search for and delete if found.</param>
    public override void DeleteKey(int key)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID, -1, [], [], 0, -1, [], []));
      int i = Search(this, key);
      if (i != -1 && _Keys[i] == key)
      {
        for (; i < _NumKeys - 1; i++)
        {
          _Keys[i] = _Keys[i + 1];
          _Contents[i] = _Contents[i + 1];
        }
        _NumKeys--;
        _Keys[_NumKeys] = default;
        _Contents[_NumKeys] = default;
        (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.Deleted, ID, bufferVar.NumKeys, bufferVar.Keys, bufferVar.Contents, 0, -1, [], []));
      }
      else
      {
        _BufferBlock.SendAsync((NodeStatus.Deleted, ID, -1, [], [], 0, -1, [], []));
      }
    }

    /// <summary>
    /// Deletes the entries that match the range from
    /// key to endKey but not including endKey.
    /// Also applies to rightSibiling.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Start of range, inclusive</param>
    /// <param name="endKey">end of range, exclusive</param>
    /// <param name="rightSibiling">A node that is on the
    /// edge of the range.</param>
    /// <param name="parentID"></param>
    public override void DeleteKeysSplit(int key, int endKey,
      BTreeNode<T> rightSibiling, long parentID)
    {
      // if -1 it is last child index
      int firstKeyIndex = Search(this, key);
      // if -1 it is last child index
      int lastIndex = Search(rightSibiling, endKey);
      if (lastIndex == -1)
        lastIndex = rightSibiling.NumKeys;
      if (firstKeyIndex == -1)
        firstKeyIndex = _NumKeys;
      DeleteKeysLeft(firstKeyIndex, parentID);
      rightSibiling.DeleteKeysRight(lastIndex, parentID);
    }

    /// <summary>
    /// Deletes the entries that match the range from
    /// key to endKey but not including endKey.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Start of range, inclusive</param>
    /// <param name="endKey">end of range, exclusive</param>
    public override void DeleteKeysMain(int key, int endKey, long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID, -1, [], [],
        0, -1, [], []));
      int firstKeyIndex = Search(this, key);
      // if -1 it is last child index
      int lastIndex = Search(this, endKey);
      if (lastIndex == -1)
        lastIndex = _NumKeys;
      if (firstKeyIndex != -1 && firstKeyIndex != lastIndex)
      {// Range includes keys from this node.
        for (; lastIndex < _NumKeys; firstKeyIndex++, lastIndex++)
        {
          _Keys[firstKeyIndex] = _Keys[lastIndex];
          _Contents[firstKeyIndex] = _Contents[lastIndex];
        }
        for (int i = firstKeyIndex; i < _NumKeys; i++)
        {
          _Keys[i] = default;
          _Contents[i] = default;
        }
        _NumKeys = firstKeyIndex;
        (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.DeletedRange,
          ID, bufferVar.NumKeys, bufferVar.Keys, bufferVar.Contents, 0, -1, [], []));
      }
    }

    /// <summary>
    /// Deletes all entries from index and up.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="index">Index to start
    /// deleting from.</param>
    public override void DeleteKeysLeft(int index, long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID,
        -1, [], [], 0, -1, [], []));
      if (index != _NumKeys)
      {
        for (int i = index; i < _NumKeys; i++)
        {
          _Keys[i] = default;
          _Contents[i] = default;
        }
        _NumKeys = index;
        (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.DeletedRange,
          ID, bufferVar.NumKeys, bufferVar.Keys, bufferVar.Contents, 0, -1, [], []));
      }
      else
      {// No keys belong to range.
        _BufferBlock.SendAsync((NodeStatus.DeletedRange,
          ID, -1, [], [], 0, -1, [], []));
      }
    }

    /// <summary>
    /// Deletes all entries up to index but
    /// not including index. Then shifts
    /// remaining entries to the beginning of
    /// their arrays.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="index">Index to start
    /// copying from.</param>
    public override void DeleteKeysRight(int index, long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID,
        -1, [], [], 0, -1, [], []));
      if (index > 0)
      {
        int j = 0;
        for (int i = index; i < _NumKeys; i++, j++)
        {
          _Keys[j] = _Keys[i];
          _Contents[j] = _Contents[i];
        }
        for (; j < _NumKeys; j++)
        {
          _Keys[j] = default;
          _Contents[j] = default;
        }
        _NumKeys -= index;
        (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.DeletedRange, ID,
          bufferVar.NumKeys, bufferVar.Keys,
          bufferVar.Contents, 0, -1, [], []));
      }
      else
      {// No keys belong to range.
        _BufferBlock.SendAsync((NodeStatus.DeletedRange,
          ID, -1, [], [], 0, -1, [], []));
      }
    }

    /// <summary>
    /// Does nothing but indicate the recursion can stop.
    /// </summary>
    public override void RestoreRight()
    {
      _BufferBlock.SendAsync((NodeStatus.Restoration, ID,
        -1, [], [], 0, -1, [], []));
    }

    /// <summary>
    /// Does nothing but indicate the recursion can stop.
    /// </summary>
    public override void RestoreLeft()
    {
      _BufferBlock.SendAsync((NodeStatus.Restoration, ID,
        -1, [], [], 0, -1, [], []));
    }

    /// <summary>
    /// Returns its rigth most / last 
    /// key to the parent. Then it decrements the number of keys.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <returns>Tuple of Key and corresponding content.</returns>
    public override (int, T?) ForfeitKey()
    {
      (int, T?) keyToBeLost;
      if (_NumKeys != 0)
      {
        _NumKeys--;
        keyToBeLost = (_Keys[_NumKeys], _Contents[_NumKeys]
          ?? throw new NullContentReferenceException(
            $"Content at index:{_NumKeys} within node:{ID}"));
        _Keys[_NumKeys] = default;
        _Contents[_NumKeys] = default;
        (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.Forfeit, ID, bufferVar.NumKeys, bufferVar.Keys, bufferVar.Contents, 0, -1, [], []));
      }
      else
      {
        keyToBeLost = (0, default);
        _BufferBlock.SendAsync((NodeStatus.Forfeit, ID, -1, [], [], 0, -1, [], []));
      }
      return keyToBeLost;
    }

    /// <summary>
    /// Appends the given divider to its own arrays and grabs 
    /// all the entries from the sibiling adding those to its arrays as well.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="dividerKey">Key from parent between this node and sibiling.</param>
    /// <param name="dividerData">Coresponding Content to dividerKey.</param>
    /// <param name="sibiling">SibilinSpecialg to right. (Sibiling's Keys should be
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
      (int NumKeys, int[] Keys, T?[] Contents) bufferVar = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.Merge, ID, bufferVar.NumKeys, bufferVar.Keys, bufferVar.Contents, sibiling.ID, -1, [], []));
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
    public override void GainsFromRight(int diff, int dividerKey, T? dividerData, BTreeNode<T> sibiling)
    {
      if (dividerData != null)
      {
        _Keys[_NumKeys] = dividerKey;
        _Contents[_NumKeys] = dividerData;
        _NumKeys++;
        diff--;
        for (int i = 0; i < diff; i++)
        {
          _Keys[_NumKeys] = sibiling.Keys[i];
          _Contents[_NumKeys] = sibiling.Contents[i];
          _NumKeys++;
        }
      }
    }

    /// <summary>
    /// Shifts the values in the arrays by one to the left overwriting 
    /// the first entries and decrements the _NumKeys var.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    public override void LosesToLeft(int diff)
    {
      if (diff > 0)
      {
        int i = 0;
        int j = diff;
        for (; j < _NumKeys; i++, j++)
        {
          _Keys[i] = _Keys[j];
          _Contents[i] = _Contents[j];
        }
        while (i < _NumKeys)
        {
          _Keys[i] = default;
          _Contents[i++] = default;
        }
        _NumKeys -= diff;
      }
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
    public override void GainsFromLeft(int diff, int dividerKey, T? dividerData, BTreeNode<T> sibiling)
    {
      if (dividerData != null)
      {
        for (int i = _NumKeys - 1; i >= 0; i--)
        {
          _Keys[i + diff] = _Keys[i];
          _Contents[i + diff] = _Contents[i];
        }
        _NumKeys += diff;
        diff--;
        _Keys[diff] = dividerKey;
        _Contents[diff] = dividerData;
        diff--;
        for (int j = sibiling.NumKeys - 1; diff >= 0; diff--, j--)
        {
          _Keys[diff] = sibiling.Keys[j];
          _Contents[diff] = sibiling.Contents[j];
        }
      }
    }

    /// <summary>
    /// Decrements the _NumKeys var.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-22</remarks>
    public override void LosesToRight(int diff)
    {
      for (int i = 0; i < diff; i++)
      {
        _NumKeys--;
        _Keys[_NumKeys] = default;
        _Contents[_NumKeys] = default;
      }
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
      output += Spacer(x) + "  \"type\":\"leafnode\",\n"
        + Spacer(x) + "  \"node\":\"" + x + "\",\n"
        + Spacer(x) + "  \"ID\":" + _ID + ",\n"
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