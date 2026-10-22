==================================
Choose and Connect to a Controller
==================================

The command :guilabel:`IronPLC: Network: Choose Controller…` chooses the
controller of the project. Choosing a controller does two things, and each is
kept where it belongs:

* This PC stores the *route* to the controller: the address and port by which the
  controller answers here. The route is the data of the IronLink extension, and
  :program:`ironlink` verifies it before it stores it. See
  :doc:`/reference/ironlink/vscode-extension`.
* The project remembers the choice in the *path record*,
  :file:`{STEM}.path.iplcinfo`, beside the application container
  (:file:`{STEM}.iplc`). The extension writes it with
  :program:`ironplcc record-path`. The record holds the device ID of the
  controller, the last address that is not a loopback address, and the time of the
  choice. It travels with the project, so a project that is moved to another PC
  keeps its last controller.

The choice is a remembered convenience. It is not a binding of the project to the
controller, and you can connect to any controller.

The Path Record
===============

The path record sits beside the application container that the download record
sits beside: the container :file:`{FOLDER}.iplc` in the first workspace folder.
When no folder is open, the command shows
:doc:`E0020 </reference/editor/problems/E0020>` and chooses nothing.

Choose from the Network View
============================

In the Network view of the IronLink extension, select :guilabel:`Choose` on the
row of a controller. IronLink stores the route to the controller and then runs
this command with the device ID, the address and the session port of the row. The
command writes the path record. If IronLink cannot store the route, it shows the
error of :program:`ironlink` and the command does not run.

Choose from the Command Palette
===============================

Run :guilabel:`IronPLC: Network: Choose Controller…` without a row to pick the
controller from a list. The list has up to three groups, in this order, and a
controller is listed once, in the first group it belongs to:

:guilabel:`Last chosen controller`
   The controller that the path record holds, with the time of the choice and its
   last address.

:guilabel:`Controllers with a route on this PC`
   The controllers for which IronLink stores a route, with the address and the
   time the route was verified.

:guilabel:`Recently observed`
   The controllers that the last search of IronLink heard, with the age of the
   observation.

The last row, :guilabel:`Search network…`, shows the Network view of IronLink and
starts a search. Select a controller from the result with :guilabel:`Choose`.

Picking a controller stores the route through IronLink and then writes the path
record. When the route cannot be stored, for example because nothing answers at
the address or another controller answers there, the extension shows the error of
:program:`ironlink` (see :doc:`/reference/ironlink/problems/L6003` and
:doc:`/reference/ironlink/problems/L6008`) and the path record stays as it was.

Connect to the Chosen Controller
================================

The command :guilabel:`IronPLC: Network: Connect to Chosen Controller` connects to
the controller that the path record holds. It opens the session through the
connection method ``tcp``, by a profile that the extension makes for the occasion
and does not save: the name of the controller (else its short device ID), the
address, and the session port. Other connections are made as before, from the
profiles in the ``ironplc.connections`` setting.

The command finds the address in this order:

#. The route that this PC stores for the device ID of the chosen controller
   (IronLink), when the IronLink extension is installed.
#. The route hint of the path record.
#. Otherwise the command says why there is no address, and offers
   :guilabel:`Choose Controller…`.

In the Network view of IronLink, :guilabel:`Connect` on the row of a controller
runs the same command for that controller: it connects to the address of the row
and does not change the chosen controller.

When the session is open and the controller reports its device ID, the extension
compares it with the controller it meant and with the controller chosen for the
project. When they differ, the extension writes an information line to the
:guilabel:`IronPLC Connection` output channel and shows it in the
:guilabel:`IronPLC Device` panel, for example::

   The connected controller (a0a1a2a3) is not the controller chosen for this project (00112233).

The connection is never refused because the controller is a different one. A
controller that is busy is refused as before (see
:doc:`/reference/runtime/problems/V6014`), and a controller that requires
credentials cannot be connected yet.

The Status Item
---------------

While no connection is open and a controller is chosen, the status item at the
bottom of the window reads :guilabel:`IronPLC: NAME (not connected)`, where NAME
is the name the controller reported at a connection in this session, else its
short device ID. Selecting the item connects to the chosen controller. When no
controller is chosen, the item reads :guilabel:`IronPLC: Not connected` and
selecting it connects as before, with a profile of the ``ironplc.connections``
setting. When a connection is open, the
item shows the connection as before.

Without the IronLink Extension
==============================

The IronLink extension is optional. Without it, the list shows the controller last
chosen, if there is one, and one row that says that searching the network needs
the IronLink extension. Picking that row does nothing. The extension does not
declare a dependency on IronLink, so install it when you want to search.

Problem Codes
=============

.. list-table::
   :header-rows: 1
   :widths: 20 80

   * - Code
     - Meaning
   * - :doc:`E0020 </reference/editor/problems/E0020>`
     - The extension could not remember the chosen controller.
   * - :doc:`E0010 </reference/editor/problems/E0010>`
     - The command that connects was run with an argument that is not a controller.
   * - :doc:`E0013 </reference/editor/problems/E0013>`
     - The session could not be opened.
