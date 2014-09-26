using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace UniViewer {
	public partial class App : Application {
		public static App Instance { get { return App.Current as App; } }
		public string FileName { get; private set; }

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			if (e.Args.Length > 0) {
				this.FileName = e.Args.First();
			}
		}
	}
}
