using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using VM.Core;
using VM.PlatformSDKCS;
using VMControls.Interface;

namespace 码料机
{
    /// <summary>
    /// VisionMaster 方案封装：加载 .sol、运行流程、读取「输出设置」、定位「格式化1」供渲染控件绑定。
    /// 相机与采图在方案内完成，应用侧不送图、不取 Bitmap。
    /// </summary>
    public static class VMSol
    {
        public const string DefaultSolutionFileName = "码料机.sol";
        public const string DefaultProcedureName = "木箱定位";
        /// <summary>取料圆心识别流程（与 .sol 内流程名一致）。</summary>
        public const string DefaultPickProcedureName = "取料位定位";

        // 与方案「输出设置」订阅名一致
        public const string OutputTopLeft = "Top-left";
        public const string OutputLineAngle = "LineAngle";
        public const string OutputFormattedText = "stringTEX";

        private static readonly string[] LineAngleAliases = { OutputLineAngle, "Line Angle", "线角度", "箱体角度" };
        private static readonly string[] TopLeftAliases = { OutputTopLeft, "TopLeft", "左上角" };
        private static readonly string[] FormattedTextAliases = { OutputFormattedText, "stringTEXT", "格式化", "FormatText", "TEXT" };
        private static readonly string[] PickCenterAliases =
        {
            "PickCenter", "取料圆心", "圆心", "Center", "MatchPoint", "匹配点", "WorldPoint", "机械坐标",
        };

        /// <summary>渲染控件绑定的模块名（与 VM 方案工具名一致）。</summary>
        public const string FormatModuleName = "格式化1";

        /// <summary>离线采图默认文件名（exe 同目录，供金沃与 VM 本地图模式共用）。</summary>
        public const string DefaultOfflineFeedFileName = "Feed.bmp";

        /// <summary>VM 图像源：单张本地图（与方案内「图像源」离线模式一致）。</summary>
        public const int ImageSourceTypeLocalFile = 1;

        private static readonly string[] ImageSourceModuleNames =
        {
            "图像源1", "图像源", "ImageSource1", "Image Source1", "Image Source 1",
        };

        private static readonly object SdkInitLock = new object();
        private static bool SdkInitAttempted;

        /// <summary>流程 Run 结束后解析的工位显示项（偏差方案未配置）。</summary>
        public sealed class BoxPlacementOutputs
        {
            public string TopLeftText = "—";
            public string AngleText = "—";
            public string FormattedText;
            public bool HasTopLeft;
            public bool HasAngle;
            public float TopLeftX, TopLeftY, AngleDeg;
        }

        private static string ResolveProcedureName(string procedureName) =>
            string.IsNullOrWhiteSpace(procedureName) ? DefaultProcedureName : procedureName.Trim();

        public static string GetDefaultSolutionPath() =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DefaultSolutionFileName);

        public static bool DefaultSolutionFileExists() => File.Exists(GetDefaultSolutionPath());

        /// <summary>加载 exe 同目录方案（需已安装 VM 运行库）。</summary>
        public static void Load(string solutionPath = null, string password = null)
        {
            EnsureVisionSdkRuntimeInitialized();
            string path = string.IsNullOrWhiteSpace(solutionPath) ? GetDefaultSolutionPath() : Path.GetFullPath(solutionPath);
            if (!File.Exists(path))
                throw new FileNotFoundException(path);
            VmSolution.Load(path, password ?? string.Empty);
        }

        public static void EnsureVisionSdkRuntimeInitialized()
        {
            lock (SdkInitLock)
            {
                if (SdkInitAttempted) return;
                SdkInitAttempted = true;
                TryInvokeStaticNoArg(typeof(VmSolution), "Init", "Initialize", "SDKInit", "VMInit");
                Type global = typeof(VmSolution).Assembly.GetType("VM.PlatformSDKCS.VMGlobal", false, true)
                    ?? typeof(VmSolution).Assembly.GetType("VMGlobal", false, true);
                if (global != null)
                    TryInvokeStaticNoArg(global, "Init", "Initialize", "InitSDK");
            }
        }

        public static void ReleaseLoadedSolution()
        {
            try
            {
                if (VmSolution.Instance == null) return;
                try { VmSolution.Instance.CloseSolution(); } catch { }
                try { VmSolution.Instance.Dispose(); } catch { }
                TryInvokeStaticNoArg(typeof(VmSolution), "Destroy", "UnInit", "Release");
            }
            catch (Exception ex) { Debug.WriteLine("VMSol.Release: " + ex.Message); }
        }

        public static List<string> ListProcedureNames()
        {
            var names = new List<string>();
            if (VmSolution.Instance == null) return names;
            try
            {
                object list = VmSolution.Instance.GetType()
                    .GetMethod("GetProcedureList", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null)
                    ?.Invoke(VmSolution.Instance, null);
                if (list is IEnumerable en)
                {
                    foreach (object item in en)
                    {
                        string n = item as string ?? item?.ToString();
                        if (!string.IsNullOrWhiteSpace(n)) names.Add(n.Trim());
                    }
                    if (names.Count > 0) return names;
                }
            }
            catch { }

            foreach (string key in new[] { DefaultProcedureName, "流程1", "Process1" })
            {
                try
                {
                    if (VmSolution.Instance[key] != null && !names.Contains(key))
                        names.Add(key);
                }
                catch { }
            }
            return names;
        }

        public static VmProcedure GetProcedure(string procedureName = null)
        {
            string name = ResolveProcedureName(procedureName);
            if (VmSolution.Instance == null)
                throw new InvalidOperationException("VM 未加载");
            object obj = VmSolution.Instance[name];
            if (obj is VmProcedure p) return p;
            if (obj == null) throw new InvalidOperationException("VM 无流程:" + name);
            throw new InvalidOperationException("VM 流程类型错误:" + name);
        }

        /// <summary>取流程内「格式化1」模块，供 VmRenderControl 显示渲染图。</summary>
        public static bool TryGetProcedureFormatRenderModule(string procedureName, out IVmModule module, out string displayName)
        {
            module = null;
            displayName = FormatModuleName;
            try
            {
                VmProcedure proc = GetProcedure(procedureName);
                return TryBindModule(proc[FormatModuleName], FormatModuleName, ref module, ref displayName);
            }
            catch { return false; }
        }

        /// <summary>读取流程「输出设置」：Top-left、LineAngle、stringTEX。</summary>
        public static bool TryReadBoxPlacementOutputs(string procedureName, out BoxPlacementOutputs outputs)
        {
            outputs = new BoxPlacementOutputs();
            VmProcedure proc;
            try { proc = GetProcedure(procedureName); }
            catch { return false; }

            TryRefreshProcedureModuResult(proc);

            if (TryReadPoint(proc, TopLeftAliases, out float px, out float py))
            {
                outputs.HasTopLeft = true;
                outputs.TopLeftX = px;
                outputs.TopLeftY = py;
                outputs.TopLeftText = $"{px:F2}, {py:F2}";
            }
            else if (TryReadString(proc, TopLeftAliases, out string topStr) && !string.IsNullOrWhiteSpace(topStr))
                outputs.TopLeftText = topStr.Trim();

            if (TryReadFloat(proc, LineAngleAliases, out float ang))
            {
                outputs.HasAngle = true;
                outputs.AngleDeg = ang;
                outputs.AngleText = $"{ang:F2}°";
            }
            else if (TryReadString(proc, LineAngleAliases, out string angStr) && !string.IsNullOrWhiteSpace(angStr))
                outputs.AngleText = angStr.Trim();

            if (TryReadString(proc, FormattedTextAliases, out string fmt) && !string.IsNullOrWhiteSpace(fmt))
                outputs.FormattedText = fmt.Trim();

            return outputs.HasTopLeft || outputs.HasAngle || !string.IsNullOrEmpty(outputs.FormattedText)
                || outputs.TopLeftText != "—" || outputs.AngleText != "—";
        }

        /// <summary>读取取料流程「输出设置」中的圆心/匹配点（X、Y，mm）。</summary>
        public static bool TryReadPickCenterOutputs(string procedureName, out float x, out float y)
        {
            x = y = 0f;
            VmProcedure proc;
            try { proc = GetProcedure(procedureName); }
            catch { return false; }

            TryRefreshProcedureModuResult(proc);
            if (TryReadPoint(proc, PickCenterAliases, out x, out y))
                return true;
            return TryReadPoint(proc, TopLeftAliases, out x, out y);
        }

        /// <summary>将用户选择的图片落盘为 Feed.bmp，并写入 vm_vision.ini（金沃采图路径回退）。</summary>
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

            string vmIni = Path.Combine(Parameters.IniDir, "vm_vision.ini");
            Directory.CreateDirectory(Parameters.IniDir);
            if (!File.Exists(vmIni))
                File.WriteAllText(vmIni, "[LocalImage]\r\nFolder=\r\nFileName=Feed.bmp\r\n", System.Text.Encoding.Default);
            IniAPI.INIWriteValue(vmIni, "LocalImage", "Folder", Path.GetDirectoryName(feedPath) ?? "");
            IniAPI.INIWriteValue(vmIni, "LocalImage", "FileName", Path.GetFileName(feedPath));
            return Path.GetFullPath(feedPath);
        }

        /// <summary>无相机时：把本地图注入流程「图像源」并切换为单张本地图模式。</summary>
        public static bool TryInjectLocalImage(string procedureName, string imagePath, out string detail)
        {
            detail = "";
            try
            {
                if (VmSolution.Instance == null)
                {
                    detail = "VM 未加载";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                {
                    detail = "图片不存在";
                    return false;
                }

                VmProcedure proc = GetProcedure(procedureName);
                if (!TryFindImageSourceModule(proc, out VmModule imgMod, out string modName))
                {
                    detail = "未找到图像源模块（请确认方案中存在「图像源1」）";
                    return false;
                }

                string fullPath = Path.GetFullPath(imagePath);
                var param = new CModuleParamBase((uint)imgMod.ID);
                param.SetParamValue("ImageSourceType", ImageSourceTypeLocalFile.ToString());
                param.SetParamValue("CurrentImagePath", fullPath);
                detail = modName + " ← " + Path.GetFileName(fullPath);
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return false;
            }
        }

        /// <summary>运行流程（触发/相机在 .sol 内配置）。</summary>
        public static bool TryRunProcedure(string procedureName, out string detail)
        {
            detail = "";
            try
            {
                VmProcedure proc = GetProcedure(procedureName);
                proc.Run();
                TryRefreshProcedureModuResult(proc);
                detail = "「" + ResolveProcedureName(procedureName) + "」已运行";
                return true;
            }
            catch (Exception ex)
            {
                detail = ex.Message;
                return false;
            }
        }

        #region 内部：模块绑定与 ModuResult 读取

        private static bool TryFindImageSourceModule(VmProcedure proc, out VmModule module, out string displayName)
        {
            module = null;
            displayName = null;
            if (proc == null) return false;

            foreach (string name in ImageSourceModuleNames)
            {
                try
                {
                    if (proc[name] is VmModule vm)
                    {
                        module = vm;
                        displayName = name;
                        return true;
                    }
                }
                catch { }
            }

            try
            {
                object modulesObj = proc.GetType().GetProperty("Modules", BindingFlags.Public | BindingFlags.Instance)?.GetValue(proc);
                if (modulesObj is IEnumerable en)
                {
                    foreach (object item in en)
                    {
                        if (!(item is VmModule vm)) continue;
                        string n = vm.Name ?? vm.StrModuleName ?? "";
                        if (n.IndexOf("图像源", StringComparison.OrdinalIgnoreCase) >= 0
                            || n.IndexOf("ImageSource", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            module = vm;
                            displayName = n;
                            return true;
                        }
                    }
                }
            }
            catch { }

            return false;
        }

        private static bool TryBindModule(object obj, string name, ref IVmModule module, ref string displayName)
        {
            if (obj is IVmModule ivm)
            {
                module = ivm;
                displayName = name;
                return true;
            }
            return false;
        }

        private static void TryRefreshProcedureModuResult(VmProcedure proc)
        {
            if (proc == null) return;
            try { proc.GetType().GetMethod("SyncModuResult", BindingFlags.Public | BindingFlags.Instance)?.Invoke(proc, null); }
            catch { }
        }

        private static bool TryReadPoint(VmProcedure proc, IEnumerable<string> names, out float x, out float y)
        {
            x = y = 0f;
            var modu = proc?.ModuResult;
            if (modu == null) return false;
            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                try
                {
                    var pts = modu.GetOutputPointArray(name);
                    if (pts == null || pts.Count == 0) continue;
                    x = pts[0].X;
                    y = pts[0].Y;
                    return true;
                }
                catch { }
            }
            return false;
        }

        private static bool TryReadFloat(VmProcedure proc, IEnumerable<string> names, out float value)
        {
            value = 0f;
            if (proc?.ModuResult == null) return false;
            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                try
                {
                    FloatDataArray arr = proc.ModuResult.GetOutputFloat(name);
                    if (arr.nValueNum > 0 && arr.pFloatVal != null && arr.pFloatVal.Length > 0)
                    {
                        value = arr.pFloatVal[0];
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        private static bool TryReadString(VmProcedure proc, IEnumerable<string> names, out string value)
        {
            value = null;
            if (proc?.ModuResult == null) return false;
            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name)) continue;
                try
                {
                    StringDataArray arr = proc.ModuResult.GetOutputString(name);
                    if (arr.astStringVal != null && arr.nValueNum > 0
                        && !string.IsNullOrWhiteSpace(arr.astStringVal[0].strValue))
                    {
                        value = arr.astStringVal[0].strValue;
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        private static void TryInvokeStaticNoArg(Type type, params string[] methodNames)
        {
            if (type == null) return;
            foreach (string name in methodNames)
            {
                try
                {
                    MethodInfo m = type.GetMethod(name, BindingFlags.Public | BindingFlags.Static, null, Type.EmptyTypes, null);
                    m?.Invoke(null, null);
                }
                catch { }
            }
        }

        #endregion
    }
}
