using System;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>位置设定页底部「限位报警参数」读写与安全区域边界校验。</summary>
    partial class PhotoPositionsForm
    {
        private bool _limitLoading;

        private void LimitField_Changed(object sender, EventArgs e)
        {
            if (!_limitLoading)
                _dirty = true;
        }

        private void LimitField_Leave(object sender, EventArgs e)
        {
            if (!(sender is TextBox tb) || string.IsNullOrWhiteSpace(tb.Text))
                return;
            if (!double.TryParse(tb.Text.Trim(), out double value))
                return;

            bool isLeft = IsLeftLimitControl(tb);
            bool isPick = IsPickLimitControl(tb);
            char axis = GetLimitAxis(tb);
            var envelope = MainForm?.GetSafetyEnvelope(isLeft);
            if (envelope == null || !envelope.Enabled)
                return;
            if (!envelope.IsAxisValueOutOfLimit(isPick, axis, value, out string detail))
                return;

            tb.Text = "";
            DialogPrompts.ShowWarning(
                "限位报警参数超出安全区域允许范围：\n" + detail + "\n已清除该输入。\n如需放宽范围，请先通过「安全区域」修改。");
        }

        void LoadLimitToUi(bool isLeft, AlarmPositionLimitConfig c)
        {
            _limitLoading = true;
            try
            {
                if (isLeft)
                {
                    checkLeftLimitEnabled.Checked = c?.Enabled == true;
                    LoadRange(c?.Pick,
                        textLeftLimitPickXMin, textLeftLimitPickXMax,
                        textLeftLimitPickYMin, textLeftLimitPickYMax,
                        textLeftLimitPickZMin, textLeftLimitPickZMax);
                    LoadRange(c?.Place,
                        textLeftLimitPlaceXMin, textLeftLimitPlaceXMax,
                        textLeftLimitPlaceYMin, textLeftLimitPlaceYMax,
                        textLeftLimitPlaceZMin, textLeftLimitPlaceZMax);
                }
                else
                {
                    checkRightLimitEnabled.Checked = c?.Enabled == true;
                    LoadRange(c?.Pick,
                        textRightLimitPickXMin, textRightLimitPickXMax,
                        textRightLimitPickYMin, textRightLimitPickYMax,
                        textRightLimitPickZMin, textRightLimitPickZMax);
                    LoadRange(c?.Place,
                        textRightLimitPlaceXMin, textRightLimitPlaceXMax,
                        textRightLimitPlaceYMin, textRightLimitPlaceYMax,
                        textRightLimitPlaceZMin, textRightLimitPlaceZMax);
                }
            }
            finally
            {
                _limitLoading = false;
            }
        }

        AlarmPositionLimitConfig PeekLimitFromUi(bool isLeft)
        {
            if (!TryReadLimitFromUi(isLeft, isLeft ? "左机台" : "右机台", out var c, quiet: true))
                return null;
            return c;
        }

        bool TryReadLimitFromUi(bool isLeft, string stationName, out AlarmPositionLimitConfig c, bool quiet = false)
        {
            c = new AlarmPositionLimitConfig
            {
                Enabled = isLeft ? checkLeftLimitEnabled.Checked : checkRightLimitEnabled.Checked,
            };

            if (isLeft)
            {
                if (!TryReadRange($"{stationName} 取料", quiet, out var pick,
                        textLeftLimitPickXMin, textLeftLimitPickXMax,
                        textLeftLimitPickYMin, textLeftLimitPickYMax,
                        textLeftLimitPickZMin, textLeftLimitPickZMax))
                    return false;
                if (!TryReadRange($"{stationName} 放料", quiet, out var place,
                        textLeftLimitPlaceXMin, textLeftLimitPlaceXMax,
                        textLeftLimitPlaceYMin, textLeftLimitPlaceYMax,
                        textLeftLimitPlaceZMin, textLeftLimitPlaceZMax))
                    return false;
                c.Pick = pick;
                c.Place = place;
            }
            else
            {
                if (!TryReadRange($"{stationName} 取料", quiet, out var pick,
                        textRightLimitPickXMin, textRightLimitPickXMax,
                        textRightLimitPickYMin, textRightLimitPickYMax,
                        textRightLimitPickZMin, textRightLimitPickZMax))
                    return false;
                if (!TryReadRange($"{stationName} 放料", quiet, out var place,
                        textRightLimitPlaceXMin, textRightLimitPlaceXMax,
                        textRightLimitPlaceYMin, textRightLimitPlaceYMax,
                        textRightLimitPlaceZMin, textRightLimitPlaceZMax))
                    return false;
                c.Pick = pick;
                c.Place = place;
            }

            if (!quiet && c.Enabled && !c.HasAnyAxisLimit())
            {
                DialogPrompts.ShowWarning($"「{stationName}」已启用限位报警，请至少为取料或放料配置一根轴的有效范围（最大＞最小）。");
                return false;
            }

            if (!quiet && !ValidateLimitAgainstEnvelope(isLeft, stationName, c))
                return false;

            return true;
        }

        bool ValidateLimitAgainstEnvelope(bool isLeft, string stationName, AlarmPositionLimitConfig c)
        {
            var envelope = MainForm?.GetSafetyEnvelope(isLeft);
            if (envelope == null || !envelope.Enabled || !envelope.HasAnyAxisLimit())
                return true;

            if (!ValidateRangeAgainstEnvelope($"{stationName} 取料", c.Pick, envelope.Pick, true)
                || !ValidateRangeAgainstEnvelope($"{stationName} 放料", c.Place, envelope.Place, false))
                return false;
            return true;
        }

        bool ValidateRangeAgainstEnvelope(string name, AxisLimitRange value, AxisLimitRange env, bool isPick)
        {
            if (value == null || env == null) return true;
            return CheckAxis(name, 'X', value.MinX, value.MaxX, env, isPick)
                && CheckAxis(name, 'Y', value.MinY, value.MaxY, env, isPick)
                && CheckAxis(name, 'Z', value.MinZ, value.MaxZ, env, isPick);
        }

        bool CheckAxis(string name, char axis, double min, double max, AxisLimitRange env, bool isPick)
        {
            bool minSet = !IsEffectivelyEmpty(min);
            bool maxSet = !IsEffectivelyEmpty(max);
            if (!minSet && !maxSet) return true;

            // Pick/Place 都指向同一 env 段，用 isPick 选择无影响；统一走 Enabled 校验
            var wrap = new AlarmPositionLimitConfig { Enabled = true, Pick = env, Place = env };

            if (minSet && wrap.IsAxisValueOutOfLimit(isPick, axis, min, out string dMin))
            {
                DialogPrompts.ShowWarning($"{name} {axis}最小 超出安全区域：\n{dMin}");
                return false;
            }
            if (maxSet && wrap.IsAxisValueOutOfLimit(isPick, axis, max, out string dMax))
            {
                DialogPrompts.ShowWarning($"{name} {axis}最大 超出安全区域：\n{dMax}");
                return false;
            }
            return true;
        }

        static bool IsEffectivelyEmpty(double v) => Math.Abs(v) < 1e-9;

        static void LoadRange(AxisLimitRange r, TextBox minX, TextBox maxX, TextBox minY, TextBox maxY, TextBox minZ, TextBox maxZ)
        {
            minX.Text = FormatLimit(r?.MinX ?? 0);
            maxX.Text = FormatLimit(r?.MaxX ?? 0);
            minY.Text = FormatLimit(r?.MinY ?? 0);
            maxY.Text = FormatLimit(r?.MaxY ?? 0);
            minZ.Text = FormatLimit(r?.MinZ ?? 0);
            maxZ.Text = FormatLimit(r?.MaxZ ?? 0);
        }

        bool TryReadRange(string name, bool quiet, out AxisLimitRange r,
            TextBox minX, TextBox maxX, TextBox minY, TextBox maxY, TextBox minZ, TextBox maxZ)
        {
            r = new AxisLimitRange();
            if (!TryParseOptional(minX, $"{name} X最小", quiet, out double vMinX)) return false;
            if (!TryParseOptional(maxX, $"{name} X最大", quiet, out double vMaxX)) return false;
            if (!TryParseOptional(minY, $"{name} Y最小", quiet, out double vMinY)) return false;
            if (!TryParseOptional(maxY, $"{name} Y最大", quiet, out double vMaxY)) return false;
            if (!TryParseOptional(minZ, $"{name} Z最小", quiet, out double vMinZ)) return false;
            if (!TryParseOptional(maxZ, $"{name} Z最大", quiet, out double vMaxZ)) return false;
            r.MinX = vMinX; r.MaxX = vMaxX;
            r.MinY = vMinY; r.MaxY = vMaxY;
            r.MinZ = vMinZ; r.MaxZ = vMaxZ;
            return true;
        }

        static bool TryParseOptional(TextBox tb, string name, bool quiet, out double value)
        {
            value = 0;
            string text = (tb.Text ?? "").Trim();
            if (string.IsNullOrEmpty(text)) return true;
            if (double.TryParse(text, out value)) return true;
            if (!quiet)
            {
                DialogPrompts.ShowWarning($"{name} 不是有效数字。");
                tb.SelectAll();
                tb.Focus();
            }
            return false;
        }

        static string FormatLimit(double v) => Math.Abs(v) < 1e-9 ? "" : v.ToString("G");

        bool IsLeftLimitControl(Control c) =>
            c != null && (c.Name.StartsWith("textLeftLimit", StringComparison.Ordinal)
                         || c.Name.StartsWith("checkLeftLimit", StringComparison.Ordinal));

        static bool IsPickLimitControl(Control c) =>
            c?.Name != null && c.Name.IndexOf("LimitPick", StringComparison.Ordinal) >= 0;

        static char GetLimitAxis(Control c)
        {
            string n = c?.Name ?? "";
            if (n.IndexOf("XMin", StringComparison.Ordinal) >= 0 || n.IndexOf("XMax", StringComparison.Ordinal) >= 0) return 'X';
            if (n.IndexOf("YMin", StringComparison.Ordinal) >= 0 || n.IndexOf("YMax", StringComparison.Ordinal) >= 0) return 'Y';
            if (n.IndexOf("ZMin", StringComparison.Ordinal) >= 0 || n.IndexOf("ZMax", StringComparison.Ordinal) >= 0) return 'Z';
            return '?';
        }
    }
}
