PROGRAM main
VAR
    result : DINT;
    obj : INT;
END_VAR
    obj := __NEW(INT);
    obj := __NEW(INT, 4);
    result := __XADD(1, 2);
    result := __DELETE(obj);
    result := __TYPEOF(DINT);
    result := __ISVALIDREF(obj);
END_PROGRAM
