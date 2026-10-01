PROGRAM TimeDemo
VAR
    delay : TIME := T#1m30s;
    stamp : LTIME := LTIME#2h15m;
    timeout : TIME := T#250ms;
END_VAR
delay := delay + T#1s;
END_PROGRAM
