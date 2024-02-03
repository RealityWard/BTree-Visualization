using NodeData;

namespace BTreeVisualization{
  public class BTree{
    private BTreeNode _Root;
    private int _Degree;
    public BTree(int degree){
      _Root = null;
      _Degree = degree;
    }
    
    public void insert(int key, Data data){
      if(_Root == null){
        _Root = new LeafNode(_Degree);
        _Root.InsertKey(key, data);
      }else{
        if(_Root.GetNumKeys() == 2*_Degree-1){
          BTreeNode node = new NonLeafNode(_Degree);
          node.InsertChild(key, _Root);
          _Root = node;
        }else{
          _Root.SearchKey(key).Item2.InsertKey(key, data);
        }
      }
    }


    public void delete(int key){
      _Root.SearchKey(key).Item2.DeleteKey(key);
    }


    public (int,BTreeNode) search(int key){
      if(_Root.SearchKey(key).Item1 == -1){

      }
      return (-1,null);
    }


    public string traverse(){
      string result = "";
      return "";
    }
  }
}