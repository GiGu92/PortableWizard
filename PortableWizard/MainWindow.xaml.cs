using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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

            appManager = new ApplicationManager();
            InitializeComponent();
        }

        private System.Reflection.Assembly ReferenceResolveEventHandler(object sender, ResolveEventArgs args)
        {
            if (!args.Name.Substring(0, args.Name.IndexOf(",")).StartsWith("Xceed")) return null;

            string folderPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\App\PortableWizard\bin\";

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

        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            Wizard.CurrentPage = AppChooser;
        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {
            Wizard.CurrentPage = UninstallAppChooser;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region AppChooser

        private void AppChooserAppsPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            appManager.SetApplicationList(AppChooserAppsPathTextBox.Text);
            AppChooserAppsCheckListBox.ItemsSource = appManager.ApplicationList;
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

        private void AppChooserConfigFilePathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                AppChooserConfigFilePathTextBox.Text = dlg.FileName;
            }
        }

        private void AppChooserAppsCheckListBox_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            appManager.SetApplicationList(AppChooserAppsPathTextBox.Text);
            ShortcutsChooserAppsDataGrid.ItemsSource = AppChooserAppsCheckListBox.SelectedItems;
            StartupChooserCheckListBox.ItemsSource = AppChooserAppsCheckListBox.SelectedItems;
            FileExtensionChooserProgramsListBox.ItemsSource = AppChooserAppsCheckListBox.SelectedItems;
			FileExtensionChooserProgramsListBox.UnselectAll();

            PortableWizard.Model.Application[] selectedItems = new PortableWizard.Model.Application[AppChooserAppsCheckListBox.SelectedItems.Count];
            AppChooserAppsCheckListBox.SelectedItems.CopyTo(selectedItems, 0);
            appManager.SelectedApplicationList = new ObservableCollection<PortableWizard.Model.Application>(selectedItems);
        }

        private void AppChooserSelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            AppChooserAppsCheckListBox.SelectedItemsOverride = AppChooserAppsCheckListBox.Items;
        }

        private void AppChooserDeselectAllButton_Click(object sender, RoutedEventArgs e)
        {
            AppChooserAppsCheckListBox.SelectedItemsOverride = new ObservableCollection<Object>();
        }

        #endregion

        #region ShortcutsChooser
        #endregion

        #region StartupChooser
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

        #endregion

        #region SummaryPage

        private void CreateIcon_Click(object sender, RoutedEventArgs e)
        {
            appManager.CreateShortcuts();
        }
        private void DeleteIcon_Click(object sender, RoutedEventArgs e)
        {
            appManager.DeleteShortcuts();
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

        private void PinToStart_Click(object sender, RoutedEventArgs e)
        {
            appManager.PinShortcutsToStart();
        }

        private void UnPinFromStart_Click(object sender, RoutedEventArgs e)
        {
            appManager.UnPinShortcutsFromStart();
        }

        private void Windetect_Click(object sender, RoutedEventArgs e)
        {
            bool x = appManager.IsPinToStartSupported;
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
            appManager.AddFileAssoc();
        }

        private void DeleteFileAssoc_Click(object sender, RoutedEventArgs e)
        {
            appManager.DeleteFileAssoc();
        }

        private void RestartExplorer_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.WinProcessRestarter.KillProcess("explorer.exe");
        }

        #endregion

        #region UninstallAppChooser

        private void UninstallAppChooserAppsPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            appManager.SetApplicationList(UninstallAppChooserAppsPathTextBox.Text);
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

        #region UninstallProgressPage

        private void UninstallProgressPage_Enter(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            appManager.DeleteShortcuts();
            System.Threading.Thread.Sleep(1000);
            (sender as BackgroundWorker).ReportProgress(33);

            appManager.DeleteStartMenuShortcuts();
            System.Threading.Thread.Sleep(1000);
            (sender as BackgroundWorker).ReportProgress(66);

            appManager.UnPinShortcutsFromTaskBar();
            System.Threading.Thread.Sleep(1000);
            (sender as BackgroundWorker).ReportProgress(100);
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case 33:
                    UninstallProgressPageTextBlock.Text += "\n\tDeleting shortcuts from start menu...";
                    break;
                case 66:
					UninstallProgressPageTextBlock.Text += "\n\tUnpinning shortcuts from taskbar...";
                    break;
                case 100:
					UninstallProgressPageTextBlock.Text += "\nFinished!";
                    UninstallProgressPage.CanFinish = true;
                    break;
            }

            UninstallProgressPageProgressBar.Value = e.ProgressPercentage;
        }

        #endregion UninstallProcessing

    }
}
