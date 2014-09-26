using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace UniViewer {
	public class Document : DependencyObject {
		public Guid Id {
			get { return (Guid)GetValue(IdProperty); }
			set { SetValue(IdProperty, value); }
		}

		public string ObjectType {
			get { return (string)GetValue(ObjectTypeProperty); }
			set { SetValue(ObjectTypeProperty, value); }
		}

		public string Content {
			get { return (string)GetValue(ContentProperty); }
			set { SetValue(ContentProperty, value); }
		}

		public DateTime CreatedAt {
			get { return (DateTime)GetValue(CreatedAtProperty); }
			set { SetValue(CreatedAtProperty, value); }
		}

		public static readonly DependencyProperty CreatedAtProperty =
			DependencyProperty.Register("CreatedAt", typeof(DateTime), typeof(Document), new UIPropertyMetadata(DateTime.MinValue));

		public static readonly DependencyProperty ContentProperty =
			DependencyProperty.Register("Content", typeof(string), typeof(Document), new UIPropertyMetadata(null));

		public static readonly DependencyProperty ObjectTypeProperty =
			DependencyProperty.Register("ObjectType", typeof(string), typeof(Document), new UIPropertyMetadata(string.Empty));

		public static readonly DependencyProperty IdProperty =
			DependencyProperty.Register("Id", typeof(Guid), typeof(Document), new UIPropertyMetadata(Guid.Empty));
	}
}
