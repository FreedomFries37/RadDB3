using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace RadDB3.scripting {
	public class ParseNode {

		private string data;
		private List<ParseNode> children;

		public string Data {
			private set => data = value;
			get => data;
		}

		public ParseNode[] Children => children.ToArray();

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

		public void CleanUp() {
			for (int i = children.Count - 1; i >= 0; i--) {
				if(String.IsNullOrEmpty(children[i].Data)) children.RemoveAt(i);
				else children[i].CleanUp();
			}
		}
	}
}