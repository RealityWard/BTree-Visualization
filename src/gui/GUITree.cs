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
        private GUINode root;
        public float centerx, centery;
        private Panel displayPanel;
        

        public GUITree(GUINode root, Panel displayPanel)
        {
            this.root = root;
            this.displayPanel = displayPanel;
            centerx = this.ClientSize.Width / 2;
            centery = this.ClientSize.Height / 2;
        }

        public void DrawTree(Graphics graphics, GUINode currentNode, float x, float y)
        {
            if (currentNode == null) return;

            var pen = new Pen(Color.MediumSlateBlue, 2);

            // Draw node
            currentNode.DisplayNode(graphics, x, y);

            // Recursively draw child nodes if this is not a leaf
            if (currentNode.Children != null)
            {
                for (int i = 0; i < currentNode.NumKeys; i++)
                {
                    // Store parent coordinates
                    float parentX = x;
                    float parentY = y + currentNode.NodeHeight;

                    // Calculate child node position
                    float childX = x - (200 / 2) + (currentNode.NodeWidth * i) + currentNode.NodeWidth / 2;
                    float childY = y + currentNode.NodeHeight + 50;

                    // Draw child
                    DrawTree(graphics, currentNode.Children[i], childX, childY);

                    // Draw link from parent to child
                    graphics.DrawLine(pen, parentX, parentY, childX + currentNode.NodeWidth / 2, childY);
                }
            }
        }
    }
}
