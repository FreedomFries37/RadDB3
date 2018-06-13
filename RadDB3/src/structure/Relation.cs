using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RadDB3.structure.Types;

namespace RadDB3.structure {
	public class Relation : IEnumerable<(string, Type)>, ICloneable{
		private readonly Type[] types;
		private readonly Type[] subTypes;
		private readonly string[] names;
		
		/*
		 * Key 0 -> Primary Key
		 * Keys 1 - infinity -> Secondary Keys
		 */
		private readonly int[] keys;
		public int PrimaryKey => keys[0];
		
		public string PrimaryKeyName => names[PrimaryKey];
		
		public int Arity => names.Length;

		public Type[] Types => types;

		public Type[] SubTypes => subTypes;

		public string[] Names => names;

		public int[] Keys => keys;
		
		public Relation(params NameTypePair[] pairs) : this(ConvertNtpToTuples(pairs)) { }

		private static (string, Type)[] ConvertNtpToTuples(NameTypePair[] pairs) {
			(string, Type)[] output = new (string, Type)[pairs.Length];
			int index = 0;
			foreach (NameTypePair nameTypePair in pairs) {
				output[index] = (nameTypePair.Name, nameTypePair.Type);
				index++;
			}

			return output;
		}
		

		public Relation(params (string, Type)[] pairs) {
			int index = 0;
			types = new Type[pairs.Length];
			subTypes = new Type[pairs.Length];
			names = new string[pairs.Length];
			LinkedList<int> newKeys = new LinkedList<int>();


			foreach ((string, Type) valueTuple in pairs) {
				string name = valueTuple.Item1;
				if (name.Contains("*")) {
					newKeys.AddFirst(index);
					name = name.Substring(1);
				}else if (name.Contains("&")) {
					newKeys.AddLast(index);
					name = name.Substring(1);
				}
				names[index] = name;
				Type t = valueTuple.Item2;
				if (valueTuple.Item2.IsSubclassOf(typeof(Element))) {
					types[index] = valueTuple.Item2;
					if (valueTuple.Item2 == typeof(RADGeneric<>)) {
						subTypes[index] = typeof(RADGeneric);
					} else {
						subTypes[index] = null;
					}

				} else {
					types[index] = new RADGeneric(valueTuple.Item2).GetType();
					subTypes[index] = valueTuple.Item2;
				}

				index++;
			}

			if (newKeys.Count == 0) newKeys.AddFirst(0);
			keys = newKeys.ToArray();
			
		}

		public Relation(Relation r, string name1, params string[] optionalNames) : this(
			ConvertRelationAndStrings(r, name1, optionalNames)) { }

		private static NameTypePair[] ConvertRelationAndStrings(Relation r, string name1, params string[] optionalNames) {
			NameTypePair[] pairs = new NameTypePair[1 + optionalNames.Length];
			if (r.IsKey(name1) >= 0) {
				string keyInfo = r.IsKey(name1) == 1 ? "*" : r.IsKey(name1) == 2 ? "&" : "";
				pairs[0] = new NameTypePair(keyInfo + name1, r.Types[r[name1]]);
			}

			for (int i = 0; i < optionalNames.Length; i++) {
				if (r.IsKey(optionalNames[i]) >= 0) {
					string keyInfo = r.IsKey(optionalNames[i]) == 1 ? "*" : r.IsKey(optionalNames[i]) == 2 ? "&" : "";
					pairs[1 + i] = new NameTypePair(keyInfo + name1, r.Types[r[name1]]);
				}
			}

			return pairs;
		}

		public string this[int i] => names[i];
		public int this[string s] => names.ToList().IndexOf(s);

		/// <summary>
		/// Returns -1 if not a key
		/// 		0 if exists but not a key
		/// 		1 if primary key
		/// 		2 if secondary key
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public int IsKey(string name) {
			if (this[name] == -1) return -1;
			int index = this[name];
			if (keys[0] == index) return 1;
			if (keys.ToList().Contains(index)) return 2;
			return 0;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IEnumerator<(string, Type)> GetEnumerator() {
			List<(string, Type)> output = new List<(string, Type)>();
			for (int i = 0; i < Arity; i++) {
				string name = "";
				if (i == Keys[0]) name += "*";
				else if (Keys.Contains(i)) name += "&";
				name += names[i];
				output.Add((name, types[i]));
			}

			return output.GetEnumerator();
		}

		public override string ToString() {
			string output = "";
			for (int i = 0; i < Arity-1; i++) {
				output += names[i] + "-";
			}

			output += names[Arity - 1];
			return output;
		}

		public string Dump() {
			string output = "";
			for (int i = 0; i < Arity-1; i++) {
				if (i == Keys[0]) output += "*";
				else if (Keys.Contains(i)) output += "&";
				output += $"{names[i]}<{types[i].Name}>-";
			}
			if (Arity - 1 == Keys[0]) output += "*";
			else if (Keys.Contains(Arity - 1)) output += "&";
			output += $"{names[Arity - 1]}<{types[Arity - 1].Name}>-";
			output += names[Arity - 1];
			return output;
		}

		public void PrintRelation(int width) {
			string final = "";
			foreach (string name in names) {
				string output = name;
				if ((width-output.Length) % 2 == 0) {
					for (int i = 0; i < (width-name.Length)/2; i++) {
						output = " " + output + " ";
					}
				} else {
					for (int i = 0; i < (width-name.Length-1)/2; i++) {
						output = " " + output + " ";
					}

					output += " ";
				}

				final += output;
			}

			
			Console.WriteLine(final);
			foreach (Type type in Types) {
				Console.Write(Table.centerPad("<" + type.Name + ">", width));
			}

			Console.WriteLine();
		}

		public object Clone() {
			(string, Type)[] output = new (string, Type)[Arity];
			for (int i = 0; i < Arity; i++) {
				string keyInfo = IsKey(names[i]) == 1 ? "*" : IsKey(names[i]) == 2 ? "&" : "";
				output[i] = (keyInfo + names[i], types[i]);
			}
			
			return new Relation(output);
		}

		/// <summary>
		/// Appends <para>s</para> to each name
		/// </summary>
		/// <param name="s">The string to append</param>
		/// <returns>the clone with modified names</returns>
		public Relation Clone(string s) {
			(string, Type)[] output = new (string, Type)[Arity];
			for (int i = 0; i < Arity; i++) {
				string keyInfo = IsKey(names[i]) == 1 ? "*" : IsKey(names[i]) == 2 ? "&" : "";
				output[i] = (keyInfo + s + "." + names[i], types[i]);
			}
			
			return new Relation(output);
		}
	}
}