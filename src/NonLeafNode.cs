﻿using System.Text.RegularExpressions;

namespace BTreeVisualization
{
	public class NonLeafNode<T>(int degree) : BTreeNode<T>(degree){
		private BTreeNode<T>[] _Children = new BTreeNode<T>[2 * degree];
    public BTreeNode<T>[] Children{
      get{ return _Children; }
    }

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
		private int Search(int key)
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
    /// Finds the correct branch of the the tree to place the new key. 
    /// It shouldn't add to anything but a leaf node.
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
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// After deleting a key from itself a NonLeafNode needs to replace the key 
    /// thus it looks to the left child of said key and calls ForfeitKey on it. 
    /// Otherwise it passes the search down to the child with keys greater than itself. 
    /// Afterwards checks the child for underflow. 
    /// </summary>
    /// <param name="key"></param>
		public override void DeleteKey(int key){
      string printed = Traverse(key.ToString() + "P");
      int result = Search(key);
			if(result == -1){
        // Search only goes through keys and thus if it did not 
        // find it at the last index it returns -1.
        _Children[_NumKeys].DeleteKey(key);
        MergeAt(_NumKeys);
      }else if(_Keys[result] == key){
        (_Keys[result],_Contents[result]) = _Children[result].ForfeitKey();
        MergeAt(result);
      }else{
        _Children[result].DeleteKey(key);
        MergeAt(result);
      }
      printed = Traverse(key.ToString());
		}

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-23
    /// Calls ForfeitKey() on last child for a replacement key for the parent node. Afterwards checks the child for underflow.
    /// </summary>
    /// <param name="leftMost"></param>
    /// <returns></returns>
    public override (int,T) ForfeitKey(){
      (int,T) result;
      result = _Children[_NumKeys].ForfeitKey();
      MergeAt(_NumKeys);
      return result;
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Tacks on the given divider to its own arrays and grabs 
    /// all the entries from the sibiling adding those to its arrays as well.
    /// </summary>
    /// <param name="dividerKey"></param>
    /// <param name="dividerData"></param>
    /// <param name="sibiling"></param>
    public override void Merge(int dividerKey, T dividerData, BTreeNode<T> sibiling){
      _Keys[_NumKeys] = dividerKey;
      _Contents[_NumKeys] = dividerData;
      _NumKeys++;
      for(int i = 0; i < sibiling.NumKeys; i++){
        _Keys[_NumKeys+i] = sibiling.Keys[i];
        _Contents[_NumKeys+i] = sibiling.Contents[i];
        _Children[_NumKeys+i] = ((NonLeafNode<T>)sibiling).Children[i];
      }
      _Children[_NumKeys+sibiling.NumKeys] = ((NonLeafNode<T>)sibiling).Children[sibiling.NumKeys];
      _NumKeys += sibiling.NumKeys;
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Checks the child at index for underflow. If so it then checks for _Degree 
    /// number of children in the right child of the key. _Degree or greater means 
    /// either overflow or split. 
    /// </summary>
    /// <param name="index"></param>
    private void MergeAt(int index){
      if(_Children[index].IsUnderflow()){
        if(index == _NumKeys){ index--; }
        if(_Children[index+1].NumKeys >= _Degree){
          _Children[index].GainsFromRight(_Keys[index],_Contents[index],_Children[index+1]);
          _Keys[index] = _Children[index+1].Keys[0];
          _Contents[index] = _Children[index+1].Contents[0];
          _Children[index+1].LosesToLeft();
        }else if(_Children[index].NumKeys >= _Degree){
          _Children[index+1].GainsFromLeft(_Keys[index],_Contents[index],_Children[index]);
          _Keys[index] = _Children[index].Keys[_Children[index].NumKeys-1];
          _Contents[index] = _Children[index].Contents[_Children[index].NumKeys-1];
          _Children[index].LosesToRight();
        }else{
          _Children[index].Merge(_Keys[index],_Contents[index],_Children[index+1]);
          for(; index < _NumKeys-1; ){
            _Keys[index] = _Keys[index+1];
            _Contents[index] = _Contents[index+1];
            index++;
            _Children[index] = _Children[index+1];
          }
          _NumKeys--;
        }
      }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Tacks on the given key and data and grabs the first child of the sibiling.
    /// </summary>
    /// <param name="dividerKey"></param>
    /// <param name="dividerData"></param>
    /// <param name="sibiling"></param>
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
    public override void LosesToLeft()
    {
      for(int i = 0; i < _NumKeys-1; i++){
        _Keys[i] = _Keys[i+1];
        _Contents[i] = _Contents[i+1];
        _Children[i] = _Children[i+1];
      }
      _NumKeys--;
      _Children[_NumKeys] = _Children[_NumKeys+1];
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-22
    /// Inserts at the beginning of this node arrays the 
    /// given key and data and grabs the last child of the sibiling.
    /// </summary>
    /// <param name="dividerKey"></param>
    /// <param name="dividerData"></param>
    /// <param name="sibiling"></param>
    public override void GainsFromLeft(int dividerKey, T dividerData, BTreeNode<T> sibiling)
    {
      _Children[_NumKeys+1] = _Children[_NumKeys];
      for(int i = _NumKeys; i > 0; i--){
        _Keys[i] = _Keys[i-1];
        _Contents[i] = _Contents[i-1];
        _Children[i] = _Children[i-1];
      }
      _NumKeys++;
      _Keys[0] = dividerKey;
      _Contents[0] = dividerData;
      _Children[0] = ((NonLeafNode<T>)sibiling).Children[sibiling.NumKeys];
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-22
    /// Decrements the _NumKeys var.
    /// </summary>
    public override void LosesToRight()
    {
      _NumKeys--;
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
	}
}
