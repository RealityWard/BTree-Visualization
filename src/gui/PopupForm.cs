namespace B_TreeVisualizationGUI
{
    public partial class PopupForm : Form
    {
        private Form1 mainForm;
        public PopupForm(Form callingForm)
        {
            mainForm = (Form1)callingForm;
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
            mainForm.SearchOrDeleteOverRange(txtStart.Text, txtEnd.Text, "search");
            Close();
        }

        private void btnDeleteRange_Click(object sender, EventArgs e)
        {
            mainForm.SearchOrDeleteOverRange(txtStart.Text, txtEnd.Text, "delete");
            Close();
        }

        private void PopupForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            mainForm.EnableButtonEvents();
        }
    }
}
