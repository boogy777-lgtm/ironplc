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
   (the compiled container as a JSON array of byte values, plus an optional
   ``migration`` map of stable-UID decisions — ``"init"`` or ``"preserve"``),
   ``testEdits``, ``untestEdits``, ``assembleEdits`` and ``cancelEdits``, and
   ``identity`` and ``setDeviceSettings`` for the device itself (see
   :ref:`ironplcvm-device-settings`).
   Responses are JSON: an acknowledgment, a status payload, or an error
   carrying a stable ``vCode`` and ``message``; a V4010 refusal additionally
   carries a ``pairs`` array naming every out-of-policy type change, so the
   client can resubmit the payload with the decisions map. A V4020 refusal
   names the persistent variables that have no stable variable ID, which no
   decision can replace. A line that does
   not parse as a command is answered with an error line whose ``vCode`` is
   null — codec errors carry no V-code. A ``testEdits``, ``untestEdits`` or
   ``assembleEdits`` acknowledgment records a swap that applies at the next
   scan boundary: after the acknowledgment, the session drives one scan round
   at a constant zero uptime, before the response line is written, so the
   swap has been applied when the line reaches the client. A trap in that
   round is logged to stderr with the trap's V-code and the session
   continues.

.. _ironplcvm-device-settings:

Device settings
===============

:program:`ironplcvm serve` keeps the settings that belong to the device, and not
to the program it runs, in the file :file:`{FILE}.device.json` beside the served
container. The file is JSON:

.. code-block:: json

   {
     "version": 1,
     "deviceId": "0123456789abcdef0123456789abcdef",
     "name": "ironplcvm"
   }

``version``
   The version of the file. The only version is ``1``.

``deviceId``
   The unique id of the device: 32 lowercase hexadecimal digits. The device
   generates it from 128 random bits when it starts without the file, and keeps
   it across restarts. The ``identity`` answer reports it. A workstation
   recognises the device again by this id. Change it in the file only, and only
   on the device itself: see below.

``name``
   The name of the device: 1 to 64 characters, not blank, without control
   characters. The default is ``ironplcvm``. The ``identity`` answer reports it.

A device without a settings file is a new device. Delete the file to reset a
device: the next start generates a new ``deviceId``. A file that lacks a setting
takes the default of that setting, and the device writes the file complete.

Change a setting by editing the file while the device is stopped. A setting can
also be changed online, with the session command ``setDeviceSettings``, when the
table below allows it. The command names the settings to change by their keys; a
setting that it does not name stays as it is:

.. code-block:: json

   {"command": "setDeviceSettings", "name": "cell 1"}

.. list-table::
   :header-rows: 1

   * - Setting
     - Changed in the file
     - Changed by ``setDeviceSettings``
   * - ``deviceId``
     - yes
     - no
   * - ``name``
     - yes
     - yes

The device id is how a workstation recognises the device again, and the session
cannot change it: an id that an engineer could change over the network could give
a device the identity of another one. Give a replacement device the id of the device it
replaces, or give a cloned device a new id, by editing the file on that device.

The device checks every named value, writes the file, and answers
``{"response": "ack"}``. The next ``identity`` answer and the next start report
the new values. The command works while a candidate is staged, because the
settings are not the program. A command that names ``deviceId`` is refused as a
whole with :doc:`V6015 <problems/V6015>`, even when it also names ``name``. A
value that a setting refuses is answered with the same code, and a file that
cannot be written with :doc:`V6016 <problems/V6016>`; the file and the device
stay as they were.

A settings file that the device cannot use stops :program:`ironplcvm serve`
with :doc:`V6015 <problems/V6015>`. The device does not replace a damaged file
with a new identity.

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
