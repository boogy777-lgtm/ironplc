PROGRAM IoDemo
VAR
    input_bit AT %IX0.0 : BOOL;
    output_word AT %QW1 : WORD;
    counter : DINT;
END_VAR
output_word := 16#00FF;
input_bit := FALSE;
END_PROGRAM
