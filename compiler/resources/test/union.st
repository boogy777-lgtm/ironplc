(* CODESYS UNION type declaration (IEC 61131-3:2013).
   Parsed with --allow-union-type / --dialect codesys. *)
TYPE
U_Data :
UNION
    intVal : INT;
    realVal : REAL;
    flags : ARRAY[0..31] OF BOOL;
END_UNION;
U_Point :
UNION
    x : INT := 1;
    pair : ARRAY[0..1] OF INT;
END_UNION;
END_TYPE

PROGRAM main
VAR
    data : U_Data;
    x : INT;
END_VAR
    data.intVal := 1;
    x := data.intVal;
END_PROGRAM
