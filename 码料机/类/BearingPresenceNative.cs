using System;
using System.IO;
using System.Runtime.InteropServices;

namespace 码料机
{
    /// <summary>判断有无轴承.dll（F_ProcessImage / F_Run，cdecl + Unicode 路径）。</summary>
    public static class BearingPresenceNative
    {
        public const CallingConvention ApiConv = CallingConvention.Cdecl;

        [UnmanagedFunctionPointer(ApiConv, CharSet = CharSet.Unicode)]
        public delegate int FProcessImageFn([MarshalAs(UnmanagedType.LPWStr)] string imagePath,
            [MarshalAs(UnmanagedType.LPWStr)] string outputDir);

        [UnmanagedFunctionPointer(ApiConv, CharSet = CharSet.Unicode)]
        public delegate int FRunFn([MarshalAs(UnmanagedType.LPWStr)] string inputPath,
            [MarshalAs(UnmanagedType.LPWStr)] string outputDir);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        public sealed class BearingPresenceDll : IDisposable
        {
            private IntPtr _module;

            public FProcessImageFn ProcessImage { get; private set; }
            public FRunFn Run { get; private set; }

            public static BearingPresenceDll Load(string dllPath, string dependencyDir)
            {
                if (!string.IsNullOrWhiteSpace(dependencyDir))
                    SetDllDirectory(dependencyDir);

                IntPtr module = LoadLibrary(dllPath);
                if (module == IntPtr.Zero)
                    throw new DllNotFoundException("无法加载判断有无轴承.dll: " + dllPath);

                var dll = new BearingPresenceDll { _module = module };
                dll.ProcessImage = LoadFn<FProcessImageFn>(module, "F_ProcessImage");
                dll.Run = TryLoadFn<FRunFn>(module, "F_Run");
                if (dll.ProcessImage == null && dll.Run == null)
                    throw new EntryPointNotFoundException("DLL 缺少 F_ProcessImage / F_Run: " + dllPath);
                return dll;
            }

            private static T LoadFn<T>(IntPtr module, string name) where T : class
            {
                IntPtr addr = GetProcAddress(module, name);
                if (addr == IntPtr.Zero)
                    return null;
                return Marshal.GetDelegateForFunctionPointer<T>(addr);
            }

            private static T TryLoadFn<T>(IntPtr module, string name) where T : class
            {
                IntPtr addr = GetProcAddress(module, name);
                return addr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(addr);
            }

            public void Dispose()
            {
                if (_module != IntPtr.Zero)
                {
                    FreeLibrary(_module);
                    _module = IntPtr.Zero;
                }
            }
        }

        /// <summary>解析返回值：&gt;0 检测到目标（放料场景视为异物），0 未检测到，&lt;0 失败。</summary>
        public static bool TryParseResult(int code, out bool hasDetected, out int detectCount, out string error)
        {
            detectCount = 0;
            hasDetected = false;
            if (code > 0)
            {
                hasDetected = true;
                detectCount = code;
                error = null;
                return true;
            }
            if (code == 0)
            {
                error = null;
                return true;
            }
            if (code == -10) error = "图像路径为空";
            else if (code == -1) error = "无图像或读图失败";
            else if (code == -2) error = "定位点检测失败";
            else if (code == -100) error = "DLL 内部异常";
            else error = "识别失败，错误码 " + code;
            return false;
        }
    }
}
