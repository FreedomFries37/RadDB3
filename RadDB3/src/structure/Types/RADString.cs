namespace RadDB3.structure.Types {
	public class RADString : Element {
		
		public RADString(string data) {
			Data = data;
		}

		// Constrainment
		public RADString(string str, int maxSize) {
			if (str.Length > maxSize) str = str.Substring(0, maxSize);
			Data = str;
		}
	}
}