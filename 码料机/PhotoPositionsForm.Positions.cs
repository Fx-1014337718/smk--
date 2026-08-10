using System;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>左/右工位点位读写（Designer 控件）。</summary>
    partial class PhotoPositionsForm
    {
        void LoadPositionToUi(bool isLeft, PhotoPositionConfig c)
        {
            if (c == null) c = new PhotoPositionConfig();
            if (isLeft)
            {
                textLeftPickX.Text = Format(c.PickX);
                textLeftPickY.Text = Format(c.PickY);
                textLeftPickZ.Text = Format(c.PickZ);
                textLeftPickRz.Text = Format(c.PickRz);
                textLeftPlaceX.Text = Format(c.PlaceX);
                textLeftPlaceY.Text = Format(c.PlaceY);
                textLeftPlaceZ.Text = Format(c.PlaceZ);
                textLeftPlaceRz.Text = Format(c.PlaceRz);
                textLeftPlacePhotoX.Text = Format(c.PlacePhotoX);
                textLeftPlacePhotoY.Text = Format(c.PlacePhotoY);
                textLeftPlacePhotoZ.Text = Format(c.PlacePhotoZ);
                textLeftPlacePhotoRz.Text = Format(c.PlacePhotoRz);
                textLeftPlaceCenterRz.Text = Format(c.PlaceCenterRz);
            }
            else
            {
                textRightPickX.Text = Format(c.PickX);
                textRightPickY.Text = Format(c.PickY);
                textRightPickZ.Text = Format(c.PickZ);
                textRightPickRz.Text = Format(c.PickRz);
                textRightPlaceX.Text = Format(c.PlaceX);
                textRightPlaceY.Text = Format(c.PlaceY);
                textRightPlaceZ.Text = Format(c.PlaceZ);
                textRightPlaceRz.Text = Format(c.PlaceRz);
                textRightPlacePhotoX.Text = Format(c.PlacePhotoX);
                textRightPlacePhotoY.Text = Format(c.PlacePhotoY);
                textRightPlacePhotoZ.Text = Format(c.PlacePhotoZ);
                textRightPlacePhotoRz.Text = Format(c.PlacePhotoRz);
                textRightPlaceCenterRz.Text = Format(c.PlaceCenterRz);
            }
        }

        bool TryReadPositionFromUi(bool isLeft, string stationName, AlarmPositionLimitConfig safety, out PhotoPositionConfig c)
        {
            c = new PhotoPositionConfig();
            TextBox pickX, pickY, pickZ, pickRz, placeX, placeY, placeZ, placeRz;
            TextBox photoX, photoY, photoZ, photoRz, centerRz;
            if (isLeft)
            {
                pickX = textLeftPickX; pickY = textLeftPickY; pickZ = textLeftPickZ; pickRz = textLeftPickRz;
                placeX = textLeftPlaceX; placeY = textLeftPlaceY; placeZ = textLeftPlaceZ; placeRz = textLeftPlaceRz;
                photoX = textLeftPlacePhotoX; photoY = textLeftPlacePhotoY; photoZ = textLeftPlacePhotoZ; photoRz = textLeftPlacePhotoRz;
                centerRz = textLeftPlaceCenterRz;
            }
            else
            {
                pickX = textRightPickX; pickY = textRightPickY; pickZ = textRightPickZ; pickRz = textRightPickRz;
                placeX = textRightPlaceX; placeY = textRightPlaceY; placeZ = textRightPlaceZ; placeRz = textRightPlaceRz;
                photoX = textRightPlacePhotoX; photoY = textRightPlacePhotoY; photoZ = textRightPlacePhotoZ; photoRz = textRightPlacePhotoRz;
                centerRz = textRightPlaceCenterRz;
            }

            if (!TryParse(pickX, $"{stationName} 取料位置 X", out double vPickX)) return false;
            if (!TryParse(pickY, $"{stationName} 取料位置 Y", out double vPickY)) return false;
            if (!TryParse(pickZ, $"{stationName} 取料位置 Z", out double vPickZ)) return false;
            if (!TryParse(placeX, $"{stationName} 放料位置 X", out double vPlaceX)) return false;
            if (!TryParse(placeY, $"{stationName} 放料位置 Y", out double vPlaceY)) return false;
            if (!TryParse(placeZ, $"{stationName} 放料位置 Z", out double vPlaceZ)) return false;
            if (!ValidateAgainstSafety(safety, true, pickX, pickY, pickZ, vPickX, vPickY, vPickZ, stationName + " 取料")) return false;
            if (!ValidateAgainstSafety(safety, false, placeX, placeY, placeZ, vPlaceX, vPlaceY, vPlaceZ, stationName + " 放料")) return false;
            if (!TryParse(photoX, $"{stationName} 放料拍照位置 X", out double vPhotoX)) return false;
            if (!TryParse(photoY, $"{stationName} 放料拍照位置 Y", out double vPhotoY)) return false;
            if (!TryParse(photoZ, $"{stationName} 放料拍照位置 Z", out double vPhotoZ)) return false;
            if (!TryParseOptional(pickRz, $"{stationName} 取料位置 RZ", out double vPickRz)) return false;
            if (!TryParseOptional(placeRz, $"{stationName} 放料位置 RZ", out double vPlaceRz)) return false;
            if (!TryParseOptional(photoRz, $"{stationName} 放料拍照位置 RZ", out double vPhotoRz)) return false;
            if (!TryParseOptional(centerRz, $"{stationName} 放料中心点 RZ", out double vCenterRz)) return false;

            c.PickX = vPickX; c.PickY = vPickY; c.PickZ = vPickZ; c.PickRz = vPickRz;
            c.PlaceX = vPlaceX; c.PlaceY = vPlaceY; c.PlaceZ = vPlaceZ; c.PlaceRz = vPlaceRz;
            c.PlacePhotoX = vPhotoX; c.PlacePhotoY = vPhotoY; c.PlacePhotoZ = vPhotoZ; c.PlacePhotoRz = vPhotoRz;
            c.PlaceCenterRz = vCenterRz;
            return true;
        }

        static bool ValidateAgainstSafety(
            AlarmPositionLimitConfig safety, bool isPick,
            TextBox tbX, TextBox tbY, TextBox tbZ,
            double x, double y, double z, string label)
        {
            if (safety == null || !safety.Enabled) return true;
            if (!safety.IsOutOfLimit(isPick, x, y, z, out string detail)) return true;
            var range = isPick ? safety.Pick : safety.Place;
            if (range != null)
            {
                if (range.HasAxisLimit(range.MinX, range.MaxX) && (x < range.MinX || x > range.MaxX)) tbX.Text = "";
                if (range.HasAxisLimit(range.MinY, range.MaxY) && (y < range.MinY || y > range.MaxY)) tbY.Text = "";
                if (range.HasAxisLimit(range.MinZ, range.MaxZ) && (z < range.MinZ || z > range.MaxZ)) tbZ.Text = "";
            }
            DialogPrompts.ShowWarning(label + "坐标超出安全区域：\n" + detail + "\n已清除超限输入框。");
            return false;
        }

        bool IsPickXYEmpty(bool isLeft)
        {
            var x = isLeft ? textLeftPickX : textRightPickX;
            var y = isLeft ? textLeftPickY : textRightPickY;
            return IsEmpty(x) && IsEmpty(y);
        }

        bool IsPlacePhotoXYEmpty(bool isLeft)
        {
            var x = isLeft ? textLeftPlacePhotoX : textRightPlacePhotoX;
            var y = isLeft ? textLeftPlacePhotoY : textRightPlacePhotoY;
            return IsEmpty(x) && IsEmpty(y);
        }

        void SetPickXY(bool isLeft, double x, double y)
        {
            if (isLeft) { textLeftPickX.Text = x.ToString("G"); textLeftPickY.Text = y.ToString("G"); }
            else { textRightPickX.Text = x.ToString("G"); textRightPickY.Text = y.ToString("G"); }
        }

        void SetPlacePhotoXY(bool isLeft, double x, double y)
        {
            if (isLeft) { textLeftPlacePhotoX.Text = x.ToString("G"); textLeftPlacePhotoY.Text = y.ToString("G"); }
            else { textRightPlacePhotoX.Text = x.ToString("G"); textRightPlacePhotoY.Text = y.ToString("G"); }
        }

        static bool IsEmpty(TextBox tb) =>
            string.IsNullOrWhiteSpace(tb.Text) || (double.TryParse(tb.Text.Trim(), out double v) && Math.Abs(v) < 1e-9);

        static string Format(double v) => Math.Abs(v) < 1e-9 ? "" : v.ToString("G");

        static bool TryParse(TextBox tb, string name, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(tb.Text))
                return true;
            if (double.TryParse(tb.Text.Trim(), out value))
                return true;
            DialogPrompts.ShowWarning($"{name} 不是有效数字。");
            tb.SelectAll();
            tb.Focus();
            return false;
        }

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
    }
}
