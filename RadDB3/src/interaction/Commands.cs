using RadDB3.structure;

namespace RadDB3.interaction {
	
	
	public static partial class Commands {
		
		public static Table SelectTable(Database db, string s) {
			return db[s];
		}
		
	}
}