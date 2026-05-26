using System.IO;

namespace 码料机
{
    /// <summary>现场运行选项：设定放料位 / 手动指定算法位（配置文件\界面设置.ini）。</summary>
    internal sealed class RuntimeOperationConfig
    {
        const string Section = "运行调试";
        const string KeyLeftManualPlace = "左机台使用设定放料位";
        const string KeyRightManualPlace = "右机台使用设定放料位";
        const string KeyLeftManualSlot = "左机台手动指定放料位";
        const string KeyRightManualSlot = "右机台手动指定放料位";

        public bool LeftUseConfiguredPlace { get; set; }
        public bool RightUseConfiguredPlace { get; set; }
        /// <summary>启用后由算法生成整箱位姿表，现场在「手动指定放料」界面挑选下一发位。</summary>
        public bool LeftUseManualSlotSelect { get; set; }
        public bool RightUseManualSlotSelect { get; set; }

        public bool UseConfiguredPlace(bool isLeft) =>
            isLeft ? LeftUseConfiguredPlace : RightUseConfiguredPlace;

        public bool UseManualSlotSelect(bool isLeft) =>
            isLeft ? LeftUseManualSlotSelect : RightUseManualSlotSelect;

        public bool HasManualPlaceMode => LeftUseConfiguredPlace || RightUseConfiguredPlace;

        public bool HasManualSlotSelectMode => LeftUseManualSlotSelect || RightUseManualSlotSelect;

        public void Load()
        {
            StationUiSelectionConfig.EnsureIniFile();
            string path = StationUiSelectionConfig.IniFile;
            if (!File.Exists(path)) return;
            LeftUseConfiguredPlace = IniAPI.GetPrivateProfileInt(Section, KeyLeftManualPlace, 0, path) != 0;
            RightUseConfiguredPlace = IniAPI.GetPrivateProfileInt(Section, KeyRightManualPlace, 0, path) != 0;
            LeftUseManualSlotSelect = IniAPI.GetPrivateProfileInt(Section, KeyLeftManualSlot, 0, path) != 0;
            RightUseManualSlotSelect = IniAPI.GetPrivateProfileInt(Section, KeyRightManualSlot, 0, path) != 0;
        }

        public void Save()
        {
            StationUiSelectionConfig.EnsureIniFile();
            string path = StationUiSelectionConfig.IniFile;
            IniAPI.INIWriteValue(path, Section, KeyLeftManualPlace, LeftUseConfiguredPlace ? "1" : "0");
            IniAPI.INIWriteValue(path, Section, KeyRightManualPlace, RightUseConfiguredPlace ? "1" : "0");
            IniAPI.INIWriteValue(path, Section, KeyLeftManualSlot, LeftUseManualSlotSelect ? "1" : "0");
            IniAPI.INIWriteValue(path, Section, KeyRightManualSlot, RightUseManualSlotSelect ? "1" : "0");
        }
    }
}
