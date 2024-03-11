using System.Threading.Tasks.Dataflow;
using ThreadCommunication;

namespace BPlusTreeVisualization{

    public  class BPlusLeafNode<T>(int degree, BufferBlock<(Status status, long id, int numKeys, int[] keys, 
            T?[] contents, long altID, int altNumKeys, int[] altKeys, T?[] altContents)> bufferBlock)    
            : BPlusTreeNode<T>(degree, bufferBlock)
    {
        private BPlusLeafNode<T>? _NextNode;

        private BPlusLeafNode<T>? _PrevNode;


        public BPlusLeafNode(int degree, int[] keys, T[] contents,
                BufferBlock<(Status status, long id, int numKeys, int[] keys,
                T?[] contents, long altID, int altNumKeys, int[] altKeys,
                T?[] altContents)> bufferBlock) : this(degree, bufferBlock)
            {
            _NumKeys = keys.Length;
            for (int i = 0; i < keys.Length; i++)
                {
                    _Keys[i] = keys[i];
                    _Contents[i] = contents[i];
                }
                _NextNode = null;
                _PrevNode = null;
            }
        private int Search(int key){
            for (int i = 0; i < _NumKeys; i++)
            {
                if (_Keys[i] == key)
                {
                    _BufferBlock.SendAsync((Status.Found, ID, i, [key], [Contents[i]], 0, -1, [], []));
                    return i;
                }
            }
            _BufferBlock.SendAsync((Status.Found, ID, -1, [], [], 0, -1, [], []));
            return -1;
        }

        public override (int, BPlusTreeNode<T>) SearchKey(int key){
            _BufferBlock.SendAsync((Status.SSearching, ID, -1, [], [], 0, -1, [], []));
            return (Search(key), this);
        }

        public override ((int, T), BPlusTreeNode<T>) Split(){
            int[] newKeys = new int[_Degree - 1];
            T[] newContent = new T[_Degree - 1];
            for (int i = 0; i < _Degree - 1; i++){
                newKeys[i] = _Keys[i + _Degree];
                newContent[i] = _Contents[i + _Degree] 
                ?? throw new NullContentReferenceException(
                $"Content at index:{i + _Degree} within node:{ID}");
                _Keys[i + _Degree] = default;
                _Contents[i + _Degree] = default;
            }
            _NumKeys = _Degree - 1;
            BPlusLeafNode<T> newNode = new(_Degree, newKeys, newContent, _BufferBlock);
            _BufferBlock.SendAsync((Status.Split, ID, NumKeys, Keys, Contents, newNode.ID,
                          newNode.NumKeys, newNode.Keys, newNode.Contents));
            newNode._NextNode = _NextNode;
            _NextNode = newNode;
            newNode._PrevNode = this;
            if(newNode._NextNode != null){
                newNode._NextNode._PrevNode = newNode;
            }
            (int,T) dividerEntry = (_Keys[_NumKeys], _Contents[_NumKeys] 
            ?? throw new NullContentReferenceException(
            $"Content at index:{NumKeys} within node:{ID}"));
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
            return (dividerEntry, newNode);
        }

        public override ((int, T?), BPlusTreeNode<T>?) InsertKey(int key, T data){
            _BufferBlock.SendAsync((Status.ISearching, ID, -1, [], [], 0, -1, [], []));
            int i = 0;
            while (i < _NumKeys && key > _Keys[i])
                i++;
            if (i == _NumKeys || key != _Keys[i] || key == 0){
                for (int j = _NumKeys - 1; j >= i; j--){
                    _Keys[j + 1] = _Keys[j];
                    _Contents[j + 1] = _Contents[j];
                }
                _Keys[i] = key;
                _Contents[i] = data;
                _NumKeys++;
                _BufferBlock.SendAsync((Status.Inserted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
                if (IsFull()){
                    return Split();
                }
            }
            else{
                _BufferBlock.SendAsync((Status.Inserted, 0, -1, [], [], 0, -1, [], []));
      }
            return ((-1, default(T)), null);
        }

        public override void DeleteKey(int key){
            _BufferBlock.SendAsync((Status.DSearching, ID, -1, [], [], 0, -1, [], []));
            int i = Search(key);
            if (i != -1)
            {
            for (; i < _NumKeys; i++)
            {
                _Keys[i] = _Keys[i + 1];
                _Contents[i] = _Contents[i + 1];
            }
            _NumKeys--;
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
            _BufferBlock.SendAsync((Status.Deleted, ID, NumKeys, Keys, Contents, 0, -1, [], []));
            }
            _BufferBlock.SendAsync((Status.Deleted, ID, -1, [], [], 0, -1, [], []));
        }


        public override (int, T) ForfeitKey(){
            _NumKeys--;
            (int,T) keyToBeLost = (_Keys[_NumKeys], _Contents[_NumKeys] 
            ?? throw new NullContentReferenceException(
            $"Content at index:{_NumKeys} within node:{ID}"));
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
            _BufferBlock.SendAsync((Status.Forfeit, ID, NumKeys, Keys, Contents, 0, -1, [], []));
            return keyToBeLost;
        }


        public override void Merge(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling){
            _Keys[_NumKeys] = dividerKey;
            _Contents[_NumKeys] = dividerData;
            _NumKeys++;
            for (int i = 0; i < sibiling.NumKeys; i++){
                _Keys[_NumKeys + i] = sibiling.Keys[i];
                _Contents[_NumKeys + i] = sibiling.Contents[i];
            }
            _NumKeys += sibiling.NumKeys;
            _BufferBlock.SendAsync((Status.Merge, ID, NumKeys, Keys, Contents, sibiling.ID, -1, [], []));
        }

        public override void GainsFromRight(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling)
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
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
        }

        public override void GainsFromLeft(int dividerKey, T dividerData, BPlusTreeNode<T> sibiling){
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
            _Keys[_NumKeys] = default;
            _Contents[_NumKeys] = default;
        }

        public override string Traverse(string x){
            string output = Spacer(x) + "{\n";
            output += Spacer(x) + "  \"leafnode\":\"" + x + "\",\n" 
            + Spacer(x) + "\"  ID\":" + _ID + ",\n"
            //+ Spacer(x) + "\" Prev\":" + _PrevNode.ID           
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
            output += "\n";
            if(_NextNode != null){
                output += Spacer(x) + "\"  Next\":" + _NextNode._ID + ",\n";
            }
            if(_PrevNode != null){
                output += Spacer(x) + "\"  Prev\":" + _PrevNode._ID + ",\n";
            }
            return output + "]\n" + Spacer(x) + "}";
        }
    }
}