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
            this.tabPageRight = new System.Windows.Forms.TabPage();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.flowButtonsRight = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonSafetyZone = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonFromReco = new System.Windows.Forms.Button();
            this.buttonFromRecoTab = new System.Windows.Forms.Button();
            this.panelLeftScroll = new System.Windows.Forms.Panel();
            this.tableLeftRoot = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftPick = new System.Windows.Forms.GroupBox();
            this.tableLeftPick = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftPlace = new System.Windows.Forms.GroupBox();
            this.tableLeftPlace = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftPlacePhoto = new System.Windows.Forms.GroupBox();
            this.tableLeftPlacePhoto = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftPlaceCenter = new System.Windows.Forms.GroupBox();
            this.tableLeftPlaceCenter = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftLimit = new System.Windows.Forms.GroupBox();
            this.tableLeftLimitRoot = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftLimitHint = new System.Windows.Forms.Label();
            this.checkLeftLimitEnabled = new System.Windows.Forms.CheckBox();
            this.tableLeftLimitRanges = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftLimitPick = new System.Windows.Forms.GroupBox();
            this.tableLeftLimitPick = new System.Windows.Forms.TableLayoutPanel();
            this.groupLeftLimitPlace = new System.Windows.Forms.GroupBox();
            this.tableLeftLimitPlace = new System.Windows.Forms.TableLayoutPanel();
            this.labelLeftPickX = new System.Windows.Forms.Label();
            this.textLeftPickX = new System.Windows.Forms.TextBox();
            this.labelLeftPickY = new System.Windows.Forms.Label();
            this.textLeftPickY = new System.Windows.Forms.TextBox();
            this.labelLeftPickZ = new System.Windows.Forms.Label();
            this.textLeftPickZ = new System.Windows.Forms.TextBox();
            this.labelLeftPickRz = new System.Windows.Forms.Label();
            this.textLeftPickRz = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceX = new System.Windows.Forms.Label();
            this.textLeftPlaceX = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceY = new System.Windows.Forms.Label();
            this.textLeftPlaceY = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceZ = new System.Windows.Forms.Label();
            this.textLeftPlaceZ = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceRz = new System.Windows.Forms.Label();
            this.textLeftPlaceRz = new System.Windows.Forms.TextBox();
            this.labelLeftPlacePhotoX = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoX = new System.Windows.Forms.TextBox();
            this.labelLeftPlacePhotoY = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoY = new System.Windows.Forms.TextBox();
            this.labelLeftPlacePhotoZ = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoZ = new System.Windows.Forms.TextBox();
            this.labelLeftPlacePhotoRz = new System.Windows.Forms.Label();
            this.textLeftPlacePhotoRz = new System.Windows.Forms.TextBox();
            this.labelLeftPlaceCenterRz = new System.Windows.Forms.Label();
            this.textLeftPlaceCenterRz = new System.Windows.Forms.TextBox();
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
            this.panelRightScroll = new System.Windows.Forms.Panel();
            this.tableRightRoot = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightPick = new System.Windows.Forms.GroupBox();
            this.tableRightPick = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightPlace = new System.Windows.Forms.GroupBox();
            this.tableRightPlace = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightPlacePhoto = new System.Windows.Forms.GroupBox();
            this.tableRightPlacePhoto = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightPlaceCenter = new System.Windows.Forms.GroupBox();
            this.tableRightPlaceCenter = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightLimit = new System.Windows.Forms.GroupBox();
            this.tableRightLimitRoot = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightLimitHint = new System.Windows.Forms.Label();
            this.checkRightLimitEnabled = new System.Windows.Forms.CheckBox();
            this.tableRightLimitRanges = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightLimitPick = new System.Windows.Forms.GroupBox();
            this.tableRightLimitPick = new System.Windows.Forms.TableLayoutPanel();
            this.groupRightLimitPlace = new System.Windows.Forms.GroupBox();
            this.tableRightLimitPlace = new System.Windows.Forms.TableLayoutPanel();
            this.labelRightPickX = new System.Windows.Forms.Label();
            this.textRightPickX = new System.Windows.Forms.TextBox();
            this.labelRightPickY = new System.Windows.Forms.Label();
            this.textRightPickY = new System.Windows.Forms.TextBox();
            this.labelRightPickZ = new System.Windows.Forms.Label();
            this.textRightPickZ = new System.Windows.Forms.TextBox();
            this.labelRightPickRz = new System.Windows.Forms.Label();
            this.textRightPickRz = new System.Windows.Forms.TextBox();
            this.labelRightPlaceX = new System.Windows.Forms.Label();
            this.textRightPlaceX = new System.Windows.Forms.TextBox();
            this.labelRightPlaceY = new System.Windows.Forms.Label();
            this.textRightPlaceY = new System.Windows.Forms.TextBox();
            this.labelRightPlaceZ = new System.Windows.Forms.Label();
            this.textRightPlaceZ = new System.Windows.Forms.TextBox();
            this.labelRightPlaceRz = new System.Windows.Forms.Label();
            this.textRightPlaceRz = new System.Windows.Forms.TextBox();
            this.labelRightPlacePhotoX = new System.Windows.Forms.Label();
            this.textRightPlacePhotoX = new System.Windows.Forms.TextBox();
            this.labelRightPlacePhotoY = new System.Windows.Forms.Label();
            this.textRightPlacePhotoY = new System.Windows.Forms.TextBox();
            this.labelRightPlacePhotoZ = new System.Windows.Forms.Label();
            this.textRightPlacePhotoZ = new System.Windows.Forms.TextBox();
            this.labelRightPlacePhotoRz = new System.Windows.Forms.Label();
            this.textRightPlacePhotoRz = new System.Windows.Forms.TextBox();
            this.labelRightPlaceCenterRz = new System.Windows.Forms.Label();
            this.textRightPlaceCenterRz = new System.Windows.Forms.TextBox();
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
            this.tableLayoutMain.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageLeft.SuspendLayout();
            this.tabPageRight.SuspendLayout();
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
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.Padding = new System.Windows.Forms.Padding(0, 0, 0, 4);
            this.tableLayoutMain.RowCount = 3;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 68F));
            // 
            // labelHint
            // 
            this.labelHint.AutoSize = true;
            this.labelHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHint.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.labelHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelHint.Margin = new System.Windows.Forms.Padding(4, 4, 4, 10);
            this.labelHint.MaximumSize = new System.Drawing.Size(900, 0);
            this.labelHint.Name = "labelHint";
            this.labelHint.Text = "左/右机台各自独立维护点位与限位报警参数。空白 XY 可带入最近识别结果；放料中心点只需设 RZ。";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPageLeft);
            this.tabControl.Controls.Add(this.tabPageRight);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(12, 6);
            this.tabControl.SelectedIndex = 0;

            // 
            // tabPageLeft
            // 
            this.tabPageLeft.Controls.Add(this.panelLeftScroll);
            this.tabPageLeft.Name = "tabPageLeft";
            this.tabPageLeft.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageLeft.Text = "左机台";
            this.tabPageLeft.UseVisualStyleBackColor = true;
            // 
            // panelLeftScroll
            // 
            this.panelLeftScroll.AutoScroll = true;
            this.panelLeftScroll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.panelLeftScroll.Controls.Add(this.tableLeftRoot);
            this.panelLeftScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeftScroll.Name = "panelLeftScroll";
            this.panelLeftScroll.Padding = new System.Windows.Forms.Padding(12, 12, 28, 12);
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
            this.tableLeftRoot.Name = "tableLeftRoot";
            this.tableLeftRoot.RowCount = 5;
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLeftRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 380F));

            // 
            // groupLeftPick
            // 
            this.groupLeftPick.Controls.Add(this.tableLeftPick);
            this.groupLeftPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPick.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPick.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupLeftPick.Name = "groupLeftPick";
            this.groupLeftPick.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableLeftPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftPick.Name = "tableLeftPick";
            this.tableLeftPick.RowCount = 2;
            this.tableLeftPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftPick.Controls.Add(this.labelLeftPickX, 0, 0);
            this.tableLeftPick.Controls.Add(this.textLeftPickX, 1, 0);
            this.tableLeftPick.Controls.Add(this.labelLeftPickY, 2, 0);
            this.tableLeftPick.Controls.Add(this.textLeftPickY, 3, 0);
            this.tableLeftPick.Controls.Add(this.labelLeftPickZ, 0, 1);
            this.tableLeftPick.Controls.Add(this.textLeftPickZ, 1, 1);
            this.tableLeftPick.Controls.Add(this.labelLeftPickRz, 2, 1);
            this.tableLeftPick.Controls.Add(this.textLeftPickRz, 3, 1);
            // 
            // labelLeftPickX
            // 
            this.labelLeftPickX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickX.AutoSize = true;
            this.labelLeftPickX.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPickX.Name = "labelLeftPickX";
            this.labelLeftPickX.Text = "X:";
            this.labelLeftPickX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickX
            // 
            this.textLeftPickX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickX.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPickX.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPickX.Name = "textLeftPickX";
            this.textLeftPickX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPickY
            // 
            this.labelLeftPickY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickY.AutoSize = true;
            this.labelLeftPickY.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPickY.Name = "labelLeftPickY";
            this.labelLeftPickY.Text = "Y:";
            this.labelLeftPickY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickY
            // 
            this.textLeftPickY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickY.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPickY.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPickY.Name = "textLeftPickY";
            this.textLeftPickY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPickZ
            // 
            this.labelLeftPickZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickZ.AutoSize = true;
            this.labelLeftPickZ.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPickZ.Name = "labelLeftPickZ";
            this.labelLeftPickZ.Text = "Z:";
            this.labelLeftPickZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickZ
            // 
            this.textLeftPickZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickZ.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPickZ.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPickZ.Name = "textLeftPickZ";
            this.textLeftPickZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPickRz
            // 
            this.labelLeftPickRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPickRz.AutoSize = true;
            this.labelLeftPickRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPickRz.Name = "labelLeftPickRz";
            this.labelLeftPickRz.Text = "RZ:";
            this.labelLeftPickRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPickRz
            // 
            this.textLeftPickRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPickRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPickRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPickRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPickRz.Name = "textLeftPickRz";
            this.textLeftPickRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPickRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupLeftPlace
            // 
            this.groupLeftPlace.Controls.Add(this.tableLeftPlace);
            this.groupLeftPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPlace.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPlace.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupLeftPlace.Name = "groupLeftPlace";
            this.groupLeftPlace.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableLeftPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftPlace.Name = "tableLeftPlace";
            this.tableLeftPlace.RowCount = 2;
            this.tableLeftPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceX, 0, 0);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceX, 1, 0);
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceY, 2, 0);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceY, 3, 0);
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceZ, 0, 1);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceZ, 1, 1);
            this.tableLeftPlace.Controls.Add(this.labelLeftPlaceRz, 2, 1);
            this.tableLeftPlace.Controls.Add(this.textLeftPlaceRz, 3, 1);
            // 
            // labelLeftPlaceX
            // 
            this.labelLeftPlaceX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceX.AutoSize = true;
            this.labelLeftPlaceX.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlaceX.Name = "labelLeftPlaceX";
            this.labelLeftPlaceX.Text = "X:";
            this.labelLeftPlaceX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceX
            // 
            this.textLeftPlaceX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceX.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlaceX.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlaceX.Name = "textLeftPlaceX";
            this.textLeftPlaceX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlaceY
            // 
            this.labelLeftPlaceY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceY.AutoSize = true;
            this.labelLeftPlaceY.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlaceY.Name = "labelLeftPlaceY";
            this.labelLeftPlaceY.Text = "Y:";
            this.labelLeftPlaceY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceY
            // 
            this.textLeftPlaceY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceY.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlaceY.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlaceY.Name = "textLeftPlaceY";
            this.textLeftPlaceY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlaceZ
            // 
            this.labelLeftPlaceZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceZ.AutoSize = true;
            this.labelLeftPlaceZ.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlaceZ.Name = "labelLeftPlaceZ";
            this.labelLeftPlaceZ.Text = "Z:";
            this.labelLeftPlaceZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceZ
            // 
            this.textLeftPlaceZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceZ.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlaceZ.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlaceZ.Name = "textLeftPlaceZ";
            this.textLeftPlaceZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlaceRz
            // 
            this.labelLeftPlaceRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceRz.AutoSize = true;
            this.labelLeftPlaceRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlaceRz.Name = "labelLeftPlaceRz";
            this.labelLeftPlaceRz.Text = "RZ:";
            this.labelLeftPlaceRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlaceRz
            // 
            this.textLeftPlaceRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlaceRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlaceRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlaceRz.Name = "textLeftPlaceRz";
            this.textLeftPlaceRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlaceRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupLeftPlacePhoto
            // 
            this.groupLeftPlacePhoto.Controls.Add(this.tableLeftPlacePhoto);
            this.groupLeftPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPlacePhoto.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPlacePhoto.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupLeftPlacePhoto.Name = "groupLeftPlacePhoto";
            this.groupLeftPlacePhoto.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableLeftPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftPlacePhoto.Name = "tableLeftPlacePhoto";
            this.tableLeftPlacePhoto.RowCount = 2;
            this.tableLeftPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoX, 0, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoX, 1, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoY, 2, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoY, 3, 0);
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoZ, 0, 1);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoZ, 1, 1);
            this.tableLeftPlacePhoto.Controls.Add(this.labelLeftPlacePhotoRz, 2, 1);
            this.tableLeftPlacePhoto.Controls.Add(this.textLeftPlacePhotoRz, 3, 1);
            // 
            // labelLeftPlacePhotoX
            // 
            this.labelLeftPlacePhotoX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoX.AutoSize = true;
            this.labelLeftPlacePhotoX.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlacePhotoX.Name = "labelLeftPlacePhotoX";
            this.labelLeftPlacePhotoX.Text = "X:";
            this.labelLeftPlacePhotoX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoX
            // 
            this.textLeftPlacePhotoX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoX.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlacePhotoX.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlacePhotoX.Name = "textLeftPlacePhotoX";
            this.textLeftPlacePhotoX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlacePhotoY
            // 
            this.labelLeftPlacePhotoY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoY.AutoSize = true;
            this.labelLeftPlacePhotoY.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlacePhotoY.Name = "labelLeftPlacePhotoY";
            this.labelLeftPlacePhotoY.Text = "Y:";
            this.labelLeftPlacePhotoY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoY
            // 
            this.textLeftPlacePhotoY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoY.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlacePhotoY.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlacePhotoY.Name = "textLeftPlacePhotoY";
            this.textLeftPlacePhotoY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlacePhotoZ
            // 
            this.labelLeftPlacePhotoZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoZ.AutoSize = true;
            this.labelLeftPlacePhotoZ.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlacePhotoZ.Name = "labelLeftPlacePhotoZ";
            this.labelLeftPlacePhotoZ.Text = "Z:";
            this.labelLeftPlacePhotoZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoZ
            // 
            this.textLeftPlacePhotoZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoZ.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlacePhotoZ.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlacePhotoZ.Name = "textLeftPlacePhotoZ";
            this.textLeftPlacePhotoZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelLeftPlacePhotoRz
            // 
            this.labelLeftPlacePhotoRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlacePhotoRz.AutoSize = true;
            this.labelLeftPlacePhotoRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlacePhotoRz.Name = "labelLeftPlacePhotoRz";
            this.labelLeftPlacePhotoRz.Text = "RZ:";
            this.labelLeftPlacePhotoRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textLeftPlacePhotoRz
            // 
            this.textLeftPlacePhotoRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlacePhotoRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textLeftPlacePhotoRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlacePhotoRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlacePhotoRz.Name = "textLeftPlacePhotoRz";
            this.textLeftPlacePhotoRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textLeftPlacePhotoRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupLeftPlaceCenter
            // 
            this.groupLeftPlaceCenter.Controls.Add(this.tableLeftPlaceCenter);
            this.groupLeftPlaceCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftPlaceCenter.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftPlaceCenter.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.groupLeftPlaceCenter.Name = "groupLeftPlaceCenter";
            this.groupLeftPlaceCenter.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableLeftPlaceCenter.Name = "tableLeftPlaceCenter";
            this.tableLeftPlaceCenter.RowCount = 1;
            this.tableLeftPlaceCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            // 
            // labelLeftPlaceCenterRz
            // 
            this.labelLeftPlaceCenterRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftPlaceCenterRz.AutoSize = true;
            this.labelLeftPlaceCenterRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftPlaceCenterRz.Name = "labelLeftPlaceCenterRz";
            this.labelLeftPlaceCenterRz.Text = "RZ:";
            // 
            // textLeftPlaceCenterRz
            // 
            this.textLeftPlaceCenterRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftPlaceCenterRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textLeftPlaceCenterRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textLeftPlaceCenterRz.Name = "textLeftPlaceCenterRz";
            this.textLeftPlaceCenterRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            // 
            // groupLeftLimit
            // 
            this.groupLeftLimit.Controls.Add(this.tableLeftLimitRoot);
            this.groupLeftLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftLimit.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupLeftLimit.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.groupLeftLimit.Name = "groupLeftLimit";
            this.groupLeftLimit.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableLeftLimitRoot.Name = "tableLeftLimitRoot";
            this.tableLeftLimitRoot.RowCount = 3;
            this.tableLeftLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLeftLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLeftLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            // 
            // labelLeftLimitHint
            // 
            this.labelLeftLimitHint.AutoSize = true;
            this.labelLeftLimitHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelLeftLimitHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelLeftLimitHint.Margin = new System.Windows.Forms.Padding(4, 2, 4, 8);
            this.labelLeftLimitHint.MaximumSize = new System.Drawing.Size(860, 0);
            this.labelLeftLimitHint.Name = "labelLeftLimitHint";
            this.labelLeftLimitHint.Text = "本机台取料/放料坐标发送前按此范围校验。输入值受「安全区域」约束；更改该限制需点「安全区域」登录。";
            // 
            // checkLeftLimitEnabled
            // 
            this.checkLeftLimitEnabled.AutoSize = true;
            this.checkLeftLimitEnabled.Margin = new System.Windows.Forms.Padding(8, 2, 4, 10);
            this.checkLeftLimitEnabled.Name = "checkLeftLimitEnabled";
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
            this.tableLeftLimitRanges.Name = "tableLeftLimitRanges";
            this.tableLeftLimitRanges.RowCount = 1;
            this.tableLeftLimitRanges.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            // 
            // groupLeftLimitPick
            // 
            this.groupLeftLimitPick.Controls.Add(this.tableLeftLimitPick);
            this.groupLeftLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftLimitPick.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.groupLeftLimitPick.Name = "groupLeftLimitPick";
            this.groupLeftLimitPick.Padding = new System.Windows.Forms.Padding(10, 14, 10, 10);
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
            this.tableLeftLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftLimitPick.Name = "tableLeftLimitPick";
            this.tableLeftLimitPick.RowCount = 3;
            this.tableLeftLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
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
            // 
            // labelLeftLimitPickXMin
            // 
            this.labelLeftLimitPickXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickXMin.AutoSize = true;
            this.labelLeftLimitPickXMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPickXMin.Name = "labelLeftLimitPickXMin";
            this.labelLeftLimitPickXMin.Text = "X最小";
            // 
            // textLeftLimitPickXMin
            // 
            this.textLeftLimitPickXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickXMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPickXMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPickXMin.Name = "textLeftLimitPickXMin";
            this.textLeftLimitPickXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickXMax
            // 
            this.labelLeftLimitPickXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickXMax.AutoSize = true;
            this.labelLeftLimitPickXMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPickXMax.Name = "labelLeftLimitPickXMax";
            this.labelLeftLimitPickXMax.Text = "X最大";
            // 
            // textLeftLimitPickXMax
            // 
            this.textLeftLimitPickXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickXMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPickXMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPickXMax.Name = "textLeftLimitPickXMax";
            this.textLeftLimitPickXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickYMin
            // 
            this.labelLeftLimitPickYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickYMin.AutoSize = true;
            this.labelLeftLimitPickYMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPickYMin.Name = "labelLeftLimitPickYMin";
            this.labelLeftLimitPickYMin.Text = "Y最小";
            // 
            // textLeftLimitPickYMin
            // 
            this.textLeftLimitPickYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickYMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPickYMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPickYMin.Name = "textLeftLimitPickYMin";
            this.textLeftLimitPickYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickYMax
            // 
            this.labelLeftLimitPickYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickYMax.AutoSize = true;
            this.labelLeftLimitPickYMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPickYMax.Name = "labelLeftLimitPickYMax";
            this.labelLeftLimitPickYMax.Text = "Y最大";
            // 
            // textLeftLimitPickYMax
            // 
            this.textLeftLimitPickYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickYMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPickYMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPickYMax.Name = "textLeftLimitPickYMax";
            this.textLeftLimitPickYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickZMin
            // 
            this.labelLeftLimitPickZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickZMin.AutoSize = true;
            this.labelLeftLimitPickZMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPickZMin.Name = "labelLeftLimitPickZMin";
            this.labelLeftLimitPickZMin.Text = "Z最小";
            // 
            // textLeftLimitPickZMin
            // 
            this.textLeftLimitPickZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickZMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPickZMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPickZMin.Name = "textLeftLimitPickZMin";
            this.textLeftLimitPickZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPickZMax
            // 
            this.labelLeftLimitPickZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPickZMax.AutoSize = true;
            this.labelLeftLimitPickZMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPickZMax.Name = "labelLeftLimitPickZMax";
            this.labelLeftLimitPickZMax.Text = "Z最大";
            // 
            // textLeftLimitPickZMax
            // 
            this.textLeftLimitPickZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPickZMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPickZMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPickZMax.Name = "textLeftLimitPickZMax";
            this.textLeftLimitPickZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPickZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // groupLeftLimitPlace
            // 
            this.groupLeftLimitPlace.Controls.Add(this.tableLeftLimitPlace);
            this.groupLeftLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupLeftLimitPlace.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.groupLeftLimitPlace.Name = "groupLeftLimitPlace";
            this.groupLeftLimitPlace.Padding = new System.Windows.Forms.Padding(10, 14, 10, 10);
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
            this.tableLeftLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLeftLimitPlace.Name = "tableLeftLimitPlace";
            this.tableLeftLimitPlace.RowCount = 3;
            this.tableLeftLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableLeftLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
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
            // 
            // labelLeftLimitPlaceXMin
            // 
            this.labelLeftLimitPlaceXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceXMin.AutoSize = true;
            this.labelLeftLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPlaceXMin.Name = "labelLeftLimitPlaceXMin";
            this.labelLeftLimitPlaceXMin.Text = "X最小";
            // 
            // textLeftLimitPlaceXMin
            // 
            this.textLeftLimitPlaceXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPlaceXMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPlaceXMin.Name = "textLeftLimitPlaceXMin";
            this.textLeftLimitPlaceXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceXMax
            // 
            this.labelLeftLimitPlaceXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceXMax.AutoSize = true;
            this.labelLeftLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPlaceXMax.Name = "labelLeftLimitPlaceXMax";
            this.labelLeftLimitPlaceXMax.Text = "X最大";
            // 
            // textLeftLimitPlaceXMax
            // 
            this.textLeftLimitPlaceXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPlaceXMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPlaceXMax.Name = "textLeftLimitPlaceXMax";
            this.textLeftLimitPlaceXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceYMin
            // 
            this.labelLeftLimitPlaceYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceYMin.AutoSize = true;
            this.labelLeftLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPlaceYMin.Name = "labelLeftLimitPlaceYMin";
            this.labelLeftLimitPlaceYMin.Text = "Y最小";
            // 
            // textLeftLimitPlaceYMin
            // 
            this.textLeftLimitPlaceYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPlaceYMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPlaceYMin.Name = "textLeftLimitPlaceYMin";
            this.textLeftLimitPlaceYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceYMax
            // 
            this.labelLeftLimitPlaceYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceYMax.AutoSize = true;
            this.labelLeftLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPlaceYMax.Name = "labelLeftLimitPlaceYMax";
            this.labelLeftLimitPlaceYMax.Text = "Y最大";
            // 
            // textLeftLimitPlaceYMax
            // 
            this.textLeftLimitPlaceYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPlaceYMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPlaceYMax.Name = "textLeftLimitPlaceYMax";
            this.textLeftLimitPlaceYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceZMin
            // 
            this.labelLeftLimitPlaceZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceZMin.AutoSize = true;
            this.labelLeftLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPlaceZMin.Name = "labelLeftLimitPlaceZMin";
            this.labelLeftLimitPlaceZMin.Text = "Z最小";
            // 
            // textLeftLimitPlaceZMin
            // 
            this.textLeftLimitPlaceZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPlaceZMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPlaceZMin.Name = "textLeftLimitPlaceZMin";
            this.textLeftLimitPlaceZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelLeftLimitPlaceZMax
            // 
            this.labelLeftLimitPlaceZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelLeftLimitPlaceZMax.AutoSize = true;
            this.labelLeftLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelLeftLimitPlaceZMax.Name = "labelLeftLimitPlaceZMax";
            this.labelLeftLimitPlaceZMax.Text = "Z最大";
            // 
            // textLeftLimitPlaceZMax
            // 
            this.textLeftLimitPlaceZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textLeftLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textLeftLimitPlaceZMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textLeftLimitPlaceZMax.Name = "textLeftLimitPlaceZMax";
            this.textLeftLimitPlaceZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textLeftLimitPlaceZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // tabPageRight
            // 
            this.tabPageRight.Controls.Add(this.panelRightScroll);
            this.tabPageRight.Name = "tabPageRight";
            this.tabPageRight.Padding = new System.Windows.Forms.Padding(4);
            this.tabPageRight.Text = "右机台";
            this.tabPageRight.UseVisualStyleBackColor = true;
            // 
            // panelRightScroll
            // 
            this.panelRightScroll.AutoScroll = true;
            this.panelRightScroll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.panelRightScroll.Controls.Add(this.tableRightRoot);
            this.panelRightScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightScroll.Name = "panelRightScroll";
            this.panelRightScroll.Padding = new System.Windows.Forms.Padding(12, 12, 28, 12);
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
            this.tableRightRoot.Name = "tableRightRoot";
            this.tableRightRoot.RowCount = 5;
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 152F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableRightRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 380F));

            // 
            // groupRightPick
            // 
            this.groupRightPick.Controls.Add(this.tableRightPick);
            this.groupRightPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPick.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPick.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupRightPick.Name = "groupRightPick";
            this.groupRightPick.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableRightPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightPick.Name = "tableRightPick";
            this.tableRightPick.RowCount = 2;
            this.tableRightPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightPick.Controls.Add(this.labelRightPickX, 0, 0);
            this.tableRightPick.Controls.Add(this.textRightPickX, 1, 0);
            this.tableRightPick.Controls.Add(this.labelRightPickY, 2, 0);
            this.tableRightPick.Controls.Add(this.textRightPickY, 3, 0);
            this.tableRightPick.Controls.Add(this.labelRightPickZ, 0, 1);
            this.tableRightPick.Controls.Add(this.textRightPickZ, 1, 1);
            this.tableRightPick.Controls.Add(this.labelRightPickRz, 2, 1);
            this.tableRightPick.Controls.Add(this.textRightPickRz, 3, 1);
            // 
            // labelRightPickX
            // 
            this.labelRightPickX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickX.AutoSize = true;
            this.labelRightPickX.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPickX.Name = "labelRightPickX";
            this.labelRightPickX.Text = "X:";
            this.labelRightPickX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickX
            // 
            this.textRightPickX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickX.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPickX.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPickX.Name = "textRightPickX";
            this.textRightPickX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPickY
            // 
            this.labelRightPickY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickY.AutoSize = true;
            this.labelRightPickY.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPickY.Name = "labelRightPickY";
            this.labelRightPickY.Text = "Y:";
            this.labelRightPickY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickY
            // 
            this.textRightPickY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickY.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPickY.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPickY.Name = "textRightPickY";
            this.textRightPickY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPickZ
            // 
            this.labelRightPickZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickZ.AutoSize = true;
            this.labelRightPickZ.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPickZ.Name = "labelRightPickZ";
            this.labelRightPickZ.Text = "Z:";
            this.labelRightPickZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickZ
            // 
            this.textRightPickZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickZ.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPickZ.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPickZ.Name = "textRightPickZ";
            this.textRightPickZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPickRz
            // 
            this.labelRightPickRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPickRz.AutoSize = true;
            this.labelRightPickRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPickRz.Name = "labelRightPickRz";
            this.labelRightPickRz.Text = "RZ:";
            this.labelRightPickRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPickRz
            // 
            this.textRightPickRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPickRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPickRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPickRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPickRz.Name = "textRightPickRz";
            this.textRightPickRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPickRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupRightPlace
            // 
            this.groupRightPlace.Controls.Add(this.tableRightPlace);
            this.groupRightPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPlace.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPlace.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupRightPlace.Name = "groupRightPlace";
            this.groupRightPlace.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableRightPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightPlace.Name = "tableRightPlace";
            this.tableRightPlace.RowCount = 2;
            this.tableRightPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightPlace.Controls.Add(this.labelRightPlaceX, 0, 0);
            this.tableRightPlace.Controls.Add(this.textRightPlaceX, 1, 0);
            this.tableRightPlace.Controls.Add(this.labelRightPlaceY, 2, 0);
            this.tableRightPlace.Controls.Add(this.textRightPlaceY, 3, 0);
            this.tableRightPlace.Controls.Add(this.labelRightPlaceZ, 0, 1);
            this.tableRightPlace.Controls.Add(this.textRightPlaceZ, 1, 1);
            this.tableRightPlace.Controls.Add(this.labelRightPlaceRz, 2, 1);
            this.tableRightPlace.Controls.Add(this.textRightPlaceRz, 3, 1);
            // 
            // labelRightPlaceX
            // 
            this.labelRightPlaceX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceX.AutoSize = true;
            this.labelRightPlaceX.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlaceX.Name = "labelRightPlaceX";
            this.labelRightPlaceX.Text = "X:";
            this.labelRightPlaceX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceX
            // 
            this.textRightPlaceX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceX.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlaceX.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlaceX.Name = "textRightPlaceX";
            this.textRightPlaceX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlaceY
            // 
            this.labelRightPlaceY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceY.AutoSize = true;
            this.labelRightPlaceY.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlaceY.Name = "labelRightPlaceY";
            this.labelRightPlaceY.Text = "Y:";
            this.labelRightPlaceY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceY
            // 
            this.textRightPlaceY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceY.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlaceY.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlaceY.Name = "textRightPlaceY";
            this.textRightPlaceY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlaceZ
            // 
            this.labelRightPlaceZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceZ.AutoSize = true;
            this.labelRightPlaceZ.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlaceZ.Name = "labelRightPlaceZ";
            this.labelRightPlaceZ.Text = "Z:";
            this.labelRightPlaceZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceZ
            // 
            this.textRightPlaceZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceZ.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlaceZ.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlaceZ.Name = "textRightPlaceZ";
            this.textRightPlaceZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlaceRz
            // 
            this.labelRightPlaceRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceRz.AutoSize = true;
            this.labelRightPlaceRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlaceRz.Name = "labelRightPlaceRz";
            this.labelRightPlaceRz.Text = "RZ:";
            this.labelRightPlaceRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlaceRz
            // 
            this.textRightPlaceRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlaceRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlaceRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlaceRz.Name = "textRightPlaceRz";
            this.textRightPlaceRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlaceRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupRightPlacePhoto
            // 
            this.groupRightPlacePhoto.Controls.Add(this.tableRightPlacePhoto);
            this.groupRightPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPlacePhoto.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPlacePhoto.Margin = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.groupRightPlacePhoto.Name = "groupRightPlacePhoto";
            this.groupRightPlacePhoto.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableRightPlacePhoto.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightPlacePhoto.Name = "tableRightPlacePhoto";
            this.tableRightPlacePhoto.RowCount = 2;
            this.tableRightPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightPlacePhoto.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoX, 0, 0);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoX, 1, 0);
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoY, 2, 0);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoY, 3, 0);
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoZ, 0, 1);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoZ, 1, 1);
            this.tableRightPlacePhoto.Controls.Add(this.labelRightPlacePhotoRz, 2, 1);
            this.tableRightPlacePhoto.Controls.Add(this.textRightPlacePhotoRz, 3, 1);
            // 
            // labelRightPlacePhotoX
            // 
            this.labelRightPlacePhotoX.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoX.AutoSize = true;
            this.labelRightPlacePhotoX.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlacePhotoX.Name = "labelRightPlacePhotoX";
            this.labelRightPlacePhotoX.Text = "X:";
            this.labelRightPlacePhotoX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoX
            // 
            this.textRightPlacePhotoX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoX.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoX.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlacePhotoX.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlacePhotoX.Name = "textRightPlacePhotoX";
            this.textRightPlacePhotoX.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoX.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlacePhotoY
            // 
            this.labelRightPlacePhotoY.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoY.AutoSize = true;
            this.labelRightPlacePhotoY.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlacePhotoY.Name = "labelRightPlacePhotoY";
            this.labelRightPlacePhotoY.Text = "Y:";
            this.labelRightPlacePhotoY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoY
            // 
            this.textRightPlacePhotoY.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoY.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoY.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlacePhotoY.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlacePhotoY.Name = "textRightPlacePhotoY";
            this.textRightPlacePhotoY.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoY.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlacePhotoZ
            // 
            this.labelRightPlacePhotoZ.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoZ.AutoSize = true;
            this.labelRightPlacePhotoZ.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlacePhotoZ.Name = "labelRightPlacePhotoZ";
            this.labelRightPlacePhotoZ.Text = "Z:";
            this.labelRightPlacePhotoZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoZ
            // 
            this.textRightPlacePhotoZ.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoZ.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoZ.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlacePhotoZ.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlacePhotoZ.Name = "textRightPlacePhotoZ";
            this.textRightPlacePhotoZ.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoZ.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // labelRightPlacePhotoRz
            // 
            this.labelRightPlacePhotoRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlacePhotoRz.AutoSize = true;
            this.labelRightPlacePhotoRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlacePhotoRz.Name = "labelRightPlacePhotoRz";
            this.labelRightPlacePhotoRz.Text = "RZ:";
            this.labelRightPlacePhotoRz.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textRightPlacePhotoRz
            // 
            this.textRightPlacePhotoRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlacePhotoRz.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.textRightPlacePhotoRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlacePhotoRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlacePhotoRz.Name = "textRightPlacePhotoRz";
            this.textRightPlacePhotoRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            this.textRightPlacePhotoRz.Leave += new System.EventHandler(this.PositionField_Leave);
            // 
            // groupRightPlaceCenter
            // 
            this.groupRightPlaceCenter.Controls.Add(this.tableRightPlaceCenter);
            this.groupRightPlaceCenter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightPlaceCenter.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightPlaceCenter.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.groupRightPlaceCenter.Name = "groupRightPlaceCenter";
            this.groupRightPlaceCenter.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableRightPlaceCenter.Name = "tableRightPlaceCenter";
            this.tableRightPlaceCenter.RowCount = 1;
            this.tableRightPlaceCenter.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            // 
            // labelRightPlaceCenterRz
            // 
            this.labelRightPlaceCenterRz.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightPlaceCenterRz.AutoSize = true;
            this.labelRightPlaceCenterRz.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightPlaceCenterRz.Name = "labelRightPlaceCenterRz";
            this.labelRightPlaceCenterRz.Text = "RZ:";
            // 
            // textRightPlaceCenterRz
            // 
            this.textRightPlaceCenterRz.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightPlaceCenterRz.Margin = new System.Windows.Forms.Padding(0, 4, 10, 4);
            this.textRightPlaceCenterRz.MinimumSize = new System.Drawing.Size(80, 30);
            this.textRightPlaceCenterRz.Name = "textRightPlaceCenterRz";
            this.textRightPlaceCenterRz.TextChanged += new System.EventHandler(this.PositionField_Changed);
            // 
            // groupRightLimit
            // 
            this.groupRightLimit.Controls.Add(this.tableRightLimitRoot);
            this.groupRightLimit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightLimit.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.groupRightLimit.Margin = new System.Windows.Forms.Padding(0, 4, 0, 8);
            this.groupRightLimit.Name = "groupRightLimit";
            this.groupRightLimit.Padding = new System.Windows.Forms.Padding(14, 20, 14, 12);
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
            this.tableRightLimitRoot.Name = "tableRightLimitRoot";
            this.tableRightLimitRoot.RowCount = 3;
            this.tableRightLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableRightLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableRightLimitRoot.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 230F));
            // 
            // labelRightLimitHint
            // 
            this.labelRightLimitHint.AutoSize = true;
            this.labelRightLimitHint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelRightLimitHint.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelRightLimitHint.Margin = new System.Windows.Forms.Padding(4, 2, 4, 8);
            this.labelRightLimitHint.MaximumSize = new System.Drawing.Size(860, 0);
            this.labelRightLimitHint.Name = "labelRightLimitHint";
            this.labelRightLimitHint.Text = "本机台取料/放料坐标发送前按此范围校验。输入值受「安全区域」约束；更改该限制需点「安全区域」登录。";
            // 
            // checkRightLimitEnabled
            // 
            this.checkRightLimitEnabled.AutoSize = true;
            this.checkRightLimitEnabled.Margin = new System.Windows.Forms.Padding(8, 2, 4, 10);
            this.checkRightLimitEnabled.Name = "checkRightLimitEnabled";
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
            this.tableRightLimitRanges.Name = "tableRightLimitRanges";
            this.tableRightLimitRanges.RowCount = 1;
            this.tableRightLimitRanges.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            // 
            // groupRightLimitPick
            // 
            this.groupRightLimitPick.Controls.Add(this.tableRightLimitPick);
            this.groupRightLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightLimitPick.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.groupRightLimitPick.Name = "groupRightLimitPick";
            this.groupRightLimitPick.Padding = new System.Windows.Forms.Padding(10, 14, 10, 10);
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
            this.tableRightLimitPick.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightLimitPick.Name = "tableRightLimitPick";
            this.tableRightLimitPick.RowCount = 3;
            this.tableRightLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightLimitPick.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
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
            // 
            // labelRightLimitPickXMin
            // 
            this.labelRightLimitPickXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickXMin.AutoSize = true;
            this.labelRightLimitPickXMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPickXMin.Name = "labelRightLimitPickXMin";
            this.labelRightLimitPickXMin.Text = "X最小";
            // 
            // textRightLimitPickXMin
            // 
            this.textRightLimitPickXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickXMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPickXMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPickXMin.Name = "textRightLimitPickXMin";
            this.textRightLimitPickXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickXMax
            // 
            this.labelRightLimitPickXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickXMax.AutoSize = true;
            this.labelRightLimitPickXMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPickXMax.Name = "labelRightLimitPickXMax";
            this.labelRightLimitPickXMax.Text = "X最大";
            // 
            // textRightLimitPickXMax
            // 
            this.textRightLimitPickXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickXMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPickXMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPickXMax.Name = "textRightLimitPickXMax";
            this.textRightLimitPickXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickYMin
            // 
            this.labelRightLimitPickYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickYMin.AutoSize = true;
            this.labelRightLimitPickYMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPickYMin.Name = "labelRightLimitPickYMin";
            this.labelRightLimitPickYMin.Text = "Y最小";
            // 
            // textRightLimitPickYMin
            // 
            this.textRightLimitPickYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickYMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPickYMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPickYMin.Name = "textRightLimitPickYMin";
            this.textRightLimitPickYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickYMax
            // 
            this.labelRightLimitPickYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickYMax.AutoSize = true;
            this.labelRightLimitPickYMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPickYMax.Name = "labelRightLimitPickYMax";
            this.labelRightLimitPickYMax.Text = "Y最大";
            // 
            // textRightLimitPickYMax
            // 
            this.textRightLimitPickYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickYMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPickYMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPickYMax.Name = "textRightLimitPickYMax";
            this.textRightLimitPickYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickZMin
            // 
            this.labelRightLimitPickZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickZMin.AutoSize = true;
            this.labelRightLimitPickZMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPickZMin.Name = "labelRightLimitPickZMin";
            this.labelRightLimitPickZMin.Text = "Z最小";
            // 
            // textRightLimitPickZMin
            // 
            this.textRightLimitPickZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickZMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPickZMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPickZMin.Name = "textRightLimitPickZMin";
            this.textRightLimitPickZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPickZMax
            // 
            this.labelRightLimitPickZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPickZMax.AutoSize = true;
            this.labelRightLimitPickZMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPickZMax.Name = "labelRightLimitPickZMax";
            this.labelRightLimitPickZMax.Text = "Z最大";
            // 
            // textRightLimitPickZMax
            // 
            this.textRightLimitPickZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPickZMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPickZMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPickZMax.Name = "textRightLimitPickZMax";
            this.textRightLimitPickZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPickZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // groupRightLimitPlace
            // 
            this.groupRightLimitPlace.Controls.Add(this.tableRightLimitPlace);
            this.groupRightLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupRightLimitPlace.Margin = new System.Windows.Forms.Padding(4, 2, 4, 2);
            this.groupRightLimitPlace.Name = "groupRightLimitPlace";
            this.groupRightLimitPlace.Padding = new System.Windows.Forms.Padding(10, 14, 10, 10);
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
            this.tableRightLimitPlace.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableRightLimitPlace.Name = "tableRightLimitPlace";
            this.tableRightLimitPlace.RowCount = 3;
            this.tableRightLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
            this.tableRightLimitPlace.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 46F));
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
            // 
            // labelRightLimitPlaceXMin
            // 
            this.labelRightLimitPlaceXMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceXMin.AutoSize = true;
            this.labelRightLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPlaceXMin.Name = "labelRightLimitPlaceXMin";
            this.labelRightLimitPlaceXMin.Text = "X最小";
            // 
            // textRightLimitPlaceXMin
            // 
            this.textRightLimitPlaceXMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceXMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPlaceXMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPlaceXMin.Name = "textRightLimitPlaceXMin";
            this.textRightLimitPlaceXMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceXMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceXMax
            // 
            this.labelRightLimitPlaceXMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceXMax.AutoSize = true;
            this.labelRightLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPlaceXMax.Name = "labelRightLimitPlaceXMax";
            this.labelRightLimitPlaceXMax.Text = "X最大";
            // 
            // textRightLimitPlaceXMax
            // 
            this.textRightLimitPlaceXMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceXMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPlaceXMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPlaceXMax.Name = "textRightLimitPlaceXMax";
            this.textRightLimitPlaceXMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceXMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceYMin
            // 
            this.labelRightLimitPlaceYMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceYMin.AutoSize = true;
            this.labelRightLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPlaceYMin.Name = "labelRightLimitPlaceYMin";
            this.labelRightLimitPlaceYMin.Text = "Y最小";
            // 
            // textRightLimitPlaceYMin
            // 
            this.textRightLimitPlaceYMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceYMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPlaceYMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPlaceYMin.Name = "textRightLimitPlaceYMin";
            this.textRightLimitPlaceYMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceYMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceYMax
            // 
            this.labelRightLimitPlaceYMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceYMax.AutoSize = true;
            this.labelRightLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPlaceYMax.Name = "labelRightLimitPlaceYMax";
            this.labelRightLimitPlaceYMax.Text = "Y最大";
            // 
            // textRightLimitPlaceYMax
            // 
            this.textRightLimitPlaceYMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceYMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPlaceYMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPlaceYMax.Name = "textRightLimitPlaceYMax";
            this.textRightLimitPlaceYMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceYMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceZMin
            // 
            this.labelRightLimitPlaceZMin.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceZMin.AutoSize = true;
            this.labelRightLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPlaceZMin.Name = "labelRightLimitPlaceZMin";
            this.labelRightLimitPlaceZMin.Text = "Z最小";
            // 
            // textRightLimitPlaceZMin
            // 
            this.textRightLimitPlaceZMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceZMin.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPlaceZMin.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPlaceZMin.Name = "textRightLimitPlaceZMin";
            this.textRightLimitPlaceZMin.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceZMin.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // labelRightLimitPlaceZMax
            // 
            this.labelRightLimitPlaceZMax.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.labelRightLimitPlaceZMax.AutoSize = true;
            this.labelRightLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(4, 10, 8, 0);
            this.labelRightLimitPlaceZMax.Name = "labelRightLimitPlaceZMax";
            this.labelRightLimitPlaceZMax.Text = "Z最大";
            // 
            // textRightLimitPlaceZMax
            // 
            this.textRightLimitPlaceZMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textRightLimitPlaceZMax.Margin = new System.Windows.Forms.Padding(0, 4, 8, 4);
            this.textRightLimitPlaceZMax.MinimumSize = new System.Drawing.Size(72, 30);
            this.textRightLimitPlaceZMax.Name = "textRightLimitPlaceZMax";
            this.textRightLimitPlaceZMax.TextChanged += new System.EventHandler(this.LimitField_Changed);
            this.textRightLimitPlaceZMax.Leave += new System.EventHandler(this.LimitField_Leave);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.flowButtonsRight);
            this.panelButtons.Controls.Add(this.buttonFromReco);
            this.panelButtons.Controls.Add(this.buttonFromRecoTab);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButtons.Name = "panelButtons";
            // 
            // flowButtonsRight
            // 
            this.flowButtonsRight.Controls.Add(this.buttonSafetyZone);
            this.flowButtonsRight.Controls.Add(this.buttonSave);
            this.flowButtonsRight.Controls.Add(this.buttonCancel);
            this.flowButtonsRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.flowButtonsRight.Name = "flowButtonsRight";
            this.flowButtonsRight.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            this.flowButtonsRight.WrapContents = false;
            // 
            // buttonSafetyZone
            // 
            this.buttonSafetyZone.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.buttonSafetyZone.Name = "buttonSafetyZone";
            this.buttonSafetyZone.Size = new System.Drawing.Size(120, 44);
            this.buttonSafetyZone.Text = "安全区域";
            this.buttonSafetyZone.UseVisualStyleBackColor = true;
            this.buttonSafetyZone.Click += new System.EventHandler(this.buttonSafetyZone_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Margin = new System.Windows.Forms.Padding(0, 0, 8, 0);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(100, 44);
            this.buttonSave.Text = "保存";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Margin = new System.Windows.Forms.Padding(0);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(100, 44);
            this.buttonCancel.Text = "取消";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonFromReco
            // 
            this.buttonFromReco.Location = new System.Drawing.Point(0, 8);
            this.buttonFromReco.Name = "buttonFromReco";
            this.buttonFromReco.Size = new System.Drawing.Size(160, 44);
            this.buttonFromReco.Text = "全部带入识别XY";
            this.buttonFromReco.UseVisualStyleBackColor = true;
            this.buttonFromReco.Click += new System.EventHandler(this.buttonFromReco_Click);
            // 
            // buttonFromRecoTab
            // 
            this.buttonFromRecoTab.Location = new System.Drawing.Point(168, 8);
            this.buttonFromRecoTab.Name = "buttonFromRecoTab";
            this.buttonFromRecoTab.Size = new System.Drawing.Size(160, 44);
            this.buttonFromRecoTab.Text = "本页带入识别XY";
            this.buttonFromRecoTab.UseVisualStyleBackColor = true;
            this.buttonFromRecoTab.Click += new System.EventHandler(this.buttonFromRecoTab_Click);
            // 
            // PhotoPositionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(960, 960);
            this.Controls.Add(this.tableLayoutMain);
            this.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(880, 720);
            this.Name = "PhotoPositionsForm";
            this.Padding = new System.Windows.Forms.Padding(16);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "位置设定";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PhotoPositionsForm_FormClosing);
            this.Load += new System.EventHandler(this.PhotoPositionsForm_Load);

            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPageLeft.ResumeLayout(false);
            this.tabPageRight.ResumeLayout(false);
            this.panelLeftScroll.ResumeLayout(false);
            this.tableLeftRoot.ResumeLayout(false);
            this.tableLeftRoot.PerformLayout();
            this.groupLeftPick.ResumeLayout(false);
            this.groupLeftPick.PerformLayout();
            this.tableLeftPick.ResumeLayout(false);
            this.tableLeftPick.PerformLayout();
            this.groupLeftPlace.ResumeLayout(false);
            this.groupLeftPlace.PerformLayout();
            this.tableLeftPlace.ResumeLayout(false);
            this.tableLeftPlace.PerformLayout();
            this.groupLeftPlacePhoto.ResumeLayout(false);
            this.groupLeftPlacePhoto.PerformLayout();
            this.tableLeftPlacePhoto.ResumeLayout(false);
            this.tableLeftPlacePhoto.PerformLayout();
            this.groupLeftPlaceCenter.ResumeLayout(false);
            this.groupLeftPlaceCenter.PerformLayout();
            this.tableLeftPlaceCenter.ResumeLayout(false);
            this.tableLeftPlaceCenter.PerformLayout();
            this.groupLeftLimit.ResumeLayout(false);
            this.groupLeftLimit.PerformLayout();
            this.tableLeftLimitRoot.ResumeLayout(false);
            this.tableLeftLimitRoot.PerformLayout();
            this.tableLeftLimitRanges.ResumeLayout(false);
            this.tableLeftLimitRanges.PerformLayout();
            this.groupLeftLimitPick.ResumeLayout(false);
            this.groupLeftLimitPick.PerformLayout();
            this.tableLeftLimitPick.ResumeLayout(false);
            this.tableLeftLimitPick.PerformLayout();
            this.groupLeftLimitPlace.ResumeLayout(false);
            this.groupLeftLimitPlace.PerformLayout();
            this.tableLeftLimitPlace.ResumeLayout(false);
            this.tableLeftLimitPlace.PerformLayout();
            this.panelRightScroll.ResumeLayout(false);
            this.tableRightRoot.ResumeLayout(false);
            this.tableRightRoot.PerformLayout();
            this.groupRightPick.ResumeLayout(false);
            this.groupRightPick.PerformLayout();
            this.tableRightPick.ResumeLayout(false);
            this.tableRightPick.PerformLayout();
            this.groupRightPlace.ResumeLayout(false);
            this.groupRightPlace.PerformLayout();
            this.tableRightPlace.ResumeLayout(false);
            this.tableRightPlace.PerformLayout();
            this.groupRightPlacePhoto.ResumeLayout(false);
            this.groupRightPlacePhoto.PerformLayout();
            this.tableRightPlacePhoto.ResumeLayout(false);
            this.tableRightPlacePhoto.PerformLayout();
            this.groupRightPlaceCenter.ResumeLayout(false);
            this.groupRightPlaceCenter.PerformLayout();
            this.tableRightPlaceCenter.ResumeLayout(false);
            this.tableRightPlaceCenter.PerformLayout();
            this.groupRightLimit.ResumeLayout(false);
            this.groupRightLimit.PerformLayout();
            this.tableRightLimitRoot.ResumeLayout(false);
            this.tableRightLimitRoot.PerformLayout();
            this.tableRightLimitRanges.ResumeLayout(false);
            this.tableRightLimitRanges.PerformLayout();
            this.groupRightLimitPick.ResumeLayout(false);
            this.groupRightLimitPick.PerformLayout();
            this.tableRightLimitPick.ResumeLayout(false);
            this.tableRightLimitPick.PerformLayout();
            this.groupRightLimitPlace.ResumeLayout(false);
            this.groupRightLimitPlace.PerformLayout();
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
