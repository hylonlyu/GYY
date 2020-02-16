using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace GuaDan
{
    public class IniFile
    {
        private static string _Path = (Application.ExecutablePath.Substring(0, Application.ExecutablePath.LastIndexOf(".")) + ".bmp");

        [DllImport("kernel32 ")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);

        [DllImport("kernel32 ")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filepath);


        public static string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            if (GetPrivateProfileString(Section, Key, "", temp, 255, _Path) > 0)
            {
                return temp.ToString();
            }
            WritePrivateProfileString(Section, Key, "", _Path);
            return "";
        }


        public static void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, _Path);
        }


        public static object Path
        {
            get
            {
                return _Path;
            }
            set
            {
                value = _Path;
            }
        }
    }
}
