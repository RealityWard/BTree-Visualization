using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using System.Windows.Forms;
using BPlusTreeVisualization;
using BTreeVisualization;
using ThreadCommunication;
using BTreeVisualizationNode;
using Microsoft.VisualBasic;

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
    private bool isFirstNodeEncountered = true;
    private int rootHeight = 0; // Temporary to see if this works
    private GUINode oldRoot; // Temporary to see if this works
    private GUINode lastSearched;
    private ConcurrentQueue<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)> messageQueue;
    private bool isProcessing = false;
    private int animationSpeed;
    private bool animate = true;
    private Task Animation;
    private Task _Producer;
    private bool isConsumerTaskRunning = false;
    private long lastHighlightedID;
    private long lastHighlightedAltID;
    private long altShiftHighlightID;
    private bool seenShift = false;
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    // Statuses we don't want to delay the animations
    private readonly HashSet<NodeStatus> _delayRequiringStatuses = new HashSet<NodeStatus>
        {
            NodeStatus.Inserted,
            NodeStatus.SplitInsert,
            NodeStatus.Deleted,
            NodeStatus.Forfeit,
            NodeStatus.MergeParent,
            NodeStatus.UnderFlow,
            NodeStatus.Merge,
            NodeStatus.Shift,
            NodeStatus.SSearching,
            NodeStatus.FSearching,
            NodeStatus.Delete,
            NodeStatus.ISearching,
            NodeStatus.DSearching
        };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public Form1()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
      InitializeComponent();
      SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
      scrollTimer = new System.Windows.Forms.Timer();
      scrollTimer.Interval = 200;
      scrollTimer.Tick += ScrollTimer_Tick;

      this.Resize += new EventHandler(Form1_Resize); // Subscribe to the resize event
      PositionPanels(); // Initial positioning of the panels
    }

    private async void ScrollTimer_Tick(object? sender, EventArgs e)
    {
      scrollTimer.Stop();

      await Task.Run(() =>
      {
        panel1.Invalidate();
      });
    }

    private async Task Animator()
    {
      btnNext.Enabled = false;
      while (animate)
      {
        await ConsumeOneOutput();
        await Task.Delay(animationSpeed);
      }
      btnNext.Enabled = true;
      return;
    }

    private async Task StartConsumerTask()
    {
      while (messageQueue.TryDequeue(out var messageToProcess))
      {
        if (nodeDictionary.TryGetValue(lastHighlightedID, out GUINode? node))
        {
          node.nodeHighlighted = false;
          node.lineHighlighted = false;
        }
        if (nodeDictionary.TryGetValue(lastHighlightedAltID, out node))
        {
          node.nodeHighlighted = false;
          node.lineHighlighted = false;
        }

        Invoke((MethodInvoker)delegate
        {
          // Disable the button on the UI thread
          Invoke((MethodInvoker)delegate
          {
            // Below are min and max values for the animation speeds and are in milliseconds
            int minValue = 1000;
            int maxValue = 10;

            // Calculate the linear scale factor
            animationSpeed = minValue + ((maxValue - minValue) * (trbSpeed.Value - 1) / (10 - 1));
          });
          ProcessFeedback(messageToProcess);
        });

        UpdateGUITreeFromNodes();
      }
      return;
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
        string childrenIds = node.Children != null ? string.Join(", ", node.Children.Select(child => child.ID.ToString())) : "None";

        message.AppendLine($"ID: {nodePair.Key}, Keys: [{keysString}], IsLeaf: {node.IsLeaf}, IsRoot: {node.IsRoot}, Children: [{childrenIds}], Node Height: {node.height}");
        message.AppendLine(); // Add a new line between each printed node for easier reading
      }
      MessageBox.Show(message.ToString(), "Node Dictionary", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ProcessFeedback((NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents) feedback)
    {
      // RESET ALL HIGHLIGHTED NODES TO BE NON-HIGHLIGHTED
      if (lastSearched != null)
      {
        lastSearched.Searched = false;
      }
      if (nodeDictionary.Count == 0 && feedback.status == NodeStatus.Inserted)
      {
        Debug.WriteLine($"Node not found. Creating new root. ID={feedback.id}");
        bool isRoot = nodeDictionary.Count == 0;
        GUINode node = new(feedback.id, feedback.keys, true, isRoot, 0, 0);
        nodeDictionary[feedback.id] = node;
        if (chkDebugMode.Checked == true) ShowNodesMessageBox();
        SetHighlightedNode(feedback.id); // Highlights node for animations
        UpdateVisuals(); // Update the panel to show changes
      }
      if (nodeDictionary.TryGetValue(lastHighlightedID, out GUINode? highlightedNode) && highlightedNode != null)
      {
        highlightedNode.nodeHighlighted = false;
        highlightedNode.lineHighlighted = false;
      }
      if (nodeDictionary.TryGetValue(lastHighlightedAltID, out GUINode? highlightedAltNode) && highlightedAltNode != null)
      {
        highlightedAltNode.nodeHighlighted = false;
        highlightedAltNode.lineHighlighted = false;
      }
      Debug.WriteLine($"Received {feedback.status} status.");
      switch (feedback.status)
      {
        case NodeStatus.Insert:
          {
            lblCurrentProcess.Text = $"Starting Insert process for {feedback.keys[0]}";
            break;
          }
        case NodeStatus.ISearching:
          {
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
            {
              lblCurrentProcess.Text = $"Searching {feedback.id} for an adaquate node to add key to.";
              SetHighlightedNode(feedback.id); // Highlights node for animations
              SetHighlightedLine(feedback.id); // Highlights node for animations
              UpdateVisuals(); // Update the panel to show changes
            }
            break;
          }
        case NodeStatus.Inserted:
          {
            lblCurrentProcess.Text = "Inserting key";
            Debug.WriteLine($"Creating or updating GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
            {
              Debug.WriteLine($"Node found. Updating ID={feedback.id}");
              node.Keys = feedback.keys;
              node.NumKeys = feedback.numKeys;
              node.UpdateNodeWidth();
            }
            else
            {
              lblCurrentProcess.Text += "\nCreated new root node.";
              Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}");
              bool isRoot = nodeDictionary.Count == 0;
              node = new GUINode(feedback.id, feedback.keys, true, isRoot, 0, 1);
              nodeDictionary[feedback.id] = node;
            }
            if (chkDebugMode.Checked == true) ShowNodesMessageBox();
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.SplitInsert:
          {
            lblCurrentProcess.Text = "Adding key from node being split to new node";
            // Check if node is in the dictionary and is not a duplicate
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && feedback.numKeys != -1)
            {
              // Update node
              node.NumKeys = feedback.numKeys;
              node.Keys = feedback.keys;
              node.UpdateNodeWidth();
            }
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.NewRoot:
          {
            lblCurrentProcess.Text = "Creating new root as a consequence of the split";
            Debug.WriteLine($"A new root is being assigned. New root node ID={feedback.id}.");
            Debug.WriteLine($"Creating new root GUINode. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
            rootHeight++; // Update global root height
            bool isLeaf = false;
            bool isRoot = true;
            nodeDictionary[feedback.id] = new GUINode(feedback.id, feedback.keys, isLeaf, isRoot, rootHeight, feedback.numKeys);
            if (oldRoot != null)
              oldRoot.IsRoot = false; // Update the old root to not say it's a root anymore
            if (chkDebugMode.Checked == true) ShowNodesMessageBox();
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.Split:
          {
            lblCurrentProcess.Text = "Splitting node";
            Debug.WriteLine($"A split has occurred. Split node ID={feedback.id}. Preparing to handle new nodes.");
            if (nodeDictionary[feedback.id].IsRoot == true)
            {
              oldRoot = nodeDictionary[feedback.id]; // Update oldRoot global
            }
            break;
          }
        case NodeStatus.SplitResult:
          {
            Debug.WriteLine($"Creating or updating GUINode from split. ID={feedback.id}, Keys={String.Join(", ", feedback.keys)}");
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
            {
              if (node != null) // Null check
              {
                lblCurrentProcess.Text = "Updating the node that was split";
                // Update node
                node.Keys = feedback.keys;
                node.NumKeys = feedback.numKeys;
                node.UpdateNodeWidth();
              }
            }
            else
            {
              lblCurrentProcess.Text = "Creating a new node because of split";
              Debug.WriteLine($"Node not found. Creating new node. ID={feedback.id}");
              bool isLeaf = true;
              bool isRoot = false;
              int height = 0;
              node = new GUINode(feedback.id, feedback.keys, isLeaf, isRoot, height, feedback.numKeys);
              nodeDictionary[feedback.id] = node; // Add node to dictionary
              // Update the parent node's children list to include the new node
              if (feedback.altID != 0 && nodeDictionary.TryGetValue(feedback.altID, out GUINode? altNode))
              {
                altNode.Children.Add(node);
                node.height = Math.Max(0, altNode.height - 1);
                nodeDictionary[lastHighlightedID].nodeHighlighted = false;
                nodeDictionary[lastHighlightedID].lineHighlighted = false;
              }
            }
            if (chkDebugMode.Checked == true) ShowNodesMessageBox();
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.DeleteRange:
          {
            lblCurrentProcess.Text = $"Starting Delete process for ";
            if (feedback.keys.Length > 0)
              lblCurrentProcess.Text += $"{feedback.keys[0]}";
            if (feedback.keys.Length == 2)
              if (feedback.keys[0] + 1 != feedback.keys[1])
                lblCurrentProcess.Text += $" to {feedback.keys[1]}";
            lblCurrentProcess.Text += ".";
            break;
          }
        case NodeStatus.Restoration:
          {
            lblCurrentProcess.Text = $"Starting restoration process for {feedback.id}";
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.DSearching:
          {
            lblCurrentProcess.Text = $"Searching {feedback.id} for keys to delete.";
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.Deleted:
          {
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
                lblCurrentProcess.Text = ("Deleting input key");
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
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.DeletedRange:
          {
            if (feedback.numKeys == -1)
            {
              Debug.WriteLine($"Key not found for deletion in node ID={feedback.id}.");
              lblCurrentProcess.Text = $"Nothing was deleted in this node.";
            }
            else if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
            {
              lblCurrentProcess.Text = $"Key(s) were successfully deleted in this node.";
              // Update node
              node.Keys = feedback.keys;
              node.NumKeys = feedback.numKeys;
              node.UpdateNodeWidth();
              Debug.WriteLine($"Node ID={feedback.id} updated after key deletion. Remaining keys: {String.Join(", ", node.Keys)}");
            }
            else
            {
              lblCurrentProcess.Text = $"Something went wrong.";
              Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update after deletion.");
            }
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals();
            break;
          }
        case NodeStatus.FSearching:
          {
            lblCurrentProcess.Text = $"Searching {feedback.id} for right most leaf.";
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.Forfeit:
          {
            if (feedback.numKeys != -1)
            {
              if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
              {
                lblCurrentProcess.Text = "Retrieving key from node as a consequence of an underflow.";
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
            else
            {
              lblCurrentProcess.Text = "Couldn't retrieve key because the node was empty.";
            }
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.UpdateKeyValues:
          {
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
            {
              node.Keys = feedback.keys;
              node.NumKeys = feedback.numKeys;
              node.UpdateNodeWidth();
              Debug.WriteLine($"Node ID={feedback.id} updated after key deletion.");
              SetHighlightedNode(feedback.id);
              UpdateVisuals(); // Update the panel to show changes
              lblCurrentProcess.Text = "Updating the keys of the non-leaf node after key deletion.";
            }
            break;
          }
        case NodeStatus.Merge:
        case NodeStatus.MergeRoot:
          {
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
            {
              lblCurrentProcess.Text = "A merge has occurred. Updating merged node.";
              // Update node
              node.Keys = feedback.keys;
              node.NumKeys = feedback.numKeys;
              if (feedback.status == NodeStatus.MergeRoot)
              {
                node.IsRoot = true;
                rootHeight--;

                if (node.IsLeaf)
                {
                  foreach (var entry in nodeDictionary)
                  {
                    if (entry.Key != node.ID)
                      nodeDictionary.Remove(entry.Key);
                  }
                }
              }
              node.UpdateNodeWidth();
              Debug.WriteLine($"Node ID={feedback.id} updated. Remaining keys: {String.Join(", ", node.Keys)}");
            }
            else
            {
              Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update.");
            }
            // Eat sibling node
            if (nodeDictionary.TryGetValue(feedback.altID, out GUINode? sibling) && sibling != null)
            {
              lblCurrentProcess.Text += $"\nEating sibling node that had {string.Join(", ", sibling.Keys)} entries";
              if (feedback.status == NodeStatus.Merge)
              {
                if (nodeDictionary[feedback.altID].IsRoot)
                {
                  nodeDictionary[feedback.id].IsRoot = true;
                  rootHeight--;
                }
                if (sibling.Children != null)
                {
                  if (node != null && node.Children == null)
                  {
                    List<GUINode> children = [];
                    children.AddRange(sibling.Children);
                    node.Children = children;
                  }
                  else
                  {
                    nodeDictionary[feedback.id].Children.AddRange(sibling.Children);
                  }
                }
              }
              // Delete any children references from other nodes
              foreach (var kvp in nodeDictionary)
              {
                GUINode parentNode = kvp.Value;
                if (parentNode.Children != null)
                {
                  for (int i = 0; i < parentNode.Children.Count; i++)
                  {
                    if (parentNode.Children[i].ID == feedback.altID)
                      parentNode.Children.RemoveAt(i);
                  }
                  if (parentNode.Children.Count == 0)
                  {
                    parentNode.IsLeaf = true;
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
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals();
            break;
          }
        case NodeStatus.MergeParent:
          {
            lblCurrentProcess.Text = "Updating parent node as a consequence of the merge";
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
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.UnderFlow:
          {
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null)
            {
              // Update keys
              node.Keys = feedback.keys;
              node.NumKeys = feedback.numKeys;
              node.UpdateNodeWidth();
              Debug.WriteLine($"Node ID={feedback.id} updated. Remaining keys: {String.Join(", ", node.Keys)}");
            }
            else
            {
              Debug.WriteLine($"Node with ID={feedback.id} not found when attempting to update.");
            }
            if (nodeDictionary.TryGetValue(feedback.altID, out GUINode? sibling) && sibling != null)
            {
              // Bite sibling node
              sibling.Keys = feedback.altKeys;
              sibling.NumKeys = feedback.altNumKeys;
              sibling.UpdateNodeWidth();
              Debug.WriteLine($"Node ID={feedback.altID} updated. Remaining keys: {String.Join(", ", sibling.Keys)}");
            }
            else
            {
              Debug.WriteLine($"Node with ID={feedback.altID} not found when attempting to update after deletion.");
            }
            lblCurrentProcess.Text = "Node was in underflow and took one or more entries from its sibiling.";
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.Shift:
          {
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node) && node != null &&
              nodeDictionary.TryGetValue(feedback.altID, out GUINode? childNode) && childNode != null)
            {
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
              if (node.Children == null)
              {
                node.Children = new List<GUINode>() { childNode };
              }
              else
              {
                node.Children.Add(childNode);
                foreach (var child in node.Children)
                {
                  child.height = Math.Max(0, node.height - 1);
                }
                SetHighlightedNode(feedback.altID, altShiftHighlightID); // Highlights node for animations
                SetHighlightedLine(feedback.altID, altShiftHighlightID); // Highlights node for animations
              }
              lblCurrentProcess.Text = $"Moving {feedback.altID} to new parent.";
              node.IsLeaf = false;
              altShiftHighlightID = feedback.altID;
              UpdateVisuals(); // Update the panel to show changes
            }
            else
            {
              lblCurrentProcess.Text = "Something went wrong.";
            }
            break;
          }
        case NodeStatus.Search:
          {
            lblCurrentProcess.Text = $"Starting Search process for {feedback.keys[0]}";
            break;
          }
        case NodeStatus.SearchRange:
          {
            lblCurrentProcess.Text = $"Starting Search process for ";
            if (feedback.keys.Length > 0)
              lblCurrentProcess.Text += $"{feedback.keys[0]}";
            if (feedback.keys.Length == 2)
              if (feedback.keys[0] + 1 != feedback.keys[1])
                lblCurrentProcess.Text += $" to {feedback.keys[1]}";
            lblCurrentProcess.Text += ".";
            break;
          }
        case NodeStatus.SSearching:
          {
            lblCurrentProcess.Text = $"Searching {feedback.id}.";
            SetHighlightedNode(feedback.id); // Highlights node for animations
            SetHighlightedLine(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.Found:
          {
            lblCurrentProcess.Text = "Key found.";
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
            {
              node.Searched = true;
              lastSearched = node;
              Debug.WriteLine($"Node ID={feedback.id} has been highlighted.");
            }
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.FoundRange:
          {
            lblCurrentProcess.Text = $"Key(s) found: {string.Join(", ", feedback.keys)}";
            if (nodeDictionary.TryGetValue(feedback.id, out GUINode? node))
            {
              node.Searched = true;
              lastSearched = node;
              Debug.WriteLine($"Node ID={feedback.id} has been highlighted.");
            }
            SetHighlightedNode(feedback.id); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        case NodeStatus.FoundRangeComplete:
          {
            lblCurrentProcess.Text = "Listing all entries found.\nThe aggregate of all found range statuses." +
              $"\n{string.Join(", ", feedback.keys)}";
            break;
          }
        case NodeStatus.NodeDeleted:
          {
            if (chkDebugMode.Checked == true) ShowNodesMessageBox();
            lblCurrentProcess.Text = "Deleting node.";
            if (nodeDictionary.TryGetValue(feedback.altID, out GUINode? node))
            {
              for (int i = 0; i < node.Children.Count; i++)
              {
                if (node.Children[i].ID == feedback.id)
                {
                  node.Children.RemoveAt(i);
                }
              }
            }
            // If node is still in the dictionary, delete it
            if (nodeDictionary.TryGetValue(feedback.id, out node))
            {
              Debug.WriteLine($"Node ID={feedback.id} deleted.");
              nodeDictionary.Remove(feedback.id);
            }
            SetHighlightedNode(feedback.altID); // Highlights node for animations
            UpdateVisuals(); // Update the panel to show changes
            break;
          }
        default:
          {
            Debug.WriteLine($"The {feedback.status} status was unknown.");
            break;
          }
      }
    }

    private void UpdateVisuals()
    {
      UpdateGUITreeFromNodes();
      panel1.Invalidate();
    }

    private void SetHighlightedNode(long nodeID, long altNodeID = 0)
    {
      lastHighlightedID = nodeID; // Sets node to be highlighted for animations
      lastHighlightedAltID = altNodeID;
      // nodeDictionary[nodeID].nodeHighlighted = true;
      //if (lastHighlightedAltID != 0) nodeDictionary[altNodeID].nodeHighlighted = true;
      // if (lastHighlightedAltID != 0 && nodeDictionary.TryGetValue(altNodeID, out GUINode? altNode))
      //   nodeDictionary[altNode.ID].nodeHighlighted = true;
      if (nodeDictionary.TryGetValue(nodeID, out GUINode? node))
        node.nodeHighlighted = true;
      if (lastHighlightedAltID != 0 && nodeDictionary.TryGetValue(altNodeID, out node))
        node.nodeHighlighted = true;
    }

    private void SetHighlightedLine(long nodeID, long altNodeID = 0)
    {
      lastHighlightedID = nodeID; // Sets node to be highlighted for animations
      lastHighlightedAltID = altNodeID;
      if (nodeDictionary.TryGetValue(nodeID, out GUINode? node))
        node.lineHighlighted = true;
      if (lastHighlightedAltID != 0 && nodeDictionary.TryGetValue(altNodeID, out node))
        node.lineHighlighted = true;
    }

    private void Form1_Resize(object sender, EventArgs e)
    {
      PositionPanels();
    }

    private void PositionPanels()
    {
      // Positioning the buttonsPanel
      // panel2.Height = 100;
      // panel2.Width = this.ClientSize.Width; // Make buttonsPanel width equal to the form's client width
      // panel2.Location = new Point(0, this.ClientSize.Height - panel2.Height); // Align to bottom

      // Positioning the visualsPanel
      // panel1.Location = new Point(0, 0); // Start at top-left corner
      panel1.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - panel2.Height); // Fill the space above buttonsPanel

      scrollableWidth = panel1.Width + 5000;
      scrollableHeight = panel1.Height + 5000;

      panel1.AutoScrollMinSize = new Size(scrollableWidth, scrollableHeight);
      //panel1.AutoScrollPosition = new Point((panel1.AutoScrollMinSize.Width - panel1.ClientSize.Width) / 2, 0);

      panel1.Invalidate();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      InitializeBackend();
      messageQueue = new ConcurrentQueue<(NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents)>();
      _ = StartConsumerTask();
      _ = Animator();

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

      scrollableWidth = panel1.Width + 5000;
      scrollableHeight = panel1.Height + 5000;

      scrollableWidth = panel1.Width + 5000;
      scrollableHeight = panel1.Height + 5000;
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

    private async void btnInsert_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtInputData.Text))
      {
        MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (int.TryParse(txtInputData.Text, out int keyToInsert))
      {
        Debug.WriteLine($"Attempting to insert key: {keyToInsert}");
        await inputBuffer.SendAsync((TreeCommand.Insert, keyToInsert, 0, new Person(keyToInsert.ToString())));
      }
      else
      {
        MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }

      // Clear input textbox
      txtInputData.ForeColor = Color.Black;
      txtInputData.Text = "Insert Data Here...";
    }

    private async void btnInfo_Click(object sender, EventArgs e)
    {
      Form formPopup = new()
      {
        Width = 1500,
        Height = 1200
      };
      WebBrowser WebBrowser1 = new();
      WebBrowser1.Navigate("https://zackarybeckhtmlstorage.z19.web.core.windows.net/");
      WebBrowser1.Width = 1450;
      WebBrowser1.Height = 1140;
      WebBrowser1.Dock = DockStyle.Fill;
      formPopup.Controls.Add(WebBrowser1);
      formPopup.ShowDialog();
    }

    private async void btnInsertMany_Click(object sender, EventArgs e)
    {
      cancellationTokenSource = new CancellationTokenSource(); // Reset the token source for a new operation
      if (string.IsNullOrWhiteSpace(txtInputData.Text))
      {
        MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (int.TryParse(txtInputData.Text, out int keyToInsert))
      {
        Debug.WriteLine($"Attempting to insert key: {keyToInsert}");

        for (int i = 0; i < keyToInsert; i++)
        {
          if (cancellationTokenSource.IsCancellationRequested)
          {
            Debug.WriteLine("Operation cancelled due to duplicate key found.");
            //await inputBuffer.SendAsync((TreeCommand.Insert, i, new Person(keyToInsert.ToString())));
            //int delay = Invoke(new Func<int>(() => animationSpeed));
            break; // Exit the loop if cancellation is requested
          }
          Random random = new();
          await inputBuffer.SendAsync((TreeCommand.Insert, random.Next(1000), 0, new Person(keyToInsert.ToString())));
          int delay = Invoke(new Func<int>(() => animationSpeed));
          await Task.Delay(delay);
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

    private async void btnDelete_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtInputData.Text))
      {
        MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (int.TryParse(txtInputData.Text, out int keyToDelete))
      {
        Debug.WriteLine($"Attempting to delete key: {keyToDelete}");
        await inputBuffer.SendAsync((TreeCommand.Delete, keyToDelete, 0, null));
      }
      else
      {
        MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      }

      // Clear input textbox
      txtInputData.ForeColor = Color.Black;
      txtInputData.Text = "Insert Data Here...";
    }

    private async void btnSearch_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(txtInputData.Text))
      {
        MessageBox.Show("Please enter a valid integer key.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
      }

      if (int.TryParse(txtInputData.Text, out int keyToSearch))
      {
        Debug.WriteLine($"Attempting to search for key: {keyToSearch}");
        await inputBuffer.SendAsync((TreeCommand.Search, keyToSearch, 0, null));
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
      ResetTreeAndForm();
    }

    private void btnPlayAndPause_Click(object sender, EventArgs e)
    {
      if (animate)
      {
        animate = false;
      }
      else
      {
        animate = true;
        _ = Animator();
      }
    }

    private async void btnNext_Click(object sender, EventArgs e)
    {
      await ConsumeOneOutput();
    }

    private async Task ConsumeOneOutput()
    {
      if (outputBuffer.Count > 0)
      {
        _ = Task.Run(async () =>
        {
          Invoke((MethodInvoker)delegate
          {
            DisableButtonEvents();
          });
          while (outputBuffer.Count > 0)
          {
            await Task.Delay(1000);
          }
          Invoke((MethodInvoker)delegate
          {
            EnableButtonEvents();
          });
        });
        if (!animate)
          btnNext.Enabled = false;
        (NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents) recieved = await outputBuffer.ReceiveAsync();
        // Create a deep copy of the keys and altKeys arrays to ensure they are not modified elsewhere
        (NodeStatus status, long id, int numKeys, int[] keys, Person?[] contents, long altID, int altNumKeys, int[] altKeys, Person?[] altContents) feedbackCopy;
        feedbackCopy.status = recieved.status;
        feedbackCopy.id = recieved.id;
        feedbackCopy.numKeys = recieved.numKeys;
        feedbackCopy.altID = recieved.altID;
        feedbackCopy.altNumKeys = recieved.altNumKeys;
        feedbackCopy.keys = new int[recieved.keys.Length];
        feedbackCopy.contents = new Person[recieved.keys.Length];
        feedbackCopy.altKeys = new int[recieved.altKeys.Length];
        feedbackCopy.altContents = new Person[recieved.altKeys.Length];
        for (int i = 0; i < recieved.numKeys; i++)
        {
          feedbackCopy.keys[i] = recieved.keys[i];
          if (i < recieved.contents.Length)
            feedbackCopy.contents[i] = recieved.contents[i];
        }
        for (int i = 0; i < recieved.altNumKeys; i++)
        {
          feedbackCopy.altKeys[i] = recieved.altKeys[i];
          if (i < recieved.altContents.Length)
            feedbackCopy.altContents[i] = recieved.altContents[i];
        }

        messageQueue.Enqueue(feedbackCopy);
        await StartConsumerTask();
        if (!animate)
          btnNext.Enabled = true;
      }
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
      ResetTreeAndForm();
    }

    private async void ResetTreeAndForm()
    {
      isProcessing = true;
      messageQueue = new ConcurrentQueue<(NodeStatus, long, int, int[], Person?[], long, int, int[], Person?[])>();
      Task.Run(() =>
      {
        Thread.Sleep(100);
        isProcessing = false;
      });

      EnableButtonEvents();

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
      await inputBuffer.SendAsync((TreeCommand.Tree, degree, 0, default(Person?)));
      panel1.Invalidate();
      rootHeight = 0; // Temporary to see if this works
      oldRoot = null; // Temporary to see if this works
      isFirstNodeEncountered = false;

      // Clear input textbox
      txtInputData.ForeColor = Color.Black;
      txtInputData.Text = "Insert Data Here...";
      lblCurrentProcess.Text = "";
    }

    private void chkBTreeTrue_CheckedChanged(object sender, EventArgs e)
    {
      ResetTreeAndForm();
      InitializeBackend();
    }

    private void DisableButtonEvents()
    {
      btnSearch.Enabled = false;
      btnDelete.Enabled = false;
      btnInsert.Enabled = false;
      btnInsertMany.Enabled = false;
      btnclear.Enabled = false;
      cmbxMaxDegree.Enabled = false;
    }

    private void EnableButtonEvents()
    {
      btnSearch.Enabled = true;
      btnDelete.Enabled = true;
      btnInsert.Enabled = true;
      btnInsertMany.Enabled = true;
      btnclear.Enabled = true;
      cmbxMaxDegree.Enabled = true;
    }

    private void UpdateGUITreeFromNodes()
    {
      GUINode? rootNode = DetermineRootNode();
      if (rootNode != null)
      {
        _tree = new GUITree(rootNode, panel1);
        //_tree.ResetAndInitializeLeafStart();
        panel1.Invalidate();
      }
    }

    private GUINode? DetermineRootNode()
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
      int endKey,
      Person? content
      )> inputBuffer = new();

    private void InitializeBackend()
    {
      if (_Producer != null)
      {
        inputBuffer.Complete();
        outputBuffer.Complete();
        _Producer.Wait();
        inputBuffer = new();
        outputBuffer = new();
        _Producer.Dispose();
      }
      if (!chkBTreeTrue.Checked)
      {
        _Producer = Task.Run(async () =>
        {
          BTree<Person> _Tree = new BTree<Person>(3, outputBuffer);
          try
          {
            while (await inputBuffer.OutputAvailableAsync())
            {
              (TreeCommand action, int key, int endKey, Person? content) = inputBuffer.Receive();
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
      else if (chkBTreeTrue.Checked)
      {
        _Producer = Task.Run(async () =>
        {
          BPlusTree<Person> _Tree = new(3, outputBuffer);
          try
          {
            while (await inputBuffer.OutputAvailableAsync())
            {
              (TreeCommand action, int key, int endKey, Person? content) = inputBuffer.Receive();
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
                  _Tree = new BPlusTree<Person>(key, outputBuffer); // This may not be correct, but it works for now
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
}