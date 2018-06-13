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

		public AlgebraNode[] Children => children.ToArray();

		public bool IsBase => _isBase;

		public RelationalAlgebraFunction Function {
			get => function;
			set => function = value;
		}

	

		public AlgebraNode(Table tb, params string[] options) {
			_isBase = true;
			BaseTable = tb;
			children = new LinkedList<AlgebraNode>();
			function = RelationalAlgebraModule.Reflect;
			Options = options;
		}

		public AlgebraNode(RelationalAlgebraFunction func, string[] options, AlgebraNode first, params AlgebraNode[] children) {
			_isBase = false;
			function = func;
			Options = options;
			this.children = new LinkedList<AlgebraNode>(children);
			this.children.AddFirst(first);
		}

		public RADTuple[] Apply() {
			if (_isBase) return function(Options, this);
			return function(Options, Children);
		}

		public Table TableApply() {

			RADTuple[] tuples = Apply();
			if (tuples.Length == 0) return null;
			Table output = new Table(tuples);

			if (function == RelationalAlgebraModule.Reflect) {
				output.CreateSecondaryIndexing();
			}

			return output;
		}
	}
}