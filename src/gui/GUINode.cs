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
        private long _ID;
        public long ID { get { return _ID; } }
        public int[] Keys { get; set; }
        public List<GUINode> Children { get; set; }
        public bool IsLeaf { get; set; }
        public bool IsRoot { get; set; }
        public int NumKeys { get; set; }
        public float NodeWidth, NodeHeight;
        public int height { get; set; }
        public bool Searched = false;
        public bool nodeHighlighted = false;
        public bool lineHighlighted = false;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public GUINode(long id, int[] keys, bool isLeaf, bool IsRoot, int height, int NumKeys, List<GUINode> children = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            _ID = id;
            Keys = keys;
            Children = children;
            IsLeaf = isLeaf;
            this.IsRoot = IsRoot;
            this.NumKeys = NumKeys;
            this.height = height;


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

        public void DisplayNode(Graphics graphics, float x, float y)
        {
            // Brushes
            var pen = new Pen(Color.MediumSlateBlue, 2);
            if (IsLeaf)
            {
                pen = new Pen(Color.Green, 2);
            }

            if (nodeHighlighted)
            {
                pen = new Pen(Color.Red, 2);
            }

            if (Searched)
            {
                pen = new Pen(Color.Blue, 2);
            }

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

        // Added by Dakota, allows the delete method to resize the nodes since node width is generated
        // when the node is initialized.
        public void UpdateNodeWidth()
        {
            NodeWidth = 40 * NumKeys; // Adjust this formula as needed
        }
    }
}