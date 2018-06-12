using System;
using RadDB3.scripting;

namespace RadDB3.structure {
	public class NameTypePair {
		private string name;
		private Type type;

		public NameTypePair(string name, Type type) {
			this.name = name;
			this.type = type;
		}

		public NameTypePair(ParseNode p){
			if(p.Data != "<column_details>") return;
			string keyInfo = p["<key_info>"][0].Data;
			if (keyInfo == "-") keyInfo = "";
			name = keyInfo + p["<sentence>"][0].Data;
			type = Type.GetType(p["<string>"][0].Data);
		}

		public string Name => name;

		public Type Type => type;
	}
}