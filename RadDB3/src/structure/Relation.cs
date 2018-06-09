using System;
using RadDB3.structure.Types;

namespace RadDB3.structure {
	public class Relation {
		private Type[] types;
		private Type[] subTypes;
		private string[] names;
		private int[] keys; // TODO: Implement properly

		public int Arity => names.Length;

		/*
		public Relation(params NameTypePair[] pairs) {
			int index = 0;
			types = new Type[pairs.Length];
			names = new string[pairs.Length];
			keys = new [] {0};
			foreach (NameTypePair nameTypePair in pairs) {
				types[index] = nameTypePair.Type;
				names[index] = nameTypePair.Name;
				index++;
			}
		}
		*/

		public Relation(params (string, Type)[] pairs) {
			int index = 0;
			types = new Type[pairs.Length];
			subTypes = new Type[pairs.Length];
			names = new string[pairs.Length];
			keys = new [] {0};
			foreach ((string, Type) valueTuple in pairs) {
				names[index] = valueTuple.Item1;
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
		}

		public Type[] Types => types;

		public Type[] SubTypes => subTypes;

		public string[] Names => names;

		public override string ToString() {
			string output = "";
			for (int i = 0; i < Arity-1; i++) {
				output += names[i] + "-";
			}

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