

namespace RadDB3.structure {

	
	public partial class Database {
		private const int MAX_SIZE = 100;
		private Table[] tables;

		private int count;

		public int Count => count;

		partial void initiateTables() {
			tables = new Table[MAX_SIZE];
			count = 0;
		}

		public bool addTable(Table t) {
			if (Count == MAX_SIZE) return false;
			int originalPosition, position;
			position = originalPosition = t.GetHashCode() % MAX_SIZE;
			
			while (tables[position] != null) {
				++position;
				if (position == MAX_SIZE) position = 0;
				if (position == originalPosition) return false;
			}

			tables[position] = t;
			count++;
			return true;
		}

		public bool removeTable(Table t) {
			int originalPosition, position;
			position = originalPosition = t.GetHashCode() % MAX_SIZE;
			
			while (tables[position] != t) {
				++position;
				if (position == MAX_SIZE) position = 0;
				if (position == originalPosition) return false;
			}

			tables[position] = null;
			count--;
			return true;
		}
	}

}