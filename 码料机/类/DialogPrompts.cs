using System.Windows.Forms;

namespace 码料机
{
    /// <summary>统一、易懂的操作提示与关闭确认。</summary>
    internal static class DialogPrompts
    {
        public enum UnsavedCloseAction
        {
            Save,
            Discard,
            Cancel
        }

        /// <summary>关闭窗口前询问：保存 / 放弃 / 继续编辑。</summary>
        public static UnsavedCloseAction AskUnsavedClose(string configName)
        {
            var result = MessageBox.Show(
                $"「{configName}」中有尚未保存的修改。\n\n" +
                "• 点击「是」：保存修改并关闭\n" +
                "• 点击「否」：放弃修改并关闭\n" +
                "• 点击「取消」：返回继续编辑",
                "是否保存修改？",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            switch (result)
            {
                case DialogResult.Yes:
                    return UnsavedCloseAction.Save;
                case DialogResult.No:
                    return UnsavedCloseAction.Discard;
                default:
                    return UnsavedCloseAction.Cancel;
            }
        }

        public static bool ConfirmDelete(string itemDescription)
        {
            return MessageBox.Show(
                $"确定要删除「{itemDescription}」吗？\n删除后无法恢复。",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes;
        }

        public static void ShowInfo(string message, string title = "提示")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowWarning(string message, string title = "请检查输入")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void ShowError(string message, string title = "操作失败")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
