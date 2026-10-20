A code is the code of a warning, written as the list of
:doc:`problems </reference/compiler/problems/index>` writes it (for example
``P4083``) or as CODESYS writes it (for example ``C0354``). The case is exact:
``p4083`` is not a code. Only the code of a warning is accepted, because an
error cannot be disabled or reported as another category; a code that is unknown,
or is the code of an error, is refused with :doc:`P0013
</reference/compiler/problems/P0013>`, which names the code and lists the codes
that are accepted.
