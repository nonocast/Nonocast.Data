Nonocast.Data
=============

Nonocast.Data��һ���������־û��м�㣬���ϸ�Ӧ��ģ���ṩ�־û��Ͳ�ѯ���������¼���SQLite���ݿ⣬ֱ��Ƕ��Ӧ�ó���

### ����Ҫ֪����������

- ����.NET 4.0
- ����C#ʵ��
- ����SQLite
- �������JSON Schemaless�洢
- �������ݼ���
- 100%�ڴ���󻺴�
- ��ʱ�־û�
- ��ѯ����.NET LINQ����֧��SQL
- ֧������

�ؼ����ڰ��������ڰ���SQL Server��Mysql��������

### ��װ

Package-Install Nonocast.Data

### ��

���󴴽��ͱ���

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

ͨ��SQLite CPL��֤��

```
sqlite3 data.db "select content from document"
{"Name":"nonocast","Id":"7214a5ef-40fa-4a99-96ff-3b736589e6b8","Revision":"00000
000-0000-0000-0000-000000000000","_Link":{}}
```

������غͲ�ѯ

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

### ������ϵ

һ�Զ�

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

