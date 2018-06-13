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
		
		public static RADTuple[] Reflect(string[] options, params AlgebraNode[] nodes) {
			return nodes[0].BaseTable.All;
		}

		//Node count should be 1
		//Options in style <string>|<sentence>=<string>|<sentence>
		public static RADTuple[] Selection(string[] options, params AlgebraNode[] nodes) {
			Table choice = nodes[0].TableApply();
			
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
								if (!UsingRegex) columData.data = columData.data.Replace("*", ".*");
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

			return output.ToArray();
		}
		
	}
}