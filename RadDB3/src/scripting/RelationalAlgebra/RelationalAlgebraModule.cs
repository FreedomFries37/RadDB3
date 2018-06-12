using System.Linq;
using System.Net;
using RadDB3.structure;

namespace RadDB3.scripting.RelationalAlgebra {
	public delegate RADTuple[] RelationalAlgebraFunction(string[] options, params AlgebraNode[] nodes);
	public static class RelationalAlgebraModule {

		public static RADTuple[] Reflect(string[] options, params AlgebraNode[] nodes) {
			return nodes[0].BaseTable.All;
		}

		//Node count should be 1
		public static RADTuple[] Selection(string[] options, params AlgebraNode[] nodes) {
			Table choice = nodes[0].TableApply();
			
			(string name, string data)[] columnDataTuples = new (string, string)[options.Length];
			for (int i = 0; i < options.Length; i++) {
				columnDataTuples[i] = (options[i].Split("=")[0], options[i].Split("=")[1]);
			}
			
			// SPLIT IF PRIMARY KEY IS AVAILABLE

			return choice.All; // TEMPORARY
		}

		
	}
}