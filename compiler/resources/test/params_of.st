TYPE ScaledParams : PARAMS(2) OF REAL; END_TYPE

PROGRAM main
VAR
    args : PARAMS(3) OF INT;
    named : ScaledParams;
END_VAR
    args[0] := 1;
END_PROGRAM
