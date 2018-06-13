using System;
using System.Collections.Generic;
using System.Linq;
using RadDB3.structure.Types;

namespace RadDB3.structure {
	public class Relation {
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
	}
}