namespace RadDB3.structure.Types {
	public class RADBool : Element{
		
		public RADBool(bool b) {
			Data = b;
		}

		public static RADBool operator ==(RADBool a, RADBool b) =>  new RADBool(a.Data == b.Data);
		public static RADBool operator !=(RADBool a, RADBool b) => new RADBool(a.Data != b.Data);
	}
}