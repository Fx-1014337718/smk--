namespace 码料机
{
    partial class StationProductionSelectForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// 布局可在 Visual Studio 设计器中拖拽调整；业务逻辑在 StationProductionSelectForm.cs。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.labelHint = new System.Windows.Forms.Label();
            this.labelMode = new System.Windows.Forms.Label();
            this.comboMode = new System.Windows.Forms.ComboBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.flowButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.flowButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 2;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.labelHint, 0, 0);
            this.tableLayoutMain.Controls.Add(this.labelMode, 0, 1);
            this.tableLayoutMain.Controls.Add(this.comboMode, 1, 1);
            this.tableLayoutMain.Controls.Add(this.panelButtons, 0, 2);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(16, 16);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 56F));
            this.tableLayoutMain.Size = new System.Drawing.Size(368, 148);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // labelHint
            // 
            this.tableLayoutMain.SetColumnSpan(this.labelHint, 2);
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Location = new System.Drawing.Point(3, 0);
            this.labelHint.Margin = new System.Windows.Forms.Padding(3, 0, 3, 10);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(362, 40);
            this.labelHint.TabIndex = 0;
            this.labelHint.Text = "仅向 PLC D4414 下发工位生产选择；值未变化不发送，重启软件也不自动下发。";
            // 
            // labelMode
            // 
            this.labelMode.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelMode.AutoSize = true;
            this.labelMode.Location = new System.Drawing.Point(11, 63);
            this.labelMode.Name = "labelMode";
            this.labelMode.Size = new System.Drawing.Size(86, 21);
            this.labelMode.TabIndex = 1;
            this.labelMode.Text = "生产工位:";
            this.labelMode.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // comboMode
            // 
            this.comboMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboMode.FormattingEnabled = true;
            this.comboMode.IntegralHeight = false;
            this.comboMode.Items.AddRange(new object[] {
            "A工位生产",
            "B工位生产",
            "A-B工位生产"});
            this.comboMode.Location = new System.Drawing.Point(103, 59);
            this.comboMode.MinimumSize = new System.Drawing.Size(0, 26);
            this.comboMode.Name = "comboMode";
            this.comboMode.Size = new System.Drawing.Size(262, 29);
            this.comboMode.TabIndex = 2;
            // 
            // panelButtons
            // 
            this.tableLayoutMain.SetColumnSpan(this.panelButtons, 2);
            this.panelButtons.Controls.Add(this.flowButtons);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(3, 95);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(362, 50);
            this.panelButtons.TabIndex = 3;
            // 
            // flowButtons
            // 
            this.flowButtons.Controls.Add(this.buttonOk);
            this.flowButtons.Controls.Add(this.buttonCancel);
            this.flowButtons.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowButtons.Location = new System.Drawing.Point(146, 0);
            this.flowButtons.Name = "flowButtons";
            this.flowButtons.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowButtons.Size = new System.Drawing.Size(216, 50);
            this.flowButtons.TabIndex = 0;
            this.flowButtons.WrapContents = false;
            // 
            // buttonOk
            // 
            this.buttonOk.Location = new System.Drawing.Point(108, 6);
            this.buttonOk.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(100, 36);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "下发";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(0, 6);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 36);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "关闭";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // StationProductionSelectForm
            // 
            this.AcceptButton = this.buttonOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.buttonCancel;
            this.ClientSize = new System.Drawing.Size(400, 180);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StationProductionSelectForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "机械臂控制 — 工位生产选择";
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.flowButtons.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.Label labelMode;
        private System.Windows.Forms.ComboBox comboMode;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.FlowLayoutPanel flowButtons;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonCancel;
    }
}
