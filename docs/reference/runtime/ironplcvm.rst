==========
ironplcvm
==========

Name
====

ironplcvm --- IronPLC bytecode virtual machine

Synopsis
========

| :program:`ironplcvm` [*OPTIONS*] *COMMAND*

Description
===========

:program:`ironplcvm` is the IronPLC virtual machine runtime. It loads and
executes compiled bytecode container (``.iplc``) files that
:doc:`ironplcc </reference/compiler/ironplcc>` produces.

The runtime follows the IEC 61131-3 execution model. Each scheduling round,
the runtime checks which tasks are due based on elapsed time and executes
them in priority order.

By default, :program:`ironplcvm` runs continuously until interrupted with
:kbd:`Ctrl+C`. Use ``--scans`` to limit execution to a fixed number of
scheduling rounds.

Commands
========

:program:`ironplcvm run` [*OPTIONS*] *FILE*
   Load and execute a bytecode container (``.iplc``) file.

   ``--dump-vars`` *FILE*
      Write all variable values to the specified file after execution stops.
      The output contains one variable per line in the format ``NAME: VALUE``,
      using the names recorded in the container's debug section. A variable
      with no recorded name is reported as ``var[N]``. Variables are dumped on
      both normal shutdown and after a runtime error.

      Each value is written as the IEC 61131-3 literal for its declared type,
      the same rendering the debugger and the playground show:

      .. code-block:: text

         msg: 'hello'
         wide: "hello"
         flag: TRUE
         n: 42
         ratio: 1.5
         mask: 16#ABCD
         span: T#1500ms
         day: D#2024-01-15
         clock: TOD#14:30:00
         stamp: DT#2024-01-15-14:30:00
         shade: GREEN (1)

      Durations are written in milliseconds (``T#1500ms``, not ``T#1.5s``) so
      the value is exact and reparses as the same literal. A value that cannot
      be read — a string whose container carries no layout for it, or one whose
      layout does not fit the program's data — is written as ``<unavailable>``
      or ``<invalid>`` rather than as a number that would read like a value.

      Structures, arrays and function block instances are named by their type
      rather than shown, because the runtime does not yet record enough layout
      information to read their contents back:

      .. code-block:: text

         origin: <POINT>
         counts: <ARRAY OF DINT>
         timer: <TON>

   ``--scans`` *N*
      Run exactly *N* scheduling rounds then stop. Without this option, the
      runtime runs continuously until interrupted with :kbd:`Ctrl+C`.

:program:`ironplcvm version`
   Print the version number of the virtual machine.

:program:`ironplcvm serve` *FILE*
   Load a bytecode container (``.iplc``) file into the runtime host and serve
   the hot-edit command protocol on stdin/stdout until EOF. One line of
   stdin carries one command; one line of stdout carries one response,
   flushed after every line. Startup prints nothing to stdout — the stream
   carries protocol lines only, so the session is safe to script.

   Commands are the hot-edit protocol commands: ``getStatus``, ``acceptEdits``
   (the compiled container as a JSON array of byte values), ``testEdits``,
   ``untestEdits``, ``assembleEdits`` and ``cancelEdits``. Responses are JSON:
   an acknowledgment, a status payload, or an error carrying a stable
   ``vCode`` and ``message``. A line that does not parse as a command is
   answered with an error line whose ``vCode`` is null — codec errors carry
   no V-code. A ``testEdits``, ``untestEdits`` or ``assembleEdits``
   acknowledgment records a swap that applies at the next scan boundary:
   after the acknowledgment, the session drives one scan round at a constant
   zero uptime, before the response line is written, so the swap has been
   applied when the line reaches the client. A trap in that round is logged
   to stderr with the trap's V-code and the session continues.

Options
=======

``-v``, ``--verbose``
   Turn on verbose logging. Repeat the flag to increase verbosity (e.g.,
   ``-vvv``).

``-l`` *FILE*, ``--log-file`` *FILE*
   Write log output to the specified file instead of the terminal.

Examples
========

1. Run a compiled program:

   .. code-block:: shell

      ironplcvm run main.iplc

2. Run for a single scan and dump variable values:

   .. code-block:: shell

      ironplcvm run main.iplc --scans 1 --dump-vars output.txt

3. Run with verbose logging:

   .. code-block:: shell

      ironplcvm -vv run main.iplc

4. Serve a program to a scripted hot-edit session:

   .. code-block:: shell

      ironplcvm serve main.iplc

See Also
========

* :doc:`/reference/compiler/ironplcc` --- IronPLC compiler
* :doc:`overview` --- Runtime overview
