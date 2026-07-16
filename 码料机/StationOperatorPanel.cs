using System;
using System.Drawing;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 左/右机台操作区用户控件：产品/排料/箱体、规格条、取放数量、料道缓存、换框、确认。
    /// <para>布局请在 Visual Studio 设计器中拖拽维护（.Designer.cs / .resx）；</para>
    /// <para>Form1 通过 <c>MountStationOperatorPanel</c> 挂入 GroupBox，并把原 comboBox/button 字段指向本面板控件。</para>
    /// </summary>
    public partial class StationOperatorPanel : UserControl
    {
        public ComboBox ComboProduct => comboProduct;
        public ComboBox ComboStackMode => comboStackMode;
        public ComboBox ComboBoxSpec => comboBoxSpec;
        public Label LblProductSpec => lblProductSpec;
        public Label LblBoxSpec => lblBoxSpec;
        public Label LblPickQty => lblPickQty;
        public TextBox TxtPickQty => txtPickQty;
        public Label LblPlaceQty => lblPlaceQty;
        public TextBox TxtPlaceQty => txtPlaceQty;
        public TextBox TxtTrackBuffer => txtTrackBuffer;
        public Button BtnSaveTrackBuffer => btnSaveTrackBuffer;
        public Button BtnFrameChange => btnFrameChange;
        public Button BtnFrameComplete => btnFrameComplete;
        public Label LblFrameAllow => lblFrameAllow;
        public CheckBox ChkUseConfiguredPlace => chkUseConfiguredPlace;
        public CheckBox ChkManualSlotSelect => chkManualSlotSelect;
        public Button BtnConfirm => btnConfirm;

        public event EventHandler SaveTrackBufferClick;
        public event EventHandler ConfirmClick;

        public StationOperatorPanel()
        {
            InitializeComponent();
            scrollHost.Resize += (_, __) => SyncScrollContentWidth();
            Load += (_, __) => SyncScrollContentWidth();
        }

        private void SyncScrollContentWidth()
        {
            if (scrollHost == null || tableMain == null || scrollHost.IsDisposed) return;
            int w = scrollHost.ClientSize.Width - scrollHost.Padding.Horizontal;
            if (w > 80 && tableMain.Width != w)
                tableMain.Width = w;
        }

        /// <summary>按左右工位设置文案与换框按钮 Tag（PLC 位索引）。</summary>
        public void ConfigureSide(bool isLeft)
        {
            string side = isLeft ? "左" : "右";
            chkUseConfiguredPlace.Text = $"{side}机台放料用手动设定位置（不用识箱算位）";
            chkManualSlotSelect.Text = $"{side}机台手动指定放料位（算法识位，界面选下一发）";
            btnFrameChange.Tag = isLeft ? PlcFrameChangeBits.A换框按钮 : PlcFrameChangeBits.B换框按钮;
            btnFrameComplete.Tag = isLeft ? PlcFrameChangeBits.A换框完成按钮 : PlcFrameChangeBits.B换框完成按钮;
            StyleFrameButton(btnFrameChange, false);
            StyleFrameButton(btnFrameComplete, false);
        }

        public void ApplyComboStyle()
        {
            StyleCombo(comboProduct);
            StyleCombo(comboStackMode);
            StyleCombo(comboBoxSpec);
        }

        private static void StyleCombo(ComboBox cb)
        {
            if (cb == null) return;
            cb.DropDownStyle = ComboBoxStyle.DropDownList;
            cb.FlatStyle = FlatStyle.System;
            cb.BackColor = Color.White;
            cb.Font = UiLayoutHelper.Combo;
            cb.MinimumSize = new Size(0, 40);
            cb.IntegralHeight = false;
            cb.MaxDropDownItems = 14;
            cb.DropDownHeight = 280;
        }

        private static void StyleFrameButton(Button btn, bool on)
        {
            if (btn == null) return;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = Cursors.Hand;
            btn.ForeColor = Color.White;
            btn.BackColor = on
                ? Color.FromArgb(22, 163, 74)
                : Color.FromArgb(226, 232, 240);
            if (!on) btn.ForeColor = Color.FromArgb(51, 65, 85);
        }

        private void btnSaveTrackBuffer_Click(object sender, EventArgs e) =>
            SaveTrackBufferClick?.Invoke(this, EventArgs.Empty);

        private void btnConfirm_Click(object sender, EventArgs e) =>
            ConfirmClick?.Invoke(this, EventArgs.Empty);
    }
}
