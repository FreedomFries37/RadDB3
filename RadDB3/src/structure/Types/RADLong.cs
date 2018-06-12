namespace RadDB3.structure.Types {
	public class RADLong : Element{
		public RADLong(long data) : base(data) { }
		public RADLong(string s) : base(s) { }
		
		public override void ChangeData() {
			Data = long.Parse(Data);
		}
	}
	
	public class RADuLong : Element{
		public RADuLong(ulong data) : base(data) { }
		public RADuLong(string s) : base(s) { }
		
		public override void ChangeData() {
			Data = ulong.Parse(Data);
		}
	}
}