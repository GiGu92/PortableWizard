using Microsoft.Win32;
using PortableWizard.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace PortableWizard.Model
{
    /// <summary>
    /// Class that represents a portable application
    /// </summary>
    public class Application
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Version { get; set; }

        [XmlIgnore]
        public BitmapImage Icon { get; set; }

        [XmlAttribute]
        public bool NeedsDesktopShortcut { get; set; }
        [XmlAttribute]
        public bool NeedsStartMenuShortcut { get; set; }
        [XmlAttribute]
        public bool NeedsPinToTaskbar { get; set; }
        [XmlAttribute]
        public bool NeedsToBeStartup { get; set; }

        [XmlIgnore]
        public List<string> SupportedFileExtensions { get; set; }
        [XmlArray]
        public List<string> HandledFileExtensions { get; set; }

        [XmlAttribute]
        public string AppFolderName { get; set; }

        [XmlIgnore]
        public bool isNew { get; set; }
        [XmlIgnore]
        public bool isNotFound { get; set; }

        private FileInfo ConfigFile;

        public Application() { }

        /// <summary>
        /// Constructor for portable applications that meets the requirements of portableapps.com. This constructor
        /// initializes fields and properties of the application, from the appinfo.ini file, few that are not in the
        /// ini file, are set to default values.
        /// </summary>
        /// <param name="appsFolderPath">The folder the user keeps his/her portable applications</param>
        /// <param name="appFolderName">The name of the folder of the application</param>
        public Application(string appsFolderPath, string appFolderName)
        {
            this.AppFolderName = appFolderName;

            InitUnserializedData(appsFolderPath);

            IniFile iniFile = new IniFile(ConfigFile.FullName);
            this.Name = iniFile.IniReadValue("Details", "Name");
            this.Version = iniFile.IniReadValue("Format", "Version");
            this.NeedsDesktopShortcut = true;
            this.NeedsStartMenuShortcut = true;
            this.NeedsPinToTaskbar = false;
            this.NeedsToBeStartup = false;
            this.isNew = true;

            this.HandledFileExtensions = new List<string>();
        }

        /// <summary>
        /// Function that initializes data, that are not serialized when the user saves the configuration of Portable Wizard.
        /// </summary>
        /// <param name="appsFolderPath">The folder the user keeps his/her portable applications</param>
        public void InitUnserializedData(string appsFolderPath)
        {
            ConfigFile = new FileInfo(appsFolderPath + @"\" + this.AppFolderName + @"\App\AppInfo\appinfo.ini");
            IniFile iniFile = new IniFile(ConfigFile.FullName);

            this.isNotFound = !ConfigFile.Exists;

            // Loading icon
            FileInfo iconFile = new FileInfo(ConfigFile.Directory.FullName + @"\appicon_32.png");
            if (iconFile.Exists)
            {
                Icon = new BitmapImage(new Uri(ConfigFile.Directory.FullName + @"\appicon_32.png"));
            }

            this.isNew = false;

            // Loading supported extensions from ini file
            this.SupportedFileExtensions = new List<string>();
            string associations = iniFile.IniReadValue("Associations", "FileTypes");
            if (associations != "")
            {
                foreach (var association in associations.Split(','))
                {
                    SupportedFileExtensions.Add(association.Trim());
                }
            }
            this.SupportedFileExtensions.Sort();

            // Removing handled extensions if they are not supported any more
            if (this.HandledFileExtensions != null)
            {
                var handledExtensions = this.HandledFileExtensions.ToArray();
                foreach (var extension in handledExtensions)
                {
                    if (!SupportedFileExtensions.Contains(extension))
                    {
                        this.HandledFileExtensions.Remove(extension);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a shortcut for the application to the desktop
        /// </summary>
        public void AddShortcutToDesktop()
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            CreateShortcut(deskDir + "\\" + Name + ".url");
        }

        /// <summary>
        /// Deletes the shortcut for the application from the desktop
        /// </summary>
        public void DeleteShortcutFromDesktop()
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            File.Delete(deskDir + "\\" + Name + ".url");
        }

        /// <summary>
        /// Adds a folder and a shortcut for the application to the start menu
        /// </summary>
        public void AddShortcutToStartMenu()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\" + Name;
            DirectoryInfo startPath = new DirectoryInfo(startDir);
            if (!startPath.Exists)
            {
                startPath.Create();
            }
            CreateShortcut(startDir + "\\" + Name + ".url");
        }

        /// <summary>
        /// Deletes the shortcut for the application from the desktop
        /// </summary>
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

        /// <summary>
        /// Helper method for shortcut creating
        /// </summary>
        /// <param name="fullpath">Path to the location where the shortcut needs to be created</param>
        private void CreateShortcut(string fullpath)
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

        /// <summary>
        /// Pins the application to the taskbar
        /// </summary>
        public void PinShortcutToTaskBar()
        {
            PinToWindowsTaskbar(true);
        }

        /// <summary>
        /// Unpins the application from the taskbar
        /// </summary>
        public void UnPinShortcutFromTaskBar()
        {
            PinToWindowsTaskbar(false);
        }

        /// <summary>
        /// This method pin/unpin the application to the taskbar or the StartMenu.
        /// </summary>
        /// <param name="TaskBar_StartMenu">If true; pin to taskbar. If false; pin to start menu.</param>
        /// <param name="Pin_UnPin">If true; pin. If false; unpin.</param>
        private void PinToWindowsTaskbar(bool Pin_UnPin)
        {
            /*
             * This method use the windows application shell object.
             * Its looks like we programmatically right-click to the exe, and try to click to the "pin to taskbar" option.
             * The "pin to taskbar" text is locale specific see the helper functions below!
             * This method worked on win8 to pin applications to the start but in 8.1 and above this optin doesn't show up in the application shell.
             */
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
            //iterate tought the objects
            for (int i = 0; i < verbs.Count(); i++)
            {
                dynamic verb = verbs.Item(i);
                string verbName = verb.Name.Replace(@"&", string.Empty).ToLower();

                //pin
                if (Pin_UnPin)
                {
                    //if the option is seems to be "pin to start" one of the known locale
                    if (IsTaskbarPinItem(verbName))
                    {
                        //pin it
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

            shellApplication = null;
        }

        #region PinToWindows helper methods
        /*
         * These are localespecific helper methods, if you want this app to work on your locale and not just english or hungarian windowses,
         * you need to expand this functions.
        */
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

        #endregion

        /// <summary>
        /// Adds the application to the startup applications
        /// </summary>
        public void AddToAutostart()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\Startup";
            DirectoryInfo startPath = new DirectoryInfo(startDir);
            if (!startPath.Exists)
            {
                startPath.Create();
            }
            CreateShortcut(startDir + "\\" + Name + ".url");
        }

        /// <summary>
        /// Removes the application from the startup applications
        /// </summary>
        public void RemoveFromAutostart()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\Startup\" + Name + ".url";
            FileInfo fileshortcut = new FileInfo(startDir);
            if (fileshortcut.Exists)
            {
                fileshortcut.Delete();
            }
        }

        /// <summary>
        /// Adds a file association to the application in the registry
        /// </summary>
        /// <param name="ext">the file extension we want an association for e.g.: "avi" or ".avi"</param>
        public void AddFileAssociationToRegistry(string ext)
        {
            // Make the extension to ".avi" format
            if (!ext.StartsWith(".")) ext = "." + ext;

            IniFile iniFile = new IniFile(ConfigFile.FullName);
            RegistryKey key;
            
			// Navigate in the registry to the current_user/Software/Classes
            key = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree).OpenSubKey("Classes", RegistryKeyPermissionCheck.ReadWriteSubTree);
            
			// Navigate in the registry to the current_user/Software/Microsoft/CurrentVersion/Explorer/FileExts
            RegistryKey fileExt;
            fileExt = Registry.CurrentUser.OpenSubKey("Software", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Microsoft", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Windows", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("CurrentVersion", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("Explorer", RegistryKeyPermissionCheck.ReadWriteSubTree)
                .OpenSubKey("FileExts", RegistryKeyPermissionCheck.ReadWriteSubTree)
                ;

            string[] subkeys = key.GetSubKeyNames();
            
			// Read the portable app appid
            string appId = iniFile.IniReadValue("Details", "AppID");

            // If the class is not contains the appid, we register the app to the Classes
            if (!subkeys.Contains(appId))
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

            // If the extension is exist we delete it and recreate with our appid
            if (subkeys.Contains(ext))
            {
                key.DeleteSubKeyTree(ext);
            }

            key.CreateSubKey(ext);
            key.OpenSubKey(ext, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("", appId);

            //in the fileexts there are some user preferences to the choosed program
            //in win 8.1 or higher we just delete that node (After that the choos your app popup will appear if you try to open a file with the ext
            //in win 7 or win8 just rewrite the user choice value

            subkeys = fileExt.GetSubKeyNames();
            if (!subkeys.Contains(ext))
            {
                fileExt.CreateSubKey(ext);
            }
            RegistryKey extKey = fileExt.OpenSubKey(ext, RegistryKeyPermissionCheck.ReadWriteSubTree);

            try
            {
                extKey.DeleteSubKey("UserChoice");
            }
            catch { }

            //if win7 or win8 (not 8.1 or higher)
			string winVersionString = Environment.OSVersion.Version.Major + "." + Environment.OSVersion.Version.Minor;
			double winVersion = Double.Parse(winVersionString);
            if (winVersion < 6.2)
            {
                extKey.CreateSubKey("UserChoice");
                extKey.OpenSubKey("UserChoice", RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue("ProgId", appId);
            }

        }

        /// <summary>
        /// Removes a file association to the application in the registry
        /// </summary>
        /// <param name="ext">the file extension of which we want the association removed</param>
        public void RemoveFileAssociationFromRegistry()
        {
            //navigation in the registry, appid reading

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

            if (subkeys.Contains(appId))
            {
                key.DeleteSubKeyTree(appId);
            }

            foreach (var keyname in subkeys)
            {
                if (SupportedFileExtensions.Contains(keyname.Remove(0, 1)))
                {
                    //if supported and we are the choosen app in the registry delete the registry nodes!
					var currentKey = key.OpenSubKey(keyname, RegistryKeyPermissionCheck.ReadWriteSubTree);
					if (currentKey != null && currentKey.GetValue("")!= null && currentKey.GetValue("").Equals(appId))
                    {
                        key.DeleteSubKey(keyname);
                        if (fileExt.OpenSubKey(keyname, RegistryKeyPermissionCheck.ReadWriteSubTree).GetSubKeyNames().Contains("UserChoice"))
                        {
                            fileExt.OpenSubKey(keyname, RegistryKeyPermissionCheck.ReadWriteSubTree).DeleteSubKey("UserChoice");
                        }
                    }
                }
            }
        }


    }
}
