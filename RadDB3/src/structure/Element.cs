using System;

namespace RadDB3.structure {
	public abstract class Element{
		private dynamic data;
		
		public dynamic Data {
			get => data;
			protected set => data = value;
		}

		public override int GetHashCode() {
			return ToString().GetHashCode();
		}

		public override string ToString() {
			return data.ToString();
		}
	}
}