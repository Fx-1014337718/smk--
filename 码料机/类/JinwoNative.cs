using System;
using System.Runtime.InteropServices;
using System.Text;

namespace 码料机
{
    /// <summary>金沃 JinwoRobotArm.dll 原生结构与 P/Invoke（与 金沃dll-测试.cpp 一致）。</summary>
    public static class JinwoNative
    {
        public const int MarkerCount = 4;
        public const CallingConvention ApiConv = CallingConvention.StdCall;

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct JinwoPoint
        {
            public double X;
            public double Y;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct JinwoTrayConfig
        {
            public double CameraDistance;
            public int Rows;
            public int Cols;
            public int Layers;
            public double BearingOuterDiameter;
            public double BearingHeight;
            public double BearingGap;
            public double PitchX;
            public double PitchY;
            public double LayerPitchZ;
            public double TargetZ;
            public double BoxOuterLength;
            public double BoxOuterWidth;
            public double BoxDepth;
            public double BoxHeight;
            public double TargetRz;
            public double MarkerDistanceX;
            public double MarkerDistanceY;
            public double AutoInnerReserveX;
            public double AutoInnerReserveY;
            public double InnerOffsetX;
            public double InnerOffsetY;
            public double InnerWidth;
            public double InnerHeight;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarkerCount)]
            public JinwoPoint[] MarkerRobotPoints;

            public double FirstCenterOffsetX;
            public double FirstCenterOffsetY;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct JinwoMarkerResult
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarkerCount)]
            public JinwoPoint[] MarkerPixels;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct JinwoPoseResult
        {
            public double X;
            public double Y;
            public double Z;
            public double Rz;
            public int Row;
            public int Col;
            public int Layer;
            public int EffectiveRows;
            public int EffectiveCols;
            public int Capacity;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MarkerCount)]
            public JinwoPoint[] MarkerPixels;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Ansi)]
        public struct JinwoBearingCenterResult
        {
            public int Count;
            public int Row;
            public int Col;
            public int Layer;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string Name;

            public double TrayX;
            public double TrayY;
            public double PixelX;
            public double PixelY;
            public double RobotX;
            public double RobotY;
            public double RobotZ;
            public double RobotRz;
            public int HasRobot;
        }

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoInitConfigFn(ref JinwoTrayConfig config);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetTrayGridFn(ref JinwoTrayConfig config, int rows, int cols, int layers);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetBearingFn(ref JinwoTrayConfig config, double outerDiameter, double height, double gap, double layerPitchZ);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetPitchFn(ref JinwoTrayConfig config, double pitchX, double pitchY);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetBoxFn(ref JinwoTrayConfig config, double outerLength, double outerWidth, double depth, double height);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetCameraDistanceFn(ref JinwoTrayConfig config, double distance);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetMarkerDistanceFn(ref JinwoTrayConfig config, double distanceX, double distanceY);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetAutoInnerReserveFn(ref JinwoTrayConfig config, double reserveX, double reserveY);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetInnerRegionFn(ref JinwoTrayConfig config, double offsetX, double offsetY, double width, double height);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetRobotPlaceFn(ref JinwoTrayConfig config, double targetZ, double targetRz);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetMarkerRobotPointFn(ref JinwoTrayConfig config, int index, double robotX, double robotY);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoSetFirstCenterOffsetFn(ref JinwoTrayConfig config, double offsetX, double offsetY);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoValidateConfigFn(ref JinwoTrayConfig config);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoValidateTrayGeometryFn(ref JinwoTrayConfig config);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoDetectMarkersFromImageFn(
            [MarshalAs(UnmanagedType.LPStr)] string imagePath,
            ref JinwoMarkerResult result);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoCalculateAllBearingCentersFromImageFn(
            ref JinwoTrayConfig config,
            [MarshalAs(UnmanagedType.LPStr)] string imagePath,
            int includeRobotCoordinate,
            int saveEffectImage,
            int currentCount,
            [Out] JinwoBearingCenterResult[] centers,
            int bufferSize,
            out int centerCount,
            StringBuilder effectPath,
            int effectPathCapacity);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoGetEffectiveGridFn(ref JinwoTrayConfig config, out int effectiveRows, out int effectiveCols, out int capacity);

        [UnmanagedFunctionPointer(ApiConv)]
        public delegate int JinwoGetLastErrorFn(StringBuilder buffer, int capacity);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool SetDllDirectory(string lpPathName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeLibrary(IntPtr hModule);

        public sealed class JinwoDll : IDisposable
        {
            private IntPtr _module;

            public JinwoInitConfigFn InitConfig { get; private set; }
            public JinwoSetTrayGridFn SetTrayGrid { get; private set; }
            public JinwoSetBearingFn SetBearing { get; private set; }
            public JinwoSetPitchFn SetPitch { get; private set; }
            public JinwoSetBoxFn SetBox { get; private set; }
            public JinwoSetCameraDistanceFn SetCameraDistance { get; private set; }
            public JinwoSetMarkerDistanceFn SetMarkerDistance { get; private set; }
            public JinwoSetAutoInnerReserveFn SetAutoInnerReserve { get; private set; }
            public JinwoSetInnerRegionFn SetInnerRegion { get; private set; }
            public JinwoSetRobotPlaceFn SetRobotPlace { get; private set; }
            public JinwoSetMarkerRobotPointFn SetMarkerRobotPoint { get; private set; }
            public JinwoSetFirstCenterOffsetFn SetFirstCenterOffset { get; private set; }
            public JinwoValidateConfigFn ValidateConfig { get; private set; }
            public JinwoValidateTrayGeometryFn ValidateTrayGeometry { get; private set; }
            public JinwoDetectMarkersFromImageFn DetectMarkersFromImage { get; private set; }
            public JinwoCalculateAllBearingCentersFromImageFn CalculateAllBearingCentersFromImage { get; private set; }
            public JinwoGetEffectiveGridFn GetEffectiveGrid { get; private set; }
            public JinwoGetLastErrorFn GetLastError { get; private set; }

            public static JinwoDll Load(string dllPath, string openCvRuntimeDir)
            {
                if (!string.IsNullOrWhiteSpace(openCvRuntimeDir))
                    SetDllDirectory(openCvRuntimeDir);

                IntPtr module = LoadLibrary(dllPath);
                if (module == IntPtr.Zero)
                    throw new DllNotFoundException("无法加载 JinwoRobotArm.dll: " + dllPath);

                var dll = new JinwoDll { _module = module };
                dll.InitConfig = LoadFn<JinwoInitConfigFn>(module, "Jinwo_InitConfig");
                dll.SetTrayGrid = LoadFn<JinwoSetTrayGridFn>(module, "Jinwo_SetTrayGrid");
                dll.SetBearing = LoadFn<JinwoSetBearingFn>(module, "Jinwo_SetBearing");
                dll.SetPitch = LoadFn<JinwoSetPitchFn>(module, "Jinwo_SetPitch");
                dll.SetBox = LoadFn<JinwoSetBoxFn>(module, "Jinwo_SetBox");
                dll.SetCameraDistance = LoadFn<JinwoSetCameraDistanceFn>(module, "Jinwo_SetCameraDistance");
                dll.SetMarkerDistance = LoadFn<JinwoSetMarkerDistanceFn>(module, "Jinwo_SetMarkerDistance");
                dll.SetAutoInnerReserve = LoadFn<JinwoSetAutoInnerReserveFn>(module, "Jinwo_SetAutoInnerReserve");
                dll.SetInnerRegion = LoadFn<JinwoSetInnerRegionFn>(module, "Jinwo_SetInnerRegion");
                dll.SetRobotPlace = LoadFn<JinwoSetRobotPlaceFn>(module, "Jinwo_SetRobotPlace");
                dll.SetMarkerRobotPoint = LoadFn<JinwoSetMarkerRobotPointFn>(module, "Jinwo_SetMarkerRobotPoint");
                dll.SetFirstCenterOffset = LoadFn<JinwoSetFirstCenterOffsetFn>(module, "Jinwo_SetFirstCenterOffset");
                dll.ValidateConfig = LoadFn<JinwoValidateConfigFn>(module, "Jinwo_ValidateConfig");
                dll.ValidateTrayGeometry = TryLoadFn<JinwoValidateTrayGeometryFn>(module, "Jinwo_ValidateTrayGeometry");
                dll.DetectMarkersFromImage = LoadFn<JinwoDetectMarkersFromImageFn>(module, "Jinwo_DetectMarkersFromImage");
                dll.CalculateAllBearingCentersFromImage = LoadFn<JinwoCalculateAllBearingCentersFromImageFn>(
                    module, "Jinwo_CalculateAllBearingCentersFromImage");
                dll.GetEffectiveGrid = LoadFn<JinwoGetEffectiveGridFn>(module, "Jinwo_GetEffectiveGrid");
                dll.GetLastError = LoadFn<JinwoGetLastErrorFn>(module, "Jinwo_GetLastError");
                return dll;
            }

            private static T LoadFn<T>(IntPtr module, string name) where T : class
            {
                IntPtr addr = GetProcAddress(module, name);
                if (addr == IntPtr.Zero)
                    throw new EntryPointNotFoundException("DLL 缺少导出函数: " + name);
                return Marshal.GetDelegateForFunctionPointer<T>(addr);
            }

            private static T TryLoadFn<T>(IntPtr module, string name) where T : class
            {
                IntPtr addr = GetProcAddress(module, name);
                return addr == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer<T>(addr);
            }

            public string ReadLastError()
            {
                var buffer = new StringBuilder(1024);
                GetLastError?.Invoke(buffer, buffer.Capacity);
                string text = buffer.ToString();
                return string.IsNullOrEmpty(text) ? "DLL 未返回错误详情" : text;
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

        public static JinwoTrayConfig CreateEmptyTrayConfig()
        {
            return new JinwoTrayConfig
            {
                MarkerRobotPoints = new JinwoPoint[MarkerCount],
            };
        }

        public static JinwoMarkerResult CreateEmptyMarkerResult()
        {
            return new JinwoMarkerResult { MarkerPixels = new JinwoPoint[MarkerCount] };
        }

        public static JinwoPoseResult CreateEmptyPoseResult()
        {
            return new JinwoPoseResult { MarkerPixels = new JinwoPoint[MarkerCount] };
        }

        /// <summary>将 DLL 轴承中心结果转为放料位姿（启用机械坐标时含 Z、Rz）。</summary>
        public static JinwoPoseResult ToPoseResult(JinwoBearingCenterResult center, int effectiveRows = 0, int effectiveCols = 0, int capacity = 0)
        {
            return new JinwoPoseResult
            {
                X = center.HasRobot != 0 ? center.RobotX : center.TrayX,
                Y = center.HasRobot != 0 ? center.RobotY : center.TrayY,
                Z = center.HasRobot != 0 ? center.RobotZ : 0,
                Rz = center.HasRobot != 0 ? center.RobotRz : 0,
                Row = center.Row,
                Col = center.Col,
                Layer = center.Layer,
                EffectiveRows = effectiveRows,
                EffectiveCols = effectiveCols,
                Capacity = capacity,
                MarkerPixels = new JinwoPoint[MarkerCount],
            };
        }
    }
}
