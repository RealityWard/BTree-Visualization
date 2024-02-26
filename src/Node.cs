/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Base class for all the node objects used in the BTree and B+Tree.
*/
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace BTreeVisualization{
  public abstract class Node<N,T>(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock)
  {
    protected BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> _BufferBlock = bufferBlock;
    protected readonly int _Degree = degree;
    protected long _ID = DateTime.Now.Ticks;
    protected N? _Parent;
    protected int _NumKeys = 0;
    protected int[] _Keys = new int[2 * degree - 1];

    public abstract (int,N) SearchKey(int key);
    public abstract ((int,T),N) Split();
    public abstract void Merge(int dividerKey, T dividerData, N sibiling);
    public abstract void GainsFromRight(int dividerKey, T dividerData, N sibiling);
    public abstract void GainsFromLeft(int dividerKey, T dividerData, N sibiling);
    public abstract void LosesToLeft();
    public abstract void LosesToRight();
    public bool IsFull(){
      return _NumKeys == 2*_Degree-1;
    }
    public bool IsUnderflow(){
      return _NumKeys < _Degree-1;
    }
    public abstract ((int,T?),N?) InsertKey(int key, T data);
    public abstract void DeleteKey(int key);
    public abstract (int,T) ForfeitKey();
    public abstract string Traverse(string x);
    public int[] Keys{
      get{ return _Keys; }
    }
    public long ID{
      get{ return _ID; }
    }
    public int NumKeys{
      get{ return _NumKeys; }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Creates a string made up of a number of spaces equal to the length of input minus four.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string Spacer(string input){
      string result = "";
      for(int i = 0; i < input.Length - 4; i++){
        result += " ";
      }
      return result;
    }
  }

  public abstract class BTreeNode<T>(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock) : Node<BTreeNode<T>,T>(degree, bufferBlock){
    protected T[] _Contents = new T[2 * degree - 1];

    public T[] Contents{
      get{ return _Contents; }
    }
  }
}