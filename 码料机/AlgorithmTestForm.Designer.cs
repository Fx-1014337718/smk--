namespace 码料机
{
    partial class AlgorithmTestForm
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
        /// 布局可在 Visual Studio 设计器中拖拽调整；业务逻辑在 AlgorithmTestForm.cs。
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutRoot = new System.Windows.Forms.TableLayoutPanel();
            this.panelImagePath = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPathRow = new System.Windows.Forms.TableLayoutPanel();
            this.lblImage = new System.Windows.Forms.Label();
            this.txtImage = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.btnMainImage = new System.Windows.Forms.Button();
            this.flowCapture = new System.Windows.Forms.FlowLayoutPanel();
            this.btnHikCapture = new System.Windows.Forms.Button();
            this.previewSplit = new System.Windows.Forms.SplitContainer();
            this.panelPreview = new System.Windows.Forms.Panel();
            this.previewToolbarHost = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSavePreview = new System.Windows.Forms.Button();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.lblRenderPath = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tabTests = new System.Windows.Forms.TabControl();
            this.tabPresence = new System.Windows.Forms.TabPage();
            this.layoutPresence = new System.Windows.Forms.TableLayoutPanel();
            this.lblPresenceHint = new System.Windows.Forms.Label();
            this.flowPresence = new System.Windows.Forms.FlowLayoutPanel();
            this.btnRunPresence = new System.Windows.Forms.Button();
            this.tabJinwo = new System.Windows.Forms.TabPage();
            this.layoutJinwo = new System.Windows.Forms.TableLayoutPanel();
            this.lblJinwoHint = new System.Windows.Forms.Label();
            this.flowJinwo = new System.Windows.Forms.FlowLayoutPanel();
            this.btnMarkers = new System.Windows.Forms.Button();
            this.lblPlaced = new System.Windows.Forms.Label();
            this.numPlaced = new System.Windows.Forms.NumericUpDown();
            this.btnPose = new System.Windows.Forms.Button();
            this.btnPlan = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.TableLayoutPanel();
            this.lblDllStatus = new System.Windows.Forms.Label();
            this.flowBottom = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnReload = new System.Windows.Forms.Button();
            this.tableLayoutRoot.SuspendLayout();
            this.panelImagePath.SuspendLayout();
            this.tableLayoutPathRow.SuspendLayout();
            this.flowCapture.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewSplit)).BeginInit();
            this.previewSplit.Panel1.SuspendLayout();
            this.previewSplit.Panel2.SuspendLayout();
            this.previewSplit.SuspendLayout();
            this.panelPreview.SuspendLayout();
            this.previewToolbarHost.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.tabTests.SuspendLayout();
            this.tabPresence.SuspendLayout();
            this.layoutPresence.SuspendLayout();
            this.flowPresence.SuspendLayout();
            this.tabJinwo.SuspendLayout();
            this.layoutJinwo.SuspendLayout();
            this.flowJinwo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPlaced)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.flowBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutRoot
            // 
            this.tableLayoutRoot.ColumnCount = 1;
            this.tableLayoutRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutRoot.Controls.Add(this.panelImagePath, 0, 0);
            this.tableLayoutRoot.Controls.Add(this.previewSplit, 0, 1);
            this.tableLayoutRoot.Controls.Add(this.tabTests, 0, 2);
            this.tableLayoutRoot.Controls.Add(this.panelBottom, 0, 3);
            this.tableLayoutRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutRoot.Location = new System.Drawing.Point(21, 18);
            this.tableLayoutRoot.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutRoot.Name = "tableLayoutRoot";
            this.tableLayoutRoot.RowCount = 4;
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78F));
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 22F));
            this.tableLayoutRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutRoot.Size = new System.Drawing.Size(1578, 1041);
            this.tableLayoutRoot.TabIndex = 0;
            // 
            // panelImagePath
            // 
            this.panelImagePath.AutoSize = true;
            this.panelImagePath.ColumnCount = 1;
            this.panelImagePath.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelImagePath.Controls.Add(this.tableLayoutPathRow, 0, 0);
            this.panelImagePath.Controls.Add(this.flowCapture, 0, 1);
            this.panelImagePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelImagePath.Location = new System.Drawing.Point(0, 0);
            this.panelImagePath.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.panelImagePath.Name = "panelImagePath";
            this.panelImagePath.RowCount = 2;
            this.panelImagePath.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelImagePath.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelImagePath.Size = new System.Drawing.Size(1578, 134);
            this.panelImagePath.TabIndex = 0;
            // 
            // tableLayoutPathRow
            // 
            this.tableLayoutPathRow.AutoSize = true;
            this.tableLayoutPathRow.ColumnCount = 4;
            this.tableLayoutPathRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
            this.tableLayoutPathRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPathRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPathRow.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPathRow.Controls.Add(this.lblImage, 0, 0);
            this.tableLayoutPathRow.Controls.Add(this.txtImage, 1, 0);
            this.tableLayoutPathRow.Controls.Add(this.btnBrowse, 2, 0);
            this.tableLayoutPathRow.Controls.Add(this.btnMainImage, 3, 0);
            this.tableLayoutPathRow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPathRow.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPathRow.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPathRow.Name = "tableLayoutPathRow";
            this.tableLayoutPathRow.RowCount = 1;
            this.tableLayoutPathRow.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPathRow.Size = new System.Drawing.Size(1578, 74);
            this.tableLayoutPathRow.TabIndex = 0;
            // 
            // lblImage
            // 
            this.lblImage.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblImage.AutoSize = true;
            this.lblImage.Location = new System.Drawing.Point(0, 12);
            this.lblImage.Margin = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.lblImage.Name = "lblImage";
            this.lblImage.Size = new System.Drawing.Size(86, 62);
            this.lblImage.TabIndex = 0;
            this.lblImage.Text = "测试图像";
            // 
            // txtImage
            // 
            this.txtImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtImage.Location = new System.Drawing.Point(112, 4);
            this.txtImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtImage.Name = "txtImage";
            this.txtImage.Size = new System.Drawing.Size(1143, 38);
            this.txtImage.TabIndex = 1;
            // 
            // btnBrowse
            // 
            this.btnBrowse.AutoSize = true;
            this.btnBrowse.Location = new System.Drawing.Point(1268, 3);
            this.btnBrowse.Margin = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(112, 46);
            this.btnBrowse.TabIndex = 2;
            this.btnBrowse.Text = "浏览…";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // btnMainImage
            // 
            this.btnMainImage.AutoSize = true;
            this.btnMainImage.Location = new System.Drawing.Point(1389, 3);
            this.btnMainImage.Margin = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.btnMainImage.Name = "btnMainImage";
            this.btnMainImage.Size = new System.Drawing.Size(189, 46);
            this.btnMainImage.TabIndex = 3;
            this.btnMainImage.Text = "主界面当前图";
            this.btnMainImage.UseVisualStyleBackColor = true;
            this.btnMainImage.Click += new System.EventHandler(this.btnMainImage_Click);
            // 
            // flowCapture
            // 
            this.flowCapture.AutoSize = true;
            this.flowCapture.Controls.Add(this.btnHikCapture);
            this.flowCapture.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowCapture.Location = new System.Drawing.Point(0, 74);
            this.flowCapture.Margin = new System.Windows.Forms.Padding(0);
            this.flowCapture.Name = "flowCapture";
            this.flowCapture.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowCapture.Size = new System.Drawing.Size(1578, 60);
            this.flowCapture.TabIndex = 1;
            this.flowCapture.WrapContents = false;
            // 
            // btnHikCapture
            // 
            this.btnHikCapture.AutoSize = true;
            this.btnHikCapture.Location = new System.Drawing.Point(4, 10);
            this.btnHikCapture.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnHikCapture.Name = "btnHikCapture";
            this.btnHikCapture.Size = new System.Drawing.Size(135, 46);
            this.btnHikCapture.TabIndex = 0;
            this.btnHikCapture.Text = "海康采图";
            this.btnHikCapture.UseVisualStyleBackColor = true;
            this.btnHikCapture.Click += new System.EventHandler(this.btnHikCapture_Click);
            // 
            // previewSplit
            // 
            this.previewSplit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewSplit.Location = new System.Drawing.Point(4, 150);
            this.previewSplit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.previewSplit.Name = "previewSplit";
            // 
            // previewSplit.Panel1
            // 
            this.previewSplit.Panel1.Controls.Add(this.panelPreview);
            // 
            // previewSplit.Panel2
            // 
            this.previewSplit.Panel2.Controls.Add(this.txtLog);
            this.previewSplit.Size = new System.Drawing.Size(1570, 602);
            this.previewSplit.SplitterDistance = 1080;
            this.previewSplit.SplitterWidth = 9;
            this.previewSplit.TabIndex = 1;
            // 
            // panelPreview
            // 
            this.panelPreview.Controls.Add(this.previewToolbarHost);
            this.panelPreview.Controls.Add(this.picPreview);
            this.panelPreview.Controls.Add(this.lblRenderPath);
            this.panelPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelPreview.Location = new System.Drawing.Point(0, 0);
            this.panelPreview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelPreview.Name = "panelPreview";
            this.panelPreview.Size = new System.Drawing.Size(1080, 602);
            this.panelPreview.TabIndex = 0;
            this.panelPreview.Resize += new System.EventHandler(this.panelPreview_Resize);
            // 
            // previewToolbarHost
            // 
            this.previewToolbarHost.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.previewToolbarHost.AutoSize = true;
            this.previewToolbarHost.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.previewToolbarHost.BackColor = System.Drawing.Color.Transparent;
            this.previewToolbarHost.Controls.Add(this.btnSavePreview);
            this.previewToolbarHost.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.previewToolbarHost.Location = new System.Drawing.Point(924, 12);
            this.previewToolbarHost.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.previewToolbarHost.Name = "previewToolbarHost";
            this.previewToolbarHost.Size = new System.Drawing.Size(144, 66);
            this.previewToolbarHost.TabIndex = 2;
            this.previewToolbarHost.WrapContents = false;
            // 
            // btnSavePreview
            // 
            this.btnSavePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(70)))), ((int)(((byte)(229)))));
            this.btnSavePreview.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSavePreview.FlatAppearance.BorderSize = 0;
            this.btnSavePreview.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSavePreview.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.btnSavePreview.ForeColor = System.Drawing.Color.White;
            this.btnSavePreview.Location = new System.Drawing.Point(0, 0);
            this.btnSavePreview.Margin = new System.Windows.Forms.Padding(0);
            this.btnSavePreview.Name = "btnSavePreview";
            this.btnSavePreview.Size = new System.Drawing.Size(144, 66);
            this.btnSavePreview.TabIndex = 0;
            this.btnSavePreview.TabStop = false;
            this.btnSavePreview.Text = "保存图片";
            this.btnSavePreview.UseVisualStyleBackColor = false;
            this.btnSavePreview.Click += new System.EventHandler(this.btnSavePreview_Click);
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.picPreview.Location = new System.Drawing.Point(0, 0);
            this.picPreview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(1080, 554);
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // lblRenderPath
            // 
            this.lblRenderPath.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblRenderPath.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.lblRenderPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblRenderPath.Location = new System.Drawing.Point(0, 554);
            this.lblRenderPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblRenderPath.Name = "lblRenderPath";
            this.lblRenderPath.Size = new System.Drawing.Size(1080, 48);
            this.lblRenderPath.TabIndex = 1;
            this.lblRenderPath.Text = "渲染图：—";
            this.lblRenderPath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(481, 602);
            this.txtLog.TabIndex = 0;
            // 
            // tabTests
            // 
            this.tabTests.Controls.Add(this.tabPresence);
            this.tabTests.Controls.Add(this.tabJinwo);
            this.tabTests.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabTests.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.tabTests.Location = new System.Drawing.Point(4, 760);
            this.tabTests.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabTests.Name = "tabTests";
            this.tabTests.SelectedIndex = 0;
            this.tabTests.Size = new System.Drawing.Size(1570, 164);
            this.tabTests.TabIndex = 2;
            // 
            // tabPresence
            // 
            this.tabPresence.Controls.Add(this.layoutPresence);
            this.tabPresence.Location = new System.Drawing.Point(4, 40);
            this.tabPresence.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabPresence.Name = "tabPresence";
            this.tabPresence.Padding = new System.Windows.Forms.Padding(15, 15, 15, 12);
            this.tabPresence.Size = new System.Drawing.Size(1562, 120);
            this.tabPresence.TabIndex = 0;
            this.tabPresence.Text = "有无料识别";
            this.tabPresence.UseVisualStyleBackColor = true;
            // 
            // layoutPresence
            // 
            this.layoutPresence.ColumnCount = 1;
            this.layoutPresence.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutPresence.Controls.Add(this.lblPresenceHint, 0, 0);
            this.layoutPresence.Controls.Add(this.flowPresence, 0, 1);
            this.layoutPresence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutPresence.Location = new System.Drawing.Point(15, 15);
            this.layoutPresence.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.layoutPresence.Name = "layoutPresence";
            this.layoutPresence.RowCount = 2;
            this.layoutPresence.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPresence.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutPresence.Size = new System.Drawing.Size(1532, 93);
            this.layoutPresence.TabIndex = 0;
            // 
            // lblPresenceHint
            // 
            this.lblPresenceHint.AutoSize = true;
            this.lblPresenceHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPresenceHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblPresenceHint.Location = new System.Drawing.Point(4, 0);
            this.lblPresenceHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 12);
            this.lblPresenceHint.Name = "lblPresenceHint";
            this.lblPresenceHint.Size = new System.Drawing.Size(1524, 31);
            this.lblPresenceHint.TabIndex = 0;
            this.lblPresenceHint.Text = "效果图目录：算法测试效果图\\有无料\\。返回值>0 表示检测到轴承。";
            // 
            // flowPresence
            // 
            this.flowPresence.AutoSize = true;
            this.flowPresence.Controls.Add(this.btnRunPresence);
            this.flowPresence.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowPresence.Location = new System.Drawing.Point(0, 43);
            this.flowPresence.Margin = new System.Windows.Forms.Padding(0);
            this.flowPresence.Name = "flowPresence";
            this.flowPresence.Size = new System.Drawing.Size(1532, 92);
            this.flowPresence.TabIndex = 1;
            this.flowPresence.WrapContents = false;
            // 
            // btnRunPresence
            // 
            this.btnRunPresence.AutoSize = true;
            this.btnRunPresence.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnRunPresence.FlatAppearance.BorderSize = 0;
            this.btnRunPresence.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRunPresence.ForeColor = System.Drawing.Color.White;
            this.btnRunPresence.Location = new System.Drawing.Point(0, 0);
            this.btnRunPresence.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.btnRunPresence.Name = "btnRunPresence";
            this.btnRunPresence.Padding = new System.Windows.Forms.Padding(15, 9, 15, 9);
            this.btnRunPresence.Size = new System.Drawing.Size(318, 80);
            this.btnRunPresence.TabIndex = 0;
            this.btnRunPresence.Text = "运行有无料识别";
            this.btnRunPresence.UseVisualStyleBackColor = false;
            this.btnRunPresence.Click += new System.EventHandler(this.btnRunPresence_Click);
            // 
            // tabJinwo
            // 
            this.tabJinwo.Controls.Add(this.layoutJinwo);
            this.tabJinwo.Location = new System.Drawing.Point(4, 40);
            this.tabJinwo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabJinwo.Name = "tabJinwo";
            this.tabJinwo.Padding = new System.Windows.Forms.Padding(15, 15, 15, 12);
            this.tabJinwo.Size = new System.Drawing.Size(1561, 121);
            this.tabJinwo.TabIndex = 1;
            this.tabJinwo.Text = "位置识别（金沃）";
            this.tabJinwo.UseVisualStyleBackColor = true;
            // 
            // layoutJinwo
            // 
            this.layoutJinwo.ColumnCount = 1;
            this.layoutJinwo.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.layoutJinwo.Controls.Add(this.lblJinwoHint, 0, 0);
            this.layoutJinwo.Controls.Add(this.flowJinwo, 0, 1);
            this.layoutJinwo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.layoutJinwo.Location = new System.Drawing.Point(15, 15);
            this.layoutJinwo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.layoutJinwo.Name = "layoutJinwo";
            this.layoutJinwo.RowCount = 2;
            this.layoutJinwo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutJinwo.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.layoutJinwo.Size = new System.Drawing.Size(1531, 94);
            this.layoutJinwo.TabIndex = 0;
            // 
            // lblJinwoHint
            // 
            this.lblJinwoHint.AutoSize = true;
            this.lblJinwoHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblJinwoHint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.lblJinwoHint.Location = new System.Drawing.Point(4, 0);
            this.lblJinwoHint.Margin = new System.Windows.Forms.Padding(4, 0, 4, 12);
            this.lblJinwoHint.Name = "lblJinwoHint";
            this.lblJinwoHint.Size = new System.Drawing.Size(1523, 31);
            this.lblJinwoHint.TabIndex = 0;
            this.lblJinwoHint.Text = "金沃渲染图分别保存至：金沃_黑圆 / 金沃_单点算位 / 金沃_全箱规划（均在「算法测试效果图」下）。需先「确认产品与数量」。";
            // 
            // flowJinwo
            // 
            this.flowJinwo.AutoSize = true;
            this.flowJinwo.Controls.Add(this.btnMarkers);
            this.flowJinwo.Controls.Add(this.lblPlaced);
            this.flowJinwo.Controls.Add(this.numPlaced);
            this.flowJinwo.Controls.Add(this.btnPose);
            this.flowJinwo.Controls.Add(this.btnPlan);
            this.flowJinwo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowJinwo.Location = new System.Drawing.Point(0, 43);
            this.flowJinwo.Margin = new System.Windows.Forms.Padding(0);
            this.flowJinwo.Name = "flowJinwo";
            this.flowJinwo.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.flowJinwo.Size = new System.Drawing.Size(1531, 98);
            this.flowJinwo.TabIndex = 1;
            // 
            // btnMarkers
            // 
            this.btnMarkers.AutoSize = true;
            this.btnMarkers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(163)))), ((int)(((byte)(74)))));
            this.btnMarkers.FlatAppearance.BorderSize = 0;
            this.btnMarkers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMarkers.ForeColor = System.Drawing.Color.White;
            this.btnMarkers.Location = new System.Drawing.Point(0, 6);
            this.btnMarkers.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.btnMarkers.Name = "btnMarkers";
            this.btnMarkers.Padding = new System.Windows.Forms.Padding(15, 9, 15, 9);
            this.btnMarkers.Size = new System.Drawing.Size(210, 80);
            this.btnMarkers.TabIndex = 0;
            this.btnMarkers.Text = "黑圆检测";
            this.btnMarkers.UseVisualStyleBackColor = false;
            this.btnMarkers.Click += new System.EventHandler(this.btnMarkers_Click);
            // 
            // lblPlaced
            // 
            this.lblPlaced.AutoSize = true;
            this.lblPlaced.Location = new System.Drawing.Point(240, 18);
            this.lblPlaced.Margin = new System.Windows.Forms.Padding(18, 12, 6, 0);
            this.lblPlaced.Name = "lblPlaced";
            this.lblPlaced.Size = new System.Drawing.Size(110, 31);
            this.lblPlaced.TabIndex = 1;
            this.lblPlaced.Text = "已放件数";
            // 
            // numPlaced
            // 
            this.numPlaced.Location = new System.Drawing.Point(360, 10);
            this.numPlaced.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.numPlaced.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
            this.numPlaced.Name = "numPlaced";
            this.numPlaced.Size = new System.Drawing.Size(108, 38);
            this.numPlaced.TabIndex = 2;
            // 
            // btnPose
            // 
            this.btnPose.AutoSize = true;
            this.btnPose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(37)))), ((int)(((byte)(99)))), ((int)(((byte)(235)))));
            this.btnPose.FlatAppearance.BorderSize = 0;
            this.btnPose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPose.ForeColor = System.Drawing.Color.White;
            this.btnPose.Location = new System.Drawing.Point(472, 6);
            this.btnPose.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.btnPose.Name = "btnPose";
            this.btnPose.Padding = new System.Windows.Forms.Padding(15, 9, 15, 9);
            this.btnPose.Size = new System.Drawing.Size(210, 80);
            this.btnPose.TabIndex = 3;
            this.btnPose.Text = "单点算位";
            this.btnPose.UseVisualStyleBackColor = false;
            this.btnPose.Click += new System.EventHandler(this.btnPose_Click);
            // 
            // btnPlan
            // 
            this.btnPlan.AutoSize = true;
            this.btnPlan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(58)))), ((int)(((byte)(237)))));
            this.btnPlan.FlatAppearance.BorderSize = 0;
            this.btnPlan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlan.ForeColor = System.Drawing.Color.White;
            this.btnPlan.Location = new System.Drawing.Point(694, 6);
            this.btnPlan.Margin = new System.Windows.Forms.Padding(0, 0, 12, 12);
            this.btnPlan.Name = "btnPlan";
            this.btnPlan.Padding = new System.Windows.Forms.Padding(15, 9, 15, 9);
            this.btnPlan.Size = new System.Drawing.Size(282, 80);
            this.btnPlan.TabIndex = 4;
            this.btnPlan.Text = "全箱中心规划";
            this.btnPlan.UseVisualStyleBackColor = false;
            this.btnPlan.Click += new System.EventHandler(this.btnPlan_Click);
            // 
            // panelBottom
            // 
            this.panelBottom.AutoSize = true;
            this.panelBottom.ColumnCount = 1;
            this.panelBottom.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.panelBottom.Controls.Add(this.lblDllStatus, 0, 0);
            this.panelBottom.Controls.Add(this.flowBottom, 0, 1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelBottom.Location = new System.Drawing.Point(0, 928);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(0);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Padding = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.panelBottom.RowCount = 2;
            this.panelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelBottom.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.panelBottom.Size = new System.Drawing.Size(1578, 113);
            this.panelBottom.TabIndex = 3;
            // 
            // lblDllStatus
            // 
            this.lblDllStatus.AutoSize = true;
            this.lblDllStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblDllStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(85)))), ((int)(((byte)(105)))));
            this.lblDllStatus.Location = new System.Drawing.Point(4, 15);
            this.lblDllStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDllStatus.MaximumSize = new System.Drawing.Size(1290, 0);
            this.lblDllStatus.Name = "lblDllStatus";
            this.lblDllStatus.Size = new System.Drawing.Size(1290, 31);
            this.lblDllStatus.TabIndex = 0;
            this.lblDllStatus.Text = "DLL 状态加载中…";
            // 
            // flowBottom
            // 
            this.flowBottom.AutoSize = true;
            this.flowBottom.Controls.Add(this.btnClose);
            this.flowBottom.Controls.Add(this.btnReload);
            this.flowBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowBottom.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowBottom.Location = new System.Drawing.Point(0, 46);
            this.flowBottom.Margin = new System.Windows.Forms.Padding(0);
            this.flowBottom.Name = "flowBottom";
            this.flowBottom.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.flowBottom.Size = new System.Drawing.Size(1578, 67);
            this.flowBottom.TabIndex = 1;
            this.flowBottom.WrapContents = false;
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = true;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(1462, 16);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(112, 46);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnReload
            // 
            this.btnReload.AutoSize = true;
            this.btnReload.Location = new System.Drawing.Point(1203, 12);
            this.btnReload.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(243, 46);
            this.btnReload.TabIndex = 1;
            this.btnReload.Text = "重新加载 DLL/INI";
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // AlgorithmTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(1620, 1080);
            this.Controls.Add(this.tableLayoutRoot);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1339, 872);
            this.Name = "AlgorithmTestForm";
            this.Padding = new System.Windows.Forms.Padding(21, 18, 21, 21);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "算法测试";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AlgorithmTestForm_FormClosed);
            this.Load += new System.EventHandler(this.AlgorithmTestForm_Load);
            this.Shown += new System.EventHandler(this.AlgorithmTestForm_Shown);
            this.tableLayoutRoot.ResumeLayout(false);
            this.tableLayoutRoot.PerformLayout();
            this.panelImagePath.ResumeLayout(false);
            this.panelImagePath.PerformLayout();
            this.tableLayoutPathRow.ResumeLayout(false);
            this.tableLayoutPathRow.PerformLayout();
            this.flowCapture.ResumeLayout(false);
            this.flowCapture.PerformLayout();
            this.previewSplit.Panel1.ResumeLayout(false);
            this.previewSplit.Panel2.ResumeLayout(false);
            this.previewSplit.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.previewSplit)).EndInit();
            this.previewSplit.ResumeLayout(false);
            this.panelPreview.ResumeLayout(false);
            this.panelPreview.PerformLayout();
            this.previewToolbarHost.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.tabTests.ResumeLayout(false);
            this.tabPresence.ResumeLayout(false);
            this.layoutPresence.ResumeLayout(false);
            this.layoutPresence.PerformLayout();
            this.flowPresence.ResumeLayout(false);
            this.flowPresence.PerformLayout();
            this.tabJinwo.ResumeLayout(false);
            this.layoutJinwo.ResumeLayout(false);
            this.layoutJinwo.PerformLayout();
            this.flowJinwo.ResumeLayout(false);
            this.flowJinwo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPlaced)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            this.flowBottom.ResumeLayout(false);
            this.flowBottom.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutRoot;
        private System.Windows.Forms.TableLayoutPanel panelImagePath;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPathRow;
        private System.Windows.Forms.Label lblImage;
        private System.Windows.Forms.TextBox txtImage;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button btnMainImage;
        private System.Windows.Forms.FlowLayoutPanel flowCapture;
        private System.Windows.Forms.Button btnHikCapture;
        private System.Windows.Forms.SplitContainer previewSplit;
        private System.Windows.Forms.Panel panelPreview;
        private System.Windows.Forms.FlowLayoutPanel previewToolbarHost;
        private System.Windows.Forms.Button btnSavePreview;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblRenderPath;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabControl tabTests;
        private System.Windows.Forms.TabPage tabPresence;
        private System.Windows.Forms.TableLayoutPanel layoutPresence;
        private System.Windows.Forms.Label lblPresenceHint;
        private System.Windows.Forms.FlowLayoutPanel flowPresence;
        private System.Windows.Forms.Button btnRunPresence;
        private System.Windows.Forms.TabPage tabJinwo;
        private System.Windows.Forms.TableLayoutPanel layoutJinwo;
        private System.Windows.Forms.Label lblJinwoHint;
        private System.Windows.Forms.FlowLayoutPanel flowJinwo;
        private System.Windows.Forms.Button btnMarkers;
        private System.Windows.Forms.Label lblPlaced;
        private System.Windows.Forms.NumericUpDown numPlaced;
        private System.Windows.Forms.Button btnPose;
        private System.Windows.Forms.Button btnPlan;
        private System.Windows.Forms.TableLayoutPanel panelBottom;
        private System.Windows.Forms.Label lblDllStatus;
        private System.Windows.Forms.FlowLayoutPanel flowBottom;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnReload;
    }
}
