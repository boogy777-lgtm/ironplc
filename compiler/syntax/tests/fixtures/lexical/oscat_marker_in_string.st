PROGRAM OscatMarkerInString
VAR
    text : STRING[40];
    count : INT;
END_VAR
text := '(*@KEY@:X*)';
count := count + 1;
text := '(*@KEY@:END_X*)';
END_PROGRAM
