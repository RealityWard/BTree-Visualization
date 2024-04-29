


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
            btnPlayAndPause = new CustomControls.RJControls.RJButton();
            btnNext = new CustomControls.RJControls.RJButton();
            lblSpeed = new Label();
            btnInsert = new CustomControls.RJControls.RJButton();
            btnInfo = new CustomControls.RJControls.RJButton();
            lblCurrentProcess = new Label();
            panel2 = new Panel();
            chkBTreeTrue = new CheckBox();
            btnSearchDeleteRange = new CustomControls.RJControls.RJButton();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)trbSpeed).BeginInit();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.AutoScroll = true;
            panel1.BackColor = SystemColors.ControlDarkDark;
            panel1.Controls.Add(chkDebugMode);
            panel1.Location = new Point(0, 0);
            panel1.MinimumSize = new Size(905, 470);
            panel1.Name = "panel1";
            panel1.Size = new Size(905, 502);
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
            txtInputData.Location = new Point(147, 59);
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
            cmbxMaxDegree.Location = new Point(11, 55);
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
            btnclear.Location = new Point(745, 7);
            btnclear.Name = "btnclear";
            btnclear.Size = new Size(150, 40);
            btnclear.TabIndex = 3;
            btnclear.Text = "Clear";
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
            btnInsertMany.Location = new Point(277, 7);
            btnInsertMany.Name = "btnInsertMany";
            btnInsertMany.Size = new Size(150, 40);
            btnInsertMany.TabIndex = 8;
            btnInsertMany.Text = "Insert Many";
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
            btnSearch.Location = new Point(589, 7);
            btnSearch.Name = "btnSearch";
            btnSearch.Size = new Size(150, 40);
            btnSearch.TabIndex = 2;
            btnSearch.Text = "Search";
            btnSearch.TextColor = Color.White;
            btnSearch.UseVisualStyleBackColor = false;
            btnSearch.Click += btnSearch_Click;
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Bottom;
            btnNext.BackColor = Color.MediumSlateBlue;
            btnNext.BackgroundColor = Color.MediumSlateBlue;
            btnNext.BorderColor = Color.PaleVioletRed;
            btnNext.BorderRadius = 2;
            btnNext.BorderSize = 0;
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnNext.ForeColor = Color.White;
            btnNext.Location = new Point(40, 95);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(150, 28);
            btnNext.TabIndex = 13;
            btnNext.Text = "Step";
            btnNext.TextColor = Color.White;
            btnNext.UseVisualStyleBackColor = false;
            btnNext.Click += btnNext_Click;
            // 
            // trbSpeed
            // 
            trbSpeed.Anchor = AnchorStyles.Bottom;
            trbSpeed.Location = new Point(11, 7);
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
            btnDelete.Location = new Point(433, 7);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(150, 40);
            btnDelete.TabIndex = 1;
            btnDelete.Text = "Delete";
            btnDelete.TextColor = Color.White;
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnPlayAndPause
            // 
            btnPlayAndPause.Anchor = AnchorStyles.Bottom;
            btnPlayAndPause.BackColor = Color.MediumSlateBlue;
            btnPlayAndPause.BackgroundColor = Color.MediumSlateBlue;
            btnPlayAndPause.BorderColor = Color.PaleVioletRed;
            btnPlayAndPause.BorderRadius = 20;
            btnPlayAndPause.BorderSize = 0;
            btnPlayAndPause.FlatAppearance.BorderSize = 0;
            btnPlayAndPause.FlatStyle = FlatStyle.Flat;
            btnPlayAndPause.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnPlayAndPause.ForeColor = Color.White;
            btnPlayAndPause.Location = new Point(172, 90);
            btnPlayAndPause.Name = "btnPlayAndPause";
            btnPlayAndPause.Size = new Size(150, 40);
            btnPlayAndPause.TabIndex = 15;
            btnPlayAndPause.Text = "⏯";
            btnPlayAndPause.TextColor = Color.White;
            btnPlayAndPause.UseVisualStyleBackColor = false;
            btnPlayAndPause.Click += btnPlayAndPause_Click;
            // 
            // btnNext
            // 
            btnNext.Anchor = AnchorStyles.Bottom;
            btnNext.BackColor = Color.MediumSlateBlue;
            btnNext.BackgroundColor = Color.MediumSlateBlue;
            btnNext.BorderColor = Color.PaleVioletRed;
            btnNext.BorderRadius = 2;
            btnNext.BorderSize = 0;
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnNext.ForeColor = Color.White;
            btnNext.Location = new Point(16, 96);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(150, 28);
            btnNext.TabIndex = 13;
            btnNext.Text = "Step";
            btnNext.TextColor = Color.White;
            btnNext.UseVisualStyleBackColor = false;
            btnNext.Click += btnNext_Click;
            // 
            // lblSpeed
            // 
            lblSpeed.Anchor = AnchorStyles.Bottom;
            lblSpeed.AutoSize = true;
            lblSpeed.Location = new Point(16, 37);
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
            btnInsert.Location = new Point(121, 7);
            btnInsert.Name = "btnInsert";
            btnInsert.Size = new Size(150, 40);
            btnInsert.TabIndex = 0;
            btnInsert.Text = "Insert";
            btnInsert.TextColor = Color.White;
            btnInsert.UseVisualStyleBackColor = false;
            btnInsert.Click += btnInsert_Click;
            //
            // btnInfo
            //
            btnInfo.Anchor = AnchorStyles.Bottom;
            btnInfo.BackColor = Color.MediumSlateBlue;
            btnInfo.BackgroundColor = Color.MediumSlateBlue;
            btnInfo.BorderColor = Color.PaleVioletRed;
            btnInfo.BorderRadius = 20;
            btnInfo.BorderSize = 0;
            btnInfo.FlatAppearance.BorderSize = 0;
            btnInfo.FlatStyle = FlatStyle.Flat;
            btnInfo.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnInfo.ForeColor = Color.White;
            btnInfo.Location = new Point(750, 95);
            btnInfo.Name = "btnInfo";
            btnInfo.Size = new Size(150, 28);
            btnInfo.TabIndex = 0;
            btnInfo.Text = "info";
            btnInfo.TextColor = Color.White;
            btnInfo.UseVisualStyleBackColor = false;
            btnInfo.Click += btnInfo_Click;
            // 
            // lblCurrentProcess
            // 
            lblCurrentProcess.Anchor = AnchorStyles.Bottom;
            lblCurrentProcess.AutoSize = true;
            lblCurrentProcess.ForeColor = SystemColors.ActiveCaptionText;
            lblCurrentProcess.Location = new Point(359, 62);
            lblCurrentProcess.Name = "lblCurrentProcess";
            lblCurrentProcess.Size = new Size(188, 15);
            lblCurrentProcess.TabIndex = 11;
            lblCurrentProcess.Text = "No Tree Currently Being Processed";
            // 
            // panel2
            // 
            panel2.Controls.Add(btnSearchDeleteRange);
            panel2.Controls.Add(chkBTreeTrue);
            panel2.Controls.Add(lblCurrentProcess);
            panel2.Controls.Add(cmbxMaxDegree);
            panel2.Controls.Add(btnclear);
            panel2.Controls.Add(btnInsert);
            panel2.Controls.Add(btnInsertMany);
            panel2.Controls.Add(lblSpeed);
            panel2.Controls.Add(btnSearch);
            panel2.Controls.Add(trbSpeed);
            panel2.Controls.Add(btnNext);
            panel2.Controls.Add(btnPlayAndPause);
            panel2.Controls.Add(btnDelete);
            panel2.Controls.Add(txtInputData);
            panel2.Controls.Add(btnInfo);
            panel2.Location = new Point(0, 502);
            panel2.MinimumSize = new Size(905, 130);
            panel2.Name = "panel2";
            panel2.Size = new Size(905, 139);
            panel2.TabIndex = 12;
            // 
            // chkBTreeTrue
            // 
            chkBTreeTrue.Anchor = AnchorStyles.Bottom;
            chkBTreeTrue.AutoSize = true;
            chkBTreeTrue.Location = new Point(776, 61);
            chkBTreeTrue.Name = "chkBTreeTrue";
            chkBTreeTrue.Size = new Size(82, 19);
            chkBTreeTrue.TabIndex = 8;
            chkBTreeTrue.Text = "B Plus Tree";
            chkBTreeTrue.UseVisualStyleBackColor = true;
            chkBTreeTrue.CheckedChanged += chkBTreeTrue_CheckedChanged;
            // 
            // btnSearchDeleteRange
            // 
            btnSearchDeleteRange.Anchor = AnchorStyles.Bottom;
            btnSearchDeleteRange.BackColor = Color.MediumSlateBlue;
            btnSearchDeleteRange.BackgroundColor = Color.MediumSlateBlue;
            btnSearchDeleteRange.BorderColor = Color.PaleVioletRed;
            btnSearchDeleteRange.BorderRadius = 20;
            btnSearchDeleteRange.BorderSize = 0;
            btnSearchDeleteRange.FlatAppearance.BorderSize = 0;
            btnSearchDeleteRange.FlatStyle = FlatStyle.Flat;
            btnSearchDeleteRange.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnSearchDeleteRange.ForeColor = Color.White;
            btnSearchDeleteRange.Location = new Point(433, 90);
            btnSearchDeleteRange.Name = "btnSearchDeleteRange";
            btnSearchDeleteRange.Size = new Size(306, 40);
            btnSearchDeleteRange.TabIndex = 16;
            btnSearchDeleteRange.Text = "Search or Delete Over Range";
            btnSearchDeleteRange.TextColor = Color.White;
            btnSearchDeleteRange.UseVisualStyleBackColor = false;
            btnSearchDeleteRange.Click += btnSearchDeleteRange_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(905, 638);
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
        private CustomControls.RJControls.RJButton btnNext;
        private TrackBar trbSpeed;
        private CustomControls.RJControls.RJButton btnPlayAndPause;
        private CustomControls.RJControls.RJButton btnDelete;
        private Label lblSpeed;
        private CustomControls.RJControls.RJButton btnInsert;
        private CustomControls.RJControls.RJButton btnInfo;
        private Label lblCurrentProcess;
        private Panel panel2;
        private CheckBox chkBTreeTrue;
        private CustomControls.RJControls.RJButton btnSearchDeleteRange;
    }
}