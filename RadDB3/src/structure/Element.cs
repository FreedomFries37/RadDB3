using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using RadDB3.structure.Types;

namespace RadDB3.structure {
	public abstract class Element : StringParsable{
		private dynamic data;
		
		public dynamic Data {
			get => data;
			protected set => data = value;
		}

		public override int GetHashCode() {
			return HashNum();
		}

		public override string ToString() {
			return data.ToString();
		}

		/// <summary>
		/// Let n be length of string representing data stored
		/// H(s) = Sum( s[i]*31^(n-1-i) , i, 0, n-1)
		/// </summary>
		/// <returns>H(s)</returns>
		private int HashNum() {
			int output = 0;
			string str = ToString();
			for (int i = 0; i < str.Length; i++) {
				output += (int) (str[i] * Math.Pow(31, str.Length - 1 - i));
			}

			return output;
		}

		public abstract void ChangeData();

		public static bool operator ==(Element a, Element b) {
			return a.Data == b.Data;
		}

		public static bool operator !=(Element a, Element b) {
			return a.Data != b.Data;
		}

		public static Element ConvertToElement(Type type, object o) {
			ConstructorInfo constructorInfo = type.GetTypeInfo().DeclaredConstructors.ElementAt(0);
			return (Element) constructorInfo.Invoke(new[] {o});
		}

		public static Element ConvertToElement(object o) {
			ConstructorInfo constructorInfo = typeof(CallConvThiscall).GetTypeInfo().DeclaredConstructors.ElementAt(0);
			return (Element) constructorInfo.Invoke(new[] {o});
		}
	
	}
}