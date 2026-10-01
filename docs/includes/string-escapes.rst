A ``$`` inside a literal starts an escape. Each escape is one character of
the string:

.. list-table::
   :header-rows: 1
   :widths: 30 70

   * - Escape
     - Character
   * - ``$$``
     - Dollar sign
   * - ``$'``
     - Single quote
   * - ``$"``
     - Double quote
   * - ``$L`` or ``$N``
     - Line feed
   * - ``$R``
     - Carriage return
   * - ``$P``
     - Form feed
   * - ``$T``
     - Tab
   * - ``$`` and two hex digits (``STRING``)
     - The character with that code, for example ``$41`` is ``A``
   * - ``$`` and four hex digits (``WSTRING``)
     - The character with that code, for example ``$20AC`` is ``€``
   * - ``$U`` and eight hex digits
     - The Unicode character with that code, for example ``$U000020AC`` is ``€``

The letters may be lower case, except ``U``. A code from ``$80`` to ``$FF``
is the character Windows-1252 gives that byte, as it is in CODESYS, so
``$80`` is ``€``. Any other escape is error :doc:`P0012 </reference/compiler/problems/P0012>`.
