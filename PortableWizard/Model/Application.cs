using Microsoft.Win32;
using PortableWizard.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace PortableWizard.Model
{
    public class Application
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Version { get; set; }

        [XmlIgnore]
        public BitmapImage Icon { get; set; }

        [XmlAttribute]
        public bool IsDesktopShortcut { get; set; }
        [XmlAttribute]
        public bool IsStartMenuShortcut { get; set; }
        [XmlAttribute]
        public bool IsPinnedToStart { get; set; }
        [XmlAttribute]
        public bool IsPinnedToTaskbar { get; set; }
        [XmlAttribute]
        public bool IsStartup { get; set; }

        [XmlArray]
        public List<string> SupportedFileExtensions { get; set; }
        [XmlArray]
        public List<string> HandledFileExtensions { get; set; }

        [XmlAttribute]
        public string ConfigFilePath { get; set; }

        private FileInfo ConfigFile;

        public Application() { }

        public Application(string configFilePath)
        {
            this.ConfigFilePath = configFilePath;

            InitUnserializedData();

            IniFile iniFile = new IniFile(ConfigFile.FullName);
            this.Name = iniFile.IniReadValue("Details", "Name");
            this.Version = iniFile.IniReadValue("Format", "Version");
            this.IsDesktopShortcut = true;
            this.IsStartMenuShortcut = true;
            this.IsPinnedToStart = false;
            this.IsPinnedToTaskbar = true;
            this.IsStartup = false;

            this.SupportedFileExtensions = new List<string>();
            string associations = iniFile.IniReadValue("Associations", "FileTypes");
            if (associations != "")
            {
                SupportedFileExtensions.AddRange(associations.Split(','));
            }
            this.SupportedFileExtensions.Sort();

            this.HandledFileExtensions = new List<string>();
        }

        public void InitUnserializedData()
        {
            ConfigFile = new FileInfo(ConfigFilePath);

            FileInfo iconFile = new FileInfo(ConfigFile.Directory.FullName + @"\appicon_32.png");
            if (iconFile.Exists)
            {
                Icon = new BitmapImage(new Uri(ConfigFile.Directory.FullName + @"\appicon_32.png"));
            }
        }

        public void AddShortcutToDesktop()
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            createShortcut(deskDir + "\\" + Name + ".url");
        }

        public void DeleteShortcutFromDesktop()
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            File.Delete(deskDir + "\\" + Name + ".url");
        }

        public void AddShortcutToStartMenu()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\" + Name;
            DirectoryInfo startPath = new DirectoryInfo(startDir);
            if (!startPath.Exists)
            {
                startPath.Create();
            }
            createShortcut(startDir + "\\" + Name + ".url");
        }

        private void createShortcut(string fullpath)
        {
            IniFile iniFile = new IniFile(ConfigFile.FullName);
            string appexe = iniFile.IniReadValue("Control", "Start");
            FileInfo appPath = new FileInfo(ConfigFile.Directory.FullName + @"\..\..\" + appexe);

            using (StreamWriter writer = new StreamWriter(fullpath))
            {
                writer.WriteLine("[InternetShortcut]");
                writer.WriteLine("URL=file:///" + appPath.FullName.Replace("\\", "/"));
                writer.WriteLine("IconIndex=0");

                FileInfo iconPath = new FileInfo(ConfigFile.Directory.FullName + @"\appicon.ico");
                string icon = iconPath.FullName;

                writer.WriteLine("IconFile=" + icon);
                writer.Flush();
            }
        }

        public void DeleteShortcutFromStartMenu()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\" + Name;
            DirectoryInfo startPath = new DirectoryInfo(startDir);
            if (startPath.Exists)
            {
                startPath.Delete(true);
            }
        }

        public void PinShortcutToTaskBar()
        {
            PinToWindows(true, true);
        }

        public void UnPinShortcutFromTaskBar()
        {
            PinToWindows(true, false);
        }

        public void PinShortcutToStart()
        {
            PinToWindows(false, true);
        }

        public void UnPinShortcutFromStart()
        {
            IniFile iniFile = new IniFile(ConfigFile.FullName);
            string appexe = iniFile.IniReadValue("Control", "Start");
            string ext = appexe.Split('.')[appexe.Split('.').Length - 1];
            string exename = appexe.Substring(0, appexe.LastIndexOf("." + ext));
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\" + exename + ".lnk";
            FileInfo fileshortcut = new FileInfo(startDir);
            if (fileshortcut.Exists)
            {
                fileshortcut.Delete();
            }
        }

        /// <summary>
        /// This method pin/unpin the application to the taskbar or the StartMenu.
        /// </summary>
        /// <param name="TaskBar_StartMenu">If true; pin to taskbar. If false; pin to start menu.</param>
        /// <param name="Pin_UnPin">If true; pin. If false; unpin.</param>
        private void PinToWindows(bool TaskBar_StartMenu, bool Pin_UnPin)
        {
            IniFile iniFile = new IniFile(ConfigFile.FullName);
            string appexe = iniFile.IniReadValue("Control", "Start");
            string filePath = new FileInfo(ConfigFile.Directory.FullName + @"\..\..\" + appexe).FullName;

            if (!File.Exists(filePath)) throw new FileNotFoundException(filePath);

            // create the shell application object
            dynamic shellApplication = Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));

            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);

            dynamic directory = shellApplication.NameSpace(path);
            dynamic link = directory.ParseName(fileName);

            dynamic verbs = link.Verbs();
            for (int i = 0; i < verbs.Count(); i++)
            {
                dynamic verb = verbs.Item(i);
                string verbName = verb.Name.Replace(@"&", string.Empty).ToLower();

                //taskbar
                if (TaskBar_StartMenu)
                {
                    //pin
                    if (Pin_UnPin)
                    {
                        if (IsTaskbarPinItem(verbName))
                        {
                            verb.DoIt();
                        }
                    }
                    //unpin
                    else
                    {
                        if (IsTaskbarUnPinItem(verbName))
                        {
                            verb.DoIt();
                        }
                    }
                }
                //start menu
                else
                {
                    //pin
                    if (Pin_UnPin)
                    {
                        if (IsStartPinItem(verbName))
                        {
                            string ext = appexe.Split('.')[appexe.Split('.').Length - 1];
                            string exename = appexe.Substring(0, appexe.LastIndexOf("." + ext));
                            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                            startDir += @"\Microsoft\Windows\Start Menu\Programs\" + exename + ".lnk";
                            FileInfo fileshortcut = new FileInfo(startDir);
                            if (!fileshortcut.Exists)
                            {
                                verb.DoIt();
                            }
                        }
                    }
                    //unpin
                    else
                    {
                        if (IsStartUnPinItem(verbName))
                        {
                            verb.DoIt();
                        }
                    }
                }
            }

            shellApplication = null;
        }

        #region PinToWindows helper methods

        private bool IsTaskbarPinItem(string verbName)
        {
            //English locale
            if (verbName.ToLower().Contains("pin to taskbar"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.ToLower().Contains("tálcán") && !verbName.ToLower().Contains("feloldása"))
            {
                return true;
            }

            return false;
        }
        private bool IsTaskbarUnPinItem(string verbName)
        {
            //English locale
            if (verbName.ToLower().Contains("unpin from taskbar"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.ToLower().Contains("tálcán") && verbName.ToLower().Contains("feloldása"))
            {
                return true;
            }

            return false;
        }
        private bool IsStartPinItem(string verbName)
        {
            //English locale
            if (verbName.ToLower().Contains("pin to start"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.ToLower().Contains("start") && verbName.ToLower().Contains("rögzítés"))
            {
                return true;
            }

            return false;
        }
        private bool IsStartUnPinItem(string verbName)
        {
            //English locale
            if (verbName.ToLower().Contains("unpin from start"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.ToLower().Contains("start") && verbName.ToLower().Contains("feloldása"))
            {
                return true;
            }

            return false;
        }

        #endregion

        public void DeleteFromAutostart()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\Startup\" + Name + ".url";
            FileInfo fileshortcut = new FileInfo(startDir);
            if (fileshortcut.Exists)
            {
                fileshortcut.Delete();
            }
        }

        public void AddToAutostart()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\Startup";
            DirectoryInfo startPath = new DirectoryInfo(startDir);
            if (!startPath.Exists)
            {
                startPath.Create();
            }
            createShortcut(startDir + "\\" + Name + ".url");
        }

        public void TakeToRegistry(string ext)
        {
            if (!ext.StartsWith(".")) ext = "." + ext;

            IniFile iniFile = new IniFile(ConfigFile.FullName);
            RegistryKey key;
            key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
            RegistryKey fileExt;
            fileExt = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Microsoft", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Windows", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("CurrentVersion", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Explorer", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("FileExts", RegistryKeyPermissionCheck.ReadWriteSubTree)
                ;


            string[] subkeys = key.GetSubKeyNames();

            string appId = iniFile.IniReadValue("Details", "AppID");

            bool foundApp = false;
            bool foundExt = false;
            foreach (var keyname in subkeys)
            {
                if (keyname == appId)
                    foundApp = true;
                if (keyname == ext)
                    foundExt = true;
            }
            if (!foundApp)
            {
                string assoc = iniFile.IniReadValue("Associations", "FileTypeCommandLine");
                string exec = new FileInfo(ConfigFile.Directory.FullName + @"\..\..\" + iniFile.IniReadValue("Control", "Start")).FullName;

                if (assoc == "") assoc = "\"%1\"";

                key.CreateSubKey(appId);
                RegistryKey appkey = key.OpenSubKey(appId, RegistryKeyPermissionCheck.ReadWriteSubTree);
                appkey.SetValue("", Name);
                appkey.CreateSubKey("DefaultIcon");
                appkey.OpenSubKey("DefaultIcon", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("", "\"" + exec + "\",0");
                appkey.CreateSubKey("shell");
                appkey.OpenSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("", "open");
                appkey.OpenSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree).CreateSubKey("open");
                appkey.OpenSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("open", RegistryKeyPermissionCheck.ReadWriteSubTree).CreateSubKey("command");
                appkey.OpenSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("open", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("command", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("", "\"" + exec + "\" " + assoc);
                appkey.OpenSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("open", RegistryKeyPermissionCheck.ReadWriteSubTree).CreateSubKey("ddeexec");
                appkey.OpenSubKey("shell", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("open", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("ddeexec", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("", "");
            }

            if (foundExt)
            {
                key.DeleteSubKeyTree(ext);
            }

            key.CreateSubKey(ext);
            key.OpenSubKey(ext, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("", appId);


            //win8.1+

            subkeys = fileExt.GetSubKeyNames();
            foundExt = false;
            foreach (var keyname in subkeys)
            {
                if (keyname == ext)
                    foundExt = true;
            }
            if (foundExt)
            {
                RegistryKey extKey = fileExt.OpenSubKey(ext, RegistryKeyPermissionCheck.ReadWriteSubTree);

                try
                {
                    extKey.DeleteSubKey("UserChoice");
                }
                catch (Exception e){ }
                //if win8.1 or higher
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    if (Environment.OSVersion.Version.Minor >= 2)
                    {
                        return;
                    }
                }
                //if win7 or win8
                extKey.CreateSubKey("UserChoice");
                extKey.OpenSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("ProgId", appId);
            }
            else
            {
                //if everithing is awsome we never get there
                throw new Exception("reg corruption");
            }

        }

        public void DeleteFromRegistry()
        {
            //if (!ext.StartsWith(".")) ext = "." + ext;

            IniFile iniFile = new IniFile(ConfigFile.FullName);
            RegistryKey key;
            key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);

            RegistryKey fileExt;
            fileExt = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Microsoft", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Windows", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("CurrentVersion", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Explorer", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("FileExts", RegistryKeyPermissionCheck.ReadWriteSubTree)
                ;
            
            string[] subkeys = key.GetSubKeyNames();

            string appId = iniFile.IniReadValue("Details", "AppID");

            bool foundApp = false;
            foreach (var keyname in subkeys)
            {
                if (keyname == appId)
                {
                    foundApp = true;
                    break;
                }
            }
            if (foundApp)
            {
                key.DeleteSubKeyTree(appId);
            }

            foreach (var keyname in subkeys)
            {
                if (SupportedFileExtensions.Contains(keyname))
                {
                    if (key.OpenSubKey(keyname, RegistryKeyPermissionCheck.ReadWriteSubTree).GetValue("") == appId)
                    {
                        key.DeleteSubKey(keyname);
                        //fileExt
                    }
                }
            }

        }


    }
}
