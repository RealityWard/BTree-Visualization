using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Design.Serialization;

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

        public void DrawTree(Graphics graphics, GUINode currentNode, float centerX, float x, float y, float subtreeWidth, Dictionary<int, int> depthNodesDrawn, int depth = 0)
        {
            if (currentNode == null) return; // Null check

            var pen = new Pen(Color.MediumSlateBlue, 2); // Pen for drawing

            // Ensure current depth is initialized in the dictionary
            if (!depthNodesDrawn.ContainsKey(depth))
            {
                depthNodesDrawn.Add(depth, 0);
            }

            currentNode.DisplayNode(graphics, x, y); // Draw the current node

            depthNodesDrawn[depth]++;

            // Recursively draw child nodes if this is not a leaf
            if (currentNode.Children != null && currentNode.NumKeys > 0)
            {
                depth++; // Increase depth since we are going deeper into the tree

                // Initialize new depth
                if (!depthNodesDrawn.ContainsKey(depth))
                {
                    depthNodesDrawn.Add(depth, 0);
                } 
                float leftX = centerX - subtreeWidth / 2; // Calculate left x-coordinate of the tree
                float nodeSlot = (subtreeWidth / GetNodesAtDepth(currentNode.Depth + 1).Count); // Find how much space a node takes up

                // Makes sure no nodes are going to overlap
                foreach (GUINode child in currentNode.Children)
                {
                    while (child.NodeWidth >= nodeSlot)
                    {
                        nodeSlot += nodeSlot / 4;
                        int leafcount = root.CountLeaves();
                        subtreeWidth = nodeSlot * leafcount;
                    }
                }

                for (int i = 0; i < currentNode.Children.Length; i++)
                {
                    GUINode childNode = i < currentNode.Children.Length ? currentNode.Children[i] : null; // FIX LATER
                    
                    float childX = leftX + (depthNodesDrawn[depth] * nodeSlot) + nodeSlot / 2; // Horizontal spacing aka center of the node's slot
                    float childY = y + currentNode.NodeHeight + 50; // Vertical spacing: FIX LATER

                    if (childNode != null)
                    {
                        // Draw the child subtree
                        //depthNodesDrawn[depth]++;
                        DrawTree(graphics, childNode, centerX, childX, childY, subtreeWidth, depthNodesDrawn, depth);
                        
                        if (depthNodesDrawn[depth] >= currentNode.Children.Length)
                        {
                            //depth--;
                        }
                       
                        // Draw line from parent to child
                        graphics.DrawLine(pen, x, y + currentNode.NodeHeight, childX, childY);
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
        public float CalculateSubtreeWidth(GUINode node)
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

        public List<GUINode> GetNodesAtDepth(int targetDepth)
        {
            List<GUINode> nodesAtDepth = new List<GUINode>();
            Traverse(root, 0, targetDepth, nodesAtDepth);
            return nodesAtDepth;
        }

        private void Traverse(GUINode node, int currentDepth, int targetDepth, List<GUINode> nodesAtDepth)
        {
            if (node == null)
            {
                return;
            }

            if (currentDepth == targetDepth)
            {
                nodesAtDepth.Add(node);
                return;
            }

            // Increment depth and traverse child nodes
            for (int i = 0; i < node.Children.Count(); i++)
            {
                Traverse(node.Children[i], currentDepth + 1, targetDepth, nodesAtDepth);
            }
        }
    }
}