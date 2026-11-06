=========
Retention
=========

Retention qualifiers control whether variables preserve their values
across power cycles and program restarts.

:program:`ironplcvm serve` keeps ``RETAIN`` and ``PERSISTENT`` variables across
a restart of the device. :program:`ironplcvm run` does not.

.. list-table::
   :widths: 30 70

   * - **IEC 61131-3**
     - Section 2.4.3
   * - **Support**
     - Partial

Qualifiers
----------

.. list-table::
   :header-rows: 1
   :widths: 25 75

   * - Qualifier
     - Description
   * - ``RETAIN``
     - Value is preserved across power cycles
   * - ``PERSISTENT``
     - Value is preserved across power cycles and across a download whose
       declaration of the variable is equal
   * - ``NON_RETAIN``
     - Value is reset to initial value on restart
   * - ``CONSTANT``
     - Value cannot be modified after initialization

What works
----------

On :program:`ironplcvm serve`, the values of the ``RETAIN`` and ``PERSISTENT``
variables of global lists, of programs and of function block instances survive a
restart of the device. A function block instance declared ``RETAIN`` keeps all its
data. An instance without a qualifier keeps its ``RETAIN`` and ``PERSISTENT``
fields. ``NON_RETAIN`` on a declaration keeps nothing of it, whatever its
fields declare.

The device writes the values when it stops, when ``assembleEdits`` commits a new
program, and, if you set it, every number of seconds. The device applies them
only to the program they were written for: after a different program starts, the
variables take their initial values and the ``identity`` answer says why. See
:ref:`ironplcvm-retained-values` for the file, the setting, and the answer.

Across a download
-----------------

A download replaces the program on the device. :program:`ironplcvm serve` keeps
the value of a ``PERSISTENT`` variable across it when the declaration is equal in
the program that runs and in the program that is downloaded: the same variable,
with the same ``PERSISTENT`` qualifier and the same type, string length, array
bounds or function block type. Every other variable starts again at its initial
value, a ``RETAIN`` variable included: ``RETAIN`` survives a restart and not a
download. This is how CODESYS behaves.

The device recognises the same variable by its stable variable ID, which the
project keeps in the file :file:`{name}.uids.json` (create and update it with
``ironplcc refactor sync-uids``). A ``PERSISTENT`` variable without an ID in the
running program, or in the downloaded one, starts again. The answer to the
download lists every ``PERSISTENT`` variable of the running program that starts
again, and why, so a reset is never silent; see :ref:`ironplcvm-replace`.

The rule is by declaration. A variable whose meaning changes while its
declaration stays equal keeps its old value, as in CODESYS: change the name of the
variable, or give it a new type, when the old value must not be kept.

What does not work yet
----------------------

- :program:`ironplcvm run` starts every variable with its
  initial value.
- A ``PERSISTENT`` field of a function block instance that is itself declared
  without a qualifier is not kept by a download. Declare the instance
  ``PERSISTENT`` to keep all its data.

Example
-------

.. code-block::

   PROGRAM main
       VAR RETAIN
           run_hours : DINT := 0;
       END_VAR
       VAR CONSTANT
           MAX_TEMP : INT := 150;
       END_VAR
       VAR
           current_temp : INT;
       END_VAR

       run_hours := run_hours + 1;
   END_PROGRAM

See Also
--------

- :doc:`declarations` — basic variable syntax
- :doc:`initial-values` — initialization
