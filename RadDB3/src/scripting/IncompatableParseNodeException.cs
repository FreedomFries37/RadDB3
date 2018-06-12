using System;

namespace RadDB3.scripting {
	public class IncompatableParseNodeException : Exception{
		public override string ToString() {
			return "Node type incompatable";
		}

	}
}