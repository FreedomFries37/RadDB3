using System;

namespace RadDB3.structure.Types {
	public class RADDateTime : Element{
		
		public RADDateTime(DateTime data) : base(data) { }
		public RADDateTime(string s) : base(s) { }

		public override void ChangeData() {
			Data = DateTime.Parse(Data);
		}
	}
}