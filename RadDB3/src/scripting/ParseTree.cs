using System;

namespace RadDB3.scripting {
	public class ParseTree {
		private ParseNode head;
		public readonly bool successfulParse;

		public int Count => head != null ? head.Count() : 0;

		public ParseTree(ParserFunction func) {
			successfulParse = func(out head);
			if(successfulParse) head.CleanUp();
		}

		public void PrintTree() {
			head.Print(0);
		}

		public ParseNode this[string s] => head[s];

	}
}