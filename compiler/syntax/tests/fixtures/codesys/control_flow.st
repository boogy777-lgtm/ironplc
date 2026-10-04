(* Loop control, jumps and the conditional statements of CODESYS, a direct
   address read in a statement, and a qualified enumeration value in the
   initial value of a structure. *)
TYPE
    Color : (Red, Green, Blue);
    Pair : STRUCT
        a : INT;
        b : Color;
    END_STRUCT;
    First : Pair := (a := 1, b := Color#Red);
END_TYPE

PROGRAM control_flow
VAR
    i : INT;
    total : INT;
    ready : BOOL;
    sensor : BOOL;
END_VAR
    sensor := %IX0.1;
    FOR i := 0 TO 10 DO
        IF i = 3 THEN
            CONTINUE;
        END_IF;
        IF i = 8 THEN
            EXIT;
        END_IF;
        total := total + i;
    END_FOR;
    WHILE ready DO
        EXIT;
    END_WHILE;
    JMP (ready) skip;
    total := 0;
skip:
    CALC (ready, notify(1));
    __WAIT(ready);
END_PROGRAM
