=================
Special operators
=================

The CODESYS special operators that Structured Text programs can use:
``__NEW``, ``__DELETE``, ``__TYPEOF``, ``__XADD``, ``__CURRENTTASK``, and the
``__SYSTEM`` and ``__POOL`` scope prefixes.

.. list-table::
   :widths: 30 70

   * - **IEC 61131-3**
     - Not part of the standard (CODESYS extension)
   * - **Support**
     - Type-checked only (requires ``--allow-special-operators``)

IronPLC checks these operators the way CODESYS types them, so a program that
uses them can be analyzed. None of them has runtime behaviour in IronPLC: a
program that calls ``__NEW``, ``__DELETE``, ``__TYPEOF`` or ``__XADD`` passes
``ironplcc check`` but ``ironplcc compile`` refuses it with
:doc:`P9999 </reference/compiler/problems/P9999>`. ``__CURRENTTASK``,
``__SYSTEM`` and ``__POOL`` are refused by ``check`` itself.

Operators
---------

.. list-table::
   :header-rows: 1
   :widths: 22 38 20 20

   * - Operator
     - Operands
     - Type of the call
     - Support
   * - ``__NEW(T)``, ``__NEW(T, n)``
     - ``T`` a type; ``n`` an integer (elementary ``T`` only); the call is the
       value of an assignment
     - ``POINTER TO T``
     - Type-checked
   * - ``__DELETE(p)``
     - ``p`` a pointer
     - ``BOOL``
     - Type-checked
   * - ``__TYPEOF(x)``
     - a type, or an expression
     - ``INT``
     - Type-checked
   * - ``__XADD(p, v)``
     - ``p`` a ``POINTER TO DINT``; ``v`` an integer
     - ``DINT``
     - Type-checked
   * - ``__CURRENTTASK``
     -
     -
     - Not supported
   * - ``__SYSTEM.x``, ``__POOL.x``
     -
     -
     - Not supported

``__CURRENTTASK`` is a pointer to a structure that the target's system library
defines, and ``__SYSTEM.x`` and ``__POOL.x`` look ``x`` up in that library and
in the global pool. IronPLC has neither, so these three are recognised and
reported with :doc:`P4074 </reference/compiler/problems/P4074>`.

``__ISVALIDREF`` is a special operator too, and is described on its own page:
:doc:`isvalidref`.

Enabling
--------

The operators are a language extension and must be explicitly enabled, or
selected with the ``codesys`` dialect:

.. code-block:: shell

   ironplcc check --dialect codesys main.st

.. |flag| replace:: ``--allow-special-operators``
.. include:: /includes/enabled-by-flag.rst

Without the flag, the operators are ordinary undeclared names
(:doc:`P4017 </reference/compiler/problems/P4017>` and
:doc:`P4007 </reference/compiler/problems/P4007>`).

Example
-------

.. code-block::

   FUNCTION_BLOCK FB_Buffer
   VAR
       items : POINTER TO INT;
       freed : BOOL;
   END_VAR
       items := __NEW(INT, 16);   (* a pointer to INT *)
       freed := __DELETE(items);  (* BOOL *)
   END_FUNCTION_BLOCK

Restrictions
------------

- ``__NEW`` does not check the ``enable_dynamic_creation`` attribute that
  CODESYS requires on a function block or structure created with it.
- Assigning the result of ``__NEW(T)`` to a pointer to a different type is
  :doc:`P2032 </reference/compiler/problems/P2032>`, unless
  ``--allow-ref-type-punning`` is on, as it is in the ``codesys`` dialect.
- ``__DELETE`` is checked only for a pointer operand; the freed object is not
  tracked.

Related Problem Codes
---------------------

- :doc:`/reference/compiler/problems/P4073` — operands do not fit the operator
- :doc:`/reference/compiler/problems/P4074` — operator is recognised but not supported
- :doc:`/reference/compiler/problems/P9999` — operator cannot be compiled
