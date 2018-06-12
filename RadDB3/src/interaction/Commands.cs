using RadDB3.structure;

namespace RadDB3.interaction {
	public static class Commands {

		/**
		 * Basic Commands:
		 * 		select [<options>] <selection_parameters>
		 * 		create <name> [database|table] (?<=table)[<selection_parameters>]
		 * 		delete [database:]<name>
		 *
		 * JOINS:
		 * 		left join: <=
		 * 		right join: =>
		 * 		inner join: =
		 * 		full join: <>
		 *
		 * Selection Parameters:
		 * 
		 *	EXAMPLE: "Name","DoB"{(NA("Name")<=NDB("name"))"Name"="Josh"}
		 *
		 *
		 * 
		 *	<name>:
		 * 		<sentence>
		 * 		<string>
		 *
		 * 	<selection>
		 * 		<projection_list>{
		 */
		
		public static Table SelectTable(Database db, string s) {
			return db[s];
		}
		
	}
}