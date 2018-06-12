using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.structure;

namespace RadDB3.interaction {
	
	/**
	 * Base Files: <database_name>.rd3
	 * LAYOUT:
	 * 		NAME:<sentence>;
	 * 		TABLES:<name,URI>,<name,URI>,...;
	 * 
	 * Table Files: <table_name>.rdt
	 * LAYOUT:
	 * 		NAME:<sentence>;
	 * 		RELATION{
	 *			[<key_info>]<type> <name> (<constraints>);
	 *			.
	 * 			.
	 * 			.
	 * 		}
	 * 		TUPLES{
	 *			<tupleString>
	 * 			.
	 * 			.
	 * 			.
	 * 		}
	 */
	public static class FileInteraction {

		public static string FileToString(string filePath) {
			return File.ReadAllText(filePath);
		}

		public static Table ConvertStringToTable(string str) {
			Parser p = new Parser(str, Parser.ReadOptions.STRING, Parser.ParseOptions.REMOVE_ONLY_TABS);
			ParseTree tree = new ParseTree(p.ParseTable);
			tree.PrintTree();

			int size = int.Parse(tree["<int>"][0].Data);

			ParseNode relationPtr = tree["<relation_list>"];
			List<NameTypePair> ntpList = new List<NameTypePair>();
			do {
				if (relationPtr.Data == "<relation_list_tail>") relationPtr = relationPtr[0];
				NameTypePair ntp = new NameTypePair(relationPtr[0]);
				ntpList.Add(ntp);
				ntpList.Add(ntp);
								
				relationPtr = relationPtr[1];

			} while (relationPtr != null);
			Relation generatedRelation = new Relation(ntpList.ToArray());
			Table output = new Table(generatedRelation, size);

			
			// Creating the list of tuples
			LinkedList<RADTuple>[] tuples = new LinkedList<RADTuple>[size];
			for (int i = 0; i < size; i++) {
				tuples[i] = new LinkedList<RADTuple>();
			}
			
			
			var tupleListPtr = tree["<tuple_list"];
			
			while (tupleListPtr != null) {
				if (tupleListPtr.Data == "<tuple_list_tail>") tupleListPtr = tupleListPtr[0];
				int index = int.Parse(tupleListPtr[0].Data);

				LinkedList<RADTuple> newList = tuples[index];
				newList.AddFirst(new RADTuple(generatedRelation)); // TODO: Tuple ParseNode to tuple Constructor
				var tuplePtr = tupleListPtr["<tuple_more>"];
				while (tuplePtr != null) {
					ParseNode tuple = tuplePtr["<tuple>"];
					// TODO: Tuple ParseNode to tuple Constructor
					// TODO: Add tuple to {newList}
					tuplePtr = tuplePtr["<tuple_more>"];
				}
				
					
				tupleListPtr = tupleListPtr["<tuple_list_tail>"];
			}
			

			return null;
		}

		public static Table ConvertFileToTable(string filePath) {
			return ConvertStringToTable(FileToString(Environment.CurrentDirectory + @"\" + filePath));
		}

		public static void ConvertTableToFile(Table t) {
			ConvertTableToFile("", t);
		}

		public static void ConvertTableToFile(string filePath, Table t) {
			StreamWriter writer = new StreamWriter(Environment.CurrentDirectory + filePath + @"\" + t.Name + ".rdt");
			
			writer.Write("NAME:{0};\n", Parser.StringToSentence(t.Name));
			writer.Write("SIZE:{0};\n", t.Size);
			writer.Write("RELATION{\n");
			for (int i = 0; i < t.Relation.Arity; i++) {
				char keyInfo = t.Relation.Keys.ElementAt(0) == i ? '*' :
					t.Relation.Keys.Contains(i) ? '&' : '-';
				writer.Write("\t[{0}]{1} {2} ({3});\n",keyInfo,t.Relation.Types[i],Parser.StringToSentence(t.Relation.Names[i]),"");
			}
			writer.Write("}\nTUPLES{\n");
			int index = 0;
			foreach (LinkedList<RADTuple> linkedList in t.AllLists) {
				if (linkedList.Count > 0) {
					writer.Write("\t[{0}]",index);
					for (int i = 0; i < linkedList.Count-1; i++) {
						writer.Write(linkedList.ElementAt(i) + ",");
					}
					writer.Write(linkedList.ElementAt(linkedList.Count-1) + "\n");
				}
				index++;
			}
			writer.Write("}");
			
			writer.Flush();
			writer.Close();
		}
	}
}
