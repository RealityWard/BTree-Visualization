using System.Collections.Concurrent;
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
    private GUINode lastSearched;
    private ConcurrentQueue<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)> messageQueue;
    private bool isProcessing = false;
    private int setSpeed = 1;
    private bool isConsumerTaskRunning = false;

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

    private Task StartConsumerTask()
    {
      messageQueue = new ConcurrentQueue<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>();
      bool isProcessing = false;

      _ = Task.Run(async () =>
      {
        while (true) // Maybe should use a cancellation token for a graceful shutdown
        {
          if (await outputBuffer.OutputAvailableAsync())
          {
            var feedback = await outputBuffer.ReceiveAsync();

            // Create a deep copy of the keys and altKeys arrays to ensure they are not modified elsewhere
            var feedbackCopy = (
                      feedback.status,
                      feedback.id,
                      feedback.numKeys,
                      feedback.keys.Clone() as int[],
                      feedback.contents,
                      feedback.altID,
                      feedback.altNumKeys,
                      feedback.altKeys.Clone() as int[],
                      feedback.altContents
                  );

            messageQueue.Enqueue(feedbackCopy);

            if (!isProcessing)
            {
              isProcessing = true;
              // Disable the button on the UI thread
              this.Invoke((MethodInvoker)delegate
                    {
                      btnSearch.Enabled = false;
                    });

              _ = Task.Run(async () =>
                    {
                      while (messageQueue.TryDequeue(out var messageToProcess))
                      {
                        Invoke((MethodInvoker)delegate
                              {
                                if (!isFirstNodeEncountered && messageToProcess.status == NodeStatus.Inserted)
                                {
                                  Debug.WriteLine($"First node encountered: ID={messageToProcess.id}, Keys={string.Join(", ", messageToProcess.keys)}");
                                  isFirstNodeEncountered = true;
                                  return; // Skip further processing for this message
                                }
                                ProcessFeedback(messageToProcess);
                              });

                        int delay = (int)this.Invoke(new Func<int>(() => Math.Max(trbSpeed.Value * 50, 100)));
                        await Task.Delay(delay);
                      }

                      isProcessing = false;
                      // Disable the button on the UI thread
                      this.Invoke((MethodInvoker)delegate
                            {
                              btnSearch.Enabled = true;
                            });
                    });
            }
          }
        }
      });

      return Task.CompletedTask;
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
      switch (feedback.status)
      {
        // INSERT
        case NodeStatus.Insert:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Insert status."); // For debug purposes DELETE LATER
            break;
          }

        // ISEARCHING
        case NodeStatus.ISearching:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received ISearching status."); // For debug purposes DELETE LATER
            lblCurrentProcess.Text = ($"Searching for an adaquate node to add input key to."); // Inform user of what process is currently happening                                                                                //UpdateVisuals(); // Update the panel to show changes FIX
            break;
          }

        // INSERTED
        case NodeStatus.Inserted:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Inserted status."); // For debug purposes DELETE LATER
            lblCurrentProcess.Text = ("Inserting key."); // Inform user of what process is currently happening
                                                         // Check if input key is already in the tree
            if (feedback.numKeys == -1)
            {
              Debug.WriteLine($"Key already found in tree in node ID={feedback.id}."); // For debug purposes DELETE LATER
              MessageBox.Show("Input key is already in the tree.");
            }
            else
            {
              Debug.WriteLine($"Creating or updating GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}"); // For debug purposes DELETE LATER
                                                                                                                           // Check if node already exists in the dictionary
              if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
              {
                Debug.WriteLine($"Node found. Updating ID={feedback.id}"); // For debug purposes DELETE LATER
                                                                           // Update node
                node.Keys = feedback.keys;
                node.NumKeys++;
                node.NodeWidth = 40 * node.NumKeys;
              }
              else
              {
                Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}"); // For debug purposes DELETE LATER
                                                                                         // Create new node
                bool isRoot = nodeDictionary.Count == 0;
                node = new GUINode(feedback.id, feedback.keys, true, isRoot, 0, 1);
                nodeDictionary[feedback.id] = node;
              }
              if (chkDebugMode.Checked == true) ShowNodesMessageBox(); // For debug purposes DELETE LATER
              UpdateVisuals(); // Update the panel to show changes
            }
            break;
          }

        // SPLIT INSERT
        case NodeStatus.SplitInsert:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received SplitInsert status."); // For debug purposes DELETE LATER
            lblCurrentProcess.Text = ("Add later.");
            // Check if node is in the dictionary and is not a duplicate
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && feedback.numKeys != -1)
            {
              // Update node
              node.NumKeys = feedback.numKeys;
              node.Keys = feedback.keys;
              node.NodeWidth = 40 * feedback.numKeys;
            }
            break;
          }

        // NEW ROOT
        case NodeStatus.NewRoot:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine($"A new root is being assigned. New root node ID={feedback.id}."); // For debug purposes DELETE LATER
            lblCurrentProcess.Text = ("A new root is being assigned."); // Inform user of what process is currently happening
            Debug.WriteLine($"Creating new root GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}"); // For debug purposes DELETE LATER
            rootHeight++; // Update global root height
                          // Create new node 
            bool isLeaf = false;
            bool isRoot = true;
            nodeDictionary[feedback.id] = new GUINode(feedback.id, feedback.keys, isLeaf, isRoot, rootHeight, feedback.numKeys);
            oldRoot.IsRoot = false; // Update the old root to not say it's a root anymore
            if (chkDebugMode.Checked == true) ShowNodesMessageBox(); // For debug purposes DELETE LATER
            UpdateVisuals(); // Update the panel to show changes
            break;
          }

        // SPLIT
        case NodeStatus.Split:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Split status."); // For debug purposes DELETE LATER
            lblCurrentProcess.Text = ("Splitting node."); // Inform user of what process is currently happening
            Debug.WriteLine($"A split has occurred. Split node ID={feedback.id}. Preparing to handle new nodes."); // For debug purposes DELETE LATER
                                                                                                                   // Check if node is the root
            if (nodeDictionary[feedback.id].IsRoot == true)
            {
              oldRoot = nodeDictionary[feedback.id]; // Update oldRoot global
            }
            break;
          }

        // SPLIT RESULT
        case NodeStatus.SplitResult:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received SplitResult status."); // For debug purposes DELETE LATER
            Debug.WriteLine($"Creating or updating GUINode from split. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}"); // For debug purposes DELETE LATER
                                                                                                                                    // Check if node exists in the dictionary
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
            {
              if (node != null) // Null check
              {
                lblCurrentProcess.Text = ("Updating node."); // Inform user of what process is currently happening
                                                             // Update node
                node.Keys = feedback.keys;
                node.NumKeys = feedback.numKeys;
                node.NodeWidth = 40 * node.NumKeys;
              }
            }
            else
            {
              Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}"); // For debug purposes DELETE LATER
              lblCurrentProcess.Text = ("Creating new node."); // Inform user of what process is currently happening
                                                               // Create new node
              bool isLeaf = true;
              bool isRoot = false;
              int height = 0;
              node = new GUINode(feedback.id, feedback.keys, isLeaf, isRoot, height, feedback.numKeys);
              nodeDictionary[feedback.id] = node; // Add node to dictionary
                                                  // Update the parent node's children list to include the new node
              if (feedback.altID != 0)
              {
                nodeDictionary[feedback.altID].Children.Add(nodeDictionary[feedback.id]);
                node.height = Math.Max(0, nodeDictionary[feedback.altID].height - 1);
              }
            }
            if (chkDebugMode.Checked == true) ShowNodesMessageBox(); //  For debug purposes DELETE LATER
            UpdateVisuals(); // Update the panel to show changes
            break;
          }

        // DELETE
        case NodeStatus.Delete:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Delete status."); // For debug purposes DELETE LATER
                                                        // ADD ANIMATION HERE?
            break;
          }

        // DELETED RANGE
        case NodeStatus.DeleteRange:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Delete Range status."); // For debug purposes DELETE LATER
            break;
          }

        // DSEARCHING
        case NodeStatus.DSearching:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received DSearching status."); // For debug purposes DELETE LATER
            break;
          }

        // DELETED
        case NodeStatus.Deleted:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Deleted status."); // For debug purposes DELETE LATER
                                                         // Check if feedback is holding a key DELETE LATER?
            if (feedback.numKeys == -1)
            {
              Debug.WriteLine($"Key not found for deletion in node ID={feedback.id}.");
              MessageBox.Show("Key is not in the tree.");
            }
            else
            {
              // Check if key exists in the dictionary
              if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
              {
                lblCurrentProcess.Text = ("Deleting input key."); // Inform user of what process is currently happening
                                                                  // Update node
                node.Keys = feedback.keys;
                node.NumKeys = feedback.numKeys;
                node.UpdateNodeWidth();
                Debug.WriteLine($"Node ID={feedback.id} updated after key deletion. Remaining keys: {String.Join(", ", node.Keys)}");
              }
              else
              {
                Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update after deletion.");
              }
            }
            if (chkDebugMode.Checked == true) ShowNodesMessageBox();//  For debug purposes DELETE LATER
            UpdateVisuals(); // Update the panel to show changes
            break;
          }

        // DELETE RANGE
        case NodeStatus.DeletedRange:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Deleted Range status."); // For debug purposes DELETE LATER
            break;
          }

        // REBALANCED
        case NodeStatus.Rebalanced:
          {
            UnhighlightSearched(); // Unhighlight any previously searched nodes
            Debug.WriteLine("Received Rebalanced status."); // For debug purposes DELETE LATER
            break;
          }

        // UNKNOWN STATUS
        default:
          {
            Debug.WriteLine("Unknown status recieved.");
            break;
          }
      }

      // FSEARCHING
      if (feedback.status == NodeStatus.FSearching)
      {
        UnhighlightSearched();

        Debug.WriteLine("Received FSearching status.");
        lblCurrentProcess.Text = ("Received FSearching status.");
      }

      // FORFEIT
      if (feedback.status == NodeStatus.Forfeit)
      {
        UnhighlightSearched();

        if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
        {
          node.Keys = feedback.keys; // Update keys array
          node.NumKeys = feedback.numKeys; // Update NumKeys
          node.UpdateNodeWidth(); // Recalculate the node's width based on the updated number of keys
          Debug.WriteLine($"Node ID={feedback.id} updated after key deletion. Remaining keys: {String.Join(", ", node.Keys)}");
        }
        else
        {
          Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update after deletion.");
        }

        Debug.WriteLine("Received Forfeit status.");
        lblCurrentProcess.Text = ("Received Forfeit status.");
        UpdateGUITreeFromNodes();
      }

      // MERGE
      if (feedback.status == NodeStatus.Merge || feedback.status == NodeStatus.MergeRoot)
      {
        UnhighlightSearched();

        Debug.WriteLine("Received Merge status.");
        lblCurrentProcess.Text = ("Received Merge status.");

        if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
        {
          node.Keys = feedback.keys; // Update keys array
          node.NumKeys = feedback.numKeys; // Update NumKeys
          if(feedback.status == NodeStatus.MergeRoot)
            node.IsRoot = true;
          node.UpdateNodeWidth(); // Recalculate the node's width based on the updated number of keys
          Debug.WriteLine($"Node ID={feedback.id} updated. Remaining keys: {String.Join(", ", node.Keys)}");
        }
        else
        {
          Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update.");
        }

        if (nodeDictionary.TryGetValue(feedback.altID, out GUINode? sibling) && sibling != null)
        {
          if (feedback.status == NodeStatus.Merge)
          {
            if (nodeDictionary[feedback.altID].IsRoot)
            {
              nodeDictionary[feedback.id].IsRoot = true;
              rootHeight--;
            }
            if (sibling.Children != null)
            {
              if (node.Children == null)
              {
                List<GUINode> children = new List<GUINode>();
                children.AddRange(sibling.Children);
                node.Children = children;
              }
              else
              {
                nodeDictionary[feedback.id].Children.AddRange(sibling.Children);
              }
            }
          }
          nodeDictionary.Remove(feedback.altID); // Delete sibling from dicitonary
          Debug.WriteLine($"Node ID={feedback.altID} deleted.");
        }
        else
        {
          Debug.WriteLine($"Node with ID={feedback.altID} not found when attempting to update after deletion.");
        }
        UpdateGUITreeFromNodes();
      }

      // MERGE PARENT
      if (feedback.status == NodeStatus.MergeParent)
      {
        UnhighlightSearched();

        Debug.WriteLine("Received MergeParent status.");
        lblCurrentProcess.Text = ("Received MergeParent status.");

        if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
        {
          node.Keys = feedback.keys; // Update keys array
          node.NumKeys = feedback.numKeys; // Update NumKeys
          node.UpdateNodeWidth(); // Recalculate the node's width based on the updated number of keys
          Debug.WriteLine($"Node ID={feedback.id} updated. Remaining keys: {String.Join(", ", node.Keys)}");
        }
        else
        {
          Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update.");
        }
        UpdateGUITreeFromNodes();
      }

      // UNDERFLOW
      if (feedback.status == NodeStatus.UnderFlow)
      {
        UnhighlightSearched();

        Debug.WriteLine("Received UnderFlow status.");
        lblCurrentProcess.Text = ("Received UnderFlow status.");

        if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
        {
          node.Keys = feedback.keys; // Update keys array
          node.NumKeys = feedback.numKeys; // Update NumKeys
          node.UpdateNodeWidth(); // Recalculate the node's width based on the updated number of keys
          Debug.WriteLine($"Node ID={feedback.id} updated. Remaining keys: {String.Join(", ", node.Keys)}");
        }
        else
        {
          Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update.");
        }

        if (nodeDictionary.TryGetValue(feedback.altID, out GUINode? sibling) && sibling != null)
        {
          nodeDictionary.Remove(feedback.altID); // Delete sibling from dicitonary
          Debug.WriteLine($"Node ID={feedback.altID} deleted.");
        }
        else
        {
          Debug.WriteLine($"Node with ID={feedback.altID} not found when attempting to update after deletion.");
        }
        UpdateGUITreeFromNodes();
      }

      /// SHIFT NEEDS A WAY TO REMOVE CHILDREN FROM A LIST AFTER A SPLIT TO PREVENT DUPLICATE NODES. (Possibly fixed, needs more testing)

      // SHIFT
      if (feedback.status == NodeStatus.Shift)
      {
        UnhighlightSearched();

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
        UnhighlightSearched();

        Debug.WriteLine("Received Search status.");
        lblCurrentProcess.Text = ("Received Search status.");
      }

      // SEARCH RANGE
      if (feedback.status == NodeStatus.SearchRange)
      {
        UnhighlightSearched();

        Debug.WriteLine("Received SearchRange status.");
        lblCurrentProcess.Text = ("Received SearchRange status.");
      }

      // SSEARCHING
      if (feedback.status == NodeStatus.SSearching)
      {
        UnhighlightSearched();

        Debug.WriteLine("Received SSearching status.");
        lblCurrentProcess.Text = ("Received SSearching status.");
      }

      // FOUND
      if (feedback.status == NodeStatus.Found)
      {
        Debug.WriteLine("Received Found status.");

        if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
        {
          node.Searched = true;
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
          lastSearched.Searched = false;
        }

        Debug.WriteLine("Received NodeDeleted status.");
        lblCurrentProcess.Text = ("Received NodeDeleted status.");
        nodeDictionary.Remove(feedback.id);
        if (nodeDictionary.TryGetValue(feedback.altID, out GUINode? node) && feedback.numKeys == -1)
        {
          // Update node
          for (int i = 0; i < node.Children.Count; i++)
          {
            if (node.Children[i].ID == feedback.id)
            {
              node.Children.RemoveAt(i);
            }
          }
          node.NodeWidth = 40 * feedback.numKeys;
        }
      }
    }

    private void UnhighlightSearched()
    {
      if (lastSearched != null)
      {
        lastSearched.Searched = false;
      }
    }

    private void UpdateVisuals()
    {
      UpdateGUITreeFromNodes();
      panel1.Invalidate();
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

    private async void btnInsertMany_Click(object sender, EventArgs e)
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
          int delay = (int)this.Invoke(new Func<int>(() => Math.Max(trbSpeed.Value * 50, 100)));
          await Task.Delay(delay);

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
      isProcessing = true;
      messageQueue = new ConcurrentQueue<(NodeStatus, long, int, int[], Person?[], long, int, int[], Person?[])>();
      Task.Run(() =>
      {
        Thread.Sleep(100);
        isProcessing = false;
      });

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
      isFirstNodeEncountered = false;

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
                //await Task.Delay(setSpeed);
                //await Task.Delay(trbSpeed.Value);
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