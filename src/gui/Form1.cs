using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using static B_TreeVisualizationGUI.GUITree;

namespace B_TreeVisualizationGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void cmbxMaxDegree_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel1.HorizontalScroll.Enabled = false;
            panel1.HorizontalScroll.Visible = false;
            panel1.HorizontalScroll.Maximum = 0;
            panel1.AutoScroll = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            GUINode[] nodearr = new GUINode[51];

            for (int i = 0; i < 50; i++)
            {
                GUINode[] childrenarr = new GUINode[2];
                for (int j = 0; j < 2; j++)
                {
                    GUINode child = new GUINode(generateKeys(), false, false);
                    childrenarr[j] = child;
                }

                GUINode node = new GUINode(generateKeys(), false, false, childrenarr);
                nodearr[i + 1] = node;
            }

            GUINode root = new GUINode(generateKeys(), false, true, nodearr);

            GUITree tree = new GUITree(root, panel1);

            tree.DrawTree(e.Graphics, root, tree.centerx, 10);
        }

        public int[] generateKeys()
        {
            Random random = new Random();
            int[] keys = new int[random.Next(3)];
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = random.Next(5000);
            }
            return keys;
        }
    }
}