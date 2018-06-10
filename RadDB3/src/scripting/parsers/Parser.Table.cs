

namespace RadDB3.scripting.parsers{
	
	/// <summary>
	/// Table file parser
	/// </summary>
	public partial class Parser {

		/**
		 * NAME: <string>;
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
		public bool ParseTable(out ParseNode n) {
			n = null;
			if (!MatchString("yolo")) return false;
			n = new ParseNode("yolo");
			return true;
		}
	}
}