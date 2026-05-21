namespace 码料机
{
    partial class JinwoAlgorithmParamsForm
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
            this.tabPageAlgorithm = new System.Windows.Forms.TabPage();
            this.tabPageHik = new System.Windows.Forms.TabPage();
            this.tabPageTray = new System.Windows.Forms.TabPage();
            this.tabPageCalib = new System.Windows.Forms.TabPage();
            this.tabPageUndist = new System.Windows.Forms.TabPage();
            this.tabPageNinePoint = new System.Windows.Forms.TabPage();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.flowButtonsRight = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
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
            this.tableLayoutMain.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 48F));
            this.tableLayoutMain.Size = new System.Drawing.Size(576, 496);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // labelHint
            // 
            this.labelHint.AutoSize = true;
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Location = new System.Drawing.Point(3, 3);
            this.labelHint.Margin = new System.Windows.Forms.Padding(3, 3, 3, 8);
            this.labelHint.MaximumSize = new System.Drawing.Size(570, 0);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(570, 34);
            this.labelHint.TabIndex = 0;
            this.labelHint.Text = "对应 配置文件\\金沃算法.ini；保存后重新加载 DLL 与海康相机。黑圆顺序：左上(0)→右上(1)→右下(2)→左下(3)。";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageAlgorithm);
            this.tabControl.Controls.Add(this.tabPageHik);
            this.tabControl.Controls.Add(this.tabPageTray);
            this.tabControl.Controls.Add(this.tabPageCalib);
            this.tabControl.Controls.Add(this.tabPageUndist);
            this.tabControl.Controls.Add(this.tabPageNinePoint);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(3, 48);
            this.tabControl.Margin = new System.Windows.Forms.Padding(3, 3, 3, 3);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(8, 6);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(570, 441);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageAlgorithm
            // 
            this.tabPageAlgorithm.Location = new System.Drawing.Point(4, 32);
            this.tabPageAlgorithm.Name = "tabPageAlgorithm";
            this.tabPageAlgorithm.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageAlgorithm.Size = new System.Drawing.Size(562, 405);
            this.tabPageAlgorithm.TabIndex = 0;
            this.tabPageAlgorithm.Text = "算法";
            this.tabPageAlgorithm.UseVisualStyleBackColor = true;
            // 
            // tabPageHik
            // 
            this.tabPageHik.Location = new System.Drawing.Point(4, 32);
            this.tabPageHik.Name = "tabPageHik";
            this.tabPageHik.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageHik.Size = new System.Drawing.Size(562, 405);
            this.tabPageHik.TabIndex = 1;
            this.tabPageHik.Text = "海康相机";
            this.tabPageHik.UseVisualStyleBackColor = true;
            // 
            // tabPageTray
            // 
            this.tabPageTray.Location = new System.Drawing.Point(4, 32);
            this.tabPageTray.Name = "tabPageTray";
            this.tabPageTray.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageTray.Size = new System.Drawing.Size(562, 405);
            this.tabPageTray.TabIndex = 2;
            this.tabPageTray.Text = "托盘";
            this.tabPageTray.UseVisualStyleBackColor = true;
            // 
            // tabPageCalib
            // 
            this.tabPageCalib.Location = new System.Drawing.Point(4, 32);
            this.tabPageCalib.Name = "tabPageCalib";
            this.tabPageCalib.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageCalib.Size = new System.Drawing.Size(562, 405);
            this.tabPageCalib.TabIndex = 3;
            this.tabPageCalib.Text = "标定";
            this.tabPageCalib.UseVisualStyleBackColor = true;
            // 
            // tabPageUndist
            // 
            this.tabPageUndist.Location = new System.Drawing.Point(4, 32);
            this.tabPageUndist.Name = "tabPageUndist";
            this.tabPageUndist.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageUndist.Size = new System.Drawing.Size(562, 405);
            this.tabPageUndist.TabIndex = 4;
            this.tabPageUndist.Text = "畸变矫正";
            this.tabPageUndist.UseVisualStyleBackColor = true;
            // 
            // tabPageNinePoint
            // 
            this.tabPageNinePoint.Location = new System.Drawing.Point(4, 32);
            this.tabPageNinePoint.Name = "tabPageNinePoint";
            this.tabPageNinePoint.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageNinePoint.Size = new System.Drawing.Size(562, 405);
            this.tabPageNinePoint.TabIndex = 5;
            this.tabPageNinePoint.Text = "九点标定";
            this.tabPageNinePoint.UseVisualStyleBackColor = true;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.flowButtonsRight);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(3, 495);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(570, 45);
            this.panelButtons.TabIndex = 2;
            // 
            // flowButtonsRight
            // 
            this.flowButtonsRight.Controls.Add(this.buttonSave);
            this.flowButtonsRight.Controls.Add(this.buttonCancel);
            this.flowButtonsRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtonsRight.FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight;
            this.flowButtonsRight.Location = new System.Drawing.Point(420, 0);
            this.flowButtonsRight.Name = "flowButtonsRight";
            this.flowButtonsRight.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowButtonsRight.Size = new System.Drawing.Size(150, 45);
            this.flowButtonsRight.TabIndex = 0;
            this.flowButtonsRight.WrapContents = false;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(0, 6);
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
            this.buttonCancel.Location = new System.Drawing.Point(78, 6);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(72, 32);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // JinwoAlgorithmParamsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(600, 520);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(600, 520);
            this.Name = "JinwoAlgorithmParamsForm";
            this.Padding = new System.Windows.Forms.Padding(12);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "金沃算法设定";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.JinwoAlgorithmParamsForm_FormClosing);
            this.Load += new System.EventHandler(this.JinwoAlgorithmParamsForm_Load);
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
        private System.Windows.Forms.TabPage tabPageAlgorithm;
        private System.Windows.Forms.TabPage tabPageHik;
        private System.Windows.Forms.TabPage tabPageTray;
        private System.Windows.Forms.TabPage tabPageCalib;
        private System.Windows.Forms.TabPage tabPageUndist;
        private System.Windows.Forms.TabPage tabPageNinePoint;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.FlowLayoutPanel flowButtonsRight;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
    }
}
