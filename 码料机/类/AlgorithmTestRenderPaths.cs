using System;
using System.IO;

namespace 码料机
{
    /// <summary>算法测试窗体：各算法独立效果图目录与唯一文件名，避免互相覆盖。</summary>
    internal static class AlgorithmTestRenderPaths
    {
        public const string RootFolderName = "算法测试效果图";

        public enum Kind
        {
            Presence,
            JinwoMarkers,
            JinwoPose,
            JinwoPlan
        }

        public static string GetDirectory(Kind kind)
        {
            string sub;
            switch (kind)
            {
                case Kind.Presence: sub = "有无料"; break;
                case Kind.JinwoMarkers: sub = "金沃_黑圆"; break;
                case Kind.JinwoPose: sub = "金沃_单点算位"; break;
                case Kind.JinwoPlan: sub = "金沃_全箱规划"; break;
                default: sub = "其他"; break;
            }
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RootFolderName, sub);
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static string AllocateFile(Kind kind, string sourceImagePath, string extension)
        {
            string dir = GetDirectory(kind);
            string stem = SanitizeFileStem(Path.GetFileNameWithoutExtension(sourceImagePath) ?? "image");
            string ext = string.IsNullOrWhiteSpace(extension) ? ".jpg" : extension;
            if (!ext.StartsWith(".")) ext = "." + ext;
            return Path.Combine(dir, $"{stem}_{DateTime.Now:yyyyMMdd_HHmmss_fff}{ext}");
        }

        /// <summary>将 DLL/叠加图复制到本算法专用目录；源路径为空时返回 null。</summary>
        public static string Publish(string sourceRenderPath, Kind kind, string sourceImagePath)
        {
            if (string.IsNullOrWhiteSpace(sourceRenderPath) || !File.Exists(sourceRenderPath))
                return null;
            string dest = AllocateFile(kind, sourceImagePath, Path.GetExtension(sourceRenderPath));
            try
            {
                File.Copy(sourceRenderPath, dest, true);
                return dest;
            }
            catch
            {
                return null;
            }
        }

        static string SanitizeFileStem(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "image";
            foreach (char c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Length > 40 ? name.Substring(0, 40) : name;
        }
    }
}
