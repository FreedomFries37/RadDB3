using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using RadDB3.interaction;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.scripting.RelationalAlgebra;
using RadDB3.structure;
using RadDB3.structure.Types;

namespace RadDB3 {

	
	
	static class Program {
		private static Database loadedDatabase;
		
		static void Main(string[] args) {
			
			if (args.Length == 0 || args[0] == "DEBUG") {
				Database db = new Database("TestDatabase");
				Relation r = new Relation(("*Name", typeof(RADString)),
					("Age", typeof(RADInteger)));
				//RADTuple t = new RADTuple(r, new RADString("Josh"), new RADInteger(32), new RADGeneric<bool>(false));

				Table tb1 = new Table("NA", r);


				tb1.Add("Dan", 14);
				tb1.Add("Radc", 67);
				tb1.Add("Eli", 16);
				tb1.Add("Jake", 252);
				tb1.Add("Steve", 12);
				tb1.Add("Max", 44);
				tb1.Add("David", 55);
				tb1.Add("Noah", 5);

				Table tb2 = new Table("NDoB", new Relation(("*name", typeof(RADString)),
					("&DoB", typeof(RADDate))));

				tb2.Add("Dan", new DateTime(1999, 5, 7));
				tb2.Add("Radc", new DateTime(1925, 5, 7));
				tb2.Add("Eli", new DateTime(1945, 5, 7));
				tb2.Add("Jake", new DateTime(1979, 5, 7));
				tb2.Add("Steve", new DateTime(1979, 5, 7));
				tb2.Add("Max", new DateTime(2009, 5, 7));
				tb2.Add("David", new DateTime(1939, 5, 7));
				tb2.Add("Noah", new DateTime(1945, 5, 7));

				Element[] elements = tb2.SecondaryIndexing.Get(("DoB", "5/7/*"));

				db.addTable(tb1);
				db.addTable(tb2);

				AlgebraNode n0 = new AlgebraNode(tb2);
				AlgebraNode n1 = new AlgebraNode(RelationalAlgebraModule.Selection, new []{"\"Name\"=\"Dan\""}, n0);
				RADTuple[] tuples = n1.Apply();

				FileInteraction.ConvertDatabaseToFile(db);
				Commands.SelectTable(db, tb1.Name).PrintTableNoPadding();
				FileInteraction.ConvertDirectoriesInCurrentDirectoryToDatabases()[0].PrintDataBase();
			} else {
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
	}
}