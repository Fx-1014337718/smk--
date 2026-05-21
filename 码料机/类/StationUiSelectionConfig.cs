using System.IO;

namespace 码料机
{
    /// <summary>持久化各工位上次选择的产品型号与箱体规格（exe 旁 配置文件\界面设置.ini）。</summary>
    internal static class StationUiSelectionConfig
    {
        public static readonly string IniFile = Path.Combine(Parameters.IniDir, "界面设置.ini");
        const string SecLeft = "左机台";
        const string SecRight = "右机台";

        static string Section(bool left) => left ? SecLeft : SecRight;

        public static void EnsureIniFile()
        {
            Directory.CreateDirectory(Parameters.IniDir);
            if (!File.Exists(IniFile)) File.Create(IniFile).Close();
        }

        public static void Save(bool left, string product, string box)
        {
            if (string.IsNullOrWhiteSpace(product) && string.IsNullOrWhiteSpace(box)) return;
            EnsureIniFile();
            string sec = Section(left);
            if (!string.IsNullOrWhiteSpace(product))
                IniAPI.INIWriteValue(IniFile, sec, "产品型号", product.Trim());
            if (!string.IsNullOrWhiteSpace(box))
                IniAPI.INIWriteValue(IniFile, sec, "箱体规格", box.Trim());
        }

        public static void Load(bool left, out string product, out string box)
        {
            product = box = "";
            if (!File.Exists(IniFile)) return;
            string sec = Section(left);
            product = IniAPI.INIGetStringValue(IniFile, sec, "产品型号", "").Trim();
            box = IniAPI.INIGetStringValue(IniFile, sec, "箱体规格", "").Trim();
        }
    }
}
