using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using RadDB3.scripting.parsers;

namespace RadDB3.scripting {
	public class ParseNode {

		private string data;
		private List<ParseNode> children;
		
		public delegate dynamic ParseNodeConverter(ParseNode input);

		private dynamic convertedValue;
		public dynamic ConvertedValue => convertedValue;

		public string Data {
			set => data = value;
			get => data;
		}

		public ParseNode[] Children => children.ToArray();

		public List<ParseNode> ChildrenList {
			set => children = value;
			get => children;
		}

		public ParseNode(string data) {
			Data = data;
			children = new List<ParseNode>();
		}

		public void AddChild(ParseNode p) {
			children.Add(p);
		}

		public ParseNode this[string s] {
			get {
				foreach (ParseNode parseNode in Children) {
					if (parseNode.Data == s) return parseNode;
				}

				return null;
			}
		}

		public ParseNode this[int n] {
			get {
				if (n < 0 ||
					n >= children.Count) return null;
				return children[n];
			}
			set => children[n] = value;
		}

		public List<ParseNode> GetAllOfTypeOnlyDirectChildren(string s) {
			List<ParseNode> outout = new List<ParseNode>();
			foreach (ParseNode parseNode in children) {
				if (parseNode.data == s) {
					outout.Add(parseNode);
				}
			}

			return outout;
		}

		public List<ParseNode> GetAllOfType(string s) {
			List<ParseNode> outout = new List<ParseNode>();
			foreach (ParseNode parseNode in children) {
				if (parseNode.data == s) {
					outout.Add(parseNode);
				}
				outout.AddRange(parseNode.GetAllOfType(s));
			}

			return outout;
		}


		public bool Equals(string s) {
			return Data == s;
		}

		public void CleanUp() {
			Regex grammarRule = new Regex("<\\w*>");
			for (int i = children.Count - 1; i >= 0; i--) {
				if(String.IsNullOrEmpty(children[i].Data)) children.RemoveAt(i);
				else if(grammarRule.IsMatch(children[i].data) && children[i].children.Count == 0) children.RemoveAt(i);
				else if(grammarRule.IsMatch(children[i].data)){
					switch (children[i].data) {
						case "<sentence>": {
							ParseNode next = new ParseNode(Parser.ConvertSentence(children[i]));
							children[i].children = new List<ParseNode> {next};
						}
							break;
						case "<string>": {
							ParseNode next;
							if (children[i].children[0].data == "<sentence>") {
								next= new ParseNode(Parser.ConvertSentence(children[i].children[0]));
							} else next= new ParseNode(Parser.ConvertString(children[i]));
							children[i].children = new List<ParseNode> {next};
						}
							break;
						case "<int>": {
							ParseNode next = new ParseNode(Parser.ConvertInt(children[i]));
							children[i].children = new List<ParseNode> {next};
						}
							break;
					}
					
					children[i].CleanUp();
				}else{
					children[i].CleanUp();
				}
			}
			
		}

	
		
		public int Count() {
			int count = 1;
			foreach (ParseNode parseNode in children) {
				count += parseNode.Count();
			}

			return count;
		}

		public void Convert(ParseNodeConverter func) {
			convertedValue = func(this);
		}

		public bool Contains(string s) {
			return this[s] != null;
		}

		/// <summary>
		/// To String method
		/// </summary>
		/// <returns>the data</returns>
		public override string ToString() {
			return data;
		}

		public void Print(int indent) {
			for (int i = 0; i < indent; i++) {
				Console.Write("   ");
			}
			Console.Write(this + "\n");
			foreach (ParseNode parseNode in children) {
				parseNode.Print(indent+1);
			}
		}

		public void Print(int indent, int maxDepth) {
			if (indent == maxDepth) return;
			for (int i = 0; i < indent; i++) {
				Console.Write("   ");
			}
			Console.Write(this + "\n");
			foreach (ParseNode parseNode in children) {
				parseNode.Print(indent+1,maxDepth);
			}
		}
	}
}