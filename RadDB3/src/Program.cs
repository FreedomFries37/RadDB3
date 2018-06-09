using System;
using System.Runtime.InteropServices;
using RadDB3.structure;
using RadDB3.structure.Types;

namespace RadDB3 {
	class Program {
		static void Main(string[] args) {
			Database db = new Database("Your Mom");
			Relation r = new Relation(("Name", typeof(RADString)), ("Age", typeof(RADInteger)), ("Alive", typeof(bool)));
			//RADTuple t = new RADTuple(r, new RADString("Josh"), new RADInteger(32), new RADGeneric<bool>(false));
			RADTuple t = RADTuple.CreateFromObjects(r, "Josh", 32, false);
			Table tb = new Table(r);

			tb.Add(t);
			tb.PrintTable();
		}
	}
}