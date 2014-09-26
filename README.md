Nonocast.Data
=============

Nonocast.Data是一个面向对象持久化中间层，向上给应用模型提供持久化和查询能力，向下集成SQLite数据库，直接嵌入应用程序。

### 你需要知道以下事项

- 基于.NET 4.0
- 采用C#实现
- 基于SQLite
- 对象基于JSON Schemaless存储
- 百万级数据级别
- 100%内存对象缓存
- 即时持久化
- 查询依赖.NET LINQ，不支持SQL
- 支持事务

关键在于百万级数据内摆脱SQL Server和Mysql的依赖。

### 安装

Package-Install Nonocast.Data

### 起步

对象创建和保存

``` csharp
class Program {
	static void Main(string[] args) {
		Objects.Instance.Open();

		var user = Objects.Instance.Create<User>();
		user.Name = "nonocast";
		user.Save();

		Console.WriteLine("press any key to exit.");
		Console.ReadLine();
		Objects.Instance.Close();
	}
}

public class User : BusinessObject {
	public string Name { get; set; }
}
```

通过SQLite CPL验证，

```
sqlite3 data.db "select content from document"
{"Name":"nonocast","Id":"7214a5ef-40fa-4a99-96ff-3b736589e6b8","Revision":"00000
000-0000-0000-0000-000000000000","_Link":{}}
```

对象加载和查询

``` csharp
class Program {
	static void Main(string[] args) {
		Objects.Instance.Open();
		Objects.Instance.Load();

		foreach (var each in Objects.Instance.Find<User>()) {
			Console.WriteLine(each.Name);
		}

		Console.WriteLine("press any key to exit.");
		Console.ReadLine();
		Objects.Instance.Close();
	}
}

public class User : BusinessObject {
	[JsonProperty]
	public string Name { get; set; }
}
```

### 关联关系

一对多

``` csharp
public class Area : BusinessObject {
	[JsonProperty]
	public string Name {
		get { return (string)GetValue(NameProperty); }
		set { SetValue(NameProperty, value); }
	}

	public ObservableCollection<Table> Tables {
		get { return GetValues<Table>("Tables"); }
		set { SetValues<Table>("Tables", value); }
	}

	public static readonly DependencyProperty NameProperty =
		DependencyProperty.Register("Name", typeof(string), typeof(Area), new PropertyMetadata(null));
}
```

