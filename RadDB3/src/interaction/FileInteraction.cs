using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.structure;

namespace RadDB3.interaction {
	
	/**
	 * Base Files: <database_name>.rd3
	 * LAYOUT:
	 * 		NAME:<sentence>;
	 * 		TABLES:<name,URI>\n<name,URI>n,;
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

		private static string FileToString(string filePath) {
			return File.ReadAllText(filePath);
		}

		private static Table ConvertStringToTable(string str) {
			str = str.Replace("\r", "");
			Parser p = new Parser(str, Parser.ReadOptions.STRING, Parser.ParseOptions.REMOVE_ONLY_TABS);
			ParseTree tree = new ParseTree(p.ParseTable);
			if (!tree.successfulParse) return null;

			int size = int.Parse(tree["<int>"][0].Data);
			string name = tree["<sentence>"][0].Data;
			ParseNode relationPtr = tree["<relation_list>"];
			List<NameTypePair> ntpList = new List<NameTypePair>();
			do {
				if (relationPtr.Data == "<relation_list_tail>") relationPtr = relationPtr[0];
				NameTypePair ntp = new NameTypePair(relationPtr[0]);
				ntpList.Add(ntp);
								
				relationPtr = relationPtr[1];

			} while (relationPtr != null);
			Relation generatedRelation = new Relation(ntpList.ToArray());
			Table output = new Table(name, generatedRelation, true, size);

			
			
			var tupleListPtr = tree["<tuple_list>"];
			
			while (tupleListPtr != null) {
				if (tupleListPtr.Data == "<tuple_list_tail>") tupleListPtr = tupleListPtr[0];
				int index = int.Parse(tupleListPtr[0][0].Data);

				output.AllLists[index].AddFirst(RADTuple.CreateFromParseNode(generatedRelation, tupleListPtr["<tuple>"])); // TODO: Tuple ParseNode to tuple Constructor
				var tuplePtr = tupleListPtr["<tuple_more>"];
				while (tuplePtr != null) {
					ParseNode tupleNode = tuplePtr["<tuple>"];
					RADTuple tuple = RADTuple.CreateFromParseNode(generatedRelation, tupleNode);
					output.AllLists[index].AddLast(tuple);
					tuplePtr = tuplePtr["<tuple_more>"];
				}
				
					
				tupleListPtr = tupleListPtr["<tuple_list_tail>"];
			}

			output.SecondaryIndexing.ReCreateSecondaryIndex();
			//Console.WriteLine("Created Table From File");
			return output;
		}

		public static Table ConvertFileToTable(string filePath) {
			return ConvertStringToTable(FileToString(filePath));
		}

		public static FileInfo ConvertTableToFile(Table t) {
			return ConvertTableToFile("", t);
		}

		public static FileInfo ConvertTableToFile(string filePath, Table t) {
			StreamWriter writer = new StreamWriter(Environment.CurrentDirectory + @"\" + filePath + @"\" + t.Name + ".rdt");
			
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
						writer.Write(linkedList.ElementAt(i).Dump(DumpLevel.LOW) + ",");
					}
					writer.Write(linkedList.ElementAt(linkedList.Count-1).Dump(DumpLevel.LOW) + "\n");
				}
				index++;
			}
			writer.Write("}");
			
			writer.Flush();
			writer.Close();
			return new FileInfo(Environment.CurrentDirectory + @"\" + filePath + @"\" + t.Name + ".rdt");
		}

		public static void ConvertDatabaseToFile(Database db) => ConvertDatabaseToFile("", db);

		public static void ConvertDatabaseToFile(string filePath, Database db) {
			DirectoryInfo outsideDir = Directory.CreateDirectory(db.ToString());
			DirectoryInfo tableDir = Directory.CreateDirectory(outsideDir + "/tables");

			StreamWriter writer = new StreamWriter(outsideDir + @"\" + db + ".rd3");
			
			writer.Write($"NAME:{db}\n");
			writer.Write($"SIZE:{db.Size}\nTABLES:\n");
			
			foreach (Table table in db) {
				FileInfo f = ConvertTableToFile(outsideDir.Name + @"\" + tableDir.Name, table);
				writer.Write($"{f.Name}\n");
			}
			writer.Flush();
			writer.Close();
		}

		
		public static Database ConvertDirectoryToDatabase(string dirPath) {
			DirectoryInfo info = new DirectoryInfo(dirPath);
			foreach (var file in info.EnumerateFiles()) {
				if (file.Extension == ".rd3") {
					return ConvertFileToDatabase(file.FullName);
				}
			}

			return null;
		}
		
		// FOR .rd3 files
		public static Database ConvertFileToDatabase(string filepath) {
			return ConvertStringToDatabase(FileToString(filepath),  new FileInfo(filepath));
		}


		public static Database ConvertCurrentDirectoryToDatabase() {
			return ConvertDirectoryToDatabase(Environment.CurrentDirectory);
		}

		public static Database[] ConvertDirectoriesInCurrentDirectoryToDatabases() {
			DirectoryInfo directoryInfo = new DirectoryInfo(Environment.CurrentDirectory);
			List<Database> output = new List<Database>();
			foreach (DirectoryInfo enumerateDirectory in directoryInfo.EnumerateDirectories()) {
				Database next = ConvertDirectoryToDatabase(enumerateDirectory.FullName);
				if(next != null) output.Add(next);
			}

			return output.ToArray();
		}

		public static Database ConvertStringToDatabase(string str, FileInfo filepath) {
			DirectoryInfo tableDirectoryInfo =
				filepath.Directory?.GetDirectories().ToList().Find(dir => dir.Name == "tables");
			if (tableDirectoryInfo == null) {
				Console.WriteLine("MISSING TABLE FOLDER");
				return null;
			}


			str = str.Replace("\r", "");
			
			string[] strSplit = str.Split("\n");
			string name = strSplit[0].Remove(0, "NAME:".Length);
			int size = int.Parse(strSplit[1].Remove(0, "SIZE:".Length));
			
			Database output = new Database(name, size);
			for (int i = 3; i < strSplit.Length-1; i++) {
				output.addTable(ConvertFileToTable(tableDirectoryInfo.FullName + @"\" + strSplit[i]));
			}

			return output;
		}
		
		
	}
}
