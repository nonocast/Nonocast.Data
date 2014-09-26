using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Nonocast.Data {
	public class Objects : ObjectCollection, IDeferable {
		public event Action LoadCompleted;
		public static Objects Instance { get { return instance; } }
		public string Password {
			get { return provider.Password; }
			set { provider.Password = value; }
		}
		public string DatabasePath {
			get { return dbpath; }
			set {
				dbpath = value;
				provider.DataSource = dbpath;
			}
		}
		public PersistenceBlock Defer {
			get { return new PersistenceBlock(this); }
		}

		public Objects() {
			this.CollectionChanged += OnCollectionChanged;
			provider = new SchemalessProvider();
			provider.DataSource = dbpath;
		}

		public void Open() {
			if (!File.Exists(dbpath)) {
				CreateDatabase();
			}
		}

		public void Close() {
			this.Clear();
		}

		public void CreateDatabase() {
			if (File.Exists(dbpath)) {
				File.Delete(dbpath);
			}

			provider.CreateDatabase();
		}

		public BusinessObject FindById(Guid id) {
			return this[id];
		}

		public new void Add(BusinessObject arg) {
			if (arg.State == ObjectState.Added) {
				arg.Id = Guid.NewGuid();
			}
			base.Add(arg);
		}

		public void AddRange(List<BusinessObject> arg) {
			arg.ForEach(p => Add(p));
		}

		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			if (e.Action == NotifyCollectionChangedAction.Add) {
				foreach (BusinessObject each in e.NewItems) {
					if (each.State == ObjectState.Added) {
						each.PropertyChanged += OnBusineessObjectPropertyChanged;
						OnObjectCreated(each, EventArgs.Empty);
					}
				}
			} else if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (BusinessObject each in e.OldItems) {
					each.PropertyChanged -= OnBusineessObjectPropertyChanged;
					OnObjectDeleted(each, EventArgs.Empty);
				}
			}
		}

		private void OnObjectCreated(object sender, EventArgs e) {
			var action = new CreateAction(sender as BusinessObject);
			if (!defer) {
				action.Execute(provider);
			} else {
				deferActions.Add(action);
			}
		}

		private void OnObjectDeleted(object sender, EventArgs e) {
			var action = new DeleteAction(sender as BusinessObject);
			if (!defer) {
				action.Execute(provider);
			} else {
				if (deferActions.Count(p => p.Target == action.Target) < 1) {
					// move to last
					deferActions.Remove(action);
					deferActions.Add(action);
				}
			}
		}

		void OnBusineessObjectPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == "AllProperties") {
				var action = new UpdateAction(sender as BusinessObject);
				if (!defer) {
					action.Execute(provider);
				} else {
					deferActions.Add(action);
				}
			}
		}

		public void BeginDefer() {
			defer = true;
		}

		public void EndDefer() {
			try {
				List<BusinessObject> deleteObjects = deferActions.Where(p => p.GetType() == typeof(DeleteAction)).Select(p => p.Target).ToList();
				if (deleteObjects.Count > 0) { provider.Delete(deleteObjects); }

				List<BusinessObject> createObjects = deferActions.Where(p => p.GetType() == typeof(CreateAction)).Select(p => p.Target).ToList();
				if (createObjects.Count > 0) { provider.Create(createObjects); }

				List<BusinessObject> updateObjects = deferActions.Where(p => p.GetType() == typeof(UpdateAction)).Select(p => p.Target).ToList();
				if (updateObjects.Count > 0) { provider.Update(updateObjects); }

			} catch (Exception ex) {
				throw ex;
			} finally {
				deferActions.Clear();
				defer = false;
			}
		}

		public T Create<T>() where T : BusinessObject {
			var result = Activator.CreateInstance(typeof(T)) as T;
			Add(result);
			return result;
		}

		public BusinessObject Create(Type type) {
			var result = Activator.CreateInstance(type) as BusinessObject;
			Add(result);
			return result;
		}

		public void Load() {
			provider.Load().ForEach(p => {
				p.PropertyChanged += OnBusineessObjectPropertyChanged;
				this.Add(p);
			});
			if (LoadCompleted != null) { LoadCompleted(); }
		}

		public List<T> Find<T>() where T : BusinessObject {
			return this.Where(p => p is T).Select(p => p).Cast<T>().ToList();
		}

		private List<PersistenceAction> deferActions = new List<PersistenceAction>();
		private string dbpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./data.db");
		private bool defer = false;
		private SchemalessProvider provider;
		private static Objects instance = new Objects();
	}

	public class ObjectCollection : KeyedCollection<Guid, BusinessObject>, INotifyCollectionChanged {
		protected override Guid GetKeyForItem(BusinessObject item) {
			return item.Id;
		}

		public new void Add(BusinessObject arg) {
			base.Add(arg);
			FireChanged(NotifyCollectionChangedAction.Add, arg);
		}

		public new void Remove(BusinessObject arg) {
			base.Remove(arg);
			FireChanged(NotifyCollectionChangedAction.Remove, arg);
		}

		private void FireChanged(NotifyCollectionChangedAction action, BusinessObject arg) {
			if (CollectionChanged != null) {
				CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, arg));
			}
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;
	}

	public interface IDeferable {
		void BeginDefer();
		void EndDefer();
	}

	public interface PersistenceProvider {
		string DataSource { get; set; }
		string Password { get; set; }
		int Count { get; }

		void CreateDatabase();
		List<BusinessObject> Load();
		List<BusinessObject> Load(int skip, int take);
		List<T> Load<T>() where T : BusinessObject;
		void Create(BusinessObject arg);
		void Delete(BusinessObject arg);
		void Update(BusinessObject arg);
		void Create(List<BusinessObject> arg);
		void Delete(List<BusinessObject> arg);
		void Update(List<BusinessObject> arg);
		void Reset();
	}

	public class PersistenceBlock : IDisposable {
		public PersistenceBlock(IDeferable defer) {
			this.defer = defer;
			defer.BeginDefer();
		}

		public void Dispose() {
			defer.EndDefer();
		}

		private IDeferable defer;
	}

	public interface PersistenceAction {
		BusinessObject Target { get; }
		void Execute(PersistenceProvider provider);
	}

	public abstract class PersistenceActionBase : PersistenceAction {
		public BusinessObject Target { get; private set; }
		public PersistenceActionBase(BusinessObject target) {
			Target = target;
		}

		public abstract void Execute(PersistenceProvider provider);
	}

	public class CreateAction : PersistenceActionBase {
		public CreateAction(BusinessObject target) : base(target) { }

		public override void Execute(PersistenceProvider provider) {
			provider.Create(Target);
		}
	}

	public class UpdateAction : PersistenceActionBase {
		public UpdateAction(BusinessObject target) : base(target) { }

		public override void Execute(PersistenceProvider provider) {
			provider.Update(Target);
		}
	}

	public class DeleteAction : PersistenceActionBase {
		public DeleteAction(BusinessObject target) : base(target) { }

		public override void Execute(PersistenceProvider provider) {
			provider.Delete(Target);
		}
	}
}
