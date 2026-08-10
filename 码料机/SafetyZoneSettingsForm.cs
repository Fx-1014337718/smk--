using System;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 安全区域边界设定（需 admin）：约束位置设定页「限位报警参数」的可输入范围。
    /// 布局见 SafetyZoneSettingsForm.Designer.cs。
    /// </summary>
    public partial class SafetyZoneSettingsForm : Form
    {
        private readonly string _iniPath;
        private bool _dirty;
        private bool _loading;

        public Form1 MainForm { get; set; }

        public SafetyZoneSettingsForm(Form1 main)
        {
            MainForm = main;
            _iniPath = main?.pathPhotoPos ?? PhotoPositionConfig.IniFile;
            InitializeComponent();
            UiLayoutHelper.ApplyDialogChrome(this);
        }

        private void SafetyZoneSettingsForm_Load(object sender, EventArgs e)
        {
            _loading = true;
            try
            {
                AlarmPositionLimitConfig.LoadEnvelopes(_iniPath, out var left, out var right);
                LoadStation(true, left);
                LoadStation(false, right);
                _dirty = false;
            }
            finally
            {
                _loading = false;
            }
        }

        private void AnyValueChanged(object sender, EventArgs e)
        {
            if (!_loading)
                _dirty = true;
        }

        private void buttonSave_Click(object sender, EventArgs e) => SaveAndClose();

        private void buttonCancel_Click(object sender, EventArgs e) => Close();

        private void SafetyZoneSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel || !_dirty) return;
            switch (DialogPrompts.AskUnsavedClose("安全区域设定"))
            {
                case DialogPrompts.UnsavedCloseAction.Save:
                    if (!TrySave()) e.Cancel = true;
                    break;
                case DialogPrompts.UnsavedCloseAction.Discard:
                    break;
                default:
                    e.Cancel = true;
                    break;
            }
        }

        void SaveAndClose()
        {
            if (!TrySave()) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        bool TrySave()
        {
            if (!TryReadStation(true, "左机台", out var left) || !TryReadStation(false, "右机台", out var right))
                return false;
            if (!AlarmPositionLimitConfig.SaveEnvelopes(left, right, _iniPath))
            {
                DialogPrompts.ShowError("写入安全区域失败，请检查写入权限。");
                return false;
            }
            MainForm?.ReloadPhotoPositionConfig(pushToPlc: false);
            _dirty = false;
            DialogPrompts.ShowInfo("安全区域已保存。之后在「限位报警参数」中输入的值必须落在此范围内。", "保存成功");
            return true;
        }

        void LoadStation(bool isLeft, AlarmPositionLimitConfig c)
        {
            if (isLeft)
            {
                checkLeftEnabled.Checked = c?.Enabled == true;
                LoadRange(c?.Pick, textLeftPickXMin, textLeftPickXMax, textLeftPickYMin, textLeftPickYMax, textLeftPickZMin, textLeftPickZMax);
                LoadRange(c?.Place, textLeftPlaceXMin, textLeftPlaceXMax, textLeftPlaceYMin, textLeftPlaceYMax, textLeftPlaceZMin, textLeftPlaceZMax);
            }
            else
            {
                checkRightEnabled.Checked = c?.Enabled == true;
                LoadRange(c?.Pick, textRightPickXMin, textRightPickXMax, textRightPickYMin, textRightPickYMax, textRightPickZMin, textRightPickZMax);
                LoadRange(c?.Place, textRightPlaceXMin, textRightPlaceXMax, textRightPlaceYMin, textRightPlaceYMax, textRightPlaceZMin, textRightPlaceZMax);
            }
        }

        bool TryReadStation(bool isLeft, string stationName, out AlarmPositionLimitConfig c)
        {
            c = new AlarmPositionLimitConfig
            {
                Enabled = isLeft ? checkLeftEnabled.Checked : checkRightEnabled.Checked,
            };
            if (isLeft)
            {
                if (!TryReadRange($"{stationName} 取料", out var pick,
                        textLeftPickXMin, textLeftPickXMax, textLeftPickYMin, textLeftPickYMax, textLeftPickZMin, textLeftPickZMax))
                    return false;
                if (!TryReadRange($"{stationName} 放料", out var place,
                        textLeftPlaceXMin, textLeftPlaceXMax, textLeftPlaceYMin, textLeftPlaceYMax, textLeftPlaceZMin, textLeftPlaceZMax))
                    return false;
                c.Pick = pick;
                c.Place = place;
            }
            else
            {
                if (!TryReadRange($"{stationName} 取料", out var pick,
                        textRightPickXMin, textRightPickXMax, textRightPickYMin, textRightPickYMax, textRightPickZMin, textRightPickZMax))
                    return false;
                if (!TryReadRange($"{stationName} 放料", out var place,
                        textRightPlaceXMin, textRightPlaceXMax, textRightPlaceYMin, textRightPlaceYMax, textRightPlaceZMin, textRightPlaceZMax))
                    return false;
                c.Pick = pick;
                c.Place = place;
            }

            if (c.Enabled && !c.HasAnyAxisLimit())
            {
                    DialogPrompts.ShowWarning($"「{stationName}」已启用输入限制，请至少为取料或放料配置一根轴的有效范围（最大＞最小）。");
                return false;
            }
            return true;
        }

        static void LoadRange(AxisLimitRange r, TextBox minX, TextBox maxX, TextBox minY, TextBox maxY, TextBox minZ, TextBox maxZ)
        {
            minX.Text = Format(r?.MinX ?? 0);
            maxX.Text = Format(r?.MaxX ?? 0);
            minY.Text = Format(r?.MinY ?? 0);
            maxY.Text = Format(r?.MaxY ?? 0);
            minZ.Text = Format(r?.MinZ ?? 0);
            maxZ.Text = Format(r?.MaxZ ?? 0);
        }

        static bool TryReadRange(string name, out AxisLimitRange r,
            TextBox minX, TextBox maxX, TextBox minY, TextBox maxY, TextBox minZ, TextBox maxZ)
        {
            r = new AxisLimitRange();
            if (!TryParseOptional(minX, $"{name} X最小", out double vMinX)) return false;
            if (!TryParseOptional(maxX, $"{name} X最大", out double vMaxX)) return false;
            if (!TryParseOptional(minY, $"{name} Y最小", out double vMinY)) return false;
            if (!TryParseOptional(maxY, $"{name} Y最大", out double vMaxY)) return false;
            if (!TryParseOptional(minZ, $"{name} Z最小", out double vMinZ)) return false;
            if (!TryParseOptional(maxZ, $"{name} Z最大", out double vMaxZ)) return false;
            r.MinX = vMinX; r.MaxX = vMaxX;
            r.MinY = vMinY; r.MaxY = vMaxY;
            r.MinZ = vMinZ; r.MaxZ = vMaxZ;
            return true;
        }

        static string Format(double v) => Math.Abs(v) < 1e-9 ? "" : v.ToString("G");

        static bool TryParseOptional(TextBox tb, string name, out double value)
        {
            value = 0;
            string text = (tb.Text ?? "").Trim();
            if (string.IsNullOrEmpty(text)) return true;
            if (double.TryParse(text, out value)) return true;
            DialogPrompts.ShowWarning($"{name} 不是有效数字。");
            tb.SelectAll();
            tb.Focus();
            return false;
        }

        /// <summary>登录成功后打开安全区域设定。</summary>
        public static void ShowAuthenticated(IWin32Window owner, Form1 main)
        {
            if (!AdminLoginForm.TryAuthenticate(owner))
                return;
            using (var dlg = new SafetyZoneSettingsForm(main))
                dlg.ShowDialog(owner);
        }
    }
}
