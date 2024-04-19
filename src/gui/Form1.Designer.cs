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
            panel1 = new Panel();
            chkDebugMode = new CheckBox();
            txtInputData = new TextBox();
            cmbxMaxDegree = new ComboBox();
            btnclear = new CustomControls.RJControls.RJButton();
            btnInsertMany = new CustomControls.RJControls.RJButton();
            btnSearch = new CustomControls.RJControls.RJButton();
            trbSpeed = new TrackBar();
            btnDelete = new CustomControls.RJControls.RJButton();
            lblSpeed = new Label();
            btnInsert = new CustomControls.RJControls.RJButton();
            lblCurrentProcess = new Label();
            panel2 = new Panel();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trbSpeed).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.BackColor = SystemColors.ControlDarkDark;
            panel1.Controls.Add(chkDebugMode);
            panel1.Location = new Point(0, 0);
            panel1.MinimumSize = new Size(905, 503);
            panel1.Name = "panel1";
            panel1.Size = new Size(905, 503);
            panel1.TabIndex = 6;
            panel1.Paint += panel1_Paint;
            // 
            // chkDebugMode
            // 
            chkDebugMode.AutoSize = true;
            chkDebugMode.Location = new Point(801, 12);
            chkDebugMode.Name = "chkDebugMode";
            chkDebugMode.Size = new Size(95, 19);
            chkDebugMode.TabIndex = 7;
            chkDebugMode.Text = "Debug Mode";
            chkDebugMode.UseVisualStyleBackColor = true;
            chkDebugMode.Visible = false;
            // 
            // txtInputData
            // 
            txtInputData.Anchor = AnchorStyles.Bottom;
            txtInputData.Location = new Point(148, 64);
            txtInputData.Name = "txtInputData";
            txtInputData.Size = new Size(195, 23);
            txtInputData.TabIndex = 5;
            txtInputData.Text = "Insert Data Here...";
            txtInputData.Enter += txt_txtInputData_Enter;
            txtInputData.Leave += txt_txtInputData_Leave;
            // 
            // cmbxMaxDegree
            // 
            cmbxMaxDegree.Anchor = AnchorStyles.Bottom;
            cmbxMaxDegree.BackColor = Color.MediumSlateBlue;
            cmbxMaxDegree.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            cmbxMaxDegree.ForeColor = SystemColors.Window;
            cmbxMaxDegree.FormattingEnabled = true;
            cmbxMaxDegree.Items.AddRange(new object[] { "3", "4", "5", "6", "7" });
            cmbxMaxDegree.Location = new Point(12, 60);
            cmbxMaxDegree.Name = "cmbxMaxDegree";
            cmbxMaxDegree.Size = new Size(130, 30);
            cmbxMaxDegree.TabIndex = 4;
            cmbxMaxDegree.Text = "Degree 3";
            cmbxMaxDegree.SelectedIndexChanged += cmbxMaxDegree_SelectedIndexChanged;
            // 
            // btnclear
            // 
            btnclear.Anchor = AnchorStyles.Bottom;
            btnclear.BackColor = Color.MediumSlateBlue;
            btnclear.BackgroundColor = Color.MediumSlateBlue;
            btnclear.BorderColor = Color.PaleVioletRed;
            btnclear.BorderRadius = 20;
            btnclear.BorderSize = 0;
            btnclear.FlatAppearance.BorderSize = 0;
            btnclear.FlatStyle = FlatStyle.Flat;
            btnclear.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnclear.ForeColor = Color.White;
            btnclear.Location = new Point(746, 12);
            btnclear.Name = "btnclear";
            btnclear.Size = new Size(150, 40);
            btnclear.TabIndex = 3;
            btnclear.Text = "clear";
            btnclear.TextColor = Color.White;
            btnclear.UseVisualStyleBackColor = false;
            btnclear.Click += btnclear_Click;
            // 
            // btnInsertMany
            // 
            btnInsertMany.Anchor = AnchorStyles.Bottom;
            btnInsertMany.BackColor = Color.MediumSlateBlue;
            btnInsertMany.BackgroundColor = Color.MediumSlateBlue;
            btnInsertMany.BorderColor = Color.PaleVioletRed;
            btnInsertMany.BorderRadius = 20;
            btnInsertMany.BorderSize = 0;
            btnInsertMany.FlatAppearance.BorderSize = 0;
            btnInsertMany.FlatStyle = FlatStyle.Flat;
            btnInsertMany.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnInsertMany.ForeColor = Color.White;
            btnInsertMany.Location = new Point(278, 12);
            btnInsertMany.Name = "btnInsertMany";
            btnInsertMany.Size = new Size(150, 40);
            btnInsertMany.TabIndex = 8;
            btnInsertMany.Text = "insert many";
            btnInsertMany.TextColor = Color.White;
            btnInsertMany.UseVisualStyleBackColor = false;
            btnInsertMany.Click += btnInsertMany_Click;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Bottom;
            btnSearch.BackColor = Color.MediumSlateBlue;
            btnSearch.BackgroundColor = Color.MediumSlateBlue;
            btnSearch.BorderColor = Color.PaleVioletRed;
            btnSearch.BorderRadius = 20;
            btnSearch.BorderSize = 0;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(590, 12);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(150, 40);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "search";
            btnSearch.TextColor = Color.White;
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // trbSpeed
            // 
            trbSpeed.Anchor = AnchorStyles.Bottom;
            trbSpeed.Location = new Point(12, 12);
            trbSpeed.Minimum = 1;
            trbSpeed.Name = "trbSpeed";
            trbSpeed.Size = new Size(104, 45);
            trbSpeed.TabIndex = 9;
            trbSpeed.Value = 5;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom;
            btnDelete.BackColor = Color.MediumSlateBlue;
            btnDelete.BackgroundColor = Color.MediumSlateBlue;
            btnDelete.BorderColor = Color.PaleVioletRed;
            btnDelete.BorderRadius = 20;
            btnDelete.BorderSize = 0;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(434, 12);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(150, 40);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "delete";
            btnDelete.TextColor = Color.White;
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // lblSpeed
            // 
            lblSpeed.Anchor = AnchorStyles.Bottom;
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(17, 42);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(98, 15);
            lblSpeed.TabIndex = 10;
            lblSpeed.Text = "Animation Speed";
            // 
            // btnInsert
            // 
            btnInsert.Anchor = AnchorStyles.Bottom;
            btnInsert.BackColor = Color.MediumSlateBlue;
            btnInsert.BackgroundColor = Color.MediumSlateBlue;
            btnInsert.BorderColor = Color.PaleVioletRed;
            btnInsert.BorderRadius = 20;
            btnInsert.BorderSize = 0;
            btnInsert.FlatAppearance.BorderSize = 0;
            btnInsert.FlatStyle = FlatStyle.Flat;
            btnInsert.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnInsert.ForeColor = Color.White;
            btnInsert.Location = new Point(122, 12);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(150, 40);
            btnInsert.TabIndex = 0;
            btnInsert.Text = "insert";
            btnInsert.TextColor = Color.White;
            btnInsert.UseVisualStyleBackColor = false;
            btnInsert.Click += btnInsert_Click;
            // 
            // lblCurrentProcess
            // 
            lblCurrentProcess.Anchor = AnchorStyles.Bottom;
            lblCurrentProcess.AutoSize = true;
            lblCurrentProcess.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCurrentProcess.ForeColor = SystemColors.ActiveCaptionText;
            lblCurrentProcess.Location = new Point(361, 64);
            lblCurrentProcess.Name = "lblCurrentProcess";
            lblCurrentProcess.Size = new Size(0, 21);
            lblCurrentProcess.TabIndex = 11;
            // 
            // panel2
            // 
            panel2.Controls.Add(lblCurrentProcess);
            panel2.Controls.Add(cmbxMaxDegree);
            panel2.Controls.Add(btnclear);
            panel2.Controls.Add(btnInsert);
            panel2.Controls.Add(btnInsertMany);
            panel2.Controls.Add(lblSpeed);
            panel2.Controls.Add(btnSearch);
            panel2.Controls.Add(trbSpeed);
            panel2.Controls.Add(btnDelete);
            panel2.Controls.Add(txtInputData);
            panel2.Location = new Point(0, 502);
            panel2.MinimumSize = new Size(905, 100);
            panel2.Name = "panel2";
            panel2.Size = new Size(905, 100);
            panel2.TabIndex = 12;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(905, 602);
            Controls.Add(panel2);
            Controls.Add(panel1);
            MinimumSize = new Size(921, 641);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            Resize += Form1_Resize;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trbSpeed).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel1;
        private TextBox txtInputData;
        private ComboBox cmbxMaxDegree;
        private CheckBox chkDebugMode;
        private CustomControls.RJControls.RJButton btnclear;
        private CustomControls.RJControls.RJButton btnInsertMany;
        private CustomControls.RJControls.RJButton btnSearch;
        private TrackBar trbSpeed;
        private CustomControls.RJControls.RJButton btnDelete;
        private Label lblSpeed;
        private CustomControls.RJControls.RJButton btnInsert;
        private Label lblCurrentProcess;
        private Panel panel2;
    }
}