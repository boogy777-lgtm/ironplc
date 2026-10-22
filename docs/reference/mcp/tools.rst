==============
Tool Reference
==============

This chapter describes every tool the IronPLC MCP server exposes. Tools
are grouped by purpose: analysis tools validate source, context tools
summarise a project, and execution tools produce artifacts.

Every tool that takes source files shares two input fields:

``sources`` (array, required)
   One or more source files. Each entry has a ``name`` (logical file
   name — non-empty, at most 256 bytes of printable ASCII, unique
   within the array) and a ``content`` string with the full source
   text.

``options`` (object, required)
   Compiler options. Must include a ``dialect`` field set to one of the
   identifiers returned by :literal:`list_options`
   (for example, ``iec61131-3-ed2``, ``iec61131-3-ed3``, ``rusty``).
   May also include boolean feature-flag overrides
   (such as ``allow_c_style_comments``) and two lists of warning codes,
   ``disabled_warnings`` and ``warnings_as_errors``, each an array of strings.
   Call :literal:`list_options` for the authoritative list.

   ``disabled_warnings``
      Codes of warnings that are not shown and not counted.

   ``warnings_as_errors``
      Codes of warnings that are reported as errors, with the same code.

   .. include:: ../../includes/warning-codes.rst

   .. include:: ../../includes/warning-settings-effect.rst

   An option that is not accepted, a list that is not an array of strings and a
   code that is not accepted are each returned as a diagnostic, and the tool does
   not run.

All tools return ``diagnostics`` as an array of message objects with the
fields of the CODESYS scripting class ``ScriptMessage``:

.. include:: ../../includes/message-object-fields.rst

:literal:`check`, :literal:`compile` and the tools that analyze the sources and
return the ``diagnostics`` of the run (:literal:`symbols`,
:literal:`project_manifest`, :literal:`project_io`, :literal:`pou_scope`,
:literal:`pou_lineage` and :literal:`types_all`) return their ``diagnostics``
framed as :program:`ironplcc` writes them to the terminal: the messages of the
run with the line ``Compile complete -- N errors, M warnings`` that closes it,
and for :literal:`compile` the line that starts it and the build line that ends
it. The lines are messages of category ``Text`` without a code, so a program
without a problem has the closing line alone and ``ok``, not the length of the
array, says whether a problem stops the build. A POU that :literal:`symbols`,
:literal:`pou_scope` or :literal:`pou_lineage` is asked for and the sources do
not declare is one error of the run, and the closing line counts it.

Analysis Tools
==============

list_options
------------

Enumerates the dialects and feature flags accepted in the ``options``
object of the source-accepting tools. Call this once at the start of a
session to learn what your build of :program:`ironplcmcp` supports;
the set of flags evolves with the compiler.

**Inputs:** none.

**Returns:** an object listing each dialect and each option (feature flags,
behavior policies and the two lists of warnings) with its option key, its type
(``bool``, ``enum`` or ``string-list``) and its default value.

explain_diagnostic
------------------

Looks up the human-readable explanation for a compiler problem code.
Call this before editing code in response to a diagnostic you do not
fully understand — the explanation often names the exact construct
to change.

**Inputs:**

* ``code`` (string, required) --- Problem code to look up
  (e.g. ``"P0042"``). Case-insensitive; whitespace is trimmed.

**Returns:** an object with ``found``, ``code``, ``title``,
``description``, and an optional ``suggested_fix`` pulled from the
same documentation rendered at
:doc:`/reference/compiler/problems/index`.

parse
-----

Syntax check only. Confirms that the supplied sources tokenize and
parse. Use while drafting to catch missing keywords, unbalanced
brackets, and similar mistakes quickly. **Do not** use :literal:`parse`
to validate a change — it does not catch type errors, undeclared
symbols, or any other semantic rule. Call :literal:`check` for that.

**Inputs:** ``sources``, ``options``.

**Returns:** an object with ``ok``, a ``structure`` array listing the
top-level declarations found in each file, and ``diagnostics``.

check
-----

Primary validator. Runs the full pipeline through semantic analysis
and returns structured diagnostics. Always call :literal:`check`
before reporting a change as complete and before calling
:literal:`compile`. To self-heal, read the returned diagnostics, fix
the source, and call :literal:`check` again — use
:literal:`explain_diagnostic` to clarify any unfamiliar problem code
before editing.

**Inputs:** ``sources``, ``options``.

**Returns:** an object with ``ok`` and ``diagnostics``: the problems found,
then the line ``Compile complete -- N errors, M warnings``, which counts the
errors (fatal errors and errors) and the warnings of the run.

get_message_objects
-------------------

Returns the messages of a check as message objects, as
``system.get_message_objects`` of the CODESYS scripting API does. In the
:ref:`Build area <message-areas>` (the default) the messages are those of
:literal:`check`, which end with the line ``Compile complete -- N errors, M
warnings``, a ``Text`` without a code.

**Inputs:** ``sources``, ``options``, ``severities`` (integer, optional) --- the
categories to return, as the sum of their ``severity_value``; the default is
every category, suppressed ones included (4294967295) --- and ``area`` (string,
optional) --- the area to read, ``Build`` (the default) or ``Precompile``.

**Returns:** an object with ``ok`` and ``messages``, an array of message
objects.

get_messages
------------

Returns the messages of a check as texts alone, as ``system.get_messages`` of
the CODESYS scripting API does, with the line that ends the check in the Build
area. Call :literal:`get_message_objects` to see the category, the code and the
position of each.

**Inputs:** ``sources``, ``options`` and ``area`` (string, optional) --- the
area to read, ``Build`` (the default) or ``Precompile``.

**Returns:** an object with ``ok`` and ``messages``, an array of strings.

get_message_areas
-----------------

Lists the :ref:`areas <message-areas>` of messages that ``get_message_objects``
and ``get_messages`` can read, as ``system.get_message_categories`` and
``system.get_message_category_description`` of the CODESYS scripting API do.

**Inputs:** none.

**Returns:** an object with ``areas``, an array of objects with ``name`` (the
value to give as ``area``), ``source`` (the source an editor shows for a message
of the area) and ``description``.

symbols
-------

Extracts the full symbol table for the supplied sources: programs,
functions, function blocks, their variables, and user-defined types.
Large responses are capped at 256 KiB; when the cap is reached the
response is empty with ``truncated`` set to ``true``. Use the
``pou`` filter or one of the context tools below when you only need
part of the answer.

**Inputs:**

* ``sources`` (required)
* ``options`` (required)
* ``pou`` (string, optional) --- Restrict the response to a single
  program, function, or function block by name. Matching is
  case-insensitive. Referenced user-defined types are still included.

**Returns:** an object with ``ok``, arrays ``programs``, ``functions``,
``function_blocks``, ``types``, a ``truncated`` flag, and
``diagnostics``. When ``pou`` is supplied, ``found`` indicates whether
a matching POU existed.

Context Tools
=============

project_manifest
----------------

Flat summary of what is declared across the supplied sources: file
names, program / function / function-block names, and user-defined
types grouped by kind (enumerations, structures, arrays, subranges,
aliases, strings, references). Use this to orient yourself in an
unfamiliar project before calling :literal:`symbols` or a more
targeted context tool.

**Inputs:** ``sources``, ``options``.

**Returns:** an object with ``ok``, ``files``, ``programs``,
``functions``, ``function_blocks``, grouped ``types``, and
``diagnostics``.

project_io
----------

Lists the inputs the caller can drive and the outputs the caller can
observe. This is the right tool to call before planning an execution
run — for example, to decide which variables to supply as stimuli or
which to trace.

**Inputs:** ``sources``, ``options``.

**Returns:** an object with ``ok``, ``inputs``, ``outputs``, and
``diagnostics``. Each entry has a ``name``, a ``type`` and an
``address``. A program variable is named ``program.variable`` and a
global variable by its bare name.

* ``inputs`` holds the program ``VAR_INPUT``, ``VAR_IN_OUT`` and
  ``VAR_EXTERNAL`` variables, the global variables without an address,
  and the variables at an ``%I`` address.
* ``outputs`` holds the program ``VAR_OUTPUT`` and ``VAR_IN_OUT``
  variables, the global variables without an address, and the
  variables at a ``%Q`` address.
* A variable at an ``%M`` address is in neither list.
* A ``CONSTANT`` variable, or a global the compiler declares such as
  ``__SYSTEM_UP_TIME``, is never an input, because the caller cannot
  change it.

Execution Tools
===============

compile
-------

Runs the full pipeline (parse → semantic analysis → code generation)
and stores the resulting bytecode container in the server's in-memory
cache. Only call :literal:`compile` when you need a compiled artifact
— for validation, use :literal:`check`, which is faster and produces
the same diagnostics.

**Inputs:**

* ``sources`` (required)
* ``options`` (required)
* ``include_bytes`` (boolean, optional, default ``false``) --- When
  ``true``, the response includes the compiled container as a
  base64-encoded string.

**Returns:** an object with ``ok``, a ``container_id`` string for
later reference, an optional ``container_base64`` when
``include_bytes`` is ``true``, arrays ``tasks`` and ``programs``
describing the compiled configuration, and ``diagnostics``: the line
``------ Build started: Application: NAME -------``, where *NAME* is the name of
the ``CONFIGURATION`` (the first one written when there are several, which the
build rejects; for a
program without one, the name of the first source without its extension), then
the problems found, then the line ``Build complete -- N errors, M warnings``,
which ends with ``: Ready for download`` or, when an error stops the build, with
``: No download possible``.

run
---

Executes a previously compiled container in the IronPLC virtual machine
for a span of simulated time, returning a time-ordered trace of the
variables you name. Use it to check that a program behaves correctly,
not merely that it compiles.

**Inputs:**

* ``container_id`` (string, required) --- The identifier returned by a
  prior :literal:`compile` call.
* ``duration_ms`` (number, required) --- How much simulated time to
  run. The number of scan cycles follows from each task's cycle time.
* ``freewheeling_interval_ms`` (number) --- The cycle time to run a
  task that declares no ``INTERVAL`` at. Required when the container
  has such a task. See :ref:`mcp-freewheeling-cycle-time` below.
* ``variables`` (array of strings, optional) --- Fully-qualified names
  to trace, such as ``Main.Counter``.
* ``trace_outputs`` (boolean, optional, default ``false``) --- Trace
  every observable output in addition to ``variables``.
* ``limits`` (object, optional) --- Tightens the server's resource
  limits for this call. It cannot loosen them.

**Returns:** an object with ``ok``, a ``trace`` array, a ``truncated``
flag, a ``terminated_reason``, a ``summary`` (final values, completed
cycles per task, and ``interval_ms`` --- the cycle time the run
executed at), and ``diagnostics``.

.. _mcp-freewheeling-cycle-time:

Programs with no task interval
~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

A task that declares no ``INTERVAL`` --- including the task the
compiler supplies for a program with no ``CONFIGURATION`` --- is
*freewheeling*: it runs as fast as the hardware allows. That rate is a
property of the machine, not of the program, so under simulated time
there is nothing to advance the clock by.

:literal:`run` will not choose a rate for you --- a scan time belongs to
the hardware the program would run on, and a trace built on a number
you never chose reads as fact. Running such a container without
``freewheeling_interval_ms`` returns ``ok: false`` and a diagnostic
naming the task. Supply the scan time you want to simulate, and the
response reports what it used as ``summary.interval_ms``, because every
timestamp in the trace --- and every timer in the program --- depends
on it.

The debugger is deliberately more forgiving: it assumes 100 ms and says
so on the console, because a debug session that refuses to start is a
poor answer to a missing setting. See :doc:`/reference/runtime/ironplcvmd`.

:literal:`compile` reports these tasks with ``kind: "freewheeling"``,
which is how you can tell in advance that the assumption applies.
Declare a ``CONFIGURATION`` with ``TASK plc_task(INTERVAL := T#100ms)``
when you want the program itself to fix the rate.

container_drop
--------------

Explicitly releases a compiled container from the server cache. Not
usually necessary — the cache evicts on LRU pressure automatically —
but useful for long-running connections that have finished with a
particular build.

**Inputs:**

* ``container_id`` (string, required) --- The identifier returned by
  a prior :literal:`compile` call.

**Returns:** an object with ``ok``, ``removed`` (``true`` when the
entry existed and was evicted), and ``diagnostics``.

Hot-edit Tools
==============

The six :literal:`hot_edit_*` tools run the online-change protocol of the
runtime: :literal:`hot_edit_accept` compiles sources and stages them as an edit
candidate, :literal:`hot_edit_test` runs the candidate at the next scan
boundary, :literal:`hot_edit_assemble` promotes it, :literal:`hot_edit_untest`
reverts a test, :literal:`hot_edit_cancel` discards the candidate and
:literal:`hot_edit_status` reports the state. By default they act on one session
that the server itself runs: the first :literal:`hot_edit_accept` starts it.

Every one of them takes an optional ``device`` that names a device to act on
instead, over the session protocol that the editor uses. Name the device by
``address`` and optionally ``port`` (the session port, 49152, when omitted), or
by ``device_id``, the id that :literal:`browse_network` reports, for a device
that IronLink has a route to on this workstation. Do not give both. The server
does not read the path file of a project, so an id without a route is an error
that tells you to browse.

With ``device``, :literal:`hot_edit_accept` stages the compiled program on the
device, which already runs a program, and the result is ``ack``. The response
has the same fields as without ``device`` and one more, ``device``: the id, name,
model, modification and runtime version that the device reported. The server does
not refuse a device for being another one than the id asked for, so compare
``device.deviceId`` with the id you meant. A device serves one engineering
session at a time: when another session is open, ``v_code`` is ``V6014``. A
device that cannot be reached returns a
:doc:`P8003 </reference/compiler/problems/P8003>` diagnostic.

Network Tools
=============

browse_network
--------------

Finds the controllers this workstation can see. The tool does not search the
network itself: it starts :program:`ironlink`, the IronLink program that ships
with IronPLC and is found beside :program:`ironplcmcp` or on ``PATH``, and asks
it to browse. IronLink broadcasts on the local network, asks the controller
addresses stored on this workstation and asks any address you give.

**Inputs:**

* ``addresses`` (array of strings, optional) --- Controller addresses (``IP`` or
  ``IP:PORT``) to ask directly, besides the broadcast and the stored address
  list.
* ``timeout_ms`` (number, optional) --- How long to wait for replies. IronLink's
  default is 1000 and its largest value is 60000.
* ``attempts`` (number, optional) --- How many times to send the request.
  IronLink's default is 2 and its largest value is 10.

**Returns:** an object with ``ok``, ``browse`` and ``diagnostics``. ``browse`` is
the browse result of IronLink: the ``workstation`` and, for each driver that ran,
its ``kind``, ``targets``, ``devices``, ``otherVersions`` and ``undecodable``.
A device is listed by device id, with the routes by which it answered and the
time of each answer. An answer says what the device was when it was seen, not
that it is reachable now. When IronLink refuses the request, ``ok`` is ``false``
and the object has ``l_code`` (for example ``L6004``) and ``message``. When
:program:`ironlink` is not found or does not answer, ``ok`` is ``false`` and a
:doc:`P8002 </reference/compiler/problems/P8002>` diagnostic says why.

See Also
========

* :doc:`overview` --- MCP server overview
* :doc:`ironplcmcp` --- Command-line reference
* :doc:`/reference/compiler/problems/index` --- Problem code index
  used by the :literal:`diagnostics` field and by
  :literal:`explain_diagnostic`
