using PortableWizard.Toolkit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PortableWizard.Model
{
    class Application
    {
        public string Name { get; set; }
		public BitmapImage Icon { get; set; }

        public bool IsDesktopShortcut { get; set; }
        public bool IsStartMenuShortcut { get; set; }
        public bool IsPinnedToStart { get; set; }
        public bool IsPinnedToTaskbar { get; set; }
        public bool IsStartup { get; set; }

        public List<string> SupportedFileExtensions { get; set; }
        public List<string> HandledFileExtensions { get; set; }        

        private FileInfo ConfigFile;

        public Application() { }

        public Application(FileInfo configFile)
        {
            ConfigFile = configFile;

            IniFile iniFile = new IniFile(ConfigFile.FullName);
            Name = iniFile.IniReadValue("Details", "Name");
            IsDesktopShortcut = true;
            IsStartMenuShortcut = true;
            IsPinnedToStart = false;
            IsPinnedToTaskbar = true;
            IsStartup = false;

            SupportedFileExtensions = new List<string>();
            string associations = iniFile.IniReadValue("Associations", "FileTypes");
            if (associations != "")
            {
                SupportedFileExtensions.AddRange(associations.Split(','));
            }

			HandledFileExtensions = new List<string>();

            FileInfo iconPath = new FileInfo(ConfigFile.Directory.FullName + @"\appicon_32.png");
            if (iconPath.Exists)
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
            PinToWindows(false, false);
        }

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

        /// <summary>
        /// This method pin/unpin the application to the taskbar or the StartMenu.
        /// </summary>
        /// <param name="TaskBar_StartMenu">If true; pin to taskbar. If false; pin to start menu.</param>
        /// <param name="Pin_UnPin">If true; pin. If false; unpin.</param>
        private void PinToWindows(bool TaskBar_StartMenu,bool  Pin_UnPin)
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
                        if (IsStartMenuPinItem(verbName))
                        {
                            verb.DoIt();
                        }
                    }
                    //unpin
                    else
                    {
                        if (IsStartMenuUnPinItem(verbName))
                        {
                            verb.DoIt();
                        }
                    }
                }
            }

            shellApplication = null;
        }

        private bool IsTaskbarPinItem(string verbName)
        {
            //English locale
            if (verbName.Equals("pin to taskbar"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.Contains("tálcán") && verbName.Contains("rögzítés"))
            {
                return true;
            }

            return false;
        }
        private bool IsTaskbarUnPinItem(string verbName)
        {
            //English locale
            if (verbName.Equals("unpin from taskbar"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.Contains("tálcán") && verbName.Contains("feloldása"))
            {
                return true;
            }

            return false;
        }
        private bool IsStartMenuPinItem(string verbName)
        {
            //English locale
            if (verbName.Equals("pin to Start"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.Contains("Start") && verbName.Contains("rögzítés"))
            {
                return true;
            }

            return false;
        }
        private bool IsStartMenuUnPinItem(string verbName)
        {
            //English locale
            if (verbName.Equals("unpin from Start"))
            {
                return true;
            }
            //Hungarian locale
            if (verbName.Contains("Start") && verbName.Contains("feloldása"))
            {
                return true;
            }

            return false;
        }

        
    }
}
