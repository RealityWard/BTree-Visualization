/*
Author: Andreas Kramer (using BTree structure from Tristan Anderson)
Date: 03/04/2024
Desc: Describes functionality for non-leaf nodes on the B+Tree. Recursive function iteration due to children nodes.
*/

using System.Net;
using System.Threading.Tasks.Dataflow;
using BTreeVisualization;
using ThreadCommunication;



namespace BPlusTreeVisualization{

    /// <summary>
    /// Creates a leaf node for a B+Tree data structure with
    /// empty arrays of keys and contents as well as references to a next node and a previous node forming a doubly linked list
    /// </summary>
    /// <remarks>Author: Andreas Kramer</remarks>
    /// <typeparam name="T">Data type of the content to be stored under key.</typeparam>
    /// <param name="degree">Same as parent non-leaf node/tree</param>
    /// <param name="bufferBlock">Output Buffer for Status updates to
    /// be externally viewed.</param>

    public  class BPlusLeafNode<T>(int degree, BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys, 
            T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)    
            : BPlusTreeNode<T>(degree, bufferBlock)
    {
        private BPlusLeafNode<T>? _NextNode;

        private BPlusLeafNode<T>? _PrevNode;

        protected T?[] _Contents = new T[degree];

        public T?[] Contents{
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

        private int Search(int key){
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

        public override (int, BPlusTreeNode<T>) SearchKey(int key){
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

        public override ((int, T?), BPlusTreeNode<T>?) InsertKey(int key, T data){
            _BufferBlock.SendAsync((NodeStatus.ISearching, ID, -1, [], [], 0, -1, [], []));
            int i = 0;
            while (i < _NumKeys && key >= _Keys[i])
                i++;
            if (i == _NumKeys || key != _Keys[i] || key == 0){
                for (int j = _NumKeys - 1; j >= i; j--){
                    _Keys[j + 1] = _Keys[j];
                    _Contents[j + 1] = _Contents[j];
                }
                _Keys[i] = key;
                _Contents[i] = data;
                _NumKeys++;
                _BufferBlock.SendAsync((NodeStatus.Inserted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
                if (IsFull()){
                    return Split();
                }
            }
            else{
                _BufferBlock.SendAsync((NodeStatus.Inserted, 0, -1, [], [], 0, -1, [], []));
            }
            return ((-1, default(T)), null);
        }
        /// <summary>
        /// Splits this leaf node into two leaf nodes and gives the bigger half to the new node
        /// Contents and keys are split up, both NumKeys values are adjusted as well as new linking of the doubly linked list
        /// </summary>
        /// <returns>Returns the dividerEntry (dividerKey and content) and the new node.</returns>
        /// <exception cref="NullContentReferenceException"></exception>
        public ((int, T), BPlusTreeNode<T>) Split(){
            int dividerIndex = _NumKeys / 2;
            int[] newKeys = new int[_Degree];
            T[] newContent = new T[_Degree];
            (int,T) dividerEntry = (_Keys[dividerIndex], _Contents[dividerIndex] 
            ?? throw new NullContentReferenceException(
            $"Content at index:{NumKeys} within node:{ID}"));

            for (int i = 0; i < _NumKeys - dividerIndex; i++){
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
            if(newNode._NextNode != null){
                newNode._NextNode._PrevNode = newNode;
            }
            _BufferBlock.SendAsync((NodeStatus.Split, ID, NumKeys, Keys, Contents, newNode.ID,
                          newNode.NumKeys, newNode.Keys, newNode.Contents));
            return (dividerEntry, newNode);
        }

        
        
        public override void DeleteKey(int key, Stack<Tuple<BPlusNonLeafNode<T>,int>> pathStack){
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
                _BufferBlock.SendAsync((NodeStatus.Deleted, ID, NumKeys, Keys, Contents, 0, -1, [], []));

                //below is propagating the changes -> into separate method?

                if(pathStack.Count == 0){//if stack is 0, means we are in the root
                    if(isUnderflowAsRoot()){//means there are no keys left because root has to have at least 1 key/entry
                        DeleteNode(null,-1);
                        //statusupdate that this node got deleted
                    }
                }
                else{
                    Tuple<BPlusNonLeafNode<T>,int> parent = pathStack.Pop();
                    BPlusNonLeafNode<T> parentNode = parent.Item1;
                    int index = parent.Item2;

                    (BPlusLeafNode<T>?,int) leftSibling = GetLeftSibling(index, parentNode);
                    (BPlusLeafNode<T>?,int) rightSibling = GetRightSibling(index, parentNode);
                    BPlusLeafNode<T>? leftSiblingNode = leftSibling.Item1;
                    BPlusLeafNode<T>? rightSiblingNode = rightSibling.Item1;
                    
                    bool isUnderflow = IsUnderflow();
                    if(parentNode != null){
                        if(!isUnderflow){
                            // do nothing
                            // return
                        }
                        else if(isUnderflow && leftSiblingNode != null && leftSiblingNode.CanRedistribute()){
                            //send statusupdate 
                            GainsFromLeft(leftSiblingNode);
                            leftSiblingNode.LosesToRight();  
                        }
                        else if(isUnderflow && rightSiblingNode != null && rightSiblingNode.CanRedistribute()){
                            //send statusupdate
                            GainsFromRight(rightSiblingNode);
                            rightSiblingNode.LosesToLeft();
                            
                        }
                        else{
                            if(rightSiblingNode != null && rightSibling.Item1 != null){
                                //send statusupdate
                                (BPlusLeafNode<T>,int) rightSiblingNotNull = (rightSibling.Item1,rightSibling.Item2);
                                mergeWithR(rightSiblingNotNull,parentNode);
                                //Console.WriteLine("Merging with right");
                            }
                            else if(leftSiblingNode != null && leftSibling.Item1 != null){
                                //send statusupdate
                                (BPlusLeafNode<T>,int) leftSiblingNotNull = (leftSibling.Item1,leftSibling.Item2);
                                mergeWithL(leftSiblingNotNull,parentNode);
                                //Console.WriteLine("Merging with left");
                            }
                        }
                        parent.Item1.PropagateChanges(pathStack);
                    }
                }         
            }
            //send status to say key not found
            _BufferBlock.SendAsync((NodeStatus.Deleted, ID, -1, [], [], 0, -1, [], []));
        }

        public BPlusLeafNode<T>? FindLeftSibling(int key, BPlusNonLeafNode<T> parent){
            if(parent != null){
                int index = parent.FindChildIndex(key);
                if(index > 0 && parent.Children[index - 1] is BPlusLeafNode<T> leftsibling){
                    return leftsibling;
                }
            }
            return null;
        }

        public (BPlusLeafNode<T>?,int) GetLeftSibling(int index,BPlusNonLeafNode<T> parent){
            if(parent != null){
                if(index > 0 && parent.Children[index - 1] is BPlusLeafNode<T> leftSibling){
                    return (leftSibling, index - 1);
                }
            }
            return (null,-1);
        }

        public (BPlusLeafNode<T>?,int) GetRightSibling(int index,BPlusNonLeafNode<T> parent){
            if(parent != null){
                if(index >= 0 && index < parent.Children.Count() - 1 && parent.Children[index + 1] is BPlusLeafNode<T> rightSibling){
                    return (rightSibling,index + 1);
                }
            }
            return (null,-1);
        }

        public bool CanRedistribute()
            //checks if the node has more than the minimum amount of keys/entries (can spare one)
        {
            int minKeys = (int)Math.Ceiling((double)_Degree / 2) - 1;
            return _NumKeys >= minKeys + 1;
        }
        /*


        public override (int, T) ForfeitKey(){
            _NumKeys--;
            (int,T) keyToBeLost = (_Keys[_NumKeys], _Contents[_NumKeys] 
            ?? throw new NullContentReferenceException(
            $"Content at index:{_NumKeys} within node:{ID}"));
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
            _BufferBlock.SendAsync((Status.Forfeit, ID, NumKeys, Keys, Contents, 0, -1, [], []));
            return keyToBeLost;
        }

        */

        public void DeleteNode(BPlusNonLeafNode<T>? parentNode, int indexOfChildBeingDeleted){
            if(this == null){
                return;
            }          
            if(parentNode != null){
                if(indexOfChildBeingDeleted != -1){

                parentNode.Children[indexOfChildBeingDeleted] = default;
                for (; indexOfChildBeingDeleted < _Degree;)
                {
                    parentNode.Children[indexOfChildBeingDeleted] = parentNode.Children[indexOfChildBeingDeleted + 1];
                    indexOfChildBeingDeleted++;
                }
                }
                if(_NextNode != null){
                    _NextNode._PrevNode = _PrevNode;
                    _NextNode = null;
                }
                if(_PrevNode != null){
                    _PrevNode._NextNode = _NextNode;
                    _PrevNode = null;
                }
                for(int i = 0; i < _NumKeys; i++){
                    _Keys[i] = default;
                    _Contents[i] = default;
                }
                _NumKeys = default;
            }else{
                //
                return;
            }

        }
        public void Merge(int dividerKey, T dividerData, BPlusLeafNode<T> sibiling){
            _Keys[_NumKeys] = dividerKey;
            _Contents[_NumKeys] = dividerData;
            _NumKeys++;
            for (int i = 0; i < sibiling.NumKeys; i++){
                _Keys[_NumKeys + i] = sibiling.Keys[i];
                _Contents[_NumKeys + i] = sibiling.Contents[i];
            }
            _NumKeys += sibiling.NumKeys;
            _BufferBlock.SendAsync((NodeStatus.Merge, ID, NumKeys, Keys, Contents, sibiling.ID, -1, [], []));
        }

        public void mergeWithR((BPlusLeafNode<T>,int) sibling, BPlusNonLeafNode<T> parent){
            BPlusLeafNode<T> siblingNode = sibling.Item1;
            int siblingIndex = sibling.Item2;
            for(int i = 0; i < siblingNode.NumKeys; i++){
                _Keys[_NumKeys + i] = siblingNode.Keys[i];
                _Contents[_NumKeys + i] = siblingNode.Contents[i]; //insert all values from sibling Node into this
  
            }
            _NumKeys += siblingNode.NumKeys;
            siblingNode.DeleteNode(parent,siblingIndex);//use parent to pass down to delete() to properly delete

            
        }

        public void mergeWithL((BPlusLeafNode<T>,int) sibling, BPlusNonLeafNode<T> parentNode){
            BPlusLeafNode<T> siblingNode = sibling.Item1;
            int siblingIndex = sibling.Item2;
            int numKeysInSibling = siblingNode.NumKeys;
            for(int i = 0; i < numKeysInSibling; i++){
                GainsFromLeft(siblingNode);
                siblingNode.LosesToRight();
                //insert all values from sibling Node into this
            }
            //_NumKeys += siblingNode.NumKeys;
            siblingNode.DeleteNode(parentNode,siblingIndex);
            //use parent to pass down to delete() to properly delete

        }


        
        public void GainsFromRight(BPlusLeafNode<T> sibling)
        {
            _Keys[_NumKeys] = sibling.Keys[0];
            _Contents[_NumKeys] = sibling.Contents[0];
            _NumKeys++;
        }   
        
        public void LosesToLeft(){
            for (int i = 0; i < _NumKeys - 1; i++){
                _Keys[i] = _Keys[i + 1];
                _Contents[i] = _Contents[i + 1];
            }
            _NumKeys--;
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
        }

        public void GainsFromLeft(BPlusLeafNode<T> sibling){
            for (int i = _NumKeys - 1; i >= 0; i--){
                _Keys[i + 1] = _Keys[i];
                _Contents[i + 1] = _Contents[i];
            }
            _NumKeys++;
            _Keys[0] = sibling.Keys[sibling.NumKeys - 1];
            _Contents[0] = sibling.Contents[sibling.NumKeys - 1];
        }

        public void LosesToRight(){
            _NumKeys--;
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
        }
        

        public bool IsUnderflow()
            //checks if the node needs more entries
        {
            int minKeys = (int)Math.Ceiling((double)_Degree / 2) - 1;
            return _NumKeys < minKeys;
        }

        public bool isUnderflowAsRoot(){//if a leafnode is the root, it has to have at least one entry
            int minKeys = 1;
            return _NumKeys >= minKeys;
        }

        /// <summary>
        /// Prints out the contents of the node in JSON format.
        /// </summary>
        /// <remarks>Author: Tristan Anderson, modified by Andreas Kramer
        /// <param name="x">Hierachical Node ID</param>
        /// <returns>String with the entirety of this node's keys and contents arrays formmatted in JSON syntax.</returns>
        public override string Traverse(string x){
            string output = Spacer(x) + "{\n";
            output += Spacer(x) + "  \"leafnode\":\"" + x + "\",\n" 
            + Spacer(x) + "\"  ID\":" + _ID + ",\n"
            //+ Spacer(x) + "\" Prev\":" + _PrevNode.ID           
            + Spacer(x) + "  \"keys\":[";
            for (int i = 0; i < _NumKeys; i++){
                output += _Keys[i] + (i + 1 < _NumKeys ? "," : "");
            }
            
            output += "],\n" + Spacer(x) + "  \"contents\":[";
            
            for (int i = 0; i < _NumKeys; i++){
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                output += _Contents[i].ToString() + (i + 1 < _NumKeys ? "," : "");
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            
            output += "]\n";
            if(_NextNode != null){
                output += Spacer(x) + "\"  next\":" + _NextNode._ID + ",\n";
            }
            if(_PrevNode != null){
                output += Spacer(x) + "\"  prev\":" + _PrevNode._ID + ",\n";
            }
            return output + Spacer(x) + "}";
        }
        
    }
}