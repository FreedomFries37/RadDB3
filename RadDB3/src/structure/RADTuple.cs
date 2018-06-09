using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using RadDB3.structure.Types;

namespace RadDB3.structure {
	public class RADTuple {
		public readonly Relation relation;
		public readonly Element[] elements;
		public readonly Type[] subTypes; //in order

		public RADTuple(Relation r, params Element[] objects) {
			relation = r;
			elements = objects;
			
		}

		public dynamic getValue(int index) {
			return Convert.ChangeType(elements[index], relation.Types[index]);
		}

		public bool getValue(int index, out dynamic value) {
			value = null;
			if (index >= elements.Length ||
				index < 0 ||
				elements[index] == null) return false;

			value = Convert.ChangeType(elements[index], relation.Types[index]);
			return true;
		}

		public static RADTuple CreateFromObjects(Relation r, params object[] o) {
			if (o.Length != r.Arity) return null;
			
			Element[] elements = new Element[r.Arity];
			for (int i = 0; i < elements.Length; i++) {
				ConstructorInfo constructorInfo = r.Types.ElementAt(i).GetTypeInfo().DeclaredConstructors.ElementAt(0);
				if (constructorInfo.GetParameters().Length == 1) {
					elements[i] = (Element) constructorInfo.Invoke(new[] {o[i]});
				} else {
					
					elements[i] = (Element) constructorInfo.Invoke(new[] {r.SubTypes[i], o[i]});
				}
				elements[i] = (Element) r.Types.ElementAt(i).GetTypeInfo().DeclaredConstructors.ElementAt(0).Invoke(new[] {o[i]});
			}
			
			return new RADTuple(r, elements);
		}

	}
	
}