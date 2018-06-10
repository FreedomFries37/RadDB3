namespace RadDB3.structure.Types {
	public class RADBool : Element{
		
		public RADBool(bool b) : base(b) { }
		public RADBool(string b) : base(b) { }

		public override void ChangeData() {
			Data = bool.Parse(Data);
		}

		public static RADBool operator ==(RADBool a, RADBool b) =>  new RADBool(a.Data == b.Data);
		public static RADBool operator !=(RADBool a, RADBool b) => new RADBool(a.Data != b.Data);
	}
}