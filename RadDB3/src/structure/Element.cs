using System;

namespace RadDB3.structure {
	public abstract class Element{
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

		public static bool operator ==(Element a, Element b) {
			return a.Data == b.Data;
		}

		public static bool operator !=(Element a, Element b) {
			return a.Data != b.Data;
		}
	}
}