namespace RadDB3.scripting.parsers {
	public partial class Parser{
		
		/**
		 * <join_info>:
		 * 		<table_name>(<columns>)=<table_name>(<columns>)
		 *
		 * <table_name>:
		 * 		<string>
		 * 		<sentence>
		 *
		 * <columns>:
		 * 		<column_name><column_more>
		 * 
		 * <column_name>:
		 * 		<string>
		 * 		<sentence>
		 *
		 * <column_more>: optional
		 * 		,<columns>
		 */

		public bool ParseJoinInfo(out ParseNode parent) {
			parent = null;
			ParseNode next = new ParseNode("<join_info>");

			if (!ParseTableName(next)) return false;
			if (!ConsumeChar('(')) return false;
			if (!ParseColumns(next)) return false;
			if (!ConsumeString(")=")) return false;
			if (!ParseTableName(next)) return false;
			if (!ConsumeChar('(')) return false;
			if (!ParseColumns(next)) return false;
			if (!ConsumeChar(')')) return false;

			parent = next;
			return true;
		}

		private bool ParseTableName(ParseNode parent) {
			ParseNode next = new ParseNode("<table_name>");

			if (MatchChar('"')) {
				if (!ParseSentence(next)) return false;
			} else {
				if (!ParseString(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseColumns(ParseNode parent) {
			ParseNode next = new ParseNode("<columns>");

			if (!ParseColumnName(next)) return false;
			if (!ParseColumnMore(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseColumnName(ParseNode parent) {
			ParseNode next = new ParseNode("<column_name>");
			
			if (MatchChar('"')) {
				if (!ParseSentence(next)) return false;
			} else {
				if (!ParseString(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseColumnMore(ParseNode parent) {
			ParseNode next = new ParseNode("<column_more>");

			if (ConsumeChar(',')) {
				if (!ParseColumns(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}
	}
}