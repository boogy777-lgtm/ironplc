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

:program:`ironplcvm serve` [*FILE*]
   Load a bytecode container (``.iplc``) file into the runtime host, run its
   scan cycle, and serve the hot-edit command protocol on stdin/stdout until
   EOF or :kbd:`Ctrl+C`. Without *FILE* the device starts the program it last
   committed, and the empty application when it has none (see
   :ref:`ironplcvm-empty-application`). One line of
   stdin carries one command; one line of stdout carries one response,
   flushed after every line. Startup prints nothing to stdout — the stream
   carries protocol lines only, so the session is safe to script. The device
   scans by itself, as ``run`` does, and executes each command between two
   rounds (see :ref:`ironplcvm-scan`).

   Commands are the hot-edit protocol commands: ``getStatus``, ``acceptEdits``
   (the compiled container as a JSON array of byte values, plus an optional
   ``migration`` map of stable-UID decisions — ``"init"`` or ``"preserve"`` — and
   an optional ``mode``, ``"change"`` or ``"replace"``, see :ref:`ironplcvm-replace`),
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
   null — codec errors carry no V-code. A ``testEdits`` or ``untestEdits``
   records a swap that applies at a scan boundary, and so does ``cancelEdits``
   while the candidate executes under Test (see :ref:`ironplcvm-edit-view`).
   The acknowledgment of each is written after that boundary, so the swap has
   been applied when the line reaches the client. ``assembleEdits`` takes effect
   at once and is also answered after a boundary round.

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

A unit of a redundant pair (see :doc:`/how-to-guides/run-a-redundant-pair`)
is served by the same loop. The loop ticks the pair link every 5 ms, whether the
unit scans or not, and sets the execution permit from the verdict of the link
before it decides whether a round is due. The owner of the pair therefore scans
between commands, and the standby, which holds no execution permit, runs no
round and still synchronizes with the owner and answers every command. The
owner mints one pair epoch for each round it commits.

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
starting does not clear the fault; restart the device.

``startScan`` and ``stopScan`` work the same on a unit of a redundant pair. A
stopped owner runs no round and so mints no epoch, but it stays the owner: its
pair link keeps being ticked, so the standby sees a live owner and does not
promote itself, and no failover follows from the stop. The ``control`` field of
the ``redundancy`` block of ``identity`` reports ownership and ``runState``
reports the scan, apart from each other. A standby that is promoted after the
death of its peer runs the takeover round at once, unless the operator stopped
it: the stop stands until a ``startScan``.

.. _ironplcvm-edit-view:

The edit view
=============

The ``getStatus`` answer, and the ``application`` block of the ``identity``
answer, carry the field ``edit``: which edit commands the device allows now,
and for each one that it does not, why. A client shows this and decides nothing
itself, because the device builds it from the same table that it uses to accept
or refuse the command. A device that predates the field omits it.

.. code-block:: json

   {
     "stage": "accepted",
     "commands": {
       "accept": {"enabled": false, "label": "Accept",
                  "reason": {"code": "V4013", "text": "a candidate is already staged"}},
       "test": {"enabled": true, "label": "Test"},
       "untest": {"enabled": false, "label": "Untest",
                  "reason": {"code": "V4014", "text": "no test is in progress"}},
       "assemble": {"enabled": false, "label": "Assemble",
                    "reason": {"code": "V4017", "text": "assemble requires the candidate to have executed under Test"}},
       "cancel": {"enabled": true, "label": "Cancel"},
       "finalizeAll": {"enabled": false, "label": "Finalize all",
                       "reason": {"code": "V4013", "text": "a candidate is already staged"},
                       "steps": ["accept", "test", "assemble"]}
     },
     "next": {"command": "test", "label": "Test"},
     "warning": "a restart of the device loses this candidate"
   }

``stage``
   Where the edit stands: ``idle`` (no candidate), ``accepted`` (a candidate is
   staged and the original executes), ``testing`` (the candidate executes),
   ``testingMigration`` (the candidate of a state migration executes, and
   ``untestEdits`` and ``cancelEdits`` are refused with
   :doc:`V4011 <problems/V4011>`: only ``assembleEdits`` ends the edit),
   ``testingReplace`` (the same for the candidate of a download, see
   :ref:`ironplcvm-replace`) or
   ``swapPending`` (a ``testEdits``, ``untestEdits`` or ``cancelEdits`` waits for
   the next scan boundary, and no edit command is allowed until then).

``commands``
   One entry for each of ``accept``, ``test``, ``untest``, ``assemble`` and
   ``cancel``, which are ``acceptEdits``, ``testEdits``, ``untestEdits``,
   ``assembleEdits`` and ``cancelEdits``, and for ``finalizeAll``. ``enabled``
   is true exactly when the device accepts the command now. ``label`` is the
   plain English name to show. ``reason`` is present when ``enabled`` is false:
   the problem code and the text of the refusal that the command would get.
   When the refusal is :doc:`V4013 <problems/V4013>` and the accept that staged
   the candidate named the edit, the text names it too.
   ``steps`` is present when the command is made of several: ``cancel`` while the
   candidate executes under Test has the steps ``untest`` and ``cancel``. The
   client sends ``cancelEdits`` once; the device reverts to the original program
   at the next scan boundary, discards the candidate there, and answers after that
   boundary, so the one acknowledgment means the edit is over.
   ``finalizeAll`` is the composite command that runs ``steps`` in order, one
   after the other, with the scan boundary between them that applies a swap; it
   is the client that sends them. It is enabled when its first step is, so it is
   offered only while no candidate is staged.

``next``
   The command that moves the edit one stage on: ``accept`` when idle, ``test``
   when accepted, ``assemble`` when testing. It is always enabled. It is absent
   while a swap is pending.

``warning``
   Present while a candidate exists, in any stage but ``idle``: ``a restart of the
   device loses this candidate``. A candidate lives in RAM only. See
   :ref:`ironplcvm-lost-candidate`.

The scan control (``startScan`` and ``stopScan``) and ``setDeviceSettings``
are allowed in every stage and are not part of the view.

.. _ironplcvm-empty-application:

The empty application
=====================

:program:`ironplcvm serve` needs no container file. A device that has not been
given a program holds the *empty application*: a container with no program and
no task. It is not a state of the device. The device loads, verifies and runs it
as it does any other program, and the edit commands treat it as the program the
device runs. A device that holds it runs no round, because no task is there to
run, and it waits for the next command instead of repeating rounds that do
nothing.

Start the device without *FILE*:

.. code-block:: shell

   ironplcvm serve --listen 0.0.0.0

The device keeps its files in the current directory, named :file:`ironplcvm.*`
where a device that serves a file names them after the file: the slot store
(:file:`ironplcvm.slot-a`, :file:`ironplcvm.slot-b` and :file:`ironplcvm.marker`),
the settings (:file:`ironplcvm.device.json`), the retained values
(:file:`ironplcvm.retained`) and the note of an unfinished edit
(:file:`ironplcvm.edit`). The first start writes the empty application as the
first generation in the slot store. A later start boots the newest committed
generation in the store, so a device that was given a program with
``assembleEdits`` starts that program again, and the empty application only
until then. Start the device in the same directory each time; a device started
in another directory is another device, with another ``deviceId``.

A controller holds no application exactly when the ``contentHash`` of its
``application`` block (see :ref:`ironplcvm-edit-view`) is the content hash of the
empty application. The hash is a constant of the container format. It depends on
the format version, so it changes when the format version does:

.. list-table::
   :header-rows: 1

   * - Container format version
     - Content hash of the empty application
   * - 9
     - ``f363b8d33c67e163acb0b0a669a6d9ee5b3f81e13b59858a9234a85008dcb1c4``

The container format
--------------------

The ``identity`` answer carries the field ``containerFormat``: the container
format version that the device runs and accepts, a whole number. A workstation
that has built a container of another format version can say that the device
refuses it before it sends the container. The device refuses a container of
another format version with :doc:`V4016 <problems/V4016>`, and the message names
both versions, as in ``unsupported container format version 8: this reader
supports version 9``. A device that predates the field omits it, and the
protocol version stays 2.

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

.. _ironplcvm-replace:

Download: the replace mode
==========================

A download replaces the program that runs with another one. It is an
``acceptEdits`` with ``"mode": "replace"``; the default mode, ``"change"``, is
the online change of the other sections. A replace goes through the same
commands as every candidate: ``acceptEdits`` stages it, ``testEdits`` runs it and
``assembleEdits`` commits it. It is accepted when no other candidate is staged,
whatever the state of the device.

The device does not compare the layout, the task schedule or the process-image
sizes of a replace, so the candidate may differ from the running program in all
of them; the container itself is checked as for every candidate (the format, the
hashes and the bytecode verifier). The accept is answered with ``accepted``
instead of ``ack``:

.. code-block:: json

   {"response": "accepted", "reinitialised": [
     {"uid": 2, "name": "Retyped", "reason": "typeChanged"},
     {"uid": 3, "name": "Gone", "reason": "notInCandidate"}]}

When the replace is tested, at the next scan boundary, the device builds the
state of the new program from scratch and runs its initialisation, and then
copies over it the values of the ``PERSISTENT`` variables whose declaration is
equal in both programs. Every other variable, a ``RETAIN`` variable included,
starts at its initial value. This is the behaviour of CODESYS for a download. The
status reports ``candidateMode`` ``replace`` while the candidate is staged, and the
edit view the stage ``testingReplace`` while it runs.

``reinitialised``
   The ``PERSISTENT`` variables of the running program that start again, so that a
   reset is never silent. It is empty when every one is kept. Each entry has the
   ``uid`` (absent when the variable has none), the ``name`` (when the program
   carries debug information) and the ``reason``:

   .. list-table::
      :header-rows: 1

      * - Reason
        - Meaning
      * - ``noStableId``
        - The variable has no stable variable ID in the running program, or the
          downloaded program carries none, so the two cannot be matched.
      * - ``notInCandidate``
        - The downloaded program has no variable with this ID.
      * - ``qualifierChanged``
        - The downloaded program declares the variable without ``PERSISTENT``.
      * - ``typeChanged``
        - The type, string length, array bounds or function block layout differ.
      * - ``unsupportedType``
        - The device cannot yet carry a value of this type across a download (an
          instance of a standard function block, or of a block with a string field).

"Equal" means the same stable variable ID (the project keeps them in
:file:`{name}.uids.json`; create and update the file with
``ironplcc refactor sync-uids``), the same ``PERSISTENT`` qualifier and equal type
data. A project that was never synchronised has no IDs, and a download of it keeps
nothing and lists every ``PERSISTENT`` variable as ``noStableId``.

.. warning::

   The rule is by declaration, as in CODESYS. A variable whose meaning changes
   while its declaration stays equal keeps its old value. Change the name or the
   type of the variable when the old value must not survive.

A replace has no way back once it runs: the device keeps no copy of the old state,
so ``untestEdits`` and ``cancelEdits`` under test are refused with
:doc:`V4011 <problems/V4011>`, as for a state migration, and ``assembleEdits`` ends
the edit. Before ``testEdits``, ``cancelEdits`` discards the candidate and the
running program is untouched. The values the download kept are written to the
retained file when ``assembleEdits`` commits (see :ref:`ironplcvm-retained-values`),
so a restart after the download keeps them.

On a redundant pair the standby receives the candidate by the rule of an online
change and not as a replace, so a download whose layout differs is refused there
unless both programs carry stable variable IDs; the pair then does not report itself
ready. The state of the standby follows the replicated state of the primary.

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

.. _ironplcvm-lost-candidate:

A candidate lost by a restart
=============================

A candidate edit lives in RAM only, in whatever stage it is: staged, or
executing under Test. A restart of the device loses it, and the device starts the
program it last committed with no edit in progress. The engineer repeats the
edit. The edit view carries a ``warning`` while a candidate exists, so that a
client can say so before the restart happens.

So that the start after the restart can say what was lost, the device keeps a
small note of the unfinished edit in the file :file:`{FILE}.edit` beside the
served container. It is written when a candidate is accepted, rewritten when the
edit goes into or out of Test, and removed when the edit ends by ``assembleEdits``
or ``cancelEdits``. Each write is a temporary file :file:`{FILE}.edit.tmp`
renamed over the file. The note holds the ``name`` and ``origin`` of the
``acceptEdits`` command, the stage, and the content hash of the candidate. A
write that fails is logged and the device goes on: the command is answered as it
would be, and a later restart does not say that the candidate was lost.

When the device starts and finds the note, it removes it and reports the loss in
the ``identity`` answer, next to ``retained``:

``candidateLost``
   Present exactly when the previous run ended with an unfinished edit. ``name``
   and ``origin`` are present when the ``acceptEdits`` that staged the candidate
   gave them. ``stage`` is ``accepted``, ``testing``, ``testingMigration`` or
   ``testingReplace``, and is absent when the note was damaged and could not be read, so that only the
   loss is known. A device that predates the field omits it.

   .. code-block:: json

      {"candidateLost": {"name": "fix pump", "origin": "studio on WS-12", "stage": "testing"}}

The device never restores the candidate from the note, and the next start reports
nothing, because the note is gone.

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

7. Serve the engineering session with no program, in the current directory. The
   device holds the empty application until it is given a program (see
   :ref:`ironplcvm-empty-application`):

   .. code-block:: shell

      ironplcvm serve --listen 0.0.0.0

See Also
========

* :doc:`/reference/compiler/ironplcc` --- IronPLC compiler
* :doc:`overview` --- Runtime overview
