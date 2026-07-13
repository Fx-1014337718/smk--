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
            this.tabPageGlobal = new System.Windows.Forms.TabPage();
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
            this.tableLayoutMain.Size = new System.Drawing.Size(891, 744);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // labelHint
            // 
            this.labelHint.AutoSize = true;
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Location = new System.Drawing.Point(4, 4);
            this.labelHint.Margin = new System.Windows.Forms.Padding(4, 4, 4, 12);
            this.labelHint.MaximumSize = new System.Drawing.Size(855, 0);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(855, 62);
            this.labelHint.TabIndex = 0;
            this.labelHint.Text = "对应 配置文件\\金沃算法.ini；[全局] 为 DLL/启用；[左/右机台] 分别设定采图、托盘、标定、相机等。保存后重新加载。";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageGlobal);
            this.tabControl.Controls.Add(this.tabPageLeft);
            this.tabControl.Controls.Add(this.tabPageRight);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(4, 82);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(8, 6);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(883, 580);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageGlobal
            // 
            this.tabPageGlobal.Location = new System.Drawing.Point(4, 46);
            this.tabPageGlobal.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageGlobal.Name = "tabPageGlobal";
            this.tabPageGlobal.Padding = new System.Windows.Forms.Padding(6);
            this.tabPageGlobal.Size = new System.Drawing.Size(875, 530);
            this.tabPageGlobal.TabIndex = 0;
            this.tabPageGlobal.Text = "全局";
            this.tabPageGlobal.UseVisualStyleBackColor = true;
            // 
            // tabPageLeft
            // 
            this.tabPageLeft.Location = new System.Drawing.Point(4, 46);
            this.tabPageLeft.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageLeft.Name = "tabPageLeft";
            this.tabPageLeft.Padding = new System.Windows.Forms.Padding(6);
            this.tabPageLeft.Size = new System.Drawing.Size(875, 530);
            this.tabPageLeft.TabIndex = 1;
            this.tabPageLeft.Text = "左机台";
            this.tabPageLeft.UseVisualStyleBackColor = true;
            // 
            // tabPageRight
            // 
            this.tabPageRight.Location = new System.Drawing.Point(4, 46);
            this.tabPageRight.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageRight.Name = "tabPageRight";
            this.tabPageRight.Padding = new System.Windows.Forms.Padding(6);
            this.tabPageRight.Size = new System.Drawing.Size(875, 530);
            this.tabPageRight.TabIndex = 2;
            this.tabPageRight.Text = "右机台";
            this.tabPageRight.UseVisualStyleBackColor = true;
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.flowButtonsRight);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(4, 670);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(4, 4, 4, 0);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(883, 68);
            this.panelButtons.TabIndex = 2;
            // 
            // flowButtonsRight
            // 
            this.flowButtonsRight.Controls.Add(this.buttonSave);
            this.flowButtonsRight.Controls.Add(this.buttonCancel);
            this.flowButtonsRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtonsRight.Location = new System.Drawing.Point(658, 0);
            this.flowButtonsRight.Margin = new System.Windows.Forms.Padding(4);
            this.flowButtonsRight.Name = "flowButtonsRight";
            this.flowButtonsRight.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
            this.flowButtonsRight.Size = new System.Drawing.Size(225, 68);
            this.flowButtonsRight.TabIndex = 0;
            this.flowButtonsRight.WrapContents = false;
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(0, 9);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0, 0, 9, 0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(108, 48);
            this.buttonSave.TabIndex = 0;
            this.buttonSave.Text = "保存";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(117, 9);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(108, 48);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // JinwoAlgorithmParamsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(927, 780);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(949, 812);
            this.Name = "JinwoAlgorithmParamsForm";
            this.Padding = new System.Windows.Forms.Padding(18);
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

        private System.Windows.Forms.TabPage tabPageGlobal;

        private System.Windows.Forms.TabPage tabPageLeft;

        private System.Windows.Forms.TabPage tabPageRight;

        private System.Windows.Forms.Panel panelButtons;

        private System.Windows.Forms.FlowLayoutPanel flowButtonsRight;

        private System.Windows.Forms.Button buttonSave;

        private System.Windows.Forms.Button buttonCancel;

    }

}

