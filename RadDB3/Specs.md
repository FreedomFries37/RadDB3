# General Info
Commands can be converted into objects of four types: Element, RADTuple, RADTuple[] (as TupleList), or Table
Runtime variables can be created to store the types
Runtime tempvars are cleared after every command
Objects can be piped through commands

### PRIVATE TYPES:

Syntactic Category | Optional | Rule
--- | :---: | ---
nametypepair | false | (_sentence_,_string_[(_constaints_)])
selection	| false | _string_=_string_
method  | false | _string_(_list_object_)

### OBJECT CREATION:
	<object>:
		<object>.<method>
		(<object>) #objects value is whatever type it stores, so (<Table>) is also a Table
		<Table>
		<TupleList>
		<RADTuple>
		<Element>
		
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
		
		
### COMMANDS:

	BEGIN LIVE SESSION:
		load [ -d | -f ]  <sentence>
			-d directory name
			-f .rd3 file name
		load -l
			finds the first database in the current directory
		
	TABLE COMMANDS
	# all of these commands return a Table object
	<command>:
		<table>:
			<new_table>:
				new Table(<string>,<nametypepair>,...)
			
			<join_info_full>
			
			<string> #table name in database
			
			{<table>(<selection>,...)@<columns>}
			{<table>(<selection>,...)}
		
### EXAMPLES:
		
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
	