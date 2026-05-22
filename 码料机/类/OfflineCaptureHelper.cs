using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace 码料机
{
    /// <summary>离线测试图落盘（exe 旁 Feed.bmp），供金沃算法与海康采图回退。</summary>
    public static class OfflineCaptureHelper
    {
        public const string DefaultOfflineFeedFileName = "Feed.bmp";

        public static string StageOfflineCaptureImage(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("图片不存在", sourcePath);

            string feedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultOfflineFeedFileName);
            string ext = Path.GetExtension(sourcePath);
            if (string.Equals(ext, ".bmp", StringComparison.OrdinalIgnoreCase))
                File.Copy(sourcePath, feedPath, true);
            else
            {
                using (var img = Image.FromFile(sourcePath))
                    img.Save(feedPath, ImageFormat.Bmp);
            }
            return Path.GetFullPath(feedPath);
        }
    }
}
