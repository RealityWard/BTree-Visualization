namespace B_TreeVisualizationGUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnInsert = new CustomControls.RJControls.RJButton();
            btnDelete = new CustomControls.RJControls.RJButton();
            btnSearch = new CustomControls.RJControls.RJButton();
            btnclear = new CustomControls.RJControls.RJButton();
            cmbxMaxDegree = new ComboBox();
            txtInputData = new TextBox();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // btnInsert
            // 
            btnInsert.BackColor = Color.MediumSlateBlue;
            btnInsert.BackgroundColor = Color.MediumSlateBlue;
            btnInsert.BorderColor = Color.PaleVioletRed;
            btnInsert.BorderRadius = 20;
            btnInsert.BorderSize = 0;
            btnInsert.FlatAppearance.BorderSize = 0;
            btnInsert.FlatStyle = FlatStyle.Flat;
            btnInsert.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnInsert.ForeColor = Color.White;
            btnInsert.Location = new Point(130, 901);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(150, 40);
            btnInsert.TabIndex = 0;
            btnInsert.Text = "insert";
            btnInsert.TextColor = Color.White;
            btnInsert.UseVisualStyleBackColor = false;
            btnInsert.Click += btnInsert_Click;
            // 
            // btnDelete
            // 
            btnDelete.BackColor = Color.MediumSlateBlue;
            btnDelete.BackgroundColor = Color.MediumSlateBlue;
            btnDelete.BorderColor = Color.PaleVioletRed;
            btnDelete.BorderRadius = 20;
            btnDelete.BorderSize = 0;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(286, 901);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(150, 40);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "delete";
            btnDelete.TextColor = Color.White;
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnSearch
            // 
            btnSearch.BackColor = Color.MediumSlateBlue;
            btnSearch.BackgroundColor = Color.MediumSlateBlue;
            btnSearch.BorderColor = Color.PaleVioletRed;
            btnSearch.BorderRadius = 20;
            btnSearch.BorderSize = 0;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(442, 901);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(150, 40);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "search";
            btnSearch.TextColor = Color.White;
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnclear
            // 
            btnclear.BackColor = Color.MediumSlateBlue;
            btnclear.BackgroundColor = Color.MediumSlateBlue;
            btnclear.BorderColor = Color.PaleVioletRed;
            btnclear.BorderRadius = 20;
            btnclear.BorderSize = 0;
            btnclear.FlatAppearance.BorderSize = 0;
            btnclear.FlatStyle = FlatStyle.Flat;
            btnclear.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            btnclear.ForeColor = Color.White;
            btnclear.Location = new Point(598, 901);
            btnclear.Name = "btnclear";
            btnclear.Size = new Size(150, 40);
            btnclear.TabIndex = 3;
            btnclear.Text = "clear";
            btnclear.TextColor = Color.White;
            btnclear.UseVisualStyleBackColor = false;
            btnclear.Click += btnclear_Click;
            // 
            // cmbxMaxDegree
            // 
            cmbxMaxDegree.BackColor = Color.MediumSlateBlue;
            cmbxMaxDegree.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            cmbxMaxDegree.ForeColor = SystemColors.Window;
            cmbxMaxDegree.FormattingEnabled = true;
            cmbxMaxDegree.Items.AddRange(new object[] { "3", "4", "5", "6", "7" });
            cmbxMaxDegree.Location = new Point(754, 907);
            cmbxMaxDegree.Name = "cmbxMaxDegree";
            cmbxMaxDegree.Size = new Size(130, 30);
            cmbxMaxDegree.TabIndex = 4;
            cmbxMaxDegree.Text = "max degree";
            cmbxMaxDegree.SelectedIndexChanged += cmbxMaxDegree_SelectedIndexChanged;
            // 
            // txtInputData
            // 
            txtInputData.Location = new Point(12, 912);
            txtInputData.Name = "txtInputData";
            txtInputData.Size = new Size(100, 23);
            txtInputData.TabIndex = 5;
            txtInputData.Text = "Insert data here";
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(1015, 883);
            panel1.TabIndex = 6;
            panel1.Paint += panel1_Paint;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1039, 947);
            Controls.Add(panel1);
            Controls.Add(txtInputData);
            Controls.Add(cmbxMaxDegree);
            Controls.Add(btnclear);
            Controls.Add(btnSearch);
            Controls.Add(btnDelete);
            Controls.Add(btnInsert);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomControls.RJControls.RJButton btnInsert;
        private CustomControls.RJControls.RJButton btnDelete;
        private CustomControls.RJControls.RJButton btnSearch;
        private CustomControls.RJControls.RJButton btnclear;
        private ComboBox cmbxMaxDegree;
        private TextBox txtInputData;
        private Panel panel1;
    }
}