using System;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RadDB3.structure.Types {
	public class RADFile : Element{

		public String path { private set; get; }

		
		
		public RADFile(FileInfo f) : base(f) {
			path = f.FullName;
		}

		public RADFile(string s) : this(new FileInfo(fixPath(s))) {
			Console.WriteLine(path);
		}
		


		public override void ChangeData() {
			path = Data;
			Data = new FileInfo(Data);
		}

		public void OpenFile() {
			if (File.Exists(path)) {
				Process p = new Process();
				p.StartInfo = new ProcessStartInfo(path) {
					UseShellExecute = true
				};
				p.Start();
			}
	}

		private static String fixPath(String s) {
			return s.Replace("./", Environment.CurrentDirectory + "/");
		}
	}
}