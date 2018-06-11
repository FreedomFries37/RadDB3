using System;
using System.IO;
using System.Linq;
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
	public class FileInteraction {

		public static string FileToString(string filePath) {
			return File.ReadAllText(filePath);
		}

		public static Table ConvertStringToTable(string str) {
			Parser p = new Parser(str, Parser.ReadOptions.STRING, Parser.ParseOptions.REMOVE_ONLY_TABS);
			ParseTree tree = new ParseTree(p.ParseTable);

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
			
			writer.Write("NAME:{0};\n",t.Name);
			writer.Write("RELATION{\n");
			for (int i = 0; i < t.Relation.Arity; i++) {
				char keyInfo = t.Relation.Keys.ElementAt(0) == i ? '*' :
					t.Relation.Keys.Contains(i) ? '&' : '-';
				writer.Write("\t[{0}]{1} {2} ({3});\n",keyInfo,t.Relation.Types[i].Name,t.Relation.Names[i],"");
			}
			writer.Write("}\nTUPLES{\n");
			foreach (RADTuple radTuple in t.All) {
				writer.Write("\t{0}\n",radTuple);
			}
			writer.Write("}");
			
			writer.Flush();
			writer.Close();
		}
	}
}
