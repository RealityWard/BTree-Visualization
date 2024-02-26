/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Implements the leaf nodes of a B-Tree. Non-recursive function iteration due to no children.
*/
using System.Text.RegularExpressions;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace BTreeVisualization{
  public class LeafNode<T>(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock) : BTreeNode<T>(degree, bufferBlock){
    public LeafNode(int degree, int[] keys, T[] contents, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock) : this(degree, bufferBlock){
      _NumKeys = keys.Length;
      for(int i = 0; i < keys.Length; i++){
        _Keys[i] = keys[i];
        _Contents[i] = contents[i];
      }
    }
    
    /// <summary>
    /// Author: Tristan Anderson
    /// Iterates over the _Keys array to find key. If found returns the index else returns -1.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private int Search(int key){
      for(int i = 0; i < _NumKeys; i++){
        if(_Keys[i] == key){
          return i;
        }
      }
      return -1;
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Iterates over the _Keys array to find key. If found returns the index and this else returns -1 and this.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public override (int,BTreeNode<T>) SearchKey(int key){
      return (Search(key),this);
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Evenly splits the _Contents and _Keys to two new nodes
    /// </summary>
    public override ((int,T),BTreeNode<T>) Split(){
      int[] newKeys = new int[_Degree-1];
      T[] newContent = new T[_Degree-1];
      for(int i = 0; i < _Degree-1; i++){
        newKeys[i] = _Keys[i+_Degree];
        newContent[i] = _Contents[i+_Degree];
      }
      _NumKeys = _Degree-1;
      LeafNode<T> newNode = new(_Degree,newKeys,newContent,_BufferBlock);
      _BufferBlock.Post((Status.Split, ID, NumKeys, Keys, Contents, newNode.ID, newNode.NumKeys, newNode.Keys, newNode.Contents));
      return ((_Keys[_NumKeys],_Contents[_NumKeys]),newNode);
    }
    
    /// <summary>
    /// Author: Tristan Anderson
    /// Finds and places the new info in the 
    /// current node. If it reaches capacity it 
    /// calls split and returns the new node 
    /// created from the split. Otherwise it 
    /// returns ((-1,null),null).
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public override ((int,T?),BTreeNode<T>?) InsertKey(int key, T data){
      _BufferBlock.Post((Status.ISearching, ID, -1, [], [], 0, -1, [], []));
      int i = 0;
      while(i < _NumKeys && key >= _Keys[i])
        i++;
      if(key != _Keys[i] || key == 0){
        for (int j = _NumKeys - 1; j >= i; j--){
          _Keys[j+1] = _Keys[j];
          _Contents[j+1] = _Contents[j];
        }
        _Keys[i] = key;
        _Contents[i] = data;
        _NumKeys++;
        _BufferBlock.Post((Status.Inserted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
        if(IsFull()){
          return Split();
        }
      }else{
        _BufferBlock.Post((Status.Inserted, 0, -1, [], [], 0, -1, [], []));
      }
      return ((-1,default(T)),null);
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// If the entry exists it deletes it.
    /// </summary>
    /// <param name="key"></param>
    public override void DeleteKey(int key){
      int i = Search(key);
      if(i != -1){
        for (; i < _NumKeys; i++){
          _Keys[i] = _Keys[i+1];
          _Contents[i] = _Contents[i+1];
        }
        _NumKeys--;
      }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Returns either its first/LeftMost key or last 
    /// key to the parent. Then it decrements the number of keys.
    /// </summary>
    /// <param name="leftMost"></param>
    /// <returns></returns>
    public override (int, T) ForfeitKey(){
      _NumKeys--;
      return (_Keys[_NumKeys],_Contents[_NumKeys]);
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
        _Keys[_NumKeys + i] = sibiling.Keys[i];
        _Contents[_NumKeys + i] = sibiling.Contents[i];
      }
      _NumKeys += sibiling.NumKeys;
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Tacks on the given key and data of the sibiling.
    /// </summary>
    /// <param name="dividerKey"></param>
    /// <param name="dividerData"></param>
    /// <param name="sibiling"></param>
    public override void GainsFromRight(int dividerKey, T dividerData, BTreeNode<T> sibiling){
      _Keys[_NumKeys] = dividerKey;
      _Contents[_NumKeys] = dividerData;
      _NumKeys++;
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Shifts the values in the arrays by one to the left overwriting 
    /// the first entries and decrements the _NumKeys var.
    /// </summary>
    public override void LosesToLeft(){
      for(int i = 0; i < _NumKeys-1; i++){
        _Keys[i] = _Keys[i+1];
        _Contents[i] = _Contents[i+1];
      }
      _NumKeys--;
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-22
    /// Inserts at the beginning of this node arrays the 
    /// given key and data.
    /// </summary>
    /// <param name="dividerKey"></param>
    /// <param name="dividerData"></param>
    /// <param name="sibiling"></param>
    public override void GainsFromLeft(int dividerKey, T dividerData, BTreeNode<T> sibiling)
    {
      for(int i = _NumKeys-1; i >= 0; i--){
        _Keys[i+1] = _Keys[i];
        _Contents[i+1] = _Contents[i];
      }
      _NumKeys++;
      _Keys[0] = dividerKey;
      _Contents[0] = dividerData;
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
    /// Returns all the keys and the coresponding contents as JSON objects in string form.
    /// </summary>
    /// <returns></returns>
    public override string Traverse(string x){
      string output = Spacer(x) + "{\n";
      output += Spacer(x) + "  \"leafnode\":\"" + x + "\" | " + _ID + ",\n" + Spacer(x) + "  \"keys\":[";
			for(int i = 0; i < _NumKeys; i++){
        output += _Keys[i] + (i+1 < _NumKeys ? "," : "");
      }
      output += "],\n" + Spacer(x) + "  \"contents\":[";
			for(int i = 0; i < _NumKeys; i++){
        #pragma warning disable CS8602 // Dereference of a possibly null reference.
        output += _Contents[i].ToString() + (i+1 < _NumKeys ? "," : "");
        #pragma warning restore CS8602 // Dereference of a possibly null reference.
      }
			return output + "]\n" + Spacer(x) + "}";
    }
  }
}