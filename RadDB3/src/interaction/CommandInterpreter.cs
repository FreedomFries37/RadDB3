
using System.Reflection.Metadata.Ecma335;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.structure;
using static RadDB3.interaction.Commands;

namespace RadDB3.interaction {
	public class CommandInterpreter : Parser {

		public dynamic output { get; private set; }
		private Database db;
		
		public CommandInterpreter(Database db, string command, params string[] options) :
			base(command, ReadOptions.STRING) {
			this.db = db;
			ParseTree tree = new ParseTree(ParseObject);
			
		}
		
		public bool ParseCommand



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
			ParseNode next = new ParseNode("<method>");
			SaveState s = SaveParserState(next);
			if (ParseString(next)) {
				
				parent.AddChild(next);
				return true;
			}
			
			LoadSaveState(s);

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
	}
}