using System.Diagnostics;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
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
        private int rootHeight = 0; // Temporary to see if this works
        private GUINode oldRoot; // Temporary to see if this works
        private int setSpeed = 1;
        private GUINode lastSearched;


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

        private async void ScrollTimer_Tick(object? sender, EventArgs e)
        {
            scrollTimer.Stop();

            await Task.Run(() =>
            {
                panel1.Invalidate();
            });
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
                        if (!isFirstNodeEncountered && feedback.status == NodeStatus.Inserted)
                        {
                            Debug.WriteLine($"First node encountered: ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
                            isFirstNodeEncountered = true;
                            return;
                        }

                        setSpeed = trbSpeed.Value * 25;
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

                message.AppendLine($"ID: {nodePair.Key}, Keys: [{keysString}], IsLeaf: {node.IsLeaf}, IsRoot: {node.IsRoot}, Children: [{childrenIds}], Node Height: {node.height}");
                message.AppendLine(); // Add a new line between each printed node for easier reading
            }
            MessageBox.Show(message.ToString(), "Node Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProcessFeedback((NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents) feedback)
        {
            // INSERT
            if (feedback.status == NodeStatus.Insert)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Insert status.");
                lblCurrentProcess.Text = ("Received Insert status.");
            }

            // ISEARCH
            if (feedback.status == NodeStatus.ISearching)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received ISearching status.");
                lblCurrentProcess.Text = ("Received ISearching status.");
            }

            // INSERTED
            if (feedback.status == NodeStatus.Inserted)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Inserted status.");
                lblCurrentProcess.Text = ("Received Inserted status.");

                // Check if a key was successfully inserted or if the key is already in the tree
                if (feedback.numKeys == -1)
                {
                    Debug.WriteLine($"Key already found in tree in node ID={feedback.id}.");
                    MessageBox.Show("Key is already in the tree.");
                }
                else
                {
                    Debug.WriteLine($"Creating or updating GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
                    bool isLeaf = feedback.altID == 0;
                    bool isRoot = nodeDictionary.Count == 0; // Simplification, might need more accurate check
                    int height = 0; // Fix

                    if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
                    {
                        if (node != null)
                        {
                            Debug.WriteLine($"Node found. Updating ID={feedback.id}");
                            // Directly use feedback keys to update the node
                            node.Keys = feedback.keys;
                            node.NumKeys++;
                            node.NodeWidth = 40 * node.NumKeys;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}");
                        // Create new node with feedback keys
                        node = new GUINode(feedback.keys, isLeaf, isRoot, height, 1);
                        nodeDictionary[feedback.id] = node;
                    }

                    if (chkDebugMode.Checked == true) ShowNodesMessageBox();
                    UpdateGUITreeFromNodes();
                }
            }

            // SPLIT INSERT
            if (feedback.status == NodeStatus.SplitInsert)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received SplitInsert status.");
                lblCurrentProcess.Text = ("Received SplitInsert status.");

                if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && feedback.numKeys != -1)
                {
                    node.NumKeys = feedback.numKeys;
                    node.Keys = feedback.keys;
                    node.NodeWidth = 40 * feedback.numKeys;
                }
            }

            // NEW ROOT
            if (feedback.status == NodeStatus.NewRoot)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine($"A new root is being assigned. New root node ID={feedback.id}.");
                lblCurrentProcess.Text = ("A new root is being assigned.");

                Debug.WriteLine($"Creating new root GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
                bool isLeaf = false;
                bool isRoot = true;
                rootHeight++;
                Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}");
                // Create new node with feedback keys
                nodeDictionary[feedback.id] = new GUINode(feedback.keys, isLeaf, isRoot, rootHeight, feedback.numKeys);
                nodeDictionary[feedback.id] = nodeDictionary[feedback.id];
                oldRoot.IsRoot = false;

                if (chkDebugMode.Checked == true) ShowNodesMessageBox();
                UpdateGUITreeFromNodes();
            }

            // SPLIT
            if (feedback.status == NodeStatus.Split)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Split status.");
                lblCurrentProcess.Text = ("Received Split status.");

                Debug.WriteLine($"A split has occurred. Split node ID={feedback.id}. Preparing to handle new nodes.");
                if (nodeDictionary[feedback.id].IsRoot == true)
                {
                    oldRoot = nodeDictionary[feedback.id];
                    Debug.WriteLine($"Preparing node ID={feedback.id} to have root set to false.");
                }
            }

            // SPLIT RESULT
            if (feedback.status == NodeStatus.SplitResult)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received SplitResult status.");
                lblCurrentProcess.Text = ("Received SplitResult status.");

                Debug.WriteLine($"Creating or updating GUINode from split. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
                bool isLeaf = true;
                bool isRoot = false;
                int height = 0;

                if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
                {
                    if (node != null)
                    {
                        // Directly use feedback keys to update the node
                        node.Keys = feedback.keys;
                        node.NumKeys = feedback.numKeys;
                        node.NodeWidth = 40 * node.NumKeys;
                    }
                }
                else
                {
                    Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}");
                    // Create new node with feedback keys
                    node = new GUINode(feedback.keys, isLeaf, isRoot, height, feedback.numKeys);
                    nodeDictionary[feedback.id] = node;
                    if (feedback.altID != 0)
                    {
                        nodeDictionary[feedback.altID].Children.Add(nodeDictionary[feedback.id]);
                        node.height = Math.Max(0, nodeDictionary[feedback.altID].height - 1);
                    }
                }


                if (chkDebugMode.Checked == true) ShowNodesMessageBox();
                UpdateGUITreeFromNodes();
            }

            // DELETE
            if (feedback.status == NodeStatus.Delete)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Delete status.");
                lblCurrentProcess.Text = ("Received Delete status.");
            }


            // DELETED RANGE
            if (feedback.status == NodeStatus.DeleteRange)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received DeleteRange status.");
                lblCurrentProcess.Text = ("Received DeleteRange status.");
            }

            // DSEARCHING
            if (feedback.status == NodeStatus.DSearching)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received DSearching status.");
                lblCurrentProcess.Text = ("Received DSearching status.");
            }

            // DELETED
            if (feedback.status == NodeStatus.Deleted)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Deleted status.");
                lblCurrentProcess.Text = ("Received Deleted status.");

                // Check if a key was successfully deleted or if the key was not found
                if (feedback.numKeys == -1)
                {
                    Debug.WriteLine($"Key not found for deletion in node ID={feedback.id}.");
                    MessageBox.Show("Key is not in the tree.");
                }
                else
                {
                    if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
                    {
                        node.Keys = feedback.keys; // Assuming the backend sends the updated keys array
                        node.NumKeys = feedback.numKeys; // Update NumKeys based on the feedback
                        node.UpdateNodeWidth(); // Recalculate the node's width based on the updated number of keys
                        Debug.WriteLine($"Node ID={feedback.id} updated after key deletion. Remaining keys: {String.Join(", ", node.Keys)}");
                    }
                    else
                    {
                        Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update after deletion.");
                    }
                }

                if (chkDebugMode.Checked == true) ShowNodesMessageBox();
                UpdateGUITreeFromNodes();
            }

            // DELETED RANGE
            if (feedback.status == NodeStatus.DeletedRange)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received DeletedRange status.");
                lblCurrentProcess.Text = ("Received DeletedRange status.");
            }

            // REBALANCED
            if (feedback.status == NodeStatus.Rebalanced)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Rebalanced status.");
                lblCurrentProcess.Text = ("Received Rebalanced status.");
            }

            // FSEARCHING
            if (feedback.status == NodeStatus.FSearching)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received FSearching status.");
                lblCurrentProcess.Text = ("Received FSearching status.");
            }

            // FORFEIT
            if (feedback.status == NodeStatus.Forfeit)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Forfeit status.");
                lblCurrentProcess.Text = ("Received Forfeit status.");
            }

            // MERGE
            if (feedback.status == NodeStatus.Merge)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Merge status.");
                lblCurrentProcess.Text = ("Received Merge status.");
            }

            // MERGE PARENT
            if (feedback.status == NodeStatus.MergeParent)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received MergeParent status.");
                lblCurrentProcess.Text = ("Received MergeParent status.");
            }

            // UNDERFLOW
            if (feedback.status == NodeStatus.UnderFlow)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received UnderFlow status.");
                lblCurrentProcess.Text = ("Received UnderFlow status.");
            }

            /// SHIFT NEEDS A WAY TO REMOVE CHILDREN FROM A LIST AFTER A SPLIT TO PREVENT DUPLICATE NODES. (Possibly fixed, needs more testing)

            // SHIFT
            if (feedback.status == NodeStatus.Shift)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine($"Shift status received: AltID={feedback.altID}'s new parent is: ID={feedback.id}");
                lblCurrentProcess.Text = ("Shift status received.");

                GUINode childNode = nodeDictionary[feedback.altID];

                // Remove the child from its previous parent
                foreach (var kvp in nodeDictionary)
                {
                    GUINode parentNode = kvp.Value;
                    if (parentNode.Children != null && parentNode.Children.Contains(childNode))
                    {
                        parentNode.Children.Remove(childNode);
                        if (parentNode.Children.Count == 0)
                        {
                            parentNode.IsLeaf = true;
                        }
                        break;
                    }
                }

                // Add the child to the new parent
                if (nodeDictionary[feedback.id].Children == null)
                {
                    nodeDictionary[feedback.id].Children = new List<GUINode>() { childNode };
                }
                else
                {
                    nodeDictionary[feedback.id].Children.Add(childNode);
                    foreach (var child in nodeDictionary[feedback.id].Children)
                    {
                        child.height = Math.Max(0, nodeDictionary[feedback.id].height - 1);
                    }
                }
                nodeDictionary[feedback.id].IsLeaf = false;

                panel1.Invalidate();
            }

            // SEARCH
            if (feedback.status == NodeStatus.Search)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received Search status.");
                lblCurrentProcess.Text = ("Received Search status.");
            }

            // SEARCH RANGE
            if (feedback.status == NodeStatus.SearchRange)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received SearchRange status.");
                lblCurrentProcess.Text = ("Received SearchRange status.");
            }

            // SSEARCHING
            if (feedback.status == NodeStatus.SSearching)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received SSearching status.");
                lblCurrentProcess.Text = ("Received SSearching status.");
            }

            // FOUND
            if (feedback.status == NodeStatus.Found)
            {
                Debug.WriteLine("Received Found status.");

                if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
                {
                    node.highlighted = true;
                    lastSearched = node;
                    Debug.WriteLine($"Node ID={feedback.id} has been highlighted.");
                }
                panel1.Invalidate();

                lblCurrentProcess.Text = ("Received Found status.");
            }

            // FOUND RANGE
            if (feedback.status == NodeStatus.FoundRange)
            {
                Debug.WriteLine("Received FoundRange status.");
                lblCurrentProcess.Text = ("Received FoundRange status.");
            }

            // NODE DELETED
            if (feedback.status == NodeStatus.NodeDeleted)
            {
                // Unhighlight nodes if highlighted
                if (lastSearched != null)
                {
                    lastSearched.highlighted = false;
                }

                Debug.WriteLine("Received NodeDeleted status.");
                lblCurrentProcess.Text = ("Received NodeDeleted status.");
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
            Dictionary<int, int> heightNodesDrawn = new Dictionary<int, int>();

            // Use the stored tree for drawing
            panel1.SuspendLayout();
            _tree.DrawTree(e.Graphics, _tree.root, adjustedCenterX, adjustedCenterX, adjustedCenterY, width, heightNodesDrawn, _tree.root.height);
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

            // Clear input textbox
            txtInputData.ForeColor = Color.Black;
            txtInputData.Text = "Insert Data Here...";
        }

        private void btnInsertMany_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputData.Text))
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (int.TryParse(txtInputData.Text, out int keyToInsert))
            {
                if (keyToInsert <= 0) // Check if the key is non-positive
                {
                    MessageBox.Show("Please enter a valid non-zero, non-negative integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Debug.WriteLine($"Attempting to insert key: {keyToInsert}");

                // Check if it's the first node and it has not been processed yet
                if (!isFirstNodeEncountered)
                {
                    Debug.WriteLine("Skipping special command for the first node.");
                    return;
                }

                for (int i = 1; i < keyToInsert + 1; i++) // Note: This loop condition might need adjustment
                {
                    inputBuffer.Post((TreeCommand.Insert, i, new Person(keyToInsert.ToString())));
                }
            }
            else
            {
                MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Clear input textbox
            txtInputData.ForeColor = Color.Black;
            txtInputData.Text = "Insert Data Here...";
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

            // Clear input textbox
            txtInputData.ForeColor = Color.Black;
            txtInputData.Text = "Insert Data Here...";
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

            // Clear input textbox
            txtInputData.ForeColor = Color.Black;
            txtInputData.Text = "Insert Data Here...";
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            // THIS BELOW COULD BE NULLABLE STILL
            _tree = null!;
            int degree = 3; // Default value
            bool parseSuccess = false;
            if (cmbxMaxDegree.SelectedItem != null)
            {
                parseSuccess = Int32.TryParse(cmbxMaxDegree.SelectedItem.ToString(), out degree);
            }
            degree = parseSuccess ? degree : 3;
            nodeDictionary = new Dictionary<long, GUINode>();
            inputBuffer.Post((TreeCommand.Tree, degree, default(Person?)));
            panel1.Invalidate();
            rootHeight = 0; // Temporary to see if this works
            oldRoot = null; // Temporary to see if this works

        // Clear input textbox
        txtInputData.ForeColor = Color.Black;
        txtInputData.Text = "Insert Data Here...";
        lblCurrentProcess.Text = "No Tree Currently Being Processed";
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

        private void cmbxMaxDegree_SelectedIndexChanged(object sender, EventArgs e)
        {
            _tree = null!;
            int degree = 3; // Default value
            bool parseSuccess = false;
            if (cmbxMaxDegree.SelectedItem != null)
            {
                parseSuccess = Int32.TryParse(cmbxMaxDegree.SelectedItem.ToString(), out degree);
            }
            degree = parseSuccess ? degree : 3;
            nodeDictionary = new Dictionary<long, GUINode>();
            inputBuffer.Post((TreeCommand.Tree, degree, default(Person?)));
            panel1.Invalidate();
            rootHeight = 0; // Temporary to see if this works
            oldRoot = null; // Temporary to see if this works
            lblCurrentProcess.Text = "No Tree Currently Being Processed";
        }

        private void UpdateGUITreeFromNodes()
        {
            GUINode rootNode = DetermineRootNode();
            _tree = new GUITree(rootNode, panel1);
            //_tree.ResetAndInitializeLeafStart();
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
            throw new InvalidOperationException("No root node found.");
            //return null;
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
            BTree<Person> _Tree = new BTree<Person>(3, outputBuffer);
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
                                System.Threading.Thread.Sleep(setSpeed);
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
                                _Tree = new BTree<Person>(key, outputBuffer); // This may not be correct, but it works for now
                                Debug.WriteLine("Handling Tree command");
                                isFirstNodeEncountered = false;
                                break;
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