================
Enumerated Types
================

An enumerated type defines a named set of values.

.. list-table::
   :widths: 30 70

   * - **IEC 61131-3**
     - Section 2.3.3.1
   * - **Support**
     - Supported

Syntax
------

.. code-block:: bnf

   TYPE
       type_name : ( value1, value2, ... ) ;
   END_TYPE

Example
-------

.. playground::

   TYPE
       TrafficLight : (Red, Yellow, Green);
   END_TYPE

   PROGRAM main
       VAR
           state : TrafficLight := Red;
       END_VAR

       IF state = Green THEN
           state := Yellow;
       END_IF;
   END_PROGRAM

Member names must be unique within the type. The members take consecutive
values starting at zero, so ``Red`` is 0, ``Yellow`` is 1 and ``Green`` is 2.

Explicit Values
---------------

.. include:: ../../../../includes/requires-edition3.rst

A member can be given its own value instead of the one its position implies.
Members that follow continue from the value before them, so ``Type_ANY`` below
is 1 and ``Type_BOOL`` is 2:

.. playground::
   :allows: enum-explicit-values

   TYPE
       E_AssertionType : (Type_UNDEFINED := 0, Type_ANY, Type_BOOL);
   END_TYPE

   PROGRAM main
       VAR
           kind : E_AssertionType := Type_ANY;
       END_VAR

       IF kind = Type_ANY THEN
           kind := Type_BOOL;
       END_IF;
   END_PROGRAM

Values are not checked for uniqueness: ``(A := 1, B := 1)`` gives two names
for the same value and is accepted. Only the *names* must differ.

Base Type (Language Extension)
------------------------------

.. include:: ../../../../includes/requires-dialect-extension.rst

A declaration can name the elementary type the members are stored in:

.. playground::
   :allows: enum-base-type

   TYPE
       Color : (Red, Green, Blue) INT;
   END_TYPE

   PROGRAM main
       VAR
           shade : Color := Blue;
       END_VAR

       IF shade = Blue THEN
           shade := Red;
       END_IF;
   END_PROGRAM

Without it, IronPLC picks the smallest type that holds every member's value.

Integers and Enumerations
-------------------------

IronPLC follows the strict enumeration rules of CODESYS (``{attribute 'strict'}``).
An enumeration holds its own values, and an integer is not one of them unless
it is constant and names one.

- An integer constant is accepted where an enumeration is stored when it is
  the number of one of its values: ``Red`` is 0, ``Yellow`` is 1 and ``Green``
  is 2 above, so ``2`` is accepted and ``7`` is not (:doc:`P2006
  </reference/compiler/problems/P2006>`). A member with an explicit value is
  numbered by that value, so ``(A := 10, B := 20)`` accepts ``10`` and ``20``
  and neither ``0`` nor ``15``. The rule is the same wherever a value is
  stored: an assignment, an initial value, the default of a type or of a
  structure member, an element of an array or structure initializer, the
  argument of a function or of a function block input, and the result of a
  function.
- An integer that is not a constant is refused. ``light := count`` and
  ``light := count + 1`` store an ``INT`` into an enumeration, and the compiler
  reports :doc:`P4035 </reference/compiler/problems/P4035>` for an assignment
  and :doc:`P4026 </reference/compiler/problems/P4026>` for a function
  argument.
- An enumeration is accepted where an integer is expected. Its base type is
  ``INT``, unless the declaration names another (see above), so it is accepted
  wherever an ``INT`` is: for an ``INT`` or ``DINT`` target, and for an ``INT``
  or ``DINT`` parameter. A narrower target such as ``SINT`` is refused. The
  number is the value's number: ``2`` for ``Green``.
- Arithmetic on an enumeration is refused. ``light + 1``, ``light * 2`` and
  ``-light`` report :doc:`P4049 </reference/compiler/problems/P4049>`.

.. playground::

   TYPE
       TrafficLight : (Red, Yellow, Green);
   END_TYPE

   PROGRAM main
       VAR
           light : TrafficLight;
           number : DINT;
       END_VAR

       light := 2;
       number := light;
   END_PROGRAM

After one scan ``light`` is ``Green`` and ``number`` is 2.

Related Problem Codes
---------------------

- :doc:`/reference/compiler/problems/P2003` — Duplicate enumeration value
- :doc:`/reference/compiler/problems/P2006` — Enumeration uses value that is
  not defined in the enumeration
- :doc:`/reference/compiler/problems/P4055` — Explicit enumeration member
  value requires a dialect or flag
- :doc:`/reference/compiler/problems/P4056` — Enumeration base-type suffix
  requires a dialect or flag

See Also
--------

- :doc:`subrange-types` — restrict an integer to a range
