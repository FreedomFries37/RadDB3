using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using RadDB3.scripting.parsers;
using RadDB3.structure;

namespace RadDB3.scripting.RelationalAlgebra {
	public class AlgebraNodeTree {
		private AlgebraNode head;

		public AlgebraNodeTree(Database db, string[] projections, string[] selections, string joinFullInfo) {
			Parser joinParser = new Parser(joinFullInfo, Parser.ReadOptions.STRING);
			ParseTree joinInfoTree = new ParseTree(joinParser.ParseJoinInfoFull);
			joinInfoTree.PrintTree();
			List<ParseNode> tableNames = joinInfoTree.GetAllOfType("<table_name>");
			Table[] tables = new Table[tableNames.Count];

			int index = 0;
			foreach (ParseNode tableName in tableNames) {
				string name = tableName[0][0].Data;
				tables[index] = db[name];
				if (tables[index] == null) return;
				index++;
			}

			Dictionary<string, AlgebraNode> nameToAlgebraNode = new Dictionary<string, AlgebraNode>();
			foreach (Table table in tables) {
				nameToAlgebraNode.Add(table.Name, new AlgebraNode(table));
			}
			
			AlgebraNode fixedJoins = ConvertJoinInfoFullIntoMultiplesJoinInfoString(joinInfoTree.Head,nameToAlgebraNode);
			head = fixedJoins;

			if (selections.Length > 0) {
				
			}

			if (projections.Length > 0) {
				
			}
		}

		public RADTuple[] Apply() => head.Apply();
		public Table TableAply() => head.TableApply();

		private AlgebraNode ConvertJoinInfoFullIntoMultiplesJoinInfoString(ParseNode p, Dictionary<string, AlgebraNode> nameToAlgebraNode) {
			AlgebraNode left, right;
			string leftName, rightName, leftColumns, rightColumns;
			if(p[0].Contains("<table_name>")) {
				leftName = p[0]["<table_name>"][0][0].Data;
				left = nameToAlgebraNode[leftName];
			} else {
				leftName = "(...)";
				left = ConvertJoinInfoFullIntoMultiplesJoinInfoString(p[0]["<join_info_full>"], nameToAlgebraNode);
			}
			
			if(p[2].Contains("<table_name>")) {
				rightName = p[2]["<table_name>"][0][0].Data;
				right = nameToAlgebraNode[rightName];
			} else {
				rightName = "(...)";
				right = ConvertJoinInfoFullIntoMultiplesJoinInfoString(p[2]["<join_info_full>"], nameToAlgebraNode);
			}

			leftColumns = Parser.ConvertColumnsOneString(p[0]["<columns>"]);
			rightColumns = Parser.ConvertColumnsOneString(p[2]["<columns>"]);

			string joinType = p["<join_type>"][0].Data;

			string joinCommand = $"{leftName}({leftColumns}){joinType}{rightName}({rightColumns})";

			RelationalAlgebraFunction joinFunction = RelationalAlgebraModule.InnerJoin;
			switch (joinType) {
					case "=":
						joinFunction = RelationalAlgebraModule.InnerJoin;
						break;
					case "<":

						break;
			}

			return new AlgebraNode(joinFunction, new []{joinCommand}, left, right);
		}
	}
}
