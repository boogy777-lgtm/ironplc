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
     - Value is preserved across power cycles
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

What does not work yet
----------------------

- :program:`ironplcvm run` starts every variable with its
  initial value.
- A download of a different program does not keep ``PERSISTENT`` variables.
  Only ``assembleEdits`` carries state to a new program.

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
