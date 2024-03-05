using System.Drawing;
using System.Reflection.Metadata;
using System.Windows.Forms;

namespace B_TreeVisualizationGUI
{
    public partial class Form1 : Form
    {
        // Global variables
        int scrollableWidth = 10000;
        int scrollableHeight = 10000;
        private GUITree _tree; // Class member to store the tree
        Dictionary<int, GUINode> nodeDictionary = new Dictionary<int, GUINode>();
        private System.Windows.Forms.Timer scrollTimer;

        public Form1()
        {
            InitializeComponent();
            panel1.Scroll += Panel1_Scroll;

            // Flicker fixes
            scrollTimer = new System.Windows.Forms.Timer();
            scrollTimer.Interval = 200; // Adjust the delay as needed
            scrollTimer.Tick += ScrollTimer_Tick;
        }

        private async void ScrollTimer_Tick(object sender, EventArgs e)
        {
            scrollTimer.Stop();

            // Redraw panel after a delay
            panel1.Invalidate();
        }

        private void cmbxMaxDegree_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void InitializeTree()
        {
            // Generate makedo tree structure
            int[] arr = new int[1];
            int firstRowChildren = 3;
            int secondRowChildren = 3;
            GUINode[] nodearr = new GUINode[firstRowChildren];
            for (int i = 0; i < firstRowChildren; i++)
            {
                GUINode[] childrenarr = new GUINode[secondRowChildren];
                for (int j = 0; j < secondRowChildren; j++)
                {
                    // Create new leaf nodes for each child
                    GUINode[] leafChildrenArr = new GUINode[2];
                    for (int k = 0; k < 2; k++)
                    {
                        GUINode leafChild = new GUINode(generateKeys(3), true, false, 3);
                        leafChildrenArr[k] = leafChild;
                    }

                    GUINode child = new GUINode(generateKeys(3), false, false, 2, leafChildrenArr);
                    
                    childrenarr[j] = child;
                }
                GUINode node = new GUINode(generateKeys(2), false, false, 1, childrenarr);
                nodearr[i] = node;
            }
            GUINode root = new GUINode(generateKeys(2), false, true, 0, nodearr);
            _tree = new GUITree(root, panel1); // Assuming _tree and panel1 are defined elsewhere
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Create horizontal scroll bar
            System.Windows.Forms.ScrollBar hScrollBar1 = new HScrollBar();
            hScrollBar1.Dock = DockStyle.Bottom;

            hScrollBar1.Scroll += (s, ea) =>
            {
                panel1.HorizontalScroll.Value = hScrollBar1.Value;
                scrollTimer.Start(); // Start the timer when scrolling occurs
            };

            // Create vertical scroll bar
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

            InitializeTree(); // Initialize the tree when the form loads
        }

        private void Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            //panel1.Invalidate();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (_tree == null) return; // Check if the tree is initialized

            // Adjustments for drawing
            float adjustedCenterX = scrollableWidth / 2 - panel1.HorizontalScroll.Value;
            float adjustedCenterY = 10 - panel1.VerticalScroll.Value;

            // Calculate tree width
            float width = _tree.CalculateSubtreeWidth(_tree.root);

            // Initialize dictionary
            Dictionary<int, int> depthNodesDrawn = new Dictionary<int, int>();

            // Use the stored tree for drawing
            _tree.DrawTree(e.Graphics, _tree.root, adjustedCenterX, adjustedCenterX, adjustedCenterY, width, depthNodesDrawn);
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

        public int[] generateKeys1()
        {
            Random random = new Random();
            //int[] keys = new int[random.Next(1, 2)];
            int[] keys = new int[2];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = random.Next(5000);
            }
            return keys;
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            InitializeTree();
            panel1.Invalidate();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            _tree = null;
            panel1.Invalidate();
        }
    }
}