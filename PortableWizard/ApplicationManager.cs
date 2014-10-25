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
		public ObservableCollection<Application> SelectedApplicationList { get; set; }

        public ApplicationManager()
        {
            ApplicationList = new ObservableCollection<Application>();
			SelectedApplicationList = new ObservableCollection<Application>();
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

        public void CreateShortcuts() {
            foreach (var app in SelectedApplicationList)
            {
                if (app.IsDesktopShortcut)
                {
                    app.AddShortcutToDesktop();
                }
            }
        }

        public void DeleteShortcuts() {
            foreach (var app in SelectedApplicationList)
            {
                if (app.IsDesktopShortcut)
                {
                    app.DeleteShortcutFromDesktop();
                }
            }
        }

        public void CreateStartMenuShortcuts()
        {
			foreach (var app in SelectedApplicationList)
            {
                if (app.IsStartMenuShortcut)
                {
                    app.AddShortcutToStartMenu();
                }
            }
        }

        public void DeleteStartMenuShortcuts()
        {
			foreach (var app in SelectedApplicationList)
            {
                if (app.IsStartMenuShortcut)
                {
                    app.DeleteShortcutFromStartMenu();
                }
            }
        }

        public void PinShortcutsToTaskBar()
        {
			foreach (var app in SelectedApplicationList)
            {
                if (app.IsPinnedToTaskbar)
                {
                    app.PinShortcutToTaskBar();
                }
            }
        }

        public void UnPinShortcutsFromTaskBar()
        {
			foreach (var app in SelectedApplicationList)
            {
                if (app.IsPinnedToTaskbar)
                {
                    app.UnPinShortcutFromTaskBar();
                }
            }
        }

        public void PinShortcutsToStart()
        {
            foreach (var app in SelectedApplicationList)
            {
                if (app.IsPinnedToStart)
                {
                    app.PinShortcutToStart();
                }
            }
        }

        public void UnPinShortcutsFromStart()
        {
            foreach (var app in SelectedApplicationList)
            {
                if (app.IsPinnedToStart)
                {
                    app.UnPinShortcutFromStart();
                }
            }
        }


        public void AddToAutostart()
        {
            foreach (var app in SelectedApplicationList)
            {
                if (app.IsStartup)
                {
                    app.AddToAutostart();
                }
            }
        }

        public void DeleteFromAutostart()
        {
            foreach (var app in SelectedApplicationList)
            {
                if (app.IsStartup)
                {
                    app.DeleteFromAutostart();
                }
            }
        }

        public void AddFileAssoc()
        {
            foreach (var app in SelectedApplicationList)
            {
                if (app.HandledFileExtensions.Count>0)
                {
                    foreach (var ext in app.HandledFileExtensions)
                    {
                        app.TakeToRegistry(ext);
                    }
                }
            }
        }

        public void DeleteFileAssoc()
        {
            foreach (var app in SelectedApplicationList)
            {
                if (app.HandledFileExtensions.Count > 0)
                {
                    foreach (var ext in app.HandledFileExtensions)
                    {
                        app.DeleteFromRegistry(ext);
                    }
                }
            }
        }
    }
}
