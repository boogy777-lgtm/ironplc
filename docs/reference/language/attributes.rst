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

State :code:`Honoured` means a part of the compiler acts on the attribute.
:ref:`attributes-honoured` says what it does and what it does not do.

.. csv-table::
   :file: attributes-table.csv
   :header-rows: 1
   :widths: 10 14 12 16 14 26 8

.. _attributes-honoured:

Attributes the compiler acts on
===============================

pack_mode
---------

:code:`pack_mode` bounds the alignment of the members of a structure or a union.
The value is the largest alignment in bytes, one of 0, 1, 2, 4 and 8, and 0
means the same as 1. A member stands at an offset that is a multiple of the
smaller of the alignment of the member and the value, so the value is an upper
bound on the alignment of every member. The alignment of the structure itself is
the smaller of the largest alignment of its members and the value. A structure
without the attribute aligns each member to its own alignment.

.. code-block::

   {attribute 'pack_mode' := '1'}
   TYPE Packet :
       STRUCT
           flag : BYTE;
           value : LWORD;
       END_STRUCT;
   END_TYPE

   PROGRAM main
   VAR
       p : Packet;
       size : DINT;
   END_VAR
       size := SIZEOF(p);   (* 9; without the attribute it is 16 *)
   END_PROGRAM

The attribute describes the **byte image** of the structure: the offsets of its
members and the size that :code:`SIZEOF` answers. It does not change how the
virtual machine stores the structure. The virtual machine stores each member in
whole 8-byte slots whatever the value, and the code that the compiler generates
for the members is the same with and without the attribute.

:code:`SIZEOF` of a structure is the end of its last member, without padding to
the alignment of the structure. Where a structure repeats or nests, it occupies
its size padded to its alignment: this is the stride of an element of an array
and the space of a member that is itself a structure. The padding at the end of
a structure is a decision that has not been checked against CODESYS yet; it can
change.

:code:`SIZEOF` takes a variable. It does not take the name of a type.

Before an :code:`ARRAY` type the attribute is accepted and has no effect: an
array has no members to align. Before a :code:`VAR_GLOBAL` block it is reported
as :doc:`P4076 </reference/compiler/problems/P4076>`, because a variable block
gives the attribute to each variable, and a variable is not a structure.

strict
------

:code:`strict` before an enumeration declaration makes the enumeration strict: it
is no number. The compiler refuses arithmetic on a value of it (``x + 1``,
:code:`ADD(x, 1)`), the numerical functions (:code:`ABS(x)`), a :code:`FOR`
statement that counts it, and the storing of a number into it that is not one of
its values (a variable of an integer type, the result of :code:`MAX(x, 1)`, a
constant that names no value). Comparison, and an enumeration where an
:code:`INT` is expected, are accepted as they are without the attribute.

.. code-block::

   {attribute 'strict'}
   TYPE Mode : (Auto, Manual); END_TYPE

   PROGRAM main
   VAR
       m : Mode := Auto;
   END_VAR
       m := m + 1;   (* P4049 *)
   END_PROGRAM

Without the attribute the enumeration is its base type (:code:`INT`, unless the
declaration names another) in all of these places, and the program above runs.
Whether the attribute is read at all depends on the dialect: the
:code:`codesys` and :code:`twincat` dialects read it, and every other dialect
holds every enumeration strict whether or not it has the attribute. The policy
:code:`--policy-enumeration-strictness` replaces the dialect's choice. An
enumeration written inside a variable declaration has no declaration to carry the
attribute, and an alias is strict when the enumeration it names is. See
:doc:`/reference/language/data-types/derived/enumerated-types`.

subsequent
----------

:code:`subsequent` asks for the variables of a program, or of a global variable
list, to be allocated one after another in memory. The compiler allocates the
variables of a program and of a global variable list in the order in which they
are declared, in consecutive entries of the variable table, with or without the
attribute. So the attribute needs no other action.

When the list changes, CODESYS allocates the whole list to a new memory area; an
online change of IronPLC does not model this. The compiler has no memory for
retained variables either: a :code:`RETAIN` variable
of a list with the attribute is not stored apart from the other variables of the
list. CODESYS stores all variables of the list in the retained memory then.

CODESYS reports a :code:`VAR_TEMP` block in a program with the attribute as an
error. The compiler does not accept a :code:`VAR_TEMP` block in a program at all,
with or without the attribute, and reports it as a syntax error
(:doc:`P0002 </reference/compiler/problems/P0002>`).
