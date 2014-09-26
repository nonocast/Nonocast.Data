using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SQLite;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;

namespace Nonocast.Data {
	public class SchemalessProvider : PersistenceProvider {
		public string DataSource { get; set; }
		public string Password { get; set; }

		public int Count {
			get {
				int result = 0;
				string sql = @"SELECT COUNT(*) FROM [Document] ORDER BY [Sequence]";
				OpenConnection(c => {
					SQLiteCommand command = new SQLiteCommand(sql, c);
					result = Convert.ToInt32(command.ExecuteScalar());
				});
				return result;
			}
		}

		public void CreateDatabase() {
			OpenConnection(c => {
				SQLiteCommand command = c.CreateCommand();
				command.CommandText = @"CREATE TABLE [Document] (
												[Sequence] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
												[Id] GUID NOT NULL,
												[CreatedAt] DATETIME DEFAULT (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime')),
												[UpdatedAt] DATETIME DEFAULT (strftime('%Y-%m-%d %H:%M:%f', 'now', 'localtime')),
												[Type] TEXT NOT NULL,
												[Content] TEXT NOT NULL);
										CREATE INDEX [Id_Index] on [Document]([Id]);";

				command.ExecuteNonQuery();
			});
		}

		public List<BusinessObject> Load() {
			List<BusinessObject> result = new List<BusinessObject>();

			string sql = @"SELECT * FROM [Document] ORDER BY [Sequence]";
			OpenConnection(c => {
				SQLiteCommand command = new SQLiteCommand(sql, c);

				string exid = string.Empty;
				var reader = command.ExecuteReader();
				while (reader.Read()) {
					try {
						exid = reader["Sequence"].ToString();

						var type = Type.GetType(reader["Type"] as string);
						var target = Eval(type, reader["Content"] as string);

						target.Sequence = (long)reader["Sequence"];
						target.CreatedAt = (DateTime)reader["CreatedAt"];
						target.UpdatedAt = (DateTime)reader["UpdatedAt"];
						target.State = ObjectState.Unchanged;

						result.Add(target);
					} catch (Exception ex) {
						throw new InvalidDataException(exid, ex);
					}
				}
			});
			return result;
		}

		public List<BusinessObject> Load(int skip, int take) {
			throw new NotImplementedException();
		}

		public List<T> Load<T>() where T : BusinessObject {
			List<T> result = new List<T>();

			string sql = @"SELECT * FROM [Document] WHERE [Type]=@Type ORDER BY [Sequence]";
			OpenConnection(c => {
				SQLiteCommand command = new SQLiteCommand(sql, c);
				command.Parameters.Add(new SQLiteParameter("@Type", typeof(T).AssemblyQualifiedName));

				var reader = command.ExecuteReader();
				while (reader.Read()) {
					var type = Type.GetType(reader["Type"] as string);
					var target = Eval(type, reader["Content"] as string);

					target.Sequence = (long)reader["Sequence"];
					target.CreatedAt = (DateTime)reader["CreatedAt"];
					target.UpdatedAt = (DateTime)reader["UpdatedAt"];
					target.State = ObjectState.Unchanged;

					result.Add(target as T);
				}
			});
			return result;
		}

		public void Create(BusinessObject arg) {
			Create(new List<BusinessObject>() { arg });
		}

		public void Delete(BusinessObject arg) {
			Delete(new List<BusinessObject>() { arg });
		}

		public void Update(BusinessObject arg) {
			Update(new List<BusinessObject>() { arg });
		}

		public void Create(List<BusinessObject> arg) {
			string sql = @"INSERT INTO [Document] ([Id],[Type],[Content]) VALUES (@Id,@Type,@Content);";

			OpenConnection(c => {
				using (var transaction = c.BeginTransaction()) {
					try {
						SQLiteCommand command = new SQLiteCommand(sql, c);

						foreach (var each in arg) {
							command.Parameters.Add(new SQLiteParameter("@Id", each.Id));
							command.Parameters.Add(new SQLiteParameter("@Type", each.Type.AssemblyQualifiedName));
							command.Parameters.Add(new SQLiteParameter("@Content", JsonHelper.Serialize(each)));
							command.ExecuteNonQuery();
						}

						transaction.Commit();
						arg.ForEach(p => p.State = ObjectState.Unchanged);
					} catch {
						transaction.Rollback();
						arg.ForEach(p => p.State = ObjectState.Added);
						throw;
					}
				}
			});
		}

		public void Delete(List<BusinessObject> arg) {
			string sql = @"DELETE FROM [Document] WHERE [Id]=@Id";

			OpenConnection(c => {
				using (var transaction = c.BeginTransaction()) {
					try {
						SQLiteCommand command = new SQLiteCommand(sql, c);

						foreach (var each in arg) {
							command.Parameters.Add(new SQLiteParameter("@Id", each.Id));
							each.Id = Guid.Empty;
							command.ExecuteNonQuery();
						}

						transaction.Commit();
					} catch {
						transaction.Rollback();
						throw;
					}
				}
			});
		}

		public void Update(List<BusinessObject> arg) {
			string sql = @"UPDATE [Document] SET [Content]=@Content WHERE [Id]=@Id;";

			OpenConnection(c => {
				using (var transaction = c.BeginTransaction()) {
					try {
						SQLiteCommand command = new SQLiteCommand(sql, c);

						foreach (var each in arg) {
							command.Parameters.Add(new SQLiteParameter("@Id", each.Id));
							command.Parameters.Add(new SQLiteParameter("@Content", JsonHelper.Serialize(each)));
							command.ExecuteNonQuery();
						}

						transaction.Commit();
					} catch {
						transaction.Rollback();
						throw;
					}
				}
			});
		}

		public void Reset() {
			string resetSql = @"DELETE FROM [Document];";
			string vacuumSql = @"VACUUM";

			OpenConnection(c => {
				SQLiteCommand command1 = new SQLiteCommand(resetSql, c);
				command1.ExecuteNonQuery();

				SQLiteCommand command2 = new SQLiteCommand(vacuumSql, c);
				command2.ExecuteNonQuery();
			});
		}

		private void OpenConnection(SQLiteAction arg) {
			try {
				using (SQLiteConnection connection = new SQLiteConnection(connectionString)) {
					if (string.IsNullOrEmpty(Password)) {
						connection.SetPassword(Password);
					}
					connection.Open();
					arg(connection);
				}
			} catch (Exception ex) {
				throw new ApplicationException(ex.Message);
			}
		}

		private BusinessObject Eval(Type type, string content) {
			return JsonHelper.Deserialize(type, content) as BusinessObject;
		}

		private string connectionString {
			get { return string.Format("data source={0}", DataSource); }
		}
	}

	internal delegate void SQLiteAction(SQLiteConnection c);
}
