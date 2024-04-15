/*
Author: Emily Elzinga and Tristan Anderson
Date: 2/07/2024
Desc: Describes functionality for non-leaf nodes on the BTree. Recursive function iteration due to children nodes.
*/
using System.Threading.Tasks.Dataflow;
using ThreadCommunication;


namespace BTreeVisualization
{
  /// <summary>
  /// Creates a non-leaf node for a B-Tree data structure with
  /// empty arrays of keys, contents, and children.
  /// </summary>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent non-leaf node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to be externally viewed.</param>
  public class NonLeafNode<T>(int degree, BufferBlock<(NodeStatus status,
    long id, int numKeys, int[] keys, T?[] contents, long altID,
    int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)
    : BTreeNode<T>(degree, bufferBlock)
  {
    /// <summary>
    /// Array to track child nodes of this node. These can be either Leaf or Non-Leaf.
    /// </summary>
    private BTreeNode<T>?[] _Children = new BTreeNode<T>[2 * degree];
    /// <summary>
    /// Getter for _Children[]
    /// </summary>
    public BTreeNode<T>?[] Children
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
    public NonLeafNode(int degree, int[] keys, T[] data,
      BTreeNode<T>[] children, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock) : this(degree, bufferBlock)
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
    /// Iterates over the _Keys array to find key.
    /// If found returns the index and this else returns -1 and this.
    /// </summary>
    /// <remarks>Copied and modified from
    /// LeafNode.SearchKey()</remarks>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns the index and this node else returns -1 and this node.</returns>
    public override (int key, T content)? SearchKey(int key)
    {
      _BufferBlock.SendAsync((NodeStatus.SSearching, ID, -1, [], [],
        0, -1, [], []));
      int result = Search(this, key);
      if (result == -1)
      {
        return (_Children[_NumKeys]
          ?? throw new NullChildReferenceException(
            $"Child at index:{_NumKeys} within node:{ID}")).SearchKey(key);
      }
      else if (_Keys[result] == key)
      {
        _BufferBlock.SendAsync((NodeStatus.Found, ID, result, [key],
          [Contents[result]], 0, -1, [], []));
        return (Keys[result], Contents[result]
          ?? throw new NullContentReferenceException(
            $"Content at index:{result} within node:{ID}"));
      }
      else
      {
        return (_Children[result]
          ?? throw new NullChildReferenceException(
            $"Child at index:{result} within node:{ID}")).SearchKey(key);
      }
    }

    /// <summary>
    /// Recursively searches for all keys, wihtin this node and children nodes, (equal to or greater than key) and less than endKey.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <param name="key">Lower bound inclusive.</param>
    /// <param name="endKey">Upper bound exclusive.</param>
    /// <returns>A list of key-content pairs from the matching range in order of found.</returns>
    /// <exception cref="NullChildReferenceException">In the case that one of the children
    /// nodes in the range is null.</exception>
    /// <exception cref="NullContentReferenceException">In the case that one of the key-content
    /// pairs would have a null for a value.</exception>
    public override List<(int key, T content)> SearchKeys(int key, int endKey)
    {
      _BufferBlock.SendAsync((NodeStatus.SSearching, ID, -1, [], [],
        0, -1, [], []));
      List<(int, T)> result = [];
      int index = Search(this, key);
      if (index != -1)
      {
        result.AddRange((_Children[index]
          ?? throw new NullChildReferenceException(
            $"Child at index:{index} within node:{ID}")).SearchKeys(key, endKey));
        for (; index < _NumKeys && _Keys[index] >= key && _Keys[index] < endKey;)
        {
          result.Add((Keys[index], Contents[index]
            ?? throw new NullContentReferenceException(
              $"Content at index:{index} within node:{ID}")));
          _BufferBlock.SendAsync((NodeStatus.FoundRange, ID, 1, [_Keys[index]],
            [_Contents[index]], 0, -1, [], []));
          index++;
          result.AddRange((_Children[index]
            ?? throw new NullChildReferenceException(
              $"Child at index:{index} within node:{ID}")).SearchKeys(key, endKey));
        }
      }
      if (_Keys[_NumKeys - 1] < endKey)
        result.AddRange((_Children[_NumKeys]
          ?? throw new NullChildReferenceException(
            $"Child at index:{NumKeys} within node:{ID}")).SearchKeys(key,
            endKey));
      return result;
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
    public override ((int, T?), BTreeNode<T>?) InsertKey(int key, T data,
      long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.ISearching, ID, -1, [key], [data],
        0, -1, [], []));
      ((int, T?), BTreeNode<T>?) result;
      int i = 0;
      while (i < _NumKeys && key > _Keys[i])
      {
        i++;
      }
      if (i == _NumKeys || key != _Keys[i] || key == 0)
      {
        result = (_Children[i]
          ?? throw new NullChildReferenceException(
            $"Child at index:{i} within node:{ID}")).InsertKey(key, data, ID);
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
          (int, int[], T?[]) bufferVar = CreateBufferVar();
          _BufferBlock.SendAsync((NodeStatus.SplitInsert, ID, bufferVar.Item1,
            bufferVar.Item2, bufferVar.Item3, 0, -1, [], []));
          if (IsFull())
          {
            return Split(parentID);
          }
        }
      }
      else
      {
        _BufferBlock.SendAsync((NodeStatus.SplitInsert, 0, -1, [], [],
          0, -1, [], []));
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
    public override ((int, T), BTreeNode<T>) Split(long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.Split, ID, -1, [], [], 0, -1, [], []));
      int[] newKeys = new int[_Degree - 1];
      T[] newContent = new T[_Degree - 1];
      BTreeNode<T>[] newChildren = new BTreeNode<T>[_Degree];
      int i = 0;
      for (; i < _Degree - 1; i++)
      {
        newKeys[i] = _Keys[i + _Degree];
        newContent[i] = _Contents[i + _Degree]
          ?? throw new NullContentReferenceException(
            $"Content at index:{_NumKeys} within node:{ID}");
        newChildren[i] = _Children[i + _Degree]
          ?? throw new NullChildReferenceException(
            $"Child at index:{i + _Degree} within node:{ID}");
        _Keys[i + _Degree] = default;
        _Contents[i + _Degree] = default;
        _Children[i + _Degree] = default;
      }
      newChildren[i] = _Children[i + _Degree]
        ?? throw new NullChildReferenceException(
          $"Child at index:{i + _Degree} within node:{ID}");
      _Children[i + _Degree] = default;
      _NumKeys = _Degree - 1;
      NonLeafNode<T> newNode = new(_Degree, newKeys, newContent,
        newChildren, _BufferBlock);
      (int, T) dividerEntry = (_Keys[_NumKeys], _Contents[_NumKeys]
        ?? throw new NullContentReferenceException(
          $"Content at index:{_NumKeys} within node:{ID}"));
      _Keys[_NumKeys] = default;
      _Contents[_NumKeys] = default;
      (int, int[], T?[]) bufferVar = CreateBufferVar();
      (int, int[], T?[]) newNodeBufferVar = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.SplitResult, ID, bufferVar.Item1,
        bufferVar.Item2, bufferVar.Item3, parentID, -1, [], []));
      _BufferBlock.SendAsync((NodeStatus.SplitResult, newNode.ID,
        newNodeBufferVar.Item1, newNodeBufferVar.Item2, newNodeBufferVar.Item3,
        parentID, -1, [], []));
      for (int j = 0; j <= newNode.NumKeys; j++)
      {
        _BufferBlock.SendAsync((NodeStatus.Shift, newNode.ID, -1, [], [],
          (newNode.Children[j] ?? throw new NullChildReferenceException(
            $"Child at index:{j} within node:{newNode.ID}")).ID, -1, [], []));
      }
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
		public override void DeleteKey(int key)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID, -1, [], [],
        0, -1, [], []));
      int result = Search(this, key);
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
        _BufferBlock.SendAsync((NodeStatus.Deleted, ID, NumKeys, Keys,
          Contents, 0, -1, [], []));
        MergeAt(result);
      }
      else
      {
        (_Children[result] ?? throw new NullChildReferenceException(
          $"Child at index:{result} within node:{ID}")).DeleteKey(key);
        MergeAt(result);
      }
    }

    public override bool CheckMyself()
    {
      bool result = _Children[_NumKeys] != null;
      for (int i = 0; i < _NumKeys; i++)
      {
        result = result && _Children[i] != null && _Contents[i] != null;
      }
      if (!result)
        Console.Write("here");
      return result;
    }

    public override void DeleteKeysSplit(int key, int endKey,
      BTreeNode<T> rightSibiling)
    {
      // if -1 it is last child index
      int firstKeyIndex = Search(this, key);
      // if -1 it is last child index
      int lastIndex = Search(rightSibiling, endKey);
      if (lastIndex == -1)
        lastIndex = rightSibiling.NumKeys;
      if (firstKeyIndex == -1)
        firstKeyIndex = _NumKeys;
      (_Children[firstKeyIndex] ?? throw new NullChildReferenceException(
        $"Child at index:{firstKeyIndex} within node:{ID}")).DeleteKeysSplit(
          key, endKey, ((NonLeafNode<T>)rightSibiling).Children[lastIndex]
          ?? throw new NullChildReferenceException(
          $"Child at index:{lastIndex} within node:{ID}"));
      DeleteKeysLeft(firstKeyIndex);
      rightSibiling.DeleteKeysRight(lastIndex);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
      (int?, T?, BTreeNode<T>?) merged = _Children[firstKeyIndex]
        .RebalanceNodes(((NonLeafNode<T>)rightSibiling).Children[0]);
      if (_Children[0] as NonLeafNode<T> != null)
        ((NonLeafNode<T>)rightSibiling).ResolveLeadingKey(0);
      _NumKeys += NormalRebalanceReturn(merged, firstKeyIndex);
      if (_Children[firstKeyIndex].IsFull())
      {
        ((int, T?), BTreeNode<T>?) result = _Children[firstKeyIndex].Split(ID);
        MakeSpaceAt(firstKeyIndex);
        _Keys[firstKeyIndex] = result.Item1.Item1;
        _Contents[firstKeyIndex] = result.Item1.Item2;
        _Children[firstKeyIndex + 1] = result.Item2;
      }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      CheckMyself();
    }

    public override void DeleteKeys(int key, int endKey)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID, -1, [], [],
        0, -1, [], []));
      int firstKeyIndex = Search(this, key);
      // if -1 it is last child index
      int lastIndex = Search(this, endKey);
      if (lastIndex == -1)
        lastIndex = _NumKeys;
      if (firstKeyIndex == -1)
      {// Range is to the far right and doesn't include any keys of this node.
        (_Children[_NumKeys] ?? throw new NullChildReferenceException(
          $"Child at index:{_NumKeys} within node:{ID}"))
          .DeleteKeys(key, endKey);
        CheckForMultiMerge(_NumKeys);
      }
      else if (firstKeyIndex == lastIndex)
      {// Range doesn't include any keys from this node.
        (_Children[firstKeyIndex] ?? throw new NullChildReferenceException(
          $"Child at index:{firstKeyIndex} within node:{ID}"))
          .DeleteKeys(key, endKey);
        CheckForMultiMerge(firstKeyIndex);
      }
      else
      {// Range includes keys from this node.
        (_Children[firstKeyIndex] ?? throw new NullChildReferenceException(
          $"Child at index:{firstKeyIndex} within node:{ID}"))
          .DeleteKeysSplit(key, endKey, _Children[lastIndex]
            ?? throw new NullChildReferenceException(
            $"Child at index:{firstKeyIndex} within node:{ID}"));
        if (_Children[lastIndex] as NonLeafNode<T> != null)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          ((NonLeafNode<T>)_Children[lastIndex]).ResolveLeadingKey(0);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
        (int?, T?, BTreeNode<T>?) merged =
          _Children[firstKeyIndex].RebalanceNodes(_Children[lastIndex]);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        firstKeyIndex += NormalRebalanceReturn(merged, firstKeyIndex);
        if (firstKeyIndex != lastIndex)
        {
          int i = firstKeyIndex;
          for (int j = lastIndex; j < _NumKeys;)
          {
            _Keys[i] = _Keys[j];
            _Contents[i] = _Contents[j];
            Children[++i] = _Children[++j];
          }
          for (; i < _NumKeys;)
          {
            _Keys[i] = default;
            _Contents[i] = default;
            Children[++i] = default;
          }
          _NumKeys -= lastIndex - firstKeyIndex;
        }
      }
      CheckMyself();
    }

    private void CheckForMultiMerge(int firstKeyIndex)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      if (_Children[firstKeyIndex].NumKeys < _Degree - 2)
      {
        if (firstKeyIndex == _NumKeys)
          firstKeyIndex--;
#pragma warning disable CS8604 // Possible null reference argument.
        ((int, T?), BTreeNode<T>?) result = _Children[firstKeyIndex]
          .InsertKey(_Keys[firstKeyIndex], _Contents[firstKeyIndex], ID);
        if (result.Item2 != null)
        {// Split in child occured
          (_Keys[firstKeyIndex], _Contents[firstKeyIndex++]) = result.Item1;
          MakeSpaceAt(firstKeyIndex);
          _Children[firstKeyIndex] = result.Item2;
        }
        else
        {
          (int?, T?, BTreeNode<T>?) merged =
            _Children[firstKeyIndex].RebalanceNodes(_Children[firstKeyIndex + 1]);
          if (merged.Item1 != null)
          {
            _Keys[firstKeyIndex] = (int)merged.Item1;
            _Contents[firstKeyIndex] = merged.Item2;
            MakeSpaceAt(++firstKeyIndex);
            _Children[firstKeyIndex] = merged.Item3;
          }
          else if (merged.Item3 != null)
          {
            _Children[firstKeyIndex] = merged.Item3;
          }
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      }
      else
      {
        MergeAt(firstKeyIndex);
      }
      CheckMyself();
    }

    private void MakeSpaceAt(int firstKeyIndex)
    {
      for (int i = _NumKeys; i > firstKeyIndex; i--)
      {
        _Children[i + 1] = _Children[i];
        _Keys[i] = _Keys[i - 1];
        _Contents[i] = _Contents[i - 1];
      }
      CheckMyself();
    }

    public override void DeleteKeysLeft(int index)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID,
        -1, [], [], 0, -1, [], []));
      if (index != _NumKeys)
      {
        for (int i = index; i < _NumKeys;)
        {
          _Keys[i] = default;
          _Contents[i] = default;
          i++;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          _BufferBlock.SendAsync((NodeStatus.NodeDeleted,
            _Children[i].ID, -1, [], [], 0, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
          _Children[i] = default;
        }
        _NumKeys = index;
        (int, int[], T?[]) bufferVar = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.DeletedRange,
          ID, bufferVar.Item1, bufferVar.Item2, bufferVar.Item3, 0, -1, [], []));
      }
      else
      {
        _BufferBlock.SendAsync((NodeStatus.DeletedRange,
          ID, -1, [], [], 0, -1, [], []));
      }
      CheckMyself();
    }

    public override void DeleteKeysRight(int index)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID,
        -1, [], [], 0, -1, [], []));
      if (index > 0)
      {
        int j = 0;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _BufferBlock.SendAsync((NodeStatus.NodeDeleted,
          _Children[j].ID, -1, [], [], 0, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        _Children[j] = _Children[index];
        for (int i = index; i < _NumKeys;)
        {
          _Keys[j] = _Keys[i];
          _Contents[j] = _Contents[i];
          j++;
          if (j < index)
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _BufferBlock.SendAsync((NodeStatus.NodeDeleted,
              _Children[j].ID, -1, [], [], 0, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
          _Children[j] = _Children[++i];
        }
        while (j < _NumKeys)
        {
          _Keys[j] = default;
          _Contents[j] = default;
          _Children[++j] = default;
        }
        _NumKeys -= index;
        (int, int[], T?[]) bufferVar = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.DeletedRange, ID,
          bufferVar.Item1, bufferVar.Item2,
          bufferVar.Item3, 0, -1, [], []));
      }
      CheckMyself();
    }

    private void ResolveLeadingKey(int index)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
      ((int, T?), BTreeNode<T>?) result = _Children[index + 1]
        .InsertKey(_Keys[index], _Contents[index], ID);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      if (result.Item2 != null)
      {// Split in child occured
        _Children[index] = _Children[index + 1];
        (_Keys[index], _Contents[index]) = result.Item1;
        _Children[index + 1] = result.Item2;
        CheckMyself();
      }
      else
      {
        LosesToLeft(1);
        CheckMyself();
      }
    }

    public override (int?, T?, BTreeNode<T>?) RebalanceNodes(BTreeNode<T> rightSibiling)
    {
      int? dividerKey;
      T? dividerData;
      if (_NumKeys + rightSibiling.NumKeys >= 2 * _Degree - 2)
      {// Must balance keys between nodes
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        (dividerKey, dividerData) = _Children[_NumKeys].ForfeitKey();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        MergeAt(_NumKeys);
        int diff = ((_NumKeys + rightSibiling.NumKeys - 1) / 2) + 1 - _NumKeys;
        if (diff > 0)
        {// Not enough keys in this node
          _Keys[_NumKeys] = (int)dividerKey;
          _Contents[_NumKeys] = dividerData;
          _NumKeys++;
          for (int j = _NumKeys, i = 0; i < diff; i++, j++)
          {
            _Keys[j] = rightSibiling.Keys[i];
            _Contents[j] = rightSibiling.Contents[i];
            _Children[j] = ((NonLeafNode<T>)rightSibiling).Children[i];
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            _BufferBlock.SendAsync((NodeStatus.Shift, ID, -1, [], [],
              ((NonLeafNode<T>)rightSibiling).Children[i].ID, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
          }
          _NumKeys += diff - 1;
          dividerKey = _Keys[_NumKeys];
          dividerData = _Contents[_NumKeys];
          _Keys[_NumKeys] = default;
          _Contents[_NumKeys] = default;
          rightSibiling.LosesToLeft(diff);
        }
        else if (diff < 0)
        {// Not enough keys in sibiling
          rightSibiling.GainsFromLeft(-diff, (int)dividerKey, dividerData, this);
          dividerKey = _Keys[_NumKeys + diff];
          dividerData = _Contents[_NumKeys + diff];
          LosesToRight(-diff);
        }
        (int, int[], T?[]) bufferVarLeft = CreateBufferVar();
        (int, int[], T?[]) bufferVarRight = rightSibiling.CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.Rebalanced, ID,
          bufferVarLeft.Item1, bufferVarLeft.Item2, bufferVarLeft.Item3
          , rightSibiling.ID, bufferVarRight.Item1, bufferVarRight.Item2,
          bufferVarRight.Item3));
        CheckMyself();
        return (dividerKey, dividerData, rightSibiling);
      }
      else
      {// Not enough keys for two nodes
        if (_NumKeys == 0)
        {
          CheckMyself();
          return (null, default, rightSibiling);
        }
        else
        {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          (dividerKey, dividerData) = _Children[_NumKeys].ForfeitKey();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
          MergeAt(_NumKeys);
          Merge((int)dividerKey, dividerData, rightSibiling);
          CheckMyself();
          return (null, default, default);
        }
      }
    }

    private int NormalRebalanceReturn((int?, T?, BTreeNode<T>?) merged, int indexToAssignTo)
    {
      if (merged.Item2 != null)
      {
#pragma warning disable CS8629 // Nullable value type may be null.
        _Keys[indexToAssignTo] = (int)merged.Item1;
#pragma warning restore CS8629 // Nullable value type may be null.
        _Contents[indexToAssignTo] = merged.Item2;
        _Children[++indexToAssignTo] = merged.Item3;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _BufferBlock.SendAsync((NodeStatus.Shift, ID, -1, [], [],
          _Children[indexToAssignTo].ID, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        CheckMyself();
        return 1;
      }
      else if (merged.Item3 != default)
      {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _BufferBlock.SendAsync((NodeStatus.NodeDeleted, _Children[indexToAssignTo].ID, -1, [], [],
          0, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        _Children[indexToAssignTo] = merged.Item3;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _BufferBlock.SendAsync((NodeStatus.Shift, ID, -1, [], [],
          _Children[indexToAssignTo].ID, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      }
      CheckMyself();
      return 0;
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
      _BufferBlock.SendAsync((NodeStatus.FSearching, ID, -1, [], [], 0, -1, [], []));
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
    /// <param name="rightSibiling">Sibiling to right. (Sibiling's Keys should be
    /// greater than all the keys in the called node.)</param>
    public override void Merge(int dividerKey, T dividerData, BTreeNode<T> rightSibiling)
    {
      _Keys[_NumKeys] = dividerKey;
      _Contents[_NumKeys] = dividerData;
      _NumKeys++;
      for (int i = 0; i < rightSibiling.NumKeys; i++)
      {
        _Keys[_NumKeys + i] = rightSibiling.Keys[i];
        _Contents[_NumKeys + i] = rightSibiling.Contents[i];
        _Children[_NumKeys + i] = ((NonLeafNode<T>)rightSibiling).Children[i];
      }
      _Children[_NumKeys + rightSibiling.NumKeys] = ((NonLeafNode<T>)rightSibiling).Children[rightSibiling.NumKeys];
      _NumKeys += rightSibiling.NumKeys;
      (int, int[], T?[]) bufferVar = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.Merge, ID, bufferVar.Item1, bufferVar.Item2, bufferVar.Item3, rightSibiling.ID, -1, [], []));
    }

    public override void Merge(BTreeNode<T> rightSibiling)
    {
      int holdOldNumKeys = _NumKeys;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      (_Keys[_NumKeys], _Contents[_NumKeys]) = _Children[_NumKeys++].ForfeitKey();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      for (int i = 0; i < rightSibiling.NumKeys; i++)
      {
        _Keys[_NumKeys] = rightSibiling.Keys[i];
        _Contents[_NumKeys] = rightSibiling.Contents[i];
        _Children[_NumKeys++] = ((NonLeafNode<T>)rightSibiling).Children[i];
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _BufferBlock.SendAsync((NodeStatus.Shift, ID, -1, [], [], ((NonLeafNode<T>)rightSibiling).Children[i].ID, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      }
      _Children[_NumKeys] = ((NonLeafNode<T>)rightSibiling).Children[rightSibiling.NumKeys];
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _BufferBlock.SendAsync((NodeStatus.Shift, ID, -1, [], [], ((NonLeafNode<T>)rightSibiling).Children[rightSibiling.NumKeys].ID, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      (int, int[], T?[]) bufferVar = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.Merge, ID, bufferVar.Item1, bufferVar.Item2, bufferVar.Item3, rightSibiling.ID, -1, [], []));
      MergeAt(holdOldNumKeys);
    }

    /// <summary>
    /// Checks the child at index for underflow. If so it then checks for _Degree
    /// number of children in the right child of the key. _Degree or greater means
    /// merging the two will result in an overflow or a split. If less than _Degree
    /// then the merge will not be a full node needing to be split immediately.
    /// This is meant to reduce the number of times elements are moved back and forth.
    /// Underflow is _NumKeys <= _Degree - 2.
    /// Full is _NumKeys == 2 * _Degree - 1.
    /// Don't forget the dividing key adds 1
    /// Sum of the two nodes in minimal bad scenario:
    /// (_Degree - 2) + _Degree + 1 == 2 * _Degree - 1
    /// Results in Full at least.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="index">Index of affected child node.</param>
    public void MergeAt(int index)
    {
      bool emptyNode = (_Children[index] ?? throw new NullChildReferenceException(
          $"Child at index:{index} within node:{ID}")).NumKeys == 0
        || (index != _NumKeys && (_Children[index + 1]
          ?? throw new NullChildReferenceException(
            $"Child at index:{index} within node:{ID}")).NumKeys == 0);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      if (_Children[index].IsUnderflow() || emptyNode)
      {
        if (index == _NumKeys)
        {
          index--;
          if (_Children[index] == null)
          {
            throw new NullChildReferenceException(
              $"Child at index:{index} within node:{ID}");
          }
        }
        if ((_Children[index + 1] ?? throw new NullChildReferenceException(
          $"Child at index:{index} within node:{ID}")).NumKeys >= _Degree && !emptyNode)
        {
#pragma warning disable CS8604 // Possible null reference argument.
          _Children[index].GainsFromRight(_Keys[index], _Contents[index], _Children[index + 1]);
#pragma warning restore CS8604 // Possible null reference argument.
          _Keys[index] = _Children[index + 1].Keys[0];
          _Contents[index] = _Children[index + 1].Contents[0];
          _Children[index + 1].LosesToLeft();
          _BufferBlock.SendAsync((NodeStatus.UnderFlow, Children[index].ID, Children[index].NumKeys,
            Children[index].Keys, Children[index].Contents, Children[index + 1].ID,
            Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents));
          if (_Children[index] as NonLeafNode<T> != null)
          {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            _BufferBlock.SendAsync((NodeStatus.Shift, _Children[index].ID, -1, [], [],
              (((NonLeafNode<T>)Children[index]).Children[Children[index].NumKeys]
                ?? throw new NullChildReferenceException(
                  $"Child at index:{Children[index].NumKeys} within node:{ID}")
                  ).ID, -1, [], []));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
          }
        }
        else if (_Children[index].NumKeys >= _Degree && !emptyNode)
        {
#pragma warning disable CS8604 // Possible null reference argument.
          _Children[index + 1].GainsFromLeft(1, _Keys[index], _Contents[index], _Children[index]);
#pragma warning restore CS8604 // Possible null reference argument.
          _Keys[index] = _Children[index].Keys[_Children[index].NumKeys - 1];
          _Contents[index] = _Children[index].Contents[_Children[index].NumKeys - 1];
          _Children[index].LosesToRight(1);
          _BufferBlock.SendAsync((NodeStatus.UnderFlow, Children[index + 1].ID,
            Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents,
            Children[index].ID, Children[index].NumKeys,
            Children[index].Keys, Children[index].Contents));
          if (_Children[index] as NonLeafNode<T> != null)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            _BufferBlock.SendAsync((NodeStatus.Shift, _Children[index + 1].ID, -1, [], [],
              ((NonLeafNode<T>)Children[index + 1]).Children[0].ID, -1, [], []));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
        else
        {
#pragma warning disable CS8604 // Possible null reference argument.
          if (_Children[index].NumKeys == 0)
          {
            if (index == 0)
            {
              _Children[index + 1].GainsFromLeft(1, _Keys[index], _Contents[index], _Children[index]);
              _BufferBlock.SendAsync((NodeStatus.NodeDeleted, _Children[index].ID, -1, [], [], 0, -1, [], []));
              _Children[index] = _Children[index + 1];
            }
            else
            {
              _Children[index - 1].Merge(_Keys[index], _Contents[index], _Children[index]);
            }
          }
          else
          {
            _Children[index].Merge(_Keys[index], _Contents[index], _Children[index + 1]);
          }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
          for (; index < _NumKeys - 1;)
          {
            _Keys[index] = _Keys[index + 1];
            _Contents[index] = _Contents[index + 1];
            _Children[++index] = _Children[index + 1];
          }
          _Keys[index] = default;
          _Contents[index] = default;
          _Children[index + 1] = default;
          _NumKeys--;
          _BufferBlock.SendAsync((NodeStatus.MergeParent, ID, NumKeys, Keys, Contents, 0, -1, [], []));
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
      _Keys[_NumKeys] = default;
      _Contents[_NumKeys] = default;
      _Children[_NumKeys] = _Children[_NumKeys + 1];
      _Children[_NumKeys + 1] = default;
    }

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
          _Children[i] = _Children[j];
        }
        _Children[i] = _Children[j];
        while (i < _NumKeys)
        {
          _Keys[i] = default;
          _Contents[i] = default;
          i++;
          _Children[i] = default;
        }
        _NumKeys -= diff;
      }
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
    public override void GainsFromLeft(int diff, int dividerKey, T? dividerData, BTreeNode<T> sibiling)
    {
      _Children[_NumKeys + diff] = _Children[_NumKeys];
      for (int i = _NumKeys - 1; i >= 0; i--)
      {
        _Keys[i + diff] = _Keys[i];
        _Contents[i + diff] = _Contents[i];
        _Children[i + diff] = _Children[i];
      }
      _NumKeys += diff;
      diff--;
      _Keys[diff] = dividerKey;
      _Contents[diff] = dividerData;
      _Children[diff] = ((NonLeafNode<T>)sibiling).Children[sibiling.NumKeys];
      diff--;
      for (int j = sibiling.NumKeys - 1; diff >= 0; diff--, j--)
      {
        _Keys[diff] = sibiling.Keys[j];
        _Contents[diff] = sibiling.Contents[j];
        _Children[diff] = ((NonLeafNode<T>)sibiling).Children[j];
      }
    }

    public override void GainsFromLeftSpecial(int diff, BTreeNode<T> sibiling)
    {
      _Children[_NumKeys + diff] = _Children[_NumKeys];
      for (int i = _NumKeys - 1; i >= 0; i--)
      {
        _Keys[i + diff] = _Keys[i];
        _Contents[i + diff] = _Contents[i];
        _Children[i + diff] = _Children[i];
      }
      _NumKeys += diff;
      for (int j = sibiling.NumKeys - diff, i = 0; i < diff;)
      {
        _Keys[i] = sibiling.Keys[j];
        _Contents[i] = sibiling.Contents[j];
        _Children[++i] = ((NonLeafNode<T>)sibiling).Children[++j];
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        _BufferBlock.SendAsync((NodeStatus.Shift, ID, -1, [], [], ((NonLeafNode<T>)sibiling).Children[j].ID, -1, [], []));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
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
        _Children[_NumKeys] = default;
        _NumKeys--;
        _Keys[_NumKeys] = default;
        _Contents[_NumKeys] = default;
      }
    }

    /// <summary>
    /// Prints out the contents of the node in JSON format.
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-13</remarks>
    /// <param name="x">Hierachical Node ID</param>
    /// <returns>String with the entirety of this node's keys 
    /// and contents arrays formmatted in JSON syntax.</returns>
    public override string Traverse(string x)
    {
      string output = Spacer(x) + "{\n";
      output += Spacer(x) + "  \"type\":\"node\",\n"
        + Spacer(x) + "  \"node\":\"" + x + "\",\n"
        + Spacer(x) + "  \"ID\":" + _ID + ",\n" + Spacer(x) + "  \"keys\":[";
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
        output += (_Children[i] ?? throw new NullChildReferenceException(
          $"Child at index:{i} within node:{ID}")).Traverse(x + "." + i)
          + (i + 1 <= _NumKeys ? "," : "") + "\n";
      }
      return output + Spacer(x) + "  ]\n" + Spacer(x) + "}";
    }

    /// <summary>
    /// Gets the total number keys in all children of this node and itself.
    /// </summary>
    /// <returns>Count of keys.</returns>
    /// <exception cref="NullChildReferenceException"></exception>
    static public long KeyCount(NonLeafNode<T> node)
    {
      long count = 0;
      if (node.Children[0] as NonLeafNode<T> != null)
        for (int i = 0; i <= node.NumKeys && node.Children[i] != null; i++)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
          count += KeyCount((NonLeafNode<T>)node.Children[i] ?? throw new NullChildReferenceException(
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

    /// <summary>
    /// Gets the total number of nodes from this node down plus itself.
    /// </summary>
    /// <returns>Count of nodes.</returns>
    /// <exception cref="NullChildReferenceException"></exception>
    static public int NodeCount(NonLeafNode<T> node)
    {
      int count = 0;
      if (node.Children[0] as NonLeafNode<T> != null)
        for (int i = 0; i <= node.NumKeys && node.Children[i] != null; i++)
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
          count += NodeCount((NonLeafNode<T>)node.Children[i] ?? throw new NullChildReferenceException(
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
