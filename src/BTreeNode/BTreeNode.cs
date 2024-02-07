using NodeData;

namespace BTreeVisualization{
  public abstract class BTreeNode{
    protected int _Degree;
    protected BTreeNode? _Parent;
    
    protected int _NumKeys = 0;
    protected int[] _Keys;
    protected Data[] _Contents;

    protected BTreeNode(int degree){
      _Parent = null;
      _Degree = degree;
      _Keys = new int[2*degree];
      _Contents = new Data[2*degree];
    }
    public abstract (int,BTreeNode) searchKey(int key);
    public abstract void split();
    public abstract bool isFull();
    public abstract bool isUnderflow();
    public abstract void insertKey(int key, Data data);
    public abstract void deleteKey(int key);
    public int getNumKeys(){
      return _NumKeys;
    }
  }
}