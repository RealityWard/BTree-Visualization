using System.Drawing;
using System.Windows.Forms;

namespace B_TreeVisualizationGUI
{
    public partial class Form1 : Form
    {
        // Global variables
        int scrollableWidth = 5000;
        int scrollableHeight = 5000;
        private GUITree _tree; // Class member to store the tree
        Dictionary<int, GUINode> nodeDictionary = new Dictionary<int, GUINode>();

        public Form1()
        {
            InitializeComponent();
            panel1.Scroll += Panel1_Scroll;
        }

        private void cmbxMaxDegree_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void InitializeTree()
        {
            // Generate the tree structure once
            GUINode[] nodearr = new GUINode[51];
            for (int i = 0; i < 5; i++)
            {
                GUINode[] childrenarr = new GUINode[2];
                for (int j = 0; j < 2; j++)
                {
                    GUINode child = new GUINode(generateKeys(), false, false);
                    childrenarr[j] = child;
                }
                GUINode node = new GUINode(generateKeys(), false, false, childrenarr);
                nodearr[i + 1] = node;
            }
            GUINode root = new GUINode(generateKeys(), false, true, nodearr);
            _tree = new GUITree(root, panel1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Windows.Forms.ScrollBar hScrollBar1 = new HScrollBar();
            hScrollBar1.Dock = DockStyle.Bottom;

            hScrollBar1.Scroll += (s, ea) =>
            {
                panel1.HorizontalScroll.Value = hScrollBar1.Value;
                panel1.Invalidate();
            };

            System.Windows.Forms.ScrollBar vScrollBar1 = new VScrollBar();
            vScrollBar1.Dock = DockStyle.Right;

            vScrollBar1.Scroll += (s, ea) =>
            {
                panel1.VerticalScroll.Value = vScrollBar1.Value;
                panel1.Invalidate();
            };

            panel1.Controls.Add(vScrollBar1);
            panel1.Controls.Add(hScrollBar1);
            panel1.AutoScrollMinSize = new Size(scrollableWidth, scrollableHeight);
            panel1.AutoScrollPosition = new Point((panel1.AutoScrollMinSize.Width - panel1.ClientSize.Width) / 2, 0);

            InitializeTree(); // Initialize the tree when the form loads
        }

        private void Panel1_Scroll(object sender, ScrollEventArgs e)
        {
            panel1.Invalidate();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (_tree == null) return; // Check if the tree is initialized

            // Adjustments for drawing
            float adjustedCenterX = scrollableWidth / 2 - panel1.HorizontalScroll.Value;
            float adjustedCenterY = 10 - panel1.VerticalScroll.Value;

            // Use the stored tree for drawing
            _tree.DrawTree(e.Graphics, _tree.root, adjustedCenterX, adjustedCenterY);
        }

        public int[] generateKeys()
        {
            Random random = new Random();
            int[] keys = new int[random.Next(1, 4)];
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