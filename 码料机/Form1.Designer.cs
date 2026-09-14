namespace 码料机
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ReleaseHikCamera();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel13 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorZAxis = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelZAxis = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorPhotoPos = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelPhotoPos = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorJinwo = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelJinwo = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorNinePoint = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelNinePoint = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorAlgoTest = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelAlgoTest = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorManualPlace = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelManualPlace = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorStartPiece = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelStartPiece = new System.Windows.Forms.ToolStripLabel();
            this.statusStripBottom = new System.Windows.Forms.StatusStrip();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel14 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel17 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorCameraPhoto = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelPhoto = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel7 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel8 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel9 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel10 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorBuzzerMute = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelBuzzerMute = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparatorCountReset = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabelCountReset = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel12 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel18 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel11 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripStatusLabelSpring = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripLabel15 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel16 = new System.Windows.Forms.ToolStripLabel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.labelLeftLaneProductCap = new System.Windows.Forms.Label();
            this.labelLeftLaneProductVal = new System.Windows.Forms.Label();
            this.btnLeftLaneProductReset = new System.Windows.Forms.Button();
            this.labelLeftPlaceTotalCap = new System.Windows.Forms.Label();
            this.textBoxLeftPlaceTotal = new System.Windows.Forms.TextBox();
            this.btnLeftPlaceTotalSave = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.labelRightLaneProductCap = new System.Windows.Forms.Label();
            this.labelRightLaneProductVal = new System.Windows.Forms.Label();
            this.btnRightLaneProductReset = new System.Windows.Forms.Button();
            this.labelRightPlaceTotalCap = new System.Windows.Forms.Label();
            this.textBoxRightPlaceTotal = new System.Windows.Forms.TextBox();
            this.btnRightPlaceTotalSave = new System.Windows.Forms.Button();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.stationOpPanelRight = new 码料机.StationOperatorPanel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.stationOpPanelLeft = new 码料机.StationOperatorPanel();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panelVmPreviewHost = new System.Windows.Forms.Panel();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.BottomToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.TopToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.RightToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.LeftToolStripPanel = new System.Windows.Forms.ToolStripPanel();
            this.ContentPanel = new System.Windows.Forms.ToolStripContentPanel();
            this.toolStrip1.SuspendLayout();
            this.statusStripBottom.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.toolStrip1.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel1,
            this.toolStripSeparator2,
            this.toolStripLabel2,
            this.toolStripSeparator3,
            this.toolStripLabel13,
            this.toolStripSeparator1,
            this.toolStripLabel3,
            this.toolStripSeparatorZAxis,
            this.toolStripLabelZAxis,
            this.toolStripSeparatorPhotoPos,
            this.toolStripLabelPhotoPos,
            this.toolStripSeparatorJinwo,
            this.toolStripLabelJinwo,
            this.toolStripSeparatorNinePoint,
            this.toolStripLabelNinePoint,
            this.toolStripSeparatorAlgoTest,
            this.toolStripLabelAlgoTest,
            this.toolStripSeparatorManualPlace,
            this.toolStripLabelManualPlace,
            this.toolStripSeparatorStartPiece,
            this.toolStripLabelStartPiece});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.MinimumSize = new System.Drawing.Size(0, 52);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(10, 8, 10, 8);
            this.toolStrip1.Size = new System.Drawing.Size(1907, 71);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(116)))), ((int)(((byte)(139)))));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(111, 50);
            this.toolStripLabel1.Text = "[工具栏]";
            this.toolStripLabel1.Visible = false;
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparator2.Visible = false;
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabel2.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabel2.Size = new System.Drawing.Size(169, 47);
            this.toolStripLabel2.Text = "机械臂控制";
            this.toolStripLabel2.ToolTipText = "工位生产选择（仅向 PLC D4414 下发，不参与取放逻辑）";
            this.toolStripLabel2.Click += new System.EventHandler(this.toolStripLabel2_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparator3.Visible = false;
            // 
            // toolStripLabel13
            // 
            this.toolStripLabel13.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabel13.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabel13.Name = "toolStripLabel13";
            this.toolStripLabel13.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabel13.Size = new System.Drawing.Size(143, 47);
            this.toolStripLabel13.Text = "产品参数";
            this.toolStripLabel13.Click += new System.EventHandler(this.toolStripLabel13_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparator1.Visible = false;
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabel3.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabel3.Size = new System.Drawing.Size(195, 47);
            this.toolStripLabel3.Text = "箱体参数设置";
            this.toolStripLabel3.Click += new System.EventHandler(this.toolStripLabel3_Click);
            // 
            // toolStripSeparatorZAxis
            // 
            this.toolStripSeparatorZAxis.Name = "toolStripSeparatorZAxis";
            this.toolStripSeparatorZAxis.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparatorZAxis.Visible = false;
            // 
            // toolStripLabelZAxis
            // 
            this.toolStripLabelZAxis.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabelZAxis.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabelZAxis.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabelZAxis.Name = "toolStripLabelZAxis";
            this.toolStripLabelZAxis.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabelZAxis.Size = new System.Drawing.Size(186, 47);
            this.toolStripLabelZAxis.Text = "Z轴参数设定";
            this.toolStripLabelZAxis.Visible = false;
            this.toolStripLabelZAxis.Click += new System.EventHandler(this.toolStripLabelZAxis_Click);
            // 
            // toolStripSeparatorPhotoPos
            // 
            this.toolStripSeparatorPhotoPos.Name = "toolStripSeparatorPhotoPos";
            this.toolStripSeparatorPhotoPos.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparatorPhotoPos.Visible = false;
            // 
            // toolStripLabelPhotoPos
            // 
            this.toolStripLabelPhotoPos.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabelPhotoPos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabelPhotoPos.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabelPhotoPos.Name = "toolStripLabelPhotoPos";
            this.toolStripLabelPhotoPos.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabelPhotoPos.Size = new System.Drawing.Size(143, 47);
            this.toolStripLabelPhotoPos.Text = "位置设定";
            this.toolStripLabelPhotoPos.Click += new System.EventHandler(this.toolStripLabelPhotoPos_Click);
            // 
            // toolStripSeparatorJinwo
            // 
            this.toolStripSeparatorJinwo.Name = "toolStripSeparatorJinwo";
            this.toolStripSeparatorJinwo.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparatorJinwo.Visible = false;
            // 
            // toolStripLabelJinwo
            // 
            this.toolStripLabelJinwo.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabelJinwo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabelJinwo.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabelJinwo.Name = "toolStripLabelJinwo";
            this.toolStripLabelJinwo.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabelJinwo.Size = new System.Drawing.Size(195, 47);
            this.toolStripLabelJinwo.Text = "金沃算法设定";
            this.toolStripLabelJinwo.Click += new System.EventHandler(this.toolStripLabelJinwo_Click);
            // 
            // toolStripSeparatorNinePoint
            // 
            this.toolStripSeparatorNinePoint.Name = "toolStripSeparatorNinePoint";
            this.toolStripSeparatorNinePoint.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparatorNinePoint.Visible = false;
            // 
            // toolStripLabelNinePoint
            // 
            this.toolStripLabelNinePoint.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabelNinePoint.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabelNinePoint.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabelNinePoint.Name = "toolStripLabelNinePoint";
            this.toolStripLabelNinePoint.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabelNinePoint.Size = new System.Drawing.Size(195, 47);
            this.toolStripLabelNinePoint.Text = "九点标定工具";
            this.toolStripLabelNinePoint.Visible = false;
            this.toolStripLabelNinePoint.Click += new System.EventHandler(this.toolStripLabelNinePoint_Click);
            // 
            // toolStripSeparatorAlgoTest
            // 
            this.toolStripSeparatorAlgoTest.Name = "toolStripSeparatorAlgoTest";
            this.toolStripSeparatorAlgoTest.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparatorAlgoTest.Visible = false;
            // 
            // toolStripLabelAlgoTest
            // 
            this.toolStripLabelAlgoTest.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabelAlgoTest.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabelAlgoTest.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabelAlgoTest.Name = "toolStripLabelAlgoTest";
            this.toolStripLabelAlgoTest.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabelAlgoTest.Size = new System.Drawing.Size(143, 47);
            this.toolStripLabelAlgoTest.Text = "算法测试";
            this.toolStripLabelAlgoTest.Click += new System.EventHandler(this.toolStripLabelAlgoTest_Click);
            // 
            // toolStripSeparatorManualPlace
            // 
            this.toolStripSeparatorManualPlace.Name = "toolStripSeparatorManualPlace";
            this.toolStripSeparatorManualPlace.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparatorManualPlace.Visible = false;
            // 
            // toolStripLabelManualPlace
            // 
            this.toolStripLabelManualPlace.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabelManualPlace.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabelManualPlace.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabelManualPlace.Name = "toolStripLabelManualPlace";
            this.toolStripLabelManualPlace.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabelManualPlace.Size = new System.Drawing.Size(195, 47);
            this.toolStripLabelManualPlace.Text = "手动指定放料";
            this.toolStripLabelManualPlace.Click += new System.EventHandler(this.toolStripLabelManualPlace_Click);
            // 
            // toolStripSeparatorStartPiece
            // 
            this.toolStripSeparatorStartPiece.Name = "toolStripSeparatorStartPiece";
            this.toolStripSeparatorStartPiece.Size = new System.Drawing.Size(6, 55);
            this.toolStripSeparatorStartPiece.Visible = false;
            // 
            // toolStripLabelStartPiece
            // 
            this.toolStripLabelStartPiece.Font = new System.Drawing.Font("Microsoft YaHei UI", 13F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabelStartPiece.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(41)))), ((int)(((byte)(59)))));
            this.toolStripLabelStartPiece.Margin = new System.Windows.Forms.Padding(4);
            this.toolStripLabelStartPiece.Name = "toolStripLabelStartPiece";
            this.toolStripLabelStartPiece.Padding = new System.Windows.Forms.Padding(12, 8, 12, 8);
            this.toolStripLabelStartPiece.Size = new System.Drawing.Size(169, 47);
            this.toolStripLabelStartPiece.Text = "指定开始组";
            this.toolStripLabelStartPiece.ToolTipText = "左/右机台空箱离线规划并指定起始组；确认后自动补全进度，后续与自动模式相同直至满料";
            this.toolStripLabelStartPiece.Click += new System.EventHandler(this.toolStripLabelStartPiece_Click);
            // 
            // statusStripBottom
            // 
            this.statusStripBottom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(247)))), ((int)(((byte)(250)))), ((int)(((byte)(252)))));
            this.statusStripBottom.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.statusStripBottom.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStripBottom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripLabel4,
            this.toolStripSeparator4,
            this.toolStripLabel5,
            this.toolStripLabel6,
            this.toolStripSeparator9,
            this.toolStripLabel14,
            this.toolStripLabel17,
            this.toolStripSeparatorCameraPhoto,
            this.toolStripLabelPhoto,
            this.toolStripSeparator5,
            this.toolStripLabel7,
            this.toolStripLabel8,
            this.toolStripSeparator6,
            this.toolStripLabel9,
            this.toolStripLabel10,
            this.toolStripSeparatorBuzzerMute,
            this.toolStripLabelBuzzerMute,
            this.toolStripSeparatorCountReset,
            this.toolStripLabelCountReset,
            this.toolStripSeparator7,
            this.toolStripSeparator8,
            this.toolStripLabel12,
            this.toolStripLabel18,
            this.toolStripSeparator10,
            this.toolStripLabel11,
            this.toolStripStatusLabelSpring,
            this.toolStripLabel15,
            this.toolStripLabel16});
            this.statusStripBottom.Location = new System.Drawing.Point(0, 959);
            this.statusStripBottom.Name = "statusStripBottom";
            this.statusStripBottom.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.statusStripBottom.Size = new System.Drawing.Size(1907, 33);
            this.statusStripBottom.SizingGrip = false;
            this.statusStripBottom.TabIndex = 1;
            this.statusStripBottom.Text = "statusStripBottom";
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(74)))), ((int)(((byte)(85)))), ((int)(((byte)(104)))));
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(93, 30);
            this.toolStripLabel4.Text = "[状态栏]";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(57, 30);
            this.toolStripLabel5.Text = "PLC:";
            // 
            // toolStripLabel6
            // 
            this.toolStripLabel6.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(48)))), ((int)(((byte)(48)))));
            this.toolStripLabel6.Name = "toolStripLabel6";
            this.toolStripLabel6.Size = new System.Drawing.Size(79, 30);
            this.toolStripLabel6.Text = "未连接";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabel14
            // 
            this.toolStripLabel14.Name = "toolStripLabel14";
            this.toolStripLabel14.Size = new System.Drawing.Size(62, 30);
            this.toolStripLabel14.Text = "相机:";
            // 
            // toolStripLabel17
            // 
            this.toolStripLabel17.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.toolStripLabel17.ForeColor = System.Drawing.Color.Red;
            this.toolStripLabel17.Name = "toolStripLabel17";
            this.toolStripLabel17.Size = new System.Drawing.Size(79, 30);
            this.toolStripLabel17.Text = "未连接";
            // 
            // toolStripSeparatorCameraPhoto
            // 
            this.toolStripSeparatorCameraPhoto.Name = "toolStripSeparatorCameraPhoto";
            this.toolStripSeparatorCameraPhoto.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabelPhoto
            // 
            this.toolStripLabelPhoto.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Underline);
            this.toolStripLabelPhoto.ForeColor = System.Drawing.Color.DarkBlue;
            this.toolStripLabelPhoto.Name = "toolStripLabelPhoto";
            this.toolStripLabelPhoto.Size = new System.Drawing.Size(101, 30);
            this.toolStripLabelPhoto.Text = "金沃算图";
            this.toolStripLabelPhoto.ToolTipText = "临时测算法：弹窗选左右工位，采图/算图仅预览，不改自动放料与规划";
            this.toolStripLabelPhoto.Click += new System.EventHandler(this.toolStripLabelPhoto_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabel7
            // 
            this.toolStripLabel7.Name = "toolStripLabel7";
            this.toolStripLabel7.Size = new System.Drawing.Size(62, 30);
            this.toolStripLabel7.Text = "视觉:";
            // 
            // toolStripLabel8
            // 
            this.toolStripLabel8.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.toolStripLabel8.ForeColor = System.Drawing.Color.Red;
            this.toolStripLabel8.Name = "toolStripLabel8";
            this.toolStripLabel8.Size = new System.Drawing.Size(79, 30);
            this.toolStripLabel8.Text = "未加载";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabel9
            // 
            this.toolStripLabel9.Name = "toolStripLabel9";
            this.toolStripLabel9.Size = new System.Drawing.Size(64, 30);
            this.toolStripLabel9.Text = "AGV:";
            // 
            // toolStripLabel10
            // 
            this.toolStripLabel10.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Bold);
            this.toolStripLabel10.ForeColor = System.Drawing.Color.Red;
            this.toolStripLabel10.Name = "toolStripLabel10";
            this.toolStripLabel10.Size = new System.Drawing.Size(79, 30);
            this.toolStripLabel10.Text = "未连接";
            // 
            // toolStripSeparatorBuzzerMute
            // 
            this.toolStripSeparatorBuzzerMute.Name = "toolStripSeparatorBuzzerMute";
            this.toolStripSeparatorBuzzerMute.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabelBuzzerMute
            // 
            this.toolStripLabelBuzzerMute.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Underline);
            this.toolStripLabelBuzzerMute.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(175)))));
            this.toolStripLabelBuzzerMute.Name = "toolStripLabelBuzzerMute";
            this.toolStripLabelBuzzerMute.Size = new System.Drawing.Size(101, 30);
            this.toolStripLabelBuzzerMute.Text = "蜂鸣消音";
            this.toolStripLabelBuzzerMute.ToolTipText = "切换 PC_位功能地址 D4002（INT，读取当前值后 0/1 取反写入并保持，常用于蜂鸣消音）";
            this.toolStripLabelBuzzerMute.Click += new System.EventHandler(this.toolStripLabelBuzzerMute_Click);
            // 
            // toolStripSeparatorCountReset
            // 
            this.toolStripSeparatorCountReset.Name = "toolStripSeparatorCountReset";
            this.toolStripSeparatorCountReset.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabelCountReset
            // 
            this.toolStripLabelCountReset.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Underline);
            this.toolStripLabelCountReset.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(64)))), ((int)(((byte)(175)))));
            this.toolStripLabelCountReset.Name = "toolStripLabelCountReset";
            this.toolStripLabelCountReset.Size = new System.Drawing.Size(101, 30);
            this.toolStripLabelCountReset.Text = "计数清零";
            this.toolStripLabelCountReset.ToolTipText = "向 PC_计数清零 D4003.6 写入 1（整体计数清零）";
            this.toolStripLabelCountReset.Click += new System.EventHandler(this.toolStripLabelCountReset_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabel12
            // 
            this.toolStripLabel12.Name = "toolStripLabel12";
            this.toolStripLabel12.Size = new System.Drawing.Size(106, 30);
            this.toolStripLabel12.Text = "机台状态:";
            // 
            // toolStripLabel18
            // 
            this.toolStripLabel18.Name = "toolStripLabel18";
            this.toolStripLabel18.Size = new System.Drawing.Size(0, 30);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 33);
            // 
            // toolStripLabel11
            // 
            this.toolStripLabel11.Name = "toolStripLabel11";
            this.toolStripLabel11.Size = new System.Drawing.Size(113, 30);
            this.toolStripLabel11.Text = "运行: 空闲";
            this.toolStripLabel11.ToolTipText = "故障停机时：单击此处并按提示清除故障（须先排除现场隐患）。";
            this.toolStripLabel11.Click += new System.EventHandler(this.toolStripLabel11_Click);
            // 
            // toolStripStatusLabelSpring
            // 
            this.toolStripStatusLabelSpring.Name = "toolStripStatusLabelSpring";
            this.toolStripStatusLabelSpring.Size = new System.Drawing.Size(402, 26);
            this.toolStripStatusLabelSpring.Spring = true;
            // 
            // toolStripLabel15
            // 
            this.toolStripLabel15.Name = "toolStripLabel15";
            this.toolStripLabel15.Size = new System.Drawing.Size(79, 30);
            this.toolStripLabel15.Text = "时间：";
            // 
            // toolStripLabel16
            // 
            this.toolStripLabel16.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.toolStripLabel16.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.toolStripLabel16.Name = "toolStripLabel16";
            this.toolStripLabel16.Size = new System.Drawing.Size(188, 30);
            this.toolStripLabel16.Text = "----/--/-- --:--:--";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 17F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 38F));
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer2, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 71);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1907, 888);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox2);
            this.splitContainer1.Size = new System.Drawing.Size(318, 882);
            this.splitContainer1.SplitterDistance = 429;
            this.splitContainer1.SplitterWidth = 6;
            this.splitContainer1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.BackColor = System.Drawing.Color.White;
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.labelLeftLaneProductCap);
            this.groupBox1.Controls.Add(this.labelLeftLaneProductVal);
            this.groupBox1.Controls.Add(this.btnLeftLaneProductReset);
            this.groupBox1.Controls.Add(this.labelLeftPlaceTotalCap);
            this.groupBox1.Controls.Add(this.textBoxLeftPlaceTotal);
            this.groupBox1.Controls.Add(this.btnLeftPlaceTotalSave);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("楷体", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(318, 429);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "工位一";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.BackColor = System.Drawing.Color.White;
            this.label16.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label16.ForeColor = System.Drawing.Color.Blue;
            this.label16.Location = new System.Drawing.Point(13, 230);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(62, 31);
            this.label16.TabIndex = 11;
            this.label16.Text = "行数";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label15.Location = new System.Drawing.Point(13, 270);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(62, 31);
            this.label15.TabIndex = 10;
            this.label15.Text = "列数";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label8.Location = new System.Drawing.Point(102, 198);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(0, 31);
            this.label8.TabIndex = 9;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label7.Location = new System.Drawing.Point(102, 230);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(0, 31);
            this.label7.TabIndex = 8;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label6.Location = new System.Drawing.Point(102, 270);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(0, 31);
            this.label6.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.BackColor = System.Drawing.Color.White;
            this.label5.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label5.ForeColor = System.Drawing.Color.Blue;
            this.label5.Location = new System.Drawing.Point(13, 198);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(62, 31);
            this.label5.TabIndex = 6;
            this.label5.Text = "层数";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.BackColor = System.Drawing.Color.White;
            this.label2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.label2.ForeColor = System.Drawing.Color.Blue;
            this.label2.Location = new System.Drawing.Point(9, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(164, 32);
            this.label2.TabIndex = 5;
            this.label2.Text = "当前摆放情况";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.White;
            this.label3.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label3.ForeColor = System.Drawing.Color.Blue;
            this.label3.Location = new System.Drawing.Point(157, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 31);
            this.label3.TabIndex = 4;
            this.label3.Text = "未选择";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.BackColor = System.Drawing.Color.White;
            this.label12.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label12.ForeColor = System.Drawing.Color.Blue;
            this.label12.Location = new System.Drawing.Point(157, 76);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(86, 31);
            this.label12.TabIndex = 3;
            this.label12.Text = "未选择";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.Color.White;
            this.label10.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.label10.ForeColor = System.Drawing.Color.Blue;
            this.label10.Location = new System.Drawing.Point(9, 75);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(120, 32);
            this.label10.TabIndex = 1;
            this.label10.Text = "矩阵摆放:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.label9.ForeColor = System.Drawing.Color.Blue;
            this.label9.Location = new System.Drawing.Point(9, 36);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(120, 32);
            this.label9.TabIndex = 0;
            this.label9.Text = "木框状态:";
            // 
            // labelLeftLaneProductCap
            // 
            this.labelLeftLaneProductCap.AutoSize = true;
            this.labelLeftLaneProductCap.BackColor = System.Drawing.Color.White;
            this.labelLeftLaneProductCap.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.labelLeftLaneProductCap.ForeColor = System.Drawing.Color.Blue;
            this.labelLeftLaneProductCap.Location = new System.Drawing.Point(9, 114);
            this.labelLeftLaneProductCap.Name = "labelLeftLaneProductCap";
            this.labelLeftLaneProductCap.Size = new System.Drawing.Size(120, 32);
            this.labelLeftLaneProductCap.TabIndex = 35;
            this.labelLeftLaneProductCap.Text = "料道产品:";
            // 
            // labelLeftLaneProductVal
            // 
            this.labelLeftLaneProductVal.AutoSize = true;
            this.labelLeftLaneProductVal.BackColor = System.Drawing.Color.White;
            this.labelLeftLaneProductVal.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.labelLeftLaneProductVal.ForeColor = System.Drawing.Color.Blue;
            this.labelLeftLaneProductVal.Location = new System.Drawing.Point(201, 115);
            this.labelLeftLaneProductVal.Name = "labelLeftLaneProductVal";
            this.labelLeftLaneProductVal.Size = new System.Drawing.Size(40, 31);
            this.labelLeftLaneProductVal.TabIndex = 36;
            this.labelLeftLaneProductVal.Text = "—";
            // 
            // btnLeftLaneProductReset
            // 
            this.btnLeftLaneProductReset.AutoSize = true;
            this.btnLeftLaneProductReset.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnLeftLaneProductReset.Location = new System.Drawing.Point(240, 112);
            this.btnLeftLaneProductReset.MinimumSize = new System.Drawing.Size(64, 28);
            this.btnLeftLaneProductReset.Name = "btnLeftLaneProductReset";
            this.btnLeftLaneProductReset.Size = new System.Drawing.Size(67, 40);
            this.btnLeftLaneProductReset.TabIndex = 37;
            this.btnLeftLaneProductReset.Text = "重置";
            this.btnLeftLaneProductReset.UseVisualStyleBackColor = true;
            // 
            // labelLeftPlaceTotalCap
            // 
            this.labelLeftPlaceTotalCap.AutoSize = true;
            this.labelLeftPlaceTotalCap.BackColor = System.Drawing.Color.White;
            this.labelLeftPlaceTotalCap.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.labelLeftPlaceTotalCap.ForeColor = System.Drawing.Color.Blue;
            this.labelLeftPlaceTotalCap.Location = new System.Drawing.Point(13, 156);
            this.labelLeftPlaceTotalCap.Name = "labelLeftPlaceTotalCap";
            this.labelLeftPlaceTotalCap.Size = new System.Drawing.Size(62, 31);
            this.labelLeftPlaceTotalCap.TabIndex = 38;
            this.labelLeftPlaceTotalCap.Text = "总数";
            // 
            // textBoxLeftPlaceTotal
            // 
            this.textBoxLeftPlaceTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxLeftPlaceTotal.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textBoxLeftPlaceTotal.Location = new System.Drawing.Point(90, 154);
            this.textBoxLeftPlaceTotal.MinimumSize = new System.Drawing.Size(48, 28);
            this.textBoxLeftPlaceTotal.Name = "textBoxLeftPlaceTotal";
            this.textBoxLeftPlaceTotal.Size = new System.Drawing.Size(80, 38);
            this.textBoxLeftPlaceTotal.TabIndex = 39;
            this.textBoxLeftPlaceTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnLeftPlaceTotalSave
            // 
            this.btnLeftPlaceTotalSave.AutoSize = true;
            this.btnLeftPlaceTotalSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnLeftPlaceTotalSave.Location = new System.Drawing.Point(176, 152);
            this.btnLeftPlaceTotalSave.MinimumSize = new System.Drawing.Size(64, 28);
            this.btnLeftPlaceTotalSave.Name = "btnLeftPlaceTotalSave";
            this.btnLeftPlaceTotalSave.Size = new System.Drawing.Size(67, 40);
            this.btnLeftPlaceTotalSave.TabIndex = 40;
            this.btnLeftPlaceTotalSave.Text = "保存";
            this.btnLeftPlaceTotalSave.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.White;
            this.groupBox2.Controls.Add(this.label17);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label19);
            this.groupBox2.Controls.Add(this.label20);
            this.groupBox2.Controls.Add(this.label21);
            this.groupBox2.Controls.Add(this.label22);
            this.groupBox2.Controls.Add(this.label23);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Controls.Add(this.label14);
            this.groupBox2.Controls.Add(this.labelRightLaneProductCap);
            this.groupBox2.Controls.Add(this.labelRightLaneProductVal);
            this.groupBox2.Controls.Add(this.btnRightLaneProductReset);
            this.groupBox2.Controls.Add(this.labelRightPlaceTotalCap);
            this.groupBox2.Controls.Add(this.textBoxRightPlaceTotal);
            this.groupBox2.Controls.Add(this.btnRightPlaceTotalSave);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(318, 447);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "工位二";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label17.Location = new System.Drawing.Point(13, 228);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(62, 31);
            this.label17.TabIndex = 18;
            this.label17.Text = "行数";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label18.Location = new System.Drawing.Point(13, 268);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(62, 31);
            this.label18.TabIndex = 17;
            this.label18.Text = "列数";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label19.Location = new System.Drawing.Point(102, 196);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(0, 31);
            this.label19.TabIndex = 16;
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label20.Location = new System.Drawing.Point(102, 228);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(0, 31);
            this.label20.TabIndex = 15;
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label21.Location = new System.Drawing.Point(102, 268);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(0, 31);
            this.label21.TabIndex = 14;
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label22.Location = new System.Drawing.Point(13, 196);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(62, 31);
            this.label22.TabIndex = 13;
            this.label22.Text = "层数";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.label23.Location = new System.Drawing.Point(13, 120);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(164, 32);
            this.label23.TabIndex = 12;
            this.label23.Text = "当前摆放情况";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label4.Location = new System.Drawing.Point(157, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 31);
            this.label4.TabIndex = 7;
            this.label4.Text = "未选择";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.label11.Location = new System.Drawing.Point(157, 78);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(86, 31);
            this.label11.TabIndex = 6;
            this.label11.Text = "未选择";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.label13.Location = new System.Drawing.Point(9, 77);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(120, 32);
            this.label13.TabIndex = 5;
            this.label13.Text = "矩阵摆放:";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.label14.Location = new System.Drawing.Point(9, 38);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(120, 32);
            this.label14.TabIndex = 4;
            this.label14.Text = "木框状态:";
            // 
            // labelRightLaneProductCap
            // 
            this.labelRightLaneProductCap.AutoSize = true;
            this.labelRightLaneProductCap.BackColor = System.Drawing.Color.White;
            this.labelRightLaneProductCap.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F);
            this.labelRightLaneProductCap.ForeColor = System.Drawing.Color.Blue;
            this.labelRightLaneProductCap.Location = new System.Drawing.Point(9, 116);
            this.labelRightLaneProductCap.Name = "labelRightLaneProductCap";
            this.labelRightLaneProductCap.Size = new System.Drawing.Size(120, 32);
            this.labelRightLaneProductCap.TabIndex = 35;
            this.labelRightLaneProductCap.Text = "料道产品:";
            // 
            // labelRightLaneProductVal
            // 
            this.labelRightLaneProductVal.AutoSize = true;
            this.labelRightLaneProductVal.BackColor = System.Drawing.Color.White;
            this.labelRightLaneProductVal.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.labelRightLaneProductVal.ForeColor = System.Drawing.Color.Blue;
            this.labelRightLaneProductVal.Location = new System.Drawing.Point(201, 117);
            this.labelRightLaneProductVal.Name = "labelRightLaneProductVal";
            this.labelRightLaneProductVal.Size = new System.Drawing.Size(40, 31);
            this.labelRightLaneProductVal.TabIndex = 36;
            this.labelRightLaneProductVal.Text = "—";
            // 
            // btnRightLaneProductReset
            // 
            this.btnRightLaneProductReset.AutoSize = true;
            this.btnRightLaneProductReset.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnRightLaneProductReset.Location = new System.Drawing.Point(240, 114);
            this.btnRightLaneProductReset.MinimumSize = new System.Drawing.Size(64, 28);
            this.btnRightLaneProductReset.Name = "btnRightLaneProductReset";
            this.btnRightLaneProductReset.Size = new System.Drawing.Size(67, 40);
            this.btnRightLaneProductReset.TabIndex = 37;
            this.btnRightLaneProductReset.Text = "重置";
            this.btnRightLaneProductReset.UseVisualStyleBackColor = true;
            // 
            // labelRightPlaceTotalCap
            // 
            this.labelRightPlaceTotalCap.AutoSize = true;
            this.labelRightPlaceTotalCap.BackColor = System.Drawing.Color.White;
            this.labelRightPlaceTotalCap.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.labelRightPlaceTotalCap.ForeColor = System.Drawing.Color.Blue;
            this.labelRightPlaceTotalCap.Location = new System.Drawing.Point(13, 154);
            this.labelRightPlaceTotalCap.Name = "labelRightPlaceTotalCap";
            this.labelRightPlaceTotalCap.Size = new System.Drawing.Size(62, 31);
            this.labelRightPlaceTotalCap.TabIndex = 38;
            this.labelRightPlaceTotalCap.Text = "总数";
            // 
            // textBoxRightPlaceTotal
            // 
            this.textBoxRightPlaceTotal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxRightPlaceTotal.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textBoxRightPlaceTotal.Location = new System.Drawing.Point(90, 152);
            this.textBoxRightPlaceTotal.MinimumSize = new System.Drawing.Size(48, 28);
            this.textBoxRightPlaceTotal.Name = "textBoxRightPlaceTotal";
            this.textBoxRightPlaceTotal.Size = new System.Drawing.Size(56, 38);
            this.textBoxRightPlaceTotal.TabIndex = 39;
            this.textBoxRightPlaceTotal.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // btnRightPlaceTotalSave
            // 
            this.btnRightPlaceTotalSave.AutoSize = true;
            this.btnRightPlaceTotalSave.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.btnRightPlaceTotalSave.Location = new System.Drawing.Point(163, 150);
            this.btnRightPlaceTotalSave.MinimumSize = new System.Drawing.Size(64, 28);
            this.btnRightPlaceTotalSave.Name = "btnRightPlaceTotalSave";
            this.btnRightPlaceTotalSave.Size = new System.Drawing.Size(67, 40);
            this.btnRightPlaceTotalSave.TabIndex = 40;
            this.btnRightPlaceTotalSave.Text = "保存";
            this.btnRightPlaceTotalSave.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this.groupBox4, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.groupBox3, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(1185, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(719, 882);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // groupBox4
            // 
            this.groupBox4.BackColor = System.Drawing.Color.White;
            this.groupBox4.Controls.Add(this.stationOpPanelRight);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold);
            this.groupBox4.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.groupBox4.Location = new System.Drawing.Point(3, 444);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(8, 10, 8, 8);
            this.groupBox4.Size = new System.Drawing.Size(713, 435);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "右机台";
            // 
            // stationOpPanelRight
            // 
            this.stationOpPanelRight.BackColor = System.Drawing.Color.White;
            this.stationOpPanelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stationOpPanelRight.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.stationOpPanelRight.Location = new System.Drawing.Point(8, 42);
            this.stationOpPanelRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.stationOpPanelRight.Name = "stationOpPanelRight";
            this.stationOpPanelRight.Size = new System.Drawing.Size(697, 385);
            this.stationOpPanelRight.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.BackColor = System.Drawing.Color.White;
            this.groupBox3.Controls.Add(this.stationOpPanelLeft);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft YaHei UI", 12.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(8, 10, 8, 8);
            this.groupBox3.Size = new System.Drawing.Size(713, 435);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "左机台";
            // 
            // stationOpPanelLeft
            // 
            this.stationOpPanelLeft.BackColor = System.Drawing.Color.White;
            this.stationOpPanelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stationOpPanelLeft.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.stationOpPanelLeft.Location = new System.Drawing.Point(8, 42);
            this.stationOpPanelLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.stationOpPanelLeft.Name = "stationOpPanelLeft";
            this.stationOpPanelLeft.Size = new System.Drawing.Size(697, 385);
            this.stationOpPanelLeft.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(327, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.listBox1);
            this.splitContainer2.Size = new System.Drawing.Size(852, 882);
            this.splitContainer2.SplitterDistance = 619;
            this.splitContainer2.SplitterWidth = 6;
            this.splitContainer2.TabIndex = 2;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.groupBox6);
            this.splitContainer3.Panel2Collapsed = true;
            this.splitContainer3.Size = new System.Drawing.Size(852, 619);
            this.splitContainer3.SplitterDistance = 300;
            this.splitContainer3.SplitterWidth = 6;
            this.splitContainer3.TabIndex = 0;
            // 
            // groupBox6
            // 
            this.groupBox6.BackColor = System.Drawing.Color.White;
            this.groupBox6.Controls.Add(this.tableLayoutPanel3);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(55)))), ((int)(((byte)(72)))));
            this.groupBox6.Location = new System.Drawing.Point(0, 0);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(852, 619);
            this.groupBox6.TabIndex = 0;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "拍照预览";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.panelVmPreviewHost, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 34);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(846, 582);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // panelVmPreviewHost
            // 
            this.panelVmPreviewHost.BackColor = System.Drawing.Color.Black;
            this.panelVmPreviewHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelVmPreviewHost.Location = new System.Drawing.Point(3, 3);
            this.panelVmPreviewHost.Name = "panelVmPreviewHost";
            this.panelVmPreviewHost.Size = new System.Drawing.Size(840, 576);
            this.panelVmPreviewHost.TabIndex = 2;
            // 
            // listBox1
            // 
            this.listBox1.BackColor = System.Drawing.Color.White;
            this.listBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 31;
            this.listBox1.Location = new System.Drawing.Point(0, 0);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(852, 257);
            this.listBox1.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // BottomToolStripPanel
            // 
            this.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.BottomToolStripPanel.Name = "BottomToolStripPanel";
            this.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.BottomToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // TopToolStripPanel
            // 
            this.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.TopToolStripPanel.Name = "TopToolStripPanel";
            this.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.TopToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // RightToolStripPanel
            // 
            this.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.RightToolStripPanel.Name = "RightToolStripPanel";
            this.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.RightToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // LeftToolStripPanel
            // 
            this.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftToolStripPanel.Name = "LeftToolStripPanel";
            this.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.LeftToolStripPanel.Size = new System.Drawing.Size(0, 0);
            // 
            // ContentPanel
            // 
            this.ContentPanel.Size = new System.Drawing.Size(150, 150);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
            this.ClientSize = new System.Drawing.Size(1907, 992);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.statusStripBottom);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "Form1";
            this.Text = "斯美科智能码料机";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStripBottom.ResumeLayout(false);
            this.statusStripBottom.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.StatusStrip statusStripBottom;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabelSpring;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripLabel toolStripLabel6;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripLabel toolStripLabel7;
        private System.Windows.Forms.ToolStripLabel toolStripLabel8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripLabel toolStripLabel9;
        private System.Windows.Forms.ToolStripLabel toolStripLabel10;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorBuzzerMute;
        private System.Windows.Forms.ToolStripLabel toolStripLabelBuzzerMute;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorCountReset;
        private System.Windows.Forms.ToolStripLabel toolStripLabelCountReset;
        private System.Windows.Forms.ToolStripLabel toolStripLabel11;
        private System.Windows.Forms.ToolStripLabel toolStripLabel15;
        private System.Windows.Forms.ToolStripLabel toolStripLabel16;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.GroupBox groupBox4;
        private StationOperatorPanel stationOpPanelRight;
        private System.Windows.Forms.GroupBox groupBox3;
        private StationOperatorPanel stationOpPanelLeft;
        // 运行时由 MountStationOperatorPanel 指向 StationOperatorPanel 内控件
        private System.Windows.Forms.TextBox textBoxRightPlaceQty;
        private System.Windows.Forms.Label labelRightPlaceQty;
        private System.Windows.Forms.TextBox textBoxRightPickQty;
        private System.Windows.Forms.Label labelRightPickQty;
        private System.Windows.Forms.TextBox textBoxLeftPlaceQty;
        private System.Windows.Forms.Label labelLeftPlaceQty;
        private System.Windows.Forms.TextBox textBoxLeftPickQty;
        private System.Windows.Forms.Label labelLeftPickQty;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ToolStripLabel toolStripLabel13;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label labelLeftLaneProductCap;
        private System.Windows.Forms.Label labelLeftLaneProductVal;
        private System.Windows.Forms.Button btnLeftLaneProductReset;
        private System.Windows.Forms.Label labelLeftPlaceTotalCap;
        private System.Windows.Forms.TextBox textBoxLeftPlaceTotal;
        private System.Windows.Forms.Button btnLeftPlaceTotalSave;
        private System.Windows.Forms.Label labelRightLaneProductCap;
        private System.Windows.Forms.Label labelRightLaneProductVal;
        private System.Windows.Forms.Button btnRightLaneProductReset;
        private System.Windows.Forms.Label labelRightPlaceTotalCap;
        private System.Windows.Forms.TextBox textBoxRightPlaceTotal;
        private System.Windows.Forms.Button btnRightPlaceTotalSave;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorZAxis;
        private System.Windows.Forms.ToolStripLabel toolStripLabelZAxis;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorPhotoPos;
        private System.Windows.Forms.ToolStripLabel toolStripLabelPhotoPos;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorJinwo;
        private System.Windows.Forms.ToolStripLabel toolStripLabelJinwo;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorNinePoint;
        private System.Windows.Forms.ToolStripLabel toolStripLabelNinePoint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorAlgoTest;
        private System.Windows.Forms.ToolStripLabel toolStripLabelAlgoTest;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorManualPlace;
        private System.Windows.Forms.ToolStripLabel toolStripLabelManualPlace;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorStartPiece;
        private System.Windows.Forms.ToolStripLabel toolStripLabelStartPiece;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripLabel toolStripLabel14;
        private System.Windows.Forms.ToolStripLabel toolStripLabel17;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparatorCameraPhoto;
        private System.Windows.Forms.ToolStripLabel toolStripLabelPhoto;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripLabel toolStripLabel12;
        private System.Windows.Forms.ToolStripLabel toolStripLabel18;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ComboBox comboBox4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox5;
        private System.Windows.Forms.ComboBox comboBox6;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Panel panelVmPreviewHost;
        private System.Windows.Forms.ToolStripPanel BottomToolStripPanel;
        private System.Windows.Forms.ToolStripPanel TopToolStripPanel;
        private System.Windows.Forms.ToolStripPanel RightToolStripPanel;
        private System.Windows.Forms.ToolStripPanel LeftToolStripPanel;
        private System.Windows.Forms.ToolStripContentPanel ContentPanel;
    }
}

