using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;

namespace RadDB3.structure {
	public class Table : IEnumerable<RADTuple> {
		private const int DEFUALT_SIZE = 1;
		private const int DEFUALT_COLUMN_WIDTH = 15;

		private const bool MAX_DEBUG = false;
		
		private readonly string name;
		private readonly Relation relation;
		private LinkedList<RADTuple>[] tuples;

		public SecondaryIndexing SecondaryIndexing { private set; get; }

		public bool SecondaryIndexingExists => SecondaryIndexing?.Exists ?? false;

		/// <summary>
		/// Number of lists
		/// </summary>
		public int Size => tuples.Length;
		public string Name => name;
		public Relation Relation => relation;
		public int Count {
			get {
				int total = 0;

				foreach (LinkedList<RADTuple> linkedList in tuples) {
					total += linkedList.Count;
				}

				return total;
			}
		}

		public RADTuple[] All {
			get {
				List<RADTuple> output = new List<RADTuple>();
				foreach (LinkedList<RADTuple> linkedList in tuples) {
					foreach (RADTuple radTuple in linkedList) {
						output.Add(radTuple);
					}
				}

				return output.ToArray();
			}
		}

		public LinkedList<RADTuple>[] AllLists => tuples;

		public Table(Relation r, bool createSecondary = true, int size = DEFUALT_SIZE) :this(r.ToString(), r, createSecondary, size) { }
		
		public Table(string name, Relation r, bool createSecondary = true, int size = DEFUALT_SIZE) {
			if (r == null) return;
			this.name = name;
			relation = r;
			tuples = new LinkedList<RADTuple>[size];
			for (int i = 0; i < tuples.Length; i++) {
				tuples[i] = new LinkedList<RADTuple>();
			}
			if(createSecondary) SecondaryIndexing = new SecondaryIndexing(this);
		}

		public Table(RADTuple[] tuples) : this(tuples[0]?.relation, false, tuples.Length) {
			foreach (RADTuple radTuple in tuples) {
				Add(radTuple);
			}
		}

		public RADTuple this[int w, int y] => tuples[w].ElementAt(y);
		public LinkedList<RADTuple> this[int w] => tuples[w];
		
	

		/// <summary>
		/// Finds based on keys alone
		/// </summary>
		/// <param name="elements">Keys to check</param>
		public RADTuple this[params Element[] elements] {
			get {
				if (elements.Length != relation.Keys.Length && elements.Length != 1) return null;

				int hash = Math.Abs(elements[0].GetHashCode() % Size);
				foreach (RADTuple radTuple in tuples[hash]) {
					bool found = true;
					if (elements.Length > 1) {
						for (int i = 0; i < relation.Keys.Length; i++) {
							Element radItem = radTuple[relation.Keys[i]];
							if (radItem != elements[i]) found = false;
						}
					} else {
						Element radItem = radTuple[relation.Keys[0]];
						if (radItem != elements[0]) found = false;
					}

					if (found) return radTuple;
				}

				return null;
			}
		}
		
		public override int GetHashCode() {
			return name.GetHashCode();
		}

		private bool HashAlreadyExists(LinkedList<RADTuple> list, RADTuple check) {
			foreach (RADTuple radTuple in list) {
				if (radTuple[check.relation.Keys[0]] == check[check.relation.Keys[0]]) return true;
			}

			return false;
		}
		
		/// <summary>
		/// Add a tuple to the table. Fails if the relation of the tuple
		/// and the table do not match
		/// </summary>
		/// <param name="tuple">The tuple to be added</param>
		/// <returns>If it was successfully added</returns>
		public bool Add(RADTuple tuple) {
			if (tuple.relation != relation) return false;

			int hash = Math.Abs(tuple.GetHashCode() % Size);
			if (HashAlreadyExists(tuples[hash], tuple)) return false;
			tuples[hash].AddLast(tuple);
			if(MAX_DEBUG) DumpData();
			if (Count == Size) {
				Expand();
				if(MAX_DEBUG) DumpData();
			}
			if(SecondaryIndexingExists) SecondaryIndexing.Add(tuple);
			return true;
		}

		public bool Add(params object[] objects) {
			if (objects.Length != relation.Arity) return false;
			
			RADTuple t = RADTuple.CreateFromObjects(relation, objects);

			return Add(t);
		}

		public bool Add(params Element[] elements) {
			if (elements.Length != relation.Arity) return false;
			
			RADTuple t = new RADTuple(relation, elements);
			return Add(t);
		}

		/// <summary>
		/// Attempts to Remove a tuple if its the correct relation
		/// </summary>
		/// <param name="tuple">The tuple to remove</param>
		/// <returns>If the operation was sucessful</returns>
		public bool Remove(RADTuple tuple) {
			if (tuple.relation != relation) return false;
			
			int hash = Math.Abs(tuple.GetHashCode() % Size);
			var list = tuples[hash];
			SecondaryIndexing.Remove(tuple);
			return list.Remove(tuple);
		}

		/// <summary>
		/// If all the elements match
		/// </summary>
		/// <param name="elements">(string, Element) tuple representing column name and data</param>
		/// <returns></returns>
		public RADTuple Find(params (string, Element)[] elements) {
			Element[] search = new Element[relation.Arity];
			
			foreach ((string, Element) valueTuple in elements) {
				(string columnName, Element data) = valueTuple;
				int key = relation.Names.ToList().IndexOf(columnName);
				search[key] = data;
			}

			int keyIndex = relation.Keys[0];
			int hash = Math.Abs(search[keyIndex].GetHashCode() % Size);
			foreach (RADTuple radTuple in tuples[hash]) {
				bool found = true;
				int index = 0;
				foreach (Element radTupleElement in radTuple.elements) {
					if (radTupleElement != search[index++]) {
						found = false;
						break;
					}
				}

				if (found) return radTuple;
			}

			return null;
		}

		/// <summary>
		/// Same as Find, but returns true or false if RADTuple was found
		/// </summary>
		/// <param name="output">RADTuple reference to be set</param>
		/// <param name="elements">(string, Element) tuple representing column name and data</param>
		/// <returns></returns>
		public bool Find(out RADTuple output, params (string, Element)[] elements) {
			output = Find(elements);
			return output != null;
		}

		
		public bool Find(out RADTuple output, params (string, object)[] elements) {
			output = Find(elements);
			return output != null;
		}

		public RADTuple Find(params (string, object)[] elements) {
			if (elements.Length != relation.Arity) return null;
			(string, Element)[] fixedTuples = new (string, Element)[elements.Length];
			int index = 0;
			foreach ((string, object) valueTuple in elements) {
				(string columnName, object data) = valueTuple;
				int key = relation.Names.ToList().IndexOf(columnName);
				Element e;
				if(data is string) e =  Element.ConvertToElement(relation.Types[key], (string) data);
				else e = Element.ConvertToElement(relation.Types[key], data);
				fixedTuples[index++] = (columnName, e);
			}

			return Find(fixedTuples);
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

		public void CreateSecondaryIndexing() {
			if (SecondaryIndexingExists) SecondaryIndexing.ReCreateSecondaryIndex();
			else SecondaryIndexing = new SecondaryIndexing(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IEnumerator<RADTuple> GetEnumerator() {
			return All.ToList().GetEnumerator();
		}

		public double GetDistributionRatio() {
			double numOfListsWithData = 0;
			foreach (LinkedList<RADTuple> linkedList in tuples) {
				if (linkedList.Count > 0) ++numOfListsWithData;
			}

			return 1 - (Count-numOfListsWithData) / Size;
		}

		public int GetNumOfLinesWithData() {
			int output = 0;
			foreach (LinkedList<RADTuple> linkedList in tuples) {

				if (linkedList.Count > 0) {
					++output;
				}
			}

			return output;
		}

		/// <summary>
		/// Dumps relavent data and a graph representing the distribution of data in the table
		/// </summary>
		public void DumpData(int bars = 10, int length = 15) {
			Console.WriteLine($"Name: {name}");
			Console.WriteLine($"Relation: {relation.Dump()}");
			Console.WriteLine("Distribution Ratio: {0:P} ({1})", GetDistributionRatio(), Misc.percentToLetterGrade(GetDistributionRatio()));
			Console.WriteLine("Size: {0}", Size);
			Console.WriteLine("Count: {0}", Count);
			Console.WriteLine("Lines with data: {0}", GetNumOfLinesWithData());
			
			if (Count == 0) return;
			int[] pieces = new int[bars];
			int counted = 0;
			int piecesIndex = 0;
			foreach (LinkedList<RADTuple> linkedList in tuples) {
				counted++;
				pieces[piecesIndex] += linkedList.Count;

				if (counted >= Size / bars &&
					piecesIndex < bars - 1) {
					piecesIndex++;
					counted = 0;
				}
			}

			int max = pieces.Max();
			foreach (int piece in pieces) {
				string singleLine = "|";
				for (int i = 0; i < (decimal) piece/max * length; i++) {
					singleLine += "#";
				}
				
				Console.WriteLine(singleLine);
			}
		}

		public void PrintTableNoPadding() {
			Console.WriteLine(name);
			relation.PrintRelation(1);
			foreach (var linkedList in tuples) {
				foreach (var radTuple in linkedList) {
					foreach (var element in radTuple.elements) {
						Console.Write(element + "|");
					}
					
					Console.WriteLine();
					
				}
			}
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