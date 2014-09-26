using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace InstallViewer {
	public class Program {
		static void Main(string[] args) {
			var SA2Program = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Nonocast.Data.Viewer.exe");
			var icon = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico");
			if (File.Exists(SA2Program) && File.Exists(icon)) {
				Register(SA2Program, icon);
				Console.WriteLine("REGISTER OK");
			} else {
				Console.WriteLine("CANNOT FIND Nonocast.Data.Viewer.exe");
				Console.WriteLine("FAILED");
			}
		}

		private static void Register(string programPath, string iconPath) {
			string template = @"Windows Registry Editor Version 5.00
[HKEY_CLASSES_ROOT\.db]
@=""nonocastdb""
[HKEY_CLASSES_ROOT\nonocastdb]
[HKEY_CLASSES_ROOT\nonocastdb\DefaultIcon]
@=""%IconPath%""
[HKEY_CLASSES_ROOT\nonocastdb\shell]
[HKEY_CLASSES_ROOT\nonocastdb\shell\open]
[HKEY_CLASSES_ROOT\nonocastdb\shell\open\command]
@=""\""%ProgramPath%\"" \""%1\""""";

			string regfile = Path.Combine(Path.GetTempPath(), "nonocastdb.reg");
			var result = template.Replace("%IconPath%", iconPath.Replace(@"\", @"\\"))
									.Replace("%ProgramPath%", programPath.Replace(@"\", @"\\"));
			File.WriteAllText(regfile, result, Encoding.Default);
			Process.Start(regfile);
		}
	}
}
