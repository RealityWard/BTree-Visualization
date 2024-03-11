using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace B_TreeVisualizationGUI
{
    internal class BTreeVisual : Panel
    {
        private BTree tree;
        private float centerx, centery;

        public BTreeVisual(BTree tree)
        {
            this.InitializeComponent();
            this.tree = tree;
        }

        private void InitializeComponent()
        {
            this.Size = new Size(800, 600); // Set the panel size
            centerx = Size.Width / 2;
            centery = Size.Height / 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (tree != null)
            {
                DrawTree(e.Graphics, tree.Root, centerx, 15);
            }
        }

        private void DrawTree(Graphics graphics, Node currentNode, float x, float y)
        {
            if (currentNode == null) return;

            var pen = new Pen(Color.MediumSlateBlue, 2);
            var brush = new SolidBrush(Color.LightGray);
            var textBrush = new SolidBrush(Color.Black);
            var font = new Font("Consolas", 8);

            // Calculate node width based on keys
            float nodeWidth = 50 * currentNode.Keys.Count;
            float nodeHeight = 50;

            // Draw node rectangle
            graphics.FillRectangle(brush, x, y, nodeWidth, nodeHeight);
            graphics.DrawRectangle(pen, x, y, nodeWidth, nodeHeight);

            // Draw keys in node
            for (int i = 0; i < currentNode.NumKeys; i++)
            {
                string key = currentNode.Keys[i].ToString();
                graphics.DrawString(key, font, textBrush, x + (i * 50) + 15, y + 15);
            }

            // Recursively draw child nodes if this is not a leaf
            if (!currentNode.IsLeaf)
            {
                for (int i = 0; i < currentNode.NumChildren; i++)
                {
                    // Calculate child node position
                    float childX = x + (i * 100); // Horizontal spacing
                    float childY = y + 100; // Vertical spacing
                    DrawTree(graphics, currentNode.Children[i], childX, childY);

                    // Draw link from parent to child
                    graphics.DrawLine(pen, x + nodeWidth / 2, y + nodeHeight, childX + nodeWidth / 2, childY);
                }
            }
        }

        // Placeholder classes for B Tree structure
        public class BTree
        {
            public Node Root { get; private set; }

            public BTree()
            {
                // Initialize tree here
            }
        }

        public class Node
        {
            public int NumKeys { get; set; }
            public int NumChildren { get; set; }
            public bool IsLeaf { get; set; }
            public Node[] Children { get; set; }


            public Node()
            {
                // Initialize node here
            }
        }
    }
}
