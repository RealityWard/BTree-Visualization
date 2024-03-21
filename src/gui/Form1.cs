using System.Diagnostics;
using System.Text;
using System.Threading.Tasks.Dataflow;
using BTreeVisualization;
using ThreadCommunication;

namespace B_TreeVisualizationGUI
{
    public partial class Form1 : Form
    {
        // Global variables
        int scrollableWidth = 5000;
        int scrollableHeight = 5000;
        private GUITree _tree;
        Dictionary<long, GUINode> nodeDictionary = new Dictionary<long, GUINode>();
        private System.Windows.Forms.Timer scrollTimer;
        private bool isFirstNodeEncountered = false;
        int split = 0;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Form1()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            InitializeComponent();
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            scrollTimer = new System.Windows.Forms.Timer();
            scrollTimer.Interval = 200;
            scrollTimer.Tick += ScrollTimer_Tick;
        }

        private async void ScrollTimer_Tick(object sender, EventArgs e)
        {
            scrollTimer.Stop();

            await Task.Run(() =>
            {
                panel1.Invalidate();
            });
        }

        private void InitializeOrResetTree()
        {
            // EXAMPLE
            int[] rootKeys = { 0 }; // Placeholder
            GUINode rootNode = new GUINode(rootKeys, true, true, 0, 0, new GUINode[0]);
            _tree = new GUITree(rootNode, panel1);
            panel1.Invalidate();
        }

        private void StartConsumerTask()
        {
            // Task that listens to the outputBuffer and updates the GUI accordingly
            Task.Run(async () =>
            {
                while (await outputBuffer.OutputAvailableAsync())
                {
                    var feedback = await outputBuffer.ReceiveAsync();
                    this.Invoke((MethodInvoker)delegate
                    {
                        // Ignore feedback with ID of 0
                        if (feedback.id == 0)
                        {
                            Debug.WriteLine("Ignoring feedback with ID of 0.");
                            return;
                        }

                        if (!isFirstNodeEncountered && feedback.status == NodeStatus.Inserted)
                        {
                            Debug.WriteLine($"First node encountered: ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
                            isFirstNodeEncountered = true;
                            return;
                        }

                        ProcessFeedback(feedback);
                    });
                }
            });
        }

        private void ShowNodesMessageBox()
        {
            StringBuilder message = new StringBuilder();
            message.AppendLine("Node Dictionary Contents:");
            foreach (var nodePair in nodeDictionary)
            {
                var node = nodePair.Value;
                // Keys of the current node
                string keysString = string.Join(", ", node.Keys);
                // IDs of children
                string childrenIds = node.Children != null ? string.Join(", ", node.Children.Select(child => child.GetHashCode().ToString())) : "None";

                message.AppendLine($"ID: {nodePair.Key}, Keys: [{keysString}], IsLeaf: {node.IsLeaf}, IsRoot: {node.IsRoot}, Children: [{childrenIds}]");
                message.AppendLine(); // Add a new line between each printed node for easier reading
            }

            MessageBox.Show(message.ToString(), "Node Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProcessFeedback((NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents) feedback)
        {
            Debug.WriteLine($"Feedback received: Status={feedback.status}, ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
            if (feedback.status == NodeStatus.Split)
            {
                split = 2;
            }

            // Check for Node Splits
            if (feedback.status == NodeStatus.Split || split > 0)
            {
                Debug.WriteLine($"A split has occurred. Split node ID={feedback.id}. Preparing to handle new nodes.");

                /*// Update the existing (old) node with the keys and children provided in feedback
                if (nodeDictionary.TryGetValue(feedback.id, out GUINode oldNode))
                {
                    oldNode.Keys = feedback.keys;
                    oldNode.NumKeys = feedback.numKeys;
                    // If feedback provides info about children, update them as well:
                    //oldNode.Children = feedback.Children;
                    //oldNode.IsLeaf = feedback should determine if the node is now a leaf
                }

                // Create the new (split) node and add it to the dictionary
                if (!nodeDictionary.ContainsKey(feedback.altID))
                {
                    bool isLeaf = oldNode.IsLeaf; // Assuming the new node has the same leaf status as the old node.
                    int depth = oldNode.IsRoot ? oldNode.Depth + 1 : oldNode.Depth; // If the old node was the root, increment depth, otherwise keep the same.

                    GUINode newNode = new GUINode(feedback.altKeys, isLeaf, false, depth, feedback.altNumKeys);
                    nodeDictionary[feedback.altID] = newNode;
                    nodeDictionary[feedback.id].IsLeaf = false;
                    int size;
                    if (nodeDictionary[feedback.id].Children == null)
                    {
                        size = 1;
                    }
                    else
                    {
                        size = nodeDictionary[feedback.id].Children.Length + 1;
                    }
                    nodeDictionary[feedback.id].Children = new GUINode[size];
                    nodeDictionary[feedback.id].Children[size - 1] = newNode;
                    Debug.WriteLine($"New node created for split. ID={feedback.altID}, Keys={String.Join(", ", newNode.Keys)}");
                    split--;
                }*/

                // Update the existing (old) node with the keys and children provided in feedback
                if (nodeDictionary.TryGetValue(feedback.id, out GUINode oldNode))
                {
                    oldNode.Keys = feedback.keys;
                    oldNode.NumKeys = feedback.numKeys;
                    // If feedback provides info about children, update them as well:
                    //oldNode.Children = feedback.Children;
                    //oldNode.IsLeaf = feedback should determine if the node is now a leaf
                }

                // Create the new (split) node and add it to the dictionary
                if (!nodeDictionary.ContainsKey(feedback.altID))
                {
                    bool isLeaf = false;
                    GUINode[] children = new GUINode[2];
                    int depth = oldNode.IsRoot ? oldNode.Depth + 1 : oldNode.Depth; // If the old node was the root, increment depth, otherwise keep the same.

                    GUINode newNode = new GUINode(feedback.altKeys, isLeaf, false, depth, feedback.altNumKeys);
                    nodeDictionary[feedback.altID] = newNode;
                    nodeDictionary[feedback.id].IsLeaf = false;
                    int size;
                    if (nodeDictionary[feedback.id].Children == null)
                    {
                        size = 1;
                    }
                    else
                    {
                        size = nodeDictionary[feedback.id].Children.Length + 1;
                    }
                    nodeDictionary[feedback.id].Children = new GUINode[size];
                    nodeDictionary[feedback.id].Children[size - 1] = newNode;
                    Debug.WriteLine($"New node created for split. ID={feedback.altID}, Keys={String.Join(", ", newNode.Keys)}");
                    split--;

                    // Update the GUI tree structure, which might include finding and updating the parent node
                    UpdateGUITreeFromNodes();
                }

                ShowNodesMessageBox();
            }
            else if (feedback.status == NodeStatus.ISearching)
            {
                // Handle ISearching status
                // This status should not trigger any node creation or updates
                Debug.WriteLine("Received ISearching status. Ignoring...");
            }
            else
            {
                // Handle other feedback statuses (Insert, Inserted, Delete, Deleted, etc.)
                // Create or update GUINode based on feedback
                CreateOrUpdateGUINode(feedback);
                ShowNodesMessageBox();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeBackend();
            StartConsumerTask();

            // Create horizontal scroll bar
            System.Windows.Forms.ScrollBar hScrollBar1 = new HScrollBar();
            hScrollBar1.Dock = DockStyle.Bottom;

            hScrollBar1.Scroll += (s, ea) =>
            {
                panel1.HorizontalScroll.Value = hScrollBar1.Value;
                scrollTimer.Start(); // Start the timer when scrolling occurs
            };

            System.Windows.Forms.ScrollBar vScrollBar1 = new VScrollBar();
            vScrollBar1.Dock = DockStyle.Right;

            vScrollBar1.Scroll += (s, ea) =>
            {
                panel1.VerticalScroll.Value = vScrollBar1.Value;
                scrollTimer.Start(); // Start the timer when scrolling occurs
            };

            // Add scroll bars to panel
            panel1.Controls.Add(vScrollBar1);
            panel1.Controls.Add(hScrollBar1);
            panel1.AutoScrollMinSize = new Size(scrollableWidth, scrollableHeight);
            panel1.AutoScrollPosition = new Point((panel1.AutoScrollMinSize.Width - panel1.ClientSize.Width) / 2, 0);

            //InitializeTree(); // Initialize the test tree when the form loads
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (_tree == null) return; // Check if the tree is initialized

            // Adjustments for drawing
            float adjustedCenterX = scrollableWidth / 2 - panel1.HorizontalScroll.Value;
            float adjustedCenterY = 10 - panel1.VerticalScroll.Value;

            // Calculate tree width
            float width = _tree.CalculateSubtreeWidth(_tree.root);

            // Reset and initialize leafStart before drawing
            _tree.ResetAndInitializeLeafStart();

            // Initialize dictionary
            Dictionary<int, int> depthNodesDrawn = new Dictionary<int, int>();

            // Use the stored tree for drawing
            panel1.SuspendLayout();
            _tree.DrawTree(e.Graphics, _tree.root, adjustedCenterX, adjustedCenterX, adjustedCenterY, width, depthNodesDrawn);
            panel1.ResumeLayout(true);
        }

        public int[] generateKeys(int numKeys)
        {
            Random random = new Random();
            int[] keys = new int[random.Next(1, numKeys + 1)];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = random.Next(5000);
            }
            return keys;
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputData.Text))
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (int.TryParse(txtInputData.Text, out int keyToInsert))
            {
                Debug.WriteLine($"Attempting to insert key: {keyToInsert}");

                // Check if it's the first node and it has not been processed yet
                if (!isFirstNodeEncountered)
                {
                    Debug.WriteLine("Skipping special command for the first node.");
                    return;
                }

                inputBuffer.Post((TreeCommand.Insert, keyToInsert, new Person(keyToInsert.ToString())));
            }
            else
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputData.Text))
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (int.TryParse(txtInputData.Text, out int keyToDelete))
            {
                Debug.WriteLine($"Attempting to delete key: {keyToDelete}");
                inputBuffer.Post((TreeCommand.Delete, keyToDelete, null));
            }
            else
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputData.Text))
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (int.TryParse(txtInputData.Text, out int keyToSearch))
            {
                Debug.WriteLine($"Attempting to search for key: {keyToSearch}");
                inputBuffer.Post((TreeCommand.Search, keyToSearch, null));
            }
            else
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            //InitializeOrResetTree();
            _tree = null;
            panel1.Invalidate();
        }

        private void txt_txtInputData_Enter(object sender, EventArgs e)
        {
            if (txtInputData.Text == "Insert Data Here...")
            {
                txtInputData.ForeColor = Color.Black;
                txtInputData.Text = "";
            }
        }

        private void txt_txtInputData_Leave(object sender, EventArgs e)
        {
            if (txtInputData.Text.Length == 0)
            {
                txtInputData.ForeColor = Color.Black;
                txtInputData.Text = "Insert Data Here...";
            }
        }

        /* ----------------- BELOW THIS IS ALL IN TESTING ----------------- */

        /*private void CreateOrUpdateGUINode((NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents) feedback)
        {
            Debug.WriteLine($"Creating or updating GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
            bool isLeaf = feedback.altID == 0;
            bool isRoot = nodeDictionary.Count == 0; // Note: This might not accurately identify the root
            int depth = 0; // TODO: Determine actual depth
            GUINode[] children = new GUINode[0]; // TODO: Determine actual children

            if (nodeDictionary.TryGetValue(feedback.id, out GUINode node))
            {
                Debug.WriteLine($"Node found. Updating ID={feedback.id}");
                //node.Keys = feedback.keys;
                if (feedback.keys.Length > 0)
                {
                    int[] newKeys = new int[node.Keys.Length + 1];
                    for (int i = 0; i < node.Keys.Length; i++)
                    {
                        newKeys[i] = node.Keys[i];
                    }
                    newKeys[newKeys.Length - 1] = feedback.keys[0];
                    node.Keys = newKeys;
                    node.NumKeys++;
                    GUINode newNode = new GUINode(newKeys, nodeDictionary[feedback.id].IsLeaf, nodeDictionary[feedback.id].IsRoot, nodeDictionary[feedback.id].Depth, nodeDictionary[feedback.id].Children);
                    nodeDictionary[feedback.id] = newNode;
                }

            }
            else
            {
                Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}");
                node = new GUINode(feedback.keys, isLeaf, isRoot, depth, children);
                nodeDictionary[feedback.id] = node;
            }
            UpdateGUITreeFromNodes();
        }*/

        private void CreateOrUpdateGUINode((NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents) feedback)
        {
            Debug.WriteLine($"Creating or updating GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
            bool isLeaf = feedback.altID == 0;
            bool isRoot = nodeDictionary.Count == 0; // Simplification, might need more accurate check
            int depth = 0; // Simplification, should determine actual depth

            if (nodeDictionary.TryGetValue(feedback.id, out GUINode node))
            {
                Debug.WriteLine($"Node found. Updating ID={feedback.id}");
                // Directly use feedback keys to update the node
                node.Keys = feedback.keys;
                node.NumKeys++;
                node.NodeWidth = 40 * node.NumKeys;
            }
            else
            {
                Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}");
                // Create new node with feedback keys
                node = new GUINode(feedback.keys, isLeaf, isRoot, depth, 1);
                nodeDictionary[feedback.id] = node;
            }

            // Assuming you have logic here to update GUI or tree visualization
            UpdateGUITreeFromNodes();
        }

        private void UpdateGUITreeFromNodes()
        {
            GUINode rootNode = DetermineRootNode();
            _tree = new GUITree(rootNode, panel1);
            panel1.Invalidate();
        }

        private GUINode DetermineRootNode()
        {
            foreach (var node in nodeDictionary)
            {
                if (node.Value.IsRoot)
                {
                    return node.Value;
                }
            }
            return null;
        }

        // Define the Person class
        public class Person
        {
            public string Name { get; set; }

            public Person(string name)
            {
                Name = name;
            }
        }

        BufferBlock<(
          NodeStatus status,
          long id,
          int numKeys,
          int[] keys,
          Person?[] contents,
          long altID,
          int altNumKeys,
          int[] altKeys,
          Person?[] altContents
          )> outputBuffer = new();

        private BufferBlock<(
          TreeCommand action,
          int key,
          Person? content
          )> inputBuffer = new();

        private void InitializeBackend()
        {
            BTree<Person> _Tree = new BTree<Person>(3, outputBuffer); // Change 3 to be the degree
            Task producer = Task.Run(async () =>
            {
                Thread.CurrentThread.Name = "Producer";
                try
                {
                    while (await inputBuffer.OutputAvailableAsync())
                    {
                        (TreeCommand action, int key, Person? content) = inputBuffer.Receive();
                        switch (action)
                        {
                            case TreeCommand.Insert:
                                _Tree.Insert(key, content ?? throw new NullContentReferenceException("Insert on tree with null content."));
                                break;
                            case TreeCommand.Delete:
                                _Tree.Delete(key);
                                break;
                            case TreeCommand.Search:
                                _Tree.Search(key);
                                break;
                            case TreeCommand.Close:
                                inputBuffer.Complete();
                                break;
                            case TreeCommand.Tree:
                                // Insert degree from listbox here and it will generate a new tree
                            default:
                                Debug.WriteLine("TreeCommand:{0} not recognized", action);
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in Producer task: {ex.Message}");
                }
                finally
                {
                    // Complete the buffer when done processing commands
                    outputBuffer.Complete();
                }
            });
        }
    }
}