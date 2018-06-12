using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using RadDB3.interaction;
using RadDB3.scripting;
using RadDB3.scripting.parsers;
using RadDB3.structure;
using RadDB3.structure.Types;

namespace RadDB3 {

	public delegate bool ParserFunction(out ParseNode node);
	
	class Program {
		static void Main(string[] args) {
			Database db = new Database("Your Mom");
			Relation r = new Relation(("*Name", typeof(RADString)), ("Age", typeof(RADInteger)), ("Alive", typeof(RADBool)));
			//RADTuple t = new RADTuple(r, new RADString("Josh"), new RADInteger(32), new RADGeneric<bool>(false));
			
			Table tb = new Table(r);

			
			tb.Add("Dan", 14, true);
			tb.Add("Radc", 67, true);
			
			tb.Add("Eli", 16, true);
			
			tb.Add("Jake", 252, true);
			tb.Add("Steve", 12, true);
			tb.Add("Max", 44, true);
			tb.Add("David", 55, true);
			
			
			tb.PrintTable();
			tb.DumpData();
			Console.WriteLine(tb.Find(("Name", new RADString("Dan")),
				("Age", new RADInteger(14)), ("Alive", new RADBool(true))));
			Console.WriteLine(tb.Find(("Name", "Dan"), ("Age", "14"), ("Alive", "true")));
			
			
			Parser p = new Parser("\"yolo\"", Parser.ReadOptions.STRING);
			ParseTree pt = new ParseTree(p.ParseSentence);
			
			FileInteraction.ConvertTableToFile(tb);
			FileInteraction.ConvertFileToTable("Name-Age-Alive.rdt");
		}
	}
}