// =============================================================================
// IniAPI.cs — Windows API 读写 INI（Get/WritePrivateProfile*）的封装
// 供产品参数、箱体、PLC 配置等路径使用；多重重载对应不同缓冲区类型。
// =============================================================================
using System; // ArgumentException、Convert
using System.Collections.Generic; // List
using System.IO; // File、Path
using System.Runtime.InteropServices; // DllImport、Marshal
using System.Text; // StringBuilder、Encoding

namespace 码料机
{
    /// <summary>INI 文件读写静态工具类（非线程安全，由调用方控制并发）。</summary>
    internal class IniAPI
    {
        #region INI文件操作

        #region API声明

        /// <summary>
        /// 获取所有节点名称(Section)
        /// </summary>
        /// <param name="lpszReturnBuffer">存放节点名称的内存地址,每个节点之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSectionNames(IntPtr lpszReturnBuffer, uint nSize, string lpFileName);

        /// <summary>
        /// 获取某个指定节点(Section)中所有KEY和Value
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpReturnedString">返回值的内存地址,每个之间用\0分隔</param>
        /// <param name="nSize">内存大小(characters)</param>
        /// <param name="lpFileName">Ini文件</param>
        /// <returns>内容的实际长度,为0表示没有内容,为nSize-2表示内存大小不够</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileSection(string lpAppName, IntPtr lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// 读取INI文件中指定的Key的值
        /// </summary>
        /// <param name="lpAppName">节点名称。如果为null,则读取INI中所有节点名称,每个节点名称之间用\0分隔</param>
        /// <param name="lpKeyName">Key名称。如果为null,则读取INI中指定节点中的所有KEY,每个KEY之间用\0分隔</param>
        /// <param name="lpDefault">读取失败时的默认值</param>
        /// <param name="lpReturnedString">读取的内容缓冲区，读取之后，多余的地方使用\0填充</param>
        /// <param name="nSize">内容缓冲区的长度</param>
        /// <param name="lpFileName">INI文件名</param>
        /// <returns>实际读取到的长度</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, [In, Out] char[] lpReturnedString, uint nSize, string lpFileName);

        //另一种声明方式,使用 StringBuilder 作为缓冲区类型的缺点是不能接受\0字符，会将\0及其后的字符截断,
        //所以对于lpAppName或lpKeyName为null的情况就不适用
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName);

        //再一种声明，使用string作为缓冲区的类型同char[]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, string lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// 将指定的键值对写到指定的节点，如果已经存在则替换。
        /// </summary>
        /// <param name="lpAppName">节点，如果不存在此节点，则创建此节点</param>
        /// <param name="lpString">Item键值对，多个用\0分隔,形如key1=value1\0key2=value2
        /// <para>如果为string.Empty，则删除指定节点下的所有内容，保留节点</para>
        /// <para>如果为null，则删除指定节点下的所有内容，并且删除该节点</para>
        /// </param>
        /// <param name="lpFileName">INI文件</param>
        /// <returns>是否成功写入</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]     //可以没有此行
        private static extern bool WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);

        /// <summary>
        /// 将指定的键和值写到指定的节点，如果已经存在则替换
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpKeyName">键名称。如果为null，则删除指定的节点及其所有的项目</param>
        /// <param name="lpString">值内容。如果为null，则删除指定节点中指定的键。</param>
        /// <param name="lpFileName">INI文件</param>
        /// <returns>操作是否成功</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        #endregion

        #region 封装

        /// <summary>现场 INI 多为 GBK；系统 UTF-8 代码页时 Win32 API 会读乱码，故用托管按编码解析。</summary>
        private static readonly Encoding GbkEncoding = Encoding.GetEncoding(936);

        private static Encoding ResolveIniEncoding(string path)
        {
            if (!File.Exists(path)) return GbkEncoding;
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            // 记事本 / VS Code 常保存为 UTF-8 无 BOM；按 GBK 读会导致中文节名、键名对不上
            if (LooksLikeUtf8(bytes))
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            return GbkEncoding;
        }

        private static bool LooksLikeUtf8(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return false;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] <= 0x7F) continue;
                try
                {
                    int len = GetUtf8SequenceLength(bytes[i]);
                    if (len < 2 || i + len > bytes.Length) return false;
                    for (int j = 1; j < len; j++)
                    {
                        if ((bytes[i + j] & 0xC0) != 0x80) return false;
                    }
                    i += len - 1;
                }
                catch { return false; }
            }
            return true;
        }

        private static int GetUtf8SequenceLength(byte lead)
        {
            if ((lead & 0x80) == 0) return 1;
            if ((lead & 0xE0) == 0xC0) return 2;
            if ((lead & 0xF0) == 0xE0) return 3;
            if ((lead & 0xF8) == 0xF0) return 4;
            throw new InvalidOperationException();
        }

        private static List<string> ReadIniLines(string path)
        {
            if (!File.Exists(path)) return new List<string>();
            return new List<string>(File.ReadAllLines(path, ResolveIniEncoding(path)));
        }

        private static void WriteIniLines(string path, List<string> lines)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            Encoding enc = File.Exists(path) ? ResolveIniEncoding(path) : GbkEncoding;
            File.WriteAllLines(path, lines.ToArray(), enc);
        }

        private static bool IsSectionHeader(string trimmed, out string section)
        {
            section = null;
            if (trimmed.Length < 3 || trimmed[0] != '[' || trimmed[trimmed.Length - 1] != ']') return false;
            section = trimmed.Substring(1, trimmed.Length - 2);
            return true;
        }

        private static bool TryParseKeyValue(string trimmed, out string key, out string value)
        {
            key = value = null;
            if (trimmed.Length == 0 || trimmed[0] == ';' || trimmed[0] == '#') return false;
            int eq = trimmed.IndexOf('=');
            if (eq <= 0) return false;
            key = trimmed.Substring(0, eq).Trim();
            value = trimmed.Substring(eq + 1).Trim();
            return key.Length > 0;
        }

        private static string ManagedGetString(string iniFile, string section, string key, string defaultValue)
        {
            if (string.IsNullOrEmpty(section) || string.IsNullOrEmpty(key)) return defaultValue;
            bool inSection = false;
            foreach (string raw in ReadIniLines(iniFile))
            {
                string line = raw.Trim();
                if (IsSectionHeader(line, out string sec))
                {
                    inSection = string.Equals(sec, section, StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                if (!inSection || !TryParseKeyValue(line, out string k, out string v)) continue;
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) return v;
            }
            return defaultValue;
        }

        private static bool ManagedWriteValue(string iniFile, string section, string key, string value)
        {
            var lines = ReadIniLines(iniFile);
            int sectionStart = -1, sectionEnd = lines.Count;
            bool inSection = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string t = lines[i].Trim();
                if (!IsSectionHeader(t, out string sec)) continue;
                if (inSection) { sectionEnd = i; break; }
                if (string.Equals(sec, section, StringComparison.OrdinalIgnoreCase))
                {
                    inSection = true;
                    sectionStart = i;
                }
            }
            if (sectionStart < 0)
            {
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[lines.Count - 1])) lines.Add("");
                lines.Add("[" + section + "]");
                lines.Add(key + "=" + value);
                WriteIniLines(iniFile, lines);
                return true;
            }
            int keyLine = -1;
            for (int i = sectionStart + 1; i < sectionEnd; i++)
            {
                if (!TryParseKeyValue(lines[i].Trim(), out string k, out _)) continue;
                if (!string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) continue;
                keyLine = i;
                break;
            }
            string entry = key + "=" + value;
            if (keyLine >= 0) lines[keyLine] = entry;
            else lines.Insert(sectionEnd, entry);
            WriteIniLines(iniFile, lines);
            return true;
        }

        private static bool ManagedDeleteSection(string iniFile, string section)
        {
            var lines = ReadIniLines(iniFile);
            var kept = new List<string>();
            bool skipping = false;
            foreach (string raw in lines)
            {
                string t = raw.Trim();
                if (IsSectionHeader(t, out string sec))
                {
                    skipping = string.Equals(sec, section, StringComparison.OrdinalIgnoreCase);
                    if (!skipping) kept.Add(raw);
                    continue;
                }
                if (!skipping) kept.Add(raw);
            }
            WriteIniLines(iniFile, kept);
            return true;
        }

        private static bool ManagedDeleteKey(string iniFile, string section, string key)
        {
            var lines = ReadIniLines(iniFile);
            bool inSection = false, changed = false;
            for (int i = 0; i < lines.Count; i++)
            {
                string t = lines[i].Trim();
                if (IsSectionHeader(t, out string sec))
                {
                    inSection = string.Equals(sec, section, StringComparison.OrdinalIgnoreCase);
                    continue;
                }
                if (!inSection || !TryParseKeyValue(t, out string k, out _)) continue;
                if (!string.Equals(k, key, StringComparison.OrdinalIgnoreCase)) continue;
                lines.RemoveAt(i);
                changed = true;
                i--;
            }
            if (changed) WriteIniLines(iniFile, lines);
            return changed;
        }

        /// <summary>
        /// 读取INI文件中指定INI文件中的所有节点名称(Section)
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <returns>所有节点,没有内容返回string[0]</returns>
        public static string[] INIGetAllSectionNames(string iniFile)
        {
            var sections = new List<string>();
            foreach (string raw in ReadIniLines(iniFile))
            {
                string t = raw.Trim();
                if (IsSectionHeader(t, out string sec)) sections.Add(sec);
            }
            return sections.ToArray();
        }

        /// <summary>
        /// 获取INI文件中指定节点(Section)中的所有条目(key=value形式)
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <param name="section">节点名称</param>
        /// <returns>指定节点中的所有项目,没有内容返回string[0]</returns>
        public static string[] INIGetAllItems(string iniFile, string section)
        {
            //返回值形式为 key=value,例如 Color=Red
            uint MAX_BUFFER = 32767;    //默认为32767

            string[] items = new string[0];      //返回值

            //分配内存
            IntPtr pReturnedString = Marshal.AllocCoTaskMem((int)MAX_BUFFER * sizeof(char));

            uint bytesReturned = IniAPI.GetPrivateProfileSection(section, pReturnedString, MAX_BUFFER, iniFile);

            if (!(bytesReturned == MAX_BUFFER - 2) || (bytesReturned == 0))
            {

                string returnedString = Marshal.PtrToStringAuto(pReturnedString, (int)bytesReturned);
                items = returnedString.Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }

            Marshal.FreeCoTaskMem(pReturnedString);     //释放内存

            return items;
        }

        /// <summary>
        /// 获取INI文件中指定节点(Section)中的所有条目的Key列表
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <param name="section">节点名称</param>
        /// <returns>如果没有内容,反回string[0]</returns>
        public static string[] INIGetAllItemKeys(string iniFile, string section)
        {
            string[] value = new string[0];
            const int SIZE = 1024 * 10;

            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            char[] chars = new char[SIZE];
            uint bytesReturned = IniAPI.GetPrivateProfileString(section, null, null, chars, SIZE, iniFile);

            if (bytesReturned != 0)
            {
                value = new string(chars).Split(new char[] { '\0' }, StringSplitOptions.RemoveEmptyEntries);
            }
            chars = null;

            return value;
        }

        /// <summary>
        /// 读取INI文件中指定KEY的字符串型值
        /// </summary>
        /// <param name="iniFile">Ini文件</param>
        /// <param name="section">节点名称</param>
        /// <param name="key">键名称</param>
        /// <param name="defaultValue">如果没此KEY所使用的默认值</param>
        /// <returns>读取到的值</returns>
        public static string INIGetStringValue(string iniFile, string section, string key, string defaultValue)
        {
            string value = defaultValue;

            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("必须指定键名称(key)", "key");
            }

            value = ManagedGetString(iniFile, section, key, defaultValue);
            return value;
        }

        /// <summary>
        /// 读取INI文件的值
        /// </summary>
        /// <param name="lpAppName">节点名称</param>
        /// <param name="lpKeyName">键名称</param>
        /// <param name="Default">如果没此KEY所使用的默认值</param>
        /// <param name="lpFileName">ini文件的路径</param>
        /// <returns>返回值传入的key对应的值(会自动把值转成int类型)</returns>
        public static int GetPrivateProfileInt(string lpAppName, string lpKeyName, int Default, string lpFileName)
        {
            string s = GetPrivateProfileString(lpAppName, lpKeyName, Convert.ToString(Default), lpFileName);
            return int.TryParse(s, out int n) ? n : Default;
        }
        public static double GetPrivateProfileDouble(string lpAppName, string lpKeyName, double Default, string lpFielName)
        {
            string s = GetPrivateProfileString(lpAppName, lpKeyName, Convert.ToString(Default), lpFielName);
            return double.TryParse(s, out double d) ? d : Default;
        }
        public static string GetPrivateProfileString(string lpAppName, string lpKeyName, string Default, string lpFileName)
        {
            if (string.IsNullOrEmpty(lpFileName) || !File.Exists(lpFileName)) return Default;
            return ManagedGetString(lpFileName, lpAppName, lpKeyName, Default);
        }
        /// <summary>
        /// 在INI文件中，将指定的键值对写到指定的节点，如果已经存在则替换
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点，如果不存在此节点，则创建此节点</param>
        /// <param name="items">键值对，多个用\0分隔,形如key1=value1\0key2=value2</param>
        /// <returns></returns>
        public static bool INIWriteItems(string iniFile, string section, string items)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(items))
            {
                throw new ArgumentException("必须指定键值对", "items");
            }

            return IniAPI.WritePrivateProfileSection(section, items, iniFile);
        }

        /// <summary>
        /// 在INI文件中，指定节点写入指定的键及值。如果已经存在，则替换。如果没有则创建。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <returns>操作是否成功</returns>
        public static bool INIWriteValue(string iniFile, string section, string key, string value)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("必须指定键名称", "key");
            }

            if (value == null)
            {
                throw new ArgumentException("值不能为null", "value");
            }

            return ManagedWriteValue(iniFile, section, key, value);
        }

        /// <summary>
        /// 在INI文件中，删除指定节点中的指定的键。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <param name="key">键</param>
        /// <returns>操作是否成功</returns>
        public static bool INIDeleteKey(string iniFile, string section, string key)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("必须指定键名称", "key");
            }

            return ManagedDeleteKey(iniFile, section, key);
        }

        /// <summary>
        /// 在INI文件中，删除指定的节点。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <returns>操作是否成功</returns>
        public static bool INIDeleteSection(string iniFile, string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            return ManagedDeleteSection(iniFile, section);
        }

        /// <summary>
        /// 在INI文件中，删除指定节点中的所有内容。
        /// </summary>
        /// <param name="iniFile">INI文件</param>
        /// <param name="section">节点</param>
        /// <returns>操作是否成功</returns>
        public static bool INIEmptySection(string iniFile, string section)
        {
            if (string.IsNullOrEmpty(section))
            {
                throw new ArgumentException("必须指定节点名称", "section");
            }

            return IniAPI.WritePrivateProfileSection(section, string.Empty, iniFile);
        }

        #endregion

        #endregion

    }
}
