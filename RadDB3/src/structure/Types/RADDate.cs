using System;
using System.Globalization;

namespace RadDB3.structure.Types {
	public class RADDate : RADDateTime{
		
		public RADDate(DateTime data) : base(data) { }
		public RADDate(string s) : base(s) { }

		public override void ChangeData() {
			Data = DateTime.Parse(Data);
		}

		public override string ToString() {
			return ((DateTime) Data).Date.ToString(CultureInfo.CurrentCulture).Split(" ")[0];
		}
	}
}