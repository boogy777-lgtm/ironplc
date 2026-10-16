=======
PROGRAM
=======

A program is the top-level executable unit in IEC 61131-3. Programs are
instantiated within resources and scheduled by tasks.

.. list-table::
   :widths: 30 70

   * - **IEC 61131-3**
     - Section 2.5.3
   * - **Support**
     - Supported

Syntax
------

.. code-block:: bnf

   PROGRAM program_name
       variable_declarations
       statement_list
   END_PROGRAM

Example
-------

.. playground::

   PROGRAM main
       VAR
           counter : INT := 0;
       END_VAR

       counter := counter + 1;
   END_PROGRAM

Programs can contain local variables (``VAR``), input variables
(``VAR_INPUT``), output variables (``VAR_OUTPUT``) and temporary variables
(``VAR_TEMP``). Unlike functions, programs retain the values of their
``VAR``, ``VAR_INPUT`` and ``VAR_OUTPUT`` variables between execution cycles;
a ``VAR_TEMP`` variable starts again at every cycle (see
:ref:`temporary-variables`).

See Also
--------

- :doc:`function-block` — stateful callable unit
- :doc:`function` — stateless callable unit
- :doc:`task` — execution scheduling
- :doc:`configuration` — deployment container
