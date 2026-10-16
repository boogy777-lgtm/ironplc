==============
FUNCTION_BLOCK
==============

A function block is a stateful callable unit with input and output
parameters. Function block instances retain their state between calls.

.. list-table::
   :widths: 30 70

   * - **IEC 61131-3**
     - Section 2.5.2
   * - **Support**
     - Partial

Syntax
------

.. code-block:: bnf

   FUNCTION_BLOCK fb_name
       variable_declarations
       statement_list
   END_FUNCTION_BLOCK

Example
-------

.. code-block::

   FUNCTION_BLOCK Counter
       VAR_INPUT
           reset : BOOL;
       END_VAR
       VAR_OUTPUT
           count : INT;
       END_VAR
       VAR
           internal : INT := 0;
       END_VAR

       IF reset THEN
           internal := 0;
       ELSE
           internal := internal + 1;
       END_IF;
       count := internal;
   END_FUNCTION_BLOCK

Using a Function Block
----------------------

Function blocks must be instantiated as variables before use:

.. code-block::

   PROGRAM main
       VAR
           my_counter : Counter;
           value : INT;
       END_VAR

       my_counter(reset := FALSE);
       value := my_counter.count;
   END_PROGRAM

Outputs are accessed using dot notation on the instance.

A function block can declare temporary variables (``VAR_TEMP``). Unlike the
variables in ``VAR``, a temporary variable does not keep its value in the
instance: it starts again at every call (see :ref:`temporary-variables`).

Access from Outside
-------------------

What a program may do with the variables of an instance depends on the section
that declares them. It may write the inputs, and read the inputs, outputs and
internal variables (``VAR``, ``VAR_STAT`` and ``VAR_INST``). It may not assign an
output or an internal variable (:doc:`/reference/compiler/problems/P4081`), and
it may neither read nor assign a ``VAR_IN_OUT`` (:doc:`/reference/compiler/problems/P4082`);
a ``VAR_IN_OUT`` receives its variable in the call, ``counter(value := reading)``.
This holds for every way of changing a variable: an assignment, the target of
``out => variable`` in a call, and a variable passed to a ``VAR_IN_OUT``
parameter. It does not apply to the function block's own code: its body, its
methods, and the body and methods of a function block that extends it. A
``VAR_TEMP`` variable belongs to one call and cannot be reached from outside at all
(:doc:`/reference/compiler/problems/P4080`). This is how CODESYS treats them too.

Arrays of Instances
-------------------

With the :doc:`CODESYS or TwinCAT dialect </explanation/enabling-dialects-and-features>`
or ``--allow-fb-instance-arrays``, an array can hold instances. An element is
called, read and written like any instance, and each element has its own state:

.. code-block::

   PROGRAM main
       VAR
           counters : ARRAY[0..2] OF Counter;
           i : DINT;
           total : INT;
       END_VAR

       FOR i := 0 TO 2 DO
           counters[i](reset := FALSE);
           total := total + counters[i].count;
       END_FOR;
   END_PROGRAM

An index outside the array stops the program, as it does for any array. A
function block with a ``STRING`` or ``WSTRING`` field, and starting values for
the elements of the array, are not supported yet.

See Also
--------

- :doc:`function` — stateless callable unit
- :doc:`program` — top-level executable unit
- :doc:`/reference/standard-library/function-blocks/index` — standard function blocks
