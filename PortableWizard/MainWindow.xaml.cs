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

namespace PortableWizard
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
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

		}

		private void AppsPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			AppsCheckListBox.ItemsSource = new List<string> { "Internet Explorer", "Safari", "Notepad", "Anyad" };
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

		private void ConfigFileBrowseButton_Click(object sender, RoutedEventArgs e)
		{

		}

		

		
	}
}
