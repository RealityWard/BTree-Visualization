using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace B_TreeVisualizationGUI
{
    internal class GUITree : Form1
    {
        public GUINode root;
        public float centerx, centery;
        private Panel displayPanel;

        public GUITree(GUINode root, Panel displayPanel)
        {
            this.root = root;
            this.displayPanel = displayPanel;
        }

        public void DrawTree(Graphics graphics, GUINode currentNode, float x, float y)
        {
            if (currentNode == null) return;

            var pen = new Pen(Color.MediumSlateBlue, 2);

            // Calculate the total width of the current subtree
            float subtreeWidth = CalculateSubtreeWidth(currentNode);

            // Draw the current node
            currentNode.DisplayNode(graphics, x, y);

            // Recursively draw child nodes if this is not a leaf
            if (currentNode.Children != null && currentNode.NumKeys > 0)
            {
                float startX = x - subtreeWidth /2;
                float nodeSlot = (subtreeWidth / currentNode.Children.Length); // Here

                for (int i = 0; i < currentNode.Children.Length; i++)
                {
                    GUINode childNode = i < currentNode.Children.Length ? currentNode.Children[i] : null; // FIX LATER

                    // float childSubtreeWidth = CalculateSubtreeWidth(currentNode);
                    
                    float childX = startX + (i * nodeSlot) + nodeSlot / 2;
                    float childY = y + currentNode.NodeHeight + 50; // Vertical spacing: FIX LATER

                    if (childNode != null)
                    {
                        // Draw the child subtree
                        DrawTree(graphics, childNode, childX, childY);

                        // Draw line from parent to child
                        graphics.DrawLine(pen, x, y + currentNode.NodeHeight, childX, childY);
                        
                        //startX = startX - subtreeWidth / 2 + nodeSlot; // Here
                    }
                    else
                    {
                        // If a child node is null, we should still move startX to the right to keep the spacing consistent
                        //startX += currentNode.NodeWidth * 2;
                    }
                }
            }
        }

        // Method to calculate the width needed for a subtree based on the widths of its children
        private float CalculateSubtreeWidth(GUINode node)
        {
            // Return the node's own width if it is a leaf or has no children.
            if (node.IsLeaf || node.Children == null)
            {
                return node.NodeWidth;
            }

            float width = 0;

            // Loop through each child node
            for (int i = 0; i < node.Children.Length; i++)
            {
                if (node.Children[i] != null)
                {
                    // Recursively calculate the subtree width of each child
                    width += CalculateSubtreeWidth(node.Children[i]);
                }
            }

            // Add the total width of children to the width of the node
            width += (node.Children.Length - 1) * 10; // Add spacing between child nodes

            // Ensure that the width is at least as wide as the node itself to maintain the tree structure
            return Math.Max(width, node.NodeWidth);
        }
    }
}