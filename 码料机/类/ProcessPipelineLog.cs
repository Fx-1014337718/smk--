using System;
using System.IO;

namespace 码料机
{
    /// <summary>采图后的处理流水线日志：界面列表 + log\ImageProcess.log。</summary>
    public static class ProcessPipelineLog
    {
        /// <summary>由主界面绑定到 TEXT 等。</summary>
        public static Action<string> OnUiLog;

        const string LogFileName = "ImageProcess.log";

        public static void Write(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            try { OnUiLog?.Invoke(line); } catch { }
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, LogFileName),
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + line + Environment.NewLine);
            }
            catch { }
        }

        public static void ImageLoaded(string tag, string sourcePath, string feedPath, string detail = null)
        {
            string src = SafeFileName(sourcePath);
            string feed = SafeFileName(feedPath);
            string extra = string.IsNullOrWhiteSpace(detail) ? "" : " " + detail.Trim();
            Write($"{tag}[采图] 已加载 {src} → {feed}{extra}");
        }

        public static void UndistortSkipped(string sourcePath, string reason)
            => Write($"[畸变矫正] 跳过（{reason}）: {SafeFileName(sourcePath)}");

        public static void UndistortCacheHit(string preparedPath)
            => Write($"[畸变矫正] 使用缓存: {SafeFileName(preparedPath)}");

        public static void UndistortStart(string sourcePath, string outputPath)
            => Write($"[畸变矫正] 开始 {SafeFileName(sourcePath)} → {SafeFileName(outputPath)}");

        public static void UndistortDone(string sourcePath, string preparedPath)
            => Write($"[畸变矫正] 完成 {SafeFileName(sourcePath)} → {SafeFileName(preparedPath)}");

        public static void UndistortFailed(string sourcePath, string error)
            => Write($"[畸变矫正] 失败 {SafeFileName(sourcePath)}: {error}");

        public static void RecognizeStart(string action, string imagePath, string extra = null)
        {
            string tail = string.IsNullOrWhiteSpace(extra) ? "" : " " + extra.Trim();
            Write($"[算法识别] 开始{action}: {SafeFileName(imagePath)}{tail}");
        }

        public static void RecognizeDone(string action, string detail)
            => Write($"[算法识别] 完成{action}: {detail}");

        public static void RecognizeFailed(string action, string error)
            => Write($"[算法识别] 失败{action}: {error}");

        public static void PlacementPlan(string tag, string detail)
            => Write($"{tag}[放料规划] {detail}");

        static string SafeFileName(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "(空)";
            try { return Path.GetFileName(path); }
            catch { return path; }
        }
    }
}
