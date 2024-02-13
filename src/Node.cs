/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Base class for all the node objects used in the BTree and B+Tree.
*/
using NodeData;

namespace BTreeVisualization{
  public abstract class Node<T,D>(int degree)
  {
    protected int _Degree = degree;
    protected T? _Parent;
    protected int _NumKeys = 0;
    protected int[] _Keys = new int[2 * degree];

    public abstract (int,T) SearchKey(int key);
    public abstract ((int,D),T) Split();
    public abstract bool IsFull();
    public abstract bool IsUnderflow();
    public abstract ((int,D?),T?) InsertKey(int key, D data);
    public abstract void DeleteKey(int key);
    public abstract string Traverse();
    public int GetNumKeys(){
      return _NumKeys;
    }
  }

  public abstract class BTreeNode<T>(int degree) : Node<BTreeNode<T>,T>(degree){
    protected T[] _Contents = new T[2 * degree];

    public T[] Contents{
      get{ return _Contents; }
    }
  }
}