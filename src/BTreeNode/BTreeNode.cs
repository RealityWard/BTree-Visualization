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
    public abstract (int,BTreeNode) SearchKey(int key);
    public abstract ((int,Data),BTreeNode) Split();
    public abstract bool IsFull();
    public abstract bool IsUnderflow();
    public abstract ((int,Data?),BTreeNode?) InsertKey(int key, Data data);
    public abstract void DeleteKey(int key);
    public abstract string Traverse(string? output);
    public int getNumKeys(){
      return _NumKeys;
    }
  }
}