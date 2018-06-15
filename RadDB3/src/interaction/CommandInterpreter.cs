
using System.Reflection.Metadata.Ecma335;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.scripting.RelationalAlgebra;
using RadDB3.structure;
using static RadDB3.interaction.Commands;

namespace RadDB3.interaction {
	public class CommandInterpreter : Parser {

		public RADObject output { get; private set; }
		private Database db;
		
		public CommandInterpreter(Database db, string command, params string[] options) :
			base(command, ReadOptions.STRING, ParseOptions.REMOVE_ALL_NONSPACE_WHITESPACE) {
			this.db = db;
			ParseTree tree = new ParseTree(ParseCommand);
			tree.PrintTree();
			
			head = tree.Head;
			CommandObject createdObject = ConvertObject(head["<object>"]);
			output = createdObject?.Data;
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
					tableOutput = new AlgebraNodeTree(db, projections, selections, tableNode["<join_info_full>"]).TableAply();
				} else {
					tableOutput = new AlgebraNodeTree(db, projections, selections, tableNode["<string>"][0].Data).TableAply();
				}

			} else {
				string tableInfo = parseNode["<string>"].Data;
				tableOutput = db[tableInfo];
			}

			

			return new CommandTable(tableOutput);
		}
		
		
		

		public bool ParseCommand(out ParseNode parent) {
			parent = null;
			ParseNode output = new ParseNode("<command>");
			SaveState s1 = SaveParserState(output);
			
			// Table based commands
			if (ParseObject(output)) {
				
				
				parent = output;
				return true;
			}

			return true;
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
				
				parent.AddChild(next);
				return true;
			}


			return false;
		}
		
		private bool ParseTupleList(ParseNode parent) {
			ParseNode next = new ParseNode("<method>");
			
			
			parent.AddChild(next);
			return true;
		}
		
		private bool ParseTuple(ParseNode parent) {
			ParseNode next = new ParseNode("<method>");
			
			
			parent.AddChild(next);
			return true;
		}
		
		private bool ParseElement(ParseNode parent) {
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