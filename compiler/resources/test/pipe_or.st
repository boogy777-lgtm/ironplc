PROGRAM main
VAR
    a : BOOL;
    b : BOOL;
    c : BOOL;
    or_result : BOOL;
    mixed : BOOL;
END_VAR
    or_result := a | b;
    mixed := a | b & c;
END_PROGRAM
