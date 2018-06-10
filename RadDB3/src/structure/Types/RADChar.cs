namespace RadDB3.structure.Types {
	public class RADChar : Element{
		public RADChar(char c) : base(c) { }
		public RADChar(string s) : base(s) { }

		public override void ChangeData() {
			Data = char.Parse(Data);
		}
	}
}