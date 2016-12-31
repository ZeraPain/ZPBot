using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ZPBot.Common.Resources
{
    internal class IniFile
    {
        internal static class NativeMethods
        {
            [DllImport("kernel32")]
            internal static extern long WritePrivateProfileString(string section,
                string key, string val, string filePath);
            [DllImport("kernel32")]
            internal static extern int GetPrivateProfileString(string section,
                     string key, string def, StringBuilder retVal,
                int size, string filePath);
        }

        private readonly string _path;

        public IniFile(string iniPath)
        {
            _path = iniPath;
        }

        public void Write(string section, string key, string value)
        {
            NativeMethods.WritePrivateProfileString(section, key, value, _path);
        }

        public T Read<T>(string section, string key, string defValue = "0")
        {
            try
            {
                var temp = new StringBuilder(255);
                NativeMethods.GetPrivateProfileString(section, key, defValue, temp, 255, _path);

                return (T) Convert.ChangeType(temp.ToString(), typeof (T));
            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Settings Read Error - " + ex.Message);
                return default(T);
            }
        }
        
        public void RemoveSection(string pSection)
        {
            NativeMethods.WritePrivateProfileString(pSection, null, null, _path);
        }
    }
}
