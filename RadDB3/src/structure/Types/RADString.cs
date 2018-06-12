namespace RadDB3.structure.Types {
	public class RADString : Element {
		
		public RADString(string data) : base(data){ }

		private int maxSize = 30;

		// Constrainment
		public RADString(string str, int maxSize) :base(str) {
			if (str.Length > maxSize) str = str.Substring(0, maxSize);
			Data = str;
			Contraints = $"MAXSIZE({maxSize})";
			this.maxSize = maxSize;
		}

		// Don't do anything, data should be a string
		public override void ChangeData() { }
	}
}