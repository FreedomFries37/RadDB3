using System;
using System.Runtime.CompilerServices;
using RadDB3.structure;

namespace RadDB3.interaction {
	public partial class Commands{

		public class CommandTable : CommandObject {
			public CommandTable(Table data) : base(data) { }

			public RADTuple Add(RADTuple t) {
				if ((Data as Table)?.Add(t) ?? false) {
					return null;
				}

				return t;
			}

			public override void SetName(string s) {
				(Data as Table)?.SetName(s);
			}
		}

		public class RADTupleList : RADObject {
			public RADTuple[] List { private set; get; }

			public RADTupleList(RADTuple[] list) {
				List = list;
			}

			public RADTupleList(Table t) : this(t.All) { }
			public RADTupleList(CommandTable t) : this(t.Data as Table) { }
		}

		public class CommandTupleList : CommandObject {
			public CommandTupleList(RADTupleList data) : base(data) { }
			public CommandTupleList(CommandTable data) : this(new RADTupleList(data)) { }
		}

		public class CommandRADTuple : CommandObject {
			public CommandRADTuple(RADTuple data) : base(data) { }
		}

		public class CommandElement : CommandObject {
			public CommandElement(Element data) : base(data) { }
		}
	}
}