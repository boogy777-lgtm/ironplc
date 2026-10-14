==========
Attributes
==========

An attribute is a pragma that describes the declaration it stands before. It is
written as ``{attribute 'name'}`` or as ``{attribute 'name' := 'value'}``.
CODESYS, TwinCAT and the other implementations built on the CODESYS V3 runtime
write attributes before types, structure members, variables and program
organisation units.

.. code-block::

   {attribute 'pack_mode' := '2'}
   TYPE Packet :
       STRUCT
           a : INT;
       END_STRUCT;
   END_TYPE

   PROGRAM main
   VAR
       {attribute 'hide'}
       p : Packet;
   END_VAR
   END_PROGRAM

Attributes are a CODESYS and TwinCAT extension. The compiler reads them only
when pragmas are allowed. See :doc:`/explanation/enabling-dialects-and-features`.

Where an attribute stands
=========================

An attribute stands directly before the declaration it describes. Comments and
other pragmas may stand between the two. Several attributes before one
declaration all describe it.

An attribute before a :code:`TYPE` block or before a variable block
(:code:`VAR ... END_VAR` of any kind) describes every declaration the block
holds. One attribute before :code:`VAR a, b : INT; END_VAR` describes both
variables.

An attribute that stands anywhere else describes nothing, and the compiler
reports it as :doc:`P0043 </reference/compiler/problems/P0043>`.

What the compiler reports
=========================

The compiler knows the attributes in the table below: where each stands and what
value it takes. It reports an attribute that it knows and that is not written
as the table says.

.. list-table::
   :header-rows: 1
   :widths: 15 85

   * - Problem
     - Cause
   * - :doc:`P0043 </reference/compiler/problems/P0043>`
     - The attribute does not stand before a declaration.
   * - :doc:`P4076 </reference/compiler/problems/P4076>`
     - The attribute stands before a kind of declaration it does not apply to.
   * - :doc:`P4077 </reference/compiler/problems/P4077>`
     - The attribute has a value it does not take, or no value where it needs
       one.
   * - :doc:`P4078 </reference/compiler/problems/P4078>`
     - The attribute is written more than once before one declaration. A name
       and its alias are the same attribute, and names are compared without
       regard to case.

Names and aliases are compared without regard to case. So is the value of an
attribute that takes one of a list of values.

User-defined attributes
=======================

A name that is not in the table is a user-defined attribute. The compiler
accepts it before any declaration, with any value or none, and keeps it as
written. It reports it only when the same name is written twice before one
declaration.

Known attributes
================

Each row of the table is an attribute that the compiler knows. The columns say
what the attribute is called, where it stands, what it takes, and what it does
in CODESYS. The last column is the state of the attribute in IronPLC.

State :code:`Waiting` means the compiler reads and checks the attribute and does
nothing else with it yet. The attribute does not change the analysis or the
code that the compiler generates.

.. csv-table::
   :file: attributes-table.csv
   :header-rows: 1
   :widths: 10 14 12 16 14 26 8
