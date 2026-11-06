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
   Load a bytecode container (``.iplc``) file into the runtime host, run its
   scan cycle, and serve the hot-edit command protocol on stdin/stdout until
   EOF or :kbd:`Ctrl+C`. One line of
   stdin carries one command; one line of stdout carries one response,
   flushed after every line. Startup prints nothing to stdout — the stream
   carries protocol lines only, so the session is safe to script. The device
   scans by itself, as ``run`` does, and executes each command between two
   rounds (see :ref:`ironplcvm-scan`).

   Commands are the hot-edit protocol commands: ``getStatus``, ``acceptEdits``
   (the compiled container as a JSON array of byte values, plus an optional
   ``migration`` map of stable-UID decisions — ``"init"`` or ``"preserve"``),
   ``testEdits``, ``untestEdits``, ``assembleEdits`` and ``cancelEdits``, and
   ``identity`` and ``setDeviceSettings`` for the device itself (see
   :ref:`ironplcvm-device-settings`), and ``startScan`` and ``stopScan`` for the
   scan (see :ref:`ironplcvm-run-stop`).
   Responses are JSON: an acknowledgment, a status payload, or an error
   carrying a stable ``vCode`` and ``message``; a V4010 refusal additionally
   carries a ``pairs`` array naming every out-of-policy type change, so the
   client can resubmit the payload with the decisions map. A V4020 refusal
   names the persistent variables that have no stable variable ID, which no
   decision can replace. A line that does
   not parse as a command is answered with an error line whose ``vCode`` is
   null — codec errors carry no V-code. A ``testEdits``, ``untestEdits`` or
   ``assembleEdits`` records a swap that applies at a scan boundary, and its
   acknowledgment is written after that boundary, so the swap has been applied
   when the line reaches the client.

   ``--listen`` *ADDR*
      Serve the same protocol on TCP instead of stdin/stdout. *ADDR* is
      *IP*:*PORT*, or an IP address alone (IPv4 or IPv6), which uses port
      |session-port|. Write an IPv6 address with a port in brackets, as in
      ``[::1]:7000``. A host name is not accepted. Each message is one frame, a
      4-byte little-endian length followed by that many bytes of the same JSON
      line, and the device serves one engineering session at a time: a second
      connection receives one error line and is closed. The device also answers
      discovery on UDP at the same address and port (see
      :ref:`ironplcvm-discovery`).

   ``--no-discovery``
      Do not answer discovery. The device then opens no UDP port, and a
      workstation connects to it by an address it already knows. Requires
      ``--listen``.

.. _ironplcvm-scan:

The scan of the device
======================

:program:`ironplcvm serve` runs the scan cycle of the program by itself, from the
start of the device until it is stopped, paced the way ``run`` paces it: a
cyclic task runs once in each of its periods, and a program without a cyclic
task runs its next round at once. The commands of the session do not start
rounds; the device executes each command between two rounds and answers it at
once, so a command does not wait for the period of the tasks. The one kind of
command that is not answered at once is a request of a swap: ``testEdits``,
``untestEdits`` and ``assembleEdits`` are each answered after the round that
applied them, and the device runs that round without waiting for the period.

A device that holds no execution permit (a secondary unit of a redundant pair)
runs no round and still answers every command.

:kbd:`Ctrl+C` stops the device in order, on stdin/stdout and on TCP alike: the
device finishes the round that is running, answers the commands it has
executed, writes the retained values (see :ref:`ironplcvm-retained-values`) and
exits with code 0.

A trap in a round stops the scan. The device is *faulted*: it runs no further
round until the device is restarted, and it still answers every command. The trap
is logged to stderr with its V-code, and the ``identity`` answer carries the
optional field ``fault`` next to ``application`` for as long as the device is
faulted:

.. code-block::

   {"response":"identity", ..., "fault":{"vCode":"V4001","message":"runtime error: divide by zero (task 0, instance 0)","taskId":0,"instanceId":0}}

``vCode`` is the V-code of the trap, ``message`` says what happened, and
``taskId`` and ``instanceId`` name the task and the program instance that were
running. The field is absent while the device is not faulted. A ``testEdits`` or
``untestEdits`` on a faulted device is still applied and acknowledged; the
program does not run.

.. _ironplcvm-run-stop:

Run and Stop
============

The operator can stop the scan and start it again without restarting the device.
Run and Stop are separate from the edit: they change whether the device executes
rounds, and nothing else. They compile nothing, stage nothing, and leave a staged
candidate or a test in progress as it was.

``{"command":"startScan"}``
   The device executes rounds again.

``{"command":"stopScan"}``
   The device finishes the round that is running, executes no further round, and
   goes on answering every command.

Both commands answer the run state the device is in afterwards:

.. code-block::

   {"response":"runState","runState":"stopped"}

A ``startScan`` of a running device and a ``stopScan`` of a stopped device are
acknowledged the same way and change nothing. The ``identity`` answer always
carries the field ``runState`` next to ``application``, ``running`` or
``stopped``. The device starts running; it does not keep the state across a
restart. The run state is apart from a fault: a faulted device reports the state
it was commanded to, and ``fault`` says why it does not scan.

What a stopped device does:

- It runs no round and answers every command, including ``identity``,
  ``getStatus`` and the edit commands.
- It writes the retained values when it is stopped (see
  :ref:`ironplcvm-retained-values`), because they do not change while it is
  stopped and a power cut then loses nothing. A write that fails answers
  :doc:`V6018 <problems/V6018>` instead of the state, and the device is stopped
  all the same.
- A ``testEdits``, ``untestEdits`` or ``assembleEdits`` is applied at once
  without executing the program, as on a faulted device, and is acknowledged
  after it is applied. The swapped-in program runs from the first round after a
  ``startScan``.

A ``startScan`` does not catch up. The first round runs at once, every cyclic
task runs in it once, and the period of each task counts from that round; the
time spent stopped is neither a burst of rounds nor an overrun.

A faulted device accepts ``stopScan``: it is stopped and still reports its
``fault``. It refuses ``startScan`` with :doc:`V6019 <problems/V6019>`, because
starting does not clear the fault; restart the device. A device that serves a
redundant pair over the pair link does not scan between commands and answers
both commands with :doc:`V4022 <problems/V4022>`.

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
     "name": "ironplcvm",
     "retainedSavePeriod": 0
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

``retainedSavePeriod``
   How often the device writes the values of the retained variables while it
   runs, in seconds: a whole number from 0 to 86400, written as a JSON number
   (``60``, not ``"60"``). The default is ``0``: the device then writes the values
   only when it stops and when ``assembleEdits`` commits a new program. A period
   adds a write every period in which the values changed. A write replaces a
   file on the disk, so a short period wears flash storage; a long one loses
   more values when the power fails. See :ref:`ironplcvm-retained-values`.

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
   * - ``retainedSavePeriod``
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

.. _ironplcvm-retained-values:

Retained values
===============

:program:`ironplcvm serve` keeps the values of the variables declared ``RETAIN``
or ``PERSISTENT`` across a restart of the device, in the file
:file:`{FILE}.retained` beside the served container. The device applies the file
once, when it starts, before the first scan. A variable that is not retained
starts with its initial value. See :doc:`/reference/language/variables/retention`
for the variables and their qualifiers.

The device writes the file at four points:

- when :program:`ironplcvm serve` stops, at the end of its input or at
  :kbd:`Ctrl+C`;
- when ``stopScan`` stops a running device (see :ref:`ironplcvm-run-stop`);
- when ``assembleEdits`` commits a new program, with the values of the new
  program;
- every ``retainedSavePeriod`` seconds, at a scan boundary, when the values
  changed and the setting is not ``0``.

Each write is a temporary file :file:`{FILE}.retained.tmp` renamed over the
file, so the file is always the old values or the new ones. The device writes
nothing while a candidate runs under test, and nothing for a program that did
not reach the slot store (:doc:`V6012 <problems/V6012>`), because the device would
not start it again.

The device applies the file only to the program it was written for. The file
carries the content hash of that program and a hash of the variables it
retains. When either differs from the program that starts, or the file is
damaged, the device discards the file, starts the retained variables with their
initial values, and says so in the ``identity`` answer. A discarded file stays
on the disk until the next write replaces it. The ``identity`` answer carries
two optional fields next to ``application``:

``retained``
   What the device did with the file when it started.

   .. list-table::
      :header-rows: 1

      * - Answer
        - Meaning
      * - ``{"result": "applied"}``
        - The file belonged to the program and the device applied it.
      * - ``{"result": "absent"}``
        - There was no file.
      * - ``{"result": "discarded", "reason": "damaged"}``
        - The file could not be read, was not a retained file, or was cut short.
      * - ``{"result": "discarded", "reason": "otherApplication"}``
        - The file was written for another program.
      * - ``{"result": "discarded", "reason": "otherRetainedLayout"}``
        - The file holds other retained variables than the program declares.

``persistedContentHash``
   The content hash of the program the device starts after a restart. It equals
   ``application.contentHash`` unless an ``assembleEdits`` was answered with
   :doc:`V6012 <problems/V6012>`: the new program runs, and the device would
   start the previous one.

A write that fails is :doc:`V6018 <problems/V6018>`. The device keeps running and
the file keeps its previous content. ``assembleEdits`` answers the error instead
of the acknowledgment, the period logs it, and :program:`ironplcvm serve` exits
with code 2 when the write at the stop fails.

.. _ironplcvm-discovery:

Discovery
=========

With ``--listen``, :program:`ironplcvm serve` also listens on UDP at the address
and port of the TCP session, and answers a discovery request with the public
identity of the device: the device id, the name, the model, the runtime version,
the session protocol version, the TCP port of the session, and whether the
device is protected. A protected device requires credentials; no device does
today, so the answer says that the device is open. The answer says nothing about
the application or about whether a session is open, and a device answers
discovery while an engineering session runs. The answer reads the name when it is sent, so
a name changed with ``setDeviceSettings`` is in the next answer.

The device answers only the request itself, a fixed message of 8 bytes. It sends
neither an answer nor an error to any other datagram. The answer is longer than
the request, and the address that a datagram names as its source can be forged,
so the device limits its answers: at most one answer in 250 ms to one source
address and at most 64 answers in a second to all sources together. A workstation
that sends one request and repeats it a few times is within the limits.

The device receives the requests that are sent to the address it listens on. A
request that a workstation broadcasts to the subnet reaches the device only if
the operating system delivers broadcasts to the socket. On Linux, a socket that
is bound to one unicast address does not receive broadcast datagrams, so listen
on ``0.0.0.0`` for a device that a broadcast scan must find. Other operating
systems deliver broadcasts differently; test the scan on the system that runs the
device.

If the UDP port is taken, :program:`ironplcvm serve` stops with
:doc:`V6017 <problems/V6017>`. Add ``--no-discovery`` to run without discovery.

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

5. Serve the engineering session on TCP port |session-port| of all IPv4
   interfaces, and answer discovery there:

   .. code-block:: shell

      ironplcvm serve --listen 0.0.0.0 main.iplc

6. Serve the session on a chosen address and port, without discovery:

   .. code-block:: shell

      ironplcvm serve --listen 192.168.1.20:7000 --no-discovery main.iplc

See Also
========

* :doc:`/reference/compiler/ironplcc` --- IronPLC compiler
* :doc:`overview` --- Runtime overview
