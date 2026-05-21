using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace 码料机
{
    /// <summary>
    /// <c>robot_calib.yml</c>：用于像素坐标转机械坐标（读取 <c>pixel_to_robot_matrix</c> 3×3），与金沃九点标定 OpenCV 示例一致；畸变矫正请使用 <c>camera_calib.yml</c>。
    /// </summary>
    public static class NinePointRobotCalib
    {
        public static bool TryLoad(string yamlPath, out double[] pixelToRobot3x3, out double avgErrorMm, out string error)
        {
            pixelToRobot3x3 = null;
            avgErrorMm = double.NaN;
            error = null;
            if (string.IsNullOrWhiteSpace(yamlPath) || !File.Exists(yamlPath))
            {
                error = "标定文件不存在: " + yamlPath;
                return false;
            }

            try
            {
                string text = File.ReadAllText(yamlPath, Encoding.UTF8);
                if (!TryParseOpenCvMatrix(text, "pixel_to_robot_matrix", 3, 3, out pixelToRobot3x3))
                {
                    error = "无法解析 pixel_to_robot_matrix（需 3×3 opencv-matrix）";
                    return false;
                }

                var mErr = Regex.Match(text, @"avg_error_mm\s*:\s*([-0-9.eE+]+)", RegexOptions.IgnoreCase);
                if (mErr.Success)
                    double.TryParse(mErr.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out avgErrorMm);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        /// <summary>与九点标定.cpp <c>pixelToRobot</c> 相同：齐次坐标 × 单应，再除以 W。</summary>
        public static bool TryPixelToRobot(double[] h3x3, double u, double v, out double robotX, out double robotY, out string error)
        {
            robotX = robotY = 0;
            error = null;
            if (h3x3 == null || h3x3.Length != 9)
            {
                error = "单应矩阵无效";
                return false;
            }

            double h00 = h3x3[0], h01 = h3x3[1], h02 = h3x3[2];
            double h10 = h3x3[3], h11 = h3x3[4], h12 = h3x3[5];
            double h20 = h3x3[6], h21 = h3x3[7], h22 = h3x3[8];
            double w = h20 * u + h21 * v + h22;
            if (Math.Abs(w) < 1e-9)
            {
                error = "坐标转换失败：W 接近 0";
                return false;
            }

            robotX = (h00 * u + h01 * v + h02) / w;
            robotY = (h10 * u + h11 * v + h12) / w;
            return true;
        }

        private static bool TryParseOpenCvMatrix(string yaml, string name, int rows, int cols, out double[] data)
        {
            data = null;
            int idx = yaml.IndexOf(name, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return false;
            int sliceFrom = idx;
            int dataIdx = yaml.IndexOf("data:", sliceFrom, StringComparison.OrdinalIgnoreCase);
            if (dataIdx < 0) return false;
            int start = yaml.IndexOf('[', dataIdx);
            int end = yaml.IndexOf(']', start);
            if (start < 0 || end < 0 || end <= start) return false;

            string[] parts = yaml.Substring(start + 1, end - start - 1)
                .Split(new[] { ',', ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int need = rows * cols;
            if (parts.Length < need) return false;

            data = new double[need];
            for (int i = 0; i < need; i++)
                data[i] = double.Parse(parts[i], CultureInfo.InvariantCulture);
            return true;
        }
    }
}
