using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using RadDB3.scripting.parsers;
using RadDB3.structure;

namespace RadDB3.scripting.RelationalAlgebra {
	public class AlgebraNodeTree {
		private AlgebraNode head;
		public readonly bool successfulParse;

		public AlgebraNodeTree(Database db, string[] projections, string[] selections, string tableInfo) {
			successfulParse = false;
			Parser joinParser = new Parser(tableInfo, Parser.ReadOptions.STRING);
			ParseTree joinInfoTree = new ParseTree(joinParser.ParseJoinInfoFull);
			
			Dictionary<string, AlgebraNode> nameToAlgebraNode = new Dictionary<string, AlgebraNode>();

			if (joinInfoTree.successfulParse) { // creates join tree, unless there are no joins
				//joinInfoTree.PrintTree();
				List<ParseNode> tableNames = joinInfoTree.GetAllOfType("<table_name>");
				Table[] tables = new Table[tableNames.Count];

				int index = 0;
				foreach (ParseNode tableName in tableNames) {
					string name = tableName[0][0].Data;
					tables[index] = db[name];
					if (tables[index] == null) return;
					index++;
				}

				foreach (Table table in tables) {
					nameToAlgebraNode.Add(table.Name, new AlgebraNode(table));
				}

				AlgebraNode fixedJoins =
					ConvertJoinInfoFullIntoMultiplesJoinInfoString(joinInfoTree.Head, nameToAlgebraNode);
				if (fixedJoins == null) return;
				head = fixedJoins;
			} else {
				Table tb = db[tableInfo];
				if (tb == null) return;
				head = new AlgebraNode(tb);
			} 

		
			if (selections.Length > 0) {
				foreach (string selection in selections) {
					var algebraNode = head;
				
					
					Regex namedSelectionStyle = new Regex("\\w*.\\w*=\"?\\w\"?");
					if (namedSelectionStyle.IsMatch(selection)) {
						string tableName = selection.Split('.')[0];
						string columnName = selection.Split('=')[0].Split('.')[1];
						string data = selection.Split('=')[1];

						var originalNode = nameToAlgebraNode[tableName];
						var originalParent = originalNode.Parent;

						string modifiedSelection = $"{columnName}={data}";
						
						AlgebraNode n = new AlgebraNode(RelationalAlgebraModule.Selection,new []{selection}, originalNode);
						originalParent.ReplaceChild(originalNode,n);
						//originalNode.Options = new []{"pure"};
						originalParent.PrintTree();
					} else {
						AlgebraNode n = new AlgebraNode(RelationalAlgebraModule.Selection,new []{selection}, algebraNode);
						head = n;
					}
				}
			}

			if (projections.Length > 0) {
				var algebraNode = head;
				AlgebraNode p = new AlgebraNode(RelationalAlgebraModule.Projection, projections, algebraNode);
				head = p;
			}
			
			
			/**
			 * By now the general layout of the tree should be:
			 * Projections
			 * 		Selection
			 * 			Selection
			 * 				...
			 * 					Join/Table
			 * 						Join/Table
			 * 						Table/Table
			 */
			
			
			//Selection pushing
			
			
			
			

			PrintTree();
			successfulParse = true;
		}

		public AlgebraNodeTree(Database db, string[] projections, string[] selections, ParseNode joinInfoFull) : 
			this(db,projections,selections,ConvertJoinInfoNodeToString(joinInfoFull)) { }

		private static string ConvertJoinInfoNodeToString(ParseNode p) {
			if(!p.Equals("<join_info_full>")) throw new IncompatableParseNodeException();
			string output = "";
			output += ConvertJoinObjectToString(p[0]);
			output += p[1][0].Data;
			output += ConvertJoinObjectToString(p[2]);
			return output;
		}

		private static string ConvertJoinObjectToString(ParseNode p) {
			if(!p.Equals("<join_object>")) throw new IncompatableParseNodeException();
			
			if (p.Contains("<join_info_full>")) {
				return $"({ConvertJoinInfoNodeToString(p[0])})({Parser.ConvertColumnsOneString(p["<columns>"])})";
			}
			return $"{p["<table_name>"][0][0]}({Parser.ConvertColumnsOneString(p["<columns>"])})";
		}

		public RADTuple[] Apply() => head.Apply();
		public Table TableAply() => head.TableApply();

		private AlgebraNode ConvertJoinInfoFullIntoMultiplesJoinInfoString(ParseNode p, Dictionary<string, AlgebraNode> nameToAlgebraNode) {
			AlgebraNode left, right;
			string leftName, rightName, leftColumns, rightColumns;
			if(p[0].Contains("<table_name>")) {
				leftName = p[0]["<table_name>"][0][0].Data;
				left = nameToAlgebraNode[leftName];
				if (left == null) return null;
			} else {
				leftName = "(...)";
				left = ConvertJoinInfoFullIntoMultiplesJoinInfoString(p[0]["<join_info_full>"], nameToAlgebraNode);
				if (left == null) return null;
			}
			
			if(p[2].Contains("<table_name>")) {
				rightName = p[2]["<table_name>"][0][0].Data;
				right = nameToAlgebraNode[rightName];
				if (right == null) return null;
			} else {
				rightName = "(...)";
				right = ConvertJoinInfoFullIntoMultiplesJoinInfoString(p[2]["<join_info_full>"], nameToAlgebraNode);
				if (right == null) return null;
			}

			leftColumns = Parser.ConvertColumnsOneString(p[0]["<columns>"]);
			rightColumns = Parser.ConvertColumnsOneString(p[2]["<columns>"]);

			string joinType = p["<join_type>"][0].Data;

			string joinCommand = $"{leftName}({leftColumns}){joinType}{rightName}({rightColumns})";

			RelationalAlgebraFunction joinFunction = RelationalAlgebraModule.InnerJoin;
			switch (joinType) {
					case "=":
						joinFunction = RelationalAlgebraModule.InnerJoin;
						break;
					case "<":

						break;
			}

			return new AlgebraNode(joinFunction, new []{joinCommand}, left, right);
		}

		private List<AlgebraNode> FindAllJoinNodes() {
			return head.FindAll(RelationalAlgebraModule.InnerJoin);
		}

		public void PrintTree() => head.PrintTree();

		
	}
}
