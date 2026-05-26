using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace 码料机
{
    /// <summary>金沃 DLL 无效果图时，在采图上叠加黑圆标注（仅 DetectMarkers 场景）；<paramref name="sourceImagePath"/> 须与检测所用图像一致（一般为畸变矫正后路径）。</summary>
    internal static class JinwoImagePreview
    {
        /// <summary>8 位索引色等格式不能直接 Graphics.FromImage，先转为 32bpp ARGB。</summary>
        private static Bitmap LoadDrawableBitmap(string imagePath)
        {
            using (var src = new Bitmap(imagePath))
            {
                var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(dst))
                    g.DrawImage(src, 0, 0, src.Width, src.Height);
                return dst;
            }
        }

        public static string DrawMarkersOverlay(string sourceImagePath, JinwoNative.JinwoMarkerResult markers, string outputDir)
            => DrawMarkersOverlay(sourceImagePath, markers, outputDir, null);

        /// <param name="outputFilePath">非空时直接保存到该路径（用于算法测试独立渲染图）。</param>
        public static string DrawMarkersOverlay(string sourceImagePath, JinwoNative.JinwoMarkerResult markers,
            string outputDir, string outputFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceImagePath) || !File.Exists(sourceImagePath))
                return null;
            if (markers.MarkerPixels == null || markers.MarkerPixels.Length == 0)
                return null;

            string outPath = string.IsNullOrWhiteSpace(outputFilePath)
                ? Path.Combine(outputDir, "markers_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".bmp")
                : outputFilePath;
            Directory.CreateDirectory(Path.GetDirectoryName(outPath) ?? outputDir);

            using (var bmp = LoadDrawableBitmap(sourceImagePath))
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var pen = new Pen(Color.Lime, 3f))
                using (var fill = new SolidBrush(Color.FromArgb(160, Color.Red)))
                using (var font = new Font("Microsoft YaHei UI", 12f, FontStyle.Bold))
                {
                    string[] labels = { "0左上", "1右上", "2右下", "3左下" };
                    for (int i = 0; i < markers.MarkerPixels.Length && i < JinwoNative.MarkerCount; i++)
                    {
                        float x = (float)markers.MarkerPixels[i].X;
                        float y = (float)markers.MarkerPixels[i].Y;
                        if (x <= 0 && y <= 0) continue;
                        float r = Math.Max(12f, Math.Min(bmp.Width, bmp.Height) * 0.02f);
                        g.DrawEllipse(pen, x - r, y - r, r * 2, r * 2);
                        g.FillEllipse(fill, x - 4, y - 4, 8, 8);
                        string label = i < labels.Length ? labels[i] : i.ToString();
                        g.DrawString(label, font, Brushes.Yellow, x + r + 2, y - r);
                    }
                }
                bmp.Save(outPath, ImageFormat.Bmp);
            }
            return outPath;
        }
    }
}
