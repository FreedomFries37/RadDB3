using System;
using System.Collections.Generic;
using RadDB3.scripting.parsers;

namespace RadDB3.scripting {
	public class ParseTree {
		private ParseNode head;
		public readonly bool successfulParse;

		public ParseNode Head => head;

		public int Count => head != null ? head.Count() : 0;

		public ParseTree(ParserFunction func, bool cleanup = true) {
			successfulParse = func(out head);
			if(successfulParse && cleanup) head.CleanUp();
		}

		public void PrintTree() {
			head.Print(0);
		}

		public ParseNode this[string s] => head[s];

		public ParseNode this[int i] => head[i];

		public List<ParseNode> GetAllOfType(string type) {
			return head.GetAllOfType(type);
		}

	}
}