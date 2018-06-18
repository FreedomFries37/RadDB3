using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace RadDB3.scripting.parsers {
	public partial class Parser{
		
		/**
		 *	<join_info_full>:
		 * 		<join_object><join_type><join_object>
		 * 		<join_object><join_type><join_object>
		 *
		 * 	<join_object>:
		 * 		<table_name>(<columns>)
		 *		(<columns>)<table_name>
		 * 		(<columns>)(<join_info_full>)
		 * 		(<join_info_full>)(<columns>)
		 *
		 * 	<join_type>:
		 * 		= ##inner
		 * 		< ##left
		 * 		> ##right
		 * 		o ##outer
		 * 
		 * <join_info>:
		 * 		<table_name>(<columns>)=<table_name>(<columns>)
		 * 		(...)(<columns>)=<table_name>(<columns>)
		 * 		<table_name>(<columns>)=(...)(<columns>)
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

		/// <summary>
		/// Automatically fixed order of columns so that are after the table or join object
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public bool ParseJoinInfoFull(out ParseNode parent) {
			ParseNode next = new ParseNode("<join_info_full>");
			parent = null;
			
			if (!ParseJoinObject(next)) return false;
			if (!ParseJoinType(next)) return false;
			if (!ParseJoinObject(next)) return false;

			parent = next;
			return true;
		}

		protected bool ParseJoinInfoFull(ParseNode parent) {
			ParseNode next = new ParseNode("<join_info_full>");
			
			if (!ParseJoinObject(next)) return false;
			if (!ParseJoinType(next)) return false;
			if (!ParseJoinObject(next)) return false;

			
			parent.AddChild(next);
			return true;
		}

		private bool ParseJoinObject(ParseNode parent) {
			ParseNode next = new ParseNode("<join_object>");

			SaveState s = SaveParserState(next);
			if (ParseTableName(next) &&
				ConsumeChar('(') &&
				ParseColumns(next) &&
				ConsumeChar(')')) {
				parent.AddChild(next);
				return true;
			}
			LoadSaveState(s,next);
			if (ConsumeChar('(') &&
				ParseColumns(next) &&
				ConsumeChar(')')) {
				SaveState p = SaveParserState();
				if (ParseTableName(next)) {
					
					
					parent.AddChild(next);
					next.ChildrenList.Reverse();
					return true;
				}
				
				LoadSaveState(p);
				if (ConsumeChar('(') &&
					ParseJoinInfoFull(next) &&
					ConsumeChar(')')) { 
					
					parent.AddChild(next);
					next.ChildrenList.Reverse();
					return true;
				}
			}
			LoadSaveState(s,next);

			if (ConsumeChar('(') &&
				ParseJoinInfoFull(next) &&
				ConsumeString(")(") &&
				ParseColumns(next) &&
				ConsumeChar(')')) {
				parent.AddChild(next);
				return true;
			}

			return false;
		}

		private bool ParseJoinType(ParseNode parent) {
			ParseNode next = new ParseNode("<join_type>");

			string joinType = "" + CurrentCharacter;
			if (ConsumeChar('=') ||
				ConsumeChar('<') ||
				ConsumeChar('>') ||
				ConsumeChar('=')) {
				next.AddChild(new ParseNode(joinType));
			} else return false;
			
			parent.AddChild(next);
			return true;
		}

		public bool ParseJoinInfo(out ParseNode parent) {
			parent = null;
			ParseNode next = new ParseNode("<join_info>");

			if (ParseTableName(next)) {
				if (!ConsumeChar('(')) return false;
				if (!ParseColumns(next)) return false;
				if (!ConsumeString(")=")) return false;
			} else {
				if (!ConsumeString("(...)")) return false;
				next.AddChild(new ParseNode("TABLE DATA"));
				if (!ConsumeChar('(')) return false;
				if (!ParseColumns(next)) return false;
				if (!ConsumeString(")=")) return false;
			}
			
			if (ParseTableName(next)) {
				if (!ConsumeChar('(')) return false;
				if (!ParseColumns(next)) return false;
				if (!ConsumeChar(')')) return false;
			} else {
				if (!ConsumeString("(...)")) return false;
				next.AddChild(new ParseNode("TABLE DATA"));
				if (!ConsumeChar('(')) return false;
				if (!ParseColumns(next)) return false;
				if (!ConsumeChar(')')) return false;
			}

			parent = next;
			return true;
		}

		private bool ParseTableName(ParseNode parent) {
			ParseNode next = new ParseNode("<table_name>");

			if (!ParseString(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		public bool ParseColumns(ParseNode parent) {
			ParseNode next = new ParseNode("<columns>");

			if (!ParseColumnName(next)) return false;
			if (!ParseColumnMore(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseColumnName(ParseNode parent) {
			ParseNode next = new ParseNode("<column_name>");
			
			/*
			if (MatchChar('"')) {
				if (!ParseSentence(next)) return false;
			} else {
				if (!ParseString(next)) return false;
			}
			*/
			if (!ParseString(next)) return false;
			
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

	
		public static string[] ConvertColumns(ParseNode parseNode) {
			if(parseNode.Data != "<columns>") throw new IncompatableParseNodeException();
			
			List<string> output = new List<string>();
			ParseNode nodePtr = parseNode;
			do {
				if (nodePtr.Data == "<column_more>") nodePtr = nodePtr["<columns>"];
				output.Add(nodePtr["<column_name>"][0][0].Data);
				nodePtr = nodePtr["<column_more>"];
			} while (nodePtr != null);

			return output.ToArray();
		}

		public static string ConvertColumnsOneString(ParseNode p) {
			string output = "";
			string[] names = ConvertColumns(p);
			for (int i = 0; i < names.Length-1; i++) {
				output += names[i] + ",";
			}

			output += names[names.Length - 1];
			return output;
		}
		
		
	}
}