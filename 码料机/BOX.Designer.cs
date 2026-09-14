namespace 码料机
{
    partial class BOX
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
            this.panelHeader = new System.Windows.Forms.TableLayoutPanel();
            this.panelTitle = new System.Windows.Forms.FlowLayoutPanel();
            this.lblAccent = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblUnit = new System.Windows.Forms.Label();
            this.lblPathHint = new System.Windows.Forms.Label();
            this.tableBody = new System.Windows.Forms.TableLayoutPanel();
            this.groupList = new System.Windows.Forms.GroupBox();
            this.listNames = new System.Windows.Forms.ListBox();
            this.groupEdit = new System.Windows.Forms.GroupBox();
            this.tableEdit = new System.Windows.Forms.TableLayoutPanel();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.lblLength = new System.Windows.Forms.Label();
            this.txtLength = new System.Windows.Forms.TextBox();
            this.lblWidth = new System.Windows.Forms.Label();
            this.txtWidth = new System.Windows.Forms.TextBox();
            this.lblHeight = new System.Windows.Forms.Label();
            this.txtHeight = new System.Windows.Forms.TextBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.panelFooter = new System.Windows.Forms.TableLayoutPanel();
            this.panelLeftButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.panelRightButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.panelHeader.SuspendLayout();
            this.panelTitle.SuspendLayout();
            this.tableBody.SuspendLayout();
            this.groupList.SuspendLayout();
            this.groupEdit.SuspendLayout();
            this.tableEdit.SuspendLayout();
            this.panelFooter.SuspendLayout();
            this.panelLeftButtons.SuspendLayout();
            this.panelRightButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 1;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.panelHeader, 0, 0);
            this.tableLayoutMain.Controls.Add(this.tableBody, 0, 1);
            this.tableLayoutMain.Controls.Add(this.panelFooter, 0, 2);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(24, 21);
            this.tableLayoutMain.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.Size = new System.Drawing.Size(1158, 678);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // panelHeader
            // 
            this.panelHeader.AutoSize = true;
            this.panelHeader.ColumnCount = 2;
            this.panelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 55F));
            this.panelHeader.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.panelHeader.Controls.Add(this.panelTitle, 0, 0);
            this.panelHeader.Controls.Add(this.lblPathHint, 1, 0);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 18);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.RowCount = 1;
            this.panelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelHeader.Size = new System.Drawing.Size(1158, 46);
            this.panelHeader.TabIndex = 0;
            // 
            // panelTitle
            // 
            this.panelTitle.AutoSize = true;
            this.panelTitle.Controls.Add(this.lblAccent);
            this.panelTitle.Controls.Add(this.lblTitle);
            this.panelTitle.Controls.Add(this.lblUnit);
            this.panelTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTitle.Location = new System.Drawing.Point(0, 0);
            this.panelTitle.Margin = new System.Windows.Forms.Padding(0);
            this.panelTitle.Name = "panelTitle";
            this.panelTitle.Size = new System.Drawing.Size(636, 46);
            this.panelTitle.TabIndex = 0;
            this.panelTitle.WrapContents = false;
            // 
            // lblAccent
            // 
            this.lblAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.lblAccent.Location = new System.Drawing.Point(0, 9);
            this.lblAccent.Margin = new System.Windows.Forms.Padding(0, 9, 15, 0);
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size(9, 33);
            this.lblAccent.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTitle.Location = new System.Drawing.Point(24, 6);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(0, 6, 18, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(197, 40);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "箱体参数设置";
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblUnit.Location = new System.Drawing.Point(239, 15);
            this.lblUnit.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(119, 31);
            this.lblUnit.TabIndex = 2;
            this.lblUnit.Text = "单位: mm";
            // 
            // lblPathHint
            // 
            this.lblPathHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPathHint.AutoEllipsis = true;
            this.lblPathHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPathHint.Location = new System.Drawing.Point(640, 7);
            this.lblPathHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPathHint.Name = "lblPathHint";
            this.lblPathHint.Size = new System.Drawing.Size(514, 32);
            this.lblPathHint.TabIndex = 1;
            this.lblPathHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableBody
            // 
            this.tableBody.ColumnCount = 2;
            this.tableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 360F));
            this.tableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBody.Controls.Add(this.groupList, 0, 0);
            this.tableBody.Controls.Add(this.groupEdit, 1, 0);
            this.tableBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableBody.Location = new System.Drawing.Point(0, 64);
            this.tableBody.Margin = new System.Windows.Forms.Padding(0);
            this.tableBody.Name = "tableBody";
            this.tableBody.RowCount = 1;
            this.tableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBody.Size = new System.Drawing.Size(1158, 528);
            this.tableBody.TabIndex = 1;
            // 
            // groupList
            // 
            this.groupList.Controls.Add(this.listNames);
            this.groupList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupList.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.groupList.Location = new System.Drawing.Point(0, 0);
            this.groupList.Margin = new System.Windows.Forms.Padding(0, 0, 18, 0);
            this.groupList.Name = "groupList";
            this.groupList.Padding = new System.Windows.Forms.Padding(15, 12, 15, 15);
            this.groupList.Size = new System.Drawing.Size(342, 528);
            this.groupList.TabIndex = 0;
            this.groupList.TabStop = false;
            this.groupList.Text = "箱体列表";
            // 
            // listNames
            // 
            this.listNames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listNames.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.listNames.FormattingEnabled = true;
            this.listNames.ItemHeight = 31;
            this.listNames.Location = new System.Drawing.Point(15, 43);
            this.listNames.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.listNames.Name = "listNames";
            this.listNames.Size = new System.Drawing.Size(312, 470);
            this.listNames.TabIndex = 0;
            this.listNames.SelectedIndexChanged += new System.EventHandler(this.listNames_SelectedIndexChanged);
            // 
            // groupEdit
            // 
            this.groupEdit.Controls.Add(this.tableEdit);
            this.groupEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupEdit.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.groupEdit.Location = new System.Drawing.Point(360, 0);
            this.groupEdit.Margin = new System.Windows.Forms.Padding(0);
            this.groupEdit.Name = "groupEdit";
            this.groupEdit.Padding = new System.Windows.Forms.Padding(21, 15, 21, 18);
            this.groupEdit.Size = new System.Drawing.Size(798, 528);
            this.groupEdit.TabIndex = 1;
            this.groupEdit.TabStop = false;
            this.groupEdit.Text = "箱类型";
            // 
            // tableEdit
            // 
            this.tableEdit.ColumnCount = 1;
            this.tableEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableEdit.Controls.Add(this.lblName, 0, 0);
            this.tableEdit.Controls.Add(this.txtName, 0, 1);
            this.tableEdit.Controls.Add(this.lblLength, 0, 2);
            this.tableEdit.Controls.Add(this.txtLength, 0, 3);
            this.tableEdit.Controls.Add(this.lblWidth, 0, 4);
            this.tableEdit.Controls.Add(this.txtWidth, 0, 5);
            this.tableEdit.Controls.Add(this.lblHeight, 0, 6);
            this.tableEdit.Controls.Add(this.txtHeight, 0, 7);
            this.tableEdit.Controls.Add(this.lblHint, 0, 8);
            this.tableEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableEdit.Location = new System.Drawing.Point(21, 46);
            this.tableEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableEdit.Name = "tableEdit";
            this.tableEdit.RowCount = 9;
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableEdit.Size = new System.Drawing.Size(756, 464);
            this.tableEdit.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblName.Location = new System.Drawing.Point(4, 0);
            this.lblName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 6);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(79, 30);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "箱类型";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtName.Location = new System.Drawing.Point(4, 47);
            this.txtName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtName.MaximumSize = new System.Drawing.Size(478, 4);
            this.txtName.MinimumSize = new System.Drawing.Size(4, 28);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(478, 28);
            this.txtName.TabIndex = 1;
            // 
            // lblLength
            // 
            this.lblLength.AutoSize = true;
            this.lblLength.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblLength.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblLength.Location = new System.Drawing.Point(4, 108);
            this.lblLength.Margin = new System.Windows.Forms.Padding(4, 12, 4, 6);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(120, 30);
            this.lblLength.TabIndex = 2;
            this.lblLength.Text = "箱长 (mm)";
            // 
            // txtLength
            // 
            this.txtLength.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtLength.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtLength.Location = new System.Drawing.Point(4, 155);
            this.txtLength.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLength.MaximumSize = new System.Drawing.Size(418, 4);
            this.txtLength.MinimumSize = new System.Drawing.Size(4, 28);
            this.txtLength.Name = "txtLength";
            this.txtLength.Size = new System.Drawing.Size(418, 28);
            this.txtLength.TabIndex = 3;
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblWidth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblWidth.Location = new System.Drawing.Point(4, 216);
            this.lblWidth.Margin = new System.Windows.Forms.Padding(4, 12, 4, 6);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(120, 30);
            this.lblWidth.TabIndex = 4;
            this.lblWidth.Text = "箱宽 (mm)";
            // 
            // txtWidth
            // 
            this.txtWidth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtWidth.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtWidth.Location = new System.Drawing.Point(4, 263);
            this.txtWidth.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtWidth.MaximumSize = new System.Drawing.Size(418, 4);
            this.txtWidth.MinimumSize = new System.Drawing.Size(4, 28);
            this.txtWidth.Name = "txtWidth";
            this.txtWidth.Size = new System.Drawing.Size(418, 28);
            this.txtWidth.TabIndex = 5;
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblHeight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblHeight.Location = new System.Drawing.Point(4, 324);
            this.lblHeight.Margin = new System.Windows.Forms.Padding(4, 12, 4, 6);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(120, 30);
            this.lblHeight.TabIndex = 6;
            this.lblHeight.Text = "箱高 (mm)";
            // 
            // txtHeight
            // 
            this.txtHeight.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.txtHeight.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtHeight.Location = new System.Drawing.Point(4, 371);
            this.txtHeight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtHeight.MaximumSize = new System.Drawing.Size(418, 4);
            this.txtHeight.MinimumSize = new System.Drawing.Size(4, 28);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(418, 28);
            this.txtHeight.TabIndex = 7;
            // 
            // lblHint
            // 
            this.lblHint.AutoSize = true;
            this.lblHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblHint.Location = new System.Drawing.Point(4, 438);
            this.lblHint.Margin = new System.Windows.Forms.Padding(4, 18, 4, 0);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(592, 26);
            this.lblHint.TabIndex = 8;
            this.lblHint.Text = "左侧点选可查询；保存为新增或修改；删除按当前箱类型名移除。";
            // 
            // panelFooter
            // 
            this.panelFooter.AutoSize = true;
            this.panelFooter.ColumnCount = 2;
            this.panelFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panelFooter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.panelFooter.Controls.Add(this.panelLeftButtons, 0, 0);
            this.panelFooter.Controls.Add(this.panelRightButtons, 1, 0);
            this.panelFooter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFooter.Location = new System.Drawing.Point(0, 610);
            this.panelFooter.Margin = new System.Windows.Forms.Padding(0, 18, 0, 0);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.RowCount = 1;
            this.panelFooter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelFooter.Size = new System.Drawing.Size(1158, 68);
            this.panelFooter.TabIndex = 2;
            // 
            // panelLeftButtons
            // 
            this.panelLeftButtons.AutoSize = true;
            this.panelLeftButtons.Controls.Add(this.btnNew);
            this.panelLeftButtons.Controls.Add(this.btnRefresh);
            this.panelLeftButtons.Controls.Add(this.btnDelete);
            this.panelLeftButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeftButtons.Location = new System.Drawing.Point(0, 0);
            this.panelLeftButtons.Margin = new System.Windows.Forms.Padding(0);
            this.panelLeftButtons.Name = "panelLeftButtons";
            this.panelLeftButtons.Size = new System.Drawing.Size(579, 68);
            this.panelLeftButtons.TabIndex = 0;
            this.panelLeftButtons.WrapContents = false;
            // 
            // btnNew
            // 
            this.btnNew.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnNew.Location = new System.Drawing.Point(0, 4);
            this.btnNew.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(132, 60);
            this.btnNew.TabIndex = 0;
            this.btnNew.Text = "新建";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnRefresh.Location = new System.Drawing.Point(144, 4);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(0, 4, 12, 4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(132, 60);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnDelete.Location = new System.Drawing.Point(288, 4);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(132, 60);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // panelRightButtons
            // 
            this.panelRightButtons.AutoSize = true;
            this.panelRightButtons.Controls.Add(this.btnClose);
            this.panelRightButtons.Controls.Add(this.btnSave);
            this.panelRightButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelRightButtons.Location = new System.Drawing.Point(579, 0);
            this.panelRightButtons.Margin = new System.Windows.Forms.Padding(0);
            this.panelRightButtons.Name = "panelRightButtons";
            this.panelRightButtons.Size = new System.Drawing.Size(579, 68);
            this.panelRightButtons.TabIndex = 1;
            this.panelRightButtons.WrapContents = false;
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnClose.Location = new System.Drawing.Point(447, 4);
            this.btnClose.Margin = new System.Windows.Forms.Padding(12, 4, 0, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(132, 60);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(303, 4);
            this.btnSave.Margin = new System.Windows.Forms.Padding(0, 4, 0, 4);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(132, 60);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // BOX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1206, 720);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1069, 632);
            this.Name = "BOX";
            this.Padding = new System.Windows.Forms.Padding(24, 21, 24, 21);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "箱体参数设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BOX_FormClosing);
            this.Load += new System.EventHandler(this.BOX_Load);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.panelTitle.ResumeLayout(false);
            this.panelTitle.PerformLayout();
            this.tableBody.ResumeLayout(false);
            this.groupList.ResumeLayout(false);
            this.groupEdit.ResumeLayout(false);
            this.tableEdit.ResumeLayout(false);
            this.tableEdit.PerformLayout();
            this.panelFooter.ResumeLayout(false);
            this.panelFooter.PerformLayout();
            this.panelLeftButtons.ResumeLayout(false);
            this.panelRightButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.TableLayoutPanel panelHeader;
        private System.Windows.Forms.FlowLayoutPanel panelTitle;
        private System.Windows.Forms.Label lblAccent;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblUnit;
        private System.Windows.Forms.Label lblPathHint;
        private System.Windows.Forms.TableLayoutPanel tableBody;
        private System.Windows.Forms.GroupBox groupList;
        private System.Windows.Forms.ListBox listNames;
        private System.Windows.Forms.GroupBox groupEdit;
        private System.Windows.Forms.TableLayoutPanel tableEdit;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.TextBox txtLength;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.TextBox txtWidth;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.TextBox txtHeight;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.TableLayoutPanel panelFooter;
        private System.Windows.Forms.FlowLayoutPanel panelLeftButtons;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.FlowLayoutPanel panelRightButtons;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSave;
    }
}
