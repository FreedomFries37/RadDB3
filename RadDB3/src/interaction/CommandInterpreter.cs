﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.scripting.RelationalAlgebra;
using RadDB3.structure;
using static RadDB3.interaction.Commands;

namespace RadDB3.interaction {
	public class CommandInterpreter : Parser {

		public RADObject output { get; private set; }
		private Database db;

		public static readonly Dictionary<string, CommandObject> variables;

		private static readonly string[] KeyWords = {
			"return",
			"setVar"
		};

		static CommandInterpreter() {
			variables = new Dictionary<string, CommandObject>();
		}
		
		public CommandInterpreter(Database db, string command, params string[] options) :
			base(command, ReadOptions.STRING, ParseOptions.REMOVE_ALL_NONSPACE_WHITESPACE, KeyWords) {
			this.db = db;
			ParseTree tree = new ParseTree(ParseCommandStructure);
			

			head = tree.Head;
			foreach (var node in ConvertListNodeToListOfListObjects(head["<list_command>"])) {
				DoCommand(node);
			}
			
		}

		public bool ObjectExists(string name) {
			return variables.ContainsKey(name) || db.Contains(name);
		}


		public void DoCommand(ParseNode n) {
			if(!n.Equals("<command>")) throw new IncompatableParseNodeException();
			n.Print(1);
			List<CommandObject> objects = new List<CommandObject>();
			string methodName = "";
			ParseNode methodNode = null;
		
			if (n.Contains("<method>")) {
				methodName = n["<method>"][0].Data == "<string>" ? n["<method>"][0][0].Data : n["<method>"][0].Data;
				methodNode = n["<method>"];
			}

			foreach (var node in n.GetAllOfTypeOnlyDirectChildren("<object>")) {
				objects.Add(ConvertObject(node));
			}

			switch (methodName) {
				case "setVar": {
					string varName = "$" + (methodNode?["<string>"][0].Data ?? "temp");
					objects[0].SetName(varName);
					Table t = objects[0].Data as Table;
					t?.Relation.StripTableNames();
					if (variables.ContainsKey(varName)) {
						variables[varName] = objects[0];
					} else variables.Add(varName, objects[0]);
				}
					break;
				case "return": {
					output = objects[0].radObject;
				}
					break;
				default: {
					output = objects[objects.Count - 1].radObject;
				}
					break;
			}
		}


		public CommandObject ConvertObject(ParseNode parseNode) {

			if (parseNode["<table>"] != null) {
				return ConvertTable(parseNode["<table>"]);
			}

			return null;
		}

		public CommandTable ConvertTable(ParseNode parseNode) {
			if(!parseNode.Equals("<table>")) throw new IncompatableParseNodeException();

			Table tableOutput;
			
			if (parseNode["<list_selection>"] != null) {
				string[] selections, projections;
				ParseNode tableNode = parseNode["<table>"];

				if (parseNode.Contains("<columns>")) {
					projections = ConvertColumns(parseNode["<columns>"]);
				} else projections = new string[0];

				ParseNode[] selectionNodes = ConvertListNodeToListOfListObjects(parseNode["<list_selection>"]).ToArray();
				selections = new string[selectionNodes.Length];
				for (int i = 0; i < selectionNodes.Length; i++) {
					selections[i] = $"{selectionNodes[i][0][0].Data}={selectionNodes[i][1][0].Data}";
				}
				
				if (tableNode.Contains("<join_info_full>")) {
					var tree = new AlgebraNodeTree(db, projections, selections, tableNode["<join_info_full>"]);
					tableOutput = tree.TableAply();
					tree.PrintTree();
				} else if(tableNode.Contains("<table>")){
					Table t = ConvertTable(tableNode["<table>"]).Data as Table;
					var tree = new AlgebraNodeTree(t, projections, selections);
					tableOutput = tree.TableAply();
					tree.PrintTree();
				} else {
					var tree = new AlgebraNodeTree(db, projections, selections, tableNode["<string>"][0].Data);
					tableOutput = tree.TableAply();
					tree.PrintTree();
				}

			} else if (parseNode.Contains("<join_info_full>")) {
				var tree = new AlgebraNodeTree(db, new string[0], new string[0], parseNode["<join_info_full>"]);
				tableOutput = tree.TableAply();
				tree.PrintTree();
			} else {
				string tableInfo = parseNode["<string>"][0].Data;
				if (variables.ContainsKey(tableInfo)) {
					tableOutput = variables[tableInfo].Data as Table;
					if (tableOutput == null) tableOutput = db[tableInfo];
				} else tableOutput = db[tableInfo];
			}

			

			return new CommandTable(tableOutput);
		}

		public bool ParseCommandStructure(out ParseNode parent) {
			parent = null;
			ParseNode output = new ParseNode("<command_structure>");

			if (!ParseList(output, ParseCommand, ";")) return false;

			parent = output;
			return true;
		}
		

		public bool ParseCommand(out ParseNode parent) {
			parent = null;
			ParseNode output = new ParseNode("<command>");
			SaveState s1 = SaveParserState(output);
			output.AddChild(new ParseNode("<method>"));
			
			
		
			
			
			if (ParseString(output["<method>"])) {
				if (ConsumeChar('(')) {

					if (!ParseList(output, ParseObject)) return false;
					if (!ConsumeChar(')')) return false;
					
					parent = output;
					return true;
				}
				if (ConsumePattern("\\s+")) {
					if (ParseObject(output)) {
						parent = output;
						return true;
					}
				}
			}
			LoadSaveState(s1, output);
			// Table based commands
			if (ParseObject(output)) {
				if (ConsumeString(" as ")) {
					output["<method>"].AddChild(new ParseNode("setVar"));
					if (!ParseString(output["<method>"])) return false;
				}
				
				parent = output;
				return true;
			}

			return true;
		}

		public bool ParseCommand(ParseNode parent) {
			bool occured = ParseCommand(out ParseNode next);

			if (occured) {
				parent.AddChild(next);
				return true;
			}

			return false;
		}


		public bool ParseObject(out ParseNode parent) {
			parent = null;
			ParseNode next = new ParseNode("<object>");

			SaveState s = SaveParserState(next);

			if (ConsumeChar('(') &&
				ParseObject(next) &&
				ConsumeChar(')')) {

				if (ConsumeChar('.')) {
					if (!ParseMethod(next)) return false;
				}
				
				parent = next;
				return true;
			}
			
			LoadSaveState(s);
			if (ParseTable(next)) {
				
				if (ConsumeChar('.')) {
					if (!ParseMethod(next)) return false;
				}
				
				parent = next;
				return true;
			}
			
			LoadSaveState(s);
			if (ParseTupleList(next)) {
				
				if (ConsumeChar('.')) {
					if (!ParseMethod(next)) return false;
				}
				
				parent = next;
				return true;
			}
			
			LoadSaveState(s);
			if (ParseTuple(next)) {
				
				if (ConsumeChar('.')) {
					if (!ParseMethod(next)) return false;
				}
				parent = next;
				return true;
			}
			
			LoadSaveState(s);
			if (ParseElement(next)) {
				
				if (ConsumeChar('.')) {
					if (!ParseMethod(next)) return false;
				}
				parent = next;
				return true;
			}

			return false;
		}

		private bool ParseObject(ParseNode parent) {
			if (!ParseObject(out ParseNode next)) {
				return false;
			}

			parent.AddChild(next);
			return true;
		}

		private bool ParseMethod(ParseNode parent) {
			ParseNode next = new ParseNode("<method>");

			if (!ParseString(next)) return false;
			if (!ConsumeChar('(')) return false;
			if (!ParseList(next, ParseObject)) return false;
			if (!ConsumeChar(')')) return false;
			
			parent.AddChild(next);
			return true;
		}
		
		private bool ParseTable(ParseNode parent) {
			ParseNode next = new ParseNode("<table>");
			SaveState s = SaveParserState(next);
			
			

			if (ConsumeChar('{')) {
				if (!ParseTable(next)) return false;
				if (!ConsumeChar('[')) return false;
				if (!ParseList(next, ParseSelection)) return false;
				if (!ConsumeChar(']')) return false;
				if (ConsumeChar('@')) {
					if (!ParseColumns(next)) return false;
				}

				if (!ConsumeChar('}')) return false;
				
				parent.AddChild(next);
				return true;
			}
			
			LoadSaveState(s);

			if (ParseJoinInfoFull(next)) {
				parent.AddChild(next);
				return true;
			}
			LoadSaveState(s);
			
			
			if (ParseString(next)) {
				string name = ConvertString(next["<string>"]);
				
				if (ObjectExists(name)) {
					parent.AddChild(next);
					return true;
				}
			}


			return false;
		}
		
		private bool ParseTupleList(ParseNode parent) {
			return false;
			ParseNode next = new ParseNode("<method>");
			
			
			parent.AddChild(next);
			return true;
		}
		
		private bool ParseTuple(ParseNode parent) {
			return false;
			ParseNode next = new ParseNode("<method>");
			
			
			parent.AddChild(next);
			return true;
		}
		
		private bool ParseElement(ParseNode parent) {
			return false;
			ParseNode next = new ParseNode("<method>");
			
			
			parent.AddChild(next);
			return true;
		}

		private bool ParseSelection(ParseNode parent) {
			ParseNode next = new ParseNode("<selection>");

			if (!ParseString(next)) return false;
			if (!ConsumeChar('=')) return false;
			if (!ParseString(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
	}
}