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

		private static string command = "";
		private static bool buttonClicked = false;

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
				
				while (loadedDatabase != null && dontStop) {
					Console.Write(">>  ");
					bool endCommand = false;
					command = "";
					while (!endCommand) {
						var keyInfo = Console.ReadKey();
						buttonClicked = true;
						Regex acceptableCharacters = new Regex("\\w");
						if(!suggestionRunning) DisplaySuggestions(3000);
						if (keyInfo.Key == ConsoleKey.Enter) {
							Console.WriteLine();
							Console.Write("  ");
						}

						if (keyInfo.Key == ConsoleKey.Backspace) {
							command = command.Substring(0, command.Length - 1);
							Console.Write(" ");
							Console.Write("\b");
						}
						if (keyInfo.KeyChar == ';') {
							Console.WriteLine();
							if (command.ToLower() == "exit") {
								dontStop = false;
								break;
							}
							endCommand = true;
						} else if(acceptableCharacters.IsMatch("" + keyInfo.KeyChar)) command += keyInfo.KeyChar;
					}

					if (dontStop) {
						var commandInterpreter = new CommandInterpreter(loadedDatabase, command);
					}
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

				
				 
				
				
				AlgebraNode n01 = new AlgebraNode(idRole);
				AlgebraNode n00 = new AlgebraNode(nameNicknameId);
				AlgebraNode n0 = new AlgebraNode(idntm);
				AlgebraNode n1 = new AlgebraNode(RelationalAlgebraModule.Selection, new []{"IDNTM.Time=*2017 *"}, n0);
				AlgebraNode n2 = new AlgebraNode(RelationalAlgebraModule.InnerJoin, new []{"IDNTM(Name)=NN(Name)"}, n1, n00);
				AlgebraNode n3 = new AlgebraNode(RelationalAlgebraModule.InnerJoin, new []{"(...)(NN.ID)=RoleInfo(ID)"}, n2, n01);
				AlgebraNode n4 = new AlgebraNode(RelationalAlgebraModule.Projection, new []{"IDNTM.ID","NN.Nickname","RoleInfo.Role","IDNTM.Message"}, n3);
				Table t = n4.TableApply();
			
				t?.DumpData();
				t?.PrintTableNoPadding();
				*/
			}
			
		
			
		}

		private static bool suggestionRunning = false;
		static async Task DisplaySuggestions(int delay) {
			suggestionRunning = true;
			if (buttonClicked) {
				buttonClicked = false;
				await Task.Delay(delay);
				if (!buttonClicked) {
					string suggestion = "SUGGESTION";
					Console.Write(suggestion + "?");
				}

				suggestionRunning = false;
			}

		}

	}
}