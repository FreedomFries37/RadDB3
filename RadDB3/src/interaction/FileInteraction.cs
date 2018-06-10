using System;
using System.IO;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.structure;

namespace RadDB3.interaction {
	
	/**
	 * Base Files: <database_name>.rd3
	 * LAYOUT:
	 * 		NAME: <string>;
	 * 		TABLES:<name,URI>,<name,URI>,...;
	 * 
	 * Table Files: <table_name>.rdt
	 * LAYOUT:
	 * 		NAME: <string>;
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
			Parser p = new Parser(str, Parser.ReadOptions.STRING);
			ParseTree tree = new ParseTree(p.ParseTable);

			return null;
		}

		public static Table ConvertFileToTable(string filePath) {
			return ConvertStringToTable(FileToString(filePath));
		}
	}
}
