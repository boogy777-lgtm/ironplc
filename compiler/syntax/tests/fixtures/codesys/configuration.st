CONFIGURATION Plant
    VAR_GLOBAL
        shared : INT;
    END_VAR
    RESOURCE Cpu ON PLC
        VAR_GLOBAL
            local : BOOL;
        END_VAR
        TASK Fast (INTERVAL := T#10ms, PRIORITY := 1);
        TASK Slow (SINGLE := trigger, PRIORITY := 5);
        PROGRAM Main WITH Fast : Control (setpoint := 10, out => shared, Motor WITH Slow);
        PROGRAM RETAIN Aux : Logger;
    END_RESOURCE
    VAR_CONFIG
        Cpu.Main.motor.speed AT %QW0 : INT := 5;
    END_VAR
END_CONFIGURATION
