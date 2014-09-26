using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace UniViewer {
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			this.DataContext = this;
			this.Loaded += OnLoad;
		}

		private void OnLoad(object sender, RoutedEventArgs e) {
			if (!string.IsNullOrEmpty(App.Instance.FileName) && File.Exists(App.Instance.FileName)) {
				DocumentManager.Instance.Open(unipath = App.Instance.FileName);
			}
		}

		private void OpenUni(object sender, ExecutedRoutedEventArgs e) {
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.RestoreDirectory = true;
			dlg.Filter = "Uni Database|*.db";
			if (dlg.ShowDialog(this) ?? false) {
				try {
					DocumentManager.Instance.Open(unipath = dlg.FileName);
				} catch (Exception ex) {
					MessageBox.Show(ex.Message);
				}
			}
		}

		private void UpdateUni(object sender, ExecutedRoutedEventArgs e) {
			if (string.IsNullOrEmpty(unipath)) return;

			DocumentManager.Instance.Close();
			DocumentManager.Instance.Open(unipath);
		}

		private void ExecuteSQL(object sender, ExecutedRoutedEventArgs e) {
			var dlg = new ExecuteSQLWindow();
			if (dlg.ShowDialog() ?? false) {
				if (!string.IsNullOrEmpty(dlg.SQL)) {
					try {
						DocumentManager.Instance.Execute(dlg.SQL);
					} catch (Exception ex) {
						MessageBox.Show(ex.ToString());
					}
					DocumentManager.Instance.Close();
					DocumentManager.Instance.Open(unipath);
				}
			}
		}

		private void SaveSelectedDocument(object sender, ExecutedRoutedEventArgs e) {
			DocumentManager.Instance.Save(ListViewDocument.SelectedItem as Document, TextBoxContent.Text);
		}

		private void DeleteSelectedDocument(object sender, ExecutedRoutedEventArgs e) {
			DocumentManager.Instance.Delete(ListViewDocument.SelectedItem as Document);
		}

		private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e) {
			var view = (CollectionView)CollectionViewSource.GetDefaultView(ListViewDocument.ItemsSource);

			view.Filter = new Predicate<object>((p) => {
				var c = p as Document;
				return (c.Id.ToString().Contains(TextBoxSearch.Text)
				 || c.ObjectType.Contains(TextBoxSearch.Text)
				 || c.Content.Contains(TextBoxSearch.Text));
			});
		}

		private void ButtonShowAll_Click(object sender, RoutedEventArgs e) {
			TextBoxSearch.Text = string.Empty;
		}

		private string unipath;
	}

	public class MainWindowCommands {
		public static RoutedUICommand OpenCommand = new RoutedUICommand();
		public static RoutedUICommand UpdateCommand = new RoutedUICommand();
		public static RoutedUICommand ExecuteSqlCommand = new RoutedUICommand();
		public static RoutedUICommand SaveSelectedDocumentCommand = new RoutedUICommand();
		public static RoutedUICommand DeleteSelectedDocumentCommand = new RoutedUICommand();
	}
}
