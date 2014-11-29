using PortableWizard.Toolkit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;
using Xceed.Wpf.Toolkit;
using Xceed.Wpf.Toolkit.Primitives;

namespace PortableWizard
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ApplicationManager appManager;

		#region Load
		private int xceedLoadEvents = 0;

		public MainWindow()
		{

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.AssemblyResolve += new ResolveEventHandler(ReferenceResolveEventHandler);

			this.appManager = new ApplicationManager();
			Image logo = new Image();
			logo.Source = new BitmapImage(new Uri(@"Media\PortableWizard_logo.png", UriKind.Relative));
			logo.Width = 150;
			this.Logo = logo;
			this.LogoBackgroundBrush = new SolidColorBrush(new Color() { R = 28, G = 44, B = 61, A = 255 });

			InitializeComponent();
		}

		private System.Reflection.Assembly ReferenceResolveEventHandler(object sender, ResolveEventArgs args)
		{
			if (!args.Name.Substring(0, args.Name.IndexOf(",")).StartsWith("Xceed")) return null;

			string folderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\App\bin\";

			Assembly MyAssembly = null;
			string strTempAssmbPath = "";
			DirectoryInfo path = new DirectoryInfo(folderPath);
			if (!path.Exists) return null;
			FileInfo[] files = path.GetFiles("Xceed*", SearchOption.AllDirectories);

			Array.Sort(files, (x, y) => x.FullName.Length.CompareTo(y.FullName.Length));

			if (xceedLoadEvents < files.Length)
			{
				strTempAssmbPath = files[xceedLoadEvents].FullName;
				xceedLoadEvents++;
				MyAssembly = Assembly.LoadFrom(strTempAssmbPath);
			}

			//Return the loaded assembly.
			return MyAssembly;
		}
		#endregion Load

		#region IntroPage

		public Image Logo { get; set; }

		public SolidColorBrush LogoBackgroundBrush { get; set; }

		private void InitializeButton_Click(object sender, RoutedEventArgs e)
		{
			Wizard.CurrentPage = InstallWarningPage;
		}

		private void UninstallButton_Click(object sender, RoutedEventArgs e)
		{
			Wizard.CurrentPage = UninstallWarningPage;
		}
		private void IniEditorButton_Click(object sender, RoutedEventArgs e)
		{
			Wizard.CurrentPage = IniEditorWarningPage;
		}

		private void ExitButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#endregion

		#region WarningPage

		private void WarningPage_Loaded(object sender, RoutedEventArgs e)
		{
			WarningPageDearUserTextBlock.Text = "Dear " + Environment.UserName + "!\n\n";
			UninstallWarningPageDearUserTextBlock.Text = "Dear " + Environment.UserName + "!\n\n";
			IniEditorWarningPageDearUserTextBlock.Text = "Dear " + Environment.UserName + "!\n\n";
		}

		#endregion

		#region ConfigFileChooser

		private void ConfigFileChooserPathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				ConfigFileChooserPathTextBox.Text = dlg.FileName;
			}
		}

		private void ConfigFileChooserPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				XmlSerializer x = new XmlSerializer(appManager.GetType());
				FileStream fs = new FileStream(ConfigFileChooserPathTextBox.Text, FileMode.Open);
				this.appManager = x.Deserialize(fs) as ApplicationManager;
				foreach (var app in appManager.ApplicationList)
				{
					app.InitUnserializedData(appManager.AppsFolderPath);
				}
				fs.Close();

				AppChooserAppsPathTextBox.Text = this.appManager.AppsFolderPath;
				ConfigFileChooserErrorTextBlock.Foreground = new SolidColorBrush(Colors.Green);
				ConfigFileChooserErrorTextBlock.Text = "Configuration load successful.";
			}
			catch (Exception ex)
			{
				ConfigFileChooserErrorTextBlock.Foreground = new SolidColorBrush(Colors.Red);
				ConfigFileChooserErrorTextBlock.Text = ex.Message;
			}
		}

		#endregion

		#region AppChooser

		private void AppChooserAppsPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			appManager.SetApplicationList(AppChooserAppsPathTextBox.Text, true);
			AppChooserAppsCheckListBox.ItemsSource = appManager.ApplicationList;

			var selectedApps = new ObservableCollection<object>();
			List<string> selectedAppNames = new List<string>();
			foreach (var app in appManager.SelectedApplicationList)
			{
				selectedAppNames.Add(app.Name);
			}
			foreach (var app in AppChooserAppsCheckListBox.Items)
			{
				if (selectedAppNames.Contains((app as PortableWizard.Model.Application).Name))
				{
					selectedApps.Add(app);
				}
			}
			AppChooserAppsCheckListBox.SelectedItemsOverride = selectedApps;
			AppChooserAppsCheckListBox.Items.Refresh();

			// Ugh... this is ugly :(
			AppChooserAppsCheckListBox_ItemSelectionChanged(this, null);
		}

		private void AppChooserAppsPathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			var result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				AppChooserAppsPathTextBox.Text = dlg.SelectedPath;
			}
		}

		private void AppChooserAppsCheckListBox_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
		{
			// Removing items that are "not found" from the selection
			PortableWizard.Model.Application[] selectedItems = new PortableWizard.Model.Application[AppChooserAppsCheckListBox.SelectedItems.Count];
			AppChooserAppsCheckListBox.SelectedItems.CopyTo(selectedItems, 0);
			foreach (var app in selectedItems)
			{
				if ((app as PortableWizard.Model.Application).isNotFound)
				{
					AppChooserAppsCheckListBox.SelectedItemsOverride.Remove(app);
				}
			}

			// Setting ItemsSources for the other pages
			//appManager.SetApplicationList(AppChooserAppsPathTextBox.Text);
			ShortcutsChooserAppsDataGrid.ItemsSource = AppChooserAppsCheckListBox.SelectedItems;
			StartupChooserCheckListBox.ItemsSource = AppChooserAppsCheckListBox.SelectedItems;
			FileExtensionChooserProgramsListBox.ItemsSource = AppChooserAppsCheckListBox.SelectedItems;
			FileExtensionChooserProgramsListBox.UnselectAll();

			// Initializing the SelectedApplicationList property of the ApplicationManager
			selectedItems = new PortableWizard.Model.Application[AppChooserAppsCheckListBox.SelectedItems.Count];
			AppChooserAppsCheckListBox.SelectedItems.CopyTo(selectedItems, 0);
			appManager.SelectedApplicationList = new ObservableCollection<PortableWizard.Model.Application>(selectedItems);
		}

		private void AppChooserSelectAllButton_Click(object sender, RoutedEventArgs e)
		{
			object[] items = new object[AppChooserAppsCheckListBox.Items.Count];
			AppChooserAppsCheckListBox.Items.CopyTo(items, 0);
			ObservableCollection<object> itemsCollection = new ObservableCollection<object>(items);
			AppChooserAppsCheckListBox.SelectedItemsOverride = itemsCollection;
		}

		private void AppChooserDeselectAllButton_Click(object sender, RoutedEventArgs e)
		{
			AppChooserAppsCheckListBox.SelectedItemsOverride = new ObservableCollection<object>();
		}

		#endregion

		#region ShortcutsChooser

		private void ShortcutsChooserSelectAllButtons_Click(object sender, RoutedEventArgs e)
		{
			foreach (var app in appManager.SelectedApplicationList)
			{
				if ((sender.Equals(ShortcutsChooserDesktopSelectAllButton)))
				{
					app.NeedsDesktopShortcut = true;
				}
				else if ((sender.Equals(ShortcutsChooserStartMenuSelectAllButton)))
				{
					app.NeedsStartMenuShortcut = true;
				}
				else if ((sender.Equals(ShortcutsChooserTaskbarSelectAllButton)))
				{
					app.NeedsPinToTaskbar = true;
				}
			}

			ShortcutsChooserAppsDataGrid.Items.Refresh();
		}

		private void ShortcutsChooserDeselectAllButtons_Click(object sender, RoutedEventArgs e)
		{
			foreach (var app in appManager.SelectedApplicationList)
			{
				if ((sender.Equals(ShortcutsChooserDesktopDeselectAllButton)))
				{
					app.NeedsDesktopShortcut = false;
				}
				else if ((sender.Equals(ShortcutsChooserStartMenuDeselectAllButton)))
				{
					app.NeedsStartMenuShortcut = false;
				}
				else if ((sender.Equals(ShortcutsChooserTaskbarDeselectAllButton)))
				{
					app.NeedsPinToTaskbar = false;
				}
			}

			ShortcutsChooserAppsDataGrid.Items.Refresh();
		}

		#endregion

		#region StartupChooser

		private void StartupChooser_Enter(object sender, RoutedEventArgs e)
		{
			var startUpApps = new ObservableCollection<object>();
			foreach (var app in appManager.SelectedApplicationList)
			{
				if (app.NeedsToBeStartup)
				{
					startUpApps.Add(app);
				}
			}
			StartupChooserCheckListBox.SelectedItemsOverride = startUpApps;
		}

		private void StartupChooserCheckListBox_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
		{
			PortableWizard.Model.Application[] selectedItems = new PortableWizard.Model.Application[StartupChooserCheckListBox.SelectedItems.Count];
			StartupChooserCheckListBox.SelectedItems.CopyTo(selectedItems, 0);
			List<string> selectedAppNames = new List<string>();
			foreach (var app in selectedItems)
			{
				selectedAppNames.Add(app.Name);
			}

			foreach (var app in appManager.SelectedApplicationList)
			{
				if (selectedAppNames.Contains(app.Name))
				{
					app.NeedsToBeStartup = true;
				}
				else
				{
					app.NeedsToBeStartup = false;
				}
			}
		}

		private void StartupChooserSelectAllButton_Click(object sender, RoutedEventArgs e)
		{
			object[] items = new object[StartupChooserCheckListBox.Items.Count];
			StartupChooserCheckListBox.Items.CopyTo(items, 0);
			ObservableCollection<object> itemsCollection = new ObservableCollection<object>(items);
			StartupChooserCheckListBox.SelectedItemsOverride = itemsCollection;
		}

		private void StartupChooserDeselectAllButton_Click(object sender, RoutedEventArgs e)
		{
			StartupChooserCheckListBox.SelectedItemsOverride = new ObservableCollection<object>();
		}

		#endregion

		#region FileExtensionChooser

		private void FileExtensionChooserProgramsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

			List<string> supportedExtensions = null;
			List<string> handledExtensions = null;
			if (FileExtensionChooserProgramsListBox.Items.Count > 0)
			{
				var selectedApplication = (PortableWizard.Model.Application)FileExtensionChooserProgramsListBox.SelectedItem;
				if (selectedApplication != null)
				{
					supportedExtensions = selectedApplication.SupportedFileExtensions;
					handledExtensions = selectedApplication.HandledFileExtensions;
				}
			}
			FileExtensionChooserExtensionsCheckListBox.ItemsSource = supportedExtensions;
			FileExtensionChooserExtensionsCheckListBox.SelectedItemsOverride = handledExtensions;
		}

		private void FileExtensionChooserExtensionsCheckListBox_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
		{
			var selectedApplication = (PortableWizard.Model.Application)FileExtensionChooserProgramsListBox.SelectedItem;
			string[] selectedItems = new string[FileExtensionChooserExtensionsCheckListBox.SelectedItems.Count];
			FileExtensionChooserExtensionsCheckListBox.SelectedItems.CopyTo(selectedItems, 0);
			selectedApplication.HandledFileExtensions = new List<string>(selectedItems);
		}

		private void FileExtensionChooserSelectAllButton_Click(object sender, RoutedEventArgs e)
		{
			object[] items = new object[FileExtensionChooserExtensionsCheckListBox.Items.Count];
			FileExtensionChooserExtensionsCheckListBox.Items.CopyTo(items, 0);
			ObservableCollection<object> itemsCollection = new ObservableCollection<object>(items);
			FileExtensionChooserExtensionsCheckListBox.SelectedItemsOverride = itemsCollection;
		}

		private void FileExtensionChooserDeselectAllButton_Click(object sender, RoutedEventArgs e)
		{
			FileExtensionChooserExtensionsCheckListBox.SelectedItemsOverride = new ObservableCollection<object>();
		}

		#endregion

		#region SummaryPage

		private void SummaryPage_Enter(object sender, RoutedEventArgs e)
		{
			SummaryPageTextBlock.Text = "";

			SummaryPageTextBlock.Text += "Desktop shortcuts will be added for the following applications:";
			foreach (var app in appManager.SelectedApplicationList)
			{
				if (app.NeedsDesktopShortcut)
					SummaryPageTextBlock.Text += "\n\t" + app.Name;
			}

			SummaryPageTextBlock.Text += "\n\nStart menu shortcuts folders and shortcuts will be added for the following applications:";
			foreach (var app in appManager.SelectedApplicationList)
			{
				if (app.NeedsStartMenuShortcut)
					SummaryPageTextBlock.Text += "\n\t" + app.Name;
			}

			SummaryPageTextBlock.Text += "\n\nThe following applications will be pinned to the Taskbar:";
			foreach (var app in appManager.SelectedApplicationList)
			{
				if (app.NeedsPinToTaskbar)
					SummaryPageTextBlock.Text += "\n\t" + app.Name;
			}

			SummaryPageTextBlock.Text += "\n\nThe following applications will start with Windows:";
			foreach (var app in appManager.SelectedApplicationList)
			{
				if (app.NeedsToBeStartup)
					SummaryPageTextBlock.Text += "\n\t" + app.Name;
			}

			SummaryPageTextBlock.Text += "\n\nThe file associations will be added to your system:";
			foreach (var app in appManager.SelectedApplicationList)
			{
				if (app.HandledFileExtensions.Count > 0)
				{
					SummaryPageTextBlock.Text += "\n\n\t" + app.Name + ":";
					foreach (var extension in app.HandledFileExtensions)
					{
						SummaryPageTextBlock.Text += "\n\t\t" + extension;
					}
				}
			}
		}

		private void SummaryPageConfigSaveButton_Click(object sender, RoutedEventArgs e)
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.DefaultExt = "xml";
			dlg.AddExtension = true;
			var result = dlg.ShowDialog();

			if (result == System.Windows.Forms.DialogResult.OK)
			{
				XmlSerializer x = new XmlSerializer(appManager.GetType());
				FileStream fs = new FileStream(dlg.FileName, FileMode.Create);
				x.Serialize(fs, appManager);
				fs.Close();
			}
		}

		#endregion

		#region TestPage

		private void CreateIcon_Click(object sender, RoutedEventArgs e)
		{
			appManager.CreateDesktopShortcuts();
		}
		private void DeleteIcon_Click(object sender, RoutedEventArgs e)
		{
			appManager.DeleteDesktopShortcuts();
		}

		private void CreateStartMenuIcon_Click(object sender, RoutedEventArgs e)
		{
			appManager.CreateStartMenuShortcuts();
		}

		private void DeleteStartMenuIcon_Click(object sender, RoutedEventArgs e)
		{
			appManager.DeleteStartMenuShortcuts();
		}

		private void PinToTask_Click(object sender, RoutedEventArgs e)
		{
			appManager.PinShortcutsToTaskBar();
		}

		private void UnPinFromTask_Click(object sender, RoutedEventArgs e)
		{
			appManager.UnPinShortcutsFromTaskBar();
		}

		private void AddToAutostart_Click(object sender, RoutedEventArgs e)
		{
			appManager.AddToAutostart();
		}

		private void DeleteFromAutostart_Click(object sender, RoutedEventArgs e)
		{
			appManager.DeleteFromAutostart();
		}
		private void AddFileAssoc_Click(object sender, RoutedEventArgs e)
		{
			appManager.AddFileAssociations();
		}

		private void DeleteFileAssoc_Click(object sender, RoutedEventArgs e)
		{
			appManager.DeleteFileAssociations();
		}

		private void RestartExplorer_Click(object sender, RoutedEventArgs e)
		{
			Toolkit.WinProcessManager.KillProcess("explorer.exe");
		}

		#endregion

		#region UninstallAppChooser

		private void UninstallAppChooserAppsPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			appManager.SetApplicationList(UninstallAppChooserAppsPathTextBox.Text, false);
			UninstallAppChooserAppsCheckListBox.ItemsSource = appManager.ApplicationList;
		}

		private void UninstallAppChooserAppsPathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			var result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				UninstallAppChooserAppsPathTextBox.Text = dlg.SelectedPath;
			}
		}

		private void UninstallAppChooserAppsCheckListBox_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
		{
			PortableWizard.Model.Application[] selectedItems = new PortableWizard.Model.Application[UninstallAppChooserAppsCheckListBox.SelectedItems.Count];
			UninstallAppChooserAppsCheckListBox.SelectedItems.CopyTo(selectedItems, 0);
			appManager.SelectedApplicationList = new ObservableCollection<PortableWizard.Model.Application>(selectedItems);
		}

		private void UninstallAppChooserSelectAllButton_Click(object sender, RoutedEventArgs e)
		{
			UninstallAppChooserAppsCheckListBox.SelectedItemsOverride = UninstallAppChooserAppsCheckListBox.Items;
		}

		private void UninstallAppChooserDeselectAllButton_Click(object sender, RoutedEventArgs e)
		{
			UninstallAppChooserAppsCheckListBox.SelectedItemsOverride = new ObservableCollection<Object>();
		}

		#endregion

		#region ProgressPages

		private bool install = true;

		private void ProgressPage_Enter(object sender, RoutedEventArgs e)
		{
			BackgroundWorker worker = new BackgroundWorker();
			worker.WorkerReportsProgress = true;
			worker.DoWork += worker_DoWork;
			worker.ProgressChanged += worker_ProgressChanged;

			if (((WizardPage)sender).Equals(this.UninstallProgressPage)) install = false;

			worker.RunWorkerAsync(((WizardPage)sender).Title);
		}

		private void worker_DoWork(object sender, DoWorkEventArgs e)
		{
			if (!((string)e.Argument).StartsWith("Un"))
			{
				(sender as BackgroundWorker).ReportProgress(0);
				appManager.CreateDesktopShortcuts();

				(sender as BackgroundWorker).ReportProgress(15);
				appManager.CreateStartMenuShortcuts();

				(sender as BackgroundWorker).ReportProgress(30);
				appManager.PinShortcutsToTaskBar();

				(sender as BackgroundWorker).ReportProgress(45);
				appManager.AddToAutostart();

				(sender as BackgroundWorker).ReportProgress(60);
				appManager.AddFileAssociations();

				(sender as BackgroundWorker).ReportProgress(75);
				Toolkit.WinProcessManager.KillProcess("explorer.exe");
				System.Threading.Thread.Sleep(1000);

				(sender as BackgroundWorker).ReportProgress(90);
				Toolkit.WinProcessManager.StartProcessIfNotRunning("explorer.exe");

				(sender as BackgroundWorker).ReportProgress(100);


			}
			else
			{
				(sender as BackgroundWorker).ReportProgress(0);
				appManager.DeleteDesktopShortcuts();

				(sender as BackgroundWorker).ReportProgress(25);
				appManager.DeleteStartMenuShortcuts();

				(sender as BackgroundWorker).ReportProgress(50);
				appManager.UnPinShortcutsFromTaskBar();

				(sender as BackgroundWorker).ReportProgress(75);
				appManager.DeleteFileAssociations();

				(sender as BackgroundWorker).ReportProgress(100);
			}
		}

		void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			if (install)
			{
				switch (e.ProgressPercentage)
				{
					case 0:
						InstallProgressPageTextBlock.Text += "\n\tCreating shortcuts to desktop...";
						break;
					case 15:
						InstallProgressPageTextBlock.Text += "\n\tCreating shortcuts to start menu...";
						break;
					case 30:
						InstallProgressPageTextBlock.Text += "\n\tPinning shortcuts to taskbar...";
						break;
					case 45:
						InstallProgressPageTextBlock.Text += "\n\tPlace programs to autostart...";
						break;
					case 60:
						InstallProgressPageTextBlock.Text += "\n\tMake file type association...";
						break;
					case 75:
						InstallProgressPageTextBlock.Text += "\n\tRestarting explorer.exe...";
						break;
					case 90:
						InstallProgressPageTextBlock.Text += "\n\tCheck explorer.exe and start manually if not restarting...";
						break;
					case 100:
						InstallProgressPageTextBlock.Text += "\nFinished!";
						ProgressPage.CanFinish = true;
						break;
				}

				InstallProgressPageProgressBar.Value = e.ProgressPercentage;
			}
			else
			{
				switch (e.ProgressPercentage)
				{
					case 0:
						UninstallProgressPageTextBlock.Text += "\n\tDeleting shortcuts from desktop...";
						break;
					case 25:
						UninstallProgressPageTextBlock.Text += "\n\tDeleting shortcuts from start menu...";
						break;
					case 50:
						UninstallProgressPageTextBlock.Text += "\n\tUnpinning shortcuts from taskbar...";
						break;
					case 75:
						UninstallProgressPageTextBlock.Text += "\n\tDeleting file associations...";
						break;
					case 100:
						UninstallProgressPageTextBlock.Text += "\nFinished!";
						UninstallProgressPage.CanFinish = true;
						break;
				}

				UninstallProgressPageProgressBar.Value = e.ProgressPercentage;
			}
		}

		#endregion UninstallProcessing

		#region IniEditor

		#region IniEditor/Chooser

		private void IniDirChooserTextChanged(object sender, RoutedEventArgs e)
		{
			if (Directory.Exists(IniDirChooserDirPathTextBox.Text) && File.Exists(IniDirChooserAppExePathTextBox.Text))
			{
				IniDirChooser.CanSelectNextPage = true;
			}
		}

		private void IniDirChooserDirPathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			FolderBrowserDialog dlg = new FolderBrowserDialog();
			var result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				IniDirChooserDirPathTextBox.Text = dlg.SelectedPath;
			}
		}

		private void IniDirChooserAppExePathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			if (Directory.Exists(IniDirChooserDirPathTextBox.Text))
			{
				dlg.InitialDirectory = IniDirChooserDirPathTextBox.Text;
			}
			var result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				IniDirChooserAppExePathTextBox.Text = dlg.FileName;
			}
		}

		#endregion IniEditor/Chooser

		#region IniDataForm

		private void IniDatafill_Enter(object sender, RoutedEventArgs e)
		{
			string exename = (new FileInfo(IniDirChooserAppExePathTextBox.Text)).Name;
			exename = exename.Substring(0, exename.LastIndexOf('.'));

			// AppName -> App Name ; where AppNameASDWorking -> App Name ASD Working
			/////// http://stackoverflow.com/questions/272633/add-spaces-before-capital-letters
			string appname = Regex.Replace(exename, @"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))", " $0");
			appname = appname.Replace('.', ' ');
			appname = appname.Replace('_', ' ');
			appname = appname.Replace('-', ' ');

			string appid = exename;
			appid = appid.Replace(" ", ".");

			IniAppNameTextBox.Text = appname;
			IniAppAppIdTextBox.Text = appid;
			IniAppCommandLineTextBox.Text = "\"%1\"";
		}

		private void IniDataForm_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (!(IniAppNameTextBox.IsWatermarked || IniAppVersionTextBox.IsWatermarked ||
				IniAppSupportedExtensionsTextBox.IsWatermarked || IniAppAppIdTextBox.IsWatermarked || IniAppCommandLineTextBox.IsWatermarked))
			{
				IniDataForm.CanSelectNextPage = true;
			}
			else
			{
				IniDataForm.CanSelectNextPage = false;
			}
		}

		#endregion

		#region IniSummaryPage

		private void IniSummaryPage_Enter(object sender, RoutedEventArgs e)
		{
			DirectoryInfo baseDir = new DirectoryInfo(IniDirChooserDirPathTextBox.Text);
			bool appdir = false;
			bool appinfo = false;
			bool appini = false;
			bool appico = false;
			bool appicon = false;
			bool appexe = false;
			foreach (var dir in baseDir.GetDirectories())
			{
				if (dir.Name.Equals("App"))
				{
					appdir = true;
					foreach (var dir2 in dir.GetDirectories())
					{
						if (dir2.Name.Equals("AppInfo"))
						{
							appinfo = true;
							foreach (var file in dir2.GetFiles())
							{
								if (file.Name.Equals("appinfo.ini")) appini = true;
								if (file.Name.Equals("appicon.ico")) appico = true;
								if (file.Name.Equals("appicon_32.png")) appicon = true;
							}
						}
					}
				}
			}
			if (IniDirChooserAppExePathTextBox.Text.StartsWith(IniDirChooserDirPathTextBox.Text))
			{
				appexe = !IniDirChooserAppExePathTextBox.Text.Substring(IniDirChooserDirPathTextBox.Text.Length + 1).Contains("\\");
			}

			IniSummaryPageTextBlock.Text = "";
			IniSummaryPageTextBlock.Text += "New folders will be created:";
			if (!appdir)
				IniSummaryPageTextBlock.Text += "\n\t" + IniDirChooserDirPathTextBox.Text + "\\App";
			if (!appinfo)
				IniSummaryPageTextBlock.Text += "\n\t" + IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo";

			IniSummaryPageTextBlock.Text += "\n\nNew files will de created:";
			if (!appini)
				IniSummaryPageTextBlock.Text += "\n\t" + IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appinfo.ini";
			if (!appico)
				IniSummaryPageTextBlock.Text += "\n\t" + IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appicon.ico";
			if (!appicon)
				IniSummaryPageTextBlock.Text += "\n\t" + IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appicon_32.png";

		}

		#endregion

		#region IniSuccessPage

		private void IniSuccessPage_Enter(object sender, RoutedEventArgs e)
		{
			DirectoryInfo baseDir = new DirectoryInfo(IniDirChooserDirPathTextBox.Text);
			bool appdir = false;
			bool appinfo = false;
			bool appini = false;
			bool appico = false;
			bool appicon = false;
			bool appexe = false;
			foreach (var dir in baseDir.GetDirectories())
			{
				if (dir.Name.Equals("App"))
				{
					appdir = true;
					foreach (var dir2 in dir.GetDirectories())
					{
						if (dir2.Name.Equals("AppInfo"))
						{
							appinfo = true;
							foreach (var file in dir2.GetFiles())
							{
								if (file.Name.Equals("appinfo.ini")) appini = true;
								if (file.Name.Equals("appicon.ico")) appico = true;
								if (file.Name.Equals("appicon_32.png")) appicon = true;
							}
						}
					}
				}
			}

			if (IniDirChooserAppExePathTextBox.Text.StartsWith(IniDirChooserDirPathTextBox.Text))
			{
				appexe = !IniDirChooserAppExePathTextBox.Text.Substring(IniDirChooserDirPathTextBox.Text.Length + 1).Contains("\\");
			}

			if (!appdir)
			{
				(new DirectoryInfo(IniDirChooserDirPathTextBox.Text + "\\App")).Create();
			}
			if (!appinfo)
			{
				(new DirectoryInfo(IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo")).Create();
			}

			if (!appini)
			{
				string exename = (new FileInfo(IniDirChooserAppExePathTextBox.Text)).Name;
				exename = exename.Substring(0, exename.LastIndexOf('.'));
				string start = "";
				if (IniDirChooserAppExePathTextBox.Text.StartsWith(IniDirChooserDirPathTextBox.Text))
				{
					start = IniDirChooserAppExePathTextBox.Text.Substring(IniDirChooserDirPathTextBox.Text.Length + 1);
				}
				else
				{
					// somebody messed up something
					start = IniDirChooserDirPathTextBox.Text + exename + ".url";
				}

				IniFile ini = new IniFile(IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appinfo.ini");
				ini.IniWriteValue("Information", "Warning", "This is not a fully valid portableapps.com ini file!!!");
				ini.IniWriteValue("Details", "Name", IniAppNameTextBox.Text);
				ini.IniWriteValue("Details", "AppID", IniAppAppIdTextBox.Text);
				ini.IniWriteValue("Version", "DisplayVersion", IniAppVersionTextBox.Text);
				ini.IniWriteValue("Control", "Icons", "0");
				ini.IniWriteValue("Control", "Start", start);
				ini.IniWriteValue("Associations", "FileTypes", IniAppSupportedExtensionsTextBox.Text);
				ini.IniWriteValue("Associations", "FileTypeCommandLine", IniAppCommandLineTextBox.Text);

			}

			if (!appico)
			{
				System.Drawing.Icon icon = null;
				try
				{
					icon = System.Drawing.Icon.ExtractAssociatedIcon(IniDirChooserAppExePathTextBox.Text);
				}
				catch (System.ArgumentException)
				{
					// swallow and return nothing. You could supply a default Icon here as well
				}

				if (icon != null)
				{
					using (var stream = new System.IO.FileStream(IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appicon.ico", System.IO.FileMode.CreateNew))
					{
						icon.Save(stream);
					}
				}
			}

			if (!appicon)
			{
				Stream iconStream = new FileStream(IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appicon.ico", FileMode.Open);
				var decoder = new IconBitmapDecoder(
					iconStream,
					BitmapCreateOptions.PreservePixelFormat,
					BitmapCacheOption.None);

				foreach (var frame in decoder.Frames)
				{
					BitmapEncoder encoder = new PngBitmapEncoder();

					var rect = new Rect(0, 0, 32, 32);

					var group = new DrawingGroup();
					RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
					group.Children.Add(new ImageDrawing(frame, rect));

					var drawingVisual = new DrawingVisual();
					using (var drawingContext = drawingVisual.RenderOpen())
						drawingContext.DrawDrawing(group);

					var resizedImage = new RenderTargetBitmap(
						32, 32,					// Resized dimensions
						96, 96,					// Default DPI values
						PixelFormats.Default);	// Default pixel format
					resizedImage.Render(drawingVisual);

					encoder.Frames.Add(BitmapFrame.Create(resizedImage));
					var path = IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appicon_32.png";
					using (Stream saveStream = new FileStream(path, FileMode.Create))
					{
						encoder.Save(saveStream);
					}
					break;
				}
			}
		}

		private void HomeButton_Click(object sender, RoutedEventArgs e)
		{
			Wizard.CurrentPage = IntroPage;
		}

		private void OpenIniButton_Click(object sender, RoutedEventArgs e)
		{
			System.Diagnostics.Process.Start(IniDirChooserDirPathTextBox.Text + "\\App\\AppInfo\\appinfo.ini");
		}

		#endregion

		#endregion








	}
}
