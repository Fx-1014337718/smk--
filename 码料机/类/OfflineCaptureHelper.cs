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
        public const string CaptureArchiveFolderName = "采图存档";

        /// <summary>将采图复制到 exe\采图存档\，文件名带时间戳，避免覆盖历史。</summary>
        public static string ArchiveCaptureImage(string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
                return null;

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CaptureArchiveFolderName);
            Directory.CreateDirectory(dir);
            string ext = Path.GetExtension(sourcePath);
            if (string.IsNullOrEmpty(ext))
                ext = ".bmp";
            string stem = Path.GetFileNameWithoutExtension(sourcePath) ?? "capture";
            foreach (char c in Path.GetInvalidFileNameChars())
                stem = stem.Replace(c, '_');
            if (stem.Length > 40)
                stem = stem.Substring(0, 40);
            string dest = Path.Combine(dir, $"{stem}_{DateTime.Now:yyyyMMdd_HHmmss_fff}{ext}");
            File.Copy(sourcePath, dest, true);
            return Path.GetFullPath(dest);
        }

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
