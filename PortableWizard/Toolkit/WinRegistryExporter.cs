using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PortableWizard.Toolkit
{
	class WinRegistryExporter
	{
		  //    'Just a wee bit of API to fill the gaps not covered by .NET's registry access
            //    'Used to determine a value's type and read/write strings of the type REG_EXPAND_SZ
            [DllImport("advapi32.dll", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            private static extern int RegCloseKey(int hKey);
 
            [DllImport("advapi32.dll", EntryPoint="RegCreateKeyA", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            private static extern int RegCreateKey(int hKey, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpSubKey, ref int phkResult);
 
            [DllImport("advapi32.dll", EntryPoint="RegOpenKeyA", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            private static extern int RegOpenKey(int hKey, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpSubKey, ref int phkResult);
 
            [DllImport("advapi32.dll", EntryPoint="RegQueryValueExA", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            private static extern int RegQueryValueEx(int hKey, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpValueName, int lpReserved, ref int lpType, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpData, ref int lpcbData);
 
            [DllImport("advapi32.dll", EntryPoint="RegSetValueExA", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
            private static extern int RegSetValueEx(int hKey, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpValueName, int Reserved, int dwType, [MarshalAs(UnmanagedType.VBByRefStr)] ref string lpData, int cbData);
 
 
        public static void ExportKey(RegistryKey rKey, string sfile)
        {
            WinRegistryExporter.ExportKey(rKey.Name, sfile);
        }
 
        public static void ExportKey(string sRegKeyPath, string sfile)
        {
            string text1 = "\"" + sRegKeyPath + ".*\"";
            WinRegistryExporter.FileAppend(sfile, "");
            WinRegistryExporter.ShellFile("regedit.exe", "/E " + WinRegistryExporter.GetDosPath(sfile) + " " + text1, ProcessWindowStyle.Normal);
        }

		public static void FileAppend(string path, string text)
        {
                StreamWriter writer1 = File.AppendText(path);
                writer1.Write(text);
                writer1.Close();
        }

		public static string ShellFile(string path, string arguments, ProcessWindowStyle style)
        {
            string text1="";
            Process process1 = new Process();
            try
            {
                process1.StartInfo.FileName = path;
                process1.StartInfo.UseShellExecute = false;
                process1 = Process.Start(path, arguments);
                process1.WaitForExit();
            }
            finally
            {
                process1.Dispose();
            }
            return text1;
        }

		public static string GetDosPath(string path)
        {
            return WinRegistryExporter.GetShortFileName(path);
        }
		public static string GetShortFileName(string path)
        {
                StringBuilder builder1 = new StringBuilder(0x400);
                int num1 = WinRegistryExporter.GetShortPathName(ref path, builder1, builder1.Capacity);
                return builder1.ToString(0, num1);
        }
 
        [DllImport("kernel32", EntryPoint="GetShortPathNameA", CharSet=CharSet.Ansi, SetLastError=true, ExactSpelling=true)]
        private static extern int GetShortPathName([MarshalAs(UnmanagedType.VBByRefStr)] ref string lpszLongPath, StringBuilder lpszShortPath, int cchBuffer);
  
	}
}
