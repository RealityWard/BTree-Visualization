/**
Author: Tristan Anderson
Date: 2024-02-03
Desc: Maintains the entry point of the BTree data 
structure and initializes root and new node creation in the beginning.
*/
using System.Threading.Tasks.Dataflow;
namespace BTreeVisualization{
  public class BTree<T>(int degree, BufferBlock<(Status status, long id, int[] keys, T[] contents, long altID, int[] altKeys, T[] altContents)> bufferBlock)
  {
    private BTreeNode<T> _Root = new LeafNode<T>(degree,bufferBlock);
    private readonly int _Degree = degree;

    /// <summary>
    /// Author: Tristan Anderson
    /// Takes a node and the key and data to place into root.
    /// </summary>
    /// <param name="rNode"></param>
    private void Split(((int,T),BTreeNode<T>) rNode){
      _Root = new NonLeafNode<T>(_Degree,[rNode.Item1.Item1],[rNode.Item1.Item2],[_Root, rNode.Item2],bufferBlock);
    }
    
    /// <summary>
    /// Author: Tristan Anderson
    /// Inserts data at root node and set root to a new 
    /// leaf node if root isn't yet created. It also 
    /// checks for a split afterwards.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="data"></param>
    public void Insert(int key, T data){
      bufferBlock.Post((Status.Insert, 0, [], [], 0, [], []));
      if(Search(key) == null){
        ((int,T?),BTreeNode<T>?) result = _Root.InsertKey(key, data);
        if(result.Item2 != null && result.Item1.Item2 != null){
          #pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
          Split(result);
          #pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.
        }
      }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Date: 2024-02-18
    /// Invokes a delete through out the tree to find 
    /// an entry matching key and deletes it. If there 
    /// are duplicates only the first encountered is deleted. 
    /// If after the delete the root is reduced too 
    /// much it will grab its remaining child and make that the root.
    /// </summary>
    /// <param name="key"></param>
    public void Delete(int key){
      _Root.DeleteKey(key);
      if(_Root.NumKeys == 0 && _Root as NonLeafNode<T> != null){
        _Root = ((NonLeafNode<T>)_Root).Children[0];
      }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Using searchKey on the nodes to return the data 
    /// correlated to the key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public T? Search(int key){
      (int,BTreeNode<T>?) result = _Root.SearchKey(key);
      if (result.Item1 == -1 || result.Item2 == null){
        return default;
      }else{
        return result.Item2.Contents[result.Item1];
      }
    }

    /// <summary>
    /// Author: Tristan Anderson
    /// Invokes Traverse recusively through out the 
    /// tree to return a json print out of all nodes.
    /// </summary>
    /// <returns></returns>
    public string Traverse(){
      return _Root.Traverse("Root");
    }
    
    public void Clear(){
      _Root = new LeafNode<T>(_Degree,bufferBlock);
    }
  }
}