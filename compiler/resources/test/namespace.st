(* CODESYS/TwinCAT NAMESPACE grouping plus the __BEGIN_IMPLEMENTATION
   implementation marker. Parsed with --allow-namespace and
   --allow-begin-implementation (the CODESYS dialect enables both). *)

NAMESPACE Motor
TYPE
E_Direction : (Forward, Backward);
END_TYPE

FUNCTION Blocked : BOOL
VAR_INPUT
    dir : INT;
END_VAR
    Blocked := dir < 0;
END_FUNCTION

NAMESPACE Control
PROGRAM main
VAR
    moving : BOOL;
END_VAR
__BEGIN_IMPLEMENTATION
    moving := NOT Blocked(1);
END_PROGRAM
END_NAMESPACE
END_NAMESPACE
