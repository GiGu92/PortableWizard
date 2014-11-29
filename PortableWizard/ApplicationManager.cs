using PortableWizard.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace PortableWizard
{
	/// <summary>
	/// Class that handles the portable applications
	/// </summary>
	[XmlRoot]
	public class ApplicationManager
	{
		[XmlAttribute]
		public string AppsFolderPath { get; set; }

		[XmlArray]
		public ObservableCollection<Application> ApplicationList { get; set; }
		[XmlArray]
		public ObservableCollection<Application> SelectedApplicationList { get; set; }

		/// <summary>
		/// Default constructor for ApplicationManager, initializes the ApplicationList and the
		/// SelectedApplicationList as empty collections.
		/// </summary>
		public ApplicationManager()
		{
			ApplicationList = new ObservableCollection<Application>();
			SelectedApplicationList = new ObservableCollection<Application>();
		}

		/// <summary>
		/// Sets the AppsFolderPath property and discovers the portable applications within the given folder.
		/// The found applications will be added to the ApplicationList
		/// </summary>
		/// <param name="appsPath">path to the folder</param>
		/// <param name="install">true if the program is inicializing the apps, false if it's uninstalling</param>
		public void SetApplicationList(string appsPath, bool install)
		{
			if (appsPath.EndsWith("\\"))
			{
				this.AppsFolderPath = appsPath.Substring(0, appsPath.Length - 2);
			}
			else
			{
				this.AppsFolderPath = appsPath;
			}

			LoadPortableApps();

			if (!install)
			{
				RemoveNotFoundApps();
			}
		}

		/// <summary>
		/// Helper method for portable application discovery.
		/// </summary>
		private void LoadPortableApps()
		{
			foreach (var tmpapp in ApplicationList)
			{
				tmpapp.isNotFound = true;
			}

			// Fetching the names of the applications currently in the ApplicationList
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

						// If the app is not already in the ApplicationList
						if (!currentAppNames.Contains(app.Name))
						{
							this.ApplicationList.Add(app);
						}
						// If the app is already in the ApplicationList (it was deserialized)
						else
						{
							// Fetching the matching app from the ApplicationList
							foreach (var tmpapp in ApplicationList)
							{
								if (tmpapp.Name == app.Name)
								{
									// Calling initialization of unserialized data with keeping the isNew property value
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

		/// <summary>
		/// Removes apps from the ApplicationList that are not in the apps folder.
		/// </summary>
		private void RemoveNotFoundApps()
		{
			var foundApps = new ObservableCollection<Application>();
			foreach (var app in ApplicationList)
			{
				if (!app.isNotFound)
				{
					foundApps.Add(app);
				}
			}

			this.ApplicationList = foundApps;
		}

		/// <summary>
		/// Creates desktop shortcuts for the applications that are in the
		/// SelectedApplicationsList and need desktop icons.
		/// </summary>
		public void CreateDesktopShortcuts()
		{
			foreach (var app in SelectedApplicationList)
			{
				if (app.NeedsDesktopShortcut)
				{
					app.AddShortcutToDesktop();
				}
			}
		}

		/// <summary>
		/// Deletes the desktop shortcuts of the selected applications.
		/// </summary>
		public void DeleteDesktopShortcuts()
		{
			foreach (var app in SelectedApplicationList)
			{
				app.DeleteShortcutFromDesktop();
			}
		}

		/// <summary>
		/// Creates start menu folders and shortcuts for the applications that are in the
		/// SelectedApplicationsList and need start menu icons.
		/// </summary>
		public void CreateStartMenuShortcuts()
		{
			foreach (var app in SelectedApplicationList)
			{
				if (app.NeedsStartMenuShortcut)
				{
					app.AddShortcutToStartMenu();
				}
			}
		}

		/// <summary>
		/// Deletes the start menu folders and shortcuts of the selected applications.
		/// </summary>
		public void DeleteStartMenuShortcuts()
		{
			foreach (var app in SelectedApplicationList)
			{
				app.DeleteShortcutFromStartMenu();
			}
		}

		/// <summary>
		/// Pins the selected applications to the taskbar, if they need to be pinned.
		/// </summary>
		public void PinShortcutsToTaskBar()
		{
			foreach (var app in SelectedApplicationList)
			{
				if (app.NeedsPinToTaskbar)
				{
					app.PinShortcutToTaskBar();
				}
			}
		}

		/// <summary>
		/// Unpins the selected applications from the taskbar.
		/// </summary>
		public void UnPinShortcutsFromTaskBar()
		{
			foreach (var app in SelectedApplicationList)
			{
				app.UnPinShortcutFromTaskBar();
			}
		}

		/// <summary>
		/// Adds the selected applications to the startup if they need to be startup applications.
		/// </summary>
		public void AddToAutostart()
		{
			foreach (var app in SelectedApplicationList)
			{
				if (app.NeedsToBeStartup)
				{
					app.AddToAutostart();
				}
			}
		}

		/// <summary>
		/// Removes the selected applications from the startup applications.
		/// </summary>
		public void DeleteFromAutostart()
		{
			foreach (var app in SelectedApplicationList)
			{
				app.RemoveFromAutostart();
			}
		}

		/// <summary>
		/// Adds the selected file associations for the selected applications.
		/// </summary>
		public void AddFileAssociations()
		{
			foreach (var app in SelectedApplicationList)
			{
				if (app.HandledFileExtensions.Count > 0)
				{
					foreach (var ext in app.HandledFileExtensions)
					{
						app.AddFileAssociationToRegistry(ext);
					}
				}
			}
		}

		/// <summary>
		/// Removes all file associations for the selected applications.
		/// </summary>
		public void DeleteFileAssociations()
		{
			foreach (var app in SelectedApplicationList)
			{
				app.RemoveFileAssociationFromRegistry();
			}
		}
	}
}
