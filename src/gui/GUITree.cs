using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Design.Serialization;
using System.Xml.Linq;
using System.Drawing.Design;

namespace B_TreeVisualizationGUI
{
    internal class GUITree : Form1
    {
        public GUINode root;
        public float centerx, centery;
        private Panel displayPanel;
        private float nodeSpacing = 10;
        private List<float> leafStart; // Useful for figuring out the position of leaves

        public GUITree(GUINode root, Panel displayPanel)
        {
            this.root = root;
            this.displayPanel = displayPanel;
            leafStart = initializeLeafStart();
        }

        // Displays the tree on Panel1
        public void DrawTree(Graphics graphics, GUINode currentNode, float centerX, float x, float y, float subtreeWidth, Dictionary<int, int> depthNodesDrawn, int depth = 0)
        {
            if (currentNode == null) return; // Null check

            var pen = new Pen(Color.MediumSlateBlue, 2); // Pen for drawing

            // Ensures current depth is initialized in the dictionary 
            if (!depthNodesDrawn.ContainsKey(depth))
            {
                depthNodesDrawn.Add(depth, 0);
            }

            currentNode.DisplayNode(graphics, x, y); // Draw the current node

            depthNodesDrawn[depth]++; // Increases the number of nodes drawn at the current depth

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

                float nodeSlot; // Initialize nodeSlot aka, the space that a node is going to be centered in

                for (int i = 0; i < currentNode.Children.Length; i++)
                {
                    GUINode childNode = i < currentNode.Children.Length ? currentNode.Children[i] : null; // FIX LATER

                    float childX = 0; // Initialize horizontal spacing, aka center of the node's slot

                    // If node is a leaf, calculates custom node slot, if not, divides subtreeWdith in equal parts
                    if (childNode.IsLeaf || childNode.Children == null)
                    {
                        //nodeSlot = childNode.NodeWidth + nodeSpacing;
                        if (leafStart.Count > 0)
                        {
                            childX = leftX + leafStart[0] + (childNode.NodeWidth / 2) + 5;
                            leafStart.RemoveAt(0);
                        }
                    }
                    else
                    {
                        nodeSlot = (subtreeWidth / GetNodesAtDepth(currentNode.Depth + 1).Count);
                        childX = leftX + (depthNodesDrawn[depth] * nodeSlot) + nodeSlot / 2;
                    }

                    float childY = y + currentNode.NodeHeight + 50; // Vertical spacing: FIX LATER

                    if (childNode != null)
                    {
                        DrawTree(graphics, childNode, centerX, childX, childY, subtreeWidth, depthNodesDrawn, depth); // Draw the child subtree
                        graphics.DrawLine(pen, x, y + currentNode.NodeHeight, childX, childY); // Draw line from parent to child
                    }
                }
            }
        }

        // Finds the starting point for all of the leaf nodes
        public List<float> initializeLeafStart()
        {
            List<float> leafStart = new List<float>(); // Initialized the leafStart List
            float start = 0; // Adds the first float, since the leftmost leaf's slot will start at 0
            List<GUINode> leafList = GetNodesAtDepth(root.FindDepthOfFirstLeaf());
            leafStart.Add(0);
            for (int i = 0; i < leafList.Count; i++)
            {
                start += leafList[i].NodeWidth + nodeSpacing;
                leafStart.Add(start);
            }
            return leafStart;
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
            width += (node.Children.Length - 1) * nodeSpacing; // Add spacing between child nodes

            // Ensures that the width is at least as wide as the node itself to maintain the tree structure
            return Math.Max(width, node.NodeWidth);
        }

        // Calculates the nodes at a certain depth
        public List<GUINode> GetNodesAtDepth(int targetDepth)
        {
            List<GUINode> nodesAtDepth = new List<GUINode>();
            Traverse(root, 0, targetDepth, nodesAtDepth);
            return nodesAtDepth;
        }

        // Traverses through the tree
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