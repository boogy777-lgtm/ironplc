FUNCTION_BLOCK typed_literals
VAR
    narrow : STRING[20];
    wide : WSTRING[20];
    code : STRING[20];
END_VAR
    narrow := UTF8#'a$41b';
    wide := __XSTRING#"a$0041b";
    code := UCHAR#'A';
END_FUNCTION_BLOCK
