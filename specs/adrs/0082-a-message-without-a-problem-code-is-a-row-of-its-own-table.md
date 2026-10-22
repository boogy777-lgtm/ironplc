# A Message Without a Problem Code Is a Row of Its Own Table

status: accepted
date: 2026-10-19

## Context and Problem Statement

Every `Diagnostic` has a problem code, and the code gives it its category
([ADR-0081](0081-a-diagnostic-takes-its-category-from-its-problem-code.md)) and
its text ([ADR-0077](0077-message-texts-in-a-checked-dictionary.md)). CODESYS
also has messages without a code (`MessageId.None`): the line it keeps when a
run passes 500 errors, the line that starts a build and the lines that end a
build or a compile with their counts. The message ontology numbers this MSG-040,
and the user messages written by pragmas (MSG-080) will be a further member of the
same class. Such a message has no problem to take its category or its text from,
and it must still go through both forms of a message (the person form and the
program form) and the editor mapping, as a message with a code does.

## Decision Drivers

- **A text is a row of the dictionary.** No message text is written at a call
  site, with or without a code.
- **A category cannot be chosen at a call site.** The category comes from a
  declaration.
- **The consumers of a message do not branch on a second type.** The command line,
  the language server, MCP and the playground already take a list of
  `Diagnostic`.
- **A new member of the class is one row and one call.**

## Considered Options

1. **A second table of rows, `codeless-texts.csv` (`Key,Severity,Template`), and
   `Diagnostic::codeless` (chosen).** `problems/build.rs` generates
   `ironplc_problems::codeless::<key>(...)`, which returns a `CodelessText`: the
   key, the category and the message of one row. The diagnostic it makes has an
   empty `code`, `has_code()` false and no place.
2. **A row of `problem-codes.csv` with an empty code.** `Problem::code()` would
   answer an empty string for a problem, and every reader that parses the file by
   position would need a rule for it.
3. **A sibling type `Message` that holds a `Diagnostic` or a codeless line.** Every
   consumer that takes a list of diagnostics changes, and `any_stops_build` and
   the other helpers need a second form.
4. **A text and a category written at the call site.** Nothing is checked, and a
   text can be paired with any category.

## Decision Outcome

Chosen option: **a second table of rows with `Diagnostic::codeless`**, because
the declaration of the category and the text stays one row that the build checks,
and the consumers keep one list.

- A row names its category (`Error`, `Warning`, `Information`, `Text`, `FatalError`;
  not a suppressed category), because there is no problem to take it from. The key
  replaces the code as the identity of the message: two messages repeat each other
  when their key, category and place are the same.
- `Diagnostic::codeless` is the only constructor. The diagnostic has no code and no
  CODESYS counterpart, points into no file, and writes its person form as the word
  alone (`Error`) or, for a `Text`, as the plain line. Its program form has a null
  `prefix`, `number` and `codesys`.
- The rules for a run's messages (order, repeats, limits, summary lines) are
  written once in `ironplc_dsl::diagnostic` (`kept`, `counts`, `Summary`), where
  the limit of a kind and the row of its line are declared together. The limit
  lines are rows of this table, so they are messages like any other.
- `Diagnostic.code` stays a `String` that is empty for a message without a code,
  because the field is read in some two hundred files that only ever see coded
  messages. A place that must tell the two apart asks `has_code()`. This is the one
  declared gap: nothing stops a new reader from using the empty string as a code.
  The condition that closes it is making `code` private and giving it an accessor
  that returns `Option<&str>`.

### Consequences

- Good, because adding a limit line, a summary line or a pragma message is a row
  and a call; the text, the category and the parameters are checked by the build.
- Good, because a consumer that shows a list of diagnostics shows these messages
  without a change, and the language server and the playground need only to skip
  the code and the documentation link.
- Bad, because the empty `code` is a sentinel (the declared gap above).
- Neutral: the table is a second file beside `problem-texts.csv`, because its rows
  carry a column the other does not.

### Confirmation

- `compiler/msg_gen/src/tests.rs` and `compiler/problems/src/lib.rs`: one test
  for each failure of the table (header, missing, unknown or suppressed category,
  key repeated or not snake_case, malformed placeholder) and the texts of the seven
  rows (REQ-DS-problems-010, -011).
- `compiler/dsl/src/diagnostic/forms/tests.rs`: the diagnostic, its person form and
  its program form (REQ-DS-dsl-012 to -014).
- `compiler/dsl/src/diagnostic/run/tests.rs`: the limits, the repeats, the order and
  the summary lines (REQ-DS-dsl-015 to -023).
