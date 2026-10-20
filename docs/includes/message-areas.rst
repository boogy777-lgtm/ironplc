The messages are kept in separate lists, the *areas*, as the CODESYS Messages
view does, and a message names its area as its source:

``IronPLC Build``
   The messages of a compile or a build: ``ironplcc check`` and
   ``ironplcc compile`` write them to the terminal, with the lines that start
   and end the run and the lines that report a limit. The MCP tools and the
   playground read them too.

``IronPLC Precompile``
   The messages of the check that the language server makes while you edit.
   Only a message that has a position in the source is shown, so the lines
   that count the messages of a run are not. The area holds at most 950
   messages, 950 for each file at most. When there are more, the messages of
   the files written first are dropped first, and the file you edited last is
   written last.

A warning can be disabled, or reported as an error, by its code in the settings
of a run (``--disable-warning`` and ``--warning-as-error`` of
:program:`ironplcc`, the options ``disabled_warnings`` and ``warnings_as_errors``
of the MCP tools, and ``ironplc.disabledWarnings`` and
``ironplc.warningsAsErrors`` in VS Code). A disabled warning is in neither area.
A warning reported as an error is an error in the ``IronPLC Build`` area, and
stays a warning in the ``IronPLC Precompile`` area, as in the CODESYS Messages
view.

In VS Code the ``IronPLC Precompile`` area is the :guilabel:`Problems` panel,
and the ``IronPLC Build`` area is the :guilabel:`Output` channel of the same
name. The command :guilabel:`IronPLC: Build` and the device build
(:guilabel:`IronPLC: Build & Commit`) write that channel; each clears it when
it starts, as a CODESYS build clears its Build list. Select the channel in the
drop-down of the :guilabel:`Output` panel, as you select the area in the
drop-down of the CODESYS Messages view.
