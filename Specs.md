Commands can be converted into objects of four types: Element, RADTuple, RADTuple[] (as TupleList), or Table
Runtime variables can be created to store the types
Runtime tempvars are cleared after every command
Objects can be piped through commands

PRIVATE TYPES:
	<new_table> : <table>
	<nametypepair>:
		(<sentence>:<string>[(constaints)])

OBJECT CREATION:
	<object>:
		<Table>
		<TupleList>
		<RADTuple>
		<Element>
		(<object>) #objects value is whatever type it stores, so (<Table>) is also a Table
		<object>.<method>
		
		Fields:
			dynamic data;
		
		Methods:
			void Dump()
		
	<Table>: 
		<string:name>
		<command>
		<new_table>:
			new Table(<nametypepair>,...) #emulates the internal constructor
		
		Fields:
			Relation relation;
		
		Methods:
			RADTuple Add(<RADTuple>) 
			RADTuple Remove(<RADTuple>)
			
	<TupleList>:
		list(<Table>)
	
	
	<RADTuple>:
		private <Element>,... #relation gets automatically piped by Table
		<Table>[<sentence:primary key>]
		<TupleList>[<sentence:primary key>]
	<Element>:
		[<RADtype> ]<sentence>|<string> # defaults to string RADtype
		<RADTuple>[<sentence:column name>]
		<RADTuple>[<int:index>]
		
	example: (ITNAP).list["0040402121232"]["Message"]
	
	example with tree:

new Table(("Name",String),("Age",Integer)).Add("Joshua",14)

<object>
	<object>
		<Table>
			<new_table>
				<nametypepair>
					<sentence>
						"Name"
					<string>
						String
				<nametypepair>
					<sentence>
						"Age"
					<string>
						Integer
	<method>
		Add(<RADTuple>)
			<Element>
				<sentence>
					"Joshua"
			<Element>
				<string>
					14

tempvar temp<int> = {
	tempvar temp<int> = <Table>{
		.Relation = 
			(<sentence>,<RADtype>[(constaints)])
				"Name"
				String
			(<sentence>,<RADtype>[(constaints)])
				"Age"
				Integer
	}
	return temp<int>.Add(
		temp<int>.Relation,
		"Joshua",
		14
	)
}

Commands: 
	