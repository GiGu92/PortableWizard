using PortableWizard.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableWizard.Model
{
    class Application
    {
        public string Name { get; set; }
        public bool IsDesktopShortcut { get; set; }
        public bool IsStartMenuShortcut { get; set; }
        public bool IsPinnedToStart { get; set; }
        public bool IsPinnedToTaskbar { get; set; }
        public bool IsStartup { get; set; }

        //public bool WillProcessed { get; set; }

        public List<string> SupportedFileExtensions { get; set; }
        public List<string> HandledFileExtensions { get; set; }

        public System.Windows.Media.Imaging.BitmapImage Icon { get; set; }

        private System.IO.FileInfo ConfigFile;
        public Application() { }

        public Application(System.IO.FileInfo iniFile)
        {
            ConfigFile = iniFile;

            IniFile file = new IniFile(ConfigFile.FullName);
            Name = file.IniReadValue("Details", "Name");
            IsDesktopShortcut = false;
            IsStartMenuShortcut = false;
            IsPinnedToStart = false;
            IsPinnedToTaskbar = false;
            //WillProcessed = false;
            IsStartup = false;

            SupportedFileExtensions = new List<string>();
            string association = file.IniReadValue("Associations", "FileTypes");
            if (association != "")
            {
                SupportedFileExtensions.AddRange(association.Split(','));
            }

            System.IO.FileInfo iconPath = new System.IO.FileInfo(ConfigFile.Directory.FullName + @"\appicon_32.png");
            if (iconPath.Exists)
            {
                Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(ConfigFile.Directory.FullName + @"\appicon_32.png"));
            }
        }

    }
}
