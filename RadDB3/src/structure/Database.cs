namespace RadDB3.structure {
	public partial class Database {
		private string name;

		public Database(string name) {
			this.name = name;
			initiateTables();
		}
		
		partial void initiateTables();
	}
}