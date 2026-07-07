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
            this.buttonFromReco = new System.Windows.Forms.Button();
            this.buttonFromRecoTab = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.flowButtonsRight.SuspendLayout();
            this.SuspendLayout();
            // 【间距说明】tableLayoutMain：.Padding 为表格外边距；RowStyles 第 3 行 Absolute(48) 为底栏固定高度；
            // 子控件 Margin（如 labelHint）控制单元格内留白；tabControl.Padding 为标签页头区域边距。
            // flowButtonsRight：FlowDirection 决定排列方向；控件 Margin 为按钮之间空隙；WrapContents 为是否换行。
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.labelHint, 0, 0);
            this.tableLayoutMain.Controls.Add(this.tabControl, 0, 1);
            this.tableLayoutMain.Controls.Add(this.panelButtons, 0, 2);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutMain.Size = new System.Drawing.Size(516, 536);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // labelHint
            // 
            this.labelHint.AutoSize = true;
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Location = new System.Drawing.Point(3, 3);
            this.labelHint.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.labelHint.MaximumSize = new System.Drawing.Size(510, 0);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(510, 34);
            this.labelHint.TabIndex = 0;
            this.labelHint.Text = "左/右机台：取料/放料/放料拍照位置(mm)与 RZ(°)；各机台下方可设运动超限报警范围。放料中心点 X/Y/Z 由算法规划自动计算，仅 RZ 在此手动设定。";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageLeft);
            this.tabControl.Controls.Add(this.tabPageRight);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.tabControl.Location = new System.Drawing.Point(3, 48);
            this.tabControl.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(8, 6);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(510, 481);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageLeft
            // 
            this.tabPageLeft.Location = new System.Drawing.Point(4, 32);
            this.tabPageLeft.Name = "tabPageLeft";
            this.tabPageLeft.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageLeft.Size = new System.Drawing.Size(502, 445);
            this.tabPageLeft.TabIndex = 0;
            this.tabPageLeft.Text = "左机台";
            this.tabPageLeft.UseVisualStyleBackColor = true;
            // 
            // tabPageRight
            // 
            this.tabPageRight.Location = new System.Drawing.Point(4, 32);
            this.tabPageRight.Name = "tabPageRight";
            this.tabPageRight.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageRight.Size = new System.Drawing.Size(502, 445);
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
            this.panelButtons.Location = new System.Drawing.Point(3, 535);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(510, 45);
            this.panelButtons.TabIndex = 2;
            // 
            // buttonFromReco
            // 
            this.buttonFromReco.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.buttonFromReco.Location = new System.Drawing.Point(0, 6);
            this.buttonFromReco.Name = "buttonFromReco";
            this.buttonFromReco.Size = new System.Drawing.Size(118, 32);
            this.buttonFromReco.TabIndex = 0;
            this.buttonFromReco.Text = "全部带入XY";
            this.buttonFromReco.UseVisualStyleBackColor = true;
            this.buttonFromReco.Click += new System.EventHandler(this.buttonFromReco_Click);
            // 
            // buttonFromRecoTab
            // 
            this.buttonFromRecoTab.Font = new System.Drawing.Font("Microsoft YaHei UI", 9.5F);
            this.buttonFromRecoTab.Location = new System.Drawing.Point(124, 6);
            this.buttonFromRecoTab.Name = "buttonFromRecoTab";
            this.buttonFromRecoTab.Size = new System.Drawing.Size(118, 32);
            this.buttonFromRecoTab.TabIndex = 1;
            this.buttonFromRecoTab.Text = "本页带入XY";
            this.buttonFromRecoTab.UseVisualStyleBackColor = true;
            this.buttonFromRecoTab.Click += new System.EventHandler(this.buttonFromRecoTab_Click);
            // 
            // flowButtonsRight
            // 
            this.flowButtonsRight.Controls.Add(this.buttonSave);
            this.flowButtonsRight.Controls.Add(this.buttonCancel);
            this.flowButtonsRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtonsRight.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowButtonsRight.Location = new System.Drawing.Point(360, 0);
            this.flowButtonsRight.Name = "flowButtonsRight";
            this.flowButtonsRight.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowButtonsRight.Size = new System.Drawing.Size(150, 45);
            this.flowButtonsRight.TabIndex = 3;
            this.flowButtonsRight.WrapContents = false;
            // 
            // buttonSave
            // 
            this.buttonSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.buttonSave.Location = new System.Drawing.Point(0, 0);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(72, 32);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "保存";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.buttonCancel.Location = new System.Drawing.Point(78, 0);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(72, 32);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // PhotoPositionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(540, 560);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(580, 600);
            this.Name = "PhotoPositionsForm";
            this.Padding = new System.Windows.Forms.Padding(12);
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
