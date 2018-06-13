using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RadDB3.structure {
	public partial class Database : IEnumerable<Table> {
		private string name;
		public string Name => name;

		public Database(string name, int size = DEFAULT_SIZE) {
			this.name = name;
			initiateTables(size);
		}
		
		partial void initiateTables(int size);

		public override string ToString() {
			return name;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IEnumerator<Table> GetEnumerator() {
			List<Table> list = tables.ToList();
			list.RemoveAll(item => item == null);
			return list.GetEnumerator();
		}

		public Table this[int i] => i >= 0 && i < tables.Length ? tables[i] : null;

		public Table this[string s] {
			get {
				foreach (Table table in this) {
					if (table.Name == s) return table;
				}

				return null;
			}
		}

		public void PrintDataBase() {
			foreach (Table table in this) {
				table.PrintTable(25);
				Console.WriteLine();
			}
		}

		public void DumpDataBase() {
			foreach (Table table in this) {
				Console.WriteLine(table.Name);
				table.DumpData();
				Console.WriteLine();
			}
		}
	}
}