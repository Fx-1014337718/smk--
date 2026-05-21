// =============================================================================
// Program.cs — 应用程序入口
// 作用：启用视觉样式、启动消息循环并显示主窗体 Form1。
// =============================================================================
using System; // CLR 基础类型
using System.Windows.Forms; // WinForms 应用程序类

namespace 码料机 // 与项目根命名空间一致
{
    /// <summary>程序入口点；由 exe 启动时 CLR 首先调用 <see cref="Main"/>。</summary>
    internal static class Program
    {
        /// <summary>STAThread：WinForms 与 COM（如剪贴板、部分相机 SDK）要求 STA 线程模型。</summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles(); // 使用操作系统主题绘制控件（XP+）
            Application.SetCompatibleTextRenderingDefault(false); // 使用 GDI+ 文本度量，与设计器一致
            Application.Run(new Form1()); // 创建主窗体并进入消息泵，直到主窗体关闭
        }
    }
}
