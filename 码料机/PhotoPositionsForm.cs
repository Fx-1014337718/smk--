using System;
using System.Drawing;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 左/右机台各自独立：点位 + 限位报警参数。布局见 PhotoPositionsForm.Designer.cs。
    /// </summary>
    public partial class PhotoPositionsForm : Form
    {
        public Form1 MainForm;
        private bool _dirty;
        private readonly string _iniPath;

        public PhotoPositionsForm(Form1 main)
        {
            MainForm = main;
            _iniPath = main.pathPhotoPos;
            InitializeComponent();
            MaximizeBox = true;
            UiLayoutHelper.ApplyDialogChrome(this);
            WireScrollContentWidth(panelLeftScroll, tableLeftRoot);
            WireScrollContentWidth(panelRightScroll, tableRightRoot);
        }

        private static void WireScrollContentWidth(Panel scroll, Control content)
        {
            void Sync()
            {
                if (scroll.IsDisposed || content.IsDisposed) return;
                int w = scroll.ClientSize.Width - scroll.Padding.Horizontal;
                if (w > 40 && content.Width != w)
                    content.Width = w;
            }
            scroll.SizeChanged += (_, __) => Sync();
            scroll.HandleCreated += (_, __) => Sync();
            content.HandleCreated += (_, __) => Sync();
        }

        private bool IsLeftTab => tabControl.SelectedTab == tabPageLeft;

        private void PhotoPositionsForm_Load(object sender, EventArgs e)
        {
            PhotoPositionConfig.EnsureIniFile();
            PhotoPositionConfig.LoadBoth(_iniPath, out var left, out var right);
            LoadPositionToUi(true, left);
            LoadPositionToUi(false, right);
            AlarmPositionLimitConfig.LoadBoth(_iniPath, out var limitLeft, out var limitRight);
            LoadLimitToUi(true, limitLeft);
            LoadLimitToUi(false, limitRight);
            ApplyRecognitionToEmptyXY(true);
            ApplyRecognitionToEmptyXY(false);
            _dirty = false;
        }

        private void PositionField_Changed(object sender, EventArgs e) => _dirty = true;

        private void PositionField_Leave(object sender, EventArgs e)
        {
            if (!(sender is TextBox tb) || string.IsNullOrWhiteSpace(tb.Text)) return;
            if (!double.TryParse(tb.Text.Trim(), out double value)) return;

            bool isLeft = tb.Name.StartsWith("textLeft", StringComparison.Ordinal);
            bool isPick = tb.Name.IndexOf("Pick", StringComparison.Ordinal) >= 0
                          && tb.Name.IndexOf("Place", StringComparison.Ordinal) < 0;
            // textLeftPickX / textLeftPlaceX / textLeftPlacePhotoX — PlacePhoto 不按放料限位校验
            if (tb.Name.IndexOf("PlacePhoto", StringComparison.Ordinal) >= 0
                || tb.Name.IndexOf("PlaceCenter", StringComparison.Ordinal) >= 0
                || tb.Name.IndexOf("Rz", StringComparison.Ordinal) >= 0
                || tb.Name.EndsWith("Rz", StringComparison.Ordinal))
                return;

            char axis = '?';
            if (tb.Name.EndsWith("X", StringComparison.Ordinal)) axis = 'X';
            else if (tb.Name.EndsWith("Y", StringComparison.Ordinal)) axis = 'Y';
            else if (tb.Name.EndsWith("Z", StringComparison.Ordinal)) axis = 'Z';
            else return;

            // Place vs Pick: textLeftPlaceX contains Place
            if (tb.Name.IndexOf("Place", StringComparison.Ordinal) >= 0)
                isPick = false;

            var safety = PeekLimitFromUi(isLeft) ?? MainForm?.GetAlarmPositionLimits(isLeft);
            if (safety == null || !safety.Enabled) return;
            if (!safety.IsAxisValueOutOfLimit(isPick, axis, value, out string detail)) return;

            tb.Text = "";
            DialogPrompts.ShowWarning((isPick ? "取料" : "放料") + "坐标超出安全区域：\n" + detail + "\n已清除该输入。");
        }

        private void buttonSafetyZone_Click(object sender, EventArgs e)
            => SafetyZoneSettingsForm.ShowAuthenticated(this, MainForm);

        private void ApplyRecognitionToEmptyXY(bool isLeft)
        {
            if (MainForm == null) return;
            if (IsPickXYEmpty(isLeft) && MainForm.TryGetRecognizedPickPhotoXY(isLeft, out double px, out double py))
                SetPickXY(isLeft, px, py);
            if (IsPlacePhotoXYEmpty(isLeft) && MainForm.TryGetRecognizedPlacePhotoXY(isLeft, out double bx, out double by))
                SetPlacePhotoXY(isLeft, bx, by);
        }

        private void buttonFromReco_Click(object sender, EventArgs e)
        {
            if (MainForm == null) return;
            bool any = false;
            if (MainForm.TryGetRecognizedPickPhotoXY(true, out double plx, out double ply))
            { SetPickXY(true, plx, ply); any = true; }
            if (MainForm.TryGetRecognizedPlacePhotoXY(true, out double blx, out double bly))
            { SetPlacePhotoXY(true, blx, bly); any = true; }
            if (MainForm.TryGetRecognizedPickPhotoXY(false, out double prx, out double pry))
            { SetPickXY(false, prx, pry); any = true; }
            if (MainForm.TryGetRecognizedPlacePhotoXY(false, out double brx, out double bry))
            { SetPlacePhotoXY(false, brx, bry); any = true; }
            if (!any)
                DialogPrompts.ShowInfo("当前没有可用的算法识别 X、Y 值。\n请先运行视觉方案或金沃算位。", "提示");
            else
                _dirty = true;
        }

        private void buttonFromRecoTab_Click(object sender, EventArgs e)
        {
            if (MainForm == null) return;
            bool isLeft = IsLeftTab;
            bool any = false;
            if (MainForm.TryGetRecognizedPickPhotoXY(isLeft, out double px, out double py))
            { SetPickXY(isLeft, px, py); any = true; }
            if (MainForm.TryGetRecognizedPlacePhotoXY(isLeft, out double bx, out double by))
            { SetPlacePhotoXY(isLeft, bx, by); any = true; }
            if (!any)
                DialogPrompts.ShowInfo($"「{(isLeft ? "左" : "右")}机台」暂无识别 X、Y。", "提示");
            else
                _dirty = true;
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (!TryBuildConfigs(out var left, out var right)) return;
            if (!TryReadLimitFromUi(true, "左机台", out var limitLeft) || !TryReadLimitFromUi(false, "右机台", out var limitRight))
                return;
            if (!PhotoPositionConfig.SaveBoth(left, right, _iniPath))
            {
                DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                return;
            }
            if (!AlarmPositionLimitConfig.SaveBoth(limitLeft, limitRight, _iniPath))
            {
                DialogPrompts.ShowError("写入限位报警参数失败，请检查写入权限。");
                return;
            }
            _dirty = false;
            DialogPrompts.ShowInfo("左/右机台位置与限位报警参数已分别保存。", "保存成功");
            MainForm?.ReloadPhotoPositionConfig(pushToPlc: true);
        }

        private void buttonCancel_Click(object sender, EventArgs e) => TryClose();

        private void PhotoPositionsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel) return;
            if (!TryHandleUnsavedClose()) e.Cancel = true;
        }

        private void TryClose()
        {
            if (TryHandleUnsavedClose()) Close();
        }

        private bool TryHandleUnsavedClose()
        {
            if (!_dirty) return true;
            switch (DialogPrompts.AskUnsavedClose("位置设定"))
            {
                case DialogPrompts.UnsavedCloseAction.Save:
                    if (!TryBuildConfigs(out var left, out var right)) return false;
                    if (!TryReadLimitFromUi(true, "左机台", out var limitLeft) || !TryReadLimitFromUi(false, "右机台", out var limitRight))
                        return false;
                    if (!PhotoPositionConfig.SaveBoth(left, right, _iniPath)) return false;
                    if (!AlarmPositionLimitConfig.SaveBoth(limitLeft, limitRight, _iniPath)) return false;
                    MainForm?.ReloadPhotoPositionConfig(pushToPlc: true);
                    _dirty = false;
                    return true;
                case DialogPrompts.UnsavedCloseAction.Discard:
                    return true;
                default:
                    return false;
            }
        }

        private bool TryBuildConfigs(out PhotoPositionConfig left, out PhotoPositionConfig right)
        {
            left = right = null;
            var leftLimit = PeekLimitFromUi(true) ?? MainForm?.GetAlarmPositionLimits(true);
            var rightLimit = PeekLimitFromUi(false) ?? MainForm?.GetAlarmPositionLimits(false);
            if (!TryReadPositionFromUi(true, "左机台", leftLimit, out left)) return false;
            if (!TryReadPositionFromUi(false, "右机台", rightLimit, out right)) return false;
            return true;
        }
    }
}
