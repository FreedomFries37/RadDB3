using System.Security.Cryptography.X509Certificates;
using RadDB3.structure;

namespace RadDB3.interaction {
	
	
	public static partial class Commands {

		public class CommandObject {
			public RADObject Data { set; get; }

			public CommandObject(RADObject data) {
				Data = data;
			}

			public virtual void SetName(string s) { }

			public virtual string Dump() => Data?.Dump();
		}
		
		public static Table SelectTable(Database db, string s) => SelectTableCommand(db, s).Data as Table;
		public static CommandTable SelectTableCommand(Database db, string s) {
			return new CommandTable(db[s]);
		}

		public static RADTuple[] List(Table t) => t.All;
		public static CommandTupleList ListCommand(CommandTable t) => new CommandTupleList(t);

	}
}