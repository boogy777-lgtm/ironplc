===
BIT
===

One-bit value: ``TRUE`` or ``FALSE``. ``BIT`` is a CODESYS/TwinCAT
extension — IEC 61131-3 has no one-bit elementary type — so it is enabled by
``--allow-bit-type``; without the flag ``bit`` is an ordinary identifier.

.. list-table::
   :widths: 30 70

   * - **Size**
     - 1 bit, stored as a ``BOOL``
   * - **Values**
     - ``TRUE``, ``FALSE``
   * - **Default**
     - ``FALSE``
   * - **IEC 61131-3**
     - Not defined (dialect extension)
   * - **Support**
     - Supported with ``--allow-bit-type``

The compiler stores a ``BIT`` as a ``BOOL``: the two types take the same
values and can be assigned to one another.

Literals
--------

.. code-block::

   BIT#1   (* TRUE *)
   BIT#0   (* FALSE *)

Example
-------

.. code-block::

   PROGRAM main
   VAR
       flag : BIT;
       set : BOOL;
   END_VAR
       flag := BIT#1;
       set := flag;   (* set = TRUE *)
   END_PROGRAM

See Also
--------

- :doc:`bool` — boolean value
- :doc:`/reference/compiler/ironplcc` — the ``--allow-bit-type`` flag
