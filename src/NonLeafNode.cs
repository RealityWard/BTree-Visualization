using System.Transactions;
using NodeData;

namespace BTreeVisualization
{
	public class NonLeafNode<T>(int degree) : BTreeNode<T>(degree){
		protected BTreeNode<T>[] _Children = new BTreeNode<T>[2 * degree];
		public NonLeafNode(int degree, int[] keys, T[] data, BTreeNode<T>[] children) : this(degree) {
      _NumKeys = keys.Length;
      for(int i = 0; i < keys.Length; i++){
				_Keys[i] = keys[i];
				_Contents[i] = data[i];
				_Children[i] = children[i];
			}
			_Children[keys.Length] = children[keys.Length];
		}
		
    /// <summary>
    /// Searches the tree for a key. 
    /// </summary>
    /// <param name="key"> </param>
    /// <returns></returns>
		protected int Search(int key)
		{
			//searches for correct key, finds it returns the node, else returns -1
			for (int i = 0; i < _NumKeys; i++){
				if (_Keys[i] >= key) {
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
		public override (int,BTreeNode<T>) SearchKey(int key){
      int result = Search(key);
			if(result == -1){
        return _Children[_NumKeys].SearchKey(key);
      }else if(_Keys[result] == key){
        return (result,this);
      }else{
        return _Children[result].SearchKey(key);
      }
		}

    /// <summary>
    //Finds the correct branch of the the tree to place the new key. 
    //It shouldn't add to anything but a leaf node.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <returns></returns>
	public override ((int,T?),BTreeNode<T>?) InsertKey(int key, T data){
      ((int,T?),BTreeNode<T>?) result;
      int i = 0;
      while(i < _NumKeys && key >= _Keys[i])
        i++;
      result = _Children[i].InsertKey(key, data);
      if(result.Item2 != null && result.Item1.Item2 != null){
        for (int j = _NumKeys - 1; j >= i; j--){
          _Keys[j+1] = _Keys[j];
          _Contents[j+1] = _Contents[j];
          _Children[j+2] = _Children[j+1];
        }
        _Keys[i] = result.Item1.Item1;
        _Contents[i] = result.Item1.Item2;
        _Children[i+1] = result.Item2;
        _NumKeys++;
        if(IsFull()){
          return Split();
        }
      }
      return ((-1,default(T)),null);
		}

	  /// <summary>
    /// Evenly splits the _Contents and _Keys to two new nodes
    /// </summary>
		public override ((int,T),BTreeNode<T>) Split(){
			int[] newKeys = new int[_Degree-1];
			T[] newContent = new T[_Degree-1];
			BTreeNode<T>[] newChildren = new BTreeNode<T>[_Degree];
      int i = 0;
			for(; i < _Degree-1; i++){
				newKeys[i] = _Keys[i+_Degree];
				newContent[i] = _Contents[i+_Degree];
				newChildren[i] = _Children[i+_Degree];
			}
			newChildren[i] = _Children[i+_Degree];
			_NumKeys = _Degree-1;
			NonLeafNode<T> newNode = new(_Degree,newKeys,newContent,newChildren);
			return ((_Keys[_NumKeys],_Contents[_NumKeys]),newNode);
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
					_Children[i].SearchKey(key);
				}
				if (_Keys[i] < key && _Children[i] != null){
					_Children.Last().SearchKey( key);
				}
			}
		}

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-13
    /// Desc: Prints out the contents of the node in JSON format.
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
		public override string Traverse(string x){
      string output = Spacer(x) + "{\n";
      output += Spacer(x) + "  \"node\":\"" + x + "\"\n" + Spacer(x) + "  \"keys\":[";
			for(int i = 0; i < _NumKeys; i++){
        output += _Keys[i] + (i+1 < _NumKeys ? "," : "");
      }
      output += "],\n" + Spacer(x) + "  \"contents\":[";
			for(int i = 0; i < _NumKeys; i++){
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        output += _Contents[i].ToString() + (i+1 < _NumKeys ? "," : "");
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
      }
      output += "],\n" + Spacer(x) + "  \"children\":[\n";
			for(int i = 0; i <= _NumKeys; i++){
				output += _Children[i].Traverse(x + "." + i) + (i+1 < _NumKeys ? "," : "") + "\n";
      }
			return output + Spacer(x) + "  ]\n" + Spacer(x) + "}";
		}

    // public BTreeNode<T> MakeNewRoot(((int,T),BTreeNode<T>) rNode){
    //   return _Degree,[rNode.Item1.Item1],[rNode.Item1.Item2],[_Root, rNode.Item2];
    // }
	}
}
