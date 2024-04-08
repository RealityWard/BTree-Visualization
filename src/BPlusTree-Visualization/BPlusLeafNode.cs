/*
Primary Author: Andreas Kramer 
Secondary Author: Emily Elzinga
Remark: contains code from BTree implementation (Tristan Anderson and others)
Date: 03/04/2024
Desc: Describes functionality for non-leaf nodes on the B+Tree. Recursive function iteration due to children nodes.
*/

using System.Net;
using System.Threading.Tasks.Dataflow;
using BTreeVisualization;
using ThreadCommunication;



namespace BPlusTreeVisualization
{

  /// <summary>
  /// Creates a leaf node for a B+Tree data structure with
  /// empty arrays of keys and contents as well as references to a next node and a previous node forming a doubly linked list
  /// </summary>
  /// <remarks>Author: Andreas Kramer</remarks>
  /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
  /// <param name="degree">Same as parent non-leaf node/tree</param>
  /// <param name="bufferBlock">Output Buffer for Status updates to
  /// be externally viewed.</param>

  public class BPlusLeafNode<T>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys,
          T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)
          : BPlusTreeNode<T>(degree, bufferBlock)
  {
    private BPlusLeafNode<T>? _NextNode;

    public BPlusLeafNode<T>? GetNextNode()
    {
      return _NextNode;
    }
    private BPlusLeafNode<T>? _PrevNode;

    protected T?[] _Contents = new T[degree];

    public T?[] Contents
    {
      get { return _Contents; }
    }

    /// <summary>
    /// Creates a leaf node for a B+Tree data structure
    /// with starting values of the passed arrays of
    /// keys and contents. Sets the NumKeys to the length of keys[].
    /// </summary>
    /// <remarks>Author: Andreas Kramern</remarks>
    /// <param name="degree">Same as parent non-leaf node</param>
    /// <param name="keys">Values to initialize in _Keys[]</param>
    /// <param name="contents">Values to initialize in _Contents[]</param>
    /// <param name="bufferBlock">Output Buffer for Status updates to be
    /// externally viewed.</param>

    public BPlusLeafNode(int degree, int[] keys, T[] contents,
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
      _NextNode = null;
      _PrevNode = null;
    }
    /// <summary>
    /// Traverses through the node to find the index of the found key
    /// </summary>
    /// <param name="key">Key that is being searched for</param>
    /// <returns>If found returns index, if not, returns -1.</returns>

    private int Search(int key)
    {
      for (int i = 0; i < _NumKeys; i++)
      {
        if (_Keys[i] == key)
        {
          _BufferBlock.SendAsync((NodeStatus.Found, ID, i, [key], [Contents[i]], 0, -1, [], []));
          return i;
        }
      }
      _BufferBlock.SendAsync((NodeStatus.Found, ID, -1, [], [], 0, -1, [], []));
      return -1;
    }

    /// <summary>
    /// Initializes searching for a key in a leaf node
    /// </summary>
    /// <param name="key">Key that is being searched for</param>
    /// <returns>returns the correct index or -1 and this node.</returns>

    public override (int, BPlusTreeNode<T>) SearchKey(int key)
    {
      _BufferBlock.SendAsync((NodeStatus.SSearching, ID, -1, [], [], 0, -1, [], []));
      return (Search(key), this);
    }
    /// <summary>
    /// Inserts a key and the corresponding data into the right index of this node.
    /// If full calls Split() and propagates changes upward
    /// </summary>
    /// <param name="key">Key to be inserted int _Keys[]</param>
    /// <param name="data">Corresponding data to be inserted into _Contents[]</param>
    /// <returns>If Split() is called, propagates changes upward ((dividerkey,Content),new Node)
    /// if not, returns a default value.</returns>

    public override ((int, T?), BPlusTreeNode<T>?) InsertKey(int key, T data, long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.ISearching, ID, -1, [], [], 0, -1, [], []));
      int i = 0;
      while (i < _NumKeys && key >= _Keys[i])
        i++;
      for (int j = _NumKeys - 1; j >= i; j--)
      {
        _Keys[j + 1] = _Keys[j];
        _Contents[j + 1] = _Contents[j];
      }
      _Keys[i] = key;
      _Contents[i] = data;
      _NumKeys++;
      (int, int[], T?[]) temp = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.Inserted, ID, temp.Item1, temp.Item2, temp.Item3, 0, -1, [], []));
      if (IsFull())
      {
        return Split(parentID);
      }

      return ((-1, default(T)), null);
    }
    /// <summary>
    /// Splits this leaf node into two leaf nodes and gives the bigger half to the new node
    /// Contents and keys are split up, both NumKeys values are adjusted as well as new linking of the doubly linked list
    /// </summary>
    /// <returns>Returns the dividerEntry (dividerKey and content) and the new node.</returns>
    /// <exception cref="NullContentReferenceException"></exception>
    public ((int, T), BPlusTreeNode<T>) Split(long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.Split, ID, -1, [], [], 0, -1, [], []));
      int dividerIndex = _NumKeys / 2;
      int[] newKeys = new int[_Degree];
      T[] newContent = new T[_Degree];
      (int, T) dividerEntry = (_Keys[dividerIndex], _Contents[dividerIndex]
      ?? throw new NullContentReferenceException(
      $"Content at index:{NumKeys} within node:{ID}"));

      for (int i = 0; i < _NumKeys - dividerIndex; i++)
      {
        newKeys[i] = _Keys[i + dividerIndex];
        newContent[i] = _Contents[i + dividerIndex]
        ?? throw new NullContentReferenceException(
        $"Content at index:{i + _Degree} within node:{ID}");
        _Keys[i + dividerIndex] = default;
        _Contents[i + dividerIndex] = default;
      }

      BPlusLeafNode<T> newNode = new(_Degree, newKeys, newContent, _BufferBlock)
      {
        _NextNode = _NextNode,
        _NumKeys = _NumKeys - dividerIndex
      };
      _NumKeys = dividerIndex;
      _NextNode = newNode;
      newNode._PrevNode = this;
      if (newNode._NextNode != null)
      {
        newNode._NextNode._PrevNode = newNode;
      }
      (int, int[], T?[]) temp = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.SplitResult, ID, temp.Item1,
        temp.Item2, temp.Item3, parentID, -1, [], []));
      (int, int[], T?[]) temp2 = newNode.CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.SplitResult, newNode.ID,
        temp2.Item1, temp2.Item2, temp2.Item3, parentID, -1, [], []));
      return (dividerEntry, newNode);
    }


    /// <summary>
    /// Removes a key and the corresponding data from the leafnode, propagates the changes upward
    /// </summary>
    /// <param name="key"></param>
    /// <param name="pathStack"></param>
    public override void DeleteKey(int key, Stack<Tuple<BPlusNonLeafNode<T>, int>> pathStack)
    {
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID, -1, [], [], 0, -1, [], []));
      int i = Search(key);
      if (i != -1)
      {
        for (; i < _NumKeys; i++)
        {
          _Keys[i] = _Keys[i + 1];
          _Contents[i] = _Contents[i + 1];
        }
        _NumKeys--;
        _Keys[_NumKeys] = default;
        _Contents[_NumKeys] = default;
        (int, int[], T?[]) temp = CreateBufferVar();
        _BufferBlock.SendAsync((NodeStatus.Deleted, ID, temp.Item1, temp.Item2, temp.Item3, 0, -1, [], []));

        PropagateChanges(pathStack);
      }
      //send status to say key not found
      _BufferBlock.SendAsync((NodeStatus.Deleted, ID, -1, [], [], 0, -1, [], []));
    }

    /// <summary>
    /// This method handles the cases where the leafnode is underflow (has too few keys)
    /// If the leafnode is the rootnode itself and is underflow (for a root), it calls deletenode
    /// If it is a non-root leafnode, it checks for underflow and handles cases such as redistributing key values
    /// (if it has at least one sibling with a key to spare), otherwise it calls merge
    /// </summary>
    /// <param name="pathStack">contains the nodes on the upper levels that were taken to get to this node</param>
    public void PropagateChanges(Stack<Tuple<BPlusNonLeafNode<T>, int>> pathStack)
    {
      if (pathStack.Count == 0)
      {//if stack is 0, means we are in the root
        if (isUnderflowAsRoot())
        {//means there are no keys left because root has to have at least 1 key/entry
          DeleteNode(null, -1);
          //statusupdate that this node got deleted
        }
      }
      else
      {
        Tuple<BPlusNonLeafNode<T>, int> parent = pathStack.Pop();
        BPlusNonLeafNode<T> parentNode = parent.Item1;
        int index = parent.Item2;

        (BPlusLeafNode<T>?, int) leftSibling = GetLeftSibling(index, parentNode);
        (BPlusLeafNode<T>?, int) rightSibling = GetRightSibling(index, parentNode);
        BPlusLeafNode<T>? leftSiblingNode = leftSibling.Item1;
        BPlusLeafNode<T>? rightSiblingNode = rightSibling.Item1;

        bool isUnderflow = IsUnderflow();
        if (parentNode != null)
        {
          if (!isUnderflow)
          {
            // do nothing
          }
          else if (isUnderflow && leftSiblingNode != null && leftSiblingNode.CanRedistribute())
          {
            GainsFromLeft(leftSiblingNode);
            leftSiblingNode.LosesToRight();

            //send statusupdate redistribution of keys
            (int, int[], T?[]) nodeContent = CreateBufferVar();
            (int, int[], T?[]) siblingContent = leftSiblingNode.CreateBufferVar();
            _BufferBlock.SendAsync((NodeStatus.UnderFlow, ID,nodeContent.Item1, nodeContent.Item2, nodeContent.Item3,
            leftSiblingNode.ID, siblingContent.Item1, siblingContent.Item2, siblingContent.Item3));
          }
          else if (isUnderflow && rightSiblingNode != null && rightSiblingNode.CanRedistribute())
          { 
            GainsFromRight(rightSiblingNode);
            rightSiblingNode.LosesToLeft();

            //send statusupdate redistribution of keys
            (int, int[], T?[]) nodeContent = CreateBufferVar();
            (int, int[], T?[]) siblingContent = rightSiblingNode.CreateBufferVar();
            _BufferBlock.SendAsync((NodeStatus.UnderFlow, ID,nodeContent.Item1, nodeContent.Item2, nodeContent.Item3,
            rightSiblingNode.ID, siblingContent.Item1, siblingContent.Item2, siblingContent.Item3));

          }
          else
          {
            if (rightSiblingNode != null && rightSibling.Item1 != null)
            {
              (BPlusLeafNode<T>, int) rightSiblingNotNull = (rightSibling.Item1, rightSibling.Item2);
              long siblingID = rightSiblingNode.ID;
              mergeWithRight(rightSiblingNotNull, parentNode);

              //send statusupdate
              (int, int[], T?[]) temp = CreateBufferVar();
              _BufferBlock.SendAsync((NodeStatus.Merge,ID, temp.Item1, temp.Item2, temp.Item3, siblingID,-1, [],[]));
              //Console.WriteLine("Merging with right");
            }
            else if (leftSiblingNode != null && leftSibling.Item1 != null)
            {
              
              (BPlusLeafNode<T>, int) leftSiblingNotNull = (leftSibling.Item1, leftSibling.Item2);
              long siblingID = leftSiblingNode.ID;
              mergeWithLeft(leftSiblingNotNull, parentNode);

              //send statusupdate
              (int, int[], T?[]) temp = CreateBufferVar();
              _BufferBlock.SendAsync((NodeStatus.Merge,ID, temp.Item1, temp.Item2, temp.Item3, siblingID,-1, [],[]));
              //Console.WriteLine("Merging with left");
            }
          }
          //communicate the changes one level upwards
          parent.Item1.PropagateChanges(pathStack);
        }
      }
    }

    /// <summary>
    /// Returns a node's left sibling, if it does not have one, returns null
    /// </summary>
    /// <param name="index">Index of the node</param>
    /// <param name="parent">Node's parent</param>
    /// <returns>Node's left sibling (if present)</returns>
    public (BPlusLeafNode<T>?, int) GetLeftSibling(int index, BPlusNonLeafNode<T> parent)
    {
      if (parent != null)
      {
        if (index > 0 && parent.Children[index - 1] is BPlusLeafNode<T> leftSibling)
        {
          return (leftSibling, index - 1);
        }
      }
      return (null, -1);
    }
    /// <summary>
    /// Returns a node's right sibling, if it does not have one, returns null
    /// </summary>
    /// <param name="index">Index of the node</param>
    /// <param name="parent">Node's parent</param>
    /// <returns>Node's right sibling (if present)</returns>
    public (BPlusLeafNode<T>?, int) GetRightSibling(int index, BPlusNonLeafNode<T> parent)
    {
      if (parent != null)
      {
        if (index >= 0 && index < parent.Children.Count() - 1 && parent.Children[index + 1] is BPlusLeafNode<T> rightSibling)
        {
          return (rightSibling, index + 1);
        }
      }
      return (null, -1);
    }

    /// <summary>
    /// Checks if a node has a key to spare
    /// </summary>
    /// <returns></returns>
    public bool CanRedistribute()
    //checks if the node has more than the minimum amount of keys/entries (can spare one)
    {
      int minKeys = (int)Math.Ceiling((double)_Degree / 2) - 1;
      return _NumKeys >= minKeys + 1;
    }

    /// <summary>
    /// Deletes the node and all its connections (from parent's children, from doubly linked list, etc...)
    /// </summary>
    /// <param name="parentNode"></param>
    /// <param name="indexOfChildBeingDeleted"></param>
    public void DeleteNode(BPlusNonLeafNode<T>? parentNode, int indexOfChildBeingDeleted)
    {
      if (this == null)
      {
        return;
      }
      if (parentNode != null)
      {
        if (indexOfChildBeingDeleted != -1)
        {

          parentNode.Children[indexOfChildBeingDeleted] = default;
          for (; indexOfChildBeingDeleted < _Degree;)
          {
            parentNode.Children[indexOfChildBeingDeleted] = parentNode.Children[indexOfChildBeingDeleted + 1];
            indexOfChildBeingDeleted++;
          }
        }
        if (_NextNode != null)
        {
          _NextNode._PrevNode = _PrevNode;
          _NextNode = null;
        }
        if (_PrevNode != null)
        {
          _PrevNode._NextNode = _NextNode;
          _PrevNode = null;
        }
        for (int i = 0; i < _NumKeys; i++)
        {
          _Keys[i] = default;
          _Contents[i] = default;
        }
        _NumKeys = default;
      }
      else
      {
        //
        return;
      }

    }


    /// <summary>
    /// Merges this node with its right sibling, all entries are combined, and the sibling node gets deleted
    /// </summary>
    /// <param name="sibling"></param>
    /// <param name="parent"></param>
    public void mergeWithRight((BPlusLeafNode<T>, int) sibling, BPlusNonLeafNode<T> parent)
    {
      BPlusLeafNode<T> siblingNode = sibling.Item1;
      int siblingIndex = sibling.Item2;
      for (int i = 0; i < siblingNode.NumKeys; i++)
      {
        _Keys[_NumKeys + i] = siblingNode.Keys[i];
        _Contents[_NumKeys + i] = siblingNode.Contents[i]; //insert all values from sibling Node into this

      }
      _NumKeys += siblingNode.NumKeys;
      siblingNode.DeleteNode(parent, siblingIndex);//use parent to pass down to delete() to properly delete

    }

    /// <summary>
    /// Merges this node with its left sibling and calls delete on the sibling
    /// </summary>
    /// <param name="sibling">Node's left sibling</param>
    /// <param name="parentNode">Node's parent</param>
    public void mergeWithLeft((BPlusLeafNode<T>, int) sibling, BPlusNonLeafNode<T> parentNode)
    {
      BPlusLeafNode<T> siblingNode = sibling.Item1;
      int siblingIndex = sibling.Item2;
      int numKeysInSibling = siblingNode.NumKeys;
      for (int i = 0; i < numKeysInSibling; i++)
      {
        GainsFromLeft(siblingNode);
        siblingNode.LosesToRight();
        //insert all values from sibling Node into this
      }
      //_NumKeys += siblingNode.NumKeys;
      siblingNode.DeleteNode(parentNode, siblingIndex);
      //use parent to pass down to delete() to properly delete

    }

    /// <summary>
    /// Gains a key and an entry from the right sibling
    /// </summary>
    /// <param name="sibling"></param>
    public void GainsFromRight(BPlusLeafNode<T> sibling)
    {
      _Keys[_NumKeys] = sibling.Keys[0];
      _Contents[_NumKeys] = sibling.Contents[0];
      _NumKeys++;
    }
    /// <summary>
    /// Loses a key and an entry to the left sibling
    /// </summary>
    public override void LosesToLeft()
    {
      for (int i = 0; i < _NumKeys - 1; i++)
      {
        _Keys[i] = _Keys[i + 1];
        _Contents[i] = _Contents[i + 1];
      }
      _NumKeys--;
      _Keys[_NumKeys] = default;
      _Contents[_NumKeys] = default;
    }
    /// <summary>
    /// gains a key and an entry to the left sibling
    /// </summary>
    /// <param name="sibling"></param>
    public void GainsFromLeft(BPlusLeafNode<T> sibling)
    {
      for (int i = _NumKeys - 1; i >= 0; i--)
      {
        _Keys[i + 1] = _Keys[i];
        _Contents[i + 1] = _Contents[i];
      }
      _NumKeys++;
      _Keys[0] = sibling.Keys[sibling.NumKeys - 1];
      _Contents[0] = sibling.Contents[sibling.NumKeys - 1];
    }

    /// <summary>
    /// Loses a key and an entry to the right sibling
    /// </summary>
    public override void LosesToRight()
    {
      _NumKeys--;
      _Keys[_NumKeys] = default;
      _Contents[_NumKeys] = default;
    }

    /// <summary>
    /// checks if a node has too few keys/entries
    /// </summary>
    /// <returns></returns>
    public override bool IsUnderflow()
    {
      int minKeys = (int)Math.Ceiling((double)_Degree / 2) - 1;
      return _NumKeys < minKeys;
    }

    /// <summary>
    /// checks if a node has too few entries to be a root
    /// if a leafnode is the root, it has to have at least one entry
    /// </summary>
    /// <returns></returns>
    public bool isUnderflowAsRoot()
    {
      int minKeys = 1;
      return _NumKeys >= minKeys;
    }

    public string toString()
    {
      string output = "";
      for (int i = 0; i < NumKeys; i++)
      {
        output += (Contents[0] ?? throw new NullContentReferenceException(
        $"Content at index:{i + _Degree} within node:{ID}")).ToString() + " ";
      }
      return output;
    }

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

    /// <summary>
    /// Prints out the contents of the node in JSON format.
    /// </summary>
    /// <remarks>Author: Tristan Anderson, modified by Andreas Kramer
    /// <param name="x">Hierachical Node ID</param>
    /// <returns>String with the entirety of this node's keys and contents arrays formmatted in JSON syntax.</returns>
    public override string Traverse(string x)
    {
      string output = Spacer(x) + "{\n";
      output += Spacer(x) + "  \"leafnode\":\"" + x + "\",\n"
      + Spacer(x) + "\"  ID\":" + _ID + ",\n"
      //+ Spacer(x) + "\" Prev\":" + _PrevNode.ID           
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

      output += "]\n";
      if (_NextNode != null)
      {
        output += Spacer(x) + "\"  next\":" + _NextNode._ID + ",\n";
      }
      if (_PrevNode != null)
      {
        output += Spacer(x) + "\"  prev\":" + _PrevNode._ID + ",\n";
      }
      return output + Spacer(x) + "}";
    }

  }
}