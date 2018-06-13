using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace RadDB3.structure {
	public class SecondaryIndexing {


		private class SecIndDict {
	
			private readonly Table table;
			private Dictionary<Element, LinkedList<Element>> _dictionary; // first is secondary key, second is list of primary keys
			private readonly int primaryIndex, secondaryIndex;
			public bool Exists { private set; get; }
			
			public SecIndDict(Table s, int primaryIndex, int secondaryIndex) {
				_dictionary = new Dictionary<Element, LinkedList<Element>>();
				this.primaryIndex = primaryIndex;
				this.secondaryIndex = secondaryIndex;
				table = s;
				Exists = CreateSecondaryIndexTree();
			}
			
			private bool CreateSecondaryIndexTree() {
				if (table.Relation.Keys.Length == 1) return false;

				foreach (var tuple in table.All) {
					Element primary = tuple[primaryIndex];
					Element secondary = tuple[secondaryIndex];

					if (_dictionary.ContainsKey(secondary)) {
						_dictionary[secondary].AddLast(primary);
					} else {
						LinkedList<Element> newList= new LinkedList<Element>();
						newList.AddFirst(primary);
						_dictionary.Add(secondary, newList);
					}
				}

				return true;
			}

			public void Add(RADTuple tuple) {
				Element primary = tuple[primaryIndex];
				Element secondary = tuple[secondaryIndex];

				if (_dictionary.ContainsKey(secondary)) {
					_dictionary[secondary].AddLast(primary);
				} else {
					LinkedList<Element> newList= new LinkedList<Element>();
					newList.AddFirst(primary);
					_dictionary.Add(secondary, newList);
				}
			}

			public bool Remove(RADTuple tuple) {
				Element primary = tuple[primaryIndex];
				Element secondary = tuple[secondaryIndex];

				if (_dictionary.ContainsKey(secondary)) {
					LinkedList<Element> list = _dictionary[secondary];
					if (list.Count == 1) {
						return _dictionary.Remove(secondary);
					}
					return list.Remove(primary);
				}
				
				return false;
			}

			public LinkedList<Element> Get(Element s) {
				if (_dictionary.ContainsKey(s)) return _dictionary?[s];
				return null;
			}

			public LinkedList<Element>[] Get(Regex regex) {
				LinkedList<LinkedList<Element>> output = new LinkedList<LinkedList<Element>>();
				foreach (KeyValuePair<Element,LinkedList<Element>> keyValuePair in _dictionary) {
					Element secondary = keyValuePair.Key;
					LinkedList<Element> list = keyValuePair.Value;
					Match m = regex.Match(secondary.ToString());
					if (m.Success &&
						m.Value == secondary.ToString()) {
						output.AddLast(list);
					}
				}

				return output.ToArray();
			}
		}

		private Table table;
		private Dictionary<string, SecIndDict> treeDict; //column name
		public bool Exists => treeDict.Count > 0;
		private bool operatoring;

		public bool Operatoring => operatoring;

		public SecondaryIndexing(Table t) {
			operatoring = false;
			table = t;
			treeDict = new Dictionary<string, SecIndDict>();
			for (int i = 1; i < t.Relation.Keys.Length; i++) {
				var tree = new SecIndDict(t, t.Relation.Keys[0], t.Relation.Keys[i]);
				treeDict.Add(t.Relation.Names[table.Relation.Keys[i]], tree);
			}
		}

		public void Add(RADTuple tuple) {
			if (tuple.relation != table.Relation) return;
	
			for (int i = 1; i < table.Relation.Keys.Length; i++) {
				string secondaryName = table.Relation.Names[table.Relation.Keys[i]];
				treeDict[secondaryName].Add(tuple);
			}
		}

		public bool Remove(RADTuple tuple) {
			if (tuple.relation != table.Relation) return false;
			
			for (int i = 1; i < table.Relation.Keys.Length; i++) {
				string secondaryName = table.Relation.Names[table.Relation.Keys[i]];
				bool outcome = treeDict[secondaryName].Remove(tuple);
				if (!outcome) return false;
			}

			return true;
		}

		/// <summary>
		/// Returns possible primary indexes
		/// </summary>
		/// <param name="valueTuples"></param>
		/// <returns></returns>
		public Element[] Get(params (string name, Element value)[] valueTuples) {
			(string, string)[] passdown = new (string, string)[valueTuples.Length];

			int index = 0;
			foreach ((string, Element) valueTuple in valueTuples) {
				string name = valueTuple.Item1;
				string element = valueTuple.Item2.ToString();

				passdown[index] = (name, element);
				index++;
			}

			return Get(passdown);
		}
		
		/// <summary>
		/// Returns possible primary indexes
		/// </summary>
		/// <param name="valueTuples"></param>
		/// <returns></returns>
		public Element[] Get(params (string name, string value)[] valueTuples) {
			
			(string, string)[] passdown = new (string, string)[valueTuples.Length];

			int index = 0;
			foreach ((string, string ) valueTuple in valueTuples) {
				string name = valueTuple.Item1;
				string element = valueTuple.Item2.Replace("*", ".*");

				passdown[index] = (name, element);
				index++;
			}

			return GetRegex(passdown);
		}

		/// <summary>
		/// Returns possible primary indexes
		/// </summary>
		/// <param name="valueTuples"></param>
		/// <returns></returns>
		public Element[] GetRegex(params (string name, string value)[] valueTuples) {
			LinkedList<Element>[] lists = new LinkedList<Element>[valueTuples.Length];

			for (int i = 0; i < valueTuples.Length; i++) {
				(string name, string regExpression) = valueTuples[i];
				if(!table.Relation.Names.ToList().Contains(name)) continue;
			
				Regex regex = new Regex(regExpression);
				Type type = table.Relation.Types[table.Relation.Names.ToList().IndexOf(name)];
				if (regExpression.Contains("*")) {
					var foundLists = treeDict[name].Get(regex);
					var combinedList = new LinkedList<Element>();
					foreach (LinkedList<Element> linkedList in foundLists) {
						foreach (Element element in linkedList) {
							combinedList.AddLast(element);
						}
					}

					lists[i] = combinedList;
				} else {
					lists[i] = treeDict[name].Get(Element.ConvertToElement(type, regExpression));
				}
			}
			
			LinkedList<Element> output = new LinkedList<Element>();
			foreach (LinkedList<Element> linkedList in lists) {
				foreach (Element element in linkedList) {
					if (!output.Contains(element)) output.AddLast(element);
				}
			}

			return output.ToArray();
		}

		public Task ReCreateSecondaryIndex() {
			operatoring = false;
			treeDict = new Dictionary<string, SecIndDict>();
			for (int i = 1; i < table.Relation.Keys.Length; i++) {
				var tree = new SecIndDict(table, table.Relation.Keys[0], table.Relation.Keys[i]);
				treeDict.Add(table.Relation.Names[table.Relation.Keys[i]], tree);
			}
			//Console.WriteLine($"Finished creating secondary index system for {table.Name}");
			operatoring = true;
			return Task.CompletedTask;
		}
	}
}