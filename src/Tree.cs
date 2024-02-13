/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Maintains the entry point of the BTree data structure and initializes root and new node creation in the beginning.
*/
using System.ComponentModel;
using NodeData;

namespace BTreeVisualization{
  public class BTree{
    private BTreeNode _Root;
    private int _Degree;
    public BTree(int degree){
      _Root = new LeafNode(degree);
      _Degree = degree;
    }
    
    /// <summary>
    /// Takes a node and the key and data to place into root.
    /// </summary>
    /// <param name="rNode"></param>
    private void Split(((int,Data),Node) rNode){
      _Root = new NonLeafNode(_Degree,[rNode.Item1.Item1],[rNode.Item1.Item2],[_Root, rNode.Item2]);
    }
    
    /// <summary>
    /// Inserts data at root node and set root to a new leaf node if root isn't yet created. It also checks for a split afterwards.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    public void Insert(int key, Data data){
      if(_Root == null){
        _Root = new LeafNode(_Degree);
        _Root.InsertKey(key, data);
      }else{
        ((int,Data?),Node?) result = _Root.InsertKey(key, data);
        if(result.Item2 != null && result.Item1.Item2 != null){
          Split(((result.Item1.Item1,true? result.Item1.Item2 : new Person("Placeholder")),true? result.Item2 : new LeafNode(_Degree)));
        }
      }
    }

    /// <summary>
    /// Using searchKey on the nodes to find the entry and calls delete on the node returned. If index is -1 it does nothing.
    /// </summary>
    /// <param name="key"></param>
    public void Delete(int key){
      (int,Node) result = _Root.SearchKey(key);
      if(result.Item1 != -1){
        result.Item2.DeleteKey(key);
      }
    }

    /// <summary>
    /// Using searchKey on the nodes to return the data correlated to the key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Data? Search(int key){
      (int,Node?) result = _Root.SearchKey(key);
      if (result.Item1 == -1 || result.Item2 == null){
        return null;
      }else{
        return ((BTreeNode)result.Item2).Contents[result.Item1];
      }
    }

    public string Traverse(){
      return _Root.Traverse();
    }
  }
}