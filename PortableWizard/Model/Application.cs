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
            IsPinnedToTaskbar = false;
            IsStartup = false;

            SupportedFileExtensions = new List<string>();
            string associations = iniFile.IniReadValue("Associations", "FileTypes");
            if (associations != "")
            {
                SupportedFileExtensions.AddRange(associations.Split(','));
            }

            FileInfo iconPath = new FileInfo(ConfigFile.Directory.FullName + @"\appicon_32.png");
            if (iconPath.Exists)
            {
                Icon = new BitmapImage(new Uri(ConfigFile.Directory.FullName + @"\appicon_32.png"));
            }
        }

        public void addShortcutToDesktop()
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

            IniFile iniFile = new IniFile(ConfigFile.FullName);
            string appexe = iniFile.IniReadValue("Control", "Start");
            FileInfo appPath = new FileInfo(ConfigFile.Directory.FullName + @"\..\..\"+appexe);

            using (StreamWriter writer = new StreamWriter(deskDir + "\\" + Name + ".url"))
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

        public void deleteShortcutFromDesktop()
        {
            string deskDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            File.Delete(deskDir + "\\" + Name + ".url");
        }

        public void addShortcutToStartMenu()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\" + Name;
            DirectoryInfo startPath = new DirectoryInfo(startDir);
            if (!startPath.Exists)
            {
                startPath.Create();
            }


            IniFile iniFile = new IniFile(ConfigFile.FullName);
            string appexe = iniFile.IniReadValue("Control", "Start");
            FileInfo appPath = new FileInfo(ConfigFile.Directory.FullName + @"\..\..\" + appexe);

            using (StreamWriter writer = new StreamWriter(startDir + "\\" + Name + ".url"))
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
        public void deleteShortcutFromStartMenu()
        {
            string startDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            startDir += @"\Microsoft\Windows\Start Menu\Programs\" + Name;
            DirectoryInfo startPath = new DirectoryInfo(startDir);
            startPath.Delete(true);
        }

        

    }
}
