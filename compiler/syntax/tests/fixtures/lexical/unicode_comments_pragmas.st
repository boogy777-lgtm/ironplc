{attribute 'strict'}
PROGRAM Lexical	(* tab before this comment *)
VAR
	name : STRING[16]; (* café (* nested *) tab	here *)
	Größe : INT;  // line comment with ünïcode
	flag AT %IX0.0 : BOOL; /// doc comment
END_VAR
{attribute 'hide'}
name := 'Wärme $L $41';
Größe := 1;
/* block comment */ flag := TRUE;
END_PROGRAM
