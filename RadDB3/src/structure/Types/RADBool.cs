namespace RadDB3.structure.Types {
	public class RADBool : Element{
		
		public RADBool(bool b) {
			Data = b;
		}

		public override void ChangeData() {
			if (Data.GetType() != typeof(string)) return;
			Data = bool.Parse(Data);
		}

		public static RADBool operator ==(RADBool a, RADBool b) =>  new RADBool(a.Data == b.Data);
		public static RADBool operator !=(RADBool a, RADBool b) => new RADBool(a.Data != b.Data);
	}
}