using System;
using System.IO;

namespace 码料机
{
    /// <summary>持久化各工位上次选择的产品型号、箱体规格与排料方式（exe 旁 配置文件\界面设置.ini）。</summary>
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

        public static void Save(bool left, string product, string box, string stackMode = null)
        {
            if (string.IsNullOrWhiteSpace(product) && string.IsNullOrWhiteSpace(box) && string.IsNullOrWhiteSpace(stackMode))
                return;
            EnsureIniFile();
            string sec = Section(left);
            if (!string.IsNullOrWhiteSpace(product))
                IniAPI.INIWriteValue(IniFile, sec, "产品型号", product.Trim());
            if (!string.IsNullOrWhiteSpace(box))
                IniAPI.INIWriteValue(IniFile, sec, "箱体规格", box.Trim());
            if (!string.IsNullOrWhiteSpace(stackMode))
                IniAPI.INIWriteValue(IniFile, sec, "排料方式", stackMode.Trim());
        }

        public static void Load(bool left, out string product, out string box)
        {
            Load(left, out product, out box, out _);
        }

        public static void Load(bool left, out string product, out string box, out string stackMode)
        {
            product = box = stackMode = "";
            if (!File.Exists(IniFile)) return;
            string sec = Section(left);
            product = IniAPI.INIGetStringValue(IniFile, sec, "产品型号", "").Trim();
            box = IniAPI.INIGetStringValue(IniFile, sec, "箱体规格", "").Trim();
            stackMode = IniAPI.INIGetStringValue(IniFile, sec, "排料方式", "").Trim();
        }

        public static void SaveExpectedBoxTotal(bool left, int total)
        {
            EnsureIniFile();
            IniAPI.INIWriteValue(IniFile, Section(left), "本箱总数", Math.Max(0, total).ToString());
        }

        public static int LoadExpectedBoxTotal(bool left)
        {
            if (!File.Exists(IniFile)) return 0;
            return Math.Max(0, IniAPI.GetPrivateProfileInt(Section(left), "本箱总数", 0, IniFile));
        }
    }
}
