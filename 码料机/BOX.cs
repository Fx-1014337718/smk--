// =============================================================================
// BOX.cs — 箱体规格增删改查：左列表点选，右侧编辑，写入箱体设置.ini
// =============================================================================
using System;
using System.IO;
using System.Windows.Forms;

namespace 码料机
{
    /// <summary>箱体长宽高配置；与主窗体 pathBOX 指向同一 INI（节名=箱类型）。</summary>
    public partial class BOX : Form
    {
        public Form1 cc;
        private bool _dirty;
        private bool _suppressList;
        private string _editingName;
        private string _productIniPath;
        private string _boxIniPath;

        public BOX(Form1 ms)
        {
            InitializeComponent();
            UiLayoutHelper.ApplyDialogChrome(this);
            cc = ms;
            _productIniPath = cc.path;
            _boxIniPath = cc.pathBOX;

            txtName.TextChanged += MarkDirty;
            txtLength.TextChanged += MarkDirty;
            txtWidth.TextChanged += MarkDirty;
            txtHeight.TextChanged += MarkDirty;
        }

        private void MarkDirty(object sender, EventArgs e) => _dirty = true;

        private void BOX_Load(object sender, EventArgs e)
        {
            foreach (var p in new[] { Path.GetDirectoryName(_boxIniPath), Path.GetDirectoryName(_productIniPath) })
                if (!string.IsNullOrEmpty(p)) Directory.CreateDirectory(p);
            if (!File.Exists(_boxIniPath)) File.Create(_boxIniPath).Close();
            if (!File.Exists(_productIniPath)) File.Create(_productIniPath).Close();
            lblPathHint.Text = _boxIniPath;
            ReloadList();
            _dirty = false;
        }

        private void ReloadList(string selectName = null)
        {
            _suppressList = true;
            listNames.BeginUpdate();
            listNames.Items.Clear();
            foreach (string name in IniAPI.INIGetAllSectionNames(_boxIniPath))
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
            txtLength.Text = FormatNum(IniAPI.GetPrivateProfileDouble(name, "箱长", 0, _boxIniPath));
            txtWidth.Text = FormatNum(IniAPI.GetPrivateProfileDouble(name, "箱宽", 0, _boxIniPath));
            txtHeight.Text = FormatNum(IniAPI.GetPrivateProfileDouble(name, "箱高", 0, _boxIniPath));
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
            if (!SaveBoxConfig()) return;
            _dirty = false;
            DialogPrompts.ShowInfo("箱体参数已保存。", "保存成功");
            cc?.RefreshIniData();
            cc?.Boxfresinidata();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string node = string.IsNullOrWhiteSpace(txtName.Text)
                ? Convert.ToString(listNames.SelectedItem)
                : txtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(node))
            {
                DialogPrompts.ShowWarning("请先选择或输入要删除的箱体名称。");
                return;
            }

            if (!DialogPrompts.ConfirmDelete(node)) return;

            bool ok = IniAPI.INIDeleteSection(_boxIniPath, node);
            if (ok)
            {
                DialogPrompts.ShowInfo($"已删除「{node}」。", "删除成功");
                cc?.RefreshIniData();
                cc?.Boxfresinidata();
                ClearEditor();
                ReloadList();
                _dirty = false;
            }
            else
            {
                DialogPrompts.ShowWarning($"未找到名为「{node}」的配置，请检查名称是否正确。");
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => TryClose();

        private void BOX_FormClosing(object sender, FormClosingEventArgs e)
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

            switch (DialogPrompts.AskUnsavedClose("箱体设置"))
            {
                case DialogPrompts.UnsavedCloseAction.Save:
                    if (!SaveBoxConfig()) return false;
                    cc?.RefreshIniData();
                    cc?.Boxfresinidata();
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
            txtLength.Clear();
            txtWidth.Clear();
            txtHeight.Clear();
            _dirty = false;
        }

        private bool SaveBoxConfig()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtName.Text) ||
                    string.IsNullOrWhiteSpace(txtLength.Text) ||
                    string.IsNullOrWhiteSpace(txtWidth.Text) ||
                    string.IsNullOrWhiteSpace(txtHeight.Text))
                {
                    DialogPrompts.ShowWarning("请填写箱类型、箱长、箱宽和箱高后再保存。");
                    return false;
                }

                string name = txtName.Text.Trim();
                double L = double.Parse(txtLength.Text.Trim());
                double W = double.Parse(txtWidth.Text.Trim());
                double H = double.Parse(txtHeight.Text.Trim());
                if (L <= 0 || W <= 0 || H <= 0)
                {
                    DialogPrompts.ShowWarning("箱长、箱宽、箱高须大于 0（单位 mm）。");
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(_editingName) &&
                    !string.Equals(_editingName.Trim(), name, StringComparison.OrdinalIgnoreCase))
                {
                    IniAPI.INIDeleteSection(_boxIniPath, _editingName.Trim());
                }

                bool ok = IniAPI.INIWriteValue(_boxIniPath, name, "箱长", L.ToString())
                    & IniAPI.INIWriteValue(_boxIniPath, name, "箱宽", W.ToString())
                    & IniAPI.INIWriteValue(_boxIniPath, name, "箱高", H.ToString());
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
                DialogPrompts.ShowWarning("箱长、箱宽、箱高请输入有效数字。");
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

        private static string FormatNum(double v)
        {
            if (Math.Abs(v) < 1e-12) return "0";
            return v.ToString("0.####");
        }
    }
}
