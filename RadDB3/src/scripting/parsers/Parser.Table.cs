

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
		 *		<tupleString>
		 * 		.
		 * 		.
		 * 		.
		 * 	}
		 *
		 * Grammar:
		 * <table_file>:
		 * 		NAME:<string_full>;\nRELATION{\n<relation_list>}\nTUPLES{\n<tuple_list>}
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
		 * 		[<key_info>]<string> <string_full> (<contraints>);\n
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
		 * 		<string_full>=<string_full>
		 *
		 * <constraits_tail>: optional
		 * 		,
		 *
		 * <tuple_list>: optional
		 * 		[TUPLE_STRING]\n<tuple_list_tail>
		 *
		 * <tuple_list_tail>: optional
		 * 		<tuple_list>
		 */
		public bool ParseTable(out ParseNode n) {
			n = null;
			if (!MatchString("yolo")) return false;
			n = new ParseNode("yolo");
			return true;
		}
	}
}