// =============================================================================
// StackingPlacement.cs — 码放几何：箱姿态与箱内局部坐标 → 世界坐标
// 与 Form1 中 StationData、Vision 箱位角一起使用。
// =============================================================================
using System; // Math 三角函数、字符串处理

namespace 码料机
{
    /// <summary>层内产品排列：平行或交叉（错行半径）。</summary>
    public enum StackMode { Parallel, Cross }

    /// <summary>箱体在平面内的位姿：一角为原点、一边为 X 方向（角度为度）。</summary>
    public struct BoxPose
    {
        public double OriginWorldX, OriginWorldY, AngleDeg; // 箱原点世界坐标、箱长边与世界 X 夹角
        public bool IsValid; // 若为 false，LocalBoxToWorld 中按单位阵处理
        public static BoxPose Identity => new BoxPose { IsValid = true }; // 有效但角度为 0、原点 0（占位）
        public static BoxPose FromVision(double wx, double wy, double angleDeg) // 由视觉输出的角点+线角构造
            => new BoxPose { OriginWorldX = wx, OriginWorldY = wy, AngleDeg = angleDeg, IsValid = true };

        public static BoxPose FromEdgeTwoCornersWorldMm(double c1x, double c1y, double c2x, double c2y) // 两角点定朝向
        {
            double dx = c2x - c1x, dy = c2y - c1y, len = Math.Sqrt(dx * dx + dy * dy); // 边向量与长度
            if (len < 1e-3) return Identity; // 长度过短无法定方向
            return new BoxPose
            {
                OriginWorldX = c1x, OriginWorldY = c1y, // 以第一角为原点
                AngleDeg = Math.Atan2(dy, dx) * (180.0 / Math.PI), // 弧度转角
                IsValid = true
            };
        }
    }

    /// <summary>静态工具：解析摆放模式字符串、箱局部 mm → 世界 mm 与放料角度。</summary>
    public static class StackingPlacement
    {
        private const double DegToRad = Math.PI / 180.0; // 度 → 弧度乘子

        public static StackMode ParseStackMode(string text) => // 从界面「平行/交叉」等文案解析
            string.IsNullOrWhiteSpace(text) || text.IndexOf("交叉", StringComparison.Ordinal) < 0
                ? StackMode.Parallel : StackMode.Cross; // 含「交叉」则为 Cross

        public static void LocalBoxToWorld(BoxPose box, float localX, float localY, // 箱内 mm（沿箱轴）
            out float worldX, out float worldY, out float placementAngleDeg) // 输出世界 mm 与放料 RZ
        {
            if (!box.IsValid) box = BoxPose.Identity; // 无效箱姿则按无旋转平移
            double th = box.AngleDeg * DegToRad, c = Math.Cos(th), s = Math.Sin(th); // 旋转矩阵元素
            worldX = (float)(box.OriginWorldX + localX * c - localY * s); // 旋转 + 平移 X
            worldY = (float)(box.OriginWorldY + localX * s + localY * c); // 旋转 + 平移 Y
            placementAngleDeg = (float)NormalizeAngleDeg(box.AngleDeg); // 规范化到 (-180,180]
        }

        private static double NormalizeAngleDeg(double deg) // 将角度卷绕到常用区间
        {
            deg %= 360.0; // 先模 360
            if (deg < -180) deg += 360; // 负角上移
            if (deg > 180) deg -= 360; // 正角下移
            return deg;
        }
    }
}
