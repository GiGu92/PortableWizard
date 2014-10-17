using System;
using System.Collections.Generic;
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
        private ApplicationManager AppManager;

        public MainWindow()
        {
            AppManager = new ApplicationManager();
            InitializeComponent();
        }

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

		private void AppsPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			AppManager.SetApplicationList(AppsPathTextBox.Text);
			AppsCheckListBox.ItemsSource = AppManager.ApplicationList;
		}

        private void AppsPathBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                AppsPathTextBox.Text = dlg.SelectedPath;
            }
        }       

		private void ConfigFilePathBrowseButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			var result = dlg.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK)
			{
				ConfigFilePathTextBox.Text = dlg.FileName;
			}
		}

		private void AppsCheckListBox_ItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
		{
			AppManager.SetApplicationList(AppsPathTextBox.Text);
			SelectedAppsDataGrid.ItemsSource = AppsCheckListBox.SelectedItems;
		}


    }
}
