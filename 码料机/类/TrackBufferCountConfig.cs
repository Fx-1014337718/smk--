using System.IO;

namespace 码料机
{
    /// <summary>A/B 工位料道缓存个数（配置文件\界面设置.ini [料道缓存]）。</summary>
    internal sealed class TrackBufferCountConfig
    {
        const string Section = "料道缓存";
        const string KeyA = "A工位料道缓存个数";
        const string KeyB = "B工位料道缓存个数";

        public int LeftCount { get; set; }
        public int RightCount { get; set; }

        public int Get(bool isLeft) => isLeft ? LeftCount : RightCount;

        public void Set(bool isLeft, int count)
        {
            if (isLeft) LeftCount = count;
            else RightCount = count;
        }

        public void Load()
        {
            StationUiSelectionConfig.EnsureIniFile();
            string path = StationUiSelectionConfig.IniFile;
            if (!File.Exists(path))
            {
                LeftCount = RightCount = 0;
                return;
            }
            LeftCount = IniAPI.GetPrivateProfileInt(Section, KeyA, 0, path);
            RightCount = IniAPI.GetPrivateProfileInt(Section, KeyB, 0, path);
        }

        public bool Save(bool isLeft, int count)
        {
            if (count < 0) return false;
            Set(isLeft, count);
            return SaveBoth();
        }

        public bool SaveBoth()
        {
            StationUiSelectionConfig.EnsureIniFile();
            string path = StationUiSelectionConfig.IniFile;
            IniAPI.INIWriteValue(path, Section, KeyA, LeftCount.ToString());
            IniAPI.INIWriteValue(path, Section, KeyB, RightCount.ToString());
            return true;
        }
    }
}
