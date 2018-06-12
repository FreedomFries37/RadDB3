using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using RadDB3.scripting;
using RadDB3.structure.Types;

namespace RadDB3.structure {
	public class RADTuple {
		public readonly Relation relation;
		public readonly Element[] elements;
		public readonly Type[] subTypes; //in order

		public const string ELEMENT_SEPERATOR = " $& ";

		public RADTuple(Relation r, params Element[] objects) {
			relation = r;
			elements = objects;
			
		}

		public Element this[int i] => elements[i];

		public Element this[string str] {
			set {
				int index = relation.Names.ToList().IndexOf(str);
				if (index < 0 ||
					index >= elements.Length) return;
				elements[index] = value;
			}
			get {
				int index = relation.Names.ToList().IndexOf(str);
				if (index < 0 ||
					index >= elements.Length) return null;
				return this[index];
			}
		}

		/// <summary>
		/// Returns data of an element
		/// </summary>
		/// <param name="index">the index</param>
		/// <returns>The data as a dynamic value</returns>
		public dynamic getValue(int index) {
			if (index >= elements.Length ||
				index < 0) throw new IndexOutOfRangeException();
			return Convert.ChangeType(elements[index], relation.Types[index]);
		}

		/// <summary>
		/// Returns data if its within range
		/// </summary>
		/// <param name="index">the index</param>
		/// <param name="value">dynamic value to assign to</param>
		/// <returns>if value was set to a non null reference</returns>
		public bool getValue(int index, out dynamic value) {
			value = null;
			if (index >= elements.Length ||
				index < 0 ||
				elements[index] == null) return false;

			value = Convert.ChangeType(elements[index], relation.Types[index]);
			return true;
		}

		public Element[] GetKeyElements() {
			Element[] output = new Element[relation.Arity];
			int index = 0;
			foreach (int keyIndex in relation.Keys) {
				output[index++] = elements[keyIndex];
			}

			return output;
		}


		public override int GetHashCode() {
			return elements[relation.Keys[0]].GetHashCode();
		}

		public override string ToString() {
			string output = "{";

			for (int i = 0; i < relation.Arity-1; i++) {
				output += relation.Names[i] + ":" + elements[i] + ",";
			}

			output += relation.Names[relation.Arity - 1] + ":" + elements[relation.Arity - 1];

			return output + "}";
		}

		public string DetailedDump() {
			string output = "{";

			for (int i = 0; i < relation.Arity-1; i++) {
				if (i == relation.Keys[0]) output += "*";
				else if (relation.Keys.Contains(i)) output += "&";
				output += relation.Names[i] + "<" + relation.Types[i].Name + ">" + ":" + elements[i] + ", ";
			}

			output += relation.Names[relation.Arity - 1] + ":" + elements[relation.Arity - 1];

			return output + "}";
		}

		public string LessDetailedDump() {
			string output = "{";

			for (int i = 0; i < relation.Arity-1; i++) {
				output += elements[i] + ELEMENT_SEPERATOR;
			}

			output += elements[relation.Arity - 1];

			return output + "}";
		}

		public string Dump(int level) {
			switch (level) {
					case 0:
						return LessDetailedDump();
					case 1:
						return ToString();
					case 2:
						return DetailedDump();
			}

			return null;
		}

		public string Dump(DumpLevel dumpLevel) {
			switch (dumpLevel) {
				case DumpLevel.LOW:
					return LessDetailedDump();
				case DumpLevel.NORMAL:
					return ToString();
				case DumpLevel.HIGH:
					return DetailedDump();
			}

			return null;
		}
		
		public static RADTuple CreateFromObjects(Relation r, params object[] o) {
			if (o.Length != r.Arity) return null;
			
			Element[] elements = new Element[r.Arity];
			for (int i = 0; i < elements.Length; i++) {
				
				//elements[i] = (Element) r.Types.ElementAt(i).GetTypeInfo().DeclaredConstructors.ElementAt(0).Invoke(new[] {o[i]});
				elements[i] = Element.ConvertToElement(r.Types[i], o[i]);
			}
			
			return new RADTuple(r, elements);
		}
		
		public static RADTuple CreateFromParseNode(Relation r, ParseNode parseNode) {
			if(parseNode.Data != "<tuple>") throw new IncompatableParseNodeException();
			string data = parseNode[0].Data;
			data = data.Substring(1, data.Length - 2);

			string[] stringElements = data.Split(ELEMENT_SEPERATOR);
			Element[] elements = new Element[r.Arity];
			for (int i = 0; i < elements.Length; i++) {
				elements[i] = Element.ConvertToElement(r.Types[i], stringElements[i]);
			}
			return new RADTuple(r, elements);
		}
	}
	
}