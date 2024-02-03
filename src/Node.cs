using NodeData;

namespace BTreeVisualization{
  public abstract class Node{
    private int _Degree;
    private Node? _Parent;
    private int _NumKeys = 0;
    private int[] _Keys;

    protected Node(int degree){
      _Parent = null;
      _Degree = degree;
      _Keys = new int[2*degree];
    }
    public abstract (int,Node) SearchKey(int key);
    public abstract void Split();
    public abstract bool IsFull();
    public abstract bool IsUnderflow();
    public abstract void InsertKey(int key, Data data);
    public abstract void DeleteKey(int key);
    public int GetNumKeys(){
      return _NumKeys;
    }
  }

  public abstract class BTreeNode : Node{
    private Data[] _Contents;
    protected BTreeNode(int degree) : base(degree){
      _Contents = new Data[2*degree];
    }
  }
}