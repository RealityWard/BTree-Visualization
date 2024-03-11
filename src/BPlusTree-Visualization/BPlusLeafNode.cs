using System.Threading.Tasks.Dataflow;
using ThreadCommunication;

namespace BPlusTreeVisualization{

    public  class BPlusLeafNode<T>(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, 
            T?[] contents, long altID, int altNumKeys, int[] altKeys, T[] altContents)> bufferBlock)    
            : BPlusTreeNode<T>(degree, bufferBlock)
    {
        private BPlusLeafNode<T>? _NextNode;

        private BPlusLeafNode<T>? _PrevNode;

        public BPlusLeafNode(int degree, int[] keys, T[] contents,
                BufferBlock<(Status status, long id, int numKeys, int[] keys,
                T?[] contents, long altID, int altNumKeys, int[] altKeys,
                T[] altContents)> bufferBlock) : this(degree, bufferBlock)
            {
            _NumKeys = keys.Length;
            for (int i = 0; i < keys.Length; i++)
                {
                    _Keys[i] = keys[i];
                    _Contents[i] = contents[i];
                }
            }
        private int Search(int key){
            for (int i = 0; i < _NumKeys; i++)
            {
                if (_Keys[i] == key)
                {
                    //_BufferBlock.Post((Status.Found, ID, i, [key], [Contents[i]], 0, -1, [], []));
                    return i;
                }
            }
            _BufferBlock.Post((Status.Found, ID, -1, [], [], 0, -1, [], []));
            return -1;
        }

        public override (int, BPlusTreeNode<T>) SearchKey(int key){
            _BufferBlock.Post((Status.SSearching, ID, -1, [], [], 0, -1, [], []));
            return (Search(key), this);
        }

        public override ((int, T), BPlusTreeNode<T>) Split(){
            int[] newKeys = new int[_Degree - 1];
            T[] newContent = new T[_Degree - 1];
            for (int i = 0; i < _Degree - 1; i++){
                newKeys[i] = _Keys[i + _Degree];
                newContent[i] = _Contents[i + _Degree];
            }

            _NumKeys = _Degree - 1;
            BPlusLeafNode<T> newNode = new(_Degree, newKeys, newContent, _BufferBlock);
            _BufferBlock.Post((Status.Split, ID, NumKeys, Keys, Contents, newNode.ID,
                          newNode.NumKeys, newNode.Keys, newNode.Contents));
            newNode._PrevNode = this;
            _NextNode = newNode;
            return ((_Keys[_NumKeys], _Contents[_NumKeys]), newNode);
        }

        public override ((int, T?), BPlusTreeNode<T>?) InsertKey(int key, T data){
            _BufferBlock.Post((Status.ISearching, ID, -1, [], [], 0, -1, [], []));
            int i = 0;
            while (i < _NumKeys && key > _Keys[i])
                i++;
                if (i == _NumKeys || key != _Keys[i] || key == 0)
                {
                for (int j = _NumKeys - 1; j >= i; j--){
                    _Keys[j + 1] = _Keys[j];
                    _Contents[j + 1] = _Contents[j];
                }
            _Keys[i] = key;
            _Contents[i] = data;
            _NumKeys++;
            _BufferBlock.Post((Status.Inserted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
            if (IsFull())
            {
            return Split();
            }
            }
            else
            {
                _BufferBlock.Post((Status.Inserted, 0, -1, [], [], 0, -1, [], []));
            }
            return ((-1, default(T)), null);
        }

        public override void DeleteKey(int key){
            _BufferBlock.Post((Status.DSearching, ID, -1, [], [], 0, -1, [], []));
            int i = Search(key);
            if (i != -1)
            {
            for (; i < _NumKeys; i++)
            {
                _Keys[i] = _Keys[i + 1];
                _Contents[i] = _Contents[i + 1];
            }
            _NumKeys--;
            _BufferBlock.Post((Status.Deleted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
            }
            _BufferBlock.Post((Status.Deleted, ID, -1, [], [], 0, -1, [], []));
        }


        public override (int, T) ForfeitKey(){
            _NumKeys--;
            _BufferBlock.Post((Status.Forfeit, ID, NumKeys, Keys, Contents, 0, -1, [], []));
            return (_Keys[_NumKeys], _Contents[_NumKeys]);
        }


        public void Merge(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling) {
            _Keys[_NumKeys] = dividerKey;
            _Contents[_NumKeys] = dividerData;
            _NumKeys++;
            for (int i = 0; i < sibiling.NumKeys; i++){
                _Keys[_NumKeys + i] = sibiling.Keys[i];
                //_Contents[_NumKeys + i] = sibiling.Contents[i];
            }
            _NumKeys += sibiling.NumKeys;
            _BufferBlock.Post((Status.Merge, ID, NumKeys, Keys, Contents, sibiling.ID, -1, [], []));
        }

        public void GainsFromRight(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling)
        {
            _Keys[_NumKeys] = dividerKey;
            _Contents[_NumKeys] = dividerData;
            _NumKeys++;
        }   

        public override void LosesToLeft(){
            for (int i = 0; i < _NumKeys - 1; i++){
                _Keys[i] = _Keys[i + 1];
                _Contents[i] = _Contents[i + 1];
            }
            _NumKeys--;
        }   

        public void GainsFromLeft(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling){
            for (int i = _NumKeys - 1; i >= 0; i--){
                _Keys[i + 1] = _Keys[i];
                _Contents[i + 1] = _Contents[i];
            }
            _NumKeys++;
            _Keys[0] = dividerKey;
            _Contents[0] = dividerData;
        }

        public override void LosesToRight(){
            _NumKeys--;
        }

        public override string Traverse(string x){
            string output = Spacer(x) + "{\n";
            output += Spacer(x) + "  \"leafnode\":\"" + x + "\",\n" 
            + Spacer(x) + "\"  ID\":" + _ID + ",\n"
            + Spacer(x) + "  \"keys\":[";
            for (int i = 0; i < _NumKeys; i++){
                output += _Keys[i] + (i + 1 < _NumKeys ? "," : "");
            }
            output += "],\n" + Spacer(x) + "  \"contents\":[";
            for (int i = 0; i < _NumKeys; i++){
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                output += _Contents[i].ToString() + (i + 1 < _NumKeys ? "," : "");
                #pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            return output + "]\n" + Spacer(x) + "}";
        }










    }
}