using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using RadDB3.scripting;
using RadDB3.structure.Types;

namespace RadDB3.structure {
	public class RADTuple : RADObject, IEnumerable<(string name,Element e)> {
		private Relation _relation;
		public Relation relation => _relation;
		public readonly Element[] elements;
		public readonly Type[] subTypes; //in order

		public const string ELEMENT_SEPERATOR = " $& ";

		public RADTuple(Relation r, params Element[] objects) {
			_relation = r;
			elements = objects;
			
		}

		public RADTuple(Relation r, RADTuple t) {
			_relation = r;
			Element[] objects = new Element[r.Arity];
			foreach ((string name, Element e) valueTuple in t) {
				(string name, Element e) = valueTuple;
				if (r.IsKey(name) >= 0) {
					objects[r[name]] = e;
				}
			}

			elements = objects;
		}

		public Element this[int i] => elements[i];

		public Element this[string str] {
			set {
				int index = _relation.Names.ToList().IndexOf(str);
				if (index < 0 ||
					index >= elements.Length) return;
				elements[index] = value;
			}
			get {
				int index = _relation.Names.ToList().IndexOf(str);
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
			return Convert.ChangeType(elements[index], _relation.Types[index]);
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

			value = Convert.ChangeType(elements[index], _relation.Types[index]);
			return true;
		}

		public Element[] GetKeyElements() {
			Element[] output = new Element[_relation.Arity];
			int index = 0;
			foreach (int keyIndex in _relation.Keys) {
				output[index++] = elements[keyIndex];
			}

			return output;
		}

		public bool AttemptSwitchRelation(Relation r) {
			if (r.Arity != relation.Arity) return false;
			for (int i = 0; i < r.Arity; i++) {
				if (r.Types[i] != relation.Types[i]) return false;
			}

			_relation = r;
			return true;
		}

		public override int GetHashCode() {
			return elements[_relation.Keys[0]].GetHashCode();
		}

		public override string ToString() {
			string output = "{";

			for (int i = 0; i < _relation.Arity-1; i++) {
				output += _relation.Names[i] + ":" + elements[i] + ",";
			}

			output += _relation.Names[_relation.Arity - 1] + ":" + elements[_relation.Arity - 1];

			return output + "}";
		}

		public string DetailedDump() {
			string output = "{";

			for (int i = 0; i < _relation.Arity-1; i++) {
				if (i == _relation.Keys[0]) output += "*";
				else if (_relation.Keys.Contains(i)) output += "&";
				output += _relation.Names[i] + "<" + _relation.Types[i].Name + ">" + ":" + elements[i] + ", ";
			}

			output += _relation.Names[_relation.Arity - 1] + ":" + elements[_relation.Arity - 1];

			return output + "}";
		}

		public string LessDetailedDump() {
			string output = "{";

			for (int i = 0; i < _relation.Arity-1; i++) {
				output += elements[i] + ELEMENT_SEPERATOR;
			}

			output += elements[_relation.Arity - 1];

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

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IEnumerator<(string name, Element e)> GetEnumerator() {
			List<(string name, Element e)> output = new List<(string name, Element e)>();
			for (int i = 0; i < _relation.Arity; i++) {
				output.Add((_relation.Names[i], elements[i]));
			}

			return output.GetEnumerator();
		}
	}
	
}