namespace B_TreeVisualizationGUI
{
    partial class PopupForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnDeleteRange = new CustomControls.RJControls.RJButton();
            btnSearchRange = new CustomControls.RJControls.RJButton();
            txtData = new TextBox();
            txtStart = new TextBox();
            txtEnd = new TextBox();
            SuspendLayout();
            // 
            // btnDeleteRange
            // 
            btnDeleteRange.BackColor = Color.MediumSlateBlue;
            btnDeleteRange.BackgroundColor = Color.MediumSlateBlue;
            btnDeleteRange.BorderColor = Color.PaleVioletRed;
            btnDeleteRange.BorderRadius = 20;
            btnDeleteRange.BorderSize = 0;
            btnDeleteRange.FlatAppearance.BorderSize = 0;
            btnDeleteRange.FlatStyle = FlatStyle.Flat;
            btnDeleteRange.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnDeleteRange.ForeColor = Color.White;
            btnDeleteRange.Location = new Point(170, 73);
            btnDeleteRange.Name = "btnDeleteRange";
            btnDeleteRange.Size = new Size(150, 40);
            btnDeleteRange.TabIndex = 0;
            btnDeleteRange.Text = "Delete";
            btnDeleteRange.TextColor = Color.White;
            btnDeleteRange.UseVisualStyleBackColor = false;
            btnDeleteRange.Click += btnDeleteRange_Click;
            // 
            // btnSearchRange
            // 
            btnSearchRange.BackColor = Color.MediumSlateBlue;
            btnSearchRange.BackgroundColor = Color.MediumSlateBlue;
            btnSearchRange.BorderColor = Color.PaleVioletRed;
            btnSearchRange.BorderRadius = 20;
            btnSearchRange.BorderSize = 0;
            btnSearchRange.FlatAppearance.BorderSize = 0;
            btnSearchRange.FlatStyle = FlatStyle.Flat;
            btnSearchRange.Font = new Font("Consolas", 14.25F, FontStyle.Bold);
            btnSearchRange.ForeColor = Color.White;
            btnSearchRange.Location = new Point(12, 73);
            btnSearchRange.Name = "btnSearchRange";
            btnSearchRange.Size = new Size(150, 40);
            btnSearchRange.TabIndex = 1;
            btnSearchRange.Text = "Search";
            btnSearchRange.TextColor = Color.White;
            btnSearchRange.UseVisualStyleBackColor = false;
            btnSearchRange.Click += btnSearchRange_Click;
            // 
            // txtData
            // 
            txtData.Enabled = false;
            txtData.Location = new Point(12, 44);
            txtData.Name = "txtData";
            txtData.Size = new Size(306, 23);
            txtData.TabIndex = 6;
            txtData.Text = "Insert Data Here...";
            txtData.Enter += txtData_Enter;
            // 
            // txtStart
            // 
            txtStart.Location = new Point(12, 15);
            txtStart.Name = "txtStart";
            txtStart.Size = new Size(150, 23);
            txtStart.TabIndex = 7;
            txtStart.Text = "Insert Start Point...";
            txtStart.Enter += txtStart_Enter;
            // 
            // txtEnd
            // 
            txtEnd.Location = new Point(168, 15);
            txtEnd.Name = "txtEnd";
            txtEnd.Size = new Size(150, 23);
            txtEnd.TabIndex = 8;
            txtEnd.Text = "Insert End Point...";
            txtEnd.Enter += textEnd_Enter;
            // 
            // PopupForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(332, 125);
            Controls.Add(txtEnd);
            Controls.Add(txtStart);
            Controls.Add(txtData);
            Controls.Add(btnSearchRange);
            Controls.Add(btnDeleteRange);
            Name = "PopupForm";
            Text = "Search/Delete Parameters";
            FormClosed += PopupForm_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CustomControls.RJControls.RJButton btnDeleteRange;
        private CustomControls.RJControls.RJButton btnSearchRange;
        private TextBox txtData;
        private TextBox txtStart;
        private TextBox txtEnd;
    }
}