using System;
using System.IO;

namespace 码料机
{
    /// <summary>
    /// 拟运行：校验限位报警参数（输入约束 + 发送前超限拦截）。
    /// 启动：码料机.exe --safety-limit-test
    /// </summary>
    internal static class SafetyLimitSmokeTest
    {
        public static int Run()
        {
            int fail = 0;
            Console.WriteLine("=== 安全限位拟运行 ===");

            // 1) 发送前范围：取料安全区
            var alarm = new AlarmPositionLimitConfig
            {
                Enabled = true,
                Pick = new AxisLimitRange
                {
                    MinX = 400, MaxX = 500,
                    MinY = 500, MaxY = 600,
                    MinZ = 900, MaxZ = 1100,
                },
                Place = new AxisLimitRange
                {
                    MinX = 200, MaxX = 400,
                    MinY = -900, MaxY = -700,
                    MinZ = 700, MaxZ = 900,
                },
            };

            fail += Expect("取料坐标在范围内应放行",
                !alarm.IsOutOfLimit(true, 426.59, 531.913, 992.795, out _), true);
            fail += Expect("取料 X 超上限应拦截",
                alarm.IsOutOfLimit(true, 999, 531, 992, out string d1) && d1.Contains("X="), true);
            fail += Expect("取料 Z 超下限应拦截",
                alarm.IsOutOfLimit(true, 426, 531, 100, out string d2) && d2.Contains("Z="), true);
            fail += Expect("放料坐标在范围内应放行",
                !alarm.IsOutOfLimit(false, 305.7, -808, 772.075, out _), true);
            fail += Expect("放料 Y 超范围应拦截",
                alarm.IsOutOfLimit(false, 305, 0, 772, out string d3) && d3.Contains("Y="), true);

            // 2) 限位报警参数输入受「安全区域」边界约束
            var envelope = new AlarmPositionLimitConfig
            {
                Enabled = true,
                Pick = new AxisLimitRange { MinX = 0, MaxX = 1000, MinY = 0, MaxY = 1000, MinZ = 0, MaxZ = 2000 },
                Place = new AxisLimitRange { MinX = -1000, MaxX = 1000, MinY = -1000, MaxY = 1000, MinZ = 0, MaxZ = 2000 },
            };
            fail += Expect("限位参数 X最小=100 在边界内",
                !envelope.IsAxisValueOutOfLimit(true, 'X', 100, out _), true);
            fail += Expect("限位参数 X最小=5000 超出边界",
                envelope.IsAxisValueOutOfLimit(true, 'X', 5000, out string d4) && d4.Contains("X="), true);
            fail += Expect("限位参数 Z最大=2500 超出边界",
                envelope.IsAxisValueOutOfLimit(true, 'Z', 2500, out string d5) && d5.Contains("Z="), true);

            // 3) 未启用时不拦截
            var off = new AlarmPositionLimitConfig
            {
                Enabled = false,
                Pick = alarm.Pick.Clone(),
            };
            fail += Expect("未启用时超限坐标放行",
                !off.IsOutOfLimit(true, 9999, 9999, 9999, out _), true);

            // 4) 写读 INI（含 Z）往返
            string tmp = Path.Combine(Path.GetTempPath(), "码料机_safety_limit_smoke.ini");
            try
            {
                if (File.Exists(tmp)) File.Delete(tmp);
                File.WriteAllText(tmp, "");
                fail += Expect("保存限位报警(含Z)", alarm.Save(true, tmp), true);
                var loaded = AlarmPositionLimitConfig.Load(true, tmp);
                fail += Expect("加载后启用", loaded.Enabled, true);
                fail += Expect("加载后取料Z范围",
                    Math.Abs(loaded.Pick.MinZ - 900) < 1e-6 && Math.Abs(loaded.Pick.MaxZ - 1100) < 1e-6, true);
                fail += Expect("加载后仍能拦截超限取料",
                    loaded.IsOutOfLimit(true, 426, 531, 50, out _), true);

                fail += Expect("保存安全区域边界", envelope.SaveEnvelope(true, tmp), true);
                var envLoaded = AlarmPositionLimitConfig.LoadEnvelope(true, tmp);
                fail += Expect("边界启用", envLoaded.Enabled, true);
                fail += Expect("边界限制限位参数输入",
                    envLoaded.IsAxisValueOutOfLimit(true, 'Z', 3000, out _), true);
            }
            finally
            {
                try { if (File.Exists(tmp)) File.Delete(tmp); } catch { }
            }

            Console.WriteLine(fail == 0
                ? "=== 全部通过：输入约束与发送前超限报警逻辑正常 ==="
                : $"=== 失败 {fail} 项 ===");
            return fail == 0 ? 0 : 1;
        }

        static int Expect(string name, bool actual, bool expected)
        {
            bool ok = actual == expected;
            Console.WriteLine((ok ? "[OK] " : "[FAIL] ") + name);
            return ok ? 0 : 1;
        }
    }
}
