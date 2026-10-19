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

In VS Code the ``IronPLC Precompile`` area is the :guilabel:`Problems` panel,
and the ``IronPLC Build`` area is the :guilabel:`Output` channel of the same
name. The command :guilabel:`IronPLC: Build` and the device build
(:guilabel:`IronPLC: Build & Commit`) write that channel; each clears it when
it starts, as a CODESYS build clears its Build list. Select the channel in the
drop-down of the :guilabel:`Output` panel, as you select the area in the
drop-down of the CODESYS Messages view.
