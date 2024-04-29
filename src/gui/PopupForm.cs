using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThreadCommunication;

namespace B_TreeVisualizationGUI
{
    public partial class PopupForm : Form
    {
        public PopupForm()
        {
            InitializeComponent();
        }

        private Form1 mainForm = null;
        public PopupForm(Form callingForm)
        {
            mainForm = callingForm as Form1;
            InitializeComponent();
        }

        private void txtData_Enter(object sender, EventArgs e)
        {
            if (txtData.Text == "Insert Data Here...")
            {
                txtData.ForeColor = Color.Black;
                txtData.Text = "";
            }
        }

        private void txtStart_Enter(object sender, EventArgs e)
        {
            if (txtStart.Text == "Insert Start Point...")
            {
                txtStart.ForeColor = Color.Black;
                txtStart.Text = "";
            }
        }

        private void textEnd_Enter(object sender, EventArgs e)
        {
            if (txtEnd.Text == "Insert End Point...")
            {
                txtEnd.ForeColor = Color.Black;
                txtEnd.Text = "";
            }
        }

        private void btnSearchRange_Click(object sender, EventArgs e)
        {
            this.mainForm.SearchOrDeleteOverRange(txtStart.Text, txtEnd.Text, "search");
            this.Close();
        }

        private void btnDeleteRange_Click(object sender, EventArgs e)
        {
            this.mainForm.SearchOrDeleteOverRange(txtStart.Text, txtEnd.Text, "delete");
            this.Close();
        }

        private void PopupForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.mainForm.EnableButtonEvents();
        }
    }
}
