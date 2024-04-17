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
            lblCurrentProcess = new Label();
            chkDebugMode = new CheckBox();
            btnInsertMany = new CustomControls.RJControls.RJButton();
            trbSpeed = new TrackBar();
            lblSpeed = new Label();
            ((System.ComponentModel.ISupportInitialize)trbSpeed).BeginInit();
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
            btnInsert.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnInsert.ForeColor = Color.White;
            btnInsert.Location = new Point(724, 897);
            btnInsert.Margin = new Padding(2, 3, 2, 3);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(149, 40);
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
            btnDelete.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnDelete.ForeColor = Color.White;
            btnDelete.Location = new Point(877, 896);
            btnDelete.Margin = new Padding(2, 3, 2, 3);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(149, 40);
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
            btnSearch.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnSearch.ForeColor = Color.White;
            btnSearch.Location = new Point(1033, 896);
            btnSearch.Margin = new Padding(2, 3, 2, 3);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(149, 40);
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
            btnclear.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnclear.ForeColor = Color.White;
            btnclear.Location = new Point(1190, 896);
            btnclear.Margin = new Padding(2, 3, 2, 3);
            btnclear.Name = "btnclear";
            btnclear.Size = new Size(149, 40);
            btnclear.TabIndex = 3;
            btnclear.Text = "clear";
            btnclear.TextColor = Color.White;
            btnclear.UseVisualStyleBackColor = false;
            btnclear.Click += btnclear_Click;
            // 
            // cmbxMaxDegree
            // 
            cmbxMaxDegree.BackColor = Color.MediumSlateBlue;
            cmbxMaxDegree.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            cmbxMaxDegree.ForeColor = SystemColors.Window;
            cmbxMaxDegree.FormattingEnabled = true;
            cmbxMaxDegree.Items.AddRange(new object[] { "3", "4", "5", "6", "7" });
            cmbxMaxDegree.Location = new Point(1343, 901);
            cmbxMaxDegree.Margin = new Padding(2, 3, 2, 3);
            cmbxMaxDegree.Name = "cmbxMaxDegree";
            cmbxMaxDegree.Size = new Size(130, 30);
            cmbxMaxDegree.TabIndex = 4;
            cmbxMaxDegree.Text = "max degree";
            cmbxMaxDegree.SelectedIndexChanged += cmbxMaxDegree_SelectedIndexChanged;
            // 
            // txtInputData
            // 
            txtInputData.Location = new Point(509, 904);
            txtInputData.Margin = new Padding(2, 3, 2, 3);
            txtInputData.Name = "txtInputData";
            txtInputData.Size = new Size(195, 23);
            txtInputData.TabIndex = 5;
            txtInputData.Text = "Insert Data Here...";
            txtInputData.Enter += txt_txtInputData_Enter;
            txtInputData.Leave += txt_txtInputData_Leave;
            // 
            // panel1
            // 
            panel1.AutoScroll = true;
            panel1.BackColor = SystemColors.ControlDarkDark;
            panel1.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            panel1.Location = new Point(12, 12);
            panel1.Margin = new Padding(2, 3, 2, 3);
            panel1.Name = "panel1";
            panel1.Size = new Size(2063, 883);
            panel1.TabIndex = 6;
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
            lblCurrentProcess.TabIndex = 11;
            lblCurrentProcess.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // chkDebugMode
            // 
            chkDebugMode.AutoSize = true;
            chkDebugMode.Location = new Point(1978, 908);
            chkDebugMode.Margin = new Padding(2, 3, 2, 3);
            chkDebugMode.Name = "chkDebugMode";
            chkDebugMode.Size = new Size(95, 19);
            chkDebugMode.TabIndex = 7;
            chkDebugMode.Text = "Debug Mode";
            chkDebugMode.UseVisualStyleBackColor = true;
            // 
            // btnInsertMany
            // 
            btnInsertMany.BackColor = Color.MediumSlateBlue;
            btnInsertMany.BackgroundColor = Color.MediumSlateBlue;
            btnInsertMany.BorderColor = Color.PaleVioletRed;
            btnInsertMany.BorderRadius = 20;
            btnInsertMany.BorderSize = 0;
            btnInsertMany.FlatAppearance.BorderSize = 0;
            btnInsertMany.FlatStyle = FlatStyle.Flat;
            btnInsertMany.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnInsertMany.ForeColor = Color.White;
            btnInsertMany.Location = new Point(1820, 898);
            btnInsertMany.Margin = new Padding(2, 3, 2, 3);
            btnInsertMany.Name = "btnInsertMany";
            btnInsertMany.Size = new Size(149, 40);
            btnInsertMany.TabIndex = 8;
            btnInsertMany.Text = "insert many";
            btnInsertMany.TextColor = Color.White;
            btnInsertMany.UseVisualStyleBackColor = false;
            btnInsertMany.Click += btnInsertMany_Click;
            // 
            // trbSpeed
            // 
            trbSpeed.Location = new Point(1712, 892);
            trbSpeed.Margin = new Padding(2, 3, 2, 3);
            trbSpeed.Minimum = 1;
            trbSpeed.Name = "trbSpeed";
            trbSpeed.Size = new Size(104, 45);
            trbSpeed.TabIndex = 9;
            trbSpeed.Value = 10;
            // 
            // lblSpeed
            // 
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(1718, 927);
            lblSpeed.Margin = new Padding(2, 0, 2, 0);
            lblSpeed.Name = "lblSpeed";
            lblSpeed.Size = new Size(98, 15);
            lblSpeed.TabIndex = 10;
            lblSpeed.Text = "Animation Speed";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2087, 947);
            Controls.Add(lblCurrentProcess);
            Controls.Add(lblSpeed);
            Controls.Add(trbSpeed);
            Controls.Add(btnInsertMany);
            Controls.Add(chkDebugMode);
            Controls.Add(panel1);
            Controls.Add(txtInputData);
            Controls.Add(cmbxMaxDegree);
            Controls.Add(btnclear);
            Controls.Add(btnSearch);
            Controls.Add(btnDelete);
            Controls.Add(btnInsert);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(2, 3, 2, 3);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
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
    }
}