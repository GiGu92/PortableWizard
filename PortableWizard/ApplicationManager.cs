using PortableWizard.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortableWizard
{
    class ApplicationManager
    {
        private string AppsFolderPath;
        public ObservableCollection<Application> ApplicationList { get; set; }


        public ApplicationManager()
        {
            ApplicationList = new ObservableCollection<Application>();
        }

        public void SetApplicationList(string AppsPath)
        {
            this.AppsFolderPath = AppsPath;
            this.ApplicationList = GetPortableApps();
        }

		private ObservableCollection<Application> GetPortableApps()
        {
			var result = new ObservableCollection<Application>();
            if (AppsFolderPath.EndsWith("\\"))
            {
                AppsFolderPath = AppsFolderPath.Substring(0, AppsFolderPath.Length - 2);
            }

			if (Directory.Exists(AppsFolderPath))
			{
				DirectoryInfo directory = new DirectoryInfo(AppsFolderPath);				
				DirectoryInfo[] subDirs = directory.GetDirectories();

				foreach (DirectoryInfo dirInfo in subDirs)
				{
					string iniPath = dirInfo.FullName + @"\App\AppInfo\appinfo.ini";
					FileInfo iniFile = new FileInfo(iniPath);
					if (iniFile.Exists)
					{
						Application app = new Application(iniFile);
						result.Add(app);
					}
				}
				
			}

			return result;
        }

        public void createShortcuts() {
            foreach (var app in ApplicationList)
            {
                if (app.IsDesktopShortcut)
                {
                    app.addShortcutToDesktop();
                }
            }
        }

        public void deleteShortcuts() {
            foreach (var app in ApplicationList)
            {
                if (app.IsDesktopShortcut)
                {
                    app.deleteShortcutFromDesktop();
                }
            }
        }

        public void createStartMenuShortcuts()
        {
            foreach (var app in ApplicationList)
            {
                if (app.IsStartMenuShortcut)
                {
                    app.addShortcutToStartMenu();
                }
            }
        }

        public void deleteStartMenuShortcuts()
        {
            foreach (var app in ApplicationList)
            {
                if (app.IsStartMenuShortcut)
                {
                    app.deleteShortcutFromStartMenu();
                }
            }
        }

        public void pinShortcutsToTaskBar()
        {
            foreach (var app in ApplicationList)
            {
                if (app.IsStartMenuShortcut)
                {
                    app.pinShortcutToTaskBar();
                }
            }
        }

        public void unPinShortcutsToTaskBar()
        {
            foreach (var app in ApplicationList)
            {
                if (app.IsStartMenuShortcut)
                {
                    app.unPinShortcutToTaskBar();
                }
            }
        }

    }
}
