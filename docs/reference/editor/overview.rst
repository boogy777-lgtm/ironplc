========
Overview
========

The IronPLC extension brings IEC 61131-3 Structured Text support to your development environment.
It provides real-time feedback as you write code, helping you catch errors before
deployment.

Supported Languages
===================

IEC 61131-3 Structured Text
---------------------------

The primary language supported by IronPLC.

:File Extensions: :file:`.st`, :file:`.iec`

The extension provides:

* Syntax highlighting for keywords, types, comments, strings, and numbers
* Automatic bracket matching for block keywords (``IF``/``END_IF``, ``FUNCTION``/``END_FUNCTION``, etc.)
* Real-time diagnostic analysis

.. figure:: /images/screenshots/syntax-highlighting.png
   :alt: VS Code showing an IEC 61131-3 Structured Text file with syntax highlighting
   :width: 600px

   Structured Text with syntax highlighting in VS Code.

PLCopen XML
-----------

XML format for IEC 61131-3 projects as defined by PLCopen TC6.

The extension detects PLCopen XML files by looking for the PLCopen namespace
in the opening XML tags. It provides XML syntax highlighting with embedded
Structured Text support inside CDATA sections.

TwinCAT
-------

Beckhoff TwinCAT 3 project files containing IEC 61131-3 source code.

:File Extensions: :file:`.TcPOU`, :file:`.TcGVL`, :file:`.TcDUT`

The extension provides XML syntax highlighting with embedded Structured Text
support and real-time diagnostic analysis.

Commands
========

New Structured Text File
------------------------

Creates a new untitled file with the Structured Text language mode pre-selected.

:Menu Location: :menuselection:`File --> New File... --> Structured Text File`

To use this command:

1. Open the Command Palette (:kbd:`Ctrl+Shift+P` or :kbd:`Cmd+Shift+P`)
2. Type "New Structured Text File"
3. Press Enter

Choose Controller
-----------------

Chooses the controller of the project and remembers the choice in the project.

:Menu Location: :menuselection:`Command Palette --> IronPLC: Network: Choose Controller…`

See :doc:`choose-controller` for the list of controllers, the path record that
remembers the choice, and what the command needs from the IronLink extension.

Connect to Chosen Controller
----------------------------

Connects to the controller chosen for the project, by the route stored on this PC
or else the route hint of the project.

:Menu Location: :menuselection:`Command Palette --> IronPLC: Network: Connect to Chosen Controller`

See :doc:`choose-controller` for the order in which the address is found, and for
the status item that names the chosen controller.

.. _editor-go-online:

Go Online
---------

When a connection opens, the editor compares the controller with the project and
shows what follows. The comparison is made by :program:`ironplcc online-compare`
(:doc:`/reference/compiler/ironplcc`), which the editor runs with the answer of the
controller to ``identity``; the editor renders what the command prints and decides
nothing of it. Going online never refuses because the controller is a different
one.

:Menu Location: :menuselection:`Command Palette --> IronPLC: Show Online Comparison`

When the controller runs the last build of the project and the sources are those of
that build, the connection is online at once and no dialog opens. In every other
case a dialog (a Quick Pick, in the style of the :ref:`online edit dialog
<editor-online-edit>`) opens. Its title is the one-line summary of the situation.
Under it, lines say what the controller is and holds (its ID, runtime version, run
state, the application by its hash and the stage of an unfinished edit), what the
project has, the last download of the project to this controller, and the
conditions that stand beside the situation: another controller than the one last
chosen, a project that does not build, an unfinished online change, a controller
that restarted, a container format the controller refuses.

The items are the actions that follow. Each is enabled or disabled by the
comparison, and a disabled one shows its reason; selecting it does nothing.

.. list-table::
   :header-rows: 1
   :widths: 25 75

   * - Action
     - What it does
   * - :guilabel:`Monitor`
     - Stays connected and changes nothing. Enabled whenever the controller was
       reached.
   * - :guilabel:`Online change`
     - Opens :ref:`Online Edit <editor-online-edit>`. Enabled when the controller
       holds the last build or the last download, the project builds and the
       controller accepts an edit.
   * - :guilabel:`Download`
     - Runs :ref:`Download to Controller <editor-download>`. Enabled when the
       project builds and the controller accepts an edit, whatever the controller
       holds.
   * - :guilabel:`Search the network`
     - Searches the network for controllers with the IronLink extension.
   * - :guilabel:`Choose another controller`
     - Runs :guilabel:`IronPLC: Network: Choose Controller…`.
   * - :guilabel:`Retry`
     - Connects again. Enabled after a connection that did not open or was lost.
   * - :guilabel:`Resolve the unfinished edit`
     - Opens :ref:`Online Edit <editor-online-edit>` at the stage of the edit that
       is unfinished on the controller.
   * - :guilabel:`Upload`
     - Not available yet; always disabled.

A connection that fails opens the same dialog with the situation that applies: the
controller cannot be reached, it serves another session
(:doc:`V6014 </reference/runtime/problems/V6014>`), it is protected, or the
connection was lost.

A controller that holds neither the last build of the project nor what the project
last downloaded to it is a controller that this workstation did not load. An online change is not offered for
it, with or without a download record; :guilabel:`Download` is the way in, and it
writes the record that makes the next online change possible.

The status item at the bottom of the window shows the one-line summary of the
situation while a connection is open, and selecting it shows the dialog again. The
comparison is made again after a download, after an assemble from the online edit
dialog, when a lost connection is restored, and before the Accept stage of an
online edit.

Run Controller and Stop Controller
----------------------------------

Starts or stops the scan of the connected controller. The commands compile
nothing and leave an online edit where it is: they only send the ``startScan``
or ``stopScan`` command of the session (see :doc:`/reference/runtime/ironplcvm`).

:Menu Location: :menuselection:`Command Palette --> IronPLC: Run Controller`,
   :menuselection:`Command Palette --> IronPLC: Stop Controller`

The status item of the connection shows the run state the controller answered:
``running``, ``stopped``, or ``faulted`` with the problem code. A faulted
controller refuses Run with :doc:`V6019 </reference/runtime/problems/V6019>`;
restart the device to clear the fault. These commands differ from the Run lens
over a program, which compiles the program in the editor and runs it in the
language server.

.. _editor-online-edit:

Online Edit
-----------

Changes the running application of the connected controller in stages, in one
dialog that stays open. The command needs an open connection.

:Menu Location: :menuselection:`Command Palette --> IronPLC: Online Edit`

An online edit passes four stages: :guilabel:`Build` (the editor compiles the
program), :guilabel:`Accept` (the controller loads the compiled program as a
candidate and checks it against the running one), :guilabel:`Test` (the
controller runs the candidate while the original is kept) and :guilabel:`Assemble`
(the candidate becomes the running application). The dialog is a Quick Pick:

* The title names the stage of the edit on the controller and shows the step
  (for example ``3/4``). The line below the input marks each stage as done
  (``✔``), current (``●``) or still to do (``○``). A progress bar shows while a
  stage runs.
* :guilabel:`Next` runs the current stage and is labelled with it (Build, Accept,
  Test or Assemble).
* :guilabel:`Finalize all` runs the remaining stages in order and reads the state
  of the controller again between them. It is available when no candidate is
  staged; the controller refuses it otherwise.
* :guilabel:`Cancel` is the exit of the current stage. While the candidate is
  running it first returns to the original program, then discards the candidate.
  A candidate that migrates variables of a changed type, or a candidate sent by
  :ref:`Download to Controller <editor-download>`, cannot be reverted once it runs
  (:doc:`V4011 </reference/runtime/problems/V4011>`); :guilabel:`Assemble` is the only
  exit then. The description of a disabled item gives the reason, and selecting it
  does nothing.

The labels, which items are available and the reason for one that is not come from
the controller itself (the ``edit`` view of its status, described in
:ref:`ironplcvm-edit-view`); the editor adds none of its own. The one
thing the editor decides is that a build of the current sources succeeded in the
dialog: until then, :guilabel:`Next` is :guilabel:`Build` and
:guilabel:`Finalize all` starts with it. Editing a source file after the build
makes the build stale.

The dialog stays open when focus moves to another window. :kbd:`Esc` hides it and
leaves the candidate where it is; the status bar item :guilabel:`Online Edit`,
shown while a candidate exists, opens the dialog again. A candidate lives only in
the memory of the controller, so a restart of the controller loses it, and the
dialog says so while the candidate exists. A refused command is shown with the
problem code of the controller, and the dialog shows the state the controller
reports afterwards.

The Accept stage begins only when the go-online comparison allows an online change
(:ref:`editor-go-online`); when it does not, the editor says why in the words of the comparison and offers
:guilabel:`Download` when the comparison enables it. When the
controller acknowledges the assemble, the editor writes the download record of the
controller (:program:`ironplcc record-download`), as :ref:`Download to Controller
<editor-download>` does.

.. _editor-download:

Download to Controller
----------------------

Replaces the application on the connected controller with the project. The
command needs an open connection and sends the container in the replace mode of
the session, which :ref:`ironplcvm-replace` describes.

:Menu Location: :menuselection:`Command Palette --> IronPLC: Download to Controller`

The command runs these steps and stops at the first that fails:

#. It syncs the stable variable IDs of the project
   (:program:`ironplcc refactor sync-uids`), so the controller can match a
   ``PERSISTENT`` variable of the running application with its declaration in the
   new one. If the sync lists rename or swap candidates, you resolve them as for
   :guilabel:`IronPLC Hot Edit: Sync Variable IDs`; if you leave them open, nothing
   is sent.
#. It compiles the program once, in the :guilabel:`Output` channel of the build,
   and sends the container.
#. It shows a confirmation that names every ``PERSISTENT`` variable the controller
   will re-initialise and why, and says that the replacement cannot be undone once
   it runs. :guilabel:`Cancel` discards the candidate on the controller and the
   running application is untouched.
#. :guilabel:`Download` runs the steps that the controller offers next (test, then
   assemble). The editor does not decide which command is allowed: it follows the
   edit view that the controller reports, and a refusal is shown with the problem
   code of the controller and the state it reports.

The variables declared ``PERSISTENT`` whose declaration is equal keep their
values, and every other variable, a ``RETAIN`` variable included, starts again;
:doc:`/reference/language/variables/retention` states the rule. A project that
was never synced has no variable IDs, so the confirmation lists every
``PERSISTENT`` variable as having no stable variable ID. A download goes ahead
when the application on the controller is not the one the project last downloaded
to it, and the confirmation says that what was changed there is lost.

After the controller acknowledged the assemble, the editor writes the download
record of the controller (:program:`ironplcc record-download`) and keeps the
container that was sent beside it, as :file:`{STEM}.{ID}.download.iplc`. The names
are described with ``record-download`` in :doc:`/reference/compiler/ironplcc`.

Build Tasks
===========

The extension provides a build task that compiles your project to a bytecode
container (``.iplc``) file. Use :kbd:`Ctrl+Shift+B` to run the build task.
See :doc:`build-tasks` for details.

Bytecode Viewer
===============

Opening an :file:`.iplc` bytecode file displays a human-readable disassembly of
the compiled program, including the file header, constant pool, and function
instructions with color-coded opcodes. See :doc:`bytecode-viewer` for details.

.. figure:: /images/screenshots/bytecode-viewer.png
   :alt: VS Code custom editor showing disassembled IPLC bytecode with color-coded opcodes
   :width: 600px

   The bytecode viewer showing a disassembled :file:`.iplc` file.

Diagnostics
===========

The extension reports diagnostics from the IronPLC compiler in real-time:

* **Errors** (red underline): Code that will not compile
* **Warnings** (yellow underline): Code that may indicate problems
* **Information**: A note about valid code

A line of text about the build is not a problem of a file. It appears in the
:guilabel:`Output` channel of IronPLC and not in the :guilabel:`Problems` panel.

Each diagnostic includes a problem code (e.g., P0001) linking to documentation
that explains the issue and how to resolve it. See :doc:`/reference/compiler/problems/index`
for the complete list.

.. figure:: /images/screenshots/diagnostics-squiggles.png
   :alt: VS Code editor showing red and yellow diagnostic squiggles on invalid Structured Text code
   :width: 600px

   Diagnostics with error squiggles and problem codes.

Open the Problems panel (:kbd:`Ctrl+Shift+M` or :kbd:`⌘+Shift+M`) to see all
diagnostics in a list with their problem codes.

.. figure:: /images/screenshots/diagnostics-problems.png
   :alt: VS Code Problems panel listing IronPLC diagnostic errors with problem codes
   :width: 600px

   The Problems panel showing diagnostics with problem codes.

.. _message-areas:

Message areas
-------------

.. include:: ../../includes/message-areas.rst

Type ``IronPLC Precompile`` in the filter box of the Problems panel to list
only the messages of that area.

Run Program
===========

The extension adds a :guilabel:`Run Program` code lens above each ``PROGRAM``
declaration. Click it to compile and execute the program using the IronPLC
runtime.

.. figure:: /images/screenshots/run-program-code-lens.png
   :alt: VS Code editor showing the Run Program code lens above a PROGRAM declaration
   :width: 600px

   The Run Program code lens above a ``PROGRAM`` declaration.

While the program is running, the code lens changes to :guilabel:`Stop` and
:guilabel:`Pause` controls.

.. figure:: /images/screenshots/run-program-running.png
   :alt: VS Code editor showing Stop and Pause code lenses while a program is running
   :width: 600px

   Stop and Pause controls replace Run Program while the program executes.

Debugging
=========

The extension debugs Structured Text programs. Set a breakpoint in the gutter
and press :kbd:`F5` to pause the program on that line, step through it, and
inspect variables by name and type.

:guilabel:`Run Program` and debugging answer different questions.
:guilabel:`Run Program` executes the program and shows you what it produced.
The debugger stops the program mid-scan and shows you what it is doing.

.. include:: /includes/debugging-in-development.rst

See :doc:`debugging` for the launch configuration, the supported debug
actions, and the current limits.
