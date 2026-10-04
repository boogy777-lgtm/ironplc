FUNCTION_BLOCK Counter
    VAR_INPUT
        up : BOOL R_EDGE;
        down, reset : BOOL F_EDGE;
        amount : INT;
    END_VAR
    VAR_OUTPUT
        n : INT;
    END_VAR

    n := n + amount;

END_FUNCTION_BLOCK

PROGRAM main
    VAR_INPUT RETAIN
        go : BOOL R_EDGE;
    END_VAR
    VAR
        c : Counter;
    END_VAR

    c(up := go);

END_PROGRAM
