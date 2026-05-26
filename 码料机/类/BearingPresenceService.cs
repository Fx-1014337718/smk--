using System;
using System.IO;

namespace 码料机
{
    /// <summary>入料位有无轴承识别（判断有无轴承.dll）。</summary>
    public sealed class BearingPresenceService : IDisposable
    {
        private readonly object _sync = new object();
        private BearingPresenceNative.BearingPresenceDll _dll;
        private BearingPresenceConfig _cfg;
        private string _loadError;

        public bool IsEnabled => _cfg?.Enabled == true;
        public bool IsLoaded => _dll != null;
        public string LoadError => _loadError;

        public void ReloadConfig()
        {
            _cfg = BearingPresenceConfig.Load();
            if (!_cfg.Enabled)
            {
                Unload();
                return;
            }
            TryLoad();
        }

        private void TryLoad()
        {
            Unload();
            try
            {
                string path = _cfg.ResolveDllPath();
                if (!File.Exists(path))
                {
                    _loadError = "未找到 DLL: " + path;
                    return;
                }
                _dll = BearingPresenceNative.BearingPresenceDll.Load(path, _cfg.ResolveOpenCvRuntimeDir());
                _loadError = null;
            }
            catch (Exception ex)
            {
                _loadError = ex.Message;
                _dll = null;
            }
        }

        private void Unload()
        {
            _dll?.Dispose();
            _dll = null;
        }

        /// <summary>
        /// 识别单张图。成功时 <paramref name="hasDetected"/> / <paramref name="detectCount"/> 有效；
        /// 失败时 <paramref name="error"/> 有说明。
        /// </summary>
        public bool TryDetect(string imagePath, out bool hasDetected, out int detectCount,
            out string effectImagePath, out string error)
            => TryDetect(imagePath, out hasDetected, out detectCount, out effectImagePath, out error, null);

        /// <param name="effectOutputDir">非空时效果图写入该目录（算法测试用独立目录）。</param>
        public bool TryDetect(string imagePath, out bool hasDetected, out int detectCount,
            out string effectImagePath, out string error, string effectOutputDir)
        {
            hasDetected = false;
            detectCount = 0;
            effectImagePath = null;
            error = null;

            if (_dll == null)
            {
                error = _loadError ?? "有无料 DLL 未加载";
                return false;
            }
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                error = "采图不存在: " + (imagePath ?? "(空)");
                return false;
            }

            string outDir = string.IsNullOrWhiteSpace(effectOutputDir)
                ? _cfg.ResolveEffectImageDir()
                : effectOutputDir;
            Directory.CreateDirectory(outDir);
            lock (_sync)
            {
                int code;
                if (_dll.ProcessImage != null)
                    code = _dll.ProcessImage(imagePath, outDir);
                else
                    code = _dll.Run(imagePath, outDir);

                if (!BearingPresenceNative.TryParseResult(code, out hasDetected, out detectCount, out error))
                    return false;

                string baseName = Path.GetFileNameWithoutExtension(imagePath);
                string tested = Path.Combine(outDir, baseName + "_tested.jpg");
                if (File.Exists(tested))
                    effectImagePath = tested;
            }
            return true;
        }

        public void Dispose() => Unload();
    }
}
