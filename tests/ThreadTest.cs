/**
Desc: For making new unit tests using NUnit module.
Reference: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-nunit
*/
using BTreeVisualization;
using NodeData;
using System.Threading.Tasks.Dataflow;

namespace ThreadTesting
{
  /// <summary>
  /// Tests for threading properly ordered messages. 
  /// </summary>
  /// <remarks>Author: Tristan Anderson</remarks>
  // [TestFixture(30)]
  public partial class ThreadTests(int degree)
  {
    Dictionary<NodeStatus, int>[] _StatusPrecedence = [];
    int[] _AcceptStates = [];
    private BufferBlock<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
    private BufferBlock<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
    private List<(NodeStatus status, long id, int numKeys, int[] keys
      , Person?[] contents, long altID, int altNumKeys, int[] altKeys
      , Person?[] altContents)> _OutputBufferHistory = [];
    private List<(TreeCommand action, int key, int endKey
      , Person? content)> _InputBufferHistory = [];
    List<int> _InsertedKeys = [];
    // Dictionary<TreeCommand, int> _CommandCount = [];
    private readonly int _NumberOfKeys = 100000;
    private Task? _Producer;
    private Task? _Consumer;

    /// <summary>
    /// NUnit setup for this class. 
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    [SetUp]
    public void Setup()
    {
      DefineStatusPrecedence();
      _OutputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 20 });
      _InputBuffer = new(new DataflowBlockOptions { BoundedCapacity = 10 });
      _OutputBufferHistory = [];
      _InputBufferHistory = [];
      _InsertedKeys = [];
      // _CommandCount = [];
      // foreach (TreeCommand treeCommand in Enum.GetValues(typeof(TreeCommand)))
      // {
      //   _CommandCount.Add(treeCommand, 0);
      // }
      _Producer = TreeProduce();
      _Consumer = GuiConsume();
    }

    /// <summary>
    /// Defines a structure for an NFA like processing of status messages.
    /// </summary>
    private void DefineStatusPrecedence()
    {
      int numOfStates = 121;
      _StatusPrecedence = new Dictionary<NodeStatus, int>[numOfStates];
      for (int i = 0; i < numOfStates; i++)
      {
        _StatusPrecedence[i] = [];
      }
      _StatusPrecedence[0].Add(NodeStatus.Insert, 1);
      _StatusPrecedence[0].Add(NodeStatus.Delete, 15);
      _StatusPrecedence[0].Add(NodeStatus.Search, 29);
      _StatusPrecedence[0].Add(NodeStatus.SearchRange, 32);
      _StatusPrecedence[0].Add(NodeStatus.DeleteRange, 35);
      _AcceptStates = [0, 71, 3, 7, 14, 17, 18, 19, 21, 31, 34, 37, 39, 41, 46, 47, 49, 50, 53, 55, 61, 70, 73, 74, 79, 82, 84, 85, 88, 93, 94, 95, 96, 97, 98, 101, 103, 105, 107, 108, 109, 110, 114, 115, 116, 118, 119, 120];
      _StatusPrecedence[0].Add(NodeStatus.Close, 71);
      _StatusPrecedence[1].Add(NodeStatus.ISearching, 2);
      _StatusPrecedence[2].Add(NodeStatus.ISearching, 2);
      _StatusPrecedence[2].Add(NodeStatus.Inserted, 3);
      _StatusPrecedence[3].Add(NodeStatus.Split, 4);
      _StatusPrecedence[4].Add(NodeStatus.SplitResult, 5);
      _StatusPrecedence[5].Add(NodeStatus.SplitResult, 6);
      _StatusPrecedence[6].Add(NodeStatus.SplitInsert, 7);
      _StatusPrecedence[7].Add(NodeStatus.Split, 8);
      _StatusPrecedence[8].Add(NodeStatus.SplitResult, 9);
      _StatusPrecedence[9].Add(NodeStatus.SplitResult, 10);
      _StatusPrecedence[10].Add(NodeStatus.Shift, 11);
      _StatusPrecedence[11].Add(NodeStatus.Shift, 11);
      _StatusPrecedence[11].Add(NodeStatus.SplitInsert, 7);
      _StatusPrecedence[11].Add(NodeStatus.NewRoot, 12);
      _StatusPrecedence[12].Add(NodeStatus.Shift, 13);
      _StatusPrecedence[13].Add(NodeStatus.Shift, 14);
      _StatusPrecedence[15].Add(NodeStatus.DSearching, 16);
      _StatusPrecedence[16].Add(NodeStatus.DSearching, 16);
      _StatusPrecedence[16].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[16].Add(NodeStatus.FSearching, 23);
      _StatusPrecedence[17].Add(NodeStatus.UnderFlow, 18);
      _StatusPrecedence[17].Add(NodeStatus.Merge, 20);
      _StatusPrecedence[18].Add(NodeStatus.Shift, 19);
      _StatusPrecedence[20].Add(NodeStatus.MergeParent, 21);
      _StatusPrecedence[21].Add(NodeStatus.Merge, 20);
      _StatusPrecedence[21].Add(NodeStatus.UnderFlow, 22);
      _StatusPrecedence[22].Add(NodeStatus.Shift, 19);
      _StatusPrecedence[23].Add(NodeStatus.FSearching, 23);
      _StatusPrecedence[23].Add(NodeStatus.Forfeit, 24);
      _StatusPrecedence[24].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[24].Add(NodeStatus.UnderFlow, 25);
      _StatusPrecedence[24].Add(NodeStatus.Merge, 26);
      _StatusPrecedence[25].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[26].Add(NodeStatus.MergeParent, 27);
      _StatusPrecedence[27].Add(NodeStatus.Deleted, 17);
      _StatusPrecedence[27].Add(NodeStatus.Merge, 26);
      _StatusPrecedence[27].Add(NodeStatus.UnderFlow, 28);
      _StatusPrecedence[28].Add(NodeStatus.Shift, 25);
      _StatusPrecedence[29].Add(NodeStatus.SSearching, 30);
      _StatusPrecedence[30].Add(NodeStatus.SSearching, 30);
      _StatusPrecedence[30].Add(NodeStatus.Found, 31);
      _StatusPrecedence[32].Add(NodeStatus.SSearching, 33);
      _StatusPrecedence[33].Add(NodeStatus.SSearching, 33);
      _StatusPrecedence[33].Add(NodeStatus.FoundRange, 34);
      _StatusPrecedence[34].Add(NodeStatus.SSearching, 33);
      _StatusPrecedence[34].Add(NodeStatus.FoundRange, 34);
      _StatusPrecedence[35].Add(NodeStatus.DSearching, 36);
      _StatusPrecedence[36].Add(NodeStatus.DSearching, 36);
      _StatusPrecedence[36].Add(NodeStatus.DeletedRange, 37);
      _StatusPrecedence[36].Add(NodeStatus.DeleteRangeSplit, 38);
      _StatusPrecedence[37].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[37].Add(NodeStatus.UnderFlow, 39);
      _StatusPrecedence[37].Add(NodeStatus.Merge, 40);
      _StatusPrecedence[37].Add(NodeStatus.NodeDeleted, 41);
      _StatusPrecedence[37].Add(NodeStatus.Shift, 42);
      _StatusPrecedence[37].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[38].Add(NodeStatus.DSearching, 44);
      _StatusPrecedence[39].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[39].Add(NodeStatus.UnderFlow, 39);
      _StatusPrecedence[39].Add(NodeStatus.Merge, 45);
      _StatusPrecedence[39].Add(NodeStatus.NodeDeleted, 45);
      _StatusPrecedence[39].Add(NodeStatus.Shift, 42);
      _StatusPrecedence[39].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[40].Add(NodeStatus.MergeParent, 46);
      _StatusPrecedence[41].Add(NodeStatus.NodeDeleted, 41);
      _StatusPrecedence[41].Add(NodeStatus.MergeParent, 46);
      _StatusPrecedence[42].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[43].Add(NodeStatus.Shift, 47);
      _StatusPrecedence[44].Add(NodeStatus.DSearching, 44);
      _StatusPrecedence[44].Add(NodeStatus.DeletedRange, 48);
      _StatusPrecedence[45].Add(NodeStatus.MergeParent, 49);
      _StatusPrecedence[46].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[46].Add(NodeStatus.UnderFlow, 50);
      _StatusPrecedence[46].Add(NodeStatus.Merge, 51);
      _StatusPrecedence[46].Add(NodeStatus.NodeDeleted, 45);
      _StatusPrecedence[46].Add(NodeStatus.Shift, 42);
      _StatusPrecedence[46].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[47].Add(NodeStatus.NodeDeleted, 47);
      _StatusPrecedence[48].Add(NodeStatus.DeletedRange, 52);
      _StatusPrecedence[48].Add(NodeStatus.NodeDeleted, 48);
      _StatusPrecedence[48].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[49].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[49].Add(NodeStatus.UnderFlow, 53);
      _StatusPrecedence[49].Add(NodeStatus.Merge, 54);
      _StatusPrecedence[49].Add(NodeStatus.NodeDeleted, 45);
      _StatusPrecedence[49].Add(NodeStatus.Shift, 42);
      _StatusPrecedence[49].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[50].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[50].Add(NodeStatus.UnderFlow, 39);
      _StatusPrecedence[50].Add(NodeStatus.Merge, 45);
      _StatusPrecedence[50].Add(NodeStatus.NodeDeleted, 45);
      _StatusPrecedence[50].Add(NodeStatus.Shift, 39);
      _StatusPrecedence[50].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[51].Add(NodeStatus.MergeParent, 55);
      _StatusPrecedence[52].Add(NodeStatus.DeletedRange, 52);
      _StatusPrecedence[52].Add(NodeStatus.Rebalanced, 52);
      _StatusPrecedence[52].Add(NodeStatus.UnderFlow, 56);
      _StatusPrecedence[52].Add(NodeStatus.Merge, 57);
      _StatusPrecedence[52].Add(NodeStatus.NodeDeleted, 58);
      _StatusPrecedence[52].Add(NodeStatus.Shift, 59);
      _StatusPrecedence[52].Add(NodeStatus.SplitInsert, 60);
      _StatusPrecedence[52].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[53].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[53].Add(NodeStatus.UnderFlow, 39);
      _StatusPrecedence[53].Add(NodeStatus.Merge, 45);
      _StatusPrecedence[53].Add(NodeStatus.NodeDeleted, 45);
      _StatusPrecedence[53].Add(NodeStatus.Shift, 39);
      _StatusPrecedence[53].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[54].Add(NodeStatus.MergeParent, 61);
      _StatusPrecedence[55].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[55].Add(NodeStatus.UnderFlow, 50);
      _StatusPrecedence[55].Add(NodeStatus.Merge, 51);
      _StatusPrecedence[55].Add(NodeStatus.NodeDeleted, 45);
      _StatusPrecedence[55].Add(NodeStatus.Shift, 42);
      _StatusPrecedence[55].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[56].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[56].Add(NodeStatus.UnderFlow, 78);
      _StatusPrecedence[56].Add(NodeStatus.Merge, 72);
      _StatusPrecedence[56].Add(NodeStatus.NodeDeleted, 66);
      _StatusPrecedence[57].Add(NodeStatus.DeletedRange, 52);
      _StatusPrecedence[57].Add(NodeStatus.Rebalanced, 52);
      _StatusPrecedence[57].Add(NodeStatus.UnderFlow, 56);
      _StatusPrecedence[57].Add(NodeStatus.Merge, 57);
      _StatusPrecedence[57].Add(NodeStatus.NodeDeleted, 58);
      _StatusPrecedence[57].Add(NodeStatus.MergeParent, 62);
      _StatusPrecedence[57].Add(NodeStatus.Shift, 59);
      _StatusPrecedence[57].Add(NodeStatus.SplitInsert, 60);
      _StatusPrecedence[58].Add(NodeStatus.DeletedRange, 52);
      _StatusPrecedence[58].Add(NodeStatus.NodeDeleted, 48);
      _StatusPrecedence[58].Add(NodeStatus.MergeParent, 62);
      _StatusPrecedence[58].Add(NodeStatus.Shift, 52);
      _StatusPrecedence[59].Add(NodeStatus.Rebalanced, 52);
      _StatusPrecedence[60].Add(NodeStatus.Shift, 52);
      _StatusPrecedence[61].Add(NodeStatus.Rebalanced, 37);
      _StatusPrecedence[61].Add(NodeStatus.UnderFlow, 53);
      _StatusPrecedence[61].Add(NodeStatus.Merge, 54);
      _StatusPrecedence[61].Add(NodeStatus.NodeDeleted, 45);
      _StatusPrecedence[61].Add(NodeStatus.Shift, 42);
      _StatusPrecedence[61].Add(NodeStatus.SplitInsert, 43);
      _StatusPrecedence[62].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[62].Add(NodeStatus.UnderFlow, 64);
      _StatusPrecedence[62].Add(NodeStatus.Merge, 65);
      _StatusPrecedence[62].Add(NodeStatus.NodeDeleted, 66);
      _StatusPrecedence[63].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[63].Add(NodeStatus.DeletedRange, 67);
      _StatusPrecedence[64].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[64].Add(NodeStatus.NodeDeleted, 48);
      _StatusPrecedence[64].Add(NodeStatus.Shift, 56);
      _StatusPrecedence[65].Add(NodeStatus.MergeParent, 68);
      _StatusPrecedence[66].Add(NodeStatus.DeletedRange, 52);
      _StatusPrecedence[66].Add(NodeStatus.NodeDeleted, 48);
      _StatusPrecedence[66].Add(NodeStatus.MergeParent, 69);
      _StatusPrecedence[67].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[67].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[68].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[68].Add(NodeStatus.UnderFlow, 64);
      _StatusPrecedence[68].Add(NodeStatus.Merge, 65);
      _StatusPrecedence[68].Add(NodeStatus.NodeDeleted, 48);
      _StatusPrecedence[69].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[69].Add(NodeStatus.UnderFlow, 71);
      _StatusPrecedence[69].Add(NodeStatus.Merge, 72);
      _StatusPrecedence[69].Add(NodeStatus.NodeDeleted, 48);
      _StatusPrecedence[70].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[70].Add(NodeStatus.Rebalanced, 73);
      _StatusPrecedence[70].Add(NodeStatus.UnderFlow, 93);
      _StatusPrecedence[70].Add(NodeStatus.Merge, 74);
      _StatusPrecedence[70].Add(NodeStatus.NodeDeleted, 75);
      _StatusPrecedence[70].Add(NodeStatus.Shift, 76);
      _StatusPrecedence[70].Add(NodeStatus.SplitInsert, 77);
      _StatusPrecedence[71].Add(NodeStatus.Shift, 78);
      _StatusPrecedence[72].Add(NodeStatus.MergeParent, 69);
      _StatusPrecedence[73].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[73].Add(NodeStatus.Merge, 80);
      _StatusPrecedence[73].Add(NodeStatus.NodeDeleted, 81);
      _StatusPrecedence[74].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[74].Add(NodeStatus.Merge, 80);
      _StatusPrecedence[74].Add(NodeStatus.NodeDeleted, 81);
      _StatusPrecedence[74].Add(NodeStatus.MergeParent, 82);
      _StatusPrecedence[75].Add(NodeStatus.MergeParent, 82);
      _StatusPrecedence[75].Add(NodeStatus.Shift, 73);
      _StatusPrecedence[76].Add(NodeStatus.Rebalanced, 73);
      _StatusPrecedence[77].Add(NodeStatus.Shift, 73);
      _StatusPrecedence[78].Add(NodeStatus.DSearching, 63);
      _StatusPrecedence[78].Add(NodeStatus.NodeDeleted, 48);
      _StatusPrecedence[79].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[79].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[79].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[80].Add(NodeStatus.MergeParent, 84);
      _StatusPrecedence[81].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[81].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[81].Add(NodeStatus.MergeParent, 84);
      _StatusPrecedence[82].Add(NodeStatus.UnderFlow, 85);
      _StatusPrecedence[82].Add(NodeStatus.Merge, 86);
      _StatusPrecedence[82].Add(NodeStatus.NodeDeleted, 87);
      _StatusPrecedence[83].Add(NodeStatus.MergeParent, 119);
      _StatusPrecedence[84].Add(NodeStatus.UnderFlow, 88);
      _StatusPrecedence[84].Add(NodeStatus.Merge, 89);
      _StatusPrecedence[84].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[85].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[85].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[85].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[85].Add(NodeStatus.Shift, 93);
      _StatusPrecedence[86].Add(NodeStatus.MergeParent, 94);
      _StatusPrecedence[87].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[87].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[87].Add(NodeStatus.MergeParent, 95);
      _StatusPrecedence[88].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[88].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[88].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[88].Add(NodeStatus.Shift, 79);
      _StatusPrecedence[89].Add(NodeStatus.MergeParent, 96);
      _StatusPrecedence[90].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[90].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[90].Add(NodeStatus.MergeParent, 97);
      _StatusPrecedence[91].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[91].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[91].Add(NodeStatus.MergeParent, 119);
      _StatusPrecedence[92].Add(NodeStatus.MergeParent, 97);
      _StatusPrecedence[93].Add(NodeStatus.UnderFlow, 98);
      _StatusPrecedence[93].Add(NodeStatus.Merge, 99);
      _StatusPrecedence[93].Add(NodeStatus.NodeDeleted, 100);
      _StatusPrecedence[94].Add(NodeStatus.UnderFlow, 101);
      _StatusPrecedence[94].Add(NodeStatus.Merge, 102);
      _StatusPrecedence[94].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[95].Add(NodeStatus.UnderFlow, 103);
      _StatusPrecedence[95].Add(NodeStatus.Merge, 104);
      _StatusPrecedence[95].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[96].Add(NodeStatus.UnderFlow, 88);
      _StatusPrecedence[96].Add(NodeStatus.Merge, 89);
      _StatusPrecedence[96].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[97].Add(NodeStatus.UnderFlow, 105);
      _StatusPrecedence[97].Add(NodeStatus.Merge, 106);
      _StatusPrecedence[97].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[98].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[99].Add(NodeStatus.MergeParent, 107);
      _StatusPrecedence[100].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[100].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[100].Add(NodeStatus.MergeParent, 107);
      _StatusPrecedence[101].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[101].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[101].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[101].Add(NodeStatus.Shift, 115);
      _StatusPrecedence[102].Add(NodeStatus.MergeParent, 108);
      _StatusPrecedence[103].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[103].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[103].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[103].Add(NodeStatus.Shift, 79);
      _StatusPrecedence[104].Add(NodeStatus.MergeParent, 109);
      _StatusPrecedence[105].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[105].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[105].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[105].Add(NodeStatus.Shift, 79);
      _StatusPrecedence[106].Add(NodeStatus.MergeParent, 110);
      _StatusPrecedence[107].Add(NodeStatus.UnderFlow, 111);
      _StatusPrecedence[107].Add(NodeStatus.Merge, 99);
      _StatusPrecedence[107].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[108].Add(NodeStatus.UnderFlow, 101);
      _StatusPrecedence[108].Add(NodeStatus.Merge, 102);
      _StatusPrecedence[108].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[109].Add(NodeStatus.UnderFlow, 103);
      _StatusPrecedence[109].Add(NodeStatus.Merge, 104);
      _StatusPrecedence[109].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[110].Add(NodeStatus.UnderFlow, 105);
      _StatusPrecedence[110].Add(NodeStatus.Merge, 106);
      _StatusPrecedence[110].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[111].Add(NodeStatus.Shift, 98);
      _StatusPrecedence[112].Add(NodeStatus.MergeParent, 114);
      _StatusPrecedence[113].Add(NodeStatus.DeletedRange, 70);
      _StatusPrecedence[113].Add(NodeStatus.NodeDeleted, 67);
      _StatusPrecedence[113].Add(NodeStatus.MergeParent, 114);
      _StatusPrecedence[114].Add(NodeStatus.UnderFlow, 116);
      _StatusPrecedence[114].Add(NodeStatus.Merge, 117);
      _StatusPrecedence[114].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[115].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[115].Add(NodeStatus.Merge, 112);
      _StatusPrecedence[115].Add(NodeStatus.NodeDeleted, 113);
      _StatusPrecedence[116].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[116].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[116].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[116].Add(NodeStatus.Shift, 79);
      _StatusPrecedence[117].Add(NodeStatus.MergeParent, 118);
      _StatusPrecedence[118].Add(NodeStatus.UnderFlow, 116);
      _StatusPrecedence[118].Add(NodeStatus.Merge, 117);
      _StatusPrecedence[118].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[119].Add(NodeStatus.UnderFlow, 120);
      _StatusPrecedence[119].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[119].Add(NodeStatus.NodeDeleted, 81);
      _StatusPrecedence[120].Add(NodeStatus.UnderFlow, 79);
      _StatusPrecedence[120].Add(NodeStatus.Merge, 92);
      _StatusPrecedence[120].Add(NodeStatus.NodeDeleted, 90);
      _StatusPrecedence[120].Add(NodeStatus.Shift, 73);
    }

    /// <summary>
    /// Task creation for the tree object.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the tree.</returns>
    private async Task TreeProduce()
    {
      BTree<Person> _Tree = new(degree, _OutputBuffer);
      Random random = new();
      int key = 0;
      for (int i = 0; i < _NumberOfKeys; i++)
      {
        do
        {
          key = random.Next(1, _NumberOfKeys * 1000);
        } while (_InsertedKeys.Contains(key));
        _Tree.Insert(key, new Person(key.ToString()));
        _InsertedKeys.Add(key);
      }
      _Tree.Search(key);
      while (await _InputBuffer.OutputAvailableAsync())
      {
        _InputBufferHistory.Add(_InputBuffer.Receive());
        switch (_InputBufferHistory.Last().action)
        {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          case TreeCommand.Tree:
            _Tree = new(_InputBufferHistory.Last().key, _OutputBuffer);
            break;
          case TreeCommand.Insert:
            _Tree.Insert(_InputBufferHistory.Last().key, _InputBufferHistory.Last().content);
            break;
          case TreeCommand.Delete:
            _Tree.Delete(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.DeleteRange:
            _Tree.DeleteRange(_InputBufferHistory.Last().key, _InputBufferHistory.Last().endKey);
            break;
          case TreeCommand.Search:
            _Tree.Search(_InputBufferHistory.Last().key);
            break;
          case TreeCommand.SearchRange:
            _Tree.Search(_InputBufferHistory.Last().key, _InputBufferHistory.Last().endKey);
            break;
          case TreeCommand.Traverse:
            Console.Write(_Tree.Traverse());
            break;
          case TreeCommand.Close:
            _InputBuffer.Complete();
            _Tree.Close();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            Console.Write("TreeCommand:{0} not recognized", _InputBufferHistory.Last().action);
            break;
        }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        // if (_CommandCount.ContainsKey(_InputBufferHistory.Last().action))
        //   _CommandCount[_InputBufferHistory.Last().action]++;
      }
    }

    /// <summary>
    /// Task creation for the fake consumer "GUI" object.
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks>
    /// <returns>Task running the GUI consumer.</returns>
    private async Task GuiConsume()
    {
      bool setupConsumed = false;
      while (await _OutputBuffer.OutputAvailableAsync() && !setupConsumed)
      {
        (NodeStatus status, long id, int numKeys, int[] keys
          , Person?[] contents, long altID, int altNumKeys, int[] altKeys
          , Person?[] altContents) = _OutputBuffer.Receive();
        setupConsumed = status == NodeStatus.Found;
      }
      while (await _OutputBuffer.OutputAvailableAsync())
      {
        _OutputBufferHistory.Add(_OutputBuffer.Receive());
        switch (_OutputBufferHistory.Last().status)
        {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
          case NodeStatus.Insert:
            break;
          case NodeStatus.ISearching:
            break;
          case NodeStatus.Inserted:
            break;
          case NodeStatus.Split:
            break;
          case NodeStatus.Delete:
            break;
          case NodeStatus.DSearching:
            break;
          case NodeStatus.Deleted:
            break;
          case NodeStatus.FSearching:
            break;
          case NodeStatus.Forfeit:
            break;
          case NodeStatus.Merge:
            break;
          case NodeStatus.MergeParent:
            break;
          case NodeStatus.UnderFlow:
            break;
          case NodeStatus.Shift:
            break;
          case NodeStatus.Search:
            break;
          case NodeStatus.SSearching:
            break;
          case NodeStatus.Found:
            break;
          case NodeStatus.FoundRange:
            break;
          case NodeStatus.Close:
            _OutputBuffer.Complete();
            break;
          default:// Will close buffer upon receiving a bad TreeCommand.
            // Console.Write("TreeCommand:{0} not recognized", _OutputBufferHistory.Last().status);
            break;
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        }
      }
    }

    /// <summary>
    /// Simply test insertion times
    /// </summary>
    /// <remarks>Author: Tristan Anderson</remarks> 
    // [TestCase(100)]
    // [TestCase(1000)]
    public async Task PresedenceTesting(int x)
    {
      Random random = new();
      int key;
      int inputBufferCount = 0;
      List<(int, int)> mixKeys = [];
      for (int i = 0; i < x; i++)
      {
        key = random.Next(0, 4);
        // Randomly choose insert, delete, or search
        if (key == 0)
        {
          for (int j = 0; j < 10; j++)
          {
            do
            {
              key = random.Next(1, _NumberOfKeys * 1000);
            } while (_InsertedKeys.Contains(key));
            await _InputBuffer.SendAsync((TreeCommand.Insert, key, -1
              , new Person(key.ToString())));
            _InsertedKeys.Add(key);
            mixKeys.Add((1, key));
          }
          inputBufferCount += 9;
        }
        else if (key == 1)
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.Delete, key, -1
            , null));
          _InsertedKeys.Remove(key);
          mixKeys.Add((0, key));
        }
        else if (key == 2)
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.SearchRange, key, key + 10
            , null));
        }
        else
        {
          key = _InsertedKeys[random.Next(1, _InsertedKeys.Count)];
          await _InputBuffer.SendAsync((TreeCommand.DeleteRange, key, key + 10000
            , null));
          for (int l = 0; l < 9; l++)
          {
            _InsertedKeys.Remove(key);
            mixKeys.Add((0, key++));
          }
        }
        inputBufferCount++;
      }
      await _InputBuffer.SendAsync((TreeCommand.Close, 0, -1, null));
#pragma warning disable CS8602 // Dereference of a possibly null reference.
      _Producer.Wait();
      _Consumer.Wait();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
      Assert.That(_InputBufferHistory, Has.Count.EqualTo(inputBufferCount + 1), "Not all commands are getting through.");
      NodeStatus lastStatus = NodeStatus.Close;
      int k = -1;
      int state = 0;
      string acceptingDfaStates = "";
      int countOfAccepts = 0;
      _OutputBufferHistory.ForEach((bufMessage) =>
      {
        // if (bufMessage.status == NodeStatus.FoundRange)
        // {
        //   Console.WriteLine("Here:" + StringifyKeys(bufMessage.numKeys,bufMessage.keys));
        // }
        acceptingDfaStates += $"{countOfAccepts}, {state}, {bufMessage.status}\n";
        if (_StatusPrecedence[state].ContainsKey(bufMessage.status))
        {
          state = _StatusPrecedence[state][bufMessage.status];
        }
        else if (_AcceptStates.Contains(state) && _StatusPrecedence[0].ContainsKey(bufMessage.status))
        {
          countOfAccepts++;
          state = _StatusPrecedence[0][bufMessage.status];
        }
        else
        {
          Assert.Fail($"State:{state}\n " +
            $"Status:{bufMessage.status}\n " +
            $"Last Status:{lastStatus}\nid: " +
            $"{bufMessage.id}\nkey:{k}\nkeys:" +
            $"{StringifyKeys(bufMessage.numKeys, bufMessage.keys)}\n" +
            $"History:{acceptingDfaStates}");
        }
        lastStatus = bufMessage.status;
      });
    }

    /// <summary>
    /// Read out just the portion of the keys[] currently in use.
    /// </summary>
    /// <param name="numKeys">Index to stop at.</param>
    /// <param name="keys">Array of ints</param>
    /// <returns>String of the keys seperated by a ','</returns>
    private static string StringifyKeys(int numKeys, int[] keys)
    {
      string result = "";
      for (int i = 0; i < numKeys; i++)
        result += keys[i] + (i + 1 == numKeys ? "" : ",");
      return result;
    }
  }
}