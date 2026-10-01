INTERFACE IStartable EXTENDS IRunnable, IStoppable
END_INTERFACE

FUNCTION_BLOCK ABSTRACT Motor EXTENDS Drive IMPLEMENTS IStartable, IStoppable
VAR_INPUT
    enable : BOOL;
END_VAR
VAR_OUTPUT
    running : BOOL;
END_VAR
VAR
    speed : REAL := 0.0;
END_VAR
running := enable;

METHOD PUBLIC FINAL Start : BOOL
VAR_INPUT
    target : REAL;
END_VAR
VAR_INST
    starts : INT;
END_VAR
starts := starts + 1;
speed := target;
Start := TRUE;
END_METHOD

METHOD ABSTRACT Stop
END_METHOD

PROPERTY Rpm : REAL
GET
    Rpm := speed * 60.0;
END_GET
SET
    speed := Rpm / 60.0;
END_SET
END_PROPERTY
END_FUNCTION_BLOCK
