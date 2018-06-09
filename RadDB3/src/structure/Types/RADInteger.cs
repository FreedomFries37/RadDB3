

namespace RadDB3.structure.Types {
	public class RADInteger : Element, EnableMath{
		public RADInteger(int i) {
			Data = i;
		}

		public double ToDouble() {
			return (double) Data;
		}

		public int ToInt() {
			return Data;
		}
		
		public static RADInteger operator+(RADInteger i1, RADInteger i2) => 
			new RADInteger(i1.ToInt() + i2.ToInt());
	}
}