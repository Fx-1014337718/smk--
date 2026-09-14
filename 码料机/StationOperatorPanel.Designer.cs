namespace 码料机
{
    partial class StationOperatorPanel
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
            this.scrollHost = new System.Windows.Forms.Panel();
            this.tableMain = new System.Windows.Forms.TableLayoutPanel();
            this.tableParams = new System.Windows.Forms.TableLayoutPanel();
            this.panelLeftCol = new System.Windows.Forms.TableLayoutPanel();
            this.lblCapProduct = new System.Windows.Forms.Label();
            this.comboProduct = new System.Windows.Forms.ComboBox();
            this.lblProductSpec = new System.Windows.Forms.Label();
            this.lblCapBox = new System.Windows.Forms.Label();
            this.comboBoxSpec = new System.Windows.Forms.ComboBox();
            this.lblBoxSpec = new System.Windows.Forms.Label();
            this.panelRightCol = new System.Windows.Forms.TableLayoutPanel();
            this.lblCapStack = new System.Windows.Forms.Label();
            this.comboStackMode = new System.Windows.Forms.ComboBox();
            this.lblCapQty = new System.Windows.Forms.Label();
            this.panelQty = new System.Windows.Forms.TableLayoutPanel();
            this.lblPickQty = new System.Windows.Forms.Label();
            this.txtPickQty = new System.Windows.Forms.TextBox();
            this.lblPlaceQty = new System.Windows.Forms.Label();
            this.txtPlaceQty = new System.Windows.Forms.TextBox();
            this.panelTrack = new System.Windows.Forms.TableLayoutPanel();
            this.lblTrackBuffer = new System.Windows.Forms.Label();
            this.txtTrackBuffer = new System.Windows.Forms.TextBox();
            this.btnSaveTrackBuffer = new System.Windows.Forms.Button();
            this.panelActions = new System.Windows.Forms.FlowLayoutPanel();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lblCapFrame = new System.Windows.Forms.Label();
            this.panelFrameBtns = new System.Windows.Forms.TableLayoutPanel();
            this.btnFrameChange = new System.Windows.Forms.Button();
            this.btnFrameComplete = new System.Windows.Forms.Button();
            this.lblFrameAllow = new System.Windows.Forms.Label();
            this.chkUseConfiguredPlace = new System.Windows.Forms.CheckBox();
            this.chkManualSlotSelect = new System.Windows.Forms.CheckBox();
            this.scrollHost.SuspendLayout();
            this.tableMain.SuspendLayout();
            this.tableParams.SuspendLayout();
            this.panelLeftCol.SuspendLayout();
            this.panelRightCol.SuspendLayout();
            this.panelQty.SuspendLayout();
            this.panelTrack.SuspendLayout();
            this.panelActions.SuspendLayout();
            this.panelFrameBtns.SuspendLayout();
            this.SuspendLayout();
            // 
            // scrollHost
            // 
            this.scrollHost.AutoScroll = true;
            this.scrollHost.Controls.Add(this.tableMain);
            this.scrollHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrollHost.Location = new System.Drawing.Point(0, 0);
            this.scrollHost.Name = "scrollHost";
            this.scrollHost.Padding = new System.Windows.Forms.Padding(0, 0, 18, 0);
            this.scrollHost.Size = new System.Drawing.Size(460, 560);
            this.scrollHost.TabIndex = 0;
            // 
            // tableMain
            // 
            this.tableMain.AutoSize = true;
            this.tableMain.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableMain.ColumnCount = 1;
            this.tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableMain.Controls.Add(this.tableParams, 0, 0);
            this.tableMain.Controls.Add(this.panelActions, 0, 1);
            this.tableMain.Controls.Add(this.lblCapFrame, 0, 2);
            this.tableMain.Controls.Add(this.panelFrameBtns, 0, 3);
            this.tableMain.Controls.Add(this.chkUseConfiguredPlace, 0, 4);
            this.tableMain.Controls.Add(this.chkManualSlotSelect, 0, 5);
            this.tableMain.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableMain.Location = new System.Drawing.Point(0, 0);
            this.tableMain.Name = "tableMain";
            this.tableMain.Padding = new System.Windows.Forms.Padding(10, 8, 10, 12);
            this.tableMain.RowCount = 6;
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableMain.Size = new System.Drawing.Size(424, 580);
            this.tableMain.TabIndex = 0;
            // 
            // tableParams
            // 
            this.tableParams.AutoSize = true;
            this.tableParams.ColumnCount = 2;
            this.tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 64F));
            this.tableParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 36F));
            this.tableParams.Controls.Add(this.panelLeftCol, 0, 0);
            this.tableParams.Controls.Add(this.panelRightCol, 1, 0);
            this.tableParams.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableParams.Location = new System.Drawing.Point(10, 8);
            this.tableParams.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.tableParams.Name = "tableParams";
            this.tableParams.RowCount = 1;
            this.tableParams.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableParams.Size = new System.Drawing.Size(404, 320);
            this.tableParams.TabIndex = 0;
            // 
            // panelLeftCol
            // 
            this.panelLeftCol.AutoSize = true;
            this.panelLeftCol.ColumnCount = 1;
            this.panelLeftCol.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelLeftCol.Controls.Add(this.lblCapProduct, 0, 0);
            this.panelLeftCol.Controls.Add(this.comboProduct, 0, 1);
            this.panelLeftCol.Controls.Add(this.lblProductSpec, 0, 2);
            this.panelLeftCol.Controls.Add(this.lblCapBox, 0, 3);
            this.panelLeftCol.Controls.Add(this.comboBoxSpec, 0, 4);
            this.panelLeftCol.Controls.Add(this.lblBoxSpec, 0, 5);
            this.panelLeftCol.Controls.Add(this.lblCapStack, 0, 6);
            this.panelLeftCol.Controls.Add(this.comboStackMode, 0, 7);
            this.panelLeftCol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeftCol.Location = new System.Drawing.Point(0, 0);
            this.panelLeftCol.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.panelLeftCol.Name = "panelLeftCol";
            this.panelLeftCol.RowCount = 8;
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelLeftCol.Size = new System.Drawing.Size(202, 360);
            this.panelLeftCol.TabIndex = 0;
            // 
            // lblCapProduct
            // 
            this.lblCapProduct.AutoSize = true;
            this.lblCapProduct.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblCapProduct.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCapProduct.Location = new System.Drawing.Point(0, 0);
            this.lblCapProduct.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblCapProduct.Name = "lblCapProduct";
            this.lblCapProduct.Size = new System.Drawing.Size(69, 20);
            this.lblCapProduct.TabIndex = 0;
            this.lblCapProduct.Text = "产品型号";
            // 
            // comboProduct
            // 
            this.comboProduct.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboProduct.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboProduct.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboProduct.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.comboProduct.FormattingEnabled = true;
            this.comboProduct.Location = new System.Drawing.Point(0, 24);
            this.comboProduct.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.comboProduct.MinimumSize = new System.Drawing.Size(0, 36);
            this.comboProduct.Name = "comboProduct";
            this.comboProduct.Size = new System.Drawing.Size(202, 36);
            this.comboProduct.TabIndex = 1;
            // 
            // lblProductSpec
            // 
            this.lblProductSpec.AutoSize = true;
            this.lblProductSpec.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblProductSpec.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.lblProductSpec.Location = new System.Drawing.Point(0, 64);
            this.lblProductSpec.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblProductSpec.Name = "lblProductSpec";
            this.lblProductSpec.Size = new System.Drawing.Size(17, 19);
            this.lblProductSpec.TabIndex = 2;
            this.lblProductSpec.Text = "—";
            // 
            // lblCapBox
            // 
            this.lblCapBox.AutoSize = true;
            this.lblCapBox.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblCapBox.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCapBox.Location = new System.Drawing.Point(0, 93);
            this.lblCapBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblCapBox.Name = "lblCapBox";
            this.lblCapBox.Size = new System.Drawing.Size(69, 20);
            this.lblCapBox.TabIndex = 3;
            this.lblCapBox.Text = "箱体规格";
            // 
            // comboBoxSpec
            // 
            this.comboBoxSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBoxSpec.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSpec.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboBoxSpec.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.comboBoxSpec.FormattingEnabled = true;
            this.comboBoxSpec.Location = new System.Drawing.Point(0, 117);
            this.comboBoxSpec.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.comboBoxSpec.MinimumSize = new System.Drawing.Size(0, 36);
            this.comboBoxSpec.Name = "comboBoxSpec";
            this.comboBoxSpec.Size = new System.Drawing.Size(202, 36);
            this.comboBoxSpec.TabIndex = 4;
            // 
            // lblBoxSpec
            // 
            this.lblBoxSpec.AutoSize = true;
            this.lblBoxSpec.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblBoxSpec.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.lblBoxSpec.Location = new System.Drawing.Point(0, 157);
            this.lblBoxSpec.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.lblBoxSpec.Name = "lblBoxSpec";
            this.lblBoxSpec.Size = new System.Drawing.Size(17, 19);
            this.lblBoxSpec.TabIndex = 5;
            this.lblBoxSpec.Text = "—";
            // 
            // lblCapStack
            // 
            this.lblCapStack.AutoSize = true;
            this.lblCapStack.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblCapStack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCapStack.Location = new System.Drawing.Point(0, 186);
            this.lblCapStack.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblCapStack.Name = "lblCapStack";
            this.lblCapStack.Size = new System.Drawing.Size(69, 20);
            this.lblCapStack.TabIndex = 6;
            this.lblCapStack.Text = "排料方式";
            // 
            // comboStackMode
            // 
            this.comboStackMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboStackMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboStackMode.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.comboStackMode.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.comboStackMode.FormattingEnabled = true;
            this.comboStackMode.Items.AddRange(new object[] {
            "横向梅花",
            "竖向梅花"});
            this.comboStackMode.Location = new System.Drawing.Point(0, 210);
            this.comboStackMode.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
            this.comboStackMode.MinimumSize = new System.Drawing.Size(0, 36);
            this.comboStackMode.Name = "comboStackMode";
            this.comboStackMode.Size = new System.Drawing.Size(202, 36);
            this.comboStackMode.TabIndex = 7;
            // 
            // panelRightCol
            // 
            this.panelRightCol.AutoSize = true;
            this.panelRightCol.ColumnCount = 1;
            this.panelRightCol.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelRightCol.Controls.Add(this.lblCapQty, 0, 0);
            this.panelRightCol.Controls.Add(this.panelQty, 0, 1);
            this.panelRightCol.Controls.Add(this.panelTrack, 0, 2);
            this.panelRightCol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightCol.Location = new System.Drawing.Point(210, 0);
            this.panelRightCol.Margin = new System.Windows.Forms.Padding(0);
            this.panelRightCol.Name = "panelRightCol";
            this.panelRightCol.RowCount = 3;
            this.panelRightCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelRightCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelRightCol.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelRightCol.Size = new System.Drawing.Size(194, 360);
            this.panelRightCol.TabIndex = 1;
            // 
            // lblCapQty
            // 
            this.lblCapQty.AutoSize = true;
            this.lblCapQty.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblCapQty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblCapQty.Location = new System.Drawing.Point(0, 0);
            this.lblCapQty.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblCapQty.Name = "lblCapQty";
            this.lblCapQty.Size = new System.Drawing.Size(164, 20);
            this.lblCapQty.TabIndex = 0;
            this.lblCapQty.Text = "取放数量（竖直档）";
            // 
            // panelQty
            // 
            this.panelQty.AutoSize = true;
            this.panelQty.ColumnCount = 2;
            this.panelQty.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelQty.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelQty.Controls.Add(this.lblPickQty, 0, 0);
            this.panelQty.Controls.Add(this.txtPickQty, 1, 0);
            this.panelQty.Controls.Add(this.lblPlaceQty, 0, 1);
            this.panelQty.Controls.Add(this.txtPlaceQty, 1, 1);
            this.panelQty.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelQty.Location = new System.Drawing.Point(0, 94);
            this.panelQty.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.panelQty.Name = "panelQty";
            this.panelQty.RowCount = 2;
            this.panelQty.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.panelQty.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.panelQty.Size = new System.Drawing.Size(194, 80);
            this.panelQty.TabIndex = 3;
            // 
            // lblPickQty
            // 
            this.lblPickQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPickQty.AutoSize = true;
            this.lblPickQty.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblPickQty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPickQty.Location = new System.Drawing.Point(0, 10);
            this.lblPickQty.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.lblPickQty.Name = "lblPickQty";
            this.lblPickQty.Size = new System.Drawing.Size(39, 20);
            this.lblPickQty.TabIndex = 0;
            this.lblPickQty.Text = "取料";
            // 
            // txtPickQty
            // 
            this.txtPickQty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPickQty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPickQty.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtPickQty.Location = new System.Drawing.Point(45, 5);
            this.txtPickQty.Margin = new System.Windows.Forms.Padding(0);
            this.txtPickQty.MinimumSize = new System.Drawing.Size(60, 28);
            this.txtPickQty.Name = "txtPickQty";
            this.txtPickQty.ReadOnly = true;
            this.txtPickQty.Size = new System.Drawing.Size(149, 29);
            this.txtPickQty.TabIndex = 1;
            this.txtPickQty.Text = "2";
            this.txtPickQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // lblPlaceQty
            // 
            this.lblPlaceQty.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPlaceQty.AutoSize = true;
            this.lblPlaceQty.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblPlaceQty.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPlaceQty.Location = new System.Drawing.Point(0, 50);
            this.lblPlaceQty.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.lblPlaceQty.Name = "lblPlaceQty";
            this.lblPlaceQty.Size = new System.Drawing.Size(39, 20);
            this.lblPlaceQty.TabIndex = 2;
            this.lblPlaceQty.Text = "放料";
            // 
            // txtPlaceQty
            // 
            this.txtPlaceQty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPlaceQty.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPlaceQty.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtPlaceQty.Location = new System.Drawing.Point(45, 45);
            this.txtPlaceQty.Margin = new System.Windows.Forms.Padding(0);
            this.txtPlaceQty.MinimumSize = new System.Drawing.Size(60, 28);
            this.txtPlaceQty.Name = "txtPlaceQty";
            this.txtPlaceQty.ReadOnly = true;
            this.txtPlaceQty.Size = new System.Drawing.Size(149, 29);
            this.txtPlaceQty.TabIndex = 3;
            this.txtPlaceQty.Text = "2";
            this.txtPlaceQty.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panelTrack
            // 
            this.panelTrack.AutoSize = true;
            this.panelTrack.ColumnCount = 2;
            this.panelTrack.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelTrack.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.panelTrack.Controls.Add(this.lblTrackBuffer, 0, 0);
            this.panelTrack.SetColumnSpan(this.lblTrackBuffer, 2);
            this.panelTrack.Controls.Add(this.txtTrackBuffer, 0, 1);
            this.panelTrack.Controls.Add(this.btnSaveTrackBuffer, 1, 1);
            this.panelTrack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTrack.Location = new System.Drawing.Point(0, 182);
            this.panelTrack.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.panelTrack.Name = "panelTrack";
            this.panelTrack.RowCount = 2;
            this.panelTrack.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelTrack.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.panelTrack.Size = new System.Drawing.Size(194, 68);
            this.panelTrack.TabIndex = 4;
            // 
            // lblTrackBuffer
            // 
            this.lblTrackBuffer.AutoSize = true;
            this.lblTrackBuffer.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.lblTrackBuffer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblTrackBuffer.Location = new System.Drawing.Point(0, 0);
            this.lblTrackBuffer.Margin = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.lblTrackBuffer.Name = "lblTrackBuffer";
            this.lblTrackBuffer.Size = new System.Drawing.Size(69, 20);
            this.lblTrackBuffer.TabIndex = 0;
            this.lblTrackBuffer.Text = "料道缓存";
            // 
            // txtTrackBuffer
            // 
            this.txtTrackBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTrackBuffer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTrackBuffer.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.txtTrackBuffer.Location = new System.Drawing.Point(0, 29);
            this.txtTrackBuffer.Margin = new System.Windows.Forms.Padding(0, 0, 6, 0);
            this.txtTrackBuffer.MinimumSize = new System.Drawing.Size(48, 28);
            this.txtTrackBuffer.Name = "txtTrackBuffer";
            this.txtTrackBuffer.Size = new System.Drawing.Size(120, 29);
            this.txtTrackBuffer.TabIndex = 1;
            this.txtTrackBuffer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnSaveTrackBuffer
            // 
            this.btnSaveTrackBuffer.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.btnSaveTrackBuffer.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnSaveTrackBuffer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveTrackBuffer.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnSaveTrackBuffer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnSaveTrackBuffer.Location = new System.Drawing.Point(126, 26);
            this.btnSaveTrackBuffer.Margin = new System.Windows.Forms.Padding(0);
            this.btnSaveTrackBuffer.MinimumSize = new System.Drawing.Size(56, 32);
            this.btnSaveTrackBuffer.Name = "btnSaveTrackBuffer";
            this.btnSaveTrackBuffer.Size = new System.Drawing.Size(64, 36);
            this.btnSaveTrackBuffer.TabIndex = 2;
            this.btnSaveTrackBuffer.Text = "保存";
            this.btnSaveTrackBuffer.UseVisualStyleBackColor = true;
            this.btnSaveTrackBuffer.Click += new System.EventHandler(this.btnSaveTrackBuffer_Click);
            // 
            // panelActions
            // 
            this.panelActions.AutoSize = true;
            this.panelActions.Controls.Add(this.btnConfirm);
            this.panelActions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelActions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelActions.Location = new System.Drawing.Point(10, 336);
            this.panelActions.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.panelActions.Name = "panelActions";
            this.panelActions.Size = new System.Drawing.Size(404, 48);
            this.panelActions.TabIndex = 1;
            this.panelActions.WrapContents = false;
            // 
            // btnConfirm
            // 
            this.btnConfirm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirm.FlatAppearance.BorderSize = 0;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnConfirm.ForeColor = System.Drawing.Color.White;
            this.btnConfirm.Location = new System.Drawing.Point(204, 3);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnConfirm.MinimumSize = new System.Drawing.Size(180, 42);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(200, 42);
            this.btnConfirm.TabIndex = 0;
            this.btnConfirm.Text = "确定产品与数量";
            this.btnConfirm.UseVisualStyleBackColor = false;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // lblCapFrame
            // 
            this.lblCapFrame.AutoSize = true;
            this.lblCapFrame.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblCapFrame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(65)))), ((int)(((byte)(85)))));
            this.lblCapFrame.Location = new System.Drawing.Point(13, 394);
            this.lblCapFrame.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
            this.lblCapFrame.Name = "lblCapFrame";
            this.lblCapFrame.Size = new System.Drawing.Size(69, 20);
            this.lblCapFrame.TabIndex = 2;
            this.lblCapFrame.Text = "换框操作";
            // 
            // panelFrameBtns
            // 
            this.panelFrameBtns.ColumnCount = 3;
            this.panelFrameBtns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.panelFrameBtns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.34F));
            this.panelFrameBtns.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33F));
            this.panelFrameBtns.Controls.Add(this.btnFrameChange, 0, 0);
            this.panelFrameBtns.Controls.Add(this.btnFrameComplete, 1, 0);
            this.panelFrameBtns.Controls.Add(this.lblFrameAllow, 2, 0);
            this.panelFrameBtns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelFrameBtns.Location = new System.Drawing.Point(10, 420);
            this.panelFrameBtns.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.panelFrameBtns.Name = "panelFrameBtns";
            this.panelFrameBtns.RowCount = 1;
            this.panelFrameBtns.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.panelFrameBtns.Size = new System.Drawing.Size(404, 40);
            this.panelFrameBtns.TabIndex = 3;
            // 
            // btnFrameChange
            // 
            this.btnFrameChange.BackColor = System.Drawing.Color.White;
            this.btnFrameChange.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFrameChange.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFrameChange.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnFrameChange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFrameChange.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnFrameChange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnFrameChange.Location = new System.Drawing.Point(0, 0);
            this.btnFrameChange.Margin = new System.Windows.Forms.Padding(0, 0, 4, 0);
            this.btnFrameChange.Name = "btnFrameChange";
            this.btnFrameChange.Size = new System.Drawing.Size(130, 40);
            this.btnFrameChange.TabIndex = 0;
            this.btnFrameChange.Text = "换框";
            this.btnFrameChange.UseVisualStyleBackColor = false;
            // 
            // btnFrameComplete
            // 
            this.btnFrameComplete.BackColor = System.Drawing.Color.White;
            this.btnFrameComplete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFrameComplete.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFrameComplete.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnFrameComplete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFrameComplete.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnFrameComplete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(13)))), ((int)(((byte)(148)))), ((int)(((byte)(136)))));
            this.btnFrameComplete.Location = new System.Drawing.Point(138, 0);
            this.btnFrameComplete.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.btnFrameComplete.Name = "btnFrameComplete";
            this.btnFrameComplete.Size = new System.Drawing.Size(126, 40);
            this.btnFrameComplete.TabIndex = 1;
            this.btnFrameComplete.Text = "换框完成";
            this.btnFrameComplete.UseVisualStyleBackColor = false;
            // 
            // lblFrameAllow
            // 
            this.lblFrameAllow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(167)))), ((int)(((byte)(243)))), ((int)(((byte)(208)))));
            this.lblFrameAllow.Cursor = System.Windows.Forms.Cursors.Default;
            this.lblFrameAllow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFrameAllow.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblFrameAllow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(95)))), ((int)(((byte)(70)))));
            this.lblFrameAllow.Location = new System.Drawing.Point(272, 0);
            this.lblFrameAllow.Margin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblFrameAllow.MinimumSize = new System.Drawing.Size(0, 36);
            this.lblFrameAllow.Name = "lblFrameAllow";
            this.lblFrameAllow.Size = new System.Drawing.Size(132, 40);
            this.lblFrameAllow.TabIndex = 2;
            this.lblFrameAllow.Text = "禁止取框";
            this.lblFrameAllow.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chkUseConfiguredPlace
            // 
            this.chkUseConfiguredPlace.AutoSize = true;
            this.chkUseConfiguredPlace.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.chkUseConfiguredPlace.Location = new System.Drawing.Point(13, 470);
            this.chkUseConfiguredPlace.Margin = new System.Windows.Forms.Padding(3, 0, 3, 4);
            this.chkUseConfiguredPlace.Name = "chkUseConfiguredPlace";
            this.chkUseConfiguredPlace.Size = new System.Drawing.Size(323, 24);
            this.chkUseConfiguredPlace.TabIndex = 4;
            this.chkUseConfiguredPlace.Text = "机台放料用手动设定位置（不用识箱算位）";
            this.chkUseConfiguredPlace.UseVisualStyleBackColor = true;
            // 
            // chkManualSlotSelect
            // 
            this.chkManualSlotSelect.AutoSize = true;
            this.chkManualSlotSelect.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.chkManualSlotSelect.Location = new System.Drawing.Point(13, 498);
            this.chkManualSlotSelect.Margin = new System.Windows.Forms.Padding(3, 0, 3, 8);
            this.chkManualSlotSelect.Name = "chkManualSlotSelect";
            this.chkManualSlotSelect.Size = new System.Drawing.Size(338, 24);
            this.chkManualSlotSelect.TabIndex = 5;
            this.chkManualSlotSelect.Text = "机台手动指定放料位（算法识位，界面选下一发）";
            this.chkManualSlotSelect.UseVisualStyleBackColor = true;
            // 
            // StationOperatorPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.scrollHost);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.Name = "StationOperatorPanel";
            this.Size = new System.Drawing.Size(460, 560);
            this.scrollHost.ResumeLayout(false);
            this.scrollHost.PerformLayout();
            this.tableMain.ResumeLayout(false);
            this.tableMain.PerformLayout();
            this.tableParams.ResumeLayout(false);
            this.tableParams.PerformLayout();
            this.panelLeftCol.ResumeLayout(false);
            this.panelLeftCol.PerformLayout();
            this.panelRightCol.ResumeLayout(false);
            this.panelRightCol.PerformLayout();
            this.panelQty.ResumeLayout(false);
            this.panelQty.PerformLayout();
            this.panelTrack.ResumeLayout(false);
            this.panelTrack.PerformLayout();
            this.panelActions.ResumeLayout(false);
            this.panelFrameBtns.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel scrollHost;
        private System.Windows.Forms.TableLayoutPanel tableMain;
        private System.Windows.Forms.TableLayoutPanel tableParams;
        private System.Windows.Forms.TableLayoutPanel panelLeftCol;
        private System.Windows.Forms.TableLayoutPanel panelRightCol;
        private System.Windows.Forms.Label lblCapProduct;
        private System.Windows.Forms.ComboBox comboProduct;
        private System.Windows.Forms.Label lblProductSpec;
        private System.Windows.Forms.Label lblCapStack;
        private System.Windows.Forms.ComboBox comboStackMode;
        private System.Windows.Forms.Label lblCapBox;
        private System.Windows.Forms.ComboBox comboBoxSpec;
        private System.Windows.Forms.Label lblBoxSpec;
        private System.Windows.Forms.Label lblCapQty;
        private System.Windows.Forms.TableLayoutPanel panelQty;
        private System.Windows.Forms.Label lblPickQty;
        private System.Windows.Forms.TextBox txtPickQty;
        private System.Windows.Forms.Label lblPlaceQty;
        private System.Windows.Forms.TextBox txtPlaceQty;
        private System.Windows.Forms.TableLayoutPanel panelTrack;
        private System.Windows.Forms.Label lblTrackBuffer;
        private System.Windows.Forms.TextBox txtTrackBuffer;
        private System.Windows.Forms.Button btnSaveTrackBuffer;
        private System.Windows.Forms.FlowLayoutPanel panelActions;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label lblCapFrame;
        private System.Windows.Forms.TableLayoutPanel panelFrameBtns;
        private System.Windows.Forms.Button btnFrameChange;
        private System.Windows.Forms.Button btnFrameComplete;
        private System.Windows.Forms.Label lblFrameAllow;
        private System.Windows.Forms.CheckBox chkUseConfiguredPlace;
        private System.Windows.Forms.CheckBox chkManualSlotSelect;
    }
}
