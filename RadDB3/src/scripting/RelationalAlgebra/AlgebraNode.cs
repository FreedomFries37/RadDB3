using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RadDB3.structure;

namespace RadDB3.scripting.RelationalAlgebra {
	public class AlgebraNode {

		private RelationalAlgebraFunction function;
		private LinkedList<AlgebraNode> children;
		private readonly bool _isBase;
		public Table BaseTable { get; protected set; }

		public AlgebraNode[] Children => children.ToArray();

		public bool IsBase => _isBase;

		public RelationalAlgebraFunction Function {
			get => function;
			set => function = value;
		}

	

		public AlgebraNode(Table tb) {
			_isBase = true;
			BaseTable = tb;
			children = new LinkedList<AlgebraNode>();
			function = RelationalAlgebraModule.Reflect;
		}

		public AlgebraNode(RelationalAlgebraFunction func, string[] options, AlgebraNode first, params AlgebraNode[] children) {
			_isBase = false;
			function = func;
			this.children = new LinkedList<AlgebraNode>(children);
			this.children.AddFirst(first);
		}

		public RADTuple[] Apply(params string[] options) {
			if (_isBase) return function(options, this);
			return function(options, Children);
		}
		
		public Table TableApply(params string[] options) => new Table(Apply(options));
	}
}