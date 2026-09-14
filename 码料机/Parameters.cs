using System;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>产品型号增删改查：左列表点选，右侧编辑，写入配置文件.ini。</summary>
    public partial class Parameters : Form
    {
        public Form1 cc;
        private bool _dirty;
        private bool _suppressList;
        private string _editingName;

        public static readonly string IniDir = Path.Combine(Application.StartupPath, "配置文件");
        public static readonly string IniFile = Path.Combine(IniDir, "配置文件.ini");
        public static readonly string BoxIniFile = Path.Combine(IniDir, "箱体设置.ini");

        public Parameters(Form1 ms)
        {
            InitializeComponent();
            UiLayoutHelper.ApplyDialogChrome(this);
            cc = ms;

            txtName.TextChanged += MarkDirty;
            txtOuter.TextChanged += MarkDirty;
            txtInner.TextChanged += MarkDirty;
            txtHeight.TextChanged += MarkDirty;
        }

        private void MarkDirty(object sender, EventArgs e) => _dirty = true;

        private void Parameters_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(IniDir);
            if (!File.Exists(IniFile)) File.Create(IniFile).Close();
            lblPathHint.Text = IniFile;
            ReloadList();
            _dirty = false;
        }

        private void ReloadList(string selectName = null)
        {
            _suppressList = true;
            listNames.BeginUpdate();
            listNames.Items.Clear();
            foreach (string name in IniAPI.INIGetAllSectionNames(IniFile))
            {
                if (!string.IsNullOrWhiteSpace(name))
                    listNames.Items.Add(name);
            }
            listNames.EndUpdate();

            if (!string.IsNullOrWhiteSpace(selectName))
            {
                for (int i = 0; i < listNames.Items.Count; i++)
                {
                    if (string.Equals(Convert.ToString(listNames.Items[i]), selectName, StringComparison.OrdinalIgnoreCase))
                    {
                        listNames.SelectedIndex = i;
                        break;
                    }
                }
            }
            _suppressList = false;
        }

        private void listNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_suppressList || listNames.SelectedItem == null) return;
            string name = Convert.ToString(listNames.SelectedItem);
            if (string.IsNullOrWhiteSpace(name)) return;

            _editingName = name;
            txtName.Text = name;
            txtOuter.Text = FormatNum(IniAPI.GetPrivateProfileDouble(name, "外径", 0, IniFile));
            double h = IniAPI.GetPrivateProfileDouble(name, "产品高度", 0, IniFile);
            if (h <= 0) h = IniAPI.GetPrivateProfileDouble(name, "高度", 0, IniFile);
            txtHeight.Text = FormatNum(h);
            txtInner.Text = FormatNum(IniAPI.GetPrivateProfileDouble(name, "内径", 0, IniFile));
            _dirty = false;
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            ClearEditor();
            txtName.Focus();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            string keep = listNames.SelectedItem != null
                ? Convert.ToString(listNames.SelectedItem)
                : _editingName;
            ReloadList(keep);
            if (listNames.SelectedItem != null)
                listNames_SelectedIndexChanged(listNames, EventArgs.Empty);
            else
                _dirty = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (!SaveConfig()) return;
            _dirty = false;
            DialogPrompts.ShowInfo("产品参数已保存。", "保存成功");
            cc?.RefreshIniData();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string model = string.IsNullOrWhiteSpace(txtName.Text)
                ? Convert.ToString(listNames.SelectedItem)
                : txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(model))
            {
                DialogPrompts.ShowWarning("请先选择或输入要删除的产品型号。");
                return;
            }

            if (!DialogPrompts.ConfirmDelete(model)) return;

            if (IniAPI.INIDeleteSection(IniFile, model))
            {
                DialogPrompts.ShowInfo($"已删除型号「{model}」。", "删除成功");
                cc?.RefreshIniData();
                ClearEditor();
                ReloadList();
                _dirty = false;
            }
            else
            {
                DialogPrompts.ShowWarning($"未找到型号「{model}」，请检查名称是否正确。");
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => TryClose();

        private void Parameters_FormClosing(object sender, FormClosingEventArgs e)
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
                    cc?.RefreshIniData();
                    _dirty = false;
                    return true;
                case DialogPrompts.UnsavedCloseAction.Discard:
                    return true;
                default:
                    return false;
            }
        }

        private void ClearEditor()
        {
            _editingName = null;
            _suppressList = true;
            listNames.SelectedIndex = -1;
            _suppressList = false;
            txtName.Clear();
            txtOuter.Clear();
            txtInner.Clear();
            txtHeight.Clear();
            _dirty = false;
        }

        private bool SaveConfig()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) ||
                    string.IsNullOrWhiteSpace(txtOuter.Text) ||
                    string.IsNullOrWhiteSpace(txtHeight.Text) ||
                    string.IsNullOrWhiteSpace(txtInner.Text))
                {
                    DialogPrompts.ShowWarning("请填写型号、外径、高度和内径后再保存。");
                    return false;
                }

                string name = txtName.Text.Trim();
                double outer = double.Parse(txtOuter.Text.Trim());
                double height = double.Parse(txtHeight.Text.Trim());
                double inner = double.Parse(txtInner.Text.Trim());
                if (outer <= 0 || height <= 0 || inner < 0)
                {
                    DialogPrompts.ShowWarning("外径、高度须大于 0；内径须为有效数字（单位 mm）。");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(_editingName) &&
                    !string.Equals(_editingName.Trim(), name, StringComparison.OrdinalIgnoreCase))
                {
                    IniAPI.INIDeleteSection(IniFile, _editingName.Trim());
                }

                bool ok = IniAPI.INIWriteValue(IniFile, name, "外径", outer.ToString())
                    & IniAPI.INIWriteValue(IniFile, name, "高度", height.ToString())
                    & IniAPI.INIWriteValue(IniFile, name, "内径", inner.ToString());
                if (!ok)
                {
                    DialogPrompts.ShowError("写入配置文件失败，请检查程序是否有写入权限。");
                    return false;
                }

                _editingName = name;
                ReloadList(name);
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

        private static string FormatNum(double v)
        {
            if (Math.Abs(v) < 1e-12) return "0";
            return v.ToString("0.####");
        }
    }
}
