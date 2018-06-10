
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace RadDB3.scripting.parsers {
	public partial class Parser {

		private string parsableString;
		private int index;
		private ParseNode head;

		public char CurrentCharacter => parsableString[index];
		
		public enum ParseOptions {
			REMOVE_ALL_WHITESPACE,
			REMOVE_ALL_NONSPACE_WHITESPACE,
			ALL_WHITESPACE_TO_SPACE
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

		public Parser(string s, ReadOptions readOptions, ParseOptions parseOption) : this(s, readOptions) {
			parsableString = ApplyRule(parsableString, parseOption);
		}

		private string ApplyRule(string s, ParseOptions option) {
			switch (option) {
					case ParseOptions.ALL_WHITESPACE_TO_SPACE:
						s = s.Replace('\t', ' ');
						s = s.Replace('\n', ' ');
						break;
					
					case ParseOptions.REMOVE_ALL_WHITESPACE:
						s = s.Replace("\t", "");
						s = s.Replace("\n", "");
						s = s.Replace(" ", "");
						break;
					
					case ParseOptions.REMOVE_ALL_NONSPACE_WHITESPACE:
						break;
			}

			return s;
		}

		/// <summary>
		/// Advances the pointer by one char, unless its at the end of the file
		/// </summary>
		/// <returns>if the pointer moved forward</returns>
		private bool AdvancePointer() {
			if (index >= parsableString.Length - 1) return false;

			index++;
			return true;
		}

		/// <summary>
		/// Returns true if the character matches the current character
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>If c matches the current character</returns>
		private bool MatchChar(char c) {
			return c == CurrentCharacter;
		}

		/// <summary>
		/// Same as Match char, but also advances the pointer
		/// </summary>
		/// <param name="c">The character to check</param>
		/// <returns>If c matches the current character</returns>
		private bool ConsumeChar(char c) {
			if (!MatchChar(c)) return false;
			AdvancePointer();
			return true;
		}

		/// <summary>
		/// Returns true if you can match the input string to the string being parsed
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <returns>If it matches</returns>
		private bool MatchString(string str) {
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
		private bool MatchPattern(string @pattern) {
			Regex regex = new Regex(pattern);
			int tempIndex = 1;
			string check = parsableString.Substring(index);
			while (tempIndex < check.Length) {
				string tempSentence = check.Substring(0, tempIndex);
				if (regex.IsMatch(tempSentence)) {
					return true;
				}

				tempIndex++;
			}

			return false;
		}

		private bool ConsumePattern(string @pattern) {
			Regex regex = new Regex(pattern);
			int tempIndex = 1;
			string check = parsableString.Substring(index);
			while (tempIndex < check.Length) {
				string tempSentence = check.Substring(0, tempIndex);
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
		

		/// <summary>
		/// Same as MatchString, but also moves the pointer forward
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <returns>If it matches</returns>
		private bool ConsumeString(string str) {
			if (!MatchString(str)) return false;
			index += str.Length;
			return true;
		}
		
		/**
		 * <sentence>:
		 *
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
		 */

		public bool ParseSentence(out ParseNode output) {
			output = new ParseNode("<sentence>");

			if (!ConsumeChar('"')) return false;
			if (!ParseSentencePrime(output)) return false;
			if (!ConsumeChar('"')) return false;
			
			return true;
		}
		
		private bool ParseSentence(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence>");

			if (!ConsumeChar('"')) return false;
			if (!ParseSentencePrime(next)) return false;
			if (!ConsumeChar('"')) return false;
			
			parent.AddChild(next);
			return true;
		}
		
		private bool ParseSentencePrime(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence'>");

			if (!ParseSentenceChar(next)) return false;
			if (!ParseSentenceTail(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseSentenceChar(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence_char>");

			if (!MatchPattern(@"[\w]")) return false;
			
			ParseNode nextNext = new ParseNode("" + CurrentCharacter);
			
			next.AddChild(nextNext);
			AdvancePointer();
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseSentenceTail(ParseNode parent) {
			ParseNode next = new ParseNode("<sentence_tail>");

			if (!MatchChar('"') && !ParseSentencePrime(next)) return false;
			
			parent.AddChild(next);
			return true;
		}
		private bool ParseString(ParseNode parent) {
			ParseNode next = new ParseNode("<string>");

			
			parent.AddChild(next);
			return true;
		}
		private bool ParseChar(ParseNode parent) {
			ParseNode next = new ParseNode("<char>");

			
			parent.AddChild(next);
			return true;
		}
		private bool ParseStringTail(ParseNode parent) {
			ParseNode next = new ParseNode("<string_tail>");

			
			parent.AddChild(next);
			return true;
		}

	}
}