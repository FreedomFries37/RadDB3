using System;

namespace RadDB3 {
	public class Misc {

		// from 0 to 1
		public static string percentToLetterGrade(double percent) {
			string output = "";
			if (percent >= .9) output += "A";
			else if (percent >= .8) output += "B";
			else if (percent >= .7) output += "C";
			else if (percent >= .6) output += "D";
			else output += "F";

			if (percent >= .60) {
				bool approx0 =  ApproximateEqual(percent%.1, 0, .00001d) || ApproximateEqual(percent%.1, .1, .00001d);
				if ((percent+.001) % .1 >= .07 && !approx0 || ApproximateEqual(percent, 1, .00001d)) output += "+";
				else if ((percent+.001) % .1 < .03 || approx0) output += "-";
			}

			return output;
		}

		// from 0 to 100l
		public static string percentToLetterGrade(int percent) {
			return percentToLetterGrade(percent / 100d);
		}

		public static bool ApproximateEqual(double a, double b, double leniency) {
			return Math.Abs(a - b) <= leniency;
		}
		
	}
}