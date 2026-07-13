namespace 码料机
{
    partial class PhotoPositionsForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.labelHint = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageLeft = new System.Windows.Forms.TabPage();
            this.tabPageRight = new System.Windows.Forms.TabPage();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.flowButtonsRight = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonFromReco = new System.Windows.Forms.Button();
            this.buttonFromRecoTab = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.flowButtonsRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.labelHint, 0, 0);
            this.tableLayoutMain.Controls.Add(this.tabControl, 0, 1);
            this.tableLayoutMain.Controls.Add(this.panelButtons, 0, 2);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(18, 18);
            this.tableLayoutMain.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableLayoutMain.Size = new System.Drawing.Size(801, 804);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // labelHint
            // 
            this.labelHint.AutoSize = true;
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Location = new System.Drawing.Point(4, 4);
            this.labelHint.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.labelHint.MaximumSize = new System.Drawing.Size(765, 0);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(765, 60);
            this.labelHint.TabIndex = 0;
            this.labelHint.Text = "按左/右机台维护常用点位。空白 XY 可直接带入最近一次识别结果；放料中心点只需要手动设 RZ。";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageLeft);
            this.tabControl.Controls.Add(this.tabPageRight);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.tabControl.Location = new System.Drawing.Point(4, 80);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(8, 6);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(793, 642);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageLeft
            // 
            this.tabPageLeft.Location = new System.Drawing.Point(4, 46);
            this.tabPageLeft.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageLeft.Name = "tabPageLeft";
            this.tabPageLeft.Padding = new System.Windows.Forms.Padding(6);
            this.tabPageLeft.Size = new System.Drawing.Size(785, 592);
            this.tabPageLeft.TabIndex = 0;
            this.tabPageLeft.Text = "左机台";
            this.tabPageLeft.UseVisualStyleBackColor = true;
            // 
            // tabPageRight
            // 
            this.tabPageRight.Location = new System.Drawing.Point(4, 46);
            this.tabPageRight.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageRight.Name = "tabPageRight";
            this.tabPageRight.Padding = new System.Windows.Forms.Padding(6);
            this.tabPageRight.Size = new System.Drawing.Size(785, 592);
            this.tabPageRight.TabIndex = 1;
            this.tabPageRight.Text = "右机台";
            this.tabPageRight.UseVisualStyleBackColor = true;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.flowButtonsRight);
            this.panelButtons.Controls.Add(this.buttonFromReco);
            this.panelButtons.Controls.Add(this.buttonFromRecoTab);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(4, 730);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(793, 68);
            this.panelButtons.TabIndex = 2;
            // 
            // flowButtonsRight
            // 
            this.flowButtonsRight.Controls.Add(this.buttonSave);
            this.flowButtonsRight.Controls.Add(this.buttonCancel);
            this.flowButtonsRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtonsRight.Location = new System.Drawing.Point(568, 0);
            this.flowButtonsRight.Margin = new System.Windows.Forms.Padding(4);
            this.flowButtonsRight.Name = "flowButtonsRight";
            this.flowButtonsRight.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
            this.flowButtonsRight.Size = new System.Drawing.Size(225, 68);
            this.flowButtonsRight.TabIndex = 3;
            this.flowButtonsRight.WrapContents = false;
            // 
            // buttonSave
            // 
            this.buttonSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.buttonSave.Location = new System.Drawing.Point(0, 9);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0, 0, 9, 0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(108, 48);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "保存";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.buttonCancel.Location = new System.Drawing.Point(117, 9);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(108, 48);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonFromReco
            // 
            this.buttonFromReco.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.buttonFromReco.Location = new System.Drawing.Point(0, 9);
            this.buttonFromReco.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFromReco.Name = "buttonFromReco";
            this.buttonFromReco.Size = new System.Drawing.Size(177, 48);
            this.buttonFromReco.TabIndex = 0;
            this.buttonFromReco.Text = "全部带入识别XY";
            this.buttonFromReco.UseVisualStyleBackColor = true;
            this.buttonFromReco.Click += new System.EventHandler(this.buttonFromReco_Click);
            // 
            // buttonFromRecoTab
            // 
            this.buttonFromRecoTab.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.buttonFromRecoTab.Location = new System.Drawing.Point(186, 9);
            this.buttonFromRecoTab.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFromRecoTab.Name = "buttonFromRecoTab";
            this.buttonFromRecoTab.Size = new System.Drawing.Size(177, 48);
            this.buttonFromRecoTab.TabIndex = 1;
            this.buttonFromRecoTab.Text = "本页带入识别XY";
            this.buttonFromRecoTab.UseVisualStyleBackColor = true;
            this.buttonFromRecoTab.Click += new System.EventHandler(this.buttonFromRecoTab_Click);
            // 
            // PhotoPositionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(837, 840);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(859, 872);
            this.Name = "PhotoPositionsForm";
            this.Padding = new System.Windows.Forms.Padding(18);
            this.Text = "位置设定";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PhotoPositionsForm_FormClosing);
            this.Load += new System.EventHandler(this.PhotoPositionsForm_Load);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.flowButtonsRight.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.Label labelHint;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageLeft;
        private System.Windows.Forms.TabPage tabPageRight;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.FlowLayoutPanel flowButtonsRight;
        private System.Windows.Forms.Button buttonFromReco;
        private System.Windows.Forms.Button buttonFromRecoTab;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
    }
}
