using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static B_TreeVisualizationGUI.GUITree;

namespace B_TreeVisualizationGUI
{
    internal class GUINode
    {
        public int[] Keys { get; set; }
        public GUINode[] Children { get; set; }
        public bool IsLeaf { get; set; }
        public bool IsRoot { get; set; }
        public int NumKeys { get; set; }
        public float NodeWidth, NodeHeight;
        public int Depth { get; set; }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
    public GUINode(int[] keys, bool isLeaf, bool IsRoot, int Depth, int NumKeys, GUINode[] children = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    {
            Keys = keys;
            Children = children;
            IsLeaf = isLeaf;
            this.IsRoot = IsRoot;
            this.NumKeys = NumKeys;
            this.Depth = Depth;

            // Calculate node width based on keys
            NodeWidth = 40 * NumKeys;
            NodeHeight = 40;
        }

        // Counts the leaves it can find searching starting from this node
        public int CountLeaves()
        {
            if (IsLeaf)
                return 1;

            int leafCount = 0;
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    leafCount += child.CountLeaves();
                }
            }
            return leafCount;
        }

        // Finds the depth of the first leaf node it finds starting from the root
        public int FindDepthOfFirstLeaf()
        {
            return FindDepthOfFirstLeafHelper(this);
        }

        // Helper method to traverse the tree and find the depth of the first leaf node encountered
        private int FindDepthOfFirstLeafHelper(GUINode node)
        {
            if (node.IsLeaf)
            {
                return node.Depth;
            }

            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    int depth = FindDepthOfFirstLeafHelper(child);
                    if (depth != -1) // If leaf is found in the child subtree
                        return depth;
                }
            }

            return -1; // Leaf not found in this subtree
        }

        public void DisplayNode(Graphics graphics, float x, float y)
        {
            // Brushes
            var pen = new Pen(Color.MediumSlateBlue, 2);
            var brush = new SolidBrush(Color.LightGray);
            var textBrush = new SolidBrush(Color.Black);
            var font = new Font("Consolas", 8);

            // Draw node rectangle
            graphics.FillRectangle(brush, x - NodeWidth / 2, y, NodeWidth, NodeHeight);
            graphics.DrawRectangle(pen, x - NodeWidth / 2, y, NodeWidth, NodeHeight);

            // Calculate slot width for each key
            float slotWidth = NodeWidth / Math.Max(1, NumKeys); // Avoid division by zero

            for (int i = 0; i < NumKeys && i < Keys.Length; i++) // Use both NumKeys and Keys.Length for safety
            {
                // Display each key within its slot
                string keyString = Keys[i].ToString();
                SizeF textSize = graphics.MeasureString(keyString, font);
                float slotCenterX = x - (NodeWidth / 2) + (slotWidth * (i + 0.5f));
                float textX = slotCenterX - (textSize.Width / 2);
                float textY = y + (NodeHeight - textSize.Height) / 2; // Center text vertically within the node
                graphics.DrawString(keyString, font, textBrush, textX, textY);
            }
        }
    }
}