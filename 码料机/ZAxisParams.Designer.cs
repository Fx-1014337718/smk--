namespace 码料机
{
    partial class ZAxisParams
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
            this.labelUnitHint = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageLeft = new System.Windows.Forms.TabPage();
            this.tabPageRight = new System.Windows.Forms.TabPage();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.flowButtonsRight = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.flowButtonsRight.SuspendLayout();
            this.SuspendLayout();
            // 【间距说明】tableLayoutMain：.Padding、RowStyles（含底栏 Absolute 高度）、子控件 Margin 控制整体与分区间距；
            // tabControl.Padding 调标签页标题边距；flowButtonsRight 内各按钮 Margin 调按钮间距。
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.labelUnitHint, 0, 0);
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
            this.tableLayoutMain.Size = new System.Drawing.Size(476, 376);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // labelUnitHint
            // 
            this.labelUnitHint.AutoSize = true;
            this.labelUnitHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelUnitHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.labelUnitHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelUnitHint.Location = new System.Drawing.Point(3, 3);
            this.labelUnitHint.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.labelUnitHint.MaximumSize = new System.Drawing.Size(470, 0);
            this.labelUnitHint.Name = "labelUnitHint";
            this.labelUnitHint.Size = new System.Drawing.Size(470, 34);
            this.labelUnitHint.TabIndex = 0;
            this.labelUnitHint.Text = "左/右机台分别设定 Z 轴机械高度；保存后写入 配置文件\\Z轴参数.ini。";
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
            this.tabControl.Size = new System.Drawing.Size(470, 321);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageLeft
            // 
            this.tabPageLeft.Location = new System.Drawing.Point(4, 32);
            this.tabPageLeft.Name = "tabPageLeft";
            this.tabPageLeft.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageLeft.Size = new System.Drawing.Size(462, 285);
            this.tabPageLeft.TabIndex = 0;
            this.tabPageLeft.Text = "左机台";
            this.tabPageLeft.UseVisualStyleBackColor = true;
            // 
            // tabPageRight
            // 
            this.tabPageRight.Location = new System.Drawing.Point(4, 32);
            this.tabPageRight.Name = "tabPageRight";
            this.tabPageRight.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageRight.Size = new System.Drawing.Size(462, 285);
            this.tabPageRight.TabIndex = 1;
            this.tabPageRight.Text = "右机台";
            this.tabPageRight.UseVisualStyleBackColor = true;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.flowButtonsRight);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(3, 375);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(470, 45);
            this.panelButtons.TabIndex = 2;
            // 
            // flowButtonsRight
            // 
            this.flowButtonsRight.Controls.Add(this.buttonSave);
            this.flowButtonsRight.Controls.Add(this.buttonCancel);
            this.flowButtonsRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtonsRight.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowButtonsRight.Location = new System.Drawing.Point(320, 0);
            this.flowButtonsRight.Name = "flowButtonsRight";
            this.flowButtonsRight.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowButtonsRight.Size = new System.Drawing.Size(150, 45);
            this.flowButtonsRight.TabIndex = 2;
            this.flowButtonsRight.WrapContents = false;
            // 
            // buttonSave
            // 
            this.buttonSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.buttonSave.Location = new System.Drawing.Point(0, 0);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(72, 32);
            this.buttonSave.TabIndex = 0;
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
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // ZAxisParams
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(500, 400);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(540, 440);
            this.Name = "ZAxisParams";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.Text = "Z轴参数设定";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZAxisParams_FormClosing);
            this.Load += new System.EventHandler(this.ZAxisParams_Load);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.panelButtons.ResumeLayout(false);
            this.flowButtonsRight.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.Label labelUnitHint;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageLeft;
        private System.Windows.Forms.TabPage tabPageRight;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.FlowLayoutPanel flowButtonsRight;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
    }
}
