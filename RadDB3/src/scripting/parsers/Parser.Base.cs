
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;

namespace RadDB3.scripting.parsers {
	public delegate bool ParserFunction(out ParseNode node);

	public delegate bool InternalParserFunction(ParseNode node);
	
	
	public partial class Parser {
		protected string parsableString;
		protected int index;
		protected ParseNode head;

		public char CurrentCharacter => index < parsableString.Length ? parsableString[index] : (char) 420;
		
		public enum ParseOptions {
			REMOVE_ALL_WHITESPACE,
			REMOVE_ALL_NONSPACE_WHITESPACE,
			ALL_WHITESPACE_TO_SPACE,
			REMOVE_ONLY_TABS
		}

		
		/// <summary>
		/// Used For Nondeterminate Grammars
		/// </summary>
		protected class SaveState {
			public readonly int savedIndex;
			public readonly string originalString;
			public readonly List<ParseNode> nodeData;

			public SaveState(Parser parser) {
				savedIndex = parser.index;
				originalString = parser.parsableString;
				nodeData = null;
			}
			
			public SaveState(Parser parser, ParseNode node) {
				savedIndex = parser.index;
				originalString = parser.parsableString;
				nodeData = new List<ParseNode>(node.Children);
			}
		}

		protected SaveState SaveParserState() {
			return new SaveState(this);
		}
		
		protected SaveState SaveParserState(ParseNode p) {
			return new SaveState(this, p);
		}

		protected void LoadSaveState(SaveState s) {
			index = s.savedIndex;
			parsableString = s.originalString;
		}
		
		protected void LoadSaveState(SaveState s, ParseNode p) {
			index = s.savedIndex;
			parsableString = s.originalString;
			p.ChildrenList = new List<ParseNode>(s.nodeData);
		}

		/// <summary>
		/// Parser Read Options
		/// </summary>
		public enum ReadOptions {
			/// <summary>
			/// Extract file at path s
			/// </summary>
			FILE_PATH, 
			/// <summary>
			/// Use the string as is
			/// </summary>
			STRING
		}

		public Parser(string s, ReadOptions option) {
			switch (option) {
					case ReadOptions.FILE_PATH:
						parsableString = File.ReadAllText(s);
						break;
					case ReadOptions.STRING:
						parsableString = s;
						break;
			}
			index = 0;
			head = null;
		}

		public Parser(string s, ReadOptions option, string replacement1, params string[] replacements) :this(s, option) {
			parsableString = parsableString.Replace(replacement1, "");
			foreach (string replacement in replacements) {
				parsableString = parsableString.Replace(replacement, "");
			}
		}

		public Parser(string s, ReadOptions readOptions, ParseOptions parseOption) : this(s, readOptions) {
			parsableString = ApplyRule(parsableString, parseOption);
		}

		private string ApplyRule(string s, ParseOptions option) {
			switch (option) {
					case ParseOptions.ALL_WHITESPACE_TO_SPACE:
						s = s.Replace('\t', ' ');
						s = s.Replace('\n', ' ');
						s = s.Replace("\r", "");
						break;
					
					case ParseOptions.REMOVE_ALL_WHITESPACE:
						s = s.Replace("\t", "");
						s = s.Replace("\n", "");
						s = s.Replace("\r", "");
						s = s.Replace(" ", "");
						break;
					
					case ParseOptions.REMOVE_ALL_NONSPACE_WHITESPACE:
						s = s.Replace("\t", "");
						s = s.Replace("\n", "");
						s = s.Replace("\r", "");
						break;
					case ParseOptions.REMOVE_ONLY_TABS:
						s = s.Replace("\t", "");
						break;
			}

			return s;
		}


		/// <summary>
		/// Advances the pointer by one char, unless its at the end of the file
		/// </summary>
		/// <returns>if the pointer moved forward</returns>
		protected bool AdvancePointer() {
			index++;
			return true;
		}

		/// <summary>
		/// Returns true if the character matches the current character
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>If c matches the current character</returns>
		protected bool MatchChar(char c) {
			return c == CurrentCharacter;
		}

		/// <summary>
		/// Same as Match char, but also advances the pointer
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>If c matches the current character</returns>
		protected bool ConsumeChar(char c) {
			if (!MatchChar(c)) return false;
			AdvancePointer();
			return true;
		}

		/// <summary>
		/// Returns true if you can match the input string to the string being parsed
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <returns>If it matches</returns>
		protected bool MatchString(string str) {
			int originalIndex = index;
			for (int i = 0; i < str.Length; i++) {
				if (originalIndex + i >= parsableString.Length) return false;
				if (str[i] != parsableString[originalIndex + i]) return false;
			}

			return true;
		}
		
		/// <summary>
		/// Attempt to match a regex patter
		///	WARNING: high overhead
		/// </summary>
		/// <param name="pattern">Regex pattern</param>
		/// <returns>If it matches</returns>
		protected bool MatchPattern(string pattern) {
			Regex regex = new Regex(pattern);
			int tempIndex = 1;
			string check = parsableString.Substring(index);
			while (tempIndex <= check.Length) {
				string tempSentence = check.Substring(0, tempIndex);
				Match m = regex.Match(tempSentence);
				if (m.Length != tempSentence.Length) {
					return false;
				}

				if (regex.IsMatch(tempSentence)) {
					return true;
				}

				tempIndex++;
			}

			return false;
		}

		protected bool ConsumePattern(string pattern) {
			Regex regex = new Regex(pattern);
			int tempIndex = 1;
			string check = parsableString.Substring(index);
			while (tempIndex < check.Length) {
				string tempSentence = check.Substring(0, tempIndex);
				if (regex.Match(tempSentence).Length > tempSentence.Length) {
					return false;
				}
				if (regex.IsMatch(tempSentence)) {
					for (int i = 0; i < tempIndex; i++) {
						AdvancePointer();
					}
					return true;
				}

				tempIndex++;
			}

			return false;
		}

		protected bool ConsumePattern(string pattern, out string matchedString) {
			Regex regex = new Regex(pattern);
			int tempIndex = 1;
			string check = parsableString.Substring(index);
			while (tempIndex < check.Length) {
				string tempSentence = check.Substring(0, tempIndex);
				if (regex.IsMatch(tempSentence)) {
					if (regex.Match(tempSentence).Length > tempSentence.Length) {
						matchedString = null;
						return false;
					}
					for (int i = 0; i < tempIndex; i++) {
						AdvancePointer();
					}

					matchedString = tempSentence;
					return true;
				}

				tempIndex++;
			}

			matchedString = null;
			return false;
		}
		

		/// <summary>
		/// Same as MatchString, but also moves the pointer forward
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <returns>If it matches</returns>
		protected bool ConsumeString(string str) {
			if (!MatchString(str)) return false;
			index += str.Length;
			return true;
		}

		public static string StringToSentence(string input) => "\"" + input + "\"";

		public static string lowerCamelCaseToLowerSeperated(string input) {
			string output = "";
			for (int i = 0; i < input.Length; i++) {
				char original = input[i];
				if (i > 0 && input.ToLower()[i] != original) output += "_" + input.ToLower()[i];
				else output += original.ToString().ToLower();
			}

			return output;
		}
		
		/**
		 * <sentence>:
		 * 		"<sentence'>"
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
		 * 		<sentence>
		 * 		<char><string_tail>
		 *
		 * <char>:
		 * 		[a-zA-Z0-9_]
		 *
		 * <string_tail>: optional
		 * 		<string>
		 */

		public bool ParseSentence(out ParseNode output) {
			output = new ParseNode("<sentence>");

			if (!ConsumeChar('"')) return false;
			if (!ParseSentencePrime(output)) return false;
			if (!ConsumeChar('"')) return false;
			
			return true;
		}
		
		protected bool ParseSentence(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence>");

			if (!ConsumeChar('"')) return false;
			if (!ParseSentencePrime(next)) return false;
			if (!ConsumeChar('"')) return false;
			
			parent.AddChild(next);
			return true;
		}

		
		
		protected bool ParseSentencePrime(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence'>");

			if (!ParseSentenceChar(next)) return false;
			if (!ParseSentenceTail(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		protected bool ParseSentenceChar(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence_char>");

			if (!MatchPattern(@".")) return false;
			
			ParseNode nextNext = new ParseNode("" + CurrentCharacter);
			
			next.AddChild(nextNext);
			AdvancePointer();
			
			parent.AddChild(next);
			return true;
		}
		protected bool ParseSentenceTail(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence_tail>");

			if (!MatchChar('"') && !ParseSentencePrime(next)) return false;
			
			parent.AddChild(next);
			return true;
		}


		protected bool ParseString(out ParseNode parent) {
			ParseNode next = new ParseNode("<string>");
			parent = null;
			if (!ParseChar(next)) return false;
			if (!ParseStringTail(next)) return false;

			parent = next;
			return true;
		}
		
		protected bool ParseString(ParseNode parent) {
			ParseNode next = new ParseNode("<string>");

			if (!ParseSentence(next)) {
				if (!ParseChar(next)) return false;
				if (!ParseStringTail(next)) return false;
			}

			parent.AddChild(next);
			return true;
		}
		private bool ParseChar(ParseNode parent) {
			ParseNode next = new ParseNode("<char>");

			if (!MatchPattern(@"[a-zA-Z_.0-9*]")) return false;
			ParseNode nextNext = new ParseNode("" + CurrentCharacter);
			
			next.AddChild(nextNext);
			AdvancePointer();
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseStringTail(ParseNode parent) {
			ParseNode next = new ParseNode("<string_tail>");

			if (MatchPattern(@"[a-zA-Z_.0-9*]")) {
				if (!ParseString(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}

		protected bool ParseInt(ParseNode parent) {
			ParseNode next = new ParseNode("<int>");

			if (!ParseDigit(next)) return false;
			if (!ParseIntTail(next)) return false;
			
			parent.AddChild(next);
			return true;
		}

		private bool ParseDigit(ParseNode parent) {
			ParseNode next = new ParseNode("<digit>");

			if (!MatchPattern(@"\d")) return false;
			
			next.AddChild(new ParseNode("" + CurrentCharacter));
			AdvancePointer();
			parent.AddChild(next);
			return true;
		}
		private bool ParseIntTail(ParseNode parent) {
			ParseNode next = new ParseNode("<int_tail>");

			if (MatchPattern(@"\d")) {
				if (!ParseInt(next)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}
		
		public static string ConvertSentence(ParseNode parent) {
			if (parent.Data != "<sentence>") return null;
			string output = "";
			ParseNode ptr = parent["<sentence'>"];
			do {
				output += ptr["<sentence_char>"].Children[0].Data;
				ptr = ptr["<sentence_tail>"]["<sentence'>"];
			} while (ptr != null);
			

			return output;
		}

		public static string ConvertString(ParseNode parent) {
			if (parent.Data != "<string>") return null;
			string output = "";

			ParseNode ptr = parent;
			do {
				output += ptr["<char>"].Children[0].Data;
				ptr = ptr["<string_tail>"]["<string>"];
			} while (ptr != null);
			
			return output;
		}

		public static string ConvertInt(ParseNode parent){
			if (parent.Data != "<int>") return null;
			
			string output = "";

			ParseNode ptr = parent;
			do {
				output += ptr["<digit>"].Children[0].Data;
				ptr = ptr["<int_tail>"]["<int>"];
			} while (ptr != null);
			
			return output;
		}
		
		/// <summary>
		/// In form object,object,...
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="function"></param>
		/// <returns></returns>
		public bool ParseList(ParseNode parent, InternalParserFunction function) {
			ParseNode next = new ParseNode($"<list_{lowerCamelCaseToLowerSeperated(function.Method.Name.Replace("Parse",""))}>");

			if (ParseListObject(next, function)) {
				if (!ParseListMore(next, function)) return false;
			} else {
				next.AddChild(new ParseNode("empty"));
			}
			
			parent.AddChild(next);
			return true;
		}

		private bool ParseListObject(ParseNode parent, InternalParserFunction function) {
			ParseNode next = new ParseNode("<list_object>");

			if (!function(next)) return false;
	
			parent.AddChild(next);
			return true;
		}

		private bool ParseListMore(ParseNode parent, InternalParserFunction function) {
			ParseNode next = new ParseNode("<list_more>");
			if (ConsumeChar(',')) {
				if (!ParseList(next,function)) return false;
			}
			
			parent.AddChild(next);
			return true;
		}

		public static List<ParseNode> ConvertListNodeToListOfListObjects(ParseNode parseNode) {
			if(!parseNode.Data.Contains("list_")) throw new IncompatableParseNodeException();
			if(parseNode.Contains("empty")) return new List<ParseNode>();
			List<ParseNode> output = new List<ParseNode>();
			ParseNode nodePtr = parseNode;
			do {
				if (nodePtr.Data == "<list_more>") nodePtr = nodePtr[0];
				output.Add(nodePtr["<list_object>"][0]);
				nodePtr = nodePtr["<list_more>"];
			} while (nodePtr != null);

			return output;
		}
		
		

	}
}