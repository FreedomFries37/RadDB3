

using System;

namespace RadDB3.structure {

	
	public partial class Database {
		private const int DEFAULT_SIZE = 100;
		private Table[] tables;

		private int count;

		public int Count => count;
		public int Size => tables.Length;

		partial void initiateTables(int size) {
			tables = new Table[size];
			count = 0;
		}

		public bool addTable(Table t) {
			if (Count == Size) Expand();
			int originalPosition, position;
			position = originalPosition = Math.Abs(t.GetHashCode() % Size);
			
			while (tables[position] != null) {
				++position;
				if (position == Size) position = 0;
				if (position == originalPosition) return false;
			}

			tables[position] = t;
			count++;
			return true;
		}
		

		public bool removeTable(Table t) {
			int originalPosition, position;
			position = originalPosition = t.GetHashCode() % Size;
			
			while (tables[position] != t) {
				++position;
				if (position == Size) position = 0;
				if (position == originalPosition) return false;
			}

			tables[position] = null;
			count--;
			return true;
		}

		private void Expand() {
			Table[] newTables = new Table[Size*2];
			foreach (Table table in tables) {
				int originalPosition, position;
				position = originalPosition = Math.Abs(table.GetHashCode() % Size);
				
				while (newTables[position] != null) {
					++position;
					if (position == Size) position = 0;
					if (position == originalPosition) return;
				}

				newTables[position] = table;
			}

			tables = newTables;
		}
	}

}