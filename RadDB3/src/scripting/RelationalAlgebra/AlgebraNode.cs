using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RadDB3.structure;

namespace RadDB3.scripting.RelationalAlgebra {
	public class AlgebraNode {

		private RelationalAlgebraFunction function;
		private LinkedList<AlgebraNode> children;
		private readonly bool _isBase;
		public string[] Options { get; set; }
		public Table BaseTable { get; protected set; }
		public int Tuples { get; protected set; }

		public AlgebraNode[] Children => children.ToArray();
		public AlgebraNode Parent { private set; get; }

		public bool IsBase => _isBase;

		public RelationalAlgebraFunction Function {
			get => function;
			private set => function = value;
		}

		public AlgebraNode(Table tb, params string[] options) {
			_isBase = true;
			BaseTable = tb;
			children = new LinkedList<AlgebraNode>();
			Function = RelationalAlgebraModule.Reflect;
			Options = options;
			Tuples = tb.Count;
		}

		public AlgebraNode(RelationalAlgebraFunction func, string[] options, AlgebraNode first, params AlgebraNode[] children) {
			_isBase = false;
			Function = func;
			Options = options;
			this.children = new LinkedList<AlgebraNode>(children);
			this.children.AddFirst(first);
			foreach (AlgebraNode algebraNode in children) {
				algebraNode.Parent = this;
			}
		}

		public RADTuple[] Apply() {
			if (_isBase) return Function(Options, this);
			RADTuple[] output = Function(Options, Children);
			Tuples = output.Length;
			return output;
		}

		public Table TableApply() {

			RADTuple[] tuples = Apply();
			if (tuples.Length == 0) return null;
			Table output = new Table(tuples);
			output.ReSize();
			if (Function == RelationalAlgebraModule.Reflect) {
				output.CreateSecondaryIndexing();
			}

			return output;
		}

		public int Count() {
			int count = 1;
			foreach (AlgebraNode parseNode in children) {
				count += parseNode.Count();
			}

			return count;
		}

		public void SetChildren(params AlgebraNode[] newChildren) {
			this.children = new LinkedList<AlgebraNode>(newChildren);
			foreach (AlgebraNode algebraNode in children) {
				algebraNode.Parent = this;
			}
		}

		public void ReplaceChild(AlgebraNode child, AlgebraNode newChild) {
			if (children.Find(child) != null) {
				children.Find(child).Value = newChild;
			}
		}

		public void AddOption(string s) {
			List<string> sss = new List<string>(Options);
			sss.Add(s);
			Options = sss.ToArray();
		}

		public List<AlgebraNode> FindAll(RelationalAlgebraFunction f) {
			var output = new List<AlgebraNode>();
			if(Function == f) output.Add(this);
			foreach (AlgebraNode algebraNode in children) {
				output.AddRange(FindAll(f));
			}

			return output;
		}

		/// <summary>
		/// Finds all tables that are children of this node, and their corresponding algebra nodes
		/// </summary>
		/// <returns>The list of tables</returns>
		public List<(Table table, AlgebraNode node)> FindAllTables() {
			if(_isBase) return new List<(Table table, AlgebraNode node)>{(BaseTable, this)};
			
			var output = new List<(Table table, AlgebraNode node)>();
			foreach (AlgebraNode algebraNode in children) {
				output.AddRange(algebraNode.FindAllTables());
			}

			return output;
		}

		public void EnforceParenthood() {
			foreach (AlgebraNode algebraNode in children) {
				algebraNode.Parent = this;
				algebraNode.EnforceParenthood();
			}
		}

		public void PrintTree() {
			PrintTree(0);
			Console.WriteLine();
		}
		private void PrintTree(int index) {
			for (int i = 0; i < index; i++) {
				Console.Write("   ");
			}

			if (function != RelationalAlgebraModule.Reflect) {
				for (int i = 0; i < Options.Length - 1; i++) {
					Console.Write(Options[i] + ",");
				}

				Console.WriteLine(Options[Options.Length - 1] + "  Tuples: " + Tuples);
			} else {
				string optionDetail = Options.Length > 0 ? $"[{Options[0]}] " : "";
				Console.WriteLine(optionDetail + BaseTable.Name + "  Tuples: " + Tuples);
			}

			foreach (AlgebraNode algebraNode in children) {
				algebraNode.PrintTree(index+1);
			}
		}

		public void PrintTreeMaxDepth(int maxDepth) {
			PrintTree(0,maxDepth);
		}


		private void PrintTree(int indent, int maxDepth) {
			if (indent == maxDepth) return;
			for (int i = 0; i < indent; i++) {
				Console.Write("   ");
			}

			if (function != RelationalAlgebraModule.Reflect) {
				for (int i = 0; i < Options.Length - 1; i++) {
					Console.Write(Options[i] + ",");
				}

				Console.WriteLine(Options[Options.Length - 1] + "  Tuples: " + Tuples);
			} else {
				string optionDetail = Options.Length > 0 ? $"[{Options[0]}] " : "";
				Console.WriteLine(optionDetail + BaseTable.Name + "  Tuples: " + Tuples);
			}

			foreach (AlgebraNode algebraNode in children) {
				algebraNode.PrintTree(indent+1, maxDepth);
			}
		}
	}
}