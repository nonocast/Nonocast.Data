using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;

namespace Nonocast.Data {
	[JsonObject(MemberSerialization.OptIn)]
	public class BusinessObject : INotifyPropertyChanged {
		[JsonProperty]
		public Guid Id { get; set; }

		[JsonProperty]
		public Guid Revision { get; set; }

		public ObjectState State { get; set; }

		public long Sequence { get; set; }

		public Type Type { get { return this.GetType(); } }

		public DateTime CreatedAt { get; set; }

		public DateTime UpdatedAt { get; set; }
		public event PropertyChangedEventHandler PropertyChanged;

		public BusinessObject() {
			State = ObjectState.Added;
			CreatedAt = DateTime.Now;
			UpdatedAt = DateTime.Now;
		}

		public virtual void Save() {
			UpdateLink();
			FirePropertyChanged("AllProperties");
		}

		private void FirePropertyChanged(string property) {
			if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(property)); }
		}

		#region link

		public Dictionary<string, object> Link {
			get { return link; }
			set { link = value; }
		}

		[JsonProperty]
		public Dictionary<string, object> _Link {
			get { return _link; }
			set { _link = value; }
		}

		public T GetValue<T>(string property) where T : BusinessObject {
			T result = null;

			if (!Link.ContainsKey(property)) {
				if (_link.ContainsKey(property)) {

					Link[property] = Objects.Instance.FindById(Guid.Parse((string)_link[property])) as T;
					result = Link[property] as T;
				} else {
					result = null;
				}
			} else {
				result = Link[property] as T;
			}

			return result;
		}

		public void SetValue<T>(string property, T value) where T : BusinessObject {
			Link[property] = value;
			FirePropertyChanged(property);
		}

		public ObservableCollection<T> GetValues<T>(string property) where T : BusinessObject {
			ObservableCollection<T> result = null;

			if (!Link.ContainsKey(property)) {
				var tmp = new ObservableCollection<T>();
				if (_link.ContainsKey(property)) {
					foreach (var id in _link[property] as IList) {
						tmp.Add(Objects.Instance.FindById(Guid.Parse(id.ToString())) as T);
					}
				}
				Link[property] = tmp;
			}
			result = Link[property] as ObservableCollection<T>;

			return result;
		}

		public void SetValues<T>(string property, ObservableCollection<T> value) where T : BusinessObject {
			Link[property] = value;
			FirePropertyChanged(property);
		}

		// 注意:由于采用Lazy方式，避免造成将未加载的链接清除
		// 在Link中的Key表示已加载
		public void UpdateLink() {
			foreach (var each in Link) {
				if (each.Value is BusinessObject) {
					_link[each.Key] = (each.Value as BusinessObject).Id;
				} else if (each.Value is IList) {
					// 表示each.Value不为null，则表示已加载
					List<Guid> tmp = new List<Guid>();
					foreach (var biz in each.Value as IList) {
						tmp.Add((biz as BusinessObject).Id);
					}
					_link[each.Key] = tmp;
				} else if (each.Value == null) {
					_link[each.Key] = Guid.Empty;
				} else {
					throw new InvalidOperationException();
				}
			}
		}

		private Dictionary<string, object> link = new Dictionary<string, object>();
		private Dictionary<string, object> _link = new Dictionary<string, object>();
		#endregion
	}

	public enum ObjectState {
		Detached = 1,
		Unchanged = 2,
		Added = 4,
		Deleted = 8,
		Modified = 16
	}
}
