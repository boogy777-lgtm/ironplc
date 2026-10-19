``severity`` and ``severity_value``
   The category of the message: ``"FatalError"`` (1), ``"Error"`` (2),
   ``"Warning"`` (4), ``"Information"`` (8), ``"Text"`` (16),
   ``"SuppressedWarning"`` (32) or ``"SuppressedInformation"`` (64). A
   suppressed message is included.

``prefix`` and ``number``
   The problem code, documented in :doc:`/reference/compiler/problems/index`.
   The code ``P0042`` is ``"P"`` and 42.

``text``
   The message.

``position`` and ``length``
   The 0-indexed byte offset where the message starts in the source text, and
   the number of bytes it covers.

``position_text``
   The place for a person, ``file:line:column``, counted from 1. It is
   ``null`` when the message is not about one of the supplied sources.

``object``
   The name of the source file.

``codesys``
   The code of the same condition in CODESYS, such as ``"C0046"``, or
   ``null`` when there is none. CODESYS has no such field.
