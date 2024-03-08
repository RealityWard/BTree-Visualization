using System.Threading.Tasks.Dataflow;
using BTreeVisualization;

namespace BPlusTreeVisualization
{

    public abstract class BPlusTreeNode<T> : BTreeNode<T>
    {
        public BPlusTreeNode(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, T[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock)
            : base(degree, bufferBlock)
        {
            
        }
    }
}