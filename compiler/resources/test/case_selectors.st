TYPE
    E_State : (Idle, Run, Stop);
END_TYPE

PROGRAM main
    VAR
        n : INT;
        s : E_State;
        x : INT;
    END_VAR

    CASE n OF
        1, 2: x := 1;
        3..5: x := 2;
        6, 8..9, 12: x := 3;
        ELSE x := 4;
    END_CASE;

    CASE s OF
        Idle, E_State#Run: x := 5;
        Stop: x := 6;
    END_CASE;
END_PROGRAM
