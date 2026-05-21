// =============================================================================
// BOX.cs — 箱体尺寸设置子窗体：读写箱体 INI、删除节点、保存后通知主窗体刷新
// =============================================================================
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>箱体长宽高配置；与主窗体 pathBOX 指向同一 INI。</summary>
    public partial class BOX : Form
    {
        public Form1 cc;
        private bool _dirty;
        private string _productIniPath;
        private string _boxIniPath;

        public BOX(Form1 ms)
        {
            InitializeComponent();
            cc = ms;
            Size = new Size(450, 550);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "箱体设置";
            _productIniPath = cc.path;
            _boxIniPath = cc.pathBOX;

            textBox1.TextChanged += MarkDirty;
            textBox2.TextChanged += MarkDirty;
            textBox3.TextChanged += MarkDirty;
            textBox4.TextChanged += MarkDirty;
        }

        private void MarkDirty(object sender, EventArgs e) => _dirty = true;

        private void BOX_Load(object sender, EventArgs e)
        {
            foreach (var p in new[] { Path.GetDirectoryName(_boxIniPath), Path.GetDirectoryName(_productIniPath) })
                if (!string.IsNullOrEmpty(p)) Directory.CreateDirectory(p);
            if (!File.Exists(_boxIniPath)) File.Create(_boxIniPath).Close();
            if (!File.Exists(_productIniPath)) File.Create(_productIniPath).Close();
            LoadBoxParamsToInput();
            _dirty = false;
        }

        private void LoadBoxParamsToInput()
        {
            try
            {
                textBox2.Text = IniAPI.GetPrivateProfileDouble("箱体", "箱长", 0, _boxIniPath).ToString();
                textBox3.Text = IniAPI.GetPrivateProfileDouble("箱体", "箱高", 0, _boxIniPath).ToString();
                textBox4.Text = IniAPI.GetPrivateProfileDouble("箱体", "箱宽", 0, _boxIniPath).ToString();
            }
            catch
            {
                textBox2.Text = textBox3.Text = textBox4.Text = "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!SaveBoxConfig()) return;
            _dirty = false;
            DialogPrompts.ShowInfo("箱体参数已保存。", "保存成功");
            cc.RefreshIniData();
            cc.Boxfresinidata();
        }

        private void button3_Click(object sender, EventArgs e) => TryClose();

        private void BOX_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel) return;
            if (!TryHandleUnsavedClose()) e.Cancel = true;
        }

        private void TryClose()
        {
            if (TryHandleUnsavedClose()) Close();
        }

        /// <returns>true 表示可以关闭；false 表示应留在窗口。</returns>
        private bool TryHandleUnsavedClose()
        {
            if (!_dirty) return true;

            switch (DialogPrompts.AskUnsavedClose("箱体设置"))
            {
                case DialogPrompts.UnsavedCloseAction.Save:
                    if (!SaveBoxConfig()) return false;
                    cc.RefreshIniData();
                    cc.Boxfresinidata();
                    _dirty = false;
                    return true;
                case DialogPrompts.UnsavedCloseAction.Discard:
                    return true;
                default:
                    return false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                DialogPrompts.ShowWarning("请先输入要删除的箱体名称（如：一号木箱）。");
                return;
            }

            string node = textBox1.Text.Trim();
            if (!DialogPrompts.ConfirmDelete(node)) return;

            bool ok = IniAPI.INIDeleteSection(_boxIniPath, node) || IniAPI.INIDeleteSection(_productIniPath, node);
            if (ok)
            {
                DialogPrompts.ShowInfo($"已删除「{node}」。", "删除成功");
                cc.RefreshIniData();
                cc.Boxfresinidata();
                _dirty = false;
                textBox1.Text = "";
                LoadBoxParamsToInput();
            }
            else
            {
                DialogPrompts.ShowWarning($"未找到名为「{node}」的配置，请检查名称是否正确。");
            }
        }

        private bool SaveBoxConfig()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                    string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
                {
                    DialogPrompts.ShowWarning("请填写箱体名称、箱长、箱高和箱宽后再保存。");
                    return false;
                }

                string name = textBox1.Text.Trim();
                double L = double.Parse(textBox2.Text.Trim());
                double H = double.Parse(textBox3.Text.Trim());
                double W = double.Parse(textBox4.Text.Trim());
                bool ok = IniAPI.INIWriteValue(_boxIniPath, name, "箱长", L.ToString())
                    & IniAPI.INIWriteValue(_boxIniPath, name, "箱高", H.ToString())
                    & IniAPI.INIWriteValue(_boxIniPath, name, "箱宽", W.ToString());
                textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = "";
                if (!ok)
                {
                    DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                    return false;
                }
                return true;
            }
            catch (FormatException)
            {
                DialogPrompts.ShowWarning("箱长、箱高、箱宽请输入有效数字。");
                return false;
            }
            catch (Exception ex)
            {
                DialogPrompts.ShowError($"保存时出现问题：{ex.Message}");
                return false;
            }
        }

        public bool SaveProductConfig(string productModel, double outerDiam, double innerDiam, double height, string packingMode)
        {
            try
            {
                IniAPI.INIWriteValue(_productIniPath, productModel, "外径", outerDiam.ToString());
                IniAPI.INIWriteValue(_productIniPath, productModel, "内径", innerDiam.ToString());
                IniAPI.INIWriteValue(_productIniPath, productModel, "高度", height.ToString());
                IniAPI.INIWriteValue(_productIniPath, productModel, "排料方式", packingMode);
                return true;
            }
            catch (Exception ex)
            {
                DialogPrompts.ShowError($"产品参数保存失败：{ex.Message}");
                return false;
            }
        }
    }
}
