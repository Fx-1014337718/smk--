using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace 码料机
{
    /// <summary>
    /// 不连接 PLC 的离线拟运行。
    /// 启动：码料机.exe --offline-sim [--station B] [--packing 0|1|both] [--out-dir 路径] [--right 图]
    /// </summary>
    internal static class OfflineJinwoSim
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        public static int Run(string[] args)
        {
            AllocConsole();
            Console.OutputEncoding = Encoding.UTF8;

            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            Directory.SetCurrentDirectory(exeDir);
            string logDir = Path.Combine(exeDir, "log");
            Directory.CreateDirectory(logDir);
            string logPath = Path.Combine(logDir, $"offline_sim_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            void Log(string line)
            {
                Console.WriteLine(line);
                File.AppendAllText(logPath, line + Environment.NewLine, Encoding.UTF8);
            }

            Log("=== 离线拟运行（不连接 PLC）===");
            Log("工作目录: " + exeDir);
            Log("日志: " + logPath);

            try
            {
                DisablePlc(Path.Combine(Parameters.IniDir, "PLC配置.ini"), Log);

                string stationArg = ResolveArgOrDefault(args, "--station", "B").Trim().ToUpperInvariant();
                bool runLeft = stationArg == "A" || stationArg == "LEFT" || stationArg == "L" || stationArg == "BOTH";
                bool runRight = stationArg == "B" || stationArg == "RIGHT" || stationArg == "R" || stationArg == "BOTH";
                if (!runLeft && !runRight)
                    runRight = true;

                string leftImg = ResolveArgOrDefault(args, "--left",
                    Path.Combine(Parameters.IniDir, "工位采图", "左机台_last.bmp"));
                string rightImg = ResolveArgOrDefault(args, "--right",
                    Path.Combine(Parameters.IniDir, "工位采图", "右机台_last.bmp"));

                string outDir = ResolveArgOrDefault(args, "--out-dir",
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "拟运行"));
                Directory.CreateDirectory(outDir);
                Log("输出目录: " + outDir);

                string packingArg = ResolveArgOrDefault(args, "--packing", "both").Trim().ToLowerInvariant();
                int[] modes;
                if (packingArg == "both" || packingArg == "all")
                    modes = new[] { 0, 1 };
                else
                {
                    int.TryParse(packingArg, out int one);
                    modes = new[] { one == 1 ? 1 : 0 };
                }

                using (var jinwo = new JinwoPlacementService())
                {
                    jinwo.ReloadConfig();
                    Log($"金沃: Enabled={jinwo.IsEnabled} Loaded={jinwo.IsLoaded} Status={jinwo.StatusText}");
                    if (!string.IsNullOrEmpty(jinwo.LoadError))
                        Log("加载错误: " + jinwo.LoadError);
                    if (!jinwo.IsEnabled || !jinwo.IsLoaded)
                        return 2;

                    foreach (int packingMode in modes)
                    {
                        Log($"排料方式: {StackingPlacement.DescribeStackMode((StackMode)packingMode)} (packingMode={packingMode})");

                        if (runLeft)
                        {
                            RunStation(jinwo, isLeft: true, "左机台/A", "OP-6308", "a",
                                outer: 90.19, height: 23.1,
                                boxL: 750, boxW: 545, boxH: 395,
                                imagePath: leftImg, packingMode: packingMode, outDir: outDir, Log);
                        }

                        if (runRight)
                        {
                            RunStation(jinwo, isLeft: false, "右机台/B", "OP-6308", "上线B",
                                outer: 90.19, height: 23.1,
                                boxL: 763, boxW: 556, boxH: 395,
                                imagePath: rightImg, packingMode: packingMode, outDir: outDir, Log);
                        }
                    }
                }

                Log("=== 拟运行完成 ===");
                return 0;
            }
            catch (Exception ex)
            {
                Log("失败: " + ex);
                return 1;
            }
        }

        static void RunStation(
            JinwoPlacementService jinwo,
            bool isLeft,
            string stationName,
            string product,
            string box,
            double outer,
            double height,
            double boxL,
            double boxW,
            double boxH,
            string imagePath,
            int packingMode,
            string outDir,
            Action<string> Log)
        {
            var mode = (StackMode)(packingMode == 1 ? 1 : 0);
            string modeTag = packingMode == 1 ? "竖向梅花" : "横向梅花";
            string fileStem = $"{(isLeft ? "A" : "B")}_{product}_{box}_{modeTag}";

            Log("");
            Log($"----- {stationName} | 产品 {product} | 箱体 {box} | {modeTag} -----");
            var ini = jinwo.GetStationConfig(isLeft);
            Log($"安全预留 X={ini.AutoInnerReserveX:F2} Y={ini.AutoInnerReserveY:F2} | 倾角上限={ini.MaxMarkerTiltDegrees:F1}°");
            Log($"图像: {(File.Exists(imagePath) ? imagePath : "缺失 → " + imagePath)}");
            if (!File.Exists(imagePath))
                throw new FileNotFoundException(stationName + " 采图不存在", imagePath);

            int layers = jinwo.CalculateLayerCount(boxH, height, isLeft);
            packingMode = packingMode == 1 ? 1 : 0;
            var cfg = jinwo.BuildTrayConfig(
                boxL, boxW, boxH,
                outer, height,
                layoutRows: 0,
                layoutCols: 0,
                layoutLayers: layers,
                gridFromAlgorithmOnly: true,
                isLeft: isLeft,
                maxMarkerTiltDegrees: ini.MaxMarkerTiltDegrees,
                packingMode: packingMode);

            Log($"Validate OK → 排料={modeTag}({packingMode})，上位机层数={layers}；行列/XY 由算法识图决定");

            if (!jinwo.TryPrepareAlgorithmImage(imagePath, isLeft, out string prepared, out string prepErr))
                throw new InvalidOperationException(stationName + " 图像预处理失败: " + (prepErr ?? ""));
            Log("预处理图: " + prepared);

            var centers = jinwo.CalculateAllBearingCenters(ref cfg, imagePath, 0, isLeft, out string effect, forceSaveEffectImage: true);
            JinwoPlacementService.DeriveGridFromCenters(centers, out int effRows, out int effCols, out int capacity);

            // 与现场规划一致：按梅花方式归正顺序后再导出。
            JinwoPlacementOrder.SortCenters(centers, mode);
            string traversal = JinwoPlacementOrder.DescribeTraversal(mode);

            Log($"全箱规划: {centers.Length} 点 | 网格 {effRows}×{effCols} 容量 {capacity} | {traversal}");
            Log("DLL 效果图: " + (effect ?? "(无)"));

            string docPath = Path.Combine(outDir, fileStem + "_摆放顺序与坐标.txt");
            WriteOrderDocument(docPath, stationName, product, box, modeTag, traversal,
                layers, effRows, effCols, capacity, centers, Log);

            string annotatedPath = Path.Combine(outDir, fileStem + "_效果_带顺序.png");
            string baseImage = !string.IsNullOrEmpty(effect) && File.Exists(effect)
                ? effect
                : (!string.IsNullOrEmpty(prepared) && File.Exists(prepared) ? prepared : imagePath);
            AnnotateOrderImage(baseImage, centers, annotatedPath, modeTag, Log);

            // 顺带复制 DLL 原始效果图便于对照
            if (!string.IsNullOrEmpty(effect) && File.Exists(effect))
            {
                string rawCopy = Path.Combine(outDir, fileStem + "_DLL原始效果" + Path.GetExtension(effect));
                File.Copy(effect, rawCopy, overwrite: true);
                Log("已复制 DLL 原始效果图: " + rawCopy);
            }

            if (centers.Length > 0)
            {
                var c0 = centers[0];
                Log($"归正后首件: 序1 Row={c0.Row} Col={c0.Col} Layer={c0.Layer}" +
                    (c0.HasRobot != 0
                        ? $" robot({c0.RobotX:F2},{c0.RobotY:F2},{c0.RobotZ:F2})"
                        : $" tray({c0.TrayX:F2},{c0.TrayY:F2})"));
            }

            string verifyPath = Path.Combine(outDir, fileStem + "_指定位与开始组抽检.txt");
            VerifyManualAndStartGroup(
                verifyPath, stationName, product, box, modeTag, traversal,
                centers, layers, height, ini.TargetZ, ini.BearingGap,
                jinwo, cfg, imagePath, isLeft, mode, Log);
        }

        /// <summary>
        /// 抽检「手动指定位置」与「指定开始组」：随机取样坐标，并模拟从第 N 组起后续若干发、
        /// 以及「现场对齐 currentCount=跳过组代表槽」是否还能取到足够点。
        /// </summary>
        static void VerifyManualAndStartGroup(
            string path,
            string stationName,
            string product,
            string box,
            string modeTag,
            string traversal,
            JinwoNative.JinwoBearingCenterResult[] centers,
            int maxLayers,
            double productHeight,
            double baseZ,
            double placeLiftGap,
            JinwoPlacementService jinwo,
            JinwoNative.JinwoTrayConfig cfg,
            string imagePath,
            bool isLeft,
            StackMode mode,
            Action<string> Log)
        {
            var sb = new StringBuilder();
            sb.AppendLine("码料机离线拟运行 — 指定位置 / 指定开始组 抽检");
            sb.AppendLine("生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine($"工位: {stationName} | 产品 {product} | 箱体 {box}");
            sb.AppendLine($"排料: {modeTag} | {traversal}");
            sb.AppendLine($"基准Z={baseZ:F3} 产品高={productHeight:F3} 抬高间隙={placeLiftGap:F3} 总层={maxLayers}");
            sb.AppendLine($"竖直档: {ZStackPlacement.FormatBatchPattern(maxLayers)}");
            sb.AppendLine();

            var slots = new List<BoxPlanSlot>();
            for (int i = 0; i < centers.Length; i++)
            {
                var c = centers[i];
                double z = ZStackPlacement.ComputePlaceZForHorizontalLayer(
                    baseZ, Math.Max(0, c.Layer), maxLayers, productHeight, placeLiftGap);
                slots.Add(new BoxPlanSlot
                {
                    Index = i,
                    DllCount = c.Count,
                    WorldX = (float)(c.HasRobot != 0 ? c.RobotX : c.TrayX),
                    WorldY = (float)(c.HasRobot != 0 ? c.RobotY : c.TrayY),
                    Z = (float)z,
                    Rz = (float)(c.HasRobot != 0 ? c.RobotRz : 0),
                    Layer = c.Layer,
                    Row = c.Row,
                    Col = c.Col,
                    PixelX = c.PixelX,
                    PixelY = c.PixelY
                });
            }

            // 与 Form1.ManualPlaceSelect.EnumerateGroupStartIndices(StationData) 一致：仅竖直档起始层上的槽为组代表。
            var tierStartLayers = new HashSet<int>();
            int startLayer = 0;
            foreach (int batchRaw in ZStackPlacement.BuildBatchSizes(maxLayers))
            {
                tierStartLayers.Add(startLayer);
                startLayer += Math.Max(1, batchRaw);
            }
            var groupStarts = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (tierStartLayers.Contains(Math.Max(0, slots[i].Layer)))
                    groupStarts.Add(i);
            }

            sb.AppendLine($"规划槽总数: {slots.Count}");
            sb.AppendLine($"组代表槽数(指定开始组可选): {groupStarts.Count}");
            sb.AppendLine($"档起始层: {string.Join(",", tierStartLayers)}");
            sb.AppendLine();

            bool ok = true;
            void Fail(string msg)
            {
                ok = false;
                sb.AppendLine("[失败] " + msg);
                Log("[抽检失败] " + msg);
            }

            // —— 顺序自检：横向同行 Col 递增；竖向同列 Row 递增（仅看第 0 层前若干点）——
            int checkN = Math.Min(12, slots.Count);
            if (modeTag.Contains("横向"))
            {
                for (int i = 1; i < checkN; i++)
                {
                    if (slots[i].Layer != 0 || slots[i - 1].Layer != 0) break;
                    if (slots[i].Row == slots[i - 1].Row && slots[i].Col < slots[i - 1].Col)
                        Fail($"横向行优先被破坏: 序{i} Col={slots[i - 1].Col} → 序{i + 1} Col={slots[i].Col}");
                }
            }
            else
            {
                for (int i = 1; i < checkN; i++)
                {
                    if (slots[i].Layer != 0 || slots[i - 1].Layer != 0) break;
                    if (slots[i].Col == slots[i - 1].Col && slots[i].Row < slots[i - 1].Row)
                        Fail($"竖向列优先被破坏: 序{i} Row={slots[i - 1].Row} → 序{i + 1} Row={slots[i].Row}");
                }
            }

            var rng = new Random(20260809);
            int sampleCount = Math.Min(6, slots.Count);

            // —— 手动指定位置：随机物理槽 ——
            sb.AppendLine("========== 手动指定位置（随机物理槽） ==========");
            sb.AppendLine("槽号\tLabel\tLayer\tRow\tCol\tWorldX\tWorldY\tPlaceZ\tBatchQty\tZTier\tDllCount");
            var pickedSlots = new HashSet<int>();
            for (int n = 0; n < sampleCount; n++)
            {
                int idx;
                do { idx = rng.Next(slots.Count); } while (!pickedSlots.Add(idx) && pickedSlots.Count < slots.Count);
                var s = slots[idx];
                int zTier = ZStackPlacement.GetZTierFromStackHeight(Math.Max(0, s.Layer), maxLayers);
                int batch = ZStackPlacement.GetPickPlaceQty(Math.Max(0, s.Layer), maxLayers);
                // 组代表：同 Row/Col 在档起始层上的槽（指定位对齐到组起点）
                int groupRep = idx;
                int tier0 = GetZTierStartLayerLocal(zTier, maxLayers);
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i].Layer == tier0 && slots[i].Row == s.Row && slots[i].Col == s.Col)
                    {
                        groupRep = i;
                        break;
                    }
                }
                if (s.WorldX == 0 && s.WorldY == 0)
                    Fail($"指定位槽{idx} XY 全 0");
                if (double.IsNaN(s.Z) || s.Z < -1e6)
                    Fail($"指定位槽{idx} Z 无效: {s.Z}");

                sb.AppendLine(string.Join("\t",
                    idx.ToString(),
                    s.Label,
                    s.Layer.ToString(),
                    s.Row.ToString(),
                    s.Col.ToString(),
                    s.WorldX.ToString("F3"),
                    s.WorldY.ToString("F3"),
                    s.Z.ToString("F3"),
                    batch.ToString(),
                    zTier.ToString(),
                    s.DllCount.ToString()) + $"\t组代表槽={groupRep}");
                Log($"[指定位] 槽{idx} {s.Label} XY=({s.WorldX:F2},{s.WorldY:F2}) Z={s.Z:F2} 档{zTier + 1} 取放{batch} 组代表={groupRep}");
            }

            // —— 指定开始组：随机组号（1 基）——
            sb.AppendLine();
            sb.AppendLine("========== 指定开始组（随机组号 → 代表槽） ==========");
            sb.AppendLine("组号(1基)\t代表槽\tLayer\tRow\tCol\tWorldX\tWorldY\tPlaceZ\tBatchQty\t跳过组数");
            int groupSample = Math.Min(6, groupStarts.Count);
            var pickedGroups = new HashSet<int>();
            for (int n = 0; n < groupSample; n++)
            {
                int gIdx;
                do { gIdx = rng.Next(groupStarts.Count); } while (!pickedGroups.Add(gIdx) && pickedGroups.Count < groupStarts.Count);
                int startGroup1Based = gIdx + 1; // 与界面「从第 N 组开始」一致
                int planSlot = groupStarts[gIdx];
                var s = slots[planSlot];
                int batch = ZStackPlacement.GetPickPlaceQty(Math.Max(0, s.Layer), maxLayers);
                int skipped = startGroup1Based - 1;
                if (!tierStartLayers.Contains(Math.Max(0, s.Layer)))
                    Fail($"开始组{startGroup1Based} 代表槽{planSlot} 不在档起始层 Layer={s.Layer}");
                sb.AppendLine(string.Join("\t",
                    startGroup1Based.ToString(),
                    planSlot.ToString(),
                    s.Layer.ToString(),
                    s.Row.ToString(),
                    s.Col.ToString(),
                    s.WorldX.ToString("F3"),
                    s.WorldY.ToString("F3"),
                    s.Z.ToString("F3"),
                    batch.ToString(),
                    skipped.ToString()));
                Log($"[开始组] 第{startGroup1Based}组 → 槽{planSlot} L{s.Layer + 1}/R{s.Row + 1}/C{s.Col + 1} XY=({s.WorldX:F2},{s.WorldY:F2}) Z={s.Z:F2} 取放{batch} 跳过{skipped}组");
            }

            // —— 指定开始组后续运行：固定从约 1/3 箱处起，列出后续 8 发握手 ——
            sb.AppendLine();
            sb.AppendLine("========== 指定开始组后续运行（模拟从第 N 组起连续下发） ==========");
            if (groupStarts.Count >= 3)
            {
                int startGroup = Math.Max(1, Math.Min(groupStarts.Count - 2, groupStarts.Count / 3));
                int placedGroups = startGroup - 1; // ConfirmedPlacedCount
                int alignFrom = groupStarts[placedGroups]; // ResolveHandshakePlanSlotIndex(placedGroups)
                int skipBearings = 0;
                for (int g = 0; g < placedGroups; g++)
                {
                    var gs = slots[groupStarts[g]];
                    skipBearings += ZStackPlacement.GetPickPlaceQty(Math.Max(0, gs.Layer), maxLayers);
                }

                sb.AppendLine($"模拟：从第 {startGroup} 组开始 → 已补全前 {placedGroups} 组（约 {skipBearings} 件）");
                sb.AppendLine($"现场对齐 currentCount(物理槽)={alignFrom}（下一发第 {startGroup} 组代表槽）");
                sb.AppendLine("发次\t组号\t代表槽\tL/R/C\tWorldX\tWorldY\tPlaceZ\t取放");
                Log($"[后续运行] 从第{startGroup}组起；跳过{placedGroups}组/{skipBearings}件；对齐槽={alignFrom}");

                int runN = Math.Min(8, groupStarts.Count - placedGroups);
                int prevSlot = -1;
                for (int k = 0; k < runN; k++)
                {
                    int g0 = placedGroups + k; // 0 基握手组号
                    int planSlot = groupStarts[g0];
                    var s = slots[planSlot];
                    int batch = ZStackPlacement.GetPickPlaceQty(Math.Max(0, s.Layer), maxLayers);
                    if (planSlot <= prevSlot)
                        Fail($"后续第{g0 + 1}组代表槽{planSlot} 未严格递增（前槽={prevSlot}）");
                    if (Math.Abs(s.WorldX) < 1e-6 && Math.Abs(s.WorldY) < 1e-6)
                        Fail($"后续第{g0 + 1}组 XY 全 0");
                    if (!tierStartLayers.Contains(Math.Max(0, s.Layer)))
                        Fail($"后续第{g0 + 1}组 槽{planSlot} 不在档起始层");
                    prevSlot = planSlot;
                    sb.AppendLine(string.Join("\t",
                        (k + 1).ToString(),
                        (g0 + 1).ToString(),
                        planSlot.ToString(),
                        $"L{s.Layer + 1}/R{s.Row + 1}/C{s.Col + 1}",
                        s.WorldX.ToString("F3"),
                        s.WorldY.ToString("F3"),
                        s.Z.ToString("F3"),
                        batch.ToString()));
                    Log($"[后续#{k + 1}] 第{g0 + 1}组 槽{planSlot} L{s.Layer + 1}/R{s.Row + 1}/C{s.Col + 1} XY=({s.WorldX:F2},{s.WorldY:F2}) Z={s.Z:F2} 取放{batch}");
                }

                // 模拟首次放料现场对齐：DLL 从 alignFrom 起算位，点数须覆盖下一发
                if (jinwo != null && !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                {
                    try
                    {
                        var cfgAlign = cfg;
                        var live = jinwo.CalculateAllBearingCenters(ref cfgAlign, imagePath, alignFrom, isLeft, out _);
                        JinwoPlacementOrder.SortCenters(live, mode);
                        int need = alignFrom + 1;
                        sb.AppendLine($"现场对齐模拟: DLL 返回 {live?.Length ?? 0} 点（要求 ≥ {need}）");
                        if (live == null || live.Length < need)
                            Fail($"现场对齐点数不足: 返回 {live?.Length ?? 0}，需 ≥ {need}（起始组{startGroup}）");
                        else
                        {
                            var liveNext = live[alignFrom];
                            double lx = liveNext.HasRobot != 0 ? liveNext.RobotX : liveNext.TrayX;
                            double ly = liveNext.HasRobot != 0 ? liveNext.RobotY : liveNext.TrayY;
                            var offline = slots[alignFrom];
                            float dx = (float)Math.Abs(lx - offline.WorldX);
                            float dy = (float)Math.Abs(ly - offline.WorldY);
                            sb.AppendLine($"对齐后第{startGroup}组 XY=({lx:F3},{ly:F3}) 与离线差 dx={dx:F2} dy={dy:F2}");
                            Log($"[现场对齐] 第{startGroup}组 liveXY=({lx:F2},{ly:F2}) 离线差 dx={dx:F2} dy={dy:F2}");
                            // 同一空箱图对齐，偏差应很小；半箱实拍会更大，此处仅空箱自洽
                            if (dx > 8 || dy > 8)
                                Fail($"空箱自洽对齐偏差过大 dx={dx:F2} dy={dy:F2}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Fail("现场对齐模拟异常: " + ex.Message);
                    }
                }
            }
            else
            {
                sb.AppendLine("组数不足，跳过后续运行模拟");
            }

            // —— 同平面跨层：随机一对 (Row,Col)，检查 XY 接近、Z 随档升高 ——
            sb.AppendLine();
            sb.AppendLine("========== 同 Row/Col 跨层高度（抽 1 组） ==========");
            if (slots.Count > 0)
            {
                var seed = slots[rng.Next(Math.Min(slots.Count, Math.Max(1, slots.Count / 4)))];
                var same = new List<BoxPlanSlot>();
                foreach (var s in slots)
                {
                    if (s.Row == seed.Row && s.Col == seed.Col)
                        same.Add(s);
                }
                same.Sort((a, b) => a.Layer.CompareTo(b.Layer));
                sb.AppendLine($"抽样平面 Row={seed.Row} Col={seed.Col}，共 {same.Count} 层位");
                float prevZ = float.MinValue;
                foreach (var s in same)
                {
                    sb.AppendLine($"  槽{s.Index} Layer={s.Layer} XY=({s.WorldX:F2},{s.WorldY:F2}) Z={s.Z:F3}");
                    if (s.Layer > 0 && s.Z + 1e-3f < prevZ)
                        Fail($"层{s.Layer} Z={s.Z:F3} 低于前层 {prevZ:F3}");
                    prevZ = s.Z;
                }
                if (same.Count >= 2)
                {
                    float dx = Math.Abs(same[0].WorldX - same[same.Count - 1].WorldX);
                    float dy = Math.Abs(same[0].WorldY - same[same.Count - 1].WorldY);
                    // 同格跨层 XY 应接近（允许标定/畸变数毫米级差）
                    if (dx > 15 || dy > 15)
                        Fail($"同格跨层 XY 偏差过大 dx={dx:F2} dy={dy:F2}");
                }
            }

            sb.AppendLine();
            sb.AppendLine(ok
                ? "结论: 抽检通过（指定位置 / 指定开始组坐标 / 后续运行顺序 / 现场对齐自洽）"
                : "结论: 抽检存在失败项，见上方 [失败]");
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            Log((ok ? "[抽检通过] " : "[抽检有失败] ") + path);
        }

        static int GetZTierStartLayerLocal(int zTier, int maxLayers)
        {
            int[] batches = ZStackPlacement.BuildBatchSizes(maxLayers);
            int layer = 0;
            for (int i = 0; i < zTier && i < batches.Length; i++)
                layer += Math.Max(1, batches[i]);
            return layer;
        }

        static void WriteOrderDocument(
            string path,
            string stationName,
            string product,
            string box,
            string modeTag,
            string traversal,
            int layers,
            int effRows,
            int effCols,
            int capacity,
            JinwoNative.JinwoBearingCenterResult[] centers,
            Action<string> Log)
        {
            var sb = new StringBuilder();
            sb.AppendLine("码料机离线拟运行 — 摆放顺序与坐标");
            sb.AppendLine("生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine($"工位: {stationName}");
            sb.AppendLine($"产品: {product}");
            sb.AppendLine($"箱体: {box}");
            sb.AppendLine($"排料: {modeTag}");
            sb.AppendLine($"遍历: {traversal}");
            sb.AppendLine($"上位机层数: {layers}");
            sb.AppendLine($"算法网格: {effRows} 行 × {effCols} 列，容量参考 {capacity}");
            sb.AppendLine($"中心点数: {centers?.Length ?? 0}");
            sb.AppendLine();
            sb.AppendLine("说明: 序号=归正后的摆放顺序(1起)；DllCount=算法原始 Count；坐标单位 mm / 像素。");
            sb.AppendLine();
            sb.AppendLine("序号\tDllCount\tLayer\tRow\tCol\tTrayX\tTrayY\tRobotX\tRobotY\tRobotZ\tRobotRz\tPixelX\tPixelY");

            if (centers != null)
            {
                for (int i = 0; i < centers.Length; i++)
                {
                    var c = centers[i];
                    sb.AppendLine(string.Join("\t",
                        (i + 1).ToString(),
                        c.Count.ToString(),
                        c.Layer.ToString(),
                        c.Row.ToString(),
                        c.Col.ToString(),
                        c.TrayX.ToString("F3"),
                        c.TrayY.ToString("F3"),
                        c.HasRobot != 0 ? c.RobotX.ToString("F3") : "",
                        c.HasRobot != 0 ? c.RobotY.ToString("F3") : "",
                        c.HasRobot != 0 ? c.RobotZ.ToString("F3") : "",
                        c.HasRobot != 0 ? c.RobotRz.ToString("F3") : "",
                        c.PixelX.ToString("F1"),
                        c.PixelY.ToString("F1")));
                }
            }

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            Log("已写文档: " + path);
        }

        static void AnnotateOrderImage(
            string baseImagePath,
            JinwoNative.JinwoBearingCenterResult[] centers,
            string outPath,
            string modeTag,
            Action<string> Log)
        {
            if (string.IsNullOrEmpty(baseImagePath) || !File.Exists(baseImagePath))
                throw new FileNotFoundException("无法标注：底图不存在", baseImagePath);
            if (centers == null || centers.Length == 0)
                throw new InvalidOperationException("无中心点，无法标注顺序");

            using (var src = Image.FromFile(baseImagePath))
            using (var bmp = new Bitmap(src.Width, src.Height, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(src, 0, 0, src.Width, src.Height);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                float fontSize = Math.Max(12f, Math.Min(src.Width, src.Height) / 90f);
                using (var font = new Font("Microsoft YaHei UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
                using (var fill = new SolidBrush(Color.FromArgb(220, 255, 40, 0)))
                using (var halo = new SolidBrush(Color.FromArgb(180, 255, 255, 255)))
                using (var pen = new Pen(Color.FromArgb(220, 0, 90, 200), Math.Max(1.5f, fontSize / 8f)))
                {
                    // 标题
                    string title = modeTag + " 摆放顺序 1.." + centers.Length;
                    using (var titleFont = new Font("Microsoft YaHei UI", fontSize * 1.4f, FontStyle.Bold, GraphicsUnit.Pixel))
                    using (var titleBrush = new SolidBrush(Color.Yellow))
                    using (var titleBg = new SolidBrush(Color.FromArgb(160, 0, 0, 0)))
                    {
                        var sz = g.MeasureString(title, titleFont);
                        g.FillRectangle(titleBg, 8, 8, sz.Width + 16, sz.Height + 10);
                        g.DrawString(title, titleFont, titleBrush, 16, 12);
                    }

                    for (int i = 0; i < centers.Length; i++)
                    {
                        float x = (float)centers[i].PixelX;
                        float y = (float)centers[i].PixelY;
                        if (float.IsNaN(x) || float.IsNaN(y) || x < 0 || y < 0)
                            continue;

                        string label = (i + 1).ToString();
                        var size = g.MeasureString(label, font);
                        float cx = x - size.Width / 2f;
                        float cy = y - size.Height / 2f;
                        float r = Math.Max(size.Width, size.Height) * 0.55f + 2f;
                        g.FillEllipse(halo, x - r, y - r, r * 2, r * 2);
                        g.DrawEllipse(pen, x - r, y - r, r * 2, r * 2);
                        g.DrawString(label, font, fill, cx, cy);
                    }
                }

                bmp.Save(outPath, ImageFormat.Png);
            }

            Log("已写效果图(带顺序): " + outPath);
        }

        static void DisablePlc(string plcIni, Action<string> Log)
        {
            if (!File.Exists(plcIni))
            {
                Log("PLC 配置不存在，跳过禁用: " + plcIni);
                return;
            }
            IniAPI.INIWriteValue(plcIni, "Connection", "Enabled", "0");
            IniAPI.INIWriteValue(plcIni, "连接", "启用", "0");
            IniAPI.INIWriteValue(plcIni, "Handshake", "HandshakeEnabled", "0");
            Log("已禁用 PLC 连接/握手（拟运行）");
        }

        static string ResolveArgOrDefault(string[] args, string key, string fallback)
        {
            if (args == null) return fallback;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return fallback;
        }
    }
}
