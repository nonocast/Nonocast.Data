using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Configuration;
using Newtonsoft.Json.Linq;

namespace UniViewer {
	public class DocumentManager : ObservableCollection<Document> {
		public static DocumentManager Instance { get { return instance; } }

		public void Open(string unipath) {
			this.Clear();
			this.unipath = unipath;

			using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
				connection.Open();
				string sql = @"select * from Document order by CreatedAt";
				SQLiteCommand command = new SQLiteCommand(sql, connection);

				var reader = command.ExecuteReader();
				while (reader.Read()) {
					Document document = new Document();
					document.Id = (Guid)reader["Id"];
					document.ObjectType = GetTypeName(reader["Type"] as string);
					document.CreatedAt = Convert.ToDateTime(reader["CreatedAt"]);
					//document.Content = reader["Content"] as string;
					document.Content = JObject.Parse(reader["Content"] as string).ToString();
					this.Add(document);
				}
			}
		}

		public void Execute(String sql) {
			using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
				connection.Open();
				SQLiteCommand command = new SQLiteCommand(sql, connection);
				command.ExecuteNonQuery();
			}
		}

		public void Save(Document document, string content) {
			string sql = @"update Document set Content=@Content where Id=@Id;";

			using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
				connection.Open();
				SQLiteCommand command = new SQLiteCommand(sql, connection);

				command.Parameters.Add(new SQLiteParameter("Id", document.Id));
				command.Parameters.Add(new SQLiteParameter("Content", content));
				command.ExecuteNonQuery();
			}
		}

		public void Delete(Document document) {
			string sql = @"delete from Document where Id=@Id;";

			using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
				connection.Open();
				SQLiteCommand command = new SQLiteCommand(sql, connection);

				command.Parameters.Add(new SQLiteParameter("Id", document.Id));
				command.ExecuteNonQuery();
			}
		}

		private string GetTypeName(string arg) {
			string result = string.Empty;
			arg = arg.Substring(0, arg.IndexOf(',')).Trim();
			string[] segs = arg.Split(new char[] { '.' });
			result = segs[segs.Length - 1].Trim();
			return result;
		}

		public void Close() {
			this.Clear();
		}

		private static DocumentManager instance = new DocumentManager();
		private string connectionString {
			get {
				string result = string.Format("data source={0};Version=3;", unipath);
				string pwd = ConfigurationManager.AppSettings["password"];
				if (!string.IsNullOrEmpty(pwd)) {
					result += string.Format("password={0};", pwd);
				}
				return result;
			}
		}
		private string unipath;
	}
}
