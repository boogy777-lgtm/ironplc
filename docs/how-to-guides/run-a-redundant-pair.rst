.. _run a redundant pair target:

============================
Run a Redundant Pair of Runtimes
============================

Two :program:`ironplcvm serve` processes can form a redundant pair over a
UDP pair link on loopback. This guide shows how to run the two-runtime
demo and what the hot-edit synchronization looks like across the pair.

.. note::

   The pair link is Phase 5 work in progress: the UDP binding is the real
   transport behind the ``NicPort`` seam, but the fencing (CONTROL) and
   calibration machinery still run only on the in-process simulator
   binding. Promotion on peer death is the takeover *policy* (ADR-0064(e))
   modeled at the verdict layer; the fencing barrier is a later slice.

What the existing tests already prove
-------------------------------------

The in-process loopback suites pin the ADR-0064 contract the demo
reproduces across two real processes:

* ``compiler/ironplc-redundancy/tests/crossload.rs:275`` — Accept
  crossloads the candidate: the Secondary stages the same candidate
  generation and applies the snapshot byte-identically (ADR-0064(c)).
* ``compiler/ironplc-redundancy/tests/crossload.rs:319`` — takeover
  mid-Test of a migration candidate: the survivor executes the CANDIDATE,
  and untest stays refused (V4011) (ADR-0064(e)).
* ``compiler/ironplc-redundancy/tests/crossload.rs:380`` — takeover
  mid-Test of a layout-preserving candidate: the survivor flips the
  selector without a state swap and runs the candidate's code.
* ``compiler/ironplc-redundancy/tests/crossload.rs:416`` — Assemble is
  one transaction across the pair: both units promote one generation and
  neither holds a different canonical artifact (ADR-0064(h)).
* ``compiler/ironplc-redundancy/tests/crossload.rs:463`` — a garbled
  transfer never pretends redundancy-ready: the Secondary latches the
  coded alarm (V4104) and the Primary learns the refusal.
* ``compiler/ironplc-redundancy/tests/crossload.rs:516`` and
  ``:540`` — Cancel and Untest mirror across the pair (ADR-0064(f)/(g)).
* ``compiler/ironplc-redundancy/tests/pair_link.rs:148`` — the boot
  convergence (both units reach ``syncReady``), the silence penalty and
  peer-death detection (``:192``), the zombie rule — a revived
  ex-Primary rejoins as Secondary (``:220``) — and the peer-restart
  epoch discontinuity (``:265``).
* ``compiler/vm-cli/src/serve_tests.rs:665`` — the simulator binding's
  full HA query surface over one session (the demo binding Slice 6).

Run the demo
------------

The self-contained demo is the two-process integration test: it compiles
a base program with stable variable IDs, boots two real ``ironplcvm``
processes over ephemeral UDP ports, drives ``acceptEdits`` →
``testEdits`` → ``assembleEdits`` from the Primary's session, asserts the
ADR-0064 contract against both units, then kills the Primary mid-Test and
asserts the survivor promotes executing the candidate.

.. code-block:: console

   cargo test -p ironplc-vm-cli --test ha_pair -- --nocapture --test-threads=1

Run the two runtimes manually
-----------------------------

Each unit serves its own copy of the same compiled container and names
the other's pair-link address. The edit candidate must carry stable
variable IDs for the schema-changing edit to stage as a migration
candidate (see :doc:`/explanation/variables-and-io`).

.. code-block:: console

   # Unit A (the initial owner):
   ironplcvm serve unit-a.iplc --ha-role primary \
       --ha-peer-bind 127.0.0.1:9401 --ha-peer-peer 127.0.0.1:9402

   # Unit B (the standby), in a second terminal:
   ironplcvm serve unit-b.iplc --ha-role secondary \
       --ha-peer-bind 127.0.0.1:9402 --ha-peer-peer 127.0.0.1:9401

Both sessions speak the usual JSON command protocol on stdin/stdout, plus
the pair overview ``{"command":"haStatus"}`` from each unit's view. The
pair link stays live between commands — the standby synchronizes,
replicates, and promotes with no client connected.

A scripted session against unit A (one JSON line in, one line out):

.. code-block:: console

   $ printf '%s\n' \
       '{"command":"haStatus"}' \
       '{"command":"acceptEdits","program":[...],"edit":{"name":"add-gauge"}}' \
       '{"command":"testEdits"}' \
       '{"command":"assembleEdits"}' \
       '{"command":"getStatus"}' | ironplcvm serve unit-a.iplc --ha-role primary \
           --ha-peer-bind 127.0.0.1:9401 --ha-peer-peer 127.0.0.1:9402

What you should see
-------------------

Observed sequence from a real run of the integration test (the standby's
view, polling ``getStatus``/``haStatus``; ``app`` is the committed
application generation, ``candidate`` the staged one):

.. code-block:: text

   t+   16ms  B/secondary  sync=deSync     mode=-       epoch=0  app=0  candidate=-  rounds=0
   t+   36ms  B/secondary  sync=syncReady  mode=-       epoch=0  app=0  candidate=-  rounds=0
   t+   62ms  B/secondary  sync=-          mode=normal  epoch=0  app=1  candidate=2  rounds=0
   t+   83ms  B/secondary  sync=-          mode=testing epoch=0  app=1  candidate=2  rounds=0
   t+  125ms  B/secondary  sync=-          mode=testing epoch=0  app=1  candidate=2  rounds=1
   t+  135ms  B/secondary  sync=-          mode=normal  epoch=0  app=2  candidate=-  rounds=2

Reading the sequence:

#. Boot: deSYNC → SYNC_READY through the owner's replication stream;
   the standby holds no execution permit (rounds stay 0).
#. ``acceptEdits`` on the Primary: the offer crosses the link; the
   standby stages the same candidate generation (2) and applies the
   snapshot.
#. ``testEdits``: both selectors switch at the boundary — on the
   standby the migration candidate's replicated image is what flips it.
#. ``assembleEdits``: one commit transaction; both units promote
   application generation 2, the candidate is gone, and the epochs
   converge on the value the assemble boundary minted. Both units
   persist the identical committed bytes to their slot stores.

Takeover mid-Test
-----------------

Kill the Primary's process during ``testing`` (for example,
:kbd:`Ctrl+C` in its terminal). The standby confirms the peer's death on
the missed-exchange threshold, promotes itself, and takes over executing
the candidate — the selector never reverts to the original:

.. code-block:: text

   t+   83ms  B/secondary  mode=testing rounds=0   (peer killed here)
   t+  125ms  B/secondary  mode=testing rounds=1   (promoted; executing the candidate)
   t+  135ms  B/secondary  mode=normal  rounds=2   (assembleEdits acks on the survivor)

An ``untestEdits`` on the survivor answers ``V4011`` (a migration
candidate cannot untest, ADR-0064(e)); its ``assembleEdits`` acks and
commits the candidate it is executing.
