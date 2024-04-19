/*
Author: Andreas Kramer
Secondary Author: Emily Elzinga
Remark: contains code from BTree implementation (Tristan Anderson and others)
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
    public override ((int,T?), BPlusTreeNode<T>?) InsertKey(int key, T data, long parentID)
    {
      _BufferBlock.SendAsync((NodeStatus.ISearching,ID,-1,[],[],0,-1,[],[]));
      ((int,T?),BPlusTreeNode<T>?) result;
      int i = 0;
      while(i < _NumKeys && key > _Keys[i]){
        i++;
      }
        result = (Children[i]?? throw new NullChildReferenceException(
          $"Child at index:{i} within node:{ID}")).InsertKey(key,data,ID);
        if(result.Item2 != null){
          for (int j = _NumKeys - 1; j >= i; j--)
          {
            _Keys[j + 1] = _Keys[j];
            _Children[j + 2] = _Children[j + 1];
          }
          _Keys[i] = result.Item1.Item1;
          _Children[i + 1] = result.Item2;
          _NumKeys++;
          (int, int[]) temp = CreateBufferVar();
          _BufferBlock.SendAsync((NodeStatus.SplitInsert, ID, temp.Item1, temp.Item2, [], 0, -1, [], []));
          if (IsFull())
          {
            return Split(parentID);
          }
        }
      return ((-1, default(T)),null);
    }

    /// <summary>
    /// Splits _Keys[] of this node giving up the greater half to a new node.
    /// </summary>
    /// <returns>The new node created from the split and the dividing key with
    /// corresponding content as ((dividing Key, Content), new Node).</returns>
    public ((int,T?), BPlusTreeNode<T>) Split(long parentID)
    { 
      _BufferBlock.SendAsync((NodeStatus.Split, ID, -1, [], [], 0, -1, [], []));
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
        _BufferBlock.SendAsync((NodeStatus.Shift, ID , -1, [], [], newChildren[i-1].ID, -1, [], []));
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
      (int, int[]) temp = CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.SplitResult, ID, temp.Item1,
        temp.Item2, [], parentID, -1, [], []));

      (int, int[]) temp2 = newNode.CreateBufferVar();
      _BufferBlock.SendAsync((NodeStatus.SplitResult, ID, temp2.Item1,
        temp2.Item2, [], parentID, -1, [], []));
      return (dividerEntry, newNode);
    }

    /// <summary>
    /// Traverses down the tree and keeps track of the path it took until it hits a leafnode
    /// For further reference see its implementation in BPlusLeafNode.cs
    /// </summary>
    /// <param name="key">the key looking to be deleted</param>
    /// <param name="pathStack">Stack storing the nodes visited and the indeces used 
    /// to further traverse the tree</param>
    /// <exception cref="NullChildReferenceException"></exception>
		public override void DeleteKey(int key, Stack<Tuple<BPlusNonLeafNode<T>,int>> pathStack)
    {
      int index = Search(key);
      _BufferBlock.SendAsync((NodeStatus.DSearching, ID, -1, [], [], 0, -1, [], []));
      if(index >= 0 && index < _Children.Count()){
        Tuple<BPlusNonLeafNode<T>,int> tuple = new Tuple<BPlusNonLeafNode<T>,int>(this,index);
        pathStack.Push(tuple);
        (_Children[index]?? throw new NullChildReferenceException(
          $"Child at index:{index} within node:{ID}")).DeleteKey(key, pathStack);
      }
      else{
        //do nothing
      }  
    }

    /// <summary>
    /// Deletes this node, sets all its values to default and disconnects it from doubly linked list
    /// as well as its parent
    /// </summary>
    /// <param name="parentNode"></param>
    /// <param name="indexOfNodeBeingDeleted"></param>
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

    /// <summary>
    /// This method handles the changes that a deletion of an entry may bring
    /// It updates its key values and checks if it is underflow
    /// If it is underflow as the root it deletes itself
    /// If it is underflow as a non-root, it checks sibling(s) for redistributing children, if the sibling(s) cannot
    /// forfeit a child, it calls merge with the right or left sibling
    /// Finally it updates the key values again and propagates the changes upwards
    /// </summary>
    /// <param name="pathStack"></param>
    public void PropagateChanges(Stack<Tuple<BPlusNonLeafNode<T>,int>> pathStack){
        if(pathStack.Count == 0){//if stack is empty, means we are in the root -> no parent
          _NumKeys = GetNumberOfChildren() - 1;//switched with line below
          UpdateKeyValues();
          
          (int, int[]) temp5 = CreateBufferVar(); 
          //_BufferBlock.SendAsync((NodeStatus.UpdateKeyValues,ID,temp5.Item1,temp5.Item2,[],-1,-1,[],[]));
          bool isRootUnderflow = IsRootUnderflow();
          if(isRootUnderflow){
            //merge root status update
            DeleteNode(null,-1);
          }  
        }
        else{//means stack is not empty and we are in non-root node
          Tuple<BPlusNonLeafNode<T>,int> nextItemUpward = pathStack.Pop();
          BPlusNonLeafNode<T> parentNode = nextItemUpward.Item1;
          int selfIndex = nextItemUpward.Item2;
          BPlusNonLeafNode<T>? leftSibling = FindLeftSibling(selfIndex,parentNode);
          BPlusNonLeafNode<T>? rightSibling = FindRightSibling(selfIndex,parentNode);
          _NumKeys = GetNumberOfChildren() - 1;//switched with line below
          UpdateKeyValues();
          
          //new status update: updated key values
          (int, int[]) temp = CreateBufferVar();
          //_BufferBlock.SendAsync((NodeStatus.UpdateKeyValues,ID,temp.Item1,temp.Item2,[],-1,-1,[],[]));
          bool isUnderflow = IsUnderflow();

          if(!isUnderflow){
            //do nothing, keep propagating upwards
          }
          else if(isUnderflow && leftSibling != null && leftSibling.CanForfeit()){
            //if it is underflow, check sibling(s) for forfeiting a child
            //check if this is the correct status update
            _BufferBlock.SendAsync((NodeStatus.Shift,ID,-1,[],[],(leftSibling.Children[leftSibling._NumKeys] 
            ?? throw new NullChildReferenceException($"Child at index 0 in node:{leftSibling.ID}")).ID,
            -1,[],[]));

            GainsFromLeft(leftSibling);
            leftSibling.LosesToRight();
            
          }
          else if(isUnderflow && rightSibling != null && rightSibling.CanForfeit()){
            //check if this is the correct status update (underflow?)
            _BufferBlock.SendAsync((NodeStatus.Shift,ID,-1,[],[],(rightSibling.Children[0] 
            ?? throw new NullChildReferenceException($"Child at index 0 in node:{rightSibling.ID}")).ID,
            -1,[],[]));

            GainsFromRight(rightSibling);
            rightSibling.LosesToLeft();
          }
          else{//if sibling(s) cannot forfeit because are at min -> mergewith respective child
            if(rightSibling != null){
              long rightSiblingID = rightSibling.ID;
              mergeWithRight(rightSibling);
              rightSibling.DeleteNode(parentNode,selfIndex + 1); //we can use + 1 because we know there is a rightsibling
              (int, int[]) temp3 = CreateBufferVar();
              _BufferBlock.SendAsync((NodeStatus.Merge,ID, temp3.Item1, temp3.Item2, [], rightSiblingID,-1, [],[]));

            }
            else if(leftSibling != null){
              long leftSiblingID = leftSibling.ID;
              mergeWithLeft(leftSibling);
              leftSibling.DeleteNode(parentNode,selfIndex - 1); //we can use -1 because we know there is a leftsibling
              
              (int, int[]) temp2 = CreateBufferVar();
              _BufferBlock.SendAsync((NodeStatus.Merge,ID, temp2.Item1, temp2.Item2, [], leftSiblingID,-1, [],[]));
            }
          }
          UpdateKeyValues();
          (int, int[]) temp4 = CreateBufferVar();
          //_BufferBlock.SendAsync((NodeStatus.UpdateKeyValues,ID,temp4.Item1,temp4.Item2,[],-1,-1,[],[]));
  
          parentNode.PropagateChanges(pathStack);

        }      

    }
    /// <summary>
    /// Traverses through the (sub)tree and updates the nonleaf key values
    /// </summary>
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
      }
    }

    /// <summary>
    /// Helper method to traverse through the tree, grabs the leftmost child of the subtree
    /// </summary>
    /// <param name="index">index of subtree to take (child to go into)</param>
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
    /// <summary>
    /// Grabs the leftmostkey of the subtree
    /// </summary>
    /// <returns>its key value as int</returns>
    public int GetLeftmostKeyofSubTree()
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

    /// <summary>
    /// Gets the number of children in a nonleafnode
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// Checks if the root is underflow and would need to be deleted
    /// Root needs to have at least 1 key and 2 children
    /// </summary>
    /// <returns></returns>
    public bool IsRootUnderflow(){
      if(_NumKeys < 1 || GetNumberOfChildren() < 2){
        return true;
      }
      return false;
    }
    /// <summary>
    /// Non-root-node is underflow if it has < m/2 -1 keys or < m/2 children
    /// </summary>
    /// <returns></returns>
    public override bool IsUnderflow(){
      if(_NumKeys < (int)Math.Ceiling((double)_Degree/2 -1) || GetNumberOfChildren() < (int)Math.Ceiling((double)_Degree/2)){
        return true;
      }
      return false;
    }
    /// <summary>
    /// Checks if node has a child to spare
    /// </summary>
    /// <returns></returns>
    public bool CanForfeit(){
      if(GetNumberOfChildren() > (int)Math.Ceiling((double)_Degree/2)){
        return true;
      }
      return false;
    }

    /// <summary>
    /// Returns the index of a subtree
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int FindChildIndex(int key){
      return Search(key);
    }
    /// <summary>
    /// Finds the left sibling
    /// </summary>
    /// <param name="selfIndex"></param>
    /// <param name="parentNode"></param>
    /// <returns></returns>
    public BPlusNonLeafNode<T>? FindLeftSibling(int selfIndex, BPlusNonLeafNode<T> parentNode){
      if(parentNode != null){
        if(selfIndex > 0 && parentNode.Children[selfIndex - 1] is BPlusNonLeafNode<T> leftSibling){
          return leftSibling;
        }       
      }
      return null;
    }
    /// <summary>
    /// Finds the right sibling
    /// </summary>
    /// <param name="selfIndex">Index of node which's sibling we are looking for</param>
    /// <param name="parentNode"></param>
    /// <returns></returns>
    public BPlusNonLeafNode<T>? FindRightSibling(int selfIndex, BPlusNonLeafNode<T> parentNode){
      if(parentNode != null){
        if(selfIndex >= 0 && selfIndex < parentNode.Children.Count() - 1 && parentNode.Children[selfIndex + 1] is BPlusNonLeafNode<T> leftSibling){
          return leftSibling;
        }       
      }
      return null;
    }
    /// <summary>
    /// Gains a child from the right
    /// </summary>
    /// <param name="sibling"></param>
    public void GainsFromRight(BPlusNonLeafNode<T> sibling)
    {
      _Children[_NumKeys + 1] = sibling.Children[0];
      _NumKeys++;
    }
    /// <summary>
    /// Loses a child to the left
    /// </summary>
    public override void LosesToLeft()
    {     
      for (int i = 0; i < _NumKeys; i++)
      {
        _Keys[i] = _Keys[i + 1];
        _Children[i] = _Children[i + 1];
      }
      _Children[_NumKeys] = _Children[_NumKeys + 1];
      _Children[_NumKeys + 1] = default;
      if(_NumKeys > 0){
        _NumKeys--;
      }
      _Keys[_NumKeys] = default;
      
    }
    /// <summary>
    /// Gains a child from the left
    /// </summary>
    /// <param name="sibling"></param>
    public void GainsFromLeft(BPlusNonLeafNode<T> sibling){
      
      _Children[NumKeys + 1] = _Children[_NumKeys];
      for (int i = _NumKeys; i > 0; i--) 
      {
        //_Keys[i] = _Keys[i - 1];
        _Children[i] = _Children[i-1];
      }
      _NumKeys++;
      //_Keys[0] = sibling.Keys[sibling.NumKeys - 1];
      _Children[0] = sibling.Children[sibling.NumKeys];

    }
    /// <summary>
    /// Loses a child to the right
    /// </summary>
    public override void LosesToRight()
    {
      _Children[_NumKeys] = default;
      _Keys[_NumKeys]= default;
      _NumKeys--;
    }
    /// <summary>
    /// Merges this node with its left sibling
    /// </summary>
    /// <param name="sibling"></param>
    public void mergeWithLeft(BPlusNonLeafNode<T> sibling)
    {
      int numChildren = sibling.GetNumberOfChildren();
      
      for(int i = 0; i < numChildren; i++){
        GainsFromLeft(sibling);
        sibling.LosesToRight();    
        //needs to delete the sibling -> handled in PropagateChanges()
      }
      _BufferBlock.SendAsync((NodeStatus.Merge, ID, NumKeys, Keys, [], sibling.ID, -1, [], []));
    }
    /// <summary>
    /// Merges this node with its right sibling
    /// </summary>
    /// <param name="sibling"></param>
    public void mergeWithRight(BPlusNonLeafNode<T> sibling)
    {
      int numChildrenOfSibling = sibling.GetNumberOfChildren();
      _NumKeys = GetNumberOfChildren() - 1;
      
      for(int i = 0; i < numChildrenOfSibling; i++){
        GainsFromRight(sibling);
        sibling.LosesToLeft();  
        //needs to delete the sibling -> handled in PropagateChanges()
      }
      _BufferBlock.SendAsync((NodeStatus.Merge, ID, NumKeys, Keys, [], sibling.ID, -1, [], []));
    }
    public (int, int[]) CreateBufferVar()
    {
      int numKeys = NumKeys;
      int[] keys = new int[_Keys.Length];
      for (int i = 0; i < _Keys.Length; i++)
      {
        keys[i] = Keys[i];
      }
      return (numKeys, keys);
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
