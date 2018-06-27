using System;
using System.Collections.Generic;
using System.Linq;
using RadDB3.scripting;
using RadDB3.scripting.parsers;

namespace RadDB3.interaction {
	public class CommandLine {


		private readonly List<string> args;
		public string[] Arguments => args.ToArray();
		private string[] options { get; set; }
		private readonly Dictionary<string, string> optionValues;

		public CommandLine(string[] _args) {
			args = _args.ToList();
			optionValues = new Dictionary<string, string>();
			Extrapolate();
		}
		
		public CommandLine(string input) {
			Parser p = new Parser(input, Parser.ReadOptions.STRING);
			ParseTree tree = new ParseTree(p.ParseCommandLine);
			List<ParseNode> list = Parser.ConvertListNodeToListOfListObjects(tree["<list_string>"]);
			
			args = new List<string>();
			for (int i = 0; i < list.Count; i++) {
				args.Add(Parser.ConvertString(list[i]));
			}
			optionValues = new Dictionary<string, string>();
			Extrapolate();
		}

		private void Extrapolate() {
			List<string> _options = new List<string>();
			foreach (string argument in Arguments) {
				if (argument.StartsWith('-') ||
					argument.StartsWith("--")) {
					string s = argument;
					while (s.StartsWith('-')) {
						s = s.Substring(1);
					}

					string option;
					if (s.Contains("=")) {
						option = s.Substring(0, s.IndexOf('='));
						string value = s.Substring(s.IndexOf('=') + 1);
						if (optionValues.ContainsKey(option)) {
							optionValues[option] = value;
						} else {
							optionValues.Add(option, value);
						}
					} else {
						option = s;
					}
					
					_options.Add(option);
				}
			}

			options = _options.ToArray();
		}

		public bool OptionEnabled(string o) {
			return options.Contains(o);
		}

		public string GetValue(string option) {
			if(!OptionEnabled(option)) throw new ArgumentException();
			if(!optionValues.ContainsKey(option)) throw new ArgumentOutOfRangeException();
			return optionValues[option];
		}
	}

	
}

namespace RadDB3.scripting.parsers {
	
	public partial class Parser {

		public bool ParseCommandLine(out ParseNode output) {
			output = null;
			ParseNode next = new ParseNode("<command_line>");

			if(!ParseList(next, ParseString, " "))
			

			output = next;
			return true;
		}
		
	}
}