PROGRAM ReferenceDemo
VAR
    target : INT;
    alias : REFERENCE TO INT;
    raw_ptr : POINTER TO INT;
END_VAR
alias := target;
raw_ptr := ADR(target);
raw_ptr^ := target + 1;
END_PROGRAM
