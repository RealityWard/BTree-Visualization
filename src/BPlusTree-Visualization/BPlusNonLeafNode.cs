/*
Author: Andreas Kramer (using BTree structure from Tristan Anderson)
Date: 03/04/2024
Desc: Describes functionality for non-leaf nodes on the B+Tree. Recursive function iteration due to children nodes.
*/
using System.Threading.Tasks.Dataflow;
using System.Text.RegularExpressions;
using ThreadCommunication;


namespace BPlusTreeVisualization
{
  /// <summary>
  /// Creates a non-leaf node for a B+Tree data structure with
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
    /// Creates a non-leaf node for a B+Tree data structure
    /// with starting values of the passed arrays of keys,
    /// and children. Sets the NumKeys to the length of keys[]. (Numkeys  gets set separately when non-leaf node gets created)
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
    /// Finds the index of the key within the node (which subtree to go into)
    /// </summary>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>Returns the index of the subtree to go into</returns>
    
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
    /// Searches for a specific key, gets the right index and checks its children
    /// recursively traverses down the tree and checks its children, eventually ends up in leafnodes and returns found index and its node
    /// </summary>
    /// <param name="key">Integer to find in _Keys[] of this node.</param>
    /// <returns>If found returns index, and its node, if not returns -1 and this.</returns>
 
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
    /// ((dividing Key, null), new Node) to itself. 
    /// Afterwards calls split if full.
    /// </summary>
    /// <remarks>Copied and modified from
    /// LeafNode.InsertKey()</remarks>
    /// <param name="key">Integer to be placed into _Keys[] of this node.</param>
    /// <param name="data">Corresponding data to be stored in _Contents[] of leaf nodes
    /// at the same index as key in _Keys[].</param>
    /// <returns>If this node reaches capacity it calls split and returns
    /// the new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).
    /// <remarks>as non-leaf nodes do not contain contents it will return ((dividing Key, null), new Node).</remarks>
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
 
		public override void DeleteKey(int key, Stack<Tuple<BPlusNonLeafNode<T>,int>> pathStack)
    {
      int index = Search(key);
      if(index >= 0 && index < _Children.Count()){
        Tuple<BPlusNonLeafNode<T>,int> tuple = new Tuple<BPlusNonLeafNode<T>,int>(this,index);
        pathStack.Push(tuple);
        (_Children[index]?? throw new NullChildReferenceException(
          $"Child at index:{index} within node:{ID}")).DeleteKey(key, pathStack);
      }
      else{
        //not found
      }
      
    }

    public void DeleteNode(BPlusNonLeafNode<T>? parentNode, int indexOfNodeBeingDeleted){
      if(this == null){
        return;
      }
      for(int i = 0; i < _NumKeys;i++){
        _Keys[i] = default;
      }
      _NumKeys = default;
      if(parentNode != null){
        if(indexOfNodeBeingDeleted != -1){
          parentNode.Children[indexOfNodeBeingDeleted] = default;
          for (; indexOfNodeBeingDeleted < _Degree;)
          {
              parentNode.Children[indexOfNodeBeingDeleted] = parentNode.Children[indexOfNodeBeingDeleted + 1];
              indexOfNodeBeingDeleted++;
          }
        }
      }
    }

    public void PropagateChanges(Stack<Tuple<BPlusNonLeafNode<T>,int>> pathStack){
        if(pathStack.Count == 0){//if stack is empty, means we are in the root -> no parent
          UpdateKeyValues();
          bool isRootUnderflow = IsRootUnderflow();
          if(isRootUnderflow){
            DeleteNode(null,-1);
            //we still need to make its child the new root -> might already been taken care of, further testing for confirmation
          }  
        }
        else{//means stack is not empty and we are in non-root node
          Tuple<BPlusNonLeafNode<T>,int> nextItemUpward = pathStack.Pop();
          BPlusNonLeafNode<T> parentNode = nextItemUpward.Item1;
          int selfIndex = nextItemUpward.Item2;
          BPlusNonLeafNode<T>? leftSibling = FindLeftSibling(selfIndex,parentNode);
          BPlusNonLeafNode<T>? rightSibling = FindRightSibling(selfIndex,parentNode);
          UpdateKeyValues();
          bool isUnderflow = IsUnderflow();
          if(!isUnderflow){
            //do nothing, keep propagating upwards
          }
          else if(isUnderflow && leftSibling != null && leftSibling.CanForfeit()){
            //if it is underflow, check sibling(s) for forfeiting a child
            //if sibling(s) cannot forfeit because are at min -> mergewith respective child
            //AddChildFromLeft();
            //leftSibling.ForfeitChildToRight();

          }
          else if(isUnderflow && rightSibling != null && rightSibling.CanForfeit()){
            //AddChildFromRight()
            //rightSibling.ForfeitChildToLeft();
          }
          else{
            if(rightSibling != null){
              //mergeWith(rightSibiling);
            }
            else if(leftSibling != null){
              //mergeWith(leftSibling);
            }
          }
          UpdateKeyValues();
          
          parentNode.PropagateChanges(pathStack);

        }      
        //updateValues, etc...

    }

    public void UpdateKeyValues(){
      for(int i = 0; i < NumKeys;i++){
        _Keys[i] = default;
      }
      for(int i = 1; i < NumKeys + 1; i++){
        if(_Children[i] != null){
          UpdateKeyValuesHelper(i);
        }
        else{
          return;
        }
        //loop should only iterate as many times as there are children - 1
      }
    }

    public void UpdateKeyValuesHelper(int index){
      if(_Children[index] is BPlusLeafNode<T> leaf && leaf != null){
        if(leaf.NumKeys != 0){
          _Keys[index - 1] = leaf.Keys[0];
        }
      }
      else if(_Children[index] is BPlusNonLeafNode<T> nonLeaf){
        int keyToAdd = nonLeaf.GetLeftmostKeyofSubTree();
        _Keys[index - 1] = keyToAdd;
      }
    }

    public int GetLeftmostKeyofSubTree()
            //this method returns the leftmostKeyofaSubtree
        {
            //checking if child is a leaf, if it is, takes the leftmost leaf's leftmost key
            if (_Children[0] is BPlusLeafNode<T> leaf && leaf != null)
            {
                return leaf.Keys[0];
            }
            //otherwise traversing down the subtree
            else if(_Children[0] is BPlusNonLeafNode<T> nonleaf)
            {
                return nonleaf.GetLeftmostKeyofSubTree();
            }
            else { return -1; }
        }

    public int GetNumberOfChildren(){
      int count = 0;
      for(; count < _Children.Count();){
        if(_Children[count] == null){
          return count;
        }
        count++;
      }
      return -1;
    }
    /*
    public void RemoveChildAtIndex(int index){
      for(int i = 0; i < _Children.Count();i++){

      }
    }
    */

    public bool IsRootUnderflow(){
      //checks if the root is underflow and would need to be deleted
      //root needs to have at least 1 key and 2 children
      if(_NumKeys < 1 || GetNumberOfChildren() < 2){
        return true;
      }
      return false;
    }

    public bool IsUnderflow(){
      //non-root-node is underflow if it has < m/2 -1 keys or < m/2 children
      if(_NumKeys < _Degree/2 -1 || GetNumberOfChildren() < _Degree/2){
        return true;
      }
      return false;
    }

    public bool CanForfeit(){
      if(GetNumberOfChildren() >= _Degree/2 + 1){
        return true;
      }
      return false;
    }

    public int FindChildIndex(int key){
      return Search(key);
    }

    public BPlusNonLeafNode<T>? FindLeftSibling(int selfIndex, BPlusNonLeafNode<T> parentNode){
      if(parentNode != null){
        if(selfIndex > 0 && parentNode.Children[selfIndex - 1] is BPlusNonLeafNode<T> leftSibling){
          return leftSibling;
        }       
      }
      return null;
    }

    public BPlusNonLeafNode<T>? FindRightSibling(int selfIndex, BPlusNonLeafNode<T> parentNode){
      if(parentNode != null){
        if(selfIndex >= 0 && selfIndex < parentNode.Children.Count() - 1 && parentNode.Children[selfIndex + 1] is BPlusNonLeafNode<T> leftSibling){
          return leftSibling;
        }       
      }
      return null;
    }



    /// <summary>
    /// Checks the child at index for underflow. If so it then checks for _Degree 
    /// number of children in the right child of the key. _Degree or greater means 
    /// either overflow or split. 
    /// </summary>
    /// <remarks>Author: Tristan Anderson,
    /// Date: 2024-02-18</remarks>
    /// <param name="index">Index of affected child node.</param>
    /*
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
            _BufferBlock.SendAsync((NodeStatus.UnderFlow, Children[index].ID, Children[index].NumKeys,
              Children[index].Keys, Children[index].Contents, Children[index + 1].ID,
              Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents));
            if (_Children[index] as BPlusNonLeafNode<T> != null)
            {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
              _BufferBlock.SendAsync((NodeStatus.Shift, (((BPlusNonLeafNode<T>)Children[index])
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
            _BufferBlock.SendAsync((NodeStatus.UnderFlow, Children[index + 1].ID,
              Children[index + 1].NumKeys, Children[index + 1].Keys, Children[index + 1].Contents,
              Children[index].ID, Children[index].NumKeys,
              Children[index].Keys, Children[index].Contents));
            if (_Children[index] as BPlusNonLeafNode<T> != null)
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
              _BufferBlock.SendAsync((NodeStatus.Shift, ((BPlusNonLeafNode<T>)Children[index + 1])
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
    /*

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
    */
    
    /*

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
    /// <remarks>Author: Tristan Anderson, modified by Andreas Kramer
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
