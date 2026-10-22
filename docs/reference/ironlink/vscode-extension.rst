==================
IronLink Extension
==================

The IronLink extension shows the controllers that :program:`ironlink` finds, and
the route to each, in the activity bar of your development environment. It works
on its own: it does not need the IronPLC extension.

Like :program:`ironlink`, the extension only finds controllers: it opens no
listening port, holds no session and never writes to a controller or to a
project. :program:`ironlink` makes every decision; the extension shows its answers.

The Network View
================

Select :guilabel:`IronLink` in the activity bar to open the :guilabel:`Network`
view. The view is empty until you search. Select :guilabel:`Search network` to
search.

The view is a tree of text rows:

* The workstation, which is this PC.
* Under the workstation, one row for each way of finding controllers that ran:
  :guilabel:`Broadcast`, with the interface it sent from, and
  :guilabel:`Address list`, with the addresses it asked.
* Under each way of finding, one row for each route to a controller. The row shows
  the name of the controller. Its description holds the address and port of the
  session, the first eight digits of the device id, the markers below, and the age
  of the answer.
* The row :guilabel:`Routes not seen`, when this PC stores a route to a controller
  that did not answer in this search.

A controller that answers by two ways of finding is shown under both. A controller
that answers on several addresses has one row for each address.

.. list-table::
   :header-rows: 1
   :widths: 20 80

   * - Marker
     - Meaning
   * - ``route``
     - The route that this PC stored for the controller is this route (the same
       address and port).
   * - ``protected``
     - The controller requires credentials.
   * - ``conflict``
     - Answers under this device id say different things, for example after a
       settings file was copied to a second controller. Each different answer is a
       row.

A row that starts with ``Discovery version`` is a device that answered in a way
this :program:`ironlink` cannot read. Update the IronPLC tools to see that
controller.

Hover over a row to see the full answer: the model, the runtime version, the
session protocol version, the complete device id, the route and the time of the
answer.

Actions
=======

:guilabel:`Search network`
   Search for controllers and show what answered. The search uses the settings
   ``ironlink.browse.timeoutMs`` and ``ironlink.browse.attempts`` when they are
   set.

:guilabel:`Add address…`
   Ask for the address of a controller as *IP* or *IP*:*PORT* and add it to the
   address list that every search asks. Use it for controllers that a broadcast
   does not reach, for example behind a router. :program:`ironlink` checks the
   address and the extension shows its :doc:`L6004 <problems/L6004>` error when the
   address is not valid. The command is in the title of the view and on the row
   :guilabel:`Address list`.

:guilabel:`Remove address`
   Choose one of the stored addresses and remove it. The command is on the row
   :guilabel:`Address list` and in the command palette.

:guilabel:`Identify`
   Ask the route of a row again and show the answer and the time of the answer. A
   route that does not answer shows a warning with the time waited.

:guilabel:`Copy device id`
   Copy the device id of a row to the clipboard.

:guilabel:`Choose`
   Hand the controller of a row to the IronPLC extension, which remembers the
   connection path of a project. The extension passes the device id, the address and
   the session port of the row. :guilabel:`Choose` is available when the IronPLC
   extension provides the command that records the choice. Otherwise, selecting
   :guilabel:`Choose` shows why it is not available.

An error is shown with its code and a link to the page that describes the code:
:doc:`L#### codes </reference/ironlink/problems/index>` come from :program:`ironlink`,
and :doc:`E#### codes </reference/editor/problems/index>` come from the extension.

Settings
========

ironlink.path
-------------

:Type: String
:Default: Empty (auto-discovery)

The directory that contains the :program:`ironlink` program (:program:`ironlink.exe`
on Windows). When empty, the extension searches for the program in the directory
that the ``IRONPLC`` environment variable names, the install directories, and then
the directories of the ``PATH`` environment variable. See
:doc:`E0016 </reference/editor/problems/E0016>`.

ironlink.browse.timeoutMs
-------------------------

:Type: Number
:Default: Empty (the default of :program:`ironlink`)

How long a search waits for answers, from 1 to 60000 milliseconds.

ironlink.browse.attempts
------------------------

:Type: Number
:Default: Empty (the default of :program:`ironlink`)

How many times a search sends its request, from 1 to 10.

Problem Codes
=============

.. list-table::
   :header-rows: 1
   :widths: 20 80

   * - Code
     - Meaning
   * - :doc:`E0016 </reference/editor/problems/E0016>`
     - The extension cannot find :program:`ironlink`.
   * - :doc:`E0017 </reference/editor/problems/E0017>`
     - The extension cannot start :program:`ironlink`.
   * - :doc:`E0018 </reference/editor/problems/E0018>`
     - :program:`ironlink` ended while the extension waited for an answer.
   * - :doc:`E0019 </reference/editor/problems/E0019>`
     - :program:`ironlink` sent a response the extension cannot read.
