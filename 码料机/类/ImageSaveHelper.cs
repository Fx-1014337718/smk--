using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>预览区「保存图片」：弹出另存为对话框并落盘。</summary>
    internal static class ImageSaveHelper
    {
        public const string SaveFilter = "PNG 图像|*.png|BMP 图像|*.bmp|JPEG 图像|*.jpg;*.jpeg|所有文件|*.*";

        public static bool TrySaveImage(IWin32Window owner, Image image, string suggestedBaseName = "image")
        {
            if (image == null)
            {
                DialogPrompts.ShowInfo("当前没有可保存的图像。", "保存图片");
                return false;
            }

            using (var dlg = CreateSaveDialog(suggestedBaseName))
            {
                if (dlg.ShowDialog(owner) != DialogResult.OK)
                    return false;
                return SaveToPath(image, dlg.FileName);
            }
        }

        public static bool TrySaveImageFromPath(IWin32Window owner, string sourcePath, string suggestedBaseName = null)
        {
            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                DialogPrompts.ShowInfo("当前没有可保存的图像。", "保存图片");
                return false;
            }

            suggestedBaseName = suggestedBaseName ?? Path.GetFileNameWithoutExtension(sourcePath);
            using (var dlg = CreateSaveDialog(suggestedBaseName))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(sourcePath);
                string ext = Path.GetExtension(sourcePath);
                if (!string.IsNullOrEmpty(ext))
                    dlg.FileName = Path.GetFileNameWithoutExtension(dlg.FileName) + ext;
                if (dlg.ShowDialog(owner) != DialogResult.OK)
                    return false;
                try
                {
                    File.Copy(sourcePath, dlg.FileName, overwrite: true);
                    return true;
                }
                catch (Exception ex)
                {
                    DialogPrompts.ShowError("保存失败：\n" + ex.Message, "保存图片");
                    return false;
                }
            }
        }

        private static SaveFileDialog CreateSaveDialog(string suggestedBaseName)
        {
            return new SaveFileDialog
            {
                Title = "保存图片",
                Filter = SaveFilter,
                FileName = BuildDefaultFileName(suggestedBaseName),
                AddExtension = true,
                DefaultExt = "png",
            };
        }

        private static bool SaveToPath(Image image, string path)
        {
            try
            {
                string ext = Path.GetExtension(path)?.ToLowerInvariant();
                ImageFormat format = ImageFormat.Png;
                if (ext == ".bmp")
                    format = ImageFormat.Bmp;
                else if (ext == ".jpg" || ext == ".jpeg")
                    format = ImageFormat.Jpeg;
                image.Save(path, format);
                return true;
            }
            catch (Exception ex)
            {
                DialogPrompts.ShowError("保存失败：\n" + ex.Message, "保存图片");
                return false;
            }
        }

        private static string BuildDefaultFileName(string baseName)
        {
            string safe = string.Join("_", (baseName ?? "image").Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(safe))
                safe = "image";
            return $"{safe}_{DateTime.Now:yyyyMMdd_HHmmss}";
        }
    }
}
