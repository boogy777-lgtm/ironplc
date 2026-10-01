TYPE
    Percent : INT(0..100) := 50;
    Name : STRING[32] := 'pump';
    Wide : WSTRING(16);
    Vector : ARRAY[1..3] OF REAL := [1.0, 2.0, 3.0];
    Table : ARRAY[0..1, 0..2] OF INT;
    Open : ARRAY[*] OF BYTE;
    Color : (Red, Green := 5, Blue) BYTE := Green;
    Point : STRUCT
        x : INT := 1;
        y : INT := 2;
        label : STRING[8];
    END_STRUCT;
    Overlay : UNION
        word : WORD;
        bytes : ARRAY[0..1] OF BYTE;
    END_UNION;
    Pen : Point := (x := 3, y := 4);
    IntRef : REF_TO INT;
    Alias : REFERENCE TO INT;
    Handle : POINTER TO DINT;
    Args : PARAMS(4) OF INT;
END_TYPE

VAR_GLOBAL CONSTANT
    Limit : INT := 10;
END_VAR

VAR_GLOBAL RETAIN
    counter : DINT;
    flag AT %MX0.0 : BOOL;
END_VAR

PROGRAM Variables
VAR_INPUT
    start : BOOL R_EDGE;
    stop : BOOL F_EDGE;
END_VAR
VAR_IN_OUT
    shared : INT;
END_VAR
VAR RETAIN
    total : DINT := 0;
    zero, one : INT := 1;
END_VAR
VAR
    in_bit AT %IX0.0 : BOOL;
    words : ARRAY[1..4] OF INT := [4(0)];
    pen : Point := (x := 1, y := 2);
    motor : Fb(enable := TRUE);
    ptr : POINTER TO INT;
    ref : REF_TO INT := REF(total);
END_VAR
VAR_EXTERNAL CONSTANT
    Limit : INT;
END_VAR
VAR_TEMP
    scratch : INT;
END_VAR
VAR_STAT
    calls : INT;
END_VAR
total := total + 1;
END_PROGRAM
