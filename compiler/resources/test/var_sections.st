(* CODESYS/TwinCAT additional variable sections: VAR_STAT (static),
   VAR_INST (method instance) and VAR_GENERIC (function block generic
   constants). Parsed with the CODESYS dialect. *)

FUNCTION CountUp : DINT
VAR_STAT
    calls : DINT;
END_VAR
    calls := calls + 1;
    CountUp := calls;
END_FUNCTION

FUNCTION_BLOCK FB_Scale
VAR_GENERIC CONSTANT
    maxValue : INT := 100;
END_VAR
VAR
    value : INT;
END_VAR
    value := maxValue;
METHOD Clamp : INT
VAR_INST
    clamped : INT;
END_VAR
    IF value > maxValue THEN
        clamped := maxValue;
    ELSE
        clamped := value;
    END_IF;
    Clamp := clamped;
END_METHOD
END_FUNCTION_BLOCK

PROGRAM main
VAR_STAT
    scans : DINT;
END_VAR
VAR
    scale : FB_Scale;
    result : INT;
END_VAR
    scans := scans + 1;
    result := scale.Clamp();
END_PROGRAM
