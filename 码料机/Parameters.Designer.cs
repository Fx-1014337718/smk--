namespace 码料机
{
    partial class Parameters
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
            this.lblOuter = new System.Windows.Forms.Label();
            this.txtOuter = new System.Windows.Forms.TextBox();
            this.lblInner = new System.Windows.Forms.Label();
            this.txtInner = new System.Windows.Forms.TextBox();
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
            this.tableLayoutMain.Location = new System.Drawing.Point(16, 14);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.Size = new System.Drawing.Size(812, 492);
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
            this.panelHeader.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.RowCount = 1;
            this.panelHeader.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelHeader.Size = new System.Drawing.Size(812, 36);
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
            this.panelTitle.Size = new System.Drawing.Size(446, 36);
            this.panelTitle.TabIndex = 0;
            this.panelTitle.WrapContents = false;
            // 
            // lblAccent
            // 
            this.lblAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.lblAccent.Location = new System.Drawing.Point(0, 6);
            this.lblAccent.Margin = new System.Windows.Forms.Padding(0, 6, 10, 0);
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size(6, 22);
            this.lblAccent.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft YaHei UI", 15F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.lblTitle.Location = new System.Drawing.Point(16, 4);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(0, 4, 12, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(93, 28);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "产品参数";
            // 
            // lblUnit
            // 
            this.lblUnit.AutoSize = true;
            this.lblUnit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblUnit.Location = new System.Drawing.Point(121, 10);
            this.lblUnit.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lblUnit.Name = "lblUnit";
            this.lblUnit.Size = new System.Drawing.Size(70, 21);
            this.lblUnit.TabIndex = 2;
            this.lblUnit.Text = "单位: mm";
            // 
            // lblPathHint
            // 
            this.lblPathHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPathHint.AutoEllipsis = true;
            this.lblPathHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPathHint.Location = new System.Drawing.Point(449, 7);
            this.lblPathHint.Name = "lblPathHint";
            this.lblPathHint.Size = new System.Drawing.Size(360, 21);
            this.lblPathHint.TabIndex = 1;
            this.lblPathHint.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tableBody
            // 
            this.tableBody.ColumnCount = 2;
            this.tableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 240F));
            this.tableBody.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBody.Controls.Add(this.groupList, 0, 0);
            this.tableBody.Controls.Add(this.groupEdit, 1, 0);
            this.tableBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableBody.Location = new System.Drawing.Point(0, 48);
            this.tableBody.Margin = new System.Windows.Forms.Padding(0);
            this.tableBody.Name = "tableBody";
            this.tableBody.RowCount = 1;
            this.tableBody.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableBody.Size = new System.Drawing.Size(812, 380);
            this.tableBody.TabIndex = 1;
            // 
            // groupList
            // 
            this.groupList.Controls.Add(this.listNames);
            this.groupList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupList.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.groupList.Location = new System.Drawing.Point(0, 0);
            this.groupList.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.groupList.Name = "groupList";
            this.groupList.Padding = new System.Windows.Forms.Padding(10, 8, 10, 10);
            this.groupList.Size = new System.Drawing.Size(228, 380);
            this.groupList.TabIndex = 0;
            this.groupList.TabStop = false;
            this.groupList.Text = "型号列表";
            // 
            // listNames
            // 
            this.listNames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listNames.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.listNames.FormattingEnabled = true;
            this.listNames.ItemHeight = 21;
            this.listNames.Location = new System.Drawing.Point(10, 30);
            this.listNames.Name = "listNames";
            this.listNames.Size = new System.Drawing.Size(208, 340);
            this.listNames.TabIndex = 0;
            this.listNames.SelectedIndexChanged += new System.EventHandler(this.listNames_SelectedIndexChanged);
            // 
            // groupEdit
            // 
            this.groupEdit.Controls.Add(this.tableEdit);
            this.groupEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupEdit.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.groupEdit.Location = new System.Drawing.Point(240, 0);
            this.groupEdit.Margin = new System.Windows.Forms.Padding(0);
            this.groupEdit.Name = "groupEdit";
            this.groupEdit.Padding = new System.Windows.Forms.Padding(14, 10, 14, 12);
            this.groupEdit.Size = new System.Drawing.Size(572, 380);
            this.groupEdit.TabIndex = 1;
            this.groupEdit.TabStop = false;
            this.groupEdit.Text = "型号详情";
            // 
            // tableEdit
            // 
            this.tableEdit.ColumnCount = 2;
            this.tableEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableEdit.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableEdit.Controls.Add(this.lblName, 0, 0);
            this.tableEdit.Controls.Add(this.txtName, 0, 1);
            this.tableEdit.Controls.Add(this.lblOuter, 0, 2);
            this.tableEdit.Controls.Add(this.txtOuter, 0, 3);
            this.tableEdit.Controls.Add(this.lblInner, 1, 2);
            this.tableEdit.Controls.Add(this.txtInner, 1, 3);
            this.tableEdit.Controls.Add(this.lblHeight, 0, 4);
            this.tableEdit.Controls.Add(this.txtHeight, 0, 5);
            this.tableEdit.Controls.Add(this.lblHint, 0, 6);
            this.tableEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableEdit.Location = new System.Drawing.Point(14, 32);
            this.tableEdit.Name = "tableEdit";
            this.tableEdit.RowCount = 7;
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableEdit.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableEdit.Size = new System.Drawing.Size(544, 336);
            this.tableEdit.TabIndex = 0;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.tableEdit.SetColumnSpan(this.lblName, 2);
            this.lblName.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblName.Location = new System.Drawing.Point(3, 0);
            this.lblName.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(73, 20);
            this.lblName.TabIndex = 0;
            this.lblName.Text = "型号名称";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableEdit.SetColumnSpan(this.txtName, 2);
            this.txtName.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtName.Location = new System.Drawing.Point(3, 27);
            this.txtName.MinimumSize = new System.Drawing.Size(0, 28);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(538, 29);
            this.txtName.TabIndex = 1;
            // 
            // lblOuter
            // 
            this.lblOuter.AutoSize = true;
            this.lblOuter.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblOuter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblOuter.Location = new System.Drawing.Point(3, 64);
            this.lblOuter.Margin = new System.Windows.Forms.Padding(3, 8, 8, 4);
            this.lblOuter.Name = "lblOuter";
            this.lblOuter.Size = new System.Drawing.Size(81, 20);
            this.lblOuter.TabIndex = 2;
            this.lblOuter.Text = "外径 (mm)";
            // 
            // txtOuter
            // 
            this.txtOuter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOuter.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtOuter.Location = new System.Drawing.Point(3, 91);
            this.txtOuter.Margin = new System.Windows.Forms.Padding(3, 3, 10, 3);
            this.txtOuter.MinimumSize = new System.Drawing.Size(0, 28);
            this.txtOuter.Name = "txtOuter";
            this.txtOuter.Size = new System.Drawing.Size(259, 29);
            this.txtOuter.TabIndex = 3;
            // 
            // lblInner
            // 
            this.lblInner.AutoSize = true;
            this.lblInner.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblInner.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblInner.Location = new System.Drawing.Point(275, 64);
            this.lblInner.Margin = new System.Windows.Forms.Padding(3, 8, 3, 4);
            this.lblInner.Name = "lblInner";
            this.lblInner.Size = new System.Drawing.Size(81, 20);
            this.lblInner.TabIndex = 4;
            this.lblInner.Text = "内径 (mm)";
            // 
            // txtInner
            // 
            this.txtInner.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInner.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtInner.Location = new System.Drawing.Point(275, 91);
            this.txtInner.MinimumSize = new System.Drawing.Size(0, 28);
            this.txtInner.Name = "txtInner";
            this.txtInner.Size = new System.Drawing.Size(266, 29);
            this.txtInner.TabIndex = 5;
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.tableEdit.SetColumnSpan(this.lblHeight, 2);
            this.lblHeight.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblHeight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblHeight.Location = new System.Drawing.Point(3, 128);
            this.lblHeight.Margin = new System.Windows.Forms.Padding(3, 8, 3, 4);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(81, 20);
            this.lblHeight.TabIndex = 6;
            this.lblHeight.Text = "高度 (mm)";
            // 
            // txtHeight
            // 
            this.txtHeight.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tableEdit.SetColumnSpan(this.txtHeight, 2);
            this.txtHeight.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtHeight.Location = new System.Drawing.Point(3, 155);
            this.txtHeight.MinimumSize = new System.Drawing.Size(0, 28);
            this.txtHeight.Name = "txtHeight";
            this.txtHeight.Size = new System.Drawing.Size(538, 29);
            this.txtHeight.TabIndex = 7;
            // 
            // lblHint
            // 
            this.lblHint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top)));
            this.lblHint.AutoSize = true;
            this.tableEdit.SetColumnSpan(this.lblHint, 2);
            this.lblHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lblHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblHint.Location = new System.Drawing.Point(3, 200);
            this.lblHint.Margin = new System.Windows.Forms.Padding(3, 12, 3, 0);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(391, 20);
            this.lblHint.TabIndex = 8;
            this.lblHint.Text = "左侧点选可查询；保存为新增或修改；删除按当前型号名称移除。";
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
            this.panelFooter.Location = new System.Drawing.Point(0, 440);
            this.panelFooter.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.panelFooter.Name = "panelFooter";
            this.panelFooter.RowCount = 1;
            this.panelFooter.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelFooter.Size = new System.Drawing.Size(812, 52);
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
            this.panelLeftButtons.Size = new System.Drawing.Size(406, 52);
            this.panelLeftButtons.TabIndex = 0;
            this.panelLeftButtons.WrapContents = false;
            // 
            // btnNew
            // 
            this.btnNew.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnNew.Location = new System.Drawing.Point(0, 3);
            this.btnNew.Margin = new System.Windows.Forms.Padding(0, 3, 8, 3);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(88, 40);
            this.btnNew.TabIndex = 0;
            this.btnNew.Text = "新建";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnRefresh.Location = new System.Drawing.Point(96, 3);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(0, 3, 8, 3);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(88, 40);
            this.btnRefresh.TabIndex = 1;
            this.btnRefresh.Text = "刷新";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnDelete.Location = new System.Drawing.Point(192, 3);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(88, 40);
            this.btnDelete.TabIndex = 2;
            this.btnDelete.Text = "删除";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // panelRightButtons
            // 
            this.panelRightButtons.AutoSize = true;
            this.panelRightButtons.Controls.Add(this.btnSave);
            this.panelRightButtons.Controls.Add(this.btnClose);
            this.panelRightButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelRightButtons.Location = new System.Drawing.Point(406, 0);
            this.panelRightButtons.Margin = new System.Windows.Forms.Padding(0);
            this.panelRightButtons.Name = "panelRightButtons";
            this.panelRightButtons.Size = new System.Drawing.Size(406, 52);
            this.panelRightButtons.TabIndex = 1;
            this.panelRightButtons.WrapContents = false;
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnClose.Location = new System.Drawing.Point(318, 3);
            this.btnClose.Margin = new System.Windows.Forms.Padding(8, 3, 0, 3);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(88, 40);
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
            this.btnSave.Location = new System.Drawing.Point(222, 3);
            this.btnSave.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(88, 40);
            this.btnSave.TabIndex = 0;
            this.btnSave.Text = "保存";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // Parameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(844, 520);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(760, 480);
            this.Name = "Parameters";
            this.Padding = new System.Windows.Forms.Padding(16, 14, 16, 14);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "产品参数";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Parameters_FormClosing);
            this.Load += new System.EventHandler(this.Parameters_Load);
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
        private System.Windows.Forms.Label lblOuter;
        private System.Windows.Forms.TextBox txtOuter;
        private System.Windows.Forms.Label lblInner;
        private System.Windows.Forms.TextBox txtInner;
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
