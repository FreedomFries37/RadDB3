using System;

namespace RadDB3.structure {
	public class NameTypePair {
		private string name;
		private Type type;

		public NameTypePair(string name, Type type) {
			this.name = name;
			this.type = type;
		}

		public string Name => name;

		public Type Type => type;
	}
}