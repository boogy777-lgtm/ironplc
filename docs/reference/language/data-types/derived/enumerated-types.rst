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

Writing the Type with the Value
-------------------------------

A value can carry the name of its type: ``Motor#Running`` is the value
``Running`` of the enumeration ``Motor``. IEC 61131-3 defines this form, so every
dialect accepts it and no option enables it. It stands wherever a value stands:
an initial value, an assignment, a comparison, an argument of a call, a
``CASE`` label, and an element of an array or structure initializer.

Two enumerations can declare the same value name. A bare ``Running`` is then
refused (:doc:`P2042 </reference/compiler/problems/P2042>`), and ``Motor#Running``
and ``Pump#Running`` name one value each:

.. playground::

   TYPE
       Motor : (Idle, Running, Fault);
       Pump : (Idle, Priming, Running);
   END_TYPE

   PROGRAM main
       VAR
           m : Motor;
           p : Pump := Pump#Running;
           busy : BOOL;
       END_VAR

       m := Motor#Running;
       busy := m = Motor#Running;
       CASE p OF
           Pump#Idle: busy := FALSE;
           Pump#Running: busy := TRUE;
       END_CASE;
   END_PROGRAM

After one scan ``m`` is ``Running`` (1), ``p`` is ``Running`` (2) and ``busy`` is
``TRUE``.

The type before the ``#`` must be declared (:doc:`P2008
</reference/compiler/problems/P2008>`), must be an enumeration (:doc:`P2004
</reference/compiler/problems/P2004>`) and must declare the value (:doc:`P2006
</reference/compiler/problems/P2006>`). The check is the same wherever the value
is written. Assigning a value of one enumeration to a variable of another
reports :doc:`P4035 </reference/compiler/problems/P4035>`.

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

Whether an enumeration is a number depends on whether it is *strict*. A strict
enumeration holds its own values, and an integer is not one of them unless it
is constant and names one. An enumeration that is not strict is its base type
(``INT`` unless the declaration names another) in every place described below.

.. list-table::
   :header-rows: 1
   :widths: 40 30 30

   * - Dialect
     - Declaration
     - The enumeration is
   * - ``iec61131-3-ed2``, ``iec61131-3-ed3``, ``rusty``
     - any
     - strict
   * - ``codesys``, ``twincat``
     - has ``{attribute 'strict'}``
     - strict
   * - ``codesys``, ``twincat``
     - has no attribute
     - not strict

The standard gives the values of an enumeration no arithmetic, so every
enumeration is strict unless the dialect says otherwise. CODESYS and TwinCAT
mark the enumerations that refuse arithmetic with ``{attribute 'strict'}``
before the declaration, as in ``{attribute 'strict'} TYPE Mode : (Auto, Manual);
END_TYPE``, and the ``codesys`` and ``twincat`` dialects follow them. The
dialect only chooses a default: ``--policy-enumeration-strictness`` replaces it
(see :doc:`/reference/compiler/ironplcc`). An enumeration written inside a
variable declaration (``state : (Idle, Running);``) has no declaration to
carry the attribute, so it is strict only when every enumeration is. An alias
(``TYPE Mode2 : Mode;``) is strict when the enumeration it names is.

The rules below are those of a strict enumeration. For one that is not strict,
each place accepts the enumeration as it would an ``INT``: arithmetic on it is
accepted and has the type of its base type, so ``light + 1`` is an ``INT`` and
wraps as an ``INT`` does; a number is stored into it as into a variable of the
base type, whether or not it is a constant and whether or not it names a value;
the numerical functions and the control variable of ``FOR`` accept it; and a
selection function returns its base type.

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
  ``-light`` report :doc:`P4049 </reference/compiler/problems/P4049>`. So does
  every place that computes a new number from the value:

  - the function forms ``ADD``, ``SUB``, ``MUL`` and ``DIV``
    (:doc:`P4049 </reference/compiler/problems/P4049>`);
  - the numerical functions ``ABS``, ``SQRT``, ``LN``, ``LOG``, ``EXP``,
    ``SIN``, ``COS``, ``TAN``, ``ASIN``, ``ACOS``, ``ATAN``, ``ATAN2``,
    ``TRUNC`` and ``EXPT``, the function form ``MOD(a, b)`` and the time
    products and quotients ``MUL_TIME`` and ``DIV_TIME``
    (:doc:`P4026 </reference/compiler/problems/P4026>` for the enumeration
    argument);
  - the control variable of ``FOR``, which the statement increments on every
    pass (:doc:`P4049 </reference/compiler/problems/P4049>`).

  A place that only holds the value takes the number the enumeration stands
  for: an assignment, a parameter of an integer type, a comparison (``=``,
  ``<``, ``GT``, ...), the selection functions ``SEL``, ``MAX``, ``MIN``,
  ``LIMIT`` and ``MUX``, ``MOVE``, the conversions (``INT_TO_DINT``,
  ``INT_TO_BCD``, ...) and the bounds of a ``FOR``.
- The result of a selection function is an enumeration only when every value it
  selects among is that enumeration. ``SEL(flag, Red, Green)`` and
  ``MAX(light, other)`` give a ``TrafficLight`` back when both are one. When the
  values are an enumeration and a number, or two different enumerations, each
  stands as its number and the result is a number, so storing it into an
  enumeration is refused like any integer that is not a constant
  (:doc:`P4027 </reference/compiler/problems/P4027>`): ``light := MAX(light, 1)``
  is an error, and ``number := MAX(light, 1)`` is not.

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

.. playground::

   TYPE
       TrafficLight : (Red, Yellow, Green);
   END_TYPE

   PROGRAM main
       VAR
           light : TrafficLight := Green;
           other : TrafficLight := Yellow;
           flag : BOOL;
           chosen : TrafficLight;
           number : DINT;
       END_VAR

       chosen := SEL(flag, light, other);
       number := MAX(light, 1);
   END_PROGRAM

After one scan ``chosen`` is ``Green`` and ``number`` is 2. Replacing the last
statement with ``chosen := MAX(light, 1);`` reports
:doc:`P4027 </reference/compiler/problems/P4027>`, because the result is an
``INT``.

A dialect that makes the enumeration not strict accepts the arithmetic. Under
``--dialect codesys``, ``light`` below is not strict and the program counts from
``Red`` to ``Green``; adding ``{attribute 'strict'}`` before the declaration
makes ``light + 1`` an error again.

.. code-block::

   TYPE
       TrafficLight : (Red, Yellow, Green);
   END_TYPE

   PROGRAM main
       VAR
           light : TrafficLight := Yellow;
           next : TrafficLight;
           count : INT;
       END_VAR

       next := light + 1;
       FOR light := Red TO Green DO
           count := count + 1;
       END_FOR;
   END_PROGRAM

After one scan ``next`` is ``Green`` and ``count`` is 3.

Related Problem Codes
---------------------

- :doc:`/reference/compiler/problems/P2003` — Duplicate enumeration value
- :doc:`/reference/compiler/problems/P2004` — Enumeration is not declared
- :doc:`/reference/compiler/problems/P2006` — Enumeration uses value that is
  not defined in the enumeration
- :doc:`/reference/compiler/problems/P2008` — Cannot determine kind of type
  identifier
- :doc:`/reference/compiler/problems/P2042` — Enumeration value name is
  declared by more than one enumeration
- :doc:`/reference/compiler/problems/P4026` — Function call argument type does
  not match parameter type
- :doc:`/reference/compiler/problems/P4027` — Function return type does not
  match assignment destination type
- :doc:`/reference/compiler/problems/P4035` — Assignment value type does not
  match assignment target type
- :doc:`/reference/compiler/problems/P4049` — Operator is not defined for the
  operand types
- :doc:`/reference/compiler/problems/P4055` — Explicit enumeration member
  value requires a dialect or flag
- :doc:`/reference/compiler/problems/P4056` — Enumeration base-type suffix
  requires a dialect or flag

See Also
--------

- :doc:`subrange-types` — restrict an integer to a range
