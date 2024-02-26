/**
Primary Author: Tristan Anderson
Secondary Author: Andreas Kramer (for tree height methods)
Date: 2024-02-03
Desc: Maintains the entry point of the BTree data 
structure and initializes root and new node creation in the beginning.
*/
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks.Dataflow;
namespace BTreeVisualization{
  public class BTree<T>(int degree, BufferBlock<(Status status, long id, int[] keys, T[] contents, long altID, int[] altKeys, T[] altContents)> bufferBlock)
  {
    private BTreeNode<T> _Root = new LeafNode<T>(degree,bufferBlock);
    private readonly int _Degree = degree;
    private bool zeroKeyUsed = false;

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
      if((key == 0 && !zeroKeyUsed) || key != 0){
        if(key == 0) // Due to initializing all int[] entries as zero instead of null
          zeroKeyUsed = true; // We must use a boolean to detect whether or not to allow a zero key insertion
        bufferBlock.Post((Status.Insert, 0, [], [], 0, [], []));
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
      if(key == 0 && zeroKeyUsed)
        zeroKeyUsed = false; // After deletion there will no longer be a zero key in use, thus must re-enable insertion of zero
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

    /// <summary>
    /// Author: Andreas Kramer
    /// Calculates the Height of the B-Tree and returns it as an integer, assuming it is correctly balanced
    /// </summary>
    /// <returns></returns>

    public int GetTreeHeight(){
      if(_Root == null){
        return 0;
      }
      int height = 1;
      BTreeNode<T> currentNode = _Root;
      while(currentNode is NonLeafNode<T> nonLeafNode && nonLeafNode.Children.Length > 0)
      {
        currentNode = nonLeafNode.Children[0];
        height++;
      }
      return height;
    }  

    /// <summary>
    /// Author: Andreas Kramer
    /// Calculates the Minimum Height of the B-Tree and returns it as an integer
    /// </summary>
    /// <returns></returns>
    /// 
    public int GetMinHeight(){
      return GetMinHeight(_Root,0);
    }

    private int GetMinHeight(BTreeNode<T> node, int currentLevel){
      if(node == null || node is LeafNode<T>){
        return currentLevel;
      }
      if(node is NonLeafNode<T> nonLeafNode){
        int minHeight = currentLevel;
        foreach(var child in nonLeafNode.Children){
          int childHeight = GetMinHeight(child, currentLevel + 1);
          if(childHeight < minHeight){
            minHeight = childHeight;
          }
        }
      return minHeight;     
      }
      return currentLevel;
    }

    /// <summary>
    /// Author: Andreas Kramer
    /// Calculates the Maximum Height of the B-Tree and returns it as an integer
    /// </summary>
    /// <returns></returns>
    /// 
    public int GetMaxHeight(){
      return GetMaxHeight(_Root,0);
    }

    private int GetMaxHeight(BTreeNode<T> node, int currentLevel){
      if(node == null || node is LeafNode<T>){
        return currentLevel;
      }
      if(node is NonLeafNode<T> nonLeafNode){
        int maxHeight = currentLevel;
        foreach(var child in nonLeafNode.Children){
          int childHeight = GetMinHeight(child, currentLevel + 1);
          if(childHeight > maxHeight){
            maxHeight = childHeight;
          }
        }
      return maxHeight;     
      }
      return currentLevel;
    }
    /// <summary>
    /// Author: Andreas Kramer
    /// Alternate version of getting the maximum height of the tree, returns it as an integer
    /// </summary>
    /// <returns></returns>
    /// 

    public int GetMaxTreeHeightAlternate(BTreeNode<T> node){
      if(node == null){
        return 0;
      }
      if(node is LeafNode<T>){
        return 1;
      }
      NonLeafNode<T> nonLeafNode = (NonLeafNode<T>)node;
      int maxHeight = 0;
      foreach(var child in nonLeafNode.Children){
        int childHeight = GetMaxTreeHeightAlternate(child);
        if(childHeight > maxHeight){
            maxHeight = childHeight;
        }
      }
      return maxHeight + 1;
    }

    /// <summary>
    /// Author: Andreas Kramer
    /// Method to check if the leafNodes of the tree are all on the same level (checking for tree balance)
    /// </summary>
    /// <returns></returns>
    /// 

    public bool IsBalanced() {
        if (_Root == null) {
            return true; 
        }
        int leafLevel = -1; 
        return CheckNodeBalance(_Root, 0, ref leafLevel);
    }
    private bool CheckNodeBalance(BTreeNode<T> node, int currentLevel, ref int leafLevel) {
        if (node is LeafNode<T>) {
            if (leafLevel == -1) {
                leafLevel = currentLevel;
                return true;
            }
            return currentLevel == leafLevel;
        }

        if (node is NonLeafNode<T> nonLeafNode) {
            foreach (var child in nonLeafNode.Children) {
                if (!CheckNodeBalance(child, currentLevel + 1, ref leafLevel)) {
                    return false; // If any subtree is unbalanced, the entire tree is unbalanced.
                }
            }
        }
        return true; // If all subtrees are balanced, then this subtree is also balanced.
    }
  }
}