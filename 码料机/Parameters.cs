using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>新建或编辑产品型号节，写入应用程序目录下配置文件。</summary>
    public partial class Parameters : Form
    {
        public Form1 cc;
        private bool _dirty;
        public static readonly string IniDir = Path.Combine(Application.StartupPath, "配置文件");
        public static readonly string IniFile = Path.Combine(IniDir, "配置文件.ini");
        public static readonly string BoxIniFile = Path.Combine(IniDir, "箱体设置.ini");

        public Parameters(Form1 ms)
        {
            InitializeComponent();
            UiLayoutHelper.ApplyDialogChrome(this);
            cc = ms;
            Size = new Size(480, 580);
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "产品参数设置";

            textBox1.TextChanged += MarkDirty;
            textBox2.TextChanged += MarkDirty;
            textBox3.TextChanged += MarkDirty;
            textBox4.TextChanged += MarkDirty;
        }

        private void MarkDirty(object sender, EventArgs e) => _dirty = true;

        private void Form2_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(IniDir);
            if (!File.Exists(IniFile)) File.Create(IniFile).Close();
            _dirty = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!SaveConfig()) return;
            _dirty = false;
            DialogPrompts.ShowInfo("产品参数已保存。", "保存成功");
            cc.RefreshIniData();
        }

        private void button3_Click(object sender, EventArgs e) => TryClose();

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel) return;
            if (!TryHandleUnsavedClose()) e.Cancel = true;
        }

        private void TryClose()
        {
            if (TryHandleUnsavedClose()) Close();
        }

        private bool TryHandleUnsavedClose()
        {
            if (!_dirty) return true;

            switch (DialogPrompts.AskUnsavedClose("产品参数"))
            {
                case DialogPrompts.UnsavedCloseAction.Save:
                    if (!SaveConfig()) return false;
                    cc.RefreshIniData();
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
                DialogPrompts.ShowWarning("请先输入要删除的产品型号。");
                return;
            }

            string model = textBox1.Text.Trim();
            if (!DialogPrompts.ConfirmDelete(model)) return;

            if (IniAPI.INIDeleteSection(IniFile, model))
            {
                DialogPrompts.ShowInfo($"已删除型号「{model}」。", "删除成功");
                cc.RefreshIniData();
                _dirty = false;
                textBox1.Clear();
            }
            else
            {
                DialogPrompts.ShowWarning($"未找到型号「{model}」，请检查名称是否正确。");
            }
        }

        private bool SaveConfig()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) ||
                    string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox4.Text))
                {
                    DialogPrompts.ShowWarning("请填写型号、外径、高度和内径后再保存。");
                    return false;
                }

                string name = textBox1.Text.Trim();
                bool ok = IniAPI.INIWriteValue(IniFile, name, "外径", double.Parse(textBox2.Text).ToString())
                    & IniAPI.INIWriteValue(IniFile, name, "高度", double.Parse(textBox3.Text).ToString())
                    & IniAPI.INIWriteValue(IniFile, name, "内径", double.Parse(textBox4.Text).ToString());
                if (!ok)
                {
                    DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                    return false;
                }
                return true;
            }
            catch (FormatException)
            {
                DialogPrompts.ShowWarning("外径、高度、内径请输入有效数字。");
                return false;
            }
            catch (Exception ex)
            {
                DialogPrompts.ShowError($"保存时出现问题：{ex.Message}");
                return false;
            }
        }
    }
}
