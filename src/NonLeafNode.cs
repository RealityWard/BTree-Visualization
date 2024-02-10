using NodeData;

namespace BTreeVisualization
{
	public class NonLeafNode(int degree) : BTreeNode(degree){
		private BTreeNode[] _Children;

		public NonLeafNode(int degree, int[] keys, Data[] data, BTreeNode[] children): this(degree) {
			for(int i = 0; i < degree; i ++){
				_Keys[i] = keys[i];
				_Contents[i] = data[i];
				_Children[i] = children[i];
			}
		}
		
/// <summary>
/// Searches the tree for a key. 
/// </summary>
/// <param name="key"> </param>
/// <returns></returns>
		private int Search(int key)
		{
			//searches for correct key, finds it returns the node, else returns -1 
			bool foundKey = false;
			for (int i = 0; i < _NumKeys; i++){
				if (_Keys[i] == key) {
					return i;
				}
			}
			return -1;
		}
			
		/// <summary>
		/// Iterates over the _Keys array to find key. If found returns the index and this else returns -1 and this.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public override (int,Node) SearchKey(int key){
			return (Search(key),this);
		}

		 /// <summary>
		 //Finds the correct branch of the the tree to place the new key. 
		 //It shouldn't add to anything but a leaf node.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public override ((int,Data?),Node?) InsertKey(int key, Data data){
      ((int,Data?),Node?) result;
			for (int i = 0; i < _NumKeys; i++){
				if (_Keys[i] >= key && _Children[i] != null){
					_Children[i].InsertKey(key, data);
				}
				else if (_Keys[i] < key && _Children[i] != null){
					this.InsertKey(currentNode._children.Last(), key, data);
				}

			} 			
			
		}

	/// <summary>
    /// Evenly splits the _Contents and _Keys to two new nodes
    /// </summary>
		public override ((int,Data),Node) Split(){
			int[] newKeys = new int[_Degree];
			Data[] newContent = new Data[_Degree];
			BTreeNode[] newChildren = new BTreeNode[_Degree];
			for(int i = 0; i < _Degree; i++){
				newKeys[i] = _Keys[i+_Degree];
				newContent[i] = _Contents[i+_Degree];
				_Children[i] = _Children[i+_Degree];
			}
			_NumKeys = _Degree-1;
			NonLeafNode newNode = new(_Degree,newKeys,newContent,newChildren);
			return ((_Keys[_Degree-1],_Contents[_Degree-1]),newNode);
		}

		/// <summary>
		/// Used to determine if split is needed.
		/// </summary>
		/// <returns></returns>
		public override bool IsFull(){
			return _NumKeys == 2*_Degree;
		}

		/// <summary>
		/// Used to determine if merge is needed.
		/// </summary>
		/// <returns></returns>
		public override bool IsUnderflow(){
			return _NumKeys <= _Degree-1;
		}

    /// <summary>
    /// If the entry exists it deletes it.
    /// </summary>
    /// <param name="key"></param>
		public override void DeleteKey(int key){
			int foundKey = Search(key);

			if(foundKey != -1){
					for (int i = 0; i < _NumKeys; i++){
					_Keys[i] = _Keys[i+1];
					_Contents[i] = _Contents[i+1];
				}
				_NumKeys--;
				return;
			}
			
			for(int i = 0; i<_NumKeys; i++){
				if (_Keys[i] > key && _Children[i] != null){
					return this.searchKey(_Children[i], key);
				}
				if (_Keys[i] < key && _Children[i] != null){
					return this.searchKey(_Children.Last(), key)
				}
			}
		}

		public override string Traverse(BTreeNode currentNode, string result){
      		string result = "{\n";
			for(int i = 0; i < _NumKeys; i++){
				result += i + "th child: \n" ;
				result += this.Traverse(currentNode._children[i], result);
				result += "\"key\":\"" + _Keys[i] + "\",\n" + _Contents[i].ToString() + 
				(i+1 < _NumKeys ? "," : "") + "\n";

			}
			
			result += this.Traverse(currentNode._children.Last, result);
			return result + "}";
		}

	}
}

