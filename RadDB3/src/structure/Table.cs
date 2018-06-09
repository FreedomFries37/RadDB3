using System;
using System.Collections.Generic;
using System.Linq;

namespace RadDB3.structure {
	public class Table {
		private const int DEFUALT_SIZE = 1;
		private const int DEFUALT_COLUMN_WIDTH = 15;
		
		private readonly string name;
		private readonly Relation relation;
		private LinkedList<RADTuple>[] tuples;

		public int Size => tuples.Length;

		public int Count {
			get {
				int total = 0;

				foreach (LinkedList<RADTuple> linkedList in tuples) {
					total += linkedList.Count;
				}

				return total;
			}
		}

		public Table(Relation r, int size = DEFUALT_SIZE) :this(r.ToString(), r, size) { }
		
		public Table(string name, Relation r, int size = DEFUALT_SIZE) {
			this.name = name;
			relation = r;
			tuples = new LinkedList<RADTuple>[size];
			for (int i = 0; i < tuples.Length; i++) {
				tuples[i] = new LinkedList<RADTuple>();
			}
		}

		public RADTuple this[int w, int y] => tuples[w].ElementAt(y);
		public LinkedList<RADTuple> this[int w] => tuples[w];
		
		public override int GetHashCode() {
			return name.GetHashCode();
		}

		/// <summary>
		/// Add a tuple to the table. Fails if the relation of the tuple
		/// and the table do not match
		/// </summary>
		/// <param name="tuple">The tuple to be added</param>
		/// <returns>If it was successfully added</returns>
		public bool Add(RADTuple tuple) {
			if (tuple.relation != relation) return false;

			int hash = tuple.elements[0].GetHashCode() % Size;
			tuples[hash].AddLast(tuple);
			if(Count == Size) Expand();

			return true;
		}

		/// <summary>
		/// Attempts to Remove a tuple if its the correct relation
		/// </summary>
		/// <param name="tuple">The tuple to remove</param>
		/// <returns>If the operation was sucessful</returns>
		public bool Remove(RADTuple tuple) {
			if (tuple.relation != relation) return false;
			
			int hash = tuple.elements[0].GetHashCode() % Size;
			var list = tuples[hash];
			return list.Remove(tuple);
		}
		
		/// <summary>
		/// Doubles the size of the table and redistributes the data
		/// </summary>
		private void Expand() {
			LinkedList<RADTuple>[] newTuples = new LinkedList<RADTuple>[Size*2];
			for (int i = 0; i < newTuples.Length; i++) {
				newTuples[i] = new LinkedList<RADTuple>();
			}

			foreach (LinkedList<RADTuple> linkedList in tuples) {
				foreach (RADTuple radTuple in linkedList) {
					int hash = radTuple.GetHashCode() % newTuples.Length;
					newTuples[hash].AddLast(radTuple);
				}
			}

			tuples = newTuples;
		}

		public double GetDistributionRatio() {
			double numOfListsWithData = 0;
			foreach (LinkedList<RADTuple> linkedList in tuples) {
				if (linkedList.Count > 0) ++numOfListsWithData;
			}

			return numOfListsWithData / Count;
		}

		public void PrintTable(int width = DEFUALT_COLUMN_WIDTH) {
			
			Console.WriteLine(centerPad(name, width*relation.Arity));
			
			relation.PrintRelation(width);
			foreach (var linkedList in tuples) {
				foreach (var radTuple in linkedList) {
					string output = "";

					foreach (var element in radTuple.elements) {
						string elementString = element.ToString();
						if ((width-elementString.Length) % 2 == 0) {
							for (int i = 0; i < (width-element.ToString().Length)/2; i++) {
								elementString = " " + elementString + " ";
							}
						} else {
							for (int i = 0; i < (width-element.ToString().Length-1)/2; i++) {
								elementString = " " + elementString + " ";
							}

							elementString += " ";
						}

						output += elementString;
					}
					
					Console.WriteLine(output);
					
				}
			}

		}
		
		public static string centerPad(string str, int width) {
			if (str.Length > width) return str;
			string output = str;
			if ((width-str.Length) % 2 == 0) {
				for (int i = 0; i < (width-str.Length)/2; i++) {
					output = " " + output + " ";
				}
			} else {
				for (int i = 0; i < (width-str.ToString().Length-1)/2; i++) {
					output = " " + output + " ";
				}

				output += " ";
			}

			return output;
		}
	}
	
	
}