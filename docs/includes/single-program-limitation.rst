.. note::

   IronPLC currently runs one ``PROGRAM`` per configuration, and builds one
   ``CONFIGURATION`` per container, because a container is one PLC. Declaring a
   second ``PROGRAM``, instantiating a program more than once across the
   ``RESOURCE`` blocks, or declaring a second ``CONFIGURATION`` is reported as
   :doc:`/reference/compiler/problems/P9999` at compile time. Support for more
   than one program is tracked in
   `issue #1613 <https://github.com/ironplc/ironplc/issues/1613>`_.
