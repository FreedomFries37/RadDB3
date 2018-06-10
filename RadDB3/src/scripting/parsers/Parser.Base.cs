
using System.Collections;
using System.IO;
using System.Net;

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
		/// Same as MatchString, but also moves the pointer forward
		/// </summary>
		/// <param name="str">The string to check</param>
		/// <returns>If it matches</returns>
		private bool ConsumeString(string str) {
			if (!MatchString(str)) return false;
			index += str.Length;
			return true;
		}

	}
}