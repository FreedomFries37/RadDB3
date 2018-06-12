

using RadDB3.structure;

namespace RadDB3.scripting.parsers{
	
	/// <summary>
	/// Table file parser
	/// </summary>
	public partial class Parser {

		/**
		 * NAME:<string>;
		 * RELATION{
		 *		[<key_info>]<type> <name> (<constraints>);
		 *		.
		 * 		.
		 * 		.
		 * 	}
		 * 	TUPLES{
		 *		[<int>]<tupleString>,<tupleString>,..
		 * 		.
		 * 		.
		 * 		.
		 * 	}
		 *
		 * Grammar:
		 * <table_file>:
		 * 		NAME:<sentence>;\nRELATION{\n<relation_list>}\nTUPLES{\n<tuple_list>}
		 *
		 * <sentence>:
		 * 		"<string>"
		 *
		 * <sentence'>:
		 * 		<sentence_char><sentence_tail>
		 *
		 * <sentence_char>:
		 * 		[*"]
		 *
		 * <sentence_tail>: optional
		 * 		<sentence'>
		 * 
		 * <string>:
		 * 		<char><string_tail>
		 *
		 * <char>:
		 * 		[a-zA-Z0-9_]
		 *
		 * <string_tail>: optional
		 * 		<string>
		 *
		 * <relation_list>:
		 * 		<column_details><relation_list_tail>
		 *
		 * <relation_list_tail>: optional
		 * 		<relation_list>
		 *
		 * <column_details>:
		 * 		[<key_info>]<string> <sentence> (<contraints>);\n
		 *
		 * <key_info>:
		 * 		* //primary key (ONLY ONE)
		 * 		& //secondary key
		 * 		- //normal
		 *
		 * <contraints>: optional
		 * 		<single_constraint><constraints_tail>
		 *
		 * <single_constraint>:
		 * 		<string_full>(<contraints>)
		 *
		 * <constraits_tail>: optional
		 * 		,<constraints>
		 *
		 * <tuple_list>: optional
		 * 		[<int>]<TUPLE_STRING><tuple_list_more>\n<tuple_list_tail>
		 *
		 * <tuple_list_more>: optional
		 * 		,<TUPLE_STRING><tuple_list_more>
		 * 
		 * <tuple_list_tail>: optional
		 * 		<tuple_list>
		 */
		public bool ParseTable(out ParseNode n) {
			n = null;
			ParseNode output = new ParseNode("<table_file>");
			
			if (!ConsumeString("NAME:")) return false;
			if (!ParseSentence(output)) return false;
			if (!ConsumeString(";\nSIZE:")) return false;
			if (!ParseInt(output)) return false;
			if (!ConsumeString(";\nRELATION{\n")) return false;
			if (!ParseRelationList(output)) return false;
			if (!ConsumeString("}\nTUPLES{\n")) return false;
			if (!ParseTupleList(output)) return false;
			if (!ConsumeChar('}')) return false;
			
			n = output;
			return true;
		}
		private bool ParseRelationList(ParseNode parent) {
			ParseNode next = new ParseNode("<relation_list>");

			if (!ParseColumnDetails(next)) return false;
			if (!ParseRelationListTail(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseRelationListTail(ParseNode parent) {
			ParseNode next = new ParseNode("<relation_list_tail>");

			if (!MatchChar('}')) {
				if (!ParseRelationList(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}
		// [<key_info>]<string> <sentence> (<contraints>);\n
		private bool ParseColumnDetails(ParseNode parent) {
			ParseNode next = new ParseNode("<column_details>");

			if (!ConsumeChar('[')) return false;
			if (!ParseKeyInfo(next)) return false;
			if (!ConsumeChar(']')) return false;
			if (!ParseString(next)) return false;
			if (!ConsumeChar(' ')) return false;
			if (!ParseSentence(next)) return false;
			if (!ConsumeString(" (")) return false;
			if (!ParseContraints(next)) return false;
			if (!ConsumeString(");\n")) return false;
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseKeyInfo(ParseNode parent) {
			ParseNode next = new ParseNode("<key_info>");
			
			ParseNode nextNext;
			if (ConsumeChar('*')) {
				nextNext = new ParseNode("*");
			}else if (ConsumeChar('&')) {
				nextNext = new ParseNode("&");
			}else if (ConsumeChar('-')) {
				nextNext = new ParseNode("-");
			} else return false;

			next.AddChild(nextNext);
			parent.AddChild(next);
			return true;
		}
		private bool ParseContraints(ParseNode parent) {
			ParseNode next = new ParseNode("<contraints>");

			if (!MatchChar(')')) {
				if (!ParseSingleContraint(next)) return false;
				if (!ParseContraintsTail(next)) return false;
			}

			parent.AddChild(next);
			return true;
		}
		private bool ParseSingleContraint(ParseNode parent) {
			ParseNode next = new ParseNode("<single_constraint>");

			if (!ParseSentence(next)) return false;
			if (!ConsumeChar('=')) return false;
			if (!ParseSentence(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseContraintsTail(ParseNode parent) {
			ParseNode next = new ParseNode("<contraints_tail>");

			if (ConsumeChar(',')) {
				if (!ParseContraints(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}
		//[<int>]<TUPLE_STRING><tuple_list_more>\n<tuple_list_tail>
		private bool ParseTupleList(ParseNode parent) {
			ParseNode next = new ParseNode("<tuple_list>");

			if (!MatchChar('}')) {
				if (!ConsumeChar('[')) return false;
				if (!ParseInt(next)) return false;
				if (!ConsumeChar(']')) return false;
				if (!ParseTupleString(next)) return false;
				if (!ParseTupleListMore(next)) return false;
				if (!ConsumeChar('\n')) return false;
				if (!ParseTupleListTail(next)) return false;
			}

			parent.AddChild(next);
			return true;
		}
		private bool ParseTupleListTail(ParseNode parent) {
			ParseNode next = new ParseNode("<tuple_list_tail>");

			if (!MatchChar('}')) {
				if (!ParseTupleList(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}

		private bool ParseTupleString(ParseNode parent) {
			ParseNode next = new ParseNode("<tuple>");

			if (!ConsumePattern("{.*(" + RADTuple.ELEMENT_SEPERATOR + ".*)*}", out string singleTuple)) return false;
			next.AddChild(new ParseNode(singleTuple));
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseTupleListMore(ParseNode parent) {
			ParseNode next = new ParseNode("<tuple_more>");

			if (ConsumeChar(',')) {
				if (!ParseTupleString(next)) return false;
				if (!ParseTupleListMore(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}
	}
}