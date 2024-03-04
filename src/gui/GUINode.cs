using System;
using System.Collections.Generic;
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

        public GUINode(int[] keys, bool isLeaf, bool IsRoot, int Depth,  GUINode[] children = null)
        {
            Keys = keys;
            Children = children;
            IsLeaf = isLeaf;
            this.IsRoot = IsRoot;
            NumKeys = keys.Length;
            this.Depth = Depth;

            // Calculate node width based on keys
            NodeWidth = 40 * NumKeys; //Here
            NodeHeight = 40;
        }

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
            var brush = new SolidBrush(Color.LightGray);
            var textBrush = new SolidBrush(Color.Black);
            var font = new Font("Consolas", 8);

            // Draw node rectangle
            graphics.FillRectangle(brush, x - NodeWidth / 2, y, NodeWidth, NodeHeight);
            graphics.DrawRectangle(pen, x - NodeWidth / 2, y, NodeWidth, NodeHeight);

            // Divide node into slots
            float slot = NodeWidth / NumKeys;

            // Draw keys in node
            for (int i = 0; i < NumKeys; i++)
            {
                string key = Keys[i].ToString();

                // Measure the total width and height of the key string and print it in the center of the node
                SizeF textSize = graphics.MeasureString(key, font);
                float slotCenterX = x - (NodeWidth / 2) + (slot * (i + 0.5f));
                float textCenterX = slotCenterX - (textSize.Width / 2);
                textCenterX = (float)Math.Round(textCenterX);
                float textCenterY = y + (NodeHeight / 2) - (textSize.Height / 2);
                textCenterY = (float)Math.Round(textCenterY);
                graphics.DrawString(key, font, textBrush, textCenterX, textCenterY);
            }
        }
    }
}
