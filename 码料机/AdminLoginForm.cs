using System;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>安全区域边界设定登录（默认 admin / admin）。布局见 AdminLoginForm.Designer.cs。</summary>
    public partial class AdminLoginForm : Form
    {
        public AdminLoginForm()
        {
            InitializeComponent();
            textUser.Text = AlarmPositionLimitConfig.AdminUserName;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            string u = (textUser.Text ?? "").Trim();
            string p = textPass.Text ?? "";
            if (string.Equals(u, AlarmPositionLimitConfig.AdminUserName, StringComparison.Ordinal)
                && string.Equals(p, AlarmPositionLimitConfig.AdminPassword, StringComparison.Ordinal))
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }
            DialogPrompts.ShowWarning("用户名或密码错误。");
            textPass.SelectAll();
            textPass.Focus();
        }

        /// <summary>弹出登录；成功返回 true。</summary>
        public static bool TryAuthenticate(IWin32Window owner)
        {
            using (var dlg = new AdminLoginForm())
                return dlg.ShowDialog(owner) == DialogResult.OK;
        }
    }
}
