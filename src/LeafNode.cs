/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Implements the leaf nodes of a B-Tree. Non-recursive function iteration due to no children.
*/
using NodeData;

namespace BTreeVisualization{
  public class LeafNode(int degree) : BTreeNode(degree){
    public LeafNode(int degree, int[] keys, Data[] contents) : this(degree){
      for(int i = 0; i < degree; i++){
        _Keys[i] = keys[i];
        _Contents[i] = contents[i];
      }
    }
    
    /// <summary>
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
    /// Iterates over the _Keys array to find key. If found returns the index and this else returns -1 and this.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public override (int,Node) SearchKey(int key){
      return (Search(key),this);
    }

    /// <summary>
    /// Evenly splits the _Contents and _Keys to two new nodes
    /// </summary>
    public override ((int,Data),Node) Split(){
      int[] newKeys = new int[_Degree];
      Data[] newContent = new Data[_Degree];
      for(int i = 0; i < _Degree; i++){
        newKeys[i] = _Keys[i+_Degree];
        newContent[i] = _Contents[i+_Degree];
      }
      _NumKeys = _Degree-1;
      LeafNode newNode = new(_Degree,newKeys,newContent);
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
    /// Finds and places the new info in the current node. If it reaches capacity it calls split and
    /// returns the new node created from the split. Otherwise it returns ((-1,null),null).
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public override ((int,Data?),Node?) InsertKey(int key, Data data){
      int i = 0;
      while(i < _NumKeys && key < _Keys[i])
        i++;
      for (int j = _NumKeys - 1; j >= i; j--){
        _Keys[j+1] = _Keys[j];
        _Contents[j+1] = _Contents[j];
      }
      _Keys[i] = key;
      _Contents[i] = data;
      _NumKeys++;
      if(IsFull()){
        return Split();
      }
      return ((-1,null),null);
    }

    /// <summary>
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
    /// Returns all the keys and the coresponding contents as JSON objects in string form.
    /// </summary>
    /// <returns></returns>
    public override string Traverse(){
      string result = "{\n";
      for(int i = 0; i < _NumKeys; i++){
        result += "\"key\":\"" + _Keys[i] + "\",\n" + _Contents[i].ToString() + 
          (i+1 < _NumKeys ? "," : "") + "\n";
      }
      return result + "}";
    }
  }
}