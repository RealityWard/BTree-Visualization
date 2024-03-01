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
        public GUINode[]? Children { get; set; }
        public bool IsLeaf { get; set; }
        public bool IsRoot { get; set; }
        public int NumKeys { get; set; }
        public float NodeWidth, NodeHeight;

        public GUINode(int[] keys, bool isLeaf, bool IsRoot,  GUINode[]? children = null)
        {
            Keys = keys;
            Children = children;
            IsLeaf = isLeaf;
            this.IsRoot = IsRoot;
            NumKeys = keys.Length;

            // Calculate node width based on keys
            NodeWidth = 50;
            NodeHeight = 50 * NumKeys;
        }

        public void DisplayNode(Graphics graphics, float x, float y)
        {
            // Brushes
            var pen = new Pen(Color.MediumSlateBlue, 2);
            var brush = new SolidBrush(Color.LightGray);
            var textBrush = new SolidBrush(Color.Black);
            var font = new Font("Consolas", 8);

            // Draw node rectangle
            graphics.FillRectangle(brush, x-NodeWidth/2, y, NodeWidth, NodeHeight);
            graphics.DrawRectangle(pen, x - NodeWidth / 2, y, NodeWidth, NodeHeight);

            // Draw keys in node
            for (int i = 0; i < NumKeys; i++)
            {
                string key = Keys[i].ToString();
                graphics.DrawString(key, font, textBrush, (x - NodeWidth / 2) + 10, (y + 15) + (i * 50));
            }
        }
    }
}
