(* CODESYS exception handling: __TRY/__CATCH/__FINALLY/__ENDTRY, __THROW. *)

PROGRAM main
VAR
    e : INT;
    x : INT;
END_VAR
    __TRY
        x := 1;
        IF x > 0 THEN
            __THROW(7);
        END_IF;
        x := 2;
    __CATCH (e)
        x := e;
    __FINALLY
        x := x + 1;
    __ENDTRY;
END_PROGRAM

FUNCTION_BLOCK guarded
VAR
    n : INT;
END_VAR
    __TRY
        n := n + 1;
    __ENDTRY;
    __TRY
        n := n + 2;
    __FINALLY
        n := n + 3;
    __ENDTRY;
    __TRY
        n := n + 4;
    __CATCH ()
        n := 0;
    __ENDTRY;
    __TRY
        n := n + 5;
    __CATCH
        __THROW;
    __ENDTRY;
END_FUNCTION_BLOCK
