using System;
using System.Linq;
using System.Reflection;

namespace RadDB3.structure.Types {
	public class RADGeneric<T> : RADGeneric {
	
		public RADGeneric(T data) : base(typeof(T), data) { }
		public RADGeneric() : base(typeof(T)) { }
	}

	public class RADGeneric : Element {
		public readonly Type type;
		
		public RADGeneric(Type t, object data) {
			type = t;
			if(data == null) return;
			Data = t.GetTypeInfo().DeclaredConstructors.ElementAt(0).Invoke(new[] {data});
		}

		public RADGeneric(Type t) {
			type = t;
		}
	}
}