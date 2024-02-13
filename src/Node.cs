/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Base class for all the node objects used in the BTree and B+Tree.
*/
using NodeData;

namespace BTreeVisualization{
  public abstract class Node(int degree)
  {
    protected int _Degree = degree;
    protected Node? _Parent = null;
    protected int _NumKeys = 0;
    protected int[] _Keys = new int[2 * degree];

    public abstract (int,Node) SearchKey(int key);
    public abstract ((int,Data),Node) Split();
    public abstract bool IsFull();
    public abstract bool IsUnderflow();
    public abstract ((int,Data?),Node?) InsertKey(int key, Data data);
    public abstract void DeleteKey(int key);
    public abstract string Traverse();
    public int GetNumKeys(){
      return _NumKeys;
    }
  }

  public abstract class BTreeNode(int degree) : Node(degree){
    protected Data[] _Contents = new Data[2 * degree];

    public Data[] Contents{
      get{ return _Contents; }
    }
  }
}