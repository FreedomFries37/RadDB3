namespace RadDB3.structure.Types {
	public class RADDouble : Element, EnableMath {
		public RADDouble(double d) :base(d) { }
		public RADDouble(string s) : base(s) { }

		public override void ChangeData() {
			Data = double.Parse(Data);
		}

		public double ToDouble() {
			return Data;
		}

		public int ToInt() {
			return (int) Data;
		}
	}
}