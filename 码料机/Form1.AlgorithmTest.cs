                using System;
                using System.IO;
                using System.Text;
                using System.Threading.Tasks;
                using static 码料机.JinwoNative;

                namespace 码料机
                {
                    /// <summary>供 <see cref="AlgorithmTestForm"/> 调用的算法试跑 API（与 PLC 流程隔离）。</summary>
                    public partial class Form1
                    {
                        public sealed class AlgorithmDllStatus
                        {
                            public bool JinwoEnabled;
                            public bool JinwoLoaded;
                            public string JinwoStatus;
                            public string JinwoLoadError;
                            public bool PresenceEnabled;
                            public bool PresenceLoaded;
                            public string PresenceLoadError;
                            public string StationName;
                            public bool HasTrayConfig;
                        }

                        public sealed class BearingPresenceTestOutcome
                        {
                            public bool AlgorithmOk;
                            public bool HasDetected;
                            public int DetectCount;
                            public string Error;
                            /// <summary>本测试专用目录下的渲染图（唯一文件名）。</summary>
                            public string RenderImagePath;
                            public string Summary;
                        }

                        public sealed class JinwoMarkerTestOutcome
                        {
                            public bool Success;
                            public string Error;
                            public string RenderImagePath;
                            public string PreparedImagePath;
                            public string Summary;
                        }

                        public sealed class JinwoPoseTestOutcome
                        {
                            public bool Success;
                            public string Error;
                            public string RenderImagePath;
                            public string PreparedImagePath;
                            public JinwoPoseResult Pose;
                            public string Summary;
                        }

                        public sealed class JinwoPlanTestOutcome
                        {
                            public bool Success;
                            public string Error;
                            public string RenderImagePath;
                            public string PreparedImagePath;
                            public int CenterCount;
                            public int EffectiveRows;
                            public int EffectiveCols;
                            public int Capacity;
                            public string Summary;
                        }

                        public AlgorithmDllStatus GetAlgorithmDllStatus()
                        {
                            var st = currentStation ?? leftStation;
                            return new AlgorithmDllStatus
                            {
                                JinwoEnabled = _jinwo.IsEnabled,
                                JinwoLoaded = _jinwo.IsLoaded,
                                JinwoStatus = _jinwo.StatusText,
                                JinwoLoadError = _jinwo.LoadError,
                                PresenceEnabled = _bearingPresence.IsEnabled,
                                PresenceLoaded = _bearingPresence.IsLoaded,
                                PresenceLoadError = _bearingPresence.LoadError,
                                StationName = st?.Name ?? "(无工位)",
                                HasTrayConfig = st?.HasJinwoTrayConfig == true
                            };
                        }

                        public void ReloadAlgorithmDlls()
                        {
                            _jinwo.ReloadConfig();
                            _bearingPresence.ReloadConfig();
                            RefreshJinwoStatusUi();
                        }

                        public string GetAlgorithmTestDefaultImagePath()
                        {
                            if (!string.IsNullOrWhiteSpace(_offlineTestImagePath) && File.Exists(_offlineTestImagePath))
                                return _offlineTestImagePath;
                            string p = _jinwo.ResolveCaptureImagePath(IsLeftStation(currentStation));
                            return File.Exists(p) ? p : null;
                        }

                        public Task<bool> AlgorithmTestTryHikCaptureAsync()
                            => TryHikvisionCaptureAsync(IsLeftStation(currentStation));

                        public BearingPresenceTestOutcome TestBearingPresence(string imagePath)
                        {
                            var outcome = new BearingPresenceTestOutcome();
                            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                            {
                                outcome.Error = "图像不存在";
                                outcome.Summary = outcome.Error;
                                return outcome;
                            }
                            if (!_bearingPresence.IsEnabled)
                            {
                                outcome.Error = "有无料算法未启用（金沃算法.ini [有无料] 启用=1）";
                                outcome.Summary = outcome.Error;
                                return outcome;
                            }
                            if (!_bearingPresence.IsLoaded)
                            {
                                outcome.Error = "DLL 未加载: " + (_bearingPresence.LoadError ?? "未知");
                outcome.Summary = outcome.Error;
                return outcome;
            }

            string presenceDir = AlgorithmTestRenderPaths.GetDirectory(AlgorithmTestRenderPaths.Kind.Presence);
            if (!_bearingPresence.TryDetect(imagePath, out bool hasDetected, out int count,
                    out string dllEffectPath, out string err, presenceDir))
            {
                outcome.Error = err ?? "识别失败";
                outcome.Summary = outcome.Error;
                return outcome;
            }

            outcome.AlgorithmOk = true;
            outcome.HasDetected = hasDetected;
            outcome.DetectCount = count;
            outcome.RenderImagePath = AlgorithmTestRenderPaths.Publish(
                dllEffectPath, AlgorithmTestRenderPaths.Kind.Presence, imagePath)
                ?? dllEffectPath;
            outcome.Summary = (hasDetected
                    ? $"检测到轴承，数量={count}（放料空箱检测时视为箱内异物）"
                    : "未检测到轴承（放料空箱检测时视为箱内正常）")
                + RenderPathSuffix(outcome.RenderImagePath);
            return outcome;
        }

        public JinwoMarkerTestOutcome TestJinwoMarkers(string imagePath)
        {
            var outcome = new JinwoMarkerTestOutcome();
            if (!EnsureJinwoReady(out string readyErr))
            {
                outcome.Error = readyErr;
                outcome.Summary = readyErr;
                return outcome;
            }
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                outcome.Error = "图像不存在";
                outcome.Summary = outcome.Error;
                return outcome;
            }

            var st = currentStation ?? leftStation;
            bool isLeft = ResolveNinePointCalibIsLeft(st);

            try
            {
                if (!_jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out string prepared, out string prepErr))
                {
                    outcome.Error = prepErr ?? "图像预处理失败";
                    outcome.Summary = outcome.Error;
                    return outcome;
                }
                outcome.PreparedImagePath = prepared;

                if (!_jinwo.TryDetectMarkers(imagePath, isLeft, out JinwoMarkerResult markers, out string markerErr))
                {
                    outcome.Error = markerErr ?? "黑圆检测失败";
                    outcome.Summary = outcome.Error;
                    return outcome;
                }

                var sb = new StringBuilder();
                sb.AppendLine("黑圆检测成功");
                if (markers.MarkerPixels != null)
                {
                    for (int i = 0; i < markers.MarkerPixels.Length; i++)
                    {
                        var p = markers.MarkerPixels[i];
                        sb.AppendLine($"  黑圆{i}: x={p.X:F1}, y={p.Y:F1}");
                    }
                }
                string renderFile = AlgorithmTestRenderPaths.AllocateFile(
                    AlgorithmTestRenderPaths.Kind.JinwoMarkers, imagePath, ".bmp");
                outcome.RenderImagePath = JinwoImagePreview.DrawMarkersOverlay(
                    prepared, markers, Path.GetDirectoryName(renderFile), renderFile);
                outcome.Success = !string.IsNullOrEmpty(outcome.RenderImagePath);
                if (!outcome.Success)
                {
                    outcome.Error = "黑圆渲染图生成失败";
                    outcome.Summary = outcome.Error;
                    return outcome;
                }
                outcome.Summary = sb.ToString().TrimEnd() + RenderPathSuffix(outcome.RenderImagePath);
                return outcome;
            }
            catch (Exception ex)
            {
                outcome.Error = ex.Message;
                outcome.Summary = ex.Message;
                return outcome;
            }
        }

        public JinwoPoseTestOutcome TestJinwoPose(string imagePath, int placedCount)
        {
            var outcome = new JinwoPoseTestOutcome();
            if (!TryGetStationForAlgorithmTest(out StationData st, out string stationErr))
            {
                outcome.Error = stationErr;
                outcome.Summary = stationErr;
                return outcome;
            }
            if (!EnsureJinwoReady(out string readyErr))
            {
                outcome.Error = readyErr;
                outcome.Summary = readyErr;
                return outcome;
            }
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                outcome.Error = "图像不存在";
                outcome.Summary = outcome.Error;
                return outcome;
            }

            try
            {
                bool isLeft = IsLeftStation(st);
                if (!_jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out string prepared, out string prepErr))
                {
                    outcome.Error = prepErr ?? "图像预处理失败";
                    outcome.Summary = outcome.Error;
                    return outcome;
                }
                outcome.PreparedImagePath = prepared;

                var cfg = st.JinwoTray;
                var pose = _jinwo.CalculatePose(ref cfg, imagePath, placedCount, ResolveNinePointCalibIsLeft(st), out string effectPath, forceSaveEffectImage: true);
                ApplyConfiguredJinwoZAndRz(st, ref pose);
                outcome.Pose = pose;
                string dllEffect = ResolveJinwoDllEffectPath(effectPath, isLeft);
                outcome.RenderImagePath = AlgorithmTestRenderPaths.Publish(
                    dllEffect, AlgorithmTestRenderPaths.Kind.JinwoPose, imagePath);
                outcome.Success = true;
                outcome.Summary =
                    $"工位 {st.Name} | 已放件数={placedCount}\r\n" +
                    $"世界坐标 X={pose.X:F2} Y={pose.Y:F2} Z={pose.Z:F2} Rz={pose.Rz:F2}°\r\n" +
                    $"层{pose.Layer + 1} / 行{pose.Row + 1} / 列{pose.Col + 1}"
                    + RenderPathSuffix(outcome.RenderImagePath);
                return outcome;
            }
            catch (Exception ex)
            {
                outcome.Error = ex.Message;
                outcome.Summary = ex.Message;
                return outcome;
            }
        }

        public JinwoPlanTestOutcome TestJinwoAllCenters(string imagePath)
        {
            var outcome = new JinwoPlanTestOutcome();
            if (!TryGetStationForAlgorithmTest(out StationData st, out string stationErr))
            {
                outcome.Error = stationErr;
                outcome.Summary = stationErr;
                return outcome;
            }
            if (!EnsureJinwoReady(out string readyErr))
            {
                outcome.Error = readyErr;
                outcome.Summary = readyErr;
                return outcome;
            }
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                outcome.Error = "图像不存在";
                outcome.Summary = outcome.Error;
                return outcome;
            }

            try
            {
                bool isLeft = IsLeftStation(st);
                if (!_jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out string prepared, out string prepErr))
                {
                    outcome.Error = prepErr ?? "图像预处理失败";
                    outcome.Summary = outcome.Error;
                    return outcome;
                }
                outcome.PreparedImagePath = prepared;

                var cfg = st.JinwoTray;
                var centers = _jinwo.CalculateAllBearingCenters(ref cfg, imagePath, 0, ResolveNinePointCalibIsLeft(st), out string effectPath, forceSaveEffectImage: true);
                _jinwo.TryGetEffectiveGrid(ref cfg, out int effRows, out int effCols, out int capacity);
                JinwoPlacementOrder.SortCenters(centers, effRows > 0 ? effRows : st.MaxRows, effCols > 0 ? effCols : st.MaxCols);
                outcome.CenterCount = centers?.Length ?? 0;
                outcome.EffectiveRows = effRows;
                outcome.EffectiveCols = effCols;
                outcome.Capacity = capacity;
                string dllEffect = ResolveJinwoDllEffectPath(effectPath, isLeft);
                outcome.RenderImagePath = AlgorithmTestRenderPaths.Publish(
                    dllEffect, AlgorithmTestRenderPaths.Kind.JinwoPlan, imagePath);
                outcome.Success = outcome.CenterCount > 0;
                if (!outcome.Success)
                {
                    outcome.Error = "未返回轴承中心点";
                    outcome.Summary = outcome.Error;
                    return outcome;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"工位 {st.Name} | 中心点 {outcome.CenterCount} 个");
                sb.AppendLine($"有效网格 {effRows} 行 × {effCols} 列，容量 {capacity}");
                int show = Math.Min(centers.Length, 12);
                for (int i = 0; i < show; i++)
                {
                    var c = centers[i];
                    sb.AppendLine($"  [{c.Count}] 像素({c.PixelX:F1},{c.PixelY:F1})" +
                        (c.HasRobot != 0 ? $" 机械({c.RobotX:F2},{c.RobotY:F2},{c.RobotZ:F2},Rz={c.RobotRz:F2})" : ""));
                }
                if (centers.Length > show)
                    sb.AppendLine($"  … 另有 {centers.Length - show} 个点");
                outcome.Summary = sb.ToString().TrimEnd() + RenderPathSuffix(outcome.RenderImagePath);
                return outcome;
            }
            catch (Exception ex)
            {
                outcome.Error = ex.Message;
                outcome.Summary = ex.Message;
                return outcome;
            }
        }

        private bool EnsureJinwoReady(out string error)
        {
            if (!_jinwo.IsEnabled)
            {
                error = "金沃算法未启用";
                return false;
            }
            if (!_jinwo.IsLoaded)
            {
                error = "金沃 DLL 未加载: " + (_jinwo.LoadError ?? "未知");
                return false;
            }
            error = null;
            return true;
        }

        private bool TryGetStationForAlgorithmTest(out StationData st, out string error)
        {
            st = currentStation ?? leftStation;
            if (st == null)
            {
                error = "无工位数据";
                return false;
            }
            if (!st.HasJinwoTrayConfig)
            {
                error = $"请先在主界面对 {st.Name} 点击「确认产品与数量」，以生成金沃托盘配置";
                return false;
            }
            error = null;
            return true;
        }

        private string ResolveJinwoDllEffectPath(string rawEffectPath, bool isLeft = true)
        {
            string resolved = _jinwo.ResolveEffectImagePath(rawEffectPath, isLeft);
            if (!string.IsNullOrEmpty(resolved) && File.Exists(resolved))
                return resolved;
            return _jinwo.FindNewestEffectImage(isLeft);
        }

        private static string RenderPathSuffix(string renderPath)
        {
            if (string.IsNullOrWhiteSpace(renderPath)) return "\r\n渲染图: （未生成）";
            return "\r\n渲染图: " + renderPath;
        }
    }
}
