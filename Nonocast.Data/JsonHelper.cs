using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Nonocast.Data {
	internal class JsonHelper {
		public static string Serialize(object arg) {
			return JsonConvert.SerializeObject(arg);
		}

		public static T Deserialize<T>(string arg) {
			return JsonConvert.DeserializeObject<T>(arg);
		}

		public static object Deserialize(Type t, string arg) {
			return JsonConvert.DeserializeObject(arg, t);
		}
	}
}
