========
ironlink
========

Name
====

ironlink --- find IronPLC controllers on the network

Synopsis
========

| :program:`ironlink` [*OPTIONS*] *COMMAND*

Description
===========

:program:`ironlink` finds the IronPLC controllers that a workstation can reach,
and shows the route to each: how the workstation reaches the controller, and
when the controller last answered. A controller answers with its public
identity only: the device id, the name, the model, the runtime version, the
session protocol version, the TCP port of the session, and whether it requires
credentials (see :ref:`ironplcvm-discovery`).

:program:`ironlink` only finds controllers. It sends one kind of datagram, the
discovery request, and nothing else. It opens no listening port, holds no
session, and never writes to a controller. A broadcast that leaves through one
interface of a PC does not reach the controllers on another; to find controllers
behind a router, name their addresses.

An answer is an observation: what the controller said at the time shown. It
does not promise that the controller is reachable now.

Options
=======

``-v``, ``--verbose``
   Write log messages to standard error. Repeat the option for more detail, up
   to four times. The log never goes to standard output.

Commands
========

:program:`ironlink browse` [*OPTIONS*]
   Find controllers and print the tree of what answered: the workstation, then
   each way of finding controllers (broadcast, address list), then each
   controller found. The ways of finding run at the same time.

   ``--interface`` *IP*
      Broadcast from the interface that has this address on this PC, for
      example ``192.168.1.20``. The broadcast goes to ``255.255.255.255`` and
      leaves through that interface. Repeat the option for several interfaces.
      An interface is named by its address because the standard library cannot
      list the interfaces of a PC. An IPv6 address is refused with
      :doc:`L6004 <problems/L6004>`: IPv6 has no broadcast.

   ``--address`` *ADDR*
      Ask the controller at *ADDR* directly, by unicast. *ADDR* is *IP*:*PORT*,
      or an IP address alone, which uses the port of ``--port``. Repeat the
      option for several controllers. An address list reaches the controllers
      that a broadcast cannot.

   ``--port`` *PORT*
      The port of the broadcast and of an address without a port. The default
      is the IronPLC session port, |session-port|, on which a controller answers
      discovery by UDP and serves the session by TCP.

   ``--timeout-ms`` *MS*
      Wait this long for answers, from 1 to 60000 milliseconds. The default is
      1000. The wait ends at the time, because the number of controllers is not
      known.

   ``--attempts`` *N*
      Send the request *N* times, from 1 to 10. The default is 2. The sends are
      at least 300 milliseconds apart, and a send that would fall after the
      time is not made. A controller answers one request per 250 milliseconds
      from one source, so the second send recovers a lost datagram. Answers
      from one controller at one address are one row, with the latest time.

   ``--json``
      Print the JSON response of :program:`ironlink serve` instead of text.

   Without ``--interface`` and ``--address``, :program:`ironlink browse`
   broadcasts through the interface that the operating system picks
   (``0.0.0.0``). With only ``--address``, it does not broadcast.

   A row of the text is the name, the model, the runtime version, the device id,
   the address and port of the session, and the age of the answer:

   .. code-block:: text

      PC-ONE
        broadcast: 192.168.1.20 -> 255.255.255.255:7000
          Press line 3  IronPLC SoftPLC  runtime 0.247.0  id 00112233445566778899aabbccddeeff  192.168.1.50:7000  seen 0s ago
        address list: 10.0.4.7:7000
          no device answered

   A controller that requires credentials has the marker ``protected`` at the
   end of its row. A controller that answers on several addresses has one row,
   with the other addresses on the lines below it, each starting with ``also``.
   When replies under one device id say different things (a settings file copied
   to a second controller), the id has the marker ``CONFLICT`` and each different
   answer is a row below it. An address that answers with a discovery reply of
   another wire version is listed with its version: update :program:`ironlink`
   to see that controller. A line reports the datagrams that were not discovery
   replies.

:program:`ironlink identify` *ADDR* [*OPTIONS*]
   Send a fresh request to the controller at *ADDR* (*IP*:*PORT*, or an IP
   address alone, which uses the session port) and print the answer as one row.
   The command ends at the first answer. It accepts ``--timeout-ms``,
   ``--attempts`` and ``--json`` as :program:`ironlink browse` does. When nothing
   answers within the time, the command exits with code 2 and
   :doc:`L6003 <problems/L6003>`.

:program:`ironlink serve`
   Serve the commands ``browse``, ``identify`` and ``version`` as JSON on
   standard input and output, until the end of the input. A program starts
   :program:`ironlink serve` as a child process and uses it; the editor does.
   One line of input carries one command, and one line of output carries one
   response, flushed after every line. Standard output carries protocol lines
   only; the log goes to standard error. A line that is not a command (not JSON,
   an unknown command, a missing field) is answered with an error line whose
   ``lCode`` is null, and the session continues, as with
   :program:`ironplcvm serve`. A command that fails is answered with an error
   line that carries its ``lCode``.

:program:`ironlink version`
   Print the version of :program:`ironlink` and the discovery wire version it
   speaks.

The commands of the shell
=========================

The command ``browse`` has the fields ``interfaces`` and ``addresses`` (lists of
strings), ``port``, ``timeoutMs`` and ``attempts``, all optional, with the
meaning and the defaults of the options of the same name. The command
``identify`` has ``address``, ``timeoutMs`` and ``attempts``. The command
``version`` has no field.

.. code-block:: json

   {"command": "browse", "addresses": ["10.0.4.7"], "timeoutMs": 1000, "attempts": 2}

The response to ``browse`` is the tree. Every time is in milliseconds since the
Unix epoch, and the names of the fields are in camel case:

.. code-block:: json

   {
     "response": "browse",
     "workstation": {"name": "PC-ONE"},
     "drivers": [
       {
         "kind": "addressList",
         "targets": ["10.0.4.7:7000"],
         "devices": [
           {
             "deviceId": "00112233445566778899aabbccddeeff",
             "conflict": false,
             "variants": [
               {
                 "name": "Press line 3",
                 "model": "IronPLC SoftPLC",
                 "runtimeVersion": "0.247.0",
                 "protocol": 1,
                 "protected": false,
                 "routes": [
                   {
                     "driver": "addressList",
                     "address": "10.0.4.7",
                     "sessionPort": 7000,
                     "method": "tcp",
                     "seenAt": 1761136000123
                   }
                 ]
               }
             ]
           }
         ],
         "otherVersions": [],
         "undecodable": 0
       }
     ]
   }

``kind`` is ``broadcast`` or ``addressList``. A device has one variant for each
different set of identity fields (``name``, ``model``, ``runtimeVersion``,
``protocol``, ``protected``) under its device id; more than one variant sets
``conflict`` to true. ``otherVersions`` lists the addresses that answered with
another wire version, and ``undecodable`` counts the datagrams that were not
discovery replies.

The response to ``identify`` names the address and the answer. The answer is
``device`` with the ``observation`` (the fields of a route above, with the name,
model, runtime version, protocol, session port, protected flag and ``seenAt``),
``otherVersion``, or ``none``:

.. code-block:: json

   {"response": "identify", "address": "10.0.4.7:7000", "timeoutMs": 1000, "answer": "none"}

No answer is a normal response of the shell. The command line turns it into an
exit code. The response to ``version`` has ``ironlink`` and
``discoveryWireVersion``.

Platform notes
==============

A broadcast leaves through the interface named by its local address. On Linux, a
controller that listens on one specific unicast address (not the wildcard
address) may not receive a broadcast; name its address with ``--address``
instead. On Windows and Linux, a PC with several networks needs one
``--interface`` for each network to broadcast on.

Exit status
===========

0
   The command succeeded.

2
   A network or input error: :doc:`L6001 <problems/L6001>`,
   :doc:`L6002 <problems/L6002>`, :doc:`L6003 <problems/L6003>`,
   :doc:`L6004 <problems/L6004>` or :doc:`L6005 <problems/L6005>`.

3
   An internal error of :program:`ironlink`: :doc:`L9001 <problems/L9001>`.

See Also
========

- :doc:`/reference/runtime/ironplcvm` --- the program that answers discovery
- :doc:`problems/index` --- the problem codes of :program:`ironlink`
