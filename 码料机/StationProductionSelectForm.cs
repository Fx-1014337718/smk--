using System;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>
    /// 机械臂控制 — 工位生产选择。仅收集下拉选项，向 PLC 的发送由主窗完成。
    /// 布局见 <see cref="StationProductionSelectForm.Designer"/>。
    /// </summary>
    public partial class StationProductionSelectForm : Form
    {
        /// <summary>1=A，2=B，3=A-B；未选为 0。</summary>
        public int SelectedMode { get; private set; }

        public StationProductionSelectForm(int? lastSentMode = null)
        {
            InitializeComponent();
            if (lastSentMode.HasValue && lastSentMode.Value >= 1 && lastSentMode.Value <= 3)
                comboMode.SelectedIndex = lastSentMode.Value - 1;
            else
                comboMode.SelectedIndex = -1;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            if (comboMode.SelectedIndex < 0)
            {
                DialogPrompts.ShowWarning("请先选择工位生产方式。");
                comboMode.Focus();
                return;
            }
            SelectedMode = comboMode.SelectedIndex + 1;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
