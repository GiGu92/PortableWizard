using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        public MainWindow()
        {
            appManager = new ApplicationManager();
            InitializeComponent();
        }

		#region IntroPage

		private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            Wizard.CurrentPage = AppChooser;
        }

        private void UninstallButton_Click(object sender, RoutedEventArgs e)
        {

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
				supportedExtensions = selectedApplication.SupportedFileExtensions;
				handledExtensions = selectedApplication.HandledFileExtensions;
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
            appManager.UnPinShortcutsToTaskBar();
		}

		#endregion

	}
}
