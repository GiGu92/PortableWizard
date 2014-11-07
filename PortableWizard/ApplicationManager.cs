using PortableWizard.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PortableWizard
{
	[XmlRoot]
	public class ApplicationManager
	{
		[XmlAttribute]
		public string AppsFolderPath { get; set; }

		[XmlArray]
		public ObservableCollection<Application> ApplicationList { get; set; }
		[XmlArray]
		public ObservableCollection<Application> SelectedApplicationList { get; set; }

		public ApplicationManager()
		{
			ApplicationList = new ObservableCollection<Application>();
			SelectedApplicationList = new ObservableCollection<Application>();
		}

		public void SetApplicationList(string AppsPath)
		{
			this.AppsFolderPath = AppsPath;
			LoadPortableApps();
			
		}

		private void LoadPortableApps()
		{
			foreach (var tmpapp in ApplicationList)
			{
				tmpapp.isNotFound = true;
			}
			var result = new ObservableCollection<Application>();
			if (AppsFolderPath.EndsWith("\\"))
			{
				AppsFolderPath = AppsFolderPath.Substring(0, AppsFolderPath.Length - 2);
			}

			List<string> currentAppNames = new List<string>();
			foreach (var app in ApplicationList)
			{
				currentAppNames.Add(app.Name);
			}

			if (Directory.Exists(AppsFolderPath))
			{
				DirectoryInfo directory = new DirectoryInfo(AppsFolderPath);				
				DirectoryInfo[] subDirs = directory.GetDirectories();

				foreach (DirectoryInfo dirInfo in subDirs)
				{
					string iniPath = dirInfo.FullName + @"\App\AppInfo\appinfo.ini";
					if (File.Exists(iniPath))
					{
						Application app = new Application(AppsFolderPath, dirInfo.Name);

						if (! currentAppNames.Contains(app.Name))
						{
							this.ApplicationList.Add(app);
						}
						else 
						{
							foreach (var tmpapp in ApplicationList)
							{
								if (tmpapp.Name == app.Name) 
								{
									bool isNew = tmpapp.isNew;
									tmpapp.InitUnserializedData(AppsFolderPath);
									tmpapp.isNew = isNew;
								}
							}
						}						
					}
				}
			}
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
				app.DeleteFromRegistry();
			}
		}
	}
}
