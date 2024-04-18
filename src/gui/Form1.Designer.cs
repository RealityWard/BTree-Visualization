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
            int stdBtnWidth = 150;
            int stdBtnSpacing = 20;
            int btnCount = 0;
            int tabCount = 0;
            int GetControlLocation()
            {
                return (stdBtnWidth + stdBtnSpacing) * btnCount++;
            }
            btnInsert = new CustomControls.RJControls.RJButton();
            btnDelete = new CustomControls.RJControls.RJButton();
            btnSearch = new CustomControls.RJControls.RJButton();
            btnclear = new CustomControls.RJControls.RJButton();
            cmbxMaxDegree = new ComboBox();
            txtInputData = new TextBox();
            panel1 = new Panel();
            groupBox1 = new GroupBox();
            lblCurrentProcess = new Label();
            chkDebugMode = new CheckBox();
            btnInsertMany = new CustomControls.RJControls.RJButton();
            trbSpeed = new TrackBar();
            lblSpeed = new Label();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trbSpeed).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(txtInputData);
            groupBox1.Controls.Add(cmbxMaxDegree);
            groupBox1.Controls.Add(btnclear);
            groupBox1.Controls.Add(btnSearch);
            groupBox1.Controls.Add(btnDelete);
            groupBox1.Controls.Add(btnInsert);
            groupBox1.Controls.Add(lblSpeed);
            groupBox1.Controls.Add(trbSpeed);
            groupBox1.Controls.Add(btnInsertMany);
            groupBox1.Controls.Add(chkDebugMode);
            groupBox1.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            groupBox1.Location = new Point(1, 430);
            groupBox1.Dock = DockStyle.Bottom;
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(1000, 50);
            groupBox1.TabIndex = tabCount++;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // txtInputData
            // 
            txtInputData.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            txtInputData.Location = new Point(5 + GetControlLocation(), 1);
            txtInputData.Margin = new Padding(2, 3, 2, 3);
            txtInputData.Name = "txtInputData";
            txtInputData.Size = new Size(stdBtnWidth, 23);
            txtInputData.TabIndex = tabCount++;
            txtInputData.Text = "Insert Data Here...";
            txtInputData.Enter += txt_txtInputData_Enter;
            txtInputData.Leave += txt_txtInputData_Leave;
            // 
            // btnInsert
            // 
            btnInsert.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnInsert.BackColor = Color.MediumSlateBlue;
            btnInsert.BackgroundColor = Color.MediumSlateBlue;
            btnInsert.BorderColor = Color.PaleVioletRed;
            btnInsert.BorderRadius = 20;
            btnInsert.BorderSize = 0;
            btnInsert.FlatAppearance.BorderSize = 0;
            btnInsert.FlatStyle = FlatStyle.Flat;
            btnInsert.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnInsert.ForeColor = Color.White;
            btnInsert.Location = new Point(GetControlLocation(), 1);
            btnInsert.Margin = new Padding(2, 3, 2, 3);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(stdBtnWidth, 40);
            btnInsert.TabIndex = tabCount++;
            btnInsert.Text = "insert";
            btnInsert.TextColor = Color.White;
            btnInsert.UseVisualStyleBackColor = false;
            btnInsert.Click += btnInsert_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnDelete.BackColor = Color.MediumSlateBlue;
            btnDelete.BackgroundColor = Color.MediumSlateBlue;
            btnDelete.BorderColor = Color.PaleVioletRed;
            btnDelete.BorderRadius = 20;
            btnDelete.BorderSize = 0;
            btnDelete.FlatAppearance.BorderSize = 0;
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(GetControlLocation(), 1);
            btnDelete.Margin = new Padding(2, 3, 2, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(stdBtnWidth, 40);
            btnDelete.TabIndex = tabCount++;
            btnDelete.Text = "delete";
            btnDelete.TextColor = Color.White;
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnSearch
            // 
            btnSearch.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnSearch.BackColor = Color.MediumSlateBlue;
            btnSearch.BackgroundColor = Color.MediumSlateBlue;
            btnSearch.BorderColor = Color.PaleVioletRed;
            btnSearch.BorderRadius = 20;
            btnSearch.BorderSize = 0;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(GetControlLocation(), 1);
            btnSearch.Margin = new Padding(2, 3, 2, 3);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(stdBtnWidth, 40);
            btnSearch.TabIndex = tabCount++;
            btnSearch.Text = "search";
            btnSearch.TextColor = Color.White;
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnInsertMany
            // 
            btnSearch.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnInsertMany.BackColor = Color.MediumSlateBlue;
            btnInsertMany.BackgroundColor = Color.MediumSlateBlue;
            btnInsertMany.BorderColor = Color.PaleVioletRed;
            btnInsertMany.BorderRadius = 20;
            btnInsertMany.BorderSize = 0;
            btnInsertMany.FlatAppearance.BorderSize = 0;
            btnInsertMany.FlatStyle = FlatStyle.Flat;
            btnInsertMany.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnInsertMany.ForeColor = Color.White;
            btnInsertMany.Location = new Point(GetControlLocation(), 1);
            btnInsertMany.Margin = new Padding(2, 3, 2, 3);
            btnInsertMany.Name = "btnInsertMany";
            btnInsertMany.Size = new Size(stdBtnWidth, 40);
            btnInsertMany.TabIndex = tabCount++;
            btnInsertMany.Text = "insert many";
            btnInsertMany.TextColor = Color.White;
            btnInsertMany.UseVisualStyleBackColor = false;
            btnInsertMany.Click += btnInsertMany_Click;
            // 
            // btnclear
            // 
            btnclear.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            btnclear.BackColor = Color.MediumSlateBlue;
            btnclear.BackgroundColor = Color.MediumSlateBlue;
            btnclear.BorderColor = Color.PaleVioletRed;
            btnclear.BorderRadius = 20;
            btnclear.BorderSize = 0;
            btnclear.FlatAppearance.BorderSize = 0;
            btnclear.FlatStyle = FlatStyle.Flat;
            btnclear.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnclear.ForeColor = Color.White;
            btnclear.Location = new Point(GetControlLocation(), 1);
            btnclear.Margin = new Padding(2, 3, 2, 3);
            btnclear.Name = "btnclear";
            btnclear.Size = new Size(stdBtnWidth, 40);
            btnclear.TabIndex = tabCount++;
            btnclear.Text = "clear";
            btnclear.TextColor = Color.White;
            btnclear.UseVisualStyleBackColor = false;
            btnclear.Click += btnclear_Click;
            // 
            // cmbxMaxDegree
            // 
            cmbxMaxDegree.Anchor = AnchorStyles.Left | AnchorStyles.Bottom;
            cmbxMaxDegree.BackColor = Color.MediumSlateBlue;
            cmbxMaxDegree.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            cmbxMaxDegree.ForeColor = SystemColors.Window;
            cmbxMaxDegree.FormattingEnabled = true;
            cmbxMaxDegree.Items.AddRange(new object[] { "3", "4", "5", "6", "7" });
            cmbxMaxDegree.Location = new Point(GetControlLocation(), 1);
            cmbxMaxDegree.Margin = new Padding(2, 3, 2, 3);
            cmbxMaxDegree.Name = "cmbxMaxDegree";
            cmbxMaxDegree.Size = new Size(stdBtnWidth, 30);
            cmbxMaxDegree.TabIndex = tabCount++;
            cmbxMaxDegree.Text = "max degree";
            cmbxMaxDegree.SelectedIndexChanged += cmbxMaxDegree_SelectedIndexChanged;
            // 
            // trbSpeed
            // 
            trbSpeed.Location = new Point((stdBtnWidth + stdBtnSpacing) * btnCount - 5, 1);
            trbSpeed.Margin = new Padding(2, 3, 2, 3);
            trbSpeed.Minimum = 1;
            trbSpeed.Name = "trbSpeed";
            trbSpeed.Size = new Size(104, 45);
            trbSpeed.TabIndex = tabCount++;
            trbSpeed.Value = 10;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(GetControlLocation(), 27);
            lblSpeed.Margin = new Padding(2, 0, 2, 0);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(98, 15);
            lblSpeed.TabIndex = tabCount++;
            lblSpeed.Text = "Animation Speed";
            // 
            // chkDebugMode
            // 
            chkDebugMode.AutoSize = true;
            chkDebugMode.Location = new Point(GetControlLocation(), 1);
            chkDebugMode.Margin = new Padding(2, 3, 2, 3);
            chkDebugMode.Name = "chkDebugMode";
            chkDebugMode.Size = new Size(95, 19);
            chkDebugMode.TabIndex = tabCount++;
            chkDebugMode.Text = "Debug Mode";
            chkDebugMode.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.BackColor = SystemColors.ControlDarkDark;
            panel1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            panel1.Location = new Point(179, 12);
            panel1.Margin = new Padding(2, 3, 2, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(1896, 445);
            panel1.TabIndex = tabCount++;
            panel1.Paint += panel1_Paint;
            // 
            // lblCurrentProcess
            // 
            lblCurrentProcess.AutoSize = true;
            lblCurrentProcess.Font = new Font("Consolas", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCurrentProcess.ForeColor = SystemColors.ActiveCaptionText;
            lblCurrentProcess.Location = new Point(128, 904);
            lblCurrentProcess.Margin = new Padding(2, 0, 2, 0);
            lblCurrentProcess.Name = "lblCurrentProcess";
            lblCurrentProcess.Size = new Size(0, 22);
            lblCurrentProcess.TabIndex = tabCount++;
            lblCurrentProcess.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1924, 947);
            Controls.Add(groupBox1);
            Controls.Add(lblCurrentProcess);
            Controls.Add(panel1);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(2, 3, 2, 3);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)trbSpeed).EndInit();
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
        private CheckBox chkDebugMode;
        private CustomControls.RJControls.RJButton btnInsertMany;
        private TrackBar trbSpeed;
        private Label lblSpeed;
        private Label lblCurrentProcess;
        private GroupBox groupBox1;
    }
}