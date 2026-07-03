using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>界面日志与 log 目录落盘文件的导出。</summary>
    internal static class LogExportHelper
    {
        public const string ExportFilter =
            "界面日志 (*.txt)|*.txt|磁盘日志合并 (*.txt)|*.txt|所有文件|*.*";

        public static readonly string[] KnownLogFiles =
        {
            "PlcSend.log",
            "PlcReceive.log",
            "ImageProcess.log",
        };

        public static string LogDirectory =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");

        public static bool TryExport(IWin32Window owner, IList uiItems)
        {
            using (var dlg = new SaveFileDialog
            {
                Title = "导出日志",
                Filter = ExportFilter,
                FilterIndex = 1,
                FileName = BuildDefaultFileName("界面日志"),
                AddExtension = true,
                DefaultExt = "txt",
                InitialDirectory = Directory.Exists(LogDirectory) ? LogDirectory : null,
            })
            {
                if (dlg.ShowDialog(owner) != DialogResult.OK)
                    return false;

                try
                {
                    if (dlg.FilterIndex == 2)
                        ExportDiskLogsToPath(dlg.FileName);
                    else
                    {
                        if (uiItems == null || uiItems.Count == 0)
                        {
                            DialogPrompts.ShowInfo("当前没有可导出的界面日志。", "导出日志");
                            return false;
                        }
                        ExportUiLogToPath(uiItems, dlg.FileName);
                    }

                    DialogPrompts.ShowInfo("日志已保存至：\n" + dlg.FileName, "导出日志");
                    return true;
                }
                catch (Exception ex)
                {
                    DialogPrompts.ShowError("导出失败：\n" + ex.Message, "导出日志");
                    return false;
                }
            }
        }

        static void ExportUiLogToPath(IList items, string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"===== 码料机界面日志 {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
            sb.AppendLine($"共 {items.Count} 条（时间顺序：旧 → 新）");
            sb.AppendLine();

            for (int i = items.Count - 1; i >= 0; i--)
            {
                string line = items[i]?.ToString();
                if (!string.IsNullOrEmpty(line))
                    sb.AppendLine(line);
            }

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        static void ExportDiskLogsToPath(string path)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"===== 码料机磁盘日志 {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====");
            sb.AppendLine($"源目录: {LogDirectory}");
            sb.AppendLine();

            var written = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AppendLogFileSection(sb, written);

            if (Directory.Exists(LogDirectory))
            {
                foreach (string file in Directory.GetFiles(LogDirectory, "*.log").OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase))
                {
                    string name = Path.GetFileName(file);
                    if (written.Contains(name))
                        continue;
                    AppendFileSection(sb, name, file);
                }
            }

            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        static void AppendLogFileSection(StringBuilder sb, System.Collections.Generic.HashSet<string> written)
        {
            foreach (string name in KnownLogFiles)
            {
                string full = Path.Combine(LogDirectory, name);
                AppendFileSection(sb, name, full);
                written.Add(name);
            }
        }

        static void AppendFileSection(StringBuilder sb, string name, string fullPath)
        {
            sb.AppendLine($"---------- {name} ----------");
            if (File.Exists(fullPath))
                sb.Append(File.ReadAllText(fullPath, Encoding.UTF8));
            else
                sb.AppendLine("(文件不存在)");
            if (sb.Length > 0 && sb[sb.Length - 1] != '\n')
                sb.AppendLine();
            sb.AppendLine();
        }

        static string BuildDefaultFileName(string baseName)
        {
            string safe = string.Join("_", (baseName ?? "日志").Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safe))
                safe = "日志";
            return $"{safe}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }
    }
}
