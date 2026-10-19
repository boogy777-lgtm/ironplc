=============================
Read Compiler Error Messages
=============================

When IronPLC finds a problem in your code, it produces a diagnostic message.
This guide explains how to read those messages and find the fix.

--------------------------------------
Anatomy of an Error Message
--------------------------------------

A typical error message from :program:`ironplcc check` looks like this:

.. code-block:: text

   Error[P4007] (CODESYS C0046): Variable not defined before used (variable=x, did you mean=y)
     ┌─ main.st:5:8
     │
   5 │   y := x + 1;
     │        ^ Undefined variable
     │
     = Learn more: https://www.ironplc.com/reference/compiler/problems/P4007.html

The message has several parts:

- **Error** --- the category of the message, with the words of the CODESYS
  Messages view: ``Error`` (also for a fatal error), ``Warning`` or
  ``Information``.
- **[P4007]** --- the problem code. The code uniquely identifies the kind of
  problem.
- **(CODESYS C0046)** --- the code of the same condition in CODESYS, when
  there is one. Not every problem has one.
- **Variable not defined before used** --- a human-readable summary of what
  went wrong.
- **┌─ main.st:5:8** --- the file name, line number, and column where the
  problem was found.
- The **code snippet** shows the relevant line with a marker pointing to the
  exact location.

--------------------------------------
Errors, Warnings and Information
--------------------------------------

The severity belongs to the problem code. An error or a fatal error means the
program is not valid: :program:`ironplcc` exits with a failure status and does
not produce a container. A fatal error also ends the run, so the later stages
do not run. A warning, an information message or a line of text is shown and
does not stop :program:`ironplcc check` or :program:`ironplcc compile`. A
line of text is written as it is, without a category word and without a code. A
message that is suppressed is not shown.

--------------------------------------
Using Problem Codes
--------------------------------------

Every problem code (like P2001 above) has a documentation page that
explains:

- When the error occurs
- An example that triggers it
- How to fix it

You can find all problem codes in the :doc:`/reference/compiler/problems/index`.

--------------------------------------
In the VS Code Extension
--------------------------------------

When you use the VS Code extension, errors and warnings appear as squiggly
underlines in the editor. To see the full message:

- Hover over the underlined code to see the diagnostic in a tooltip.
- Open the :guilabel:`Problems` panel (:menuselection:`View --> Problems`)
  to see all diagnostics for the workspace.
- Click on a diagnostic in the Problems panel to jump to the source location.

The :guilabel:`Problems` panel groups messages as the CODESYS Messages view
does: errors (also fatal errors), warnings and information. A line of text is
not a problem of a file, so it appears in the :guilabel:`Output` channel of
IronPLC and not in the :guilabel:`Problems` panel.

--------------------------------------
Common Mistakes
--------------------------------------

**Misspelled type name:**

.. code-block::

   VAR
      Counter : IN := 0;   (* should be INT *)
   END_VAR

**Missing semicolons:**

.. code-block::

   Counter := Counter + 1   (* missing ; *)

**Mismatched END keyword:**

.. code-block::

   PROGRAM main
   END_FUNCTION   (* should be END_PROGRAM *)

In each case, IronPLC points to the location of the problem and provides
a code you can look up for more detail.
