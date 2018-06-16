using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using RadDB3.scripting.parsers;
using RadDB3.structure;

namespace RadDB3.scripting.RelationalAlgebra {
	public delegate RADTuple[] RelationalAlgebraFunction(string[] options, params AlgebraNode[] nodes);
	public static class RelationalAlgebraModule {

		public static bool UsingRegex = false;
		public static int TuplesCreated { private set; get; }

		public static void ResetTuplesCreated() => TuplesCreated = 0;

		static RelationalAlgebraModule() {
			TuplesCreated = 0;
		}
		
		public static RADTuple[] Reflect(string[] options, params AlgebraNode[] nodes) {
			if (options.Length == 1 &&
				options[0] == "pure") {
				return nodes[0].BaseTable.All;
			}
			Relation generatedRelation = nodes[0].BaseTable.Relation.Clone(nodes[0].BaseTable.Name);
			RADTuple[] output = nodes[0].BaseTable.All;
			foreach (RADTuple radTuple in output) {
				if (!radTuple.AttemptSwitchRelation(generatedRelation)) return null;
			}

			TuplesCreated += output.Length;
			return output;
		}

		//Node count should be 1
		//Options in style <string>|<sentence>=<string>|<sentence>
		public static RADTuple[] Selection(string[] options, params AlgebraNode[] nodes) {
			Table choice = nodes[0].TableApply();
			if (options.Length == 0) return choice.All;
			
			(string name, string data)[] columnDataTuples = new (string, string)[options.Length];
			for (int i = 0; i < options.Length; i++) {
			
				Regex sentenceRegex = new Regex("\".*\"");
				string left = options[i].Split("=")[0];
				string right =options[i].Split("=")[1];
				Parser p = new Parser(left, Parser.ReadOptions.STRING);
				ParseTree parseTree;
				if (sentenceRegex.IsMatch(left)) {
					parseTree = new ParseTree(p.ParseSentence, false);
					left = Parser.ConvertSentence(parseTree.Head);
				}

				
				
				p = new Parser(right, Parser.ReadOptions.STRING);
				if (sentenceRegex.IsMatch(right)) {
					parseTree = new ParseTree(p.ParseSentence, false);
					right = Parser.ConvertSentence(parseTree.Head);
				}

				columnDataTuples[i] = (left, right);
			}


			List<RADTuple> output = new List<RADTuple>();
			bool first = true;
			for (int i = 0; i < columnDataTuples.Length; i++) {
				List<RADTuple> next = new List<RADTuple>();
				
				var columData = columnDataTuples[i];
				if (!UsingRegex) columData.data = columData.data.Replace("*", ".*");
				if(choice.Relation.IsKey(columData.name) == -1) continue;
				
				Element eData = columData.data.Contains("*") ? null : Element.ConvertToElement(choice.Relation.Types[choice.Relation[columData.name]], columData.data);
				
				if (columData.name == choice.Relation.PrimaryKeyName && eData != null) {
					next.Add(choice[eData]);
				} else {
					if (choice.Relation.IsKey(columData.name) == 2 && choice.SecondaryIndexingExists) {
						Element[] primary = UsingRegex ? choice.SecondaryIndexing.GetRegex((columData.name, columData.data)) : choice.SecondaryIndexing.Get((columData.name, columData.data));
						foreach (Element element in primary) {
							next.Add(choice[element]);
						}
					} else {
						foreach (RADTuple radTuple in choice.All) {
							if (eData != null && radTuple[columData.name] == eData) {
								next.Add(radTuple);
							} else {
								
								Regex regex = new Regex(columData.data);
								Match m = regex.Match(radTuple[columData.name].ToString());
								if (m.Success &&
									m.Value.Length == radTuple[columData.name].ToString().Length) {
									next.Add(radTuple);
								}
							}
						}
					}
				}

				TuplesCreated += next.Count;
				output = first ? next : output.Intersect(next).ToList();
				first = false;
			}
			
			
			
			return output.ToArray(); 
		}

		// Node count should be one
		// Options in style <sentence>|<string>
		public static RADTuple[] Projection(string[] options, params AlgebraNode[] nodes) {
			Table choice = nodes[0].TableApply();
			Regex sentenceRegex = new Regex("\".*\"");
			for (int i = 0; i < options.Length; i++) {
				if (sentenceRegex.IsMatch(options[i])) {
					options[i] = options[i].Substring(0, options[i].Length - 1).Substring(1);
				}
			}
			
			Relation generatedRelation = new Relation(choice.Relation, options[0], options.Skip(1).ToArray());
			
			List<RADTuple> output = new List<RADTuple>();
			foreach (RADTuple radTuple in choice) {
				output.Add(new RADTuple(generatedRelation, radTuple));
			}

			TuplesCreated += output.Count;
			return output.ToArray();
		}

		// Node count should be two
		// only one option
		// Options in form <table_name>(<column_name>,...)=<table_name>(<column_name>,...)
		// 		or (<table>)=<table_name>(<column_name>,...)
		//		or <table_name>(<column_name>,...)=(<table>)
		public static RADTuple[] InnerJoin(string[] options, params AlgebraNode[] nodes) {
			Table table1, table2;
			(string table1ColumnName, string table2ColumnName)[] args;
			table1 = nodes[0].TableApply();
			table2 = nodes[1].TableApply();

			string option = options[0];
			Parser parser = new Parser(option, Parser.ReadOptions.STRING);
			ParseTree tree = new ParseTree(parser.ParseJoinInfo);

			List<string> leftColumns, rightColumns;
			leftColumns = Parser.ConvertColumns(tree[1]).ToList();
			string leftName = tree[0].Data != "TABLE DATA" ? tree[0][0][0].Data + "." : "";
			

			rightColumns = Parser.ConvertColumns(tree[3]).ToList();
			string rightName = tree[2].Data != "TABLE DATA" ? tree[2][0][0].Data + "." : "";

			if (leftColumns.Count != rightColumns.Count) return null;
			for (int i = 0; i < leftColumns.Count; i++) {
				leftColumns[i] = leftName + leftColumns[i];
				rightColumns[i] = rightName + rightColumns[i];
			}



			List<NameTypePair> pairs = new List<NameTypePair>();
			int maxSize = table1.Relation.Arity + table2.Relation.Arity - leftColumns.Capacity;

			int index = 0;
			bool firstSwitch = true;
			while (pairs.Count < maxSize) {
				NameTypePair next;


				if (pairs.Count < table1.Relation.Arity) { // add from left
					
					string name = table1.Relation.Names[index];
					string keyInfo = table1.Relation.Keys.ElementAt(0) == index ? "*" :
						table1.Relation.Keys.Contains(index) ? "&" : "";
					Type left = table1.Relation.Types[table1.Relation[name]];
					next = new NameTypePair(keyInfo + name, left);
				} else { //add from righht
					if (firstSwitch) {
						firstSwitch = false;
						index = 0;
					}

					string name = table2.Relation.Names[index];
					Type right = table2.Relation.Types[table2.Relation[name]];
					if (rightColumns.Contains(name)) {
						Type left = table1.Relation.Types[table1.Relation[leftColumns[rightColumns.IndexOf(name)]]];

						if (left != right) {
							next = null;
							index++;
						} else {
							index++;
							continue;
						}

					} else next = new NameTypePair(name, right);
				}

				pairs.Add(next);
				index++;
			}

			Relation generatedRelation = new Relation(pairs.ToArray());
			List<RADTuple> allGenerated = new List<RADTuple>();
			foreach (RADTuple radTuple in table1.All) {
				List<Element> importantElements = new List<Element>();
				foreach (string leftColumnName in leftColumns) {
					importantElements.Add(radTuple[leftColumnName]);
				}

				List<string> searchQueries = new List<string>();
				for (int i = 0; i < importantElements.Count; i++) {
					searchQueries.Add(
						$"\"{rightColumns[i]}\"=\"{importantElements[i]}\""
					);
				}

				AlgebraNode node0 = new AlgebraNode(table2, "pure");
				AlgebraNode node1 = new AlgebraNode(Selection, searchQueries.ToArray(), node0);
				List<RADTuple> foundRightTable = node1.Apply().ToList();

				foreach (RADTuple tuple in foundRightTable) {
					if (tuple != null) {
						List<Element> finaList = radTuple.ToListOfElements();
						foreach (string columnName in tuple.relation.Names) {
							if (!rightColumns.Contains(columnName)) {
								finaList.Add(tuple[columnName]);
							}
						}

						allGenerated.Add(new RADTuple(generatedRelation, finaList.ToArray()));
					}
				}
			}

			TuplesCreated += allGenerated.Count;
			return allGenerated.ToArray();
		}

	}
}