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

        /// <summary>左右工位独立配置；整页可滚动 TableLayout，避免 Dock 挤压。</summary>
        private void InitializeComponent()
        {
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.labelHint = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageLeft = new System.Windows.Forms.TabPage();
            this.panelLeftScroll = new System.Windows.Forms.Panel();
            this.tableLeftRoot = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftPick = new System.Windows.Forms.GroupBox();
            this.tableLeftPick = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftPickX = new System.Windows.Forms.Label();
            this.textLeftPickX = new System.Windows.Forms.TextBox();
            this.labelLeftPickY = new System.Windows.Forms.Label();
            this.textLeftPickY = new System.Windows.Forms.TextBox();
            this.labelLeftPickZ = new System.Windows.Forms.Label();
            this.textLeftPickZ = new System.Windows.Forms.TextBox();
            this.labelLeftPickRz = new System.Windows.Forms.Label();
            this.textLeftPickRz = new System.Windows.Forms.TextBox();
            this.groupLeftPlace = new System.Windows.Forms.GroupBox();
            this.tableLeftPlace = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftPlaceX = new System.Windows.Forms.Label();
            this.textLeftPlaceX = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceY = new System.Windows.Forms.Label();
            this.textLeftPlaceY = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceZ = new System.Windows.Forms.Label();
            this.textLeftPlaceZ = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceRz = new System.Windows.Forms.Label();
            this.textLeftPlaceRz = new System.Windows.Forms.TextBox();
            this.groupLeftPlacePhoto = new System.Windows.Forms.GroupBox();
            this.tableLeftPlacePhoto = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftPlacePhotoX = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoX = new System.Windows.Forms.TextBox();
            this.labelLeftPlacePhotoY = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoY = new System.Windows.Forms.TextBox();
            this.labelLeftPlacePhotoZ = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoZ = new System.Windows.Forms.TextBox();
            this.labelLeftPlacePhotoRz = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoRz = new System.Windows.Forms.TextBox();
            this.groupLeftPlaceCenter = new System.Windows.Forms.GroupBox();
            this.tableLeftPlaceCenter = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftPlaceCenterRz = new System.Windows.Forms.Label();
            this.textLeftPlaceCenterRz = new System.Windows.Forms.TextBox();
            this.groupLeftLimit = new System.Windows.Forms.GroupBox();
            this.tableLeftLimitRoot = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftLimitHint = new System.Windows.Forms.Label();
            this.checkLeftLimitEnabled = new System.Windows.Forms.CheckBox();
            this.tableLeftLimitRanges = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftLimitPick = new System.Windows.Forms.GroupBox();
            this.tableLeftLimitPick = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftLimitPickXMin = new System.Windows.Forms.Label();
            this.textLeftLimitPickXMin = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPickXMax = new System.Windows.Forms.Label();
            this.textLeftLimitPickXMax = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPickYMin = new System.Windows.Forms.Label();
            this.textLeftLimitPickYMin = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPickYMax = new System.Windows.Forms.Label();
            this.textLeftLimitPickYMax = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPickZMin = new System.Windows.Forms.Label();
            this.textLeftLimitPickZMin = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPickZMax = new System.Windows.Forms.Label();
            this.textLeftLimitPickZMax = new System.Windows.Forms.TextBox();
            this.groupLeftLimitPlace = new System.Windows.Forms.GroupBox();
            this.tableLeftLimitPlace = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftLimitPlaceXMin = new System.Windows.Forms.Label();
            this.textLeftLimitPlaceXMin = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPlaceXMax = new System.Windows.Forms.Label();
            this.textLeftLimitPlaceXMax = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPlaceYMin = new System.Windows.Forms.Label();
            this.textLeftLimitPlaceYMin = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPlaceYMax = new System.Windows.Forms.Label();
            this.textLeftLimitPlaceYMax = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPlaceZMin = new System.Windows.Forms.Label();
            this.textLeftLimitPlaceZMin = new System.Windows.Forms.TextBox();
            this.labelLeftLimitPlaceZMax = new System.Windows.Forms.Label();
            this.textLeftLimitPlaceZMax = new System.Windows.Forms.TextBox();
            this.tabPageRight = new System.Windows.Forms.TabPage();
            this.panelRightScroll = new System.Windows.Forms.Panel();
            this.tableRightRoot = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightPick = new System.Windows.Forms.GroupBox();
            this.tableRightPick = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightPickX = new System.Windows.Forms.Label();
            this.textRightPickX = new System.Windows.Forms.TextBox();
            this.labelRightPickY = new System.Windows.Forms.Label();
            this.textRightPickY = new System.Windows.Forms.TextBox();
            this.labelRightPickZ = new System.Windows.Forms.Label();
            this.textRightPickZ = new System.Windows.Forms.TextBox();
            this.labelRightPickRz = new System.Windows.Forms.Label();
            this.textRightPickRz = new System.Windows.Forms.TextBox();
            this.groupRightPlace = new System.Windows.Forms.GroupBox();
            this.tableRightPlace = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightPlaceX = new System.Windows.Forms.Label();
            this.textRightPlaceX = new System.Windows.Forms.TextBox();
            this.labelRightPlaceY = new System.Windows.Forms.Label();
            this.textRightPlaceY = new System.Windows.Forms.TextBox();
            this.labelRightPlaceZ = new System.Windows.Forms.Label();
            this.textRightPlaceZ = new System.Windows.Forms.TextBox();
            this.labelRightPlaceRz = new System.Windows.Forms.Label();
            this.textRightPlaceRz = new System.Windows.Forms.TextBox();
            this.groupRightPlacePhoto = new System.Windows.Forms.GroupBox();
            this.tableRightPlacePhoto = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightPlacePhotoX = new System.Windows.Forms.Label();
            this.textRightPlacePhotoX = new System.Windows.Forms.TextBox();
            this.labelRightPlacePhotoY = new System.Windows.Forms.Label();
            this.textRightPlacePhotoY = new System.Windows.Forms.TextBox();
            this.labelRightPlacePhotoZ = new System.Windows.Forms.Label();
            this.textRightPlacePhotoZ = new System.Windows.Forms.TextBox();
            this.labelRightPlacePhotoRz = new System.Windows.Forms.Label();
            this.textRightPlacePhotoRz = new System.Windows.Forms.TextBox();
            this.groupRightPlaceCenter = new System.Windows.Forms.GroupBox();
            this.tableRightPlaceCenter = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightPlaceCenterRz = new System.Windows.Forms.Label();
            this.textRightPlaceCenterRz = new System.Windows.Forms.TextBox();
            this.groupRightLimit = new System.Windows.Forms.GroupBox();
            this.tableRightLimitRoot = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightLimitHint = new System.Windows.Forms.Label();
            this.checkRightLimitEnabled = new System.Windows.Forms.CheckBox();
            this.tableRightLimitRanges = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightLimitPick = new System.Windows.Forms.GroupBox();
            this.tableRightLimitPick = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightLimitPickXMin = new System.Windows.Forms.Label();
            this.textRightLimitPickXMin = new System.Windows.Forms.TextBox();
            this.labelRightLimitPickXMax = new System.Windows.Forms.Label();
            this.textRightLimitPickXMax = new System.Windows.Forms.TextBox();
            this.labelRightLimitPickYMin = new System.Windows.Forms.Label();
            this.textRightLimitPickYMin = new System.Windows.Forms.TextBox();
            this.labelRightLimitPickYMax = new System.Windows.Forms.Label();
            this.textRightLimitPickYMax = new System.Windows.Forms.TextBox();
            this.labelRightLimitPickZMin = new System.Windows.Forms.Label();
            this.textRightLimitPickZMin = new System.Windows.Forms.TextBox();
            this.labelRightLimitPickZMax = new System.Windows.Forms.Label();
            this.textRightLimitPickZMax = new System.Windows.Forms.TextBox();
            this.groupRightLimitPlace = new System.Windows.Forms.GroupBox();
            this.tableRightLimitPlace = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightLimitPlaceXMin = new System.Windows.Forms.Label();
            this.textRightLimitPlaceXMin = new System.Windows.Forms.TextBox();
            this.labelRightLimitPlaceXMax = new System.Windows.Forms.Label();
            this.textRightLimitPlaceXMax = new System.Windows.Forms.TextBox();
            this.labelRightLimitPlaceYMin = new System.Windows.Forms.Label();
            this.textRightLimitPlaceYMin = new System.Windows.Forms.TextBox();
            this.labelRightLimitPlaceYMax = new System.Windows.Forms.Label();
            this.textRightLimitPlaceYMax = new System.Windows.Forms.TextBox();
            this.labelRightLimitPlaceZMin = new System.Windows.Forms.Label();
            this.textRightLimitPlaceZMin = new System.Windows.Forms.TextBox();
            this.labelRightLimitPlaceZMax = new System.Windows.Forms.Label();
            this.textRightLimitPlaceZMax = new System.Windows.Forms.TextBox();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.flowButtonsRight = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonSafetyZone = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonFromReco = new System.Windows.Forms.Button();
            this.buttonFromRecoTab = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageLeft.SuspendLayout();
            this.panelLeftScroll.SuspendLayout();
            this.tableLeftRoot.SuspendLayout();
            this.groupLeftPick.SuspendLayout();
            this.tableLeftPick.SuspendLayout();
            this.groupLeftPlace.SuspendLayout();
            this.tableLeftPlace.SuspendLayout();
            this.groupLeftPlacePhoto.SuspendLayout();
            this.tableLeftPlacePhoto.SuspendLayout();
            this.groupLeftPlaceCenter.SuspendLayout();
            this.tableLeftPlaceCenter.SuspendLayout();
            this.groupLeftLimit.SuspendLayout();
            this.tableLeftLimitRoot.SuspendLayout();
            this.tableLeftLimitRanges.SuspendLayout();
            this.groupLeftLimitPick.SuspendLayout();
            this.tableLeftLimitPick.SuspendLayout();
            this.groupLeftLimitPlace.SuspendLayout();
            this.tableLeftLimitPlace.SuspendLayout();
            this.tabPageRight.SuspendLayout();
            this.panelRightScroll.SuspendLayout();
            this.tableRightRoot.SuspendLayout();
            this.groupRightPick.SuspendLayout();
            this.tableRightPick.SuspendLayout();
            this.groupRightPlace.SuspendLayout();
            this.tableRightPlace.SuspendLayout();
            this.groupRightPlacePhoto.SuspendLayout();
            this.tableRightPlacePhoto.SuspendLayout();
            this.groupRightPlaceCenter.SuspendLayout();
            this.tableRightPlaceCenter.SuspendLayout();
            this.groupRightLimit.SuspendLayout();
            this.tableRightLimitRoot.SuspendLayout();
            this.tableRightLimitRanges.SuspendLayout();
            this.groupRightLimitPick.SuspendLayout();
            this.tableRightLimitPick.SuspendLayout();
            this.groupRightLimitPlace.SuspendLayout();
            this.tableRightLimitPlace.SuspendLayout();
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
            this.tableLayoutMain.Location = new System.Drawing.Point(24, 24);
            this.tableLayoutMain.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 6);
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 102F));
            this.tableLayoutMain.Size = new System.Drawing.Size(1392, 1392);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // labelHint
            // 
            this.labelHint.AutoSize = true;
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Location = new System.Drawing.Point(6, 6);
            this.labelHint.Margin = new System.Windows.Forms.Padding(6, 6, 6, 15);
            this.labelHint.MaximumSize = new System.Drawing.Size(1350, 0);
            this.labelHint.Name = "labelHint";
            this.labelHint.Size = new System.Drawing.Size(1350, 30);
            this.labelHint.TabIndex = 0;
            this.labelHint.Text = "左/右机台各自独立维护点位与限位报警参数。空白 XY 可带入最近识别结果；放料中心点只需设 RZ。";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageLeft);
            this.tabControl.Controls.Add(this.tabPageRight);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.tabControl.Location = new System.Drawing.Point(4, 55);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(12, 6);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(1384, 1225);
            this.tabControl.TabIndex = 1;
            // 
            // tabPageLeft
            // 
            this.tabPageLeft.Controls.Add(this.panelLeftScroll);
            this.tabPageLeft.Location = new System.Drawing.Point(4, 46);
            this.tabPageLeft.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageLeft.Name = "tabPageLeft";
            this.tabPageLeft.Padding = new System.Windows.Forms.Padding(6);
            this.tabPageLeft.Size = new System.Drawing.Size(1376, 1175);
            this.tabPageLeft.TabIndex = 0;
            this.tabPageLeft.Text = "左机台";
            this.tabPageLeft.UseVisualStyleBackColor = true;
            // 
            // panelLeftScroll
            // 
            this.panelLeftScroll.AutoScroll = true;
            this.panelLeftScroll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.panelLeftScroll.Controls.Add(this.tableLeftRoot);
            this.panelLeftScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeftScroll.Location = new System.Drawing.Point(6, 6);
            this.panelLeftScroll.Margin = new System.Windows.Forms.Padding(4);
            this.panelLeftScroll.Name = "panelLeftScroll";
            this.panelLeftScroll.Padding = new System.Windows.Forms.Padding(18, 18, 42, 18);
            this.panelLeftScroll.Size = new System.Drawing.Size(1364, 1163);
            this.panelLeftScroll.TabIndex = 0;
            // 
            // tableLeftRoot
            // 
            this.tableLeftRoot.AutoSize = true;
            this.tableLeftRoot.ColumnCount = 1;
            this.tableLeftRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLeftRoot.Controls.Add(this.groupLeftPick, 0, 0);
            this.tableLeftRoot.Controls.Add(this.groupLeftPlace, 0, 1);
            this.tableLeftRoot.Controls.Add(this.groupLeftPlacePhoto, 0, 2);
            this.tableLeftRoot.Controls.Add(this.groupLeftPlaceCenter, 0, 3);
            this.tableLeftRoot.Controls.Add(this.groupLeftLimit, 0, 4);
            this.tableLeftRoot.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLeftRoot.Location = new System.Drawing.Point(18, 18);
            this.tableLeftRoot.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftRoot.Name = "tableLeftRoot";
            this.tableLeftRoot.RowCount = 5;
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 228F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 228F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 228F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 570F));
            this.tableLeftRoot.Size = new System.Drawing.Size(1278, 1404);
            this.tableLeftRoot.TabIndex = 0;
            // 
            // groupLeftPick
            // 
            this.groupLeftPick.Controls.Add(this.tableLeftPick);
            this.groupLeftPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPick.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPick.Location = new System.Drawing.Point(0, 0);
            this.groupLeftPick.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.groupLeftPick.Name = "groupLeftPick";
            this.groupLeftPick.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupLeftPick.Size = new System.Drawing.Size(1278, 213);
            this.groupLeftPick.TabIndex = 0;
            this.groupLeftPick.TabStop = false;
            this.groupLeftPick.Text = "取料位置";
            // 
            // tableLeftPick
            // 
            this.tableLeftPick.ColumnCount = 4;
            this.tableLeftPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftPick.Controls.Add(this.labelLeftPickX, 0, 0);
            this.tableLeftPick.Controls.Add(this.textLeftPickX, 1, 0);
            this.tableLeftPick.Controls.Add(this.labelLeftPickY, 2, 0);
            this.tableLeftPick.Controls.Add(this.textLeftPickY, 3, 0);
            this.tableLeftPick.Controls.Add(this.labelLeftPickZ, 0, 1);
            this.tableLeftPick.Controls.Add(this.textLeftPickZ, 1, 1);
            this.tableLeftPick.Controls.Add(this.labelLeftPickRz, 2, 1);
            this.tableLeftPick.Controls.Add(this.textLeftPickRz, 3, 1);
            this.tableLeftPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftPick.Location = new System.Drawing.Point(21, 61);
            this.tableLeftPick.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftPick.Name = "tableLeftPick";
            this.tableLeftPick.RowCount = 2;
            this.tableLeftPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftPick.Size = new System.Drawing.Size(1236, 134);
            this.tableLeftPick.TabIndex = 0;
            // 
            // labelLeftPickX
            // 
            this.labelLeftPickX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickX.AutoSize = true;
            this.labelLeftPickX.Location = new System.Drawing.Point(6, 26);
            this.labelLeftPickX.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPickX.Name = "labelLeftPickX";
            this.labelLeftPickX.Size = new System.Drawing.Size(35, 31);
            this.labelLeftPickX.TabIndex = 0;
            this.labelLeftPickX.Text = "X:";
            this.labelLeftPickX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickX
            // 
            this.textLeftPickX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickX.Location = new System.Drawing.Point(53, 15);
            this.textLeftPickX.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPickX.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPickX.Name = "textLeftPickX";
            this.textLeftPickX.Size = new System.Drawing.Size(542, 38);
            this.textLeftPickX.TabIndex = 1;
            this.textLeftPickX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPickY
            // 
            this.labelLeftPickY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickY.AutoSize = true;
            this.labelLeftPickY.Location = new System.Drawing.Point(633, 26);
            this.labelLeftPickY.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPickY.Name = "labelLeftPickY";
            this.labelLeftPickY.Size = new System.Drawing.Size(34, 31);
            this.labelLeftPickY.TabIndex = 2;
            this.labelLeftPickY.Text = "Y:";
            this.labelLeftPickY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickY
            // 
            this.textLeftPickY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickY.Location = new System.Drawing.Point(679, 15);
            this.textLeftPickY.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPickY.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPickY.Name = "textLeftPickY";
            this.textLeftPickY.Size = new System.Drawing.Size(542, 38);
            this.textLeftPickY.TabIndex = 3;
            this.textLeftPickY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPickZ
            // 
            this.labelLeftPickZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickZ.AutoSize = true;
            this.labelLeftPickZ.Location = new System.Drawing.Point(6, 95);
            this.labelLeftPickZ.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPickZ.Name = "labelLeftPickZ";
            this.labelLeftPickZ.Size = new System.Drawing.Size(35, 31);
            this.labelLeftPickZ.TabIndex = 4;
            this.labelLeftPickZ.Text = "Z:";
            this.labelLeftPickZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickZ
            // 
            this.textLeftPickZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickZ.Location = new System.Drawing.Point(53, 84);
            this.textLeftPickZ.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPickZ.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPickZ.Name = "textLeftPickZ";
            this.textLeftPickZ.Size = new System.Drawing.Size(542, 38);
            this.textLeftPickZ.TabIndex = 5;
            this.textLeftPickZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPickRz
            // 
            this.labelLeftPickRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickRz.AutoSize = true;
            this.labelLeftPickRz.Location = new System.Drawing.Point(616, 95);
            this.labelLeftPickRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPickRz.Name = "labelLeftPickRz";
            this.labelLeftPickRz.Size = new System.Drawing.Size(51, 31);
            this.labelLeftPickRz.TabIndex = 6;
            this.labelLeftPickRz.Text = "RZ:";
            this.labelLeftPickRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickRz
            // 
            this.textLeftPickRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickRz.Location = new System.Drawing.Point(679, 84);
            this.textLeftPickRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPickRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPickRz.Name = "textLeftPickRz";
            this.textLeftPickRz.Size = new System.Drawing.Size(542, 38);
            this.textLeftPickRz.TabIndex = 7;
            this.textLeftPickRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupLeftPlace
            // 
            this.groupLeftPlace.Controls.Add(this.tableLeftPlace);
            this.groupLeftPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPlace.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPlace.Location = new System.Drawing.Point(0, 228);
            this.groupLeftPlace.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.groupLeftPlace.Name = "groupLeftPlace";
            this.groupLeftPlace.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupLeftPlace.Size = new System.Drawing.Size(1278, 213);
            this.groupLeftPlace.TabIndex = 1;
            this.groupLeftPlace.TabStop = false;
            this.groupLeftPlace.Text = "放料位置";
            // 
            // tableLeftPlace
            // 
            this.tableLeftPlace.ColumnCount = 4;
            this.tableLeftPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceX, 0, 0);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceX, 1, 0);
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceY, 2, 0);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceY, 3, 0);
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceZ, 0, 1);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceZ, 1, 1);
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceRz, 2, 1);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceRz, 3, 1);
            this.tableLeftPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftPlace.Location = new System.Drawing.Point(21, 61);
            this.tableLeftPlace.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftPlace.Name = "tableLeftPlace";
            this.tableLeftPlace.RowCount = 2;
            this.tableLeftPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftPlace.Size = new System.Drawing.Size(1236, 134);
            this.tableLeftPlace.TabIndex = 0;
            // 
            // labelLeftPlaceX
            // 
            this.labelLeftPlaceX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceX.AutoSize = true;
            this.labelLeftPlaceX.Location = new System.Drawing.Point(6, 26);
            this.labelLeftPlaceX.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlaceX.Name = "labelLeftPlaceX";
            this.labelLeftPlaceX.Size = new System.Drawing.Size(35, 31);
            this.labelLeftPlaceX.TabIndex = 0;
            this.labelLeftPlaceX.Text = "X:";
            this.labelLeftPlaceX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceX
            // 
            this.textLeftPlaceX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceX.Location = new System.Drawing.Point(53, 15);
            this.textLeftPlaceX.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlaceX.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlaceX.Name = "textLeftPlaceX";
            this.textLeftPlaceX.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlaceX.TabIndex = 1;
            this.textLeftPlaceX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlaceY
            // 
            this.labelLeftPlaceY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceY.AutoSize = true;
            this.labelLeftPlaceY.Location = new System.Drawing.Point(633, 26);
            this.labelLeftPlaceY.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlaceY.Name = "labelLeftPlaceY";
            this.labelLeftPlaceY.Size = new System.Drawing.Size(34, 31);
            this.labelLeftPlaceY.TabIndex = 2;
            this.labelLeftPlaceY.Text = "Y:";
            this.labelLeftPlaceY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceY
            // 
            this.textLeftPlaceY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceY.Location = new System.Drawing.Point(679, 15);
            this.textLeftPlaceY.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlaceY.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlaceY.Name = "textLeftPlaceY";
            this.textLeftPlaceY.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlaceY.TabIndex = 3;
            this.textLeftPlaceY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlaceZ
            // 
            this.labelLeftPlaceZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceZ.AutoSize = true;
            this.labelLeftPlaceZ.Location = new System.Drawing.Point(6, 95);
            this.labelLeftPlaceZ.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlaceZ.Name = "labelLeftPlaceZ";
            this.labelLeftPlaceZ.Size = new System.Drawing.Size(35, 31);
            this.labelLeftPlaceZ.TabIndex = 4;
            this.labelLeftPlaceZ.Text = "Z:";
            this.labelLeftPlaceZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceZ
            // 
            this.textLeftPlaceZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceZ.Location = new System.Drawing.Point(53, 84);
            this.textLeftPlaceZ.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlaceZ.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlaceZ.Name = "textLeftPlaceZ";
            this.textLeftPlaceZ.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlaceZ.TabIndex = 5;
            this.textLeftPlaceZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlaceRz
            // 
            this.labelLeftPlaceRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceRz.AutoSize = true;
            this.labelLeftPlaceRz.Location = new System.Drawing.Point(616, 95);
            this.labelLeftPlaceRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlaceRz.Name = "labelLeftPlaceRz";
            this.labelLeftPlaceRz.Size = new System.Drawing.Size(51, 31);
            this.labelLeftPlaceRz.TabIndex = 6;
            this.labelLeftPlaceRz.Text = "RZ:";
            this.labelLeftPlaceRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceRz
            // 
            this.textLeftPlaceRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceRz.Location = new System.Drawing.Point(679, 84);
            this.textLeftPlaceRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlaceRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlaceRz.Name = "textLeftPlaceRz";
            this.textLeftPlaceRz.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlaceRz.TabIndex = 7;
            this.textLeftPlaceRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupLeftPlacePhoto
            // 
            this.groupLeftPlacePhoto.Controls.Add(this.tableLeftPlacePhoto);
            this.groupLeftPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPlacePhoto.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPlacePhoto.Location = new System.Drawing.Point(0, 456);
            this.groupLeftPlacePhoto.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.groupLeftPlacePhoto.Name = "groupLeftPlacePhoto";
            this.groupLeftPlacePhoto.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupLeftPlacePhoto.Size = new System.Drawing.Size(1278, 213);
            this.groupLeftPlacePhoto.TabIndex = 2;
            this.groupLeftPlacePhoto.TabStop = false;
            this.groupLeftPlacePhoto.Text = "放料拍照位置";
            // 
            // tableLeftPlacePhoto
            // 
            this.tableLeftPlacePhoto.ColumnCount = 4;
            this.tableLeftPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoX, 0, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoX, 1, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoY, 2, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoY, 3, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoZ, 0, 1);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoZ, 1, 1);
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoRz, 2, 1);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoRz, 3, 1);
            this.tableLeftPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftPlacePhoto.Location = new System.Drawing.Point(21, 61);
            this.tableLeftPlacePhoto.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftPlacePhoto.Name = "tableLeftPlacePhoto";
            this.tableLeftPlacePhoto.RowCount = 2;
            this.tableLeftPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftPlacePhoto.Size = new System.Drawing.Size(1236, 134);
            this.tableLeftPlacePhoto.TabIndex = 0;
            // 
            // labelLeftPlacePhotoX
            // 
            this.labelLeftPlacePhotoX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoX.AutoSize = true;
            this.labelLeftPlacePhotoX.Location = new System.Drawing.Point(6, 26);
            this.labelLeftPlacePhotoX.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlacePhotoX.Name = "labelLeftPlacePhotoX";
            this.labelLeftPlacePhotoX.Size = new System.Drawing.Size(35, 31);
            this.labelLeftPlacePhotoX.TabIndex = 0;
            this.labelLeftPlacePhotoX.Text = "X:";
            this.labelLeftPlacePhotoX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoX
            // 
            this.textLeftPlacePhotoX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoX.Location = new System.Drawing.Point(53, 15);
            this.textLeftPlacePhotoX.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlacePhotoX.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlacePhotoX.Name = "textLeftPlacePhotoX";
            this.textLeftPlacePhotoX.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlacePhotoX.TabIndex = 1;
            this.textLeftPlacePhotoX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlacePhotoY
            // 
            this.labelLeftPlacePhotoY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoY.AutoSize = true;
            this.labelLeftPlacePhotoY.Location = new System.Drawing.Point(633, 26);
            this.labelLeftPlacePhotoY.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlacePhotoY.Name = "labelLeftPlacePhotoY";
            this.labelLeftPlacePhotoY.Size = new System.Drawing.Size(34, 31);
            this.labelLeftPlacePhotoY.TabIndex = 2;
            this.labelLeftPlacePhotoY.Text = "Y:";
            this.labelLeftPlacePhotoY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoY
            // 
            this.textLeftPlacePhotoY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoY.Location = new System.Drawing.Point(679, 15);
            this.textLeftPlacePhotoY.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlacePhotoY.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlacePhotoY.Name = "textLeftPlacePhotoY";
            this.textLeftPlacePhotoY.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlacePhotoY.TabIndex = 3;
            this.textLeftPlacePhotoY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlacePhotoZ
            // 
            this.labelLeftPlacePhotoZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoZ.AutoSize = true;
            this.labelLeftPlacePhotoZ.Location = new System.Drawing.Point(6, 95);
            this.labelLeftPlacePhotoZ.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlacePhotoZ.Name = "labelLeftPlacePhotoZ";
            this.labelLeftPlacePhotoZ.Size = new System.Drawing.Size(35, 31);
            this.labelLeftPlacePhotoZ.TabIndex = 4;
            this.labelLeftPlacePhotoZ.Text = "Z:";
            this.labelLeftPlacePhotoZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoZ
            // 
            this.textLeftPlacePhotoZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoZ.Location = new System.Drawing.Point(53, 84);
            this.textLeftPlacePhotoZ.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlacePhotoZ.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlacePhotoZ.Name = "textLeftPlacePhotoZ";
            this.textLeftPlacePhotoZ.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlacePhotoZ.TabIndex = 5;
            this.textLeftPlacePhotoZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlacePhotoRz
            // 
            this.labelLeftPlacePhotoRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoRz.AutoSize = true;
            this.labelLeftPlacePhotoRz.Location = new System.Drawing.Point(616, 95);
            this.labelLeftPlacePhotoRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlacePhotoRz.Name = "labelLeftPlacePhotoRz";
            this.labelLeftPlacePhotoRz.Size = new System.Drawing.Size(51, 31);
            this.labelLeftPlacePhotoRz.TabIndex = 6;
            this.labelLeftPlacePhotoRz.Text = "RZ:";
            this.labelLeftPlacePhotoRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoRz
            // 
            this.textLeftPlacePhotoRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoRz.Location = new System.Drawing.Point(679, 84);
            this.textLeftPlacePhotoRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlacePhotoRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlacePhotoRz.Name = "textLeftPlacePhotoRz";
            this.textLeftPlacePhotoRz.Size = new System.Drawing.Size(542, 38);
            this.textLeftPlacePhotoRz.TabIndex = 7;
            this.textLeftPlacePhotoRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupLeftPlaceCenter
            // 
            this.groupLeftPlaceCenter.Controls.Add(this.tableLeftPlaceCenter);
            this.groupLeftPlaceCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPlaceCenter.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPlaceCenter.Location = new System.Drawing.Point(0, 684);
            this.groupLeftPlaceCenter.Margin = new System.Windows.Forms.Padding(0, 0, 0, 18);
            this.groupLeftPlaceCenter.Name = "groupLeftPlaceCenter";
            this.groupLeftPlaceCenter.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupLeftPlaceCenter.Size = new System.Drawing.Size(1278, 132);
            this.groupLeftPlaceCenter.TabIndex = 3;
            this.groupLeftPlaceCenter.TabStop = false;
            this.groupLeftPlaceCenter.Text = "放料中心点";
            // 
            // tableLeftPlaceCenter
            // 
            this.tableLeftPlaceCenter.ColumnCount = 2;
            this.tableLeftPlaceCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftPlaceCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLeftPlaceCenter.Controls.Add(this.labelLeftPlaceCenterRz, 0, 0);
            this.tableLeftPlaceCenter.Controls.Add(this.textLeftPlaceCenterRz, 1, 0);
            this.tableLeftPlaceCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftPlaceCenter.Location = new System.Drawing.Point(21, 61);
            this.tableLeftPlaceCenter.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftPlaceCenter.Name = "tableLeftPlaceCenter";
            this.tableLeftPlaceCenter.RowCount = 1;
            this.tableLeftPlaceCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftPlaceCenter.Size = new System.Drawing.Size(1236, 53);
            this.tableLeftPlaceCenter.TabIndex = 0;
            // 
            // labelLeftPlaceCenterRz
            // 
            this.labelLeftPlaceCenterRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceCenterRz.AutoSize = true;
            this.labelLeftPlaceCenterRz.Location = new System.Drawing.Point(6, 26);
            this.labelLeftPlaceCenterRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftPlaceCenterRz.Name = "labelLeftPlaceCenterRz";
            this.labelLeftPlaceCenterRz.Size = new System.Drawing.Size(51, 31);
            this.labelLeftPlaceCenterRz.TabIndex = 0;
            this.labelLeftPlaceCenterRz.Text = "RZ:";
            // 
            // textLeftPlaceCenterRz
            // 
            this.textLeftPlaceCenterRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceCenterRz.Location = new System.Drawing.Point(69, 15);
            this.textLeftPlaceCenterRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textLeftPlaceCenterRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textLeftPlaceCenterRz.Name = "textLeftPlaceCenterRz";
            this.textLeftPlaceCenterRz.Size = new System.Drawing.Size(1152, 38);
            this.textLeftPlaceCenterRz.TabIndex = 1;
            this.textLeftPlaceCenterRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            // 
            // groupLeftLimit
            // 
            this.groupLeftLimit.Controls.Add(this.tableLeftLimitRoot);
            this.groupLeftLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftLimit.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftLimit.Location = new System.Drawing.Point(0, 840);
            this.groupLeftLimit.Margin = new System.Windows.Forms.Padding(0, 6, 0, 12);
            this.groupLeftLimit.Name = "groupLeftLimit";
            this.groupLeftLimit.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupLeftLimit.Size = new System.Drawing.Size(1278, 552);
            this.groupLeftLimit.TabIndex = 4;
            this.groupLeftLimit.TabStop = false;
            this.groupLeftLimit.Text = "限位报警参数";
            // 
            // tableLeftLimitRoot
            // 
            this.tableLeftLimitRoot.ColumnCount = 1;
            this.tableLeftLimitRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLeftLimitRoot.Controls.Add(this.labelLeftLimitHint, 0, 0);
            this.tableLeftLimitRoot.Controls.Add(this.checkLeftLimitEnabled, 0, 1);
            this.tableLeftLimitRoot.Controls.Add(this.tableLeftLimitRanges, 0, 2);
            this.tableLeftLimitRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftLimitRoot.Location = new System.Drawing.Point(21, 61);
            this.tableLeftLimitRoot.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftLimitRoot.Name = "tableLeftLimitRoot";
            this.tableLeftLimitRoot.RowCount = 3;
            this.tableLeftLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLeftLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLeftLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 345F));
            this.tableLeftLimitRoot.Size = new System.Drawing.Size(1236, 473);
            this.tableLeftLimitRoot.TabIndex = 0;
            // 
            // labelLeftLimitHint
            // 
            this.labelLeftLimitHint.AutoSize = true;
            this.labelLeftLimitHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLeftLimitHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelLeftLimitHint.Location = new System.Drawing.Point(6, 3);
            this.labelLeftLimitHint.Margin = new System.Windows.Forms.Padding(6, 3, 6, 12);
            this.labelLeftLimitHint.MaximumSize = new System.Drawing.Size(1290, 0);
            this.labelLeftLimitHint.Name = "labelLeftLimitHint";
            this.labelLeftLimitHint.Size = new System.Drawing.Size(1224, 31);
            this.labelLeftLimitHint.TabIndex = 0;
            this.labelLeftLimitHint.Text = "本机台取料/放料坐标发送前按此范围校验。输入值受「安全区域」约束；更改该限制需点「安全区域」登录。";
            // 
            // checkLeftLimitEnabled
            // 
            this.checkLeftLimitEnabled.AutoSize = true;
            this.checkLeftLimitEnabled.Location = new System.Drawing.Point(12, 49);
            this.checkLeftLimitEnabled.Margin = new System.Windows.Forms.Padding(12, 3, 6, 15);
            this.checkLeftLimitEnabled.Name = "checkLeftLimitEnabled";
            this.checkLeftLimitEnabled.Size = new System.Drawing.Size(304, 35);
            this.checkLeftLimitEnabled.TabIndex = 1;
            this.checkLeftLimitEnabled.Text = "启用发送前安全区域校验";
            this.checkLeftLimitEnabled.UseVisualStyleBackColor = true;
            this.checkLeftLimitEnabled.CheckedChanged += new System.EventHandler(this.LimitField_Changed);
            // 
            // tableLeftLimitRanges
            // 
            this.tableLeftLimitRanges.ColumnCount = 2;
            this.tableLeftLimitRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftLimitRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftLimitRanges.Controls.Add(this.groupLeftLimitPick, 0, 0);
            this.tableLeftLimitRanges.Controls.Add(this.groupLeftLimitPlace, 1, 0);
            this.tableLeftLimitRanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftLimitRanges.Location = new System.Drawing.Point(4, 103);
            this.tableLeftLimitRanges.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftLimitRanges.Name = "tableLeftLimitRanges";
            this.tableLeftLimitRanges.RowCount = 1;
            this.tableLeftLimitRanges.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLeftLimitRanges.Size = new System.Drawing.Size(1228, 366);
            this.tableLeftLimitRanges.TabIndex = 2;
            // 
            // groupLeftLimitPick
            // 
            this.groupLeftLimitPick.Controls.Add(this.tableLeftLimitPick);
            this.groupLeftLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftLimitPick.Location = new System.Drawing.Point(6, 3);
            this.groupLeftLimitPick.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.groupLeftLimitPick.Name = "groupLeftLimitPick";
            this.groupLeftLimitPick.Padding = new System.Windows.Forms.Padding(15, 21, 15, 15);
            this.groupLeftLimitPick.Size = new System.Drawing.Size(602, 360);
            this.groupLeftLimitPick.TabIndex = 0;
            this.groupLeftLimitPick.TabStop = false;
            this.groupLeftLimitPick.Text = "取料位置安全范围";
            // 
            // tableLeftLimitPick
            // 
            this.tableLeftLimitPick.ColumnCount = 4;
            this.tableLeftLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftLimitPick.Controls.Add(this.labelLeftLimitPickXMin, 0, 0);
            this.tableLeftLimitPick.Controls.Add(this.textLeftLimitPickXMin, 1, 0);
            this.tableLeftLimitPick.Controls.Add(this.labelLeftLimitPickXMax, 2, 0);
            this.tableLeftLimitPick.Controls.Add(this.textLeftLimitPickXMax, 3, 0);
            this.tableLeftLimitPick.Controls.Add(this.labelLeftLimitPickYMin, 0, 1);
            this.tableLeftLimitPick.Controls.Add(this.textLeftLimitPickYMin, 1, 1);
            this.tableLeftLimitPick.Controls.Add(this.labelLeftLimitPickYMax, 2, 1);
            this.tableLeftLimitPick.Controls.Add(this.textLeftLimitPickYMax, 3, 1);
            this.tableLeftLimitPick.Controls.Add(this.labelLeftLimitPickZMin, 0, 2);
            this.tableLeftLimitPick.Controls.Add(this.textLeftLimitPickZMin, 1, 2);
            this.tableLeftLimitPick.Controls.Add(this.labelLeftLimitPickZMax, 2, 2);
            this.tableLeftLimitPick.Controls.Add(this.textLeftLimitPickZMax, 3, 2);
            this.tableLeftLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftLimitPick.Location = new System.Drawing.Point(15, 52);
            this.tableLeftLimitPick.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftLimitPick.Name = "tableLeftLimitPick";
            this.tableLeftLimitPick.RowCount = 3;
            this.tableLeftLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftLimitPick.Size = new System.Drawing.Size(572, 293);
            this.tableLeftLimitPick.TabIndex = 0;
            // 
            // labelLeftLimitPickXMin
            // 
            this.labelLeftLimitPickXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickXMin.AutoSize = true;
            this.labelLeftLimitPickXMin.Location = new System.Drawing.Point(6, 26);
            this.labelLeftLimitPickXMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPickXMin.Name = "labelLeftLimitPickXMin";
            this.labelLeftLimitPickXMin.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPickXMin.TabIndex = 0;
            this.labelLeftLimitPickXMin.Text = "X最小";
            // 
            // textLeftLimitPickXMin
            // 
            this.textLeftLimitPickXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickXMin.Location = new System.Drawing.Point(95, 15);
            this.textLeftLimitPickXMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPickXMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPickXMin.Name = "textLeftLimitPickXMin";
            this.textLeftLimitPickXMin.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPickXMin.TabIndex = 1;
            this.textLeftLimitPickXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickXMax
            // 
            this.labelLeftLimitPickXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickXMax.AutoSize = true;
            this.labelLeftLimitPickXMax.Location = new System.Drawing.Point(292, 26);
            this.labelLeftLimitPickXMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPickXMax.Name = "labelLeftLimitPickXMax";
            this.labelLeftLimitPickXMax.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPickXMax.TabIndex = 2;
            this.labelLeftLimitPickXMax.Text = "X最大";
            // 
            // textLeftLimitPickXMax
            // 
            this.textLeftLimitPickXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickXMax.Location = new System.Drawing.Point(381, 15);
            this.textLeftLimitPickXMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPickXMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPickXMax.Name = "textLeftLimitPickXMax";
            this.textLeftLimitPickXMax.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPickXMax.TabIndex = 3;
            this.textLeftLimitPickXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickYMin
            // 
            this.labelLeftLimitPickYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickYMin.AutoSize = true;
            this.labelLeftLimitPickYMin.Location = new System.Drawing.Point(7, 95);
            this.labelLeftLimitPickYMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPickYMin.Name = "labelLeftLimitPickYMin";
            this.labelLeftLimitPickYMin.Size = new System.Drawing.Size(76, 31);
            this.labelLeftLimitPickYMin.TabIndex = 4;
            this.labelLeftLimitPickYMin.Text = "Y最小";
            // 
            // textLeftLimitPickYMin
            // 
            this.textLeftLimitPickYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickYMin.Location = new System.Drawing.Point(95, 84);
            this.textLeftLimitPickYMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPickYMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPickYMin.Name = "textLeftLimitPickYMin";
            this.textLeftLimitPickYMin.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPickYMin.TabIndex = 5;
            this.textLeftLimitPickYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickYMax
            // 
            this.labelLeftLimitPickYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickYMax.AutoSize = true;
            this.labelLeftLimitPickYMax.Location = new System.Drawing.Point(293, 95);
            this.labelLeftLimitPickYMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPickYMax.Name = "labelLeftLimitPickYMax";
            this.labelLeftLimitPickYMax.Size = new System.Drawing.Size(76, 31);
            this.labelLeftLimitPickYMax.TabIndex = 6;
            this.labelLeftLimitPickYMax.Text = "Y最大";
            // 
            // textLeftLimitPickYMax
            // 
            this.textLeftLimitPickYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickYMax.Location = new System.Drawing.Point(381, 84);
            this.textLeftLimitPickYMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPickYMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPickYMax.Name = "textLeftLimitPickYMax";
            this.textLeftLimitPickYMax.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPickYMax.TabIndex = 7;
            this.textLeftLimitPickYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickZMin
            // 
            this.labelLeftLimitPickZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickZMin.AutoSize = true;
            this.labelLeftLimitPickZMin.Location = new System.Drawing.Point(6, 207);
            this.labelLeftLimitPickZMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPickZMin.Name = "labelLeftLimitPickZMin";
            this.labelLeftLimitPickZMin.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPickZMin.TabIndex = 8;
            this.labelLeftLimitPickZMin.Text = "Z最小";
            // 
            // textLeftLimitPickZMin
            // 
            this.textLeftLimitPickZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickZMin.Location = new System.Drawing.Point(95, 196);
            this.textLeftLimitPickZMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPickZMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPickZMin.Name = "textLeftLimitPickZMin";
            this.textLeftLimitPickZMin.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPickZMin.TabIndex = 9;
            this.textLeftLimitPickZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickZMax
            // 
            this.labelLeftLimitPickZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickZMax.AutoSize = true;
            this.labelLeftLimitPickZMax.Location = new System.Drawing.Point(292, 207);
            this.labelLeftLimitPickZMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPickZMax.Name = "labelLeftLimitPickZMax";
            this.labelLeftLimitPickZMax.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPickZMax.TabIndex = 10;
            this.labelLeftLimitPickZMax.Text = "Z最大";
            // 
            // textLeftLimitPickZMax
            // 
            this.textLeftLimitPickZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickZMax.Location = new System.Drawing.Point(381, 196);
            this.textLeftLimitPickZMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPickZMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPickZMax.Name = "textLeftLimitPickZMax";
            this.textLeftLimitPickZMax.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPickZMax.TabIndex = 11;
            this.textLeftLimitPickZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // groupLeftLimitPlace
            // 
            this.groupLeftLimitPlace.Controls.Add(this.tableLeftLimitPlace);
            this.groupLeftLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftLimitPlace.Location = new System.Drawing.Point(620, 3);
            this.groupLeftLimitPlace.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.groupLeftLimitPlace.Name = "groupLeftLimitPlace";
            this.groupLeftLimitPlace.Padding = new System.Windows.Forms.Padding(15, 21, 15, 15);
            this.groupLeftLimitPlace.Size = new System.Drawing.Size(602, 360);
            this.groupLeftLimitPlace.TabIndex = 1;
            this.groupLeftLimitPlace.TabStop = false;
            this.groupLeftLimitPlace.Text = "放料位置安全范围";
            // 
            // tableLeftLimitPlace
            // 
            this.tableLeftLimitPlace.ColumnCount = 4;
            this.tableLeftLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLeftLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLeftLimitPlace.Controls.Add(this.labelLeftLimitPlaceXMin, 0, 0);
            this.tableLeftLimitPlace.Controls.Add(this.textLeftLimitPlaceXMin, 1, 0);
            this.tableLeftLimitPlace.Controls.Add(this.labelLeftLimitPlaceXMax, 2, 0);
            this.tableLeftLimitPlace.Controls.Add(this.textLeftLimitPlaceXMax, 3, 0);
            this.tableLeftLimitPlace.Controls.Add(this.labelLeftLimitPlaceYMin, 0, 1);
            this.tableLeftLimitPlace.Controls.Add(this.textLeftLimitPlaceYMin, 1, 1);
            this.tableLeftLimitPlace.Controls.Add(this.labelLeftLimitPlaceYMax, 2, 1);
            this.tableLeftLimitPlace.Controls.Add(this.textLeftLimitPlaceYMax, 3, 1);
            this.tableLeftLimitPlace.Controls.Add(this.labelLeftLimitPlaceZMin, 0, 2);
            this.tableLeftLimitPlace.Controls.Add(this.textLeftLimitPlaceZMin, 1, 2);
            this.tableLeftLimitPlace.Controls.Add(this.labelLeftLimitPlaceZMax, 2, 2);
            this.tableLeftLimitPlace.Controls.Add(this.textLeftLimitPlaceZMax, 3, 2);
            this.tableLeftLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftLimitPlace.Location = new System.Drawing.Point(15, 52);
            this.tableLeftLimitPlace.Margin = new System.Windows.Forms.Padding(4);
            this.tableLeftLimitPlace.Name = "tableLeftLimitPlace";
            this.tableLeftLimitPlace.RowCount = 3;
            this.tableLeftLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableLeftLimitPlace.Size = new System.Drawing.Size(572, 293);
            this.tableLeftLimitPlace.TabIndex = 0;
            // 
            // labelLeftLimitPlaceXMin
            // 
            this.labelLeftLimitPlaceXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceXMin.AutoSize = true;
            this.labelLeftLimitPlaceXMin.Location = new System.Drawing.Point(6, 26);
            this.labelLeftLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPlaceXMin.Name = "labelLeftLimitPlaceXMin";
            this.labelLeftLimitPlaceXMin.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPlaceXMin.TabIndex = 0;
            this.labelLeftLimitPlaceXMin.Text = "X最小";
            // 
            // textLeftLimitPlaceXMin
            // 
            this.textLeftLimitPlaceXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceXMin.Location = new System.Drawing.Point(95, 15);
            this.textLeftLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPlaceXMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPlaceXMin.Name = "textLeftLimitPlaceXMin";
            this.textLeftLimitPlaceXMin.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPlaceXMin.TabIndex = 1;
            this.textLeftLimitPlaceXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceXMax
            // 
            this.labelLeftLimitPlaceXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceXMax.AutoSize = true;
            this.labelLeftLimitPlaceXMax.Location = new System.Drawing.Point(292, 26);
            this.labelLeftLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPlaceXMax.Name = "labelLeftLimitPlaceXMax";
            this.labelLeftLimitPlaceXMax.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPlaceXMax.TabIndex = 2;
            this.labelLeftLimitPlaceXMax.Text = "X最大";
            // 
            // textLeftLimitPlaceXMax
            // 
            this.textLeftLimitPlaceXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceXMax.Location = new System.Drawing.Point(381, 15);
            this.textLeftLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPlaceXMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPlaceXMax.Name = "textLeftLimitPlaceXMax";
            this.textLeftLimitPlaceXMax.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPlaceXMax.TabIndex = 3;
            this.textLeftLimitPlaceXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceYMin
            // 
            this.labelLeftLimitPlaceYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceYMin.AutoSize = true;
            this.labelLeftLimitPlaceYMin.Location = new System.Drawing.Point(7, 95);
            this.labelLeftLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPlaceYMin.Name = "labelLeftLimitPlaceYMin";
            this.labelLeftLimitPlaceYMin.Size = new System.Drawing.Size(76, 31);
            this.labelLeftLimitPlaceYMin.TabIndex = 4;
            this.labelLeftLimitPlaceYMin.Text = "Y最小";
            // 
            // textLeftLimitPlaceYMin
            // 
            this.textLeftLimitPlaceYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceYMin.Location = new System.Drawing.Point(95, 84);
            this.textLeftLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPlaceYMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPlaceYMin.Name = "textLeftLimitPlaceYMin";
            this.textLeftLimitPlaceYMin.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPlaceYMin.TabIndex = 5;
            this.textLeftLimitPlaceYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceYMax
            // 
            this.labelLeftLimitPlaceYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceYMax.AutoSize = true;
            this.labelLeftLimitPlaceYMax.Location = new System.Drawing.Point(293, 95);
            this.labelLeftLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPlaceYMax.Name = "labelLeftLimitPlaceYMax";
            this.labelLeftLimitPlaceYMax.Size = new System.Drawing.Size(76, 31);
            this.labelLeftLimitPlaceYMax.TabIndex = 6;
            this.labelLeftLimitPlaceYMax.Text = "Y最大";
            // 
            // textLeftLimitPlaceYMax
            // 
            this.textLeftLimitPlaceYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceYMax.Location = new System.Drawing.Point(381, 84);
            this.textLeftLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPlaceYMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPlaceYMax.Name = "textLeftLimitPlaceYMax";
            this.textLeftLimitPlaceYMax.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPlaceYMax.TabIndex = 7;
            this.textLeftLimitPlaceYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceZMin
            // 
            this.labelLeftLimitPlaceZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceZMin.AutoSize = true;
            this.labelLeftLimitPlaceZMin.Location = new System.Drawing.Point(6, 207);
            this.labelLeftLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPlaceZMin.Name = "labelLeftLimitPlaceZMin";
            this.labelLeftLimitPlaceZMin.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPlaceZMin.TabIndex = 8;
            this.labelLeftLimitPlaceZMin.Text = "Z最小";
            // 
            // textLeftLimitPlaceZMin
            // 
            this.textLeftLimitPlaceZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceZMin.Location = new System.Drawing.Point(95, 196);
            this.textLeftLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPlaceZMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPlaceZMin.Name = "textLeftLimitPlaceZMin";
            this.textLeftLimitPlaceZMin.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPlaceZMin.TabIndex = 9;
            this.textLeftLimitPlaceZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceZMax
            // 
            this.labelLeftLimitPlaceZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceZMax.AutoSize = true;
            this.labelLeftLimitPlaceZMax.Location = new System.Drawing.Point(292, 207);
            this.labelLeftLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelLeftLimitPlaceZMax.Name = "labelLeftLimitPlaceZMax";
            this.labelLeftLimitPlaceZMax.Size = new System.Drawing.Size(77, 31);
            this.labelLeftLimitPlaceZMax.TabIndex = 10;
            this.labelLeftLimitPlaceZMax.Text = "Z最大";
            // 
            // textLeftLimitPlaceZMax
            // 
            this.textLeftLimitPlaceZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceZMax.Location = new System.Drawing.Point(381, 196);
            this.textLeftLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textLeftLimitPlaceZMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textLeftLimitPlaceZMax.Name = "textLeftLimitPlaceZMax";
            this.textLeftLimitPlaceZMax.Size = new System.Drawing.Size(179, 38);
            this.textLeftLimitPlaceZMax.TabIndex = 11;
            this.textLeftLimitPlaceZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // tabPageRight
            // 
            this.tabPageRight.Controls.Add(this.panelRightScroll);
            this.tabPageRight.Location = new System.Drawing.Point(4, 46);
            this.tabPageRight.Margin = new System.Windows.Forms.Padding(4);
            this.tabPageRight.Name = "tabPageRight";
            this.tabPageRight.Padding = new System.Windows.Forms.Padding(6);
            this.tabPageRight.Size = new System.Drawing.Size(1376, 1175);
            this.tabPageRight.TabIndex = 1;
            this.tabPageRight.Text = "右机台";
            this.tabPageRight.UseVisualStyleBackColor = true;
            // 
            // panelRightScroll
            // 
            this.panelRightScroll.AutoScroll = true;
            this.panelRightScroll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.panelRightScroll.Controls.Add(this.tableRightRoot);
            this.panelRightScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightScroll.Location = new System.Drawing.Point(6, 6);
            this.panelRightScroll.Margin = new System.Windows.Forms.Padding(4);
            this.panelRightScroll.Name = "panelRightScroll";
            this.panelRightScroll.Padding = new System.Windows.Forms.Padding(18, 18, 42, 18);
            this.panelRightScroll.Size = new System.Drawing.Size(1364, 1163);
            this.panelRightScroll.TabIndex = 0;
            // 
            // tableRightRoot
            // 
            this.tableRightRoot.AutoSize = true;
            this.tableRightRoot.ColumnCount = 1;
            this.tableRightRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRightRoot.Controls.Add(this.groupRightPick, 0, 0);
            this.tableRightRoot.Controls.Add(this.groupRightPlace, 0, 1);
            this.tableRightRoot.Controls.Add(this.groupRightPlacePhoto, 0, 2);
            this.tableRightRoot.Controls.Add(this.groupRightPlaceCenter, 0, 3);
            this.tableRightRoot.Controls.Add(this.groupRightLimit, 0, 4);
            this.tableRightRoot.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableRightRoot.Location = new System.Drawing.Point(18, 18);
            this.tableRightRoot.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightRoot.Name = "tableRightRoot";
            this.tableRightRoot.RowCount = 5;
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 228F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 228F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 228F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 570F));
            this.tableRightRoot.Size = new System.Drawing.Size(1278, 1404);
            this.tableRightRoot.TabIndex = 0;
            // 
            // groupRightPick
            // 
            this.groupRightPick.Controls.Add(this.tableRightPick);
            this.groupRightPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPick.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPick.Location = new System.Drawing.Point(0, 0);
            this.groupRightPick.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.groupRightPick.Name = "groupRightPick";
            this.groupRightPick.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupRightPick.Size = new System.Drawing.Size(1278, 213);
            this.groupRightPick.TabIndex = 0;
            this.groupRightPick.TabStop = false;
            this.groupRightPick.Text = "取料位置";
            // 
            // tableRightPick
            // 
            this.tableRightPick.ColumnCount = 4;
            this.tableRightPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightPick.Controls.Add(this.labelRightPickX, 0, 0);
            this.tableRightPick.Controls.Add(this.textRightPickX, 1, 0);
            this.tableRightPick.Controls.Add(this.labelRightPickY, 2, 0);
            this.tableRightPick.Controls.Add(this.textRightPickY, 3, 0);
            this.tableRightPick.Controls.Add(this.labelRightPickZ, 0, 1);
            this.tableRightPick.Controls.Add(this.textRightPickZ, 1, 1);
            this.tableRightPick.Controls.Add(this.labelRightPickRz, 2, 1);
            this.tableRightPick.Controls.Add(this.textRightPickRz, 3, 1);
            this.tableRightPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightPick.Location = new System.Drawing.Point(21, 61);
            this.tableRightPick.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightPick.Name = "tableRightPick";
            this.tableRightPick.RowCount = 2;
            this.tableRightPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightPick.Size = new System.Drawing.Size(1236, 134);
            this.tableRightPick.TabIndex = 0;
            // 
            // labelRightPickX
            // 
            this.labelRightPickX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickX.AutoSize = true;
            this.labelRightPickX.Location = new System.Drawing.Point(6, 26);
            this.labelRightPickX.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPickX.Name = "labelRightPickX";
            this.labelRightPickX.Size = new System.Drawing.Size(35, 31);
            this.labelRightPickX.TabIndex = 0;
            this.labelRightPickX.Text = "X:";
            this.labelRightPickX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickX
            // 
            this.textRightPickX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickX.Location = new System.Drawing.Point(53, 15);
            this.textRightPickX.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPickX.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPickX.Name = "textRightPickX";
            this.textRightPickX.Size = new System.Drawing.Size(542, 38);
            this.textRightPickX.TabIndex = 1;
            this.textRightPickX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPickY
            // 
            this.labelRightPickY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickY.AutoSize = true;
            this.labelRightPickY.Location = new System.Drawing.Point(633, 26);
            this.labelRightPickY.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPickY.Name = "labelRightPickY";
            this.labelRightPickY.Size = new System.Drawing.Size(34, 31);
            this.labelRightPickY.TabIndex = 2;
            this.labelRightPickY.Text = "Y:";
            this.labelRightPickY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickY
            // 
            this.textRightPickY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickY.Location = new System.Drawing.Point(679, 15);
            this.textRightPickY.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPickY.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPickY.Name = "textRightPickY";
            this.textRightPickY.Size = new System.Drawing.Size(542, 38);
            this.textRightPickY.TabIndex = 3;
            this.textRightPickY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPickZ
            // 
            this.labelRightPickZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickZ.AutoSize = true;
            this.labelRightPickZ.Location = new System.Drawing.Point(6, 95);
            this.labelRightPickZ.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPickZ.Name = "labelRightPickZ";
            this.labelRightPickZ.Size = new System.Drawing.Size(35, 31);
            this.labelRightPickZ.TabIndex = 4;
            this.labelRightPickZ.Text = "Z:";
            this.labelRightPickZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickZ
            // 
            this.textRightPickZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickZ.Location = new System.Drawing.Point(53, 84);
            this.textRightPickZ.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPickZ.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPickZ.Name = "textRightPickZ";
            this.textRightPickZ.Size = new System.Drawing.Size(542, 38);
            this.textRightPickZ.TabIndex = 5;
            this.textRightPickZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPickRz
            // 
            this.labelRightPickRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickRz.AutoSize = true;
            this.labelRightPickRz.Location = new System.Drawing.Point(616, 95);
            this.labelRightPickRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPickRz.Name = "labelRightPickRz";
            this.labelRightPickRz.Size = new System.Drawing.Size(51, 31);
            this.labelRightPickRz.TabIndex = 6;
            this.labelRightPickRz.Text = "RZ:";
            this.labelRightPickRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickRz
            // 
            this.textRightPickRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickRz.Location = new System.Drawing.Point(679, 84);
            this.textRightPickRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPickRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPickRz.Name = "textRightPickRz";
            this.textRightPickRz.Size = new System.Drawing.Size(542, 38);
            this.textRightPickRz.TabIndex = 7;
            this.textRightPickRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupRightPlace
            // 
            this.groupRightPlace.Controls.Add(this.tableRightPlace);
            this.groupRightPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPlace.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPlace.Location = new System.Drawing.Point(0, 228);
            this.groupRightPlace.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.groupRightPlace.Name = "groupRightPlace";
            this.groupRightPlace.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupRightPlace.Size = new System.Drawing.Size(1278, 213);
            this.groupRightPlace.TabIndex = 1;
            this.groupRightPlace.TabStop = false;
            this.groupRightPlace.Text = "放料位置";
            // 
            // tableRightPlace
            // 
            this.tableRightPlace.ColumnCount = 4;
            this.tableRightPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightPlace.Controls.Add(this.labelRightPlaceX, 0, 0);
            this.tableRightPlace.Controls.Add(this.textRightPlaceX, 1, 0);
            this.tableRightPlace.Controls.Add(this.labelRightPlaceY, 2, 0);
            this.tableRightPlace.Controls.Add(this.textRightPlaceY, 3, 0);
            this.tableRightPlace.Controls.Add(this.labelRightPlaceZ, 0, 1);
            this.tableRightPlace.Controls.Add(this.textRightPlaceZ, 1, 1);
            this.tableRightPlace.Controls.Add(this.labelRightPlaceRz, 2, 1);
            this.tableRightPlace.Controls.Add(this.textRightPlaceRz, 3, 1);
            this.tableRightPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightPlace.Location = new System.Drawing.Point(21, 61);
            this.tableRightPlace.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightPlace.Name = "tableRightPlace";
            this.tableRightPlace.RowCount = 2;
            this.tableRightPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightPlace.Size = new System.Drawing.Size(1236, 134);
            this.tableRightPlace.TabIndex = 0;
            // 
            // labelRightPlaceX
            // 
            this.labelRightPlaceX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceX.AutoSize = true;
            this.labelRightPlaceX.Location = new System.Drawing.Point(6, 26);
            this.labelRightPlaceX.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlaceX.Name = "labelRightPlaceX";
            this.labelRightPlaceX.Size = new System.Drawing.Size(35, 31);
            this.labelRightPlaceX.TabIndex = 0;
            this.labelRightPlaceX.Text = "X:";
            this.labelRightPlaceX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceX
            // 
            this.textRightPlaceX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceX.Location = new System.Drawing.Point(53, 15);
            this.textRightPlaceX.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlaceX.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlaceX.Name = "textRightPlaceX";
            this.textRightPlaceX.Size = new System.Drawing.Size(542, 38);
            this.textRightPlaceX.TabIndex = 1;
            this.textRightPlaceX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlaceY
            // 
            this.labelRightPlaceY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceY.AutoSize = true;
            this.labelRightPlaceY.Location = new System.Drawing.Point(633, 26);
            this.labelRightPlaceY.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlaceY.Name = "labelRightPlaceY";
            this.labelRightPlaceY.Size = new System.Drawing.Size(34, 31);
            this.labelRightPlaceY.TabIndex = 2;
            this.labelRightPlaceY.Text = "Y:";
            this.labelRightPlaceY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceY
            // 
            this.textRightPlaceY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceY.Location = new System.Drawing.Point(679, 15);
            this.textRightPlaceY.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlaceY.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlaceY.Name = "textRightPlaceY";
            this.textRightPlaceY.Size = new System.Drawing.Size(542, 38);
            this.textRightPlaceY.TabIndex = 3;
            this.textRightPlaceY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlaceZ
            // 
            this.labelRightPlaceZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceZ.AutoSize = true;
            this.labelRightPlaceZ.Location = new System.Drawing.Point(6, 95);
            this.labelRightPlaceZ.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlaceZ.Name = "labelRightPlaceZ";
            this.labelRightPlaceZ.Size = new System.Drawing.Size(35, 31);
            this.labelRightPlaceZ.TabIndex = 4;
            this.labelRightPlaceZ.Text = "Z:";
            this.labelRightPlaceZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceZ
            // 
            this.textRightPlaceZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceZ.Location = new System.Drawing.Point(53, 84);
            this.textRightPlaceZ.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlaceZ.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlaceZ.Name = "textRightPlaceZ";
            this.textRightPlaceZ.Size = new System.Drawing.Size(542, 38);
            this.textRightPlaceZ.TabIndex = 5;
            this.textRightPlaceZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlaceRz
            // 
            this.labelRightPlaceRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceRz.AutoSize = true;
            this.labelRightPlaceRz.Location = new System.Drawing.Point(616, 95);
            this.labelRightPlaceRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlaceRz.Name = "labelRightPlaceRz";
            this.labelRightPlaceRz.Size = new System.Drawing.Size(51, 31);
            this.labelRightPlaceRz.TabIndex = 6;
            this.labelRightPlaceRz.Text = "RZ:";
            this.labelRightPlaceRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceRz
            // 
            this.textRightPlaceRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceRz.Location = new System.Drawing.Point(679, 84);
            this.textRightPlaceRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlaceRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlaceRz.Name = "textRightPlaceRz";
            this.textRightPlaceRz.Size = new System.Drawing.Size(542, 38);
            this.textRightPlaceRz.TabIndex = 7;
            this.textRightPlaceRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupRightPlacePhoto
            // 
            this.groupRightPlacePhoto.Controls.Add(this.tableRightPlacePhoto);
            this.groupRightPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPlacePhoto.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPlacePhoto.Location = new System.Drawing.Point(0, 456);
            this.groupRightPlacePhoto.Margin = new System.Windows.Forms.Padding(0, 0, 0, 15);
            this.groupRightPlacePhoto.Name = "groupRightPlacePhoto";
            this.groupRightPlacePhoto.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupRightPlacePhoto.Size = new System.Drawing.Size(1278, 213);
            this.groupRightPlacePhoto.TabIndex = 2;
            this.groupRightPlacePhoto.TabStop = false;
            this.groupRightPlacePhoto.Text = "放料拍照位置";
            // 
            // tableRightPlacePhoto
            // 
            this.tableRightPlacePhoto.ColumnCount = 4;
            this.tableRightPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightPlacePhoto.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoX, 0, 0);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoX, 1, 0);
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoY, 2, 0);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoY, 3, 0);
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoZ, 0, 1);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoZ, 1, 1);
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoRz, 2, 1);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoRz, 3, 1);
            this.tableRightPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightPlacePhoto.Location = new System.Drawing.Point(21, 61);
            this.tableRightPlacePhoto.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightPlacePhoto.Name = "tableRightPlacePhoto";
            this.tableRightPlacePhoto.RowCount = 2;
            this.tableRightPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightPlacePhoto.Size = new System.Drawing.Size(1236, 134);
            this.tableRightPlacePhoto.TabIndex = 0;
            // 
            // labelRightPlacePhotoX
            // 
            this.labelRightPlacePhotoX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoX.AutoSize = true;
            this.labelRightPlacePhotoX.Location = new System.Drawing.Point(6, 26);
            this.labelRightPlacePhotoX.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlacePhotoX.Name = "labelRightPlacePhotoX";
            this.labelRightPlacePhotoX.Size = new System.Drawing.Size(35, 31);
            this.labelRightPlacePhotoX.TabIndex = 0;
            this.labelRightPlacePhotoX.Text = "X:";
            this.labelRightPlacePhotoX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoX
            // 
            this.textRightPlacePhotoX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoX.Location = new System.Drawing.Point(53, 15);
            this.textRightPlacePhotoX.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlacePhotoX.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlacePhotoX.Name = "textRightPlacePhotoX";
            this.textRightPlacePhotoX.Size = new System.Drawing.Size(542, 38);
            this.textRightPlacePhotoX.TabIndex = 1;
            this.textRightPlacePhotoX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlacePhotoY
            // 
            this.labelRightPlacePhotoY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoY.AutoSize = true;
            this.labelRightPlacePhotoY.Location = new System.Drawing.Point(633, 26);
            this.labelRightPlacePhotoY.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlacePhotoY.Name = "labelRightPlacePhotoY";
            this.labelRightPlacePhotoY.Size = new System.Drawing.Size(34, 31);
            this.labelRightPlacePhotoY.TabIndex = 2;
            this.labelRightPlacePhotoY.Text = "Y:";
            this.labelRightPlacePhotoY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoY
            // 
            this.textRightPlacePhotoY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoY.Location = new System.Drawing.Point(679, 15);
            this.textRightPlacePhotoY.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlacePhotoY.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlacePhotoY.Name = "textRightPlacePhotoY";
            this.textRightPlacePhotoY.Size = new System.Drawing.Size(542, 38);
            this.textRightPlacePhotoY.TabIndex = 3;
            this.textRightPlacePhotoY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlacePhotoZ
            // 
            this.labelRightPlacePhotoZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoZ.AutoSize = true;
            this.labelRightPlacePhotoZ.Location = new System.Drawing.Point(6, 95);
            this.labelRightPlacePhotoZ.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlacePhotoZ.Name = "labelRightPlacePhotoZ";
            this.labelRightPlacePhotoZ.Size = new System.Drawing.Size(35, 31);
            this.labelRightPlacePhotoZ.TabIndex = 4;
            this.labelRightPlacePhotoZ.Text = "Z:";
            this.labelRightPlacePhotoZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoZ
            // 
            this.textRightPlacePhotoZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoZ.Location = new System.Drawing.Point(53, 84);
            this.textRightPlacePhotoZ.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlacePhotoZ.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlacePhotoZ.Name = "textRightPlacePhotoZ";
            this.textRightPlacePhotoZ.Size = new System.Drawing.Size(542, 38);
            this.textRightPlacePhotoZ.TabIndex = 5;
            this.textRightPlacePhotoZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlacePhotoRz
            // 
            this.labelRightPlacePhotoRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoRz.AutoSize = true;
            this.labelRightPlacePhotoRz.Location = new System.Drawing.Point(616, 95);
            this.labelRightPlacePhotoRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlacePhotoRz.Name = "labelRightPlacePhotoRz";
            this.labelRightPlacePhotoRz.Size = new System.Drawing.Size(51, 31);
            this.labelRightPlacePhotoRz.TabIndex = 6;
            this.labelRightPlacePhotoRz.Text = "RZ:";
            this.labelRightPlacePhotoRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoRz
            // 
            this.textRightPlacePhotoRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoRz.Location = new System.Drawing.Point(679, 84);
            this.textRightPlacePhotoRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlacePhotoRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlacePhotoRz.Name = "textRightPlacePhotoRz";
            this.textRightPlacePhotoRz.Size = new System.Drawing.Size(542, 38);
            this.textRightPlacePhotoRz.TabIndex = 7;
            this.textRightPlacePhotoRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupRightPlaceCenter
            // 
            this.groupRightPlaceCenter.Controls.Add(this.tableRightPlaceCenter);
            this.groupRightPlaceCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPlaceCenter.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPlaceCenter.Location = new System.Drawing.Point(0, 684);
            this.groupRightPlaceCenter.Margin = new System.Windows.Forms.Padding(0, 0, 0, 18);
            this.groupRightPlaceCenter.Name = "groupRightPlaceCenter";
            this.groupRightPlaceCenter.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupRightPlaceCenter.Size = new System.Drawing.Size(1278, 132);
            this.groupRightPlaceCenter.TabIndex = 3;
            this.groupRightPlaceCenter.TabStop = false;
            this.groupRightPlaceCenter.Text = "放料中心点";
            // 
            // tableRightPlaceCenter
            // 
            this.tableRightPlaceCenter.ColumnCount = 2;
            this.tableRightPlaceCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightPlaceCenter.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRightPlaceCenter.Controls.Add(this.labelRightPlaceCenterRz, 0, 0);
            this.tableRightPlaceCenter.Controls.Add(this.textRightPlaceCenterRz, 1, 0);
            this.tableRightPlaceCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightPlaceCenter.Location = new System.Drawing.Point(21, 61);
            this.tableRightPlaceCenter.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightPlaceCenter.Name = "tableRightPlaceCenter";
            this.tableRightPlaceCenter.RowCount = 1;
            this.tableRightPlaceCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightPlaceCenter.Size = new System.Drawing.Size(1236, 53);
            this.tableRightPlaceCenter.TabIndex = 0;
            // 
            // labelRightPlaceCenterRz
            // 
            this.labelRightPlaceCenterRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceCenterRz.AutoSize = true;
            this.labelRightPlaceCenterRz.Location = new System.Drawing.Point(6, 26);
            this.labelRightPlaceCenterRz.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightPlaceCenterRz.Name = "labelRightPlaceCenterRz";
            this.labelRightPlaceCenterRz.Size = new System.Drawing.Size(51, 31);
            this.labelRightPlaceCenterRz.TabIndex = 0;
            this.labelRightPlaceCenterRz.Text = "RZ:";
            // 
            // textRightPlaceCenterRz
            // 
            this.textRightPlaceCenterRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceCenterRz.Location = new System.Drawing.Point(69, 15);
            this.textRightPlaceCenterRz.Margin = new System.Windows.Forms.Padding(0, 6, 15, 6);
            this.textRightPlaceCenterRz.MinimumSize = new System.Drawing.Size(118, 30);
            this.textRightPlaceCenterRz.Name = "textRightPlaceCenterRz";
            this.textRightPlaceCenterRz.Size = new System.Drawing.Size(1152, 38);
            this.textRightPlaceCenterRz.TabIndex = 1;
            this.textRightPlaceCenterRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            // 
            // groupRightLimit
            // 
            this.groupRightLimit.Controls.Add(this.tableRightLimitRoot);
            this.groupRightLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightLimit.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightLimit.Location = new System.Drawing.Point(0, 840);
            this.groupRightLimit.Margin = new System.Windows.Forms.Padding(0, 6, 0, 12);
            this.groupRightLimit.Name = "groupRightLimit";
            this.groupRightLimit.Padding = new System.Windows.Forms.Padding(21, 30, 21, 18);
            this.groupRightLimit.Size = new System.Drawing.Size(1278, 552);
            this.groupRightLimit.TabIndex = 4;
            this.groupRightLimit.TabStop = false;
            this.groupRightLimit.Text = "限位报警参数";
            // 
            // tableRightLimitRoot
            // 
            this.tableRightLimitRoot.ColumnCount = 1;
            this.tableRightLimitRoot.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRightLimitRoot.Controls.Add(this.labelRightLimitHint, 0, 0);
            this.tableRightLimitRoot.Controls.Add(this.checkRightLimitEnabled, 0, 1);
            this.tableRightLimitRoot.Controls.Add(this.tableRightLimitRanges, 0, 2);
            this.tableRightLimitRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightLimitRoot.Location = new System.Drawing.Point(21, 61);
            this.tableRightLimitRoot.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightLimitRoot.Name = "tableRightLimitRoot";
            this.tableRightLimitRoot.RowCount = 3;
            this.tableRightLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableRightLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableRightLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 345F));
            this.tableRightLimitRoot.Size = new System.Drawing.Size(1236, 473);
            this.tableRightLimitRoot.TabIndex = 0;
            // 
            // labelRightLimitHint
            // 
            this.labelRightLimitHint.AutoSize = true;
            this.labelRightLimitHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRightLimitHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelRightLimitHint.Location = new System.Drawing.Point(6, 3);
            this.labelRightLimitHint.Margin = new System.Windows.Forms.Padding(6, 3, 6, 12);
            this.labelRightLimitHint.MaximumSize = new System.Drawing.Size(1290, 0);
            this.labelRightLimitHint.Name = "labelRightLimitHint";
            this.labelRightLimitHint.Size = new System.Drawing.Size(1224, 31);
            this.labelRightLimitHint.TabIndex = 0;
            this.labelRightLimitHint.Text = "本机台取料/放料坐标发送前按此范围校验。输入值受「安全区域」约束；更改该限制需点「安全区域」登录。";
            // 
            // checkRightLimitEnabled
            // 
            this.checkRightLimitEnabled.AutoSize = true;
            this.checkRightLimitEnabled.Location = new System.Drawing.Point(12, 49);
            this.checkRightLimitEnabled.Margin = new System.Windows.Forms.Padding(12, 3, 6, 15);
            this.checkRightLimitEnabled.Name = "checkRightLimitEnabled";
            this.checkRightLimitEnabled.Size = new System.Drawing.Size(304, 35);
            this.checkRightLimitEnabled.TabIndex = 1;
            this.checkRightLimitEnabled.Text = "启用发送前安全区域校验";
            this.checkRightLimitEnabled.UseVisualStyleBackColor = true;
            this.checkRightLimitEnabled.CheckedChanged += new System.EventHandler(this.LimitField_Changed);
            // 
            // tableRightLimitRanges
            // 
            this.tableRightLimitRanges.ColumnCount = 2;
            this.tableRightLimitRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightLimitRanges.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightLimitRanges.Controls.Add(this.groupRightLimitPick, 0, 0);
            this.tableRightLimitRanges.Controls.Add(this.groupRightLimitPlace, 1, 0);
            this.tableRightLimitRanges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightLimitRanges.Location = new System.Drawing.Point(4, 103);
            this.tableRightLimitRanges.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightLimitRanges.Name = "tableRightLimitRanges";
            this.tableRightLimitRanges.RowCount = 1;
            this.tableRightLimitRanges.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableRightLimitRanges.Size = new System.Drawing.Size(1228, 366);
            this.tableRightLimitRanges.TabIndex = 2;
            // 
            // groupRightLimitPick
            // 
            this.groupRightLimitPick.Controls.Add(this.tableRightLimitPick);
            this.groupRightLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightLimitPick.Location = new System.Drawing.Point(6, 3);
            this.groupRightLimitPick.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.groupRightLimitPick.Name = "groupRightLimitPick";
            this.groupRightLimitPick.Padding = new System.Windows.Forms.Padding(15, 21, 15, 15);
            this.groupRightLimitPick.Size = new System.Drawing.Size(602, 360);
            this.groupRightLimitPick.TabIndex = 0;
            this.groupRightLimitPick.TabStop = false;
            this.groupRightLimitPick.Text = "取料位置安全范围";
            // 
            // tableRightLimitPick
            // 
            this.tableRightLimitPick.ColumnCount = 4;
            this.tableRightLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightLimitPick.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightLimitPick.Controls.Add(this.labelRightLimitPickXMin, 0, 0);
            this.tableRightLimitPick.Controls.Add(this.textRightLimitPickXMin, 1, 0);
            this.tableRightLimitPick.Controls.Add(this.labelRightLimitPickXMax, 2, 0);
            this.tableRightLimitPick.Controls.Add(this.textRightLimitPickXMax, 3, 0);
            this.tableRightLimitPick.Controls.Add(this.labelRightLimitPickYMin, 0, 1);
            this.tableRightLimitPick.Controls.Add(this.textRightLimitPickYMin, 1, 1);
            this.tableRightLimitPick.Controls.Add(this.labelRightLimitPickYMax, 2, 1);
            this.tableRightLimitPick.Controls.Add(this.textRightLimitPickYMax, 3, 1);
            this.tableRightLimitPick.Controls.Add(this.labelRightLimitPickZMin, 0, 2);
            this.tableRightLimitPick.Controls.Add(this.textRightLimitPickZMin, 1, 2);
            this.tableRightLimitPick.Controls.Add(this.labelRightLimitPickZMax, 2, 2);
            this.tableRightLimitPick.Controls.Add(this.textRightLimitPickZMax, 3, 2);
            this.tableRightLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightLimitPick.Location = new System.Drawing.Point(15, 52);
            this.tableRightLimitPick.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightLimitPick.Name = "tableRightLimitPick";
            this.tableRightLimitPick.RowCount = 3;
            this.tableRightLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightLimitPick.Size = new System.Drawing.Size(572, 293);
            this.tableRightLimitPick.TabIndex = 0;
            // 
            // labelRightLimitPickXMin
            // 
            this.labelRightLimitPickXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickXMin.AutoSize = true;
            this.labelRightLimitPickXMin.Location = new System.Drawing.Point(6, 26);
            this.labelRightLimitPickXMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPickXMin.Name = "labelRightLimitPickXMin";
            this.labelRightLimitPickXMin.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPickXMin.TabIndex = 0;
            this.labelRightLimitPickXMin.Text = "X最小";
            // 
            // textRightLimitPickXMin
            // 
            this.textRightLimitPickXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickXMin.Location = new System.Drawing.Point(95, 15);
            this.textRightLimitPickXMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPickXMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPickXMin.Name = "textRightLimitPickXMin";
            this.textRightLimitPickXMin.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPickXMin.TabIndex = 1;
            this.textRightLimitPickXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickXMax
            // 
            this.labelRightLimitPickXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickXMax.AutoSize = true;
            this.labelRightLimitPickXMax.Location = new System.Drawing.Point(292, 26);
            this.labelRightLimitPickXMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPickXMax.Name = "labelRightLimitPickXMax";
            this.labelRightLimitPickXMax.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPickXMax.TabIndex = 2;
            this.labelRightLimitPickXMax.Text = "X最大";
            // 
            // textRightLimitPickXMax
            // 
            this.textRightLimitPickXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickXMax.Location = new System.Drawing.Point(381, 15);
            this.textRightLimitPickXMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPickXMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPickXMax.Name = "textRightLimitPickXMax";
            this.textRightLimitPickXMax.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPickXMax.TabIndex = 3;
            this.textRightLimitPickXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickYMin
            // 
            this.labelRightLimitPickYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickYMin.AutoSize = true;
            this.labelRightLimitPickYMin.Location = new System.Drawing.Point(7, 95);
            this.labelRightLimitPickYMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPickYMin.Name = "labelRightLimitPickYMin";
            this.labelRightLimitPickYMin.Size = new System.Drawing.Size(76, 31);
            this.labelRightLimitPickYMin.TabIndex = 4;
            this.labelRightLimitPickYMin.Text = "Y最小";
            // 
            // textRightLimitPickYMin
            // 
            this.textRightLimitPickYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickYMin.Location = new System.Drawing.Point(95, 84);
            this.textRightLimitPickYMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPickYMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPickYMin.Name = "textRightLimitPickYMin";
            this.textRightLimitPickYMin.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPickYMin.TabIndex = 5;
            this.textRightLimitPickYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickYMax
            // 
            this.labelRightLimitPickYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickYMax.AutoSize = true;
            this.labelRightLimitPickYMax.Location = new System.Drawing.Point(293, 95);
            this.labelRightLimitPickYMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPickYMax.Name = "labelRightLimitPickYMax";
            this.labelRightLimitPickYMax.Size = new System.Drawing.Size(76, 31);
            this.labelRightLimitPickYMax.TabIndex = 6;
            this.labelRightLimitPickYMax.Text = "Y最大";
            // 
            // textRightLimitPickYMax
            // 
            this.textRightLimitPickYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickYMax.Location = new System.Drawing.Point(381, 84);
            this.textRightLimitPickYMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPickYMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPickYMax.Name = "textRightLimitPickYMax";
            this.textRightLimitPickYMax.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPickYMax.TabIndex = 7;
            this.textRightLimitPickYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickZMin
            // 
            this.labelRightLimitPickZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickZMin.AutoSize = true;
            this.labelRightLimitPickZMin.Location = new System.Drawing.Point(6, 207);
            this.labelRightLimitPickZMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPickZMin.Name = "labelRightLimitPickZMin";
            this.labelRightLimitPickZMin.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPickZMin.TabIndex = 8;
            this.labelRightLimitPickZMin.Text = "Z最小";
            // 
            // textRightLimitPickZMin
            // 
            this.textRightLimitPickZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickZMin.Location = new System.Drawing.Point(95, 196);
            this.textRightLimitPickZMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPickZMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPickZMin.Name = "textRightLimitPickZMin";
            this.textRightLimitPickZMin.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPickZMin.TabIndex = 9;
            this.textRightLimitPickZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickZMax
            // 
            this.labelRightLimitPickZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickZMax.AutoSize = true;
            this.labelRightLimitPickZMax.Location = new System.Drawing.Point(292, 207);
            this.labelRightLimitPickZMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPickZMax.Name = "labelRightLimitPickZMax";
            this.labelRightLimitPickZMax.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPickZMax.TabIndex = 10;
            this.labelRightLimitPickZMax.Text = "Z最大";
            // 
            // textRightLimitPickZMax
            // 
            this.textRightLimitPickZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickZMax.Location = new System.Drawing.Point(381, 196);
            this.textRightLimitPickZMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPickZMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPickZMax.Name = "textRightLimitPickZMax";
            this.textRightLimitPickZMax.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPickZMax.TabIndex = 11;
            this.textRightLimitPickZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // groupRightLimitPlace
            // 
            this.groupRightLimitPlace.Controls.Add(this.tableRightLimitPlace);
            this.groupRightLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightLimitPlace.Location = new System.Drawing.Point(620, 3);
            this.groupRightLimitPlace.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.groupRightLimitPlace.Name = "groupRightLimitPlace";
            this.groupRightLimitPlace.Padding = new System.Windows.Forms.Padding(15, 21, 15, 15);
            this.groupRightLimitPlace.Size = new System.Drawing.Size(602, 360);
            this.groupRightLimitPlace.TabIndex = 1;
            this.groupRightLimitPlace.TabStop = false;
            this.groupRightLimitPlace.Text = "放料位置安全范围";
            // 
            // tableRightLimitPlace
            // 
            this.tableRightLimitPlace.ColumnCount = 4;
            this.tableRightLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableRightLimitPlace.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableRightLimitPlace.Controls.Add(this.labelRightLimitPlaceXMin, 0, 0);
            this.tableRightLimitPlace.Controls.Add(this.textRightLimitPlaceXMin, 1, 0);
            this.tableRightLimitPlace.Controls.Add(this.labelRightLimitPlaceXMax, 2, 0);
            this.tableRightLimitPlace.Controls.Add(this.textRightLimitPlaceXMax, 3, 0);
            this.tableRightLimitPlace.Controls.Add(this.labelRightLimitPlaceYMin, 0, 1);
            this.tableRightLimitPlace.Controls.Add(this.textRightLimitPlaceYMin, 1, 1);
            this.tableRightLimitPlace.Controls.Add(this.labelRightLimitPlaceYMax, 2, 1);
            this.tableRightLimitPlace.Controls.Add(this.textRightLimitPlaceYMax, 3, 1);
            this.tableRightLimitPlace.Controls.Add(this.labelRightLimitPlaceZMin, 0, 2);
            this.tableRightLimitPlace.Controls.Add(this.textRightLimitPlaceZMin, 1, 2);
            this.tableRightLimitPlace.Controls.Add(this.labelRightLimitPlaceZMax, 2, 2);
            this.tableRightLimitPlace.Controls.Add(this.textRightLimitPlaceZMax, 3, 2);
            this.tableRightLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightLimitPlace.Location = new System.Drawing.Point(15, 52);
            this.tableRightLimitPlace.Margin = new System.Windows.Forms.Padding(4);
            this.tableRightLimitPlace.Name = "tableRightLimitPlace";
            this.tableRightLimitPlace.RowCount = 3;
            this.tableRightLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 69F));
            this.tableRightLimitPlace.Size = new System.Drawing.Size(572, 293);
            this.tableRightLimitPlace.TabIndex = 0;
            // 
            // labelRightLimitPlaceXMin
            // 
            this.labelRightLimitPlaceXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceXMin.AutoSize = true;
            this.labelRightLimitPlaceXMin.Location = new System.Drawing.Point(6, 26);
            this.labelRightLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPlaceXMin.Name = "labelRightLimitPlaceXMin";
            this.labelRightLimitPlaceXMin.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPlaceXMin.TabIndex = 0;
            this.labelRightLimitPlaceXMin.Text = "X最小";
            // 
            // textRightLimitPlaceXMin
            // 
            this.textRightLimitPlaceXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceXMin.Location = new System.Drawing.Point(95, 15);
            this.textRightLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPlaceXMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPlaceXMin.Name = "textRightLimitPlaceXMin";
            this.textRightLimitPlaceXMin.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPlaceXMin.TabIndex = 1;
            this.textRightLimitPlaceXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceXMax
            // 
            this.labelRightLimitPlaceXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceXMax.AutoSize = true;
            this.labelRightLimitPlaceXMax.Location = new System.Drawing.Point(292, 26);
            this.labelRightLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPlaceXMax.Name = "labelRightLimitPlaceXMax";
            this.labelRightLimitPlaceXMax.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPlaceXMax.TabIndex = 2;
            this.labelRightLimitPlaceXMax.Text = "X最大";
            // 
            // textRightLimitPlaceXMax
            // 
            this.textRightLimitPlaceXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceXMax.Location = new System.Drawing.Point(381, 15);
            this.textRightLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPlaceXMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPlaceXMax.Name = "textRightLimitPlaceXMax";
            this.textRightLimitPlaceXMax.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPlaceXMax.TabIndex = 3;
            this.textRightLimitPlaceXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceYMin
            // 
            this.labelRightLimitPlaceYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceYMin.AutoSize = true;
            this.labelRightLimitPlaceYMin.Location = new System.Drawing.Point(7, 95);
            this.labelRightLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPlaceYMin.Name = "labelRightLimitPlaceYMin";
            this.labelRightLimitPlaceYMin.Size = new System.Drawing.Size(76, 31);
            this.labelRightLimitPlaceYMin.TabIndex = 4;
            this.labelRightLimitPlaceYMin.Text = "Y最小";
            // 
            // textRightLimitPlaceYMin
            // 
            this.textRightLimitPlaceYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceYMin.Location = new System.Drawing.Point(95, 84);
            this.textRightLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPlaceYMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPlaceYMin.Name = "textRightLimitPlaceYMin";
            this.textRightLimitPlaceYMin.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPlaceYMin.TabIndex = 5;
            this.textRightLimitPlaceYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceYMax
            // 
            this.labelRightLimitPlaceYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceYMax.AutoSize = true;
            this.labelRightLimitPlaceYMax.Location = new System.Drawing.Point(293, 95);
            this.labelRightLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPlaceYMax.Name = "labelRightLimitPlaceYMax";
            this.labelRightLimitPlaceYMax.Size = new System.Drawing.Size(76, 31);
            this.labelRightLimitPlaceYMax.TabIndex = 6;
            this.labelRightLimitPlaceYMax.Text = "Y最大";
            // 
            // textRightLimitPlaceYMax
            // 
            this.textRightLimitPlaceYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceYMax.Location = new System.Drawing.Point(381, 84);
            this.textRightLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPlaceYMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPlaceYMax.Name = "textRightLimitPlaceYMax";
            this.textRightLimitPlaceYMax.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPlaceYMax.TabIndex = 7;
            this.textRightLimitPlaceYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceZMin
            // 
            this.labelRightLimitPlaceZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceZMin.AutoSize = true;
            this.labelRightLimitPlaceZMin.Location = new System.Drawing.Point(6, 207);
            this.labelRightLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPlaceZMin.Name = "labelRightLimitPlaceZMin";
            this.labelRightLimitPlaceZMin.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPlaceZMin.TabIndex = 8;
            this.labelRightLimitPlaceZMin.Text = "Z最小";
            // 
            // textRightLimitPlaceZMin
            // 
            this.textRightLimitPlaceZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceZMin.Location = new System.Drawing.Point(95, 196);
            this.textRightLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPlaceZMin.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPlaceZMin.Name = "textRightLimitPlaceZMin";
            this.textRightLimitPlaceZMin.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPlaceZMin.TabIndex = 9;
            this.textRightLimitPlaceZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceZMax
            // 
            this.labelRightLimitPlaceZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceZMax.AutoSize = true;
            this.labelRightLimitPlaceZMax.Location = new System.Drawing.Point(292, 207);
            this.labelRightLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(6, 15, 12, 0);
            this.labelRightLimitPlaceZMax.Name = "labelRightLimitPlaceZMax";
            this.labelRightLimitPlaceZMax.Size = new System.Drawing.Size(77, 31);
            this.labelRightLimitPlaceZMax.TabIndex = 10;
            this.labelRightLimitPlaceZMax.Text = "Z最大";
            // 
            // textRightLimitPlaceZMax
            // 
            this.textRightLimitPlaceZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceZMax.Location = new System.Drawing.Point(381, 196);
            this.textRightLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(0, 6, 12, 6);
            this.textRightLimitPlaceZMax.MinimumSize = new System.Drawing.Size(106, 30);
            this.textRightLimitPlaceZMax.Name = "textRightLimitPlaceZMax";
            this.textRightLimitPlaceZMax.Size = new System.Drawing.Size(179, 38);
            this.textRightLimitPlaceZMax.TabIndex = 11;
            this.textRightLimitPlaceZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.flowButtonsRight);
            this.panelButtons.Controls.Add(this.buttonFromReco);
            this.panelButtons.Controls.Add(this.buttonFromRecoTab);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Location = new System.Drawing.Point(4, 1288);
            this.panelButtons.Margin = new System.Windows.Forms.Padding(4);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Size = new System.Drawing.Size(1384, 94);
            this.panelButtons.TabIndex = 2;
            // 
            // flowButtonsRight
            // 
            this.flowButtonsRight.Controls.Add(this.buttonSafetyZone);
            this.flowButtonsRight.Controls.Add(this.buttonSave);
            this.flowButtonsRight.Controls.Add(this.buttonCancel);
            this.flowButtonsRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtonsRight.Location = new System.Drawing.Point(884, 0);
            this.flowButtonsRight.Margin = new System.Windows.Forms.Padding(4);
            this.flowButtonsRight.Name = "flowButtonsRight";
            this.flowButtonsRight.Padding = new System.Windows.Forms.Padding(0, 12, 0, 0);
            this.flowButtonsRight.Size = new System.Drawing.Size(500, 94);
            this.flowButtonsRight.TabIndex = 0;
            this.flowButtonsRight.WrapContents = false;
            // 
            // buttonSafetyZone
            // 
            this.buttonSafetyZone.Location = new System.Drawing.Point(0, 12);
            this.buttonSafetyZone.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.buttonSafetyZone.Name = "buttonSafetyZone";
            this.buttonSafetyZone.Size = new System.Drawing.Size(180, 66);
            this.buttonSafetyZone.TabIndex = 0;
            this.buttonSafetyZone.Text = "安全区域";
            this.buttonSafetyZone.UseVisualStyleBackColor = true;
            this.buttonSafetyZone.Click += new System.EventHandler(this.buttonSafetyZone_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Location = new System.Drawing.Point(192, 12);
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0, 0, 12, 0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(150, 66);
            this.buttonSave.TabIndex = 1;
            this.buttonSave.Text = "保存";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(354, 12);
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(125, 66);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonFromReco
            // 
            this.buttonFromReco.Location = new System.Drawing.Point(0, 12);
            this.buttonFromReco.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFromReco.Name = "buttonFromReco";
            this.buttonFromReco.Size = new System.Drawing.Size(240, 66);
            this.buttonFromReco.TabIndex = 1;
            this.buttonFromReco.Text = "全部带入识别XY";
            this.buttonFromReco.UseVisualStyleBackColor = true;
            this.buttonFromReco.Click += new System.EventHandler(this.buttonFromReco_Click);
            // 
            // buttonFromRecoTab
            // 
            this.buttonFromRecoTab.Location = new System.Drawing.Point(252, 12);
            this.buttonFromRecoTab.Margin = new System.Windows.Forms.Padding(4);
            this.buttonFromRecoTab.Name = "buttonFromRecoTab";
            this.buttonFromRecoTab.Size = new System.Drawing.Size(240, 66);
            this.buttonFromRecoTab.TabIndex = 2;
            this.buttonFromRecoTab.Text = "本页带入识别XY";
            this.buttonFromRecoTab.UseVisualStyleBackColor = true;
            this.buttonFromRecoTab.Click += new System.EventHandler(this.buttonFromRecoTab_Click);
            // 
            // PhotoPositionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(144F, 144F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1440, 1440);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1309, 1052);
            this.Name = "PhotoPositionsForm";
            this.Padding = new System.Windows.Forms.Padding(24);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "位置设定";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PhotoPositionsForm_FormClosing);
            this.Load += new System.EventHandler(this.PhotoPositionsForm_Load);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageLeft.ResumeLayout(false);
            this.panelLeftScroll.ResumeLayout(false);
            this.panelLeftScroll.PerformLayout();
            this.tableLeftRoot.ResumeLayout(false);
            this.groupLeftPick.ResumeLayout(false);
            this.tableLeftPick.ResumeLayout(false);
            this.tableLeftPick.PerformLayout();
            this.groupLeftPlace.ResumeLayout(false);
            this.tableLeftPlace.ResumeLayout(false);
            this.tableLeftPlace.PerformLayout();
            this.groupLeftPlacePhoto.ResumeLayout(false);
            this.tableLeftPlacePhoto.ResumeLayout(false);
            this.tableLeftPlacePhoto.PerformLayout();
            this.groupLeftPlaceCenter.ResumeLayout(false);
            this.tableLeftPlaceCenter.ResumeLayout(false);
            this.tableLeftPlaceCenter.PerformLayout();
            this.groupLeftLimit.ResumeLayout(false);
            this.tableLeftLimitRoot.ResumeLayout(false);
            this.tableLeftLimitRoot.PerformLayout();
            this.tableLeftLimitRanges.ResumeLayout(false);
            this.groupLeftLimitPick.ResumeLayout(false);
            this.tableLeftLimitPick.ResumeLayout(false);
            this.tableLeftLimitPick.PerformLayout();
            this.groupLeftLimitPlace.ResumeLayout(false);
            this.tableLeftLimitPlace.ResumeLayout(false);
            this.tableLeftLimitPlace.PerformLayout();
            this.tabPageRight.ResumeLayout(false);
            this.panelRightScroll.ResumeLayout(false);
            this.panelRightScroll.PerformLayout();
            this.tableRightRoot.ResumeLayout(false);
            this.groupRightPick.ResumeLayout(false);
            this.tableRightPick.ResumeLayout(false);
            this.tableRightPick.PerformLayout();
            this.groupRightPlace.ResumeLayout(false);
            this.tableRightPlace.ResumeLayout(false);
            this.tableRightPlace.PerformLayout();
            this.groupRightPlacePhoto.ResumeLayout(false);
            this.tableRightPlacePhoto.ResumeLayout(false);
            this.tableRightPlacePhoto.PerformLayout();
            this.groupRightPlaceCenter.ResumeLayout(false);
            this.tableRightPlaceCenter.ResumeLayout(false);
            this.tableRightPlaceCenter.PerformLayout();
            this.groupRightLimit.ResumeLayout(false);
            this.tableRightLimitRoot.ResumeLayout(false);
            this.tableRightLimitRoot.PerformLayout();
            this.tableRightLimitRanges.ResumeLayout(false);
            this.groupRightLimitPick.ResumeLayout(false);
            this.tableRightLimitPick.ResumeLayout(false);
            this.tableRightLimitPick.PerformLayout();
            this.groupRightLimitPlace.ResumeLayout(false);
            this.tableRightLimitPlace.ResumeLayout(false);
            this.tableRightLimitPlace.PerformLayout();
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
        private System.Windows.Forms.Button buttonSafetyZone;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonFromReco;
        private System.Windows.Forms.Button buttonFromRecoTab;
        private System.Windows.Forms.Panel panelLeftScroll;
        private System.Windows.Forms.TableLayoutPanel tableLeftRoot;
        private System.Windows.Forms.GroupBox groupLeftPick;
        private System.Windows.Forms.TableLayoutPanel tableLeftPick;
        private System.Windows.Forms.GroupBox groupLeftPlace;
        private System.Windows.Forms.TableLayoutPanel tableLeftPlace;
        private System.Windows.Forms.GroupBox groupLeftPlacePhoto;
        private System.Windows.Forms.TableLayoutPanel tableLeftPlacePhoto;
        private System.Windows.Forms.GroupBox groupLeftPlaceCenter;
        private System.Windows.Forms.TableLayoutPanel tableLeftPlaceCenter;
        private System.Windows.Forms.GroupBox groupLeftLimit;
        private System.Windows.Forms.TableLayoutPanel tableLeftLimitRoot;
        private System.Windows.Forms.Label labelLeftLimitHint;
        private System.Windows.Forms.CheckBox checkLeftLimitEnabled;
        private System.Windows.Forms.TableLayoutPanel tableLeftLimitRanges;
        private System.Windows.Forms.GroupBox groupLeftLimitPick;
        private System.Windows.Forms.TableLayoutPanel tableLeftLimitPick;
        private System.Windows.Forms.GroupBox groupLeftLimitPlace;
        private System.Windows.Forms.TableLayoutPanel tableLeftLimitPlace;
        private System.Windows.Forms.Label labelLeftPickX;
        private System.Windows.Forms.TextBox textLeftPickX;
        private System.Windows.Forms.Label labelLeftPickY;
        private System.Windows.Forms.TextBox textLeftPickY;
        private System.Windows.Forms.Label labelLeftPickZ;
        private System.Windows.Forms.TextBox textLeftPickZ;
        private System.Windows.Forms.Label labelLeftPickRz;
        private System.Windows.Forms.TextBox textLeftPickRz;
        private System.Windows.Forms.Label labelLeftPlaceX;
        private System.Windows.Forms.TextBox textLeftPlaceX;
        private System.Windows.Forms.Label labelLeftPlaceY;
        private System.Windows.Forms.TextBox textLeftPlaceY;
        private System.Windows.Forms.Label labelLeftPlaceZ;
        private System.Windows.Forms.TextBox textLeftPlaceZ;
        private System.Windows.Forms.Label labelLeftPlaceRz;
        private System.Windows.Forms.TextBox textLeftPlaceRz;
        private System.Windows.Forms.Label labelLeftPlacePhotoX;
        private System.Windows.Forms.TextBox textLeftPlacePhotoX;
        private System.Windows.Forms.Label labelLeftPlacePhotoY;
        private System.Windows.Forms.TextBox textLeftPlacePhotoY;
        private System.Windows.Forms.Label labelLeftPlacePhotoZ;
        private System.Windows.Forms.TextBox textLeftPlacePhotoZ;
        private System.Windows.Forms.Label labelLeftPlacePhotoRz;
        private System.Windows.Forms.TextBox textLeftPlacePhotoRz;
        private System.Windows.Forms.Label labelLeftPlaceCenterRz;
        private System.Windows.Forms.TextBox textLeftPlaceCenterRz;
        private System.Windows.Forms.Label labelLeftLimitPickXMin;
        private System.Windows.Forms.TextBox textLeftLimitPickXMin;
        private System.Windows.Forms.Label labelLeftLimitPickXMax;
        private System.Windows.Forms.TextBox textLeftLimitPickXMax;
        private System.Windows.Forms.Label labelLeftLimitPickYMin;
        private System.Windows.Forms.TextBox textLeftLimitPickYMin;
        private System.Windows.Forms.Label labelLeftLimitPickYMax;
        private System.Windows.Forms.TextBox textLeftLimitPickYMax;
        private System.Windows.Forms.Label labelLeftLimitPickZMin;
        private System.Windows.Forms.TextBox textLeftLimitPickZMin;
        private System.Windows.Forms.Label labelLeftLimitPickZMax;
        private System.Windows.Forms.TextBox textLeftLimitPickZMax;
        private System.Windows.Forms.Label labelLeftLimitPlaceXMin;
        private System.Windows.Forms.TextBox textLeftLimitPlaceXMin;
        private System.Windows.Forms.Label labelLeftLimitPlaceXMax;
        private System.Windows.Forms.TextBox textLeftLimitPlaceXMax;
        private System.Windows.Forms.Label labelLeftLimitPlaceYMin;
        private System.Windows.Forms.TextBox textLeftLimitPlaceYMin;
        private System.Windows.Forms.Label labelLeftLimitPlaceYMax;
        private System.Windows.Forms.TextBox textLeftLimitPlaceYMax;
        private System.Windows.Forms.Label labelLeftLimitPlaceZMin;
        private System.Windows.Forms.TextBox textLeftLimitPlaceZMin;
        private System.Windows.Forms.Label labelLeftLimitPlaceZMax;
        private System.Windows.Forms.TextBox textLeftLimitPlaceZMax;
        private System.Windows.Forms.Panel panelRightScroll;
        private System.Windows.Forms.TableLayoutPanel tableRightRoot;
        private System.Windows.Forms.GroupBox groupRightPick;
        private System.Windows.Forms.TableLayoutPanel tableRightPick;
        private System.Windows.Forms.GroupBox groupRightPlace;
        private System.Windows.Forms.TableLayoutPanel tableRightPlace;
        private System.Windows.Forms.GroupBox groupRightPlacePhoto;
        private System.Windows.Forms.TableLayoutPanel tableRightPlacePhoto;
        private System.Windows.Forms.GroupBox groupRightPlaceCenter;
        private System.Windows.Forms.TableLayoutPanel tableRightPlaceCenter;
        private System.Windows.Forms.GroupBox groupRightLimit;
        private System.Windows.Forms.TableLayoutPanel tableRightLimitRoot;
        private System.Windows.Forms.Label labelRightLimitHint;
        private System.Windows.Forms.CheckBox checkRightLimitEnabled;
        private System.Windows.Forms.TableLayoutPanel tableRightLimitRanges;
        private System.Windows.Forms.GroupBox groupRightLimitPick;
        private System.Windows.Forms.TableLayoutPanel tableRightLimitPick;
        private System.Windows.Forms.GroupBox groupRightLimitPlace;
        private System.Windows.Forms.TableLayoutPanel tableRightLimitPlace;
        private System.Windows.Forms.Label labelRightPickX;
        private System.Windows.Forms.TextBox textRightPickX;
        private System.Windows.Forms.Label labelRightPickY;
        private System.Windows.Forms.TextBox textRightPickY;
        private System.Windows.Forms.Label labelRightPickZ;
        private System.Windows.Forms.TextBox textRightPickZ;
        private System.Windows.Forms.Label labelRightPickRz;
        private System.Windows.Forms.TextBox textRightPickRz;
        private System.Windows.Forms.Label labelRightPlaceX;
        private System.Windows.Forms.TextBox textRightPlaceX;
        private System.Windows.Forms.Label labelRightPlaceY;
        private System.Windows.Forms.TextBox textRightPlaceY;
        private System.Windows.Forms.Label labelRightPlaceZ;
        private System.Windows.Forms.TextBox textRightPlaceZ;
        private System.Windows.Forms.Label labelRightPlaceRz;
        private System.Windows.Forms.TextBox textRightPlaceRz;
        private System.Windows.Forms.Label labelRightPlacePhotoX;
        private System.Windows.Forms.TextBox textRightPlacePhotoX;
        private System.Windows.Forms.Label labelRightPlacePhotoY;
        private System.Windows.Forms.TextBox textRightPlacePhotoY;
        private System.Windows.Forms.Label labelRightPlacePhotoZ;
        private System.Windows.Forms.TextBox textRightPlacePhotoZ;
        private System.Windows.Forms.Label labelRightPlacePhotoRz;
        private System.Windows.Forms.TextBox textRightPlacePhotoRz;
        private System.Windows.Forms.Label labelRightPlaceCenterRz;
        private System.Windows.Forms.TextBox textRightPlaceCenterRz;
        private System.Windows.Forms.Label labelRightLimitPickXMin;
        private System.Windows.Forms.TextBox textRightLimitPickXMin;
        private System.Windows.Forms.Label labelRightLimitPickXMax;
        private System.Windows.Forms.TextBox textRightLimitPickXMax;
        private System.Windows.Forms.Label labelRightLimitPickYMin;
        private System.Windows.Forms.TextBox textRightLimitPickYMin;
        private System.Windows.Forms.Label labelRightLimitPickYMax;
        private System.Windows.Forms.TextBox textRightLimitPickYMax;
        private System.Windows.Forms.Label labelRightLimitPickZMin;
        private System.Windows.Forms.TextBox textRightLimitPickZMin;
        private System.Windows.Forms.Label labelRightLimitPickZMax;
        private System.Windows.Forms.TextBox textRightLimitPickZMax;
        private System.Windows.Forms.Label labelRightLimitPlaceXMin;
        private System.Windows.Forms.TextBox textRightLimitPlaceXMin;
        private System.Windows.Forms.Label labelRightLimitPlaceXMax;
        private System.Windows.Forms.TextBox textRightLimitPlaceXMax;
        private System.Windows.Forms.Label labelRightLimitPlaceYMin;
        private System.Windows.Forms.TextBox textRightLimitPlaceYMin;
        private System.Windows.Forms.Label labelRightLimitPlaceYMax;
        private System.Windows.Forms.TextBox textRightLimitPlaceYMax;
        private System.Windows.Forms.Label labelRightLimitPlaceZMin;
        private System.Windows.Forms.TextBox textRightLimitPlaceZMin;
        private System.Windows.Forms.Label labelRightLimitPlaceZMax;
        private System.Windows.Forms.TextBox textRightLimitPlaceZMax;
    }
}
