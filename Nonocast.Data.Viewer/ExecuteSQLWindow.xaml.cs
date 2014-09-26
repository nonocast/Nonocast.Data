using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UniViewer {
	/// <summary>
	/// Interaction logic for ExecuteSQLWindow.xaml
	/// </summary>
	public partial class ExecuteSQLWindow : Window {
		public string SQL {
			get { return (string)GetValue(SQLProperty); }
			set { SetValue(SQLProperty, value); }
		}

		public ExecuteSQLWindow() {
			InitializeComponent();
			this.DataContext = this;
			TextBoxSQL.Focus();
		}

		private void Button_Click(object sender, RoutedEventArgs e) {
			this.DialogResult = true;
			this.Close();
		}

		public static readonly DependencyProperty SQLProperty =
			DependencyProperty.Register("SQL", typeof(string), typeof(ExecuteSQLWindow), new FrameworkPropertyMetadata("delete from document where type like '%business.arrangement%'"));
	}
}
