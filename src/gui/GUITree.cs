﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Design.Serialization;
using System.Xml.Linq;
using System.Drawing.Design;
using System.Diagnostics;

namespace B_TreeVisualizationGUI
{
    internal class GUITree
    {
        public GUINode root;
        public float centerx = 0, centery = 0;
        private Panel displayPanel;
        private float nodeSpacing = 10;
        private List<float> leafStart = new List<float>(); // Useful for figuring out the position of leaves

        public GUITree(GUINode root, Panel displayPanel)
        {
            this.root = root;
            this.displayPanel = displayPanel;
            leafStart = initializeLeafStart();
        }

        public void ResetAndInitializeLeafStart()
        {
            leafStart = initializeLeafStart();
        }

        // Displays the tree on Panel1
        public void DrawTree(Graphics graphics, GUINode currentNode, float centerX, float x, float y, float subtreeWidth, Dictionary<int, int> heightNodesDrawn, int height)
        {
            // Define the bounds of your drawing area (e.g., the visible area of 'panel1')
            // Used to prevent drawing out of bounds.
            Rectangle drawingBounds = new Rectangle(0, 0, displayPanel.ClientSize.Width, displayPanel.ClientSize.Height);

            // Set the clipping region to constrain drawing to 'drawingBounds'
            graphics.SetClip(drawingBounds);

            if (currentNode == null) return; // Null check

            var pen = new Pen(Color.MediumSlateBlue, 2); // Pen for drawing

            // Ensures current height is initialized in the dictionary 
            if (!heightNodesDrawn.ContainsKey(height))
            {
                heightNodesDrawn.Add(height, 0);
            }

            currentNode.DisplayNode(graphics, x, y); // Draw the current node

            // Log if 'x' or 'y' is NaN before drawing the node
            if (float.IsNaN(x) || float.IsNaN(y))
            {
                Console.WriteLine($"NaN detected before drawing node! x: {x}, y: {y}, centerX: {centerX}, subtreeWidth: {subtreeWidth}, height: {height}");
                return; // Or handle the NaN case as appropriate
            }

            heightNodesDrawn[height]++; // Increases the number of nodes drawn at the current height

            // Recursively draw child nodes if this is not a leaf
            if (currentNode.Children != null && currentNode.NumKeys > 0) // This is letting a node with height 0 in which allows for a divide by 0 error.
            {
                height--; // Decrease height since we are going deeper into the tree

                // Initialize new height
                if (!heightNodesDrawn.ContainsKey(height))
                {
                    heightNodesDrawn.Add(height, 0);
                }
                float leftX = centerX - subtreeWidth / 2; // Calculate left x-coordinate of the tree

                /// This was modified to be 0 when initialized for testing with NaN errors
                /// Set this back to non-initialized if errors occur.
                float nodeSlot = 0; // Initialize nodeSlot aka, the space that a node is going to be centered in

                for (int i = 0; i < currentNode.Children.Count; i++)
                {
                    GUINode? childNode = i < currentNode.Children.Count ? currentNode.Children[i] : null; // null fixed?

                    float childX = 0; // Initialize horizontal spacing, aka center of the node's slot

                    // If node is a leaf, calculates custom node slot, if not, divides subtreeWdith in equal parts
                    if (childNode?.IsLeaf == true || childNode?.Children == null) //null fixed?
                    {
                        //nodeSlot = childNode.NodeWidth + nodeSpacing;
                        if (leafStart.Count > 0 && childNode != null)
                        {
                            childX = leftX + leafStart[0] + (childNode.NodeWidth / 2) + 5;
                            leafStart.RemoveAt(0);
                        }
                    }
                    else if (currentNode.height != 0) // REMOVE
                    {
                        nodeSlot = (subtreeWidth / GetNodesAtHeight(currentNode.height - 1).Count); // currentNode.height somehow being 0 here.
                        childX = leftX + (heightNodesDrawn[height] * nodeSlot) + nodeSlot / 2;
                    }
                    else
                    {
                        throw new Exception($"Current node: {currentNode} failed to set it's leftX");
                    }

                    // Log if 'childX' or 'childY' is about to become NaN
                    if (float.IsNaN(childX))
                    {
                        Console.WriteLine($"NaN detected for childX! leftX: {leftX}, nodeSlot: {nodeSlot}, subtreeWidth: {subtreeWidth}, centerX: {centerX}, height: {height}, heightNodesDrawn: {heightNodesDrawn[height]}");
                    }

                    float childY = y + currentNode.NodeHeight + 50; // Vertical spacing: FIX LATER

                    if (childNode != null)
                    {
                        DrawTree(graphics, childNode, centerX, childX, childY, subtreeWidth, heightNodesDrawn, height); // Draw the child subtree
                        graphics.DrawLine(pen, x, y + currentNode.NodeHeight, childX, childY); // Draw line from parent to child
                    }
                }
            }

            // Reset clipping region after drawing, if necessary
            graphics.ResetClip();
        }

        // Finds the starting point for all of the leaf nodes
        public List<float> initializeLeafStart()
        {
            List<float> leafStart = new List<float>(); // Initialized the leafStart List
            float start = 0; // Adds the first float, since the leftmost leaf's slot will start at 0
            List<GUINode> leafList = GetNodesAtHeight(0);
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
            for (int i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i] != null)
                {
                    // Recursively calculate the subtree width of each child
                    width += CalculateSubtreeWidth(node.Children[i]);
                }
            }

            // Add the total width of children to the width of the node
            width += (node.Children.Count - 1) * nodeSpacing; // Add spacing between child nodes

            // Ensures that the width is at least as wide as the node itself to maintain the tree structure
            return Math.Max(width, node.NodeWidth);
        }

        // Calculates the nodes at a certain height
        public List<GUINode> GetNodesAtHeight(int targetHeight)
        {
            List<GUINode> nodesAtHeight = new List<GUINode>();
            Traverse(root, root.height, targetHeight, nodesAtHeight);
            return nodesAtHeight;
        }

        // Traverses through the tree
        private void Traverse(GUINode node, int currentHeight, int targetHeight, List<GUINode> nodesAtHeight)
        {
            if (node == null)
            {
                return;
            }

            if (currentHeight == targetHeight)
            {
                nodesAtHeight.Add(node);
                return;
            }

            if (node.Children != null) 
            {
                // Decrement height and traverse child nodes
                for (int i = 0; i < node.Children.Count(); i++)
                {
                    Traverse(node.Children[i], currentHeight - 1, targetHeight, nodesAtHeight);
                }
            }
        }
    }
}