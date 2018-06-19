using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RadDB3.interaction;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.scripting.RelationalAlgebra;
using RadDB3.structure;
using RadDB3.structure.Types;

namespace RadDB3 {

	
	
	static class Program {
		private static Database loadedDatabase;

		private static bool liveSession;

	

		static void Main(string[] args) => MainAsync(args);
		
		static async void MainAsync(string[] args) {
			
			if (args.Length == 0 || args[0] == "DEBUG") {
				
			} else {

				string path = "";
				bool useDirectory = false;
				
				for (int i = 0; i < args.Length; i++) {
					string arg = args[i];
					switch (arg) {
						case "-l":
							liveSession = true;
							break;
						case "--local": { // --local <int> or --local <string>
							Database[] databases = FileInteraction.ConvertDirectoriesInCurrentDirectoryToDatabases();
							if (int.TryParse(args[i+1], out int index)) {
								loadedDatabase = databases[index];
							} else {
								foreach (Database database in databases) {
									if (database.Name == args[i + 1]) {
										loadedDatabase = database;
									}
								}
							}
							loadedDatabase?.DumpDataBase();
						}
							break;
						case "-p":
						case "--path": {
							path = i + 1 < args.Length ? args[i + 1] : "";
						}
							break;
						case "-d": {
							useDirectory = true;
						}
							break;
					}
				}

				if (path != "") {
					loadedDatabase = useDirectory
						? FileInteraction.ConvertDirectoryToDatabase(path)
						: FileInteraction.ConvertFileToDatabase(path);
				}

				bool dontStop = true;
			
		
				if (liveSession && loadedDatabase != null) {
					while (loadedDatabase != null && dontStop) {
						Console.Write(">>  ");
						bool endCommand = false;

						bool afterCommandMode = false;
						string command = "";
						while (!endCommand) {
							var keyInfo = Console.ReadKey();
							Regex acceptableCharacters = new Regex("\\w|[*\\{\\[\\(\\}\\]\\)@_\"=><;$]");

							if (keyInfo.Key == ConsoleKey.Enter) {

								Console.WriteLine();

								if (afterCommandMode) {
									if (command.ToLower() == "exit;") {
										dontStop = false;
										break;
									}
									command = command.Substring(0, command.Length - 1);
									CommandInterpreter c = new CommandInterpreter(loadedDatabase, command);
									c.output.Dump();
									Console.Write(">> ");
								} else {
									Console.Write("  ");
								}
							}

							if (keyInfo.Key == ConsoleKey.Backspace) {
								if (command.Length > 0) {
									command = command.Substring(0, command.Length - 1);
									Console.Write(" ");
									Console.Write("\b");
								} else {
									Console.Write(" ");
								}
							} else if (keyInfo.KeyChar == ';') {
								command += ";";
								afterCommandMode = true;
								
							} else if (keyInfo.Key == ConsoleKey.Spacebar || 
										keyInfo.Key == ConsoleKey.Tab) {
								
								if (!afterCommandMode) {
									command += keyInfo.KeyChar;
								}
							} else if (acceptableCharacters.IsMatch("" + keyInfo.KeyChar)) {
								command += keyInfo.KeyChar;
								if (afterCommandMode) afterCommandMode = false;
							}
							else {
								Console.Write(" ");
								Console.Write("\b");
							}
						}

						if (dontStop) { }
					}
				} else if(loadedDatabase != null) {
					if (!args[args.Length - 1].Contains('-')) {
						var commandInterpreter = new CommandInterpreter(loadedDatabase, args[args.Length - 1]);
					}
				} else {
					Console.WriteLine("No Loaded Database");
				}
			}

			Table idntm = loadedDatabase?["IDNTM"];
			Table nameNicknameId = new Table("NN", new Relation(("*Name",typeof(RADString)), ("&Nickname",typeof(RADString)), ("&ID", typeof(RADInteger))));
			nameNicknameId.Add("FreedomFries", "Fries", 1);
			nameNicknameId.Add("Toaster443", "Toaster", 2);
			nameNicknameId.Add("arvindsindhwani", "Nilla", 3);
			Table idRole = new Table("RoleInfo", new Relation(("*ID",typeof(RADInteger)), ("Role",typeof(RADString))));
			idRole.Add(1, "Admin");
			idRole.Add(3, "Admin");
			idRole.Add(2, "Standard");

			loadedDatabase?.addTable(nameNicknameId);
			loadedDatabase?.addTable(idRole);

		
			
			if (idntm != null) {
				
				
				var c = new CommandInterpreter(loadedDatabase, "{IDNTM[Name=*]@Name} as NameTable");
				c = new CommandInterpreter(loadedDatabase, "NameTable");
				c = new CommandInterpreter(loadedDatabase, "{NameTable(Name)=NN(Name)[NN.Name=*]} as NameTable2}");
				c = new CommandInterpreter(loadedDatabase, 
					@"{IDNTM(Name)=(NN(ID)=RoleInfo(ID))(NN.Name)" +
					"[IDNTM.Time=*]" +
					"@IDNTM.ID,IDNTM.Name,IDNTM.Message} as CoolTable");
				c = new CommandInterpreter(loadedDatabase, "{NameTable2(Name)=CoolTable(Name)[CoolTable.Name=*]@CoolTable.ID}");
				
				(c.output as Table)?.PrintTableNoPadding();
				(c.output as Table)?.Dump();
				Console.WriteLine("Tuples Created: {0}", RelationalAlgebraModule.TuplesCreated);
				RelationalAlgebraModule.ResetTuplesCreated();
				/*
				c = new CommandInterpreter(loadedDatabase,
					@"	{NN(ID)=RoleInfo(ID)
						[RoleInfo.Role=Admin]
						@NN.Name,NN.Nickname}");
				(c.output as Table)?.PrintTableNoPadding();
				
				Console.WriteLine("Tuples Created: {0}", RelationalAlgebraModule.TuplesCreated);

				*/
				
			}
			
		
			
		}

		

	}
}