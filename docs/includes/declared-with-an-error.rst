A type whose own declaration has an error is declared, so a use of it is never
reported as undeclared. The compiler reports the first cause where the type is
declared, and the declarations and statements that use the type are analyzed
without a message about it. A message that does not depend on the type, such as
a mismatch between two other operands, is still reported.
