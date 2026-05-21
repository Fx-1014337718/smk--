using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace 码料机
{
    /// <summary>
    /// 相机畸变矫正：读取 <c>camera_calib.yml</c>（内参+畸变），纯 C# 对应 OpenCV initUndistortRectifyMap + remap，与「畸变矫正-测试」一致。
    /// </summary>
    public sealed class CameraUndistortion : IDisposable
    {
        public double Fx { get; private set; }
        public double Fy { get; private set; }
        public double Cx { get; private set; }
        public double Cy { get; private set; }
        public double K1 { get; private set; }
        public double K2 { get; private set; }
        public double P1 { get; private set; }
        public double P2 { get; private set; }
        public double K3 { get; private set; }
        public int CalibWidth { get; private set; }
        public int CalibHeight { get; private set; }

        public double Alpha { get; set; } = 1.0;
        public bool CropBlackEdge { get; set; }

        public string CalibFilePath { get; private set; }
        public string LastError { get; private set; }

        private float[] _mapX;
        private float[] _mapY;
        private int _mapWidth;
        private int _mapHeight;
        private int _validRoiX;
        private int _validRoiY;
        private int _validRoiW;
        private int _validRoiH;
        private double _newFx;
        private double _newFy;
        private double _newCx;
        private double _newCy;

        public bool IsReady => _mapX != null && _mapX.Length > 0;

        public static bool TryLoad(string calibYamlPath, out CameraUndistortion undistort, out string error)
        {
            undistort = null;
            error = null;
            if (string.IsNullOrWhiteSpace(calibYamlPath) || !File.Exists(calibYamlPath))
            {
                error = "标定文件不存在: " + calibYamlPath;
                return false;
            }

            try
            {
                var u = new CameraUndistortion();
                if (!u.LoadYaml(calibYamlPath, out error))
                    return false;
                u.CalibFilePath = Path.GetFullPath(calibYamlPath);
                undistort = u;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private bool LoadYaml(string path, out string error)
        {
            error = null;
            string text = File.ReadAllText(path, Encoding.UTF8);
            if (!TryParseMatrix(text, "camera_matrix", 3, 3, out double[] k))
            {
                error = "无法解析 camera_matrix";
                return false;
            }
            if (!TryParseMatrix(text, "dist_coeffs", 1, 5, out double[] d) && !TryParseMatrix(text, "dist_coeffs", 5, 1, out d))
            {
                error = "无法解析 dist_coeffs";
                return false;
            }

            Fx = k[0]; Fy = k[4]; Cx = k[2]; Cy = k[5];
            K1 = d[0]; K2 = d[1]; P1 = d[2]; P2 = d[3]; K3 = d[4];

            CalibWidth = ParseIntField(text, "image_width");
            CalibHeight = ParseIntField(text, "image_height");
            return true;
        }

        private static int ParseIntField(string text, string key)
        {
            var m = Regex.Match(text, key + @"\s*:\s*(\d+)", RegexOptions.IgnoreCase);
            return m.Success ? int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture) : 0;
        }

        private static bool TryParseMatrix(string yaml, string name, int rows, int cols, out double[] data)
        {
            data = null;
            int idx = yaml.IndexOf(name, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return false;
            int dataIdx = yaml.IndexOf("data:", idx, StringComparison.OrdinalIgnoreCase);
            if (dataIdx < 0) return false;
            int start = yaml.IndexOf('[', dataIdx);
            int end = yaml.IndexOf(']', start);
            if (start < 0 || end < 0) return false;

            string[] parts = yaml.Substring(start + 1, end - start - 1)
                .Split(new[] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int need = rows * cols;
            if (parts.Length < need) return false;

            data = new double[need];
            for (int i = 0; i < need; i++)
                data[i] = double.Parse(parts[i], CultureInfo.InvariantCulture);
            return true;
        }

        /// <summary>为指定尺寸构建 remap 表（与当前 Alpha 一致）。</summary>
        public bool EnsureMapsForSize(int width, int height, out string error)
        {
            error = null;
            if (width <= 0 || height <= 0)
            {
                error = "图像尺寸无效";
                return false;
            }
            if (_mapWidth == width && _mapHeight == height && _mapX != null)
                return true;

            if (CalibWidth > 0 && CalibHeight > 0 && (CalibWidth != width || CalibHeight != height))
                LastError = $"警告：图像 {width}x{height} 与标定 {CalibWidth}x{CalibHeight} 不一致";

            ComputeOptimalNewCameraMatrix(width, height, Alpha, out _newFx, out _newFy, out _newCx, out _newCy,
                out _validRoiX, out _validRoiY, out _validRoiW, out _validRoiH);

            BuildRemapMaps(width, height, _newFx, _newFy, _newCx, _newCy, out _mapX, out _mapY);
            _mapWidth = width;
            _mapHeight = height;
            return true;
        }

        /// <summary>矫正文件并保存（支持中文路径）。</summary>
        public bool UndistortFile(string inputPath, string outputPath, out string error)
        {
            error = null;
            if (!File.Exists(inputPath))
            {
                error = "输入图不存在";
                return false;
            }

            try
            {
                using (var src = LoadBitmap(inputPath))
                {
                    if (!EnsureMapsForSize(src.Width, src.Height, out error))
                        return false;
                    using (var dst = Remap(src))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? ".");
                        dst.Save(outputPath, ImageFormat.Bmp);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public Bitmap Undistort(Bitmap source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            string err;
            if (!EnsureMapsForSize(source.Width, source.Height, out err))
                throw new InvalidOperationException(err ?? "无法构建 remap");
            return Remap(source);
        }

        private Bitmap Remap(Bitmap src)
        {
            int w = src.Width;
            int h = src.Height;
            var dst = new Bitmap(w, h, PixelFormat.Format24bppRgb);
            var srcData = src.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            var dstData = dst.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            try
            {
                int srcStride = srcData.Stride;
                int dstStride = dstData.Stride;
                int srcLen = srcStride * h;
                int dstLen = dstStride * h;
                var srcBytes = new byte[srcLen];
                var dstBytes = new byte[dstLen];
                Marshal.Copy(srcData.Scan0, srcBytes, 0, srcLen);
                for (int y = 0; y < h; y++)
                {
                    int dstRow = y * dstStride;
                    for (int x = 0; x < w; x++)
                    {
                        float sx = _mapX[y * w + x];
                        float sy = _mapY[y * w + x];
                        BilinearSample(srcBytes, srcStride, w, h, sx, sy, out byte b, out byte g, out byte r);
                        int di = dstRow + x * 3;
                        dstBytes[di] = b;
                        dstBytes[di + 1] = g;
                        dstBytes[di + 2] = r;
                    }
                }
                Marshal.Copy(dstBytes, 0, dstData.Scan0, dstLen);
            }
            finally
            {
                src.UnlockBits(srcData);
                dst.UnlockBits(dstData);
            }

            if (CropBlackEdge && _validRoiW > 0 && _validRoiH > 0)
            {
                var crop = new Bitmap(_validRoiW, _validRoiH, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(crop))
                    g.DrawImage(dst, new Rectangle(0, 0, _validRoiW, _validRoiH),
                        new Rectangle(_validRoiX, _validRoiY, _validRoiW, _validRoiH), GraphicsUnit.Pixel);
                dst.Dispose();
                return crop;
            }
            return dst;
        }

        private static void BilinearSample(byte[] src, int stride, int w, int h, float x, float y,
            out byte b, out byte g, out byte r)
        {
            if (x < 0 || y < 0 || x >= w - 1 || y >= h - 1)
            {
                b = g = r = 0;
                return;
            }
            int x0 = (int)x;
            int y0 = (int)y;
            float dx = x - x0;
            float dy = y - y0;
            int Idx(int px, int py, int ch) => py * stride + px * 3 + ch;

            float Lerp(byte c00, byte c10, byte c01, byte c11)
            {
                float v0 = c00 + dx * (c10 - c00);
                float v1 = c01 + dx * (c11 - c01);
                return v0 + dy * (v1 - v0);
            }

            b = (byte)Lerp(src[Idx(x0, y0, 0)], src[Idx(x0 + 1, y0, 0)], src[Idx(x0, y0 + 1, 0)], src[Idx(x0 + 1, y0 + 1, 0)]);
            g = (byte)Lerp(src[Idx(x0, y0, 1)], src[Idx(x0 + 1, y0, 1)], src[Idx(x0, y0 + 1, 1)], src[Idx(x0 + 1, y0 + 1, 1)]);
            r = (byte)Lerp(src[Idx(x0, y0, 2)], src[Idx(x0 + 1, y0, 2)], src[Idx(x0, y0 + 1, 2)], src[Idx(x0 + 1, y0 + 1, 2)]);
        }

        private static Bitmap LoadBitmap(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var img = Image.FromStream(fs))
            {
                var bmp = new Bitmap(img.Width, img.Height, PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(bmp))
                    g.DrawImage(img, 0, 0, img.Width, img.Height);
                return bmp;
            }
        }

        private void BuildRemapMaps(int width, int height, double nFx, double nFy, double nCx, double nCy,
            out float[] mapX, out float[] mapY)
        {
            double[] iR = Invert3x3(new[]
            {
                nFx, 0, nCx,
                0, nFy, nCy,
                0, 0, 1
            });

            int n = width * height;
            mapX = new float[n];
            mapY = new float[n];
            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    double u = col;
                    double v = row;
                    double X = iR[0] * u + iR[1] * v + iR[2];
                    double Y = iR[3] * u + iR[4] * v + iR[5];
                    double W = iR[6] * u + iR[7] * v + iR[8];
                    double x = X / W;
                    double y = Y / W;
                    DistortNormalized(x, y, out double xd, out double yd);
                    int idx = row * width + col;
                    mapX[idx] = (float)(Fx * xd + Cx);
                    mapY[idx] = (float)(Fy * yd + Cy);
                }
            }
        }

        private void DistortNormalized(double x, double y, out double xd, out double yd)
        {
            double r2 = x * x + y * y;
            double r4 = r2 * r2;
            double r6 = r4 * r2;
            double cdist = 1 + K1 * r2 + K2 * r4 + K3 * r6;
            double a1 = 2 * x * y;
            double a2 = r2 + 2 * x * x;
            double a3 = r2 + 2 * y * y;
            xd = x * cdist + 2 * P1 * a1 + P2 * a2;
            yd = y * cdist + P1 * a3 + 2 * P2 * a1;
        }

        private void ComputeOptimalNewCameraMatrix(int imgW, int imgH, double alpha,
            out double nFx, out double nFy, out double nCx, out double nCy,
            out int roiX, out int roiY, out int roiW, out int roiH)
        {
            GetUndistortRectangles(imgW, imgH, out double iX0, out double iY0, out double iW, out double iH,
                out double oX0, out double oY0, out double oW, out double oH);

            double fx0 = (imgW - 1) / iW;
            double fy0 = (imgH - 1) / iH;
            double cx0 = -fx0 * iX0;
            double cy0 = -fy0 * iY0;

            double fx1 = (imgW - 1) / oW;
            double fy1 = (imgH - 1) / oH;
            double cx1 = -fx1 * oX0;
            double cy1 = -fy1 * oY0;

            nFx = fx0 * (1 - alpha) + fx1 * alpha;
            nFy = fy0 * (1 - alpha) + fy1 * alpha;
            nCx = cx0 * (1 - alpha) + cx1 * alpha;
            nCy = cy0 * (1 - alpha) + cy1 * alpha;

            GetUndistortRectanglesWithNewMatrix(imgW, imgH, nFx, nFy, nCx, nCy,
                out double rX, out double rY, out double rW, out double rH);
            roiX = Math.Max(0, (int)Math.Ceiling(rX));
            roiY = Math.Max(0, (int)Math.Ceiling(rY));
            int rRight = Math.Min(imgW, (int)Math.Floor(rX + rW));
            int rBottom = Math.Min(imgH, (int)Math.Floor(rY + rH));
            roiW = Math.Max(0, rRight - roiX);
            roiH = Math.Max(0, rBottom - roiY);
        }

        private void GetUndistortRectangles(int imgW, int imgH,
            out double iX0, out double iY0, out double iW, out double iH,
            out double oX0, out double oY0, out double oW, out double oH)
        {
            const int N = 9;
            double stepX = (imgW - 1.0) / (N - 1);
            double stepY = (imgH - 1.0) / (N - 1);
            double iX0v = double.NegativeInfinity, iX1 = double.PositiveInfinity;
            double iY0v = double.NegativeInfinity, iY1 = double.PositiveInfinity;
            double oX0v = double.PositiveInfinity, oX1 = double.NegativeInfinity;
            double oY0v = double.PositiveInfinity, oY1 = double.NegativeInfinity;

            for (int gy = 0; gy < N; gy++)
            {
                for (int gx = 0; gx < N; gx++)
                {
                    if (gx != 0 && gx != N - 1 && gy != 0 && gy != N - 1)
                        continue;
                    double px = gx * stepX;
                    double py = gy * stepY;
                    UndistortToNormalized(px, py, out double nx, out double ny);

                    oX0v = Math.Min(oX0v, nx);
                    oX1 = Math.Max(oX1, nx);
                    oY0v = Math.Min(oY0v, ny);
                    oY1 = Math.Max(oY1, ny);

                    if (gx == 0) iX0v = Math.Max(iX0v, nx);
                    if (gx == N - 1) iX1 = Math.Min(iX1, nx);
                    if (gy == 0) iY0v = Math.Max(iY0v, ny);
                    if (gy == N - 1) iY1 = Math.Min(iY1, ny);
                }
            }

            iX0 = iX0v; iY0 = iY0v; iW = iX1 - iX0v; iH = iY1 - iY0v;
            oX0 = oX0v; oY0 = oY0v; oW = oX1 - oX0v; oH = oY1 - oY0v;
        }

        private void GetUndistortRectanglesWithNewMatrix(int imgW, int imgH,
            double nFx, double nFy, double nCx, double nCy,
            out double rX, out double rY, out double rW, out double rH)
        {
            const int N = 9;
            double stepX = (imgW - 1.0) / (N - 1);
            double stepY = (imgH - 1.0) / (N - 1);
            double iX0 = double.NegativeInfinity, iX1 = double.PositiveInfinity;
            double iY0 = double.NegativeInfinity, iY1 = double.PositiveInfinity;

            for (int gy = 0; gy < N; gy++)
            {
                for (int gx = 0; gx < N; gx++)
                {
                    if (gx != 0 && gx != N - 1 && gy != 0 && gy != N - 1)
                        continue;
                    UndistortToNormalized(gx * stepX, gy * stepY, out double nx, out double ny);
                    double ux = nFx * nx + nCx;
                    double uy = nFy * ny + nCy;

                    if (gx == 0) iX0 = Math.Max(iX0, ux);
                    if (gx == N - 1) iX1 = Math.Min(iX1, ux);
                    if (gy == 0) iY0 = Math.Max(iY0, uy);
                    if (gy == N - 1) iY1 = Math.Min(iY1, uy);
                }
            }
            rX = iX0; rY = iY0; rW = iX1 - iX0; rH = iY1 - iY0;
        }

        /// <summary>像素坐标 → 归一化无畸变坐标（OpenCV undistortPoints，P 为空）。</summary>
        private void UndistortToNormalized(double u, double v, out double x, out double y)
        {
            double x0 = (u - Cx) / Fx;
            double y0 = (v - Cy) / Fy;
            x = x0;
            y = y0;
            const int maxIter = 5;
            const double eps = 0.01;
            for (int j = 0; j < maxIter; j++)
            {
                double r2 = x * x + y * y;
                double r4 = r2 * r2;
                double r6 = r4 * r2;
                double icdist = (1 + K3 * r6) / (1 + K1 * r2 + K2 * r4 + K3 * r6);
                if (icdist < 0)
                {
                    x = x0; y = y0;
                    break;
                }
                double deltaX = 2 * P1 * x * y + P2 * (r2 + 2 * x * x);
                double deltaY = P1 * (r2 + 2 * y * y) + 2 * P2 * x * y;
                double newX = (x0 - deltaX) * icdist;
                double newY = (y0 - deltaY) * icdist;
                DistortNormalized(newX, newY, out double xd, out double yd);
                double projX = xd * Fx + Cx;
                double projY = yd * Fy + Cy;
                double err = Math.Sqrt((projX - u) * (projX - u) + (projY - v) * (projY - v));
                if (err < eps)
                {
                    x = newX; y = newY;
                    break;
                }
                x = newX;
                y = newY;
            }
        }

        private static double[] Invert3x3(double[] m)
        {
            double a = m[0], b = m[1], c = m[2];
            double d = m[3], e = m[4], f = m[5];
            double g = m[6], h = m[7], i = m[8];
            double det = a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
            if (Math.Abs(det) < 1e-12)
                throw new InvalidOperationException("相机矩阵不可逆");
            double invDet = 1.0 / det;
            return new[]
            {
                (e * i - f * h) * invDet, (c * h - b * i) * invDet, (b * f - c * e) * invDet,
                (f * g - d * i) * invDet, (a * i - c * g) * invDet, (c * d - a * f) * invDet,
                (d * h - e * g) * invDet, (b * g - a * h) * invDet, (a * e - b * d) * invDet
            };
        }

        public void Dispose()
        {
            _mapX = null;
            _mapY = null;
        }
    }
}
