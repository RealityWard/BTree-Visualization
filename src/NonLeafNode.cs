using System;
using System.Linq;
using BTreeVisualization;

namespace BTreeVisualization
{
	public class NonLeafNode(int degree) : BTreeNode(degree){
		private BTLeafNode[] _children;

		public NonLeafNode(int degree, int[] keys, Data[] data, BTreeNode[] children): this.degree {
			for(int i = 0; i< degree; i ++){
				_Keys[i] = new keys[i];
				_Content[i] = new data[i];
				_children[i] = new children[i];
			}
		}
		
/// <summary>
/// Searches the tree for a key. 
/// </summary>
/// <param name="key"> </param>
/// <returns></returns>
		public (int, BTreeNode) searchKey(int key)
		{
			//searches for correct key, finds it returns the node, else returns -1 
			foundKey = false;
			for (int i = 0; i < _NumKeys; i++){
				if (_Keys[i] == key) {
					return this;
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
		public override ((int,Data?),Node?) InsertKey(BTreeNode currentNode, int key, Data data){
			for (int i = 0; i < _NumKeys; i++){
				if (_Keys[i] >= key && _children[i] != null){
					this.InsertKey(currentNode._children[i], key, data);
				}
				else if (_Keys[i] < key && _children[i] != null){
					this.InsertKey(currentNode._children.Last(), key, data)
				}

			} 			
			
		}
			
			

	/// <summary>
    /// Evenly splits the _Contents and _Keys to two new nodes
    /// </summary>
		public override ((int,Data),Node) Split(){
			int[] newKeys = new int[_Degree];
			Data[] newContent = new Data[_Degree];
			BTreeNode[] newChildren = new BTreeNode[_children];
			for(int i = 0; i < _Degree; i++){
				newKeys[i] = _Keys[i+_Degree];
				newContent[i] = _Contents[i+_Degree];
				_children[i] = _children[i+Degree];
			}
			_NumKeys = _Degree-1;
			NonLeafNode newNode = new(_Degree,newKeys,newContent, newChildren);
			return ((_Keys[_Degree-1],_Contents[_Degree-1],
			 _children[_Degree -1]),newNode);
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
		public void deleteKey(int key){
			foundKey = searchKey(key);
			if(foundKey != -1){
					for (; i < _NumKeys; i++){
					_Keys[i] = _Keys[i+1];
					_Contents[i] = _Contents[i+1];
				}
				_NumKeys--;
				return;
			}
			
			for(int i = 0; i<_NumKeys; i++){
				if (_Keys[i] > key && _children[i] != null){
					return this.searchKey(_children[i], key);
				}
				if (_Keys[i] < key && _children[i] != null){
					return this.searchKey(_children.Last(), key)
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

