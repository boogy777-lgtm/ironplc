import * as assert from 'assert';
import {
  canCommandSwap,
  canRunCalibration,
  canSetTimingBudget,
  disconnectedHaModel,
  HA_COMMANDED_SWAP_COMMAND,
  HA_REFRESH_COMMAND,
  HA_RUN_CALIBRATION_COMMAND,
  HA_SET_TIMING_BUDGET_COMMAND,
  HaPanelModel,
  haSections,
} from '../../haPanelLogic';
import {
  HaBarrier,
  HaCalibration,
  HaEvents,
  HaIoReady,
  HaStatus,
  HaTermStats,
  HaTimingBudget,
} from '../../haProtocol';

/** A measured-term fixture. */
function term(overrides?: Partial<HaTermStats>): HaTermStats {
  return { current: 2, min: 1, ema10: 2, ema100: 2, max: 3, count: 12, ...overrides };
}

function status(overrides?: Partial<HaStatus>): HaStatus {
  return {
    standalone: false,
    pairId: '7',
    local: {
      role: 'primary',
      controllerId: 1,
      sync: 'syncReady',
      syncReason: 'boot',
      control: 'active',
      epoch: 2,
    },
    peer: { role: 'secondary', controllerId: 2, sync: 'syncReady', control: 'idle', epoch: 2 },
    epoch: 2,
    applicationGeneration: 1,
    stateGeneration: 0,
    takeoverReady: true,
    syncReady: true,
    ioReady: true,
    linkValid: true,
    alarms: { performanceDegraded: false, timingGuaranteeLost: false, redundancyLost: false },
    ...overrides,
  };
}

function calibration(): HaCalibration {
  return {
    state: 'calibrated',
    linkValid: true,
    takeoverReady: true,
    ab: {
      rtt: term(),
      processing: term({ current: 0, min: 0, max: 0 }),
      lossRatePercent: 0,
      maxConsecutiveLoss: 0,
      pingsSent: 12,
      pongsReceived: 12,
    },
    ba: {
      rtt: term(),
      processing: term({ current: 0, min: 0, max: 0 }),
      lossRatePercent: 0,
      maxConsecutiveLoss: 0,
      pingsSent: 12,
      pongsReceived: 12,
    },
    peerDetect: term({ max: 4 }),
    leaseExpiry: 3,
    claimStart: 4,
    scan: term({ current: 1, min: 1, max: 1 }),
    baselineRttMax: 3,
    recalibrations: 1,
  };
}

function barrier(): HaBarrier {
  return {
    modules: [
      {
        module: 0,
        profile: 'reconnect',
        online: true,
        ownerState: 'armed',
        owner: 1,
        ownerEpoch: 2,
        ownerArmed: true,
        claim: term({ max: 5 }),
        arm: term({ max: 2 }),
        outputApply: term({ max: 1 }),
      },
      {
        module: 1,
        profile: 'reconnect',
        online: false,
        ownerState: 'unowned',
        ownerArmed: false,
        claim: term({ count: 0 }),
        arm: term({ count: 0 }),
        outputApply: term({ count: 0 }),
      },
    ],
    limitingDevice: 0,
    worstOwnershipRecovery: 15,
  };
}

function ioReady(): HaIoReady {
  return {
    requiredInputsObservable: true,
    standbyConnectionsValid: true,
    configsMatch: true,
    epochsValid: true,
  };
}

function budget(): HaTimingBudget {
  return {
    peerFailureConfirmation: 2,
    recoveryBudget: 100,
    peerDetect: { bound: 4, current: 4, ema10: 4 },
    claim: { bound: 7, current: 7, ema10: 7 },
    arm: { bound: 2, current: 2, ema10: 2 },
    scan: { bound: 1, current: 1, ema10: 1, ema100: 1, min: 1, max: 1, phase: 0 },
    outputApply: { bound: 1, current: 1, ema10: 1 },
    calculatedWorstCase: 15,
    predictedIfNow: 15,
    verdict: { qualified: true },
  };
}

function events(): HaEvents {
  return {
    count: 2,
    events: [
      { tick: 0, kind: 'forwardOpenReceived', detail: 'claiming unit 1' },
      { tick: 1, kind: 'ownerAccepted' },
    ],
  };
}

function pairModel(overrides?: Partial<HaPanelModel>): HaPanelModel {
  return {
    connected: true,
    supported: true,
    standalone: false,
    stale: false,
    status: status(),
    calibration: calibration(),
    barrier: barrier(),
    ioReady: ioReady(),
    budget: budget(),
    events: events(),
    ...overrides,
  };
}

suite('haSections', () => {
  test('haSections_when_disconnected_then_single_hint_section', () => {
    const sections = haSections(disconnectedHaModel());

    assert.strictEqual(sections.length, 1);
    assert.strictEqual(sections[0].title, 'Pair Overview');
    assert.strictEqual(sections[0].rows[0].description, 'not connected');
  });

  test('haSections_when_unsupported_then_unavailable_section', () => {
    const sections = haSections({ ...disconnectedHaModel(), connected: true, supported: false });

    assert.strictEqual(sections.length, 1);
    assert.strictEqual(sections[0].rows[0].description, 'unavailable on this device');
  });

  test('haSections_when_standalone_then_standalone_pair_overview', () => {
    const sections = haSections(pairModel({
      standalone: true,
      status: status({ standalone: true, pairId: undefined, peer: undefined, epoch: undefined }),
    }));

    assert.strictEqual(sections.length, 5);
    assert.strictEqual(sections[0].badge, 'Standalone');
    assert.ok(sections[0].rows[0].description!.includes('standalone'));
  });

  test('haSections_when_pair_then_five_tabs_in_contract_order', () => {
    const sections = haSections(pairModel());

    assert.deepStrictEqual(
      sections.map(section => section.title),
      ['Pair Overview', 'Calibration', 'Ownership & Barrier', 'Timing Budget', 'Events'],
    );
  });

  test('haSections_when_pair_then_takeover_ready_badge', () => {
    const sections = haSections(pairModel());

    assert.strictEqual(sections[0].badge, 'TakeoverReady');
    assert.ok(sections[0].rows.some(row => row.label === 'TakeoverReady' && row.description === 'ready'));
    assert.ok(sections[0].rows.some(row => row.label === 'Alarms' && row.description === 'none'));
  });

  test('haSections_when_not_ready_then_badge_says_so', () => {
    const sections = haSections(pairModel({ status: status({ takeoverReady: false }) }));

    assert.strictEqual(sections[0].badge, 'Not ready');
  });

  test('haSections_when_alarm_flags_then_alarm_summary', () => {
    const sections = haSections(pairModel({
      status: status({
        alarms: { performanceDegraded: true, timingGuaranteeLost: true, redundancyLost: true },
      }),
    }));

    const alarms = sections[0].rows.find(row => row.label === 'Alarms');
    assert.ok(alarms!.description!.includes('V4110'));
    assert.ok(alarms!.description!.includes('V4111'));
    assert.ok(alarms!.description!.includes('REDUNDANCY_LOST'));
  });

  test('haSections_when_calibration_then_state_badge_and_direction_rows', () => {
    const sections = haSections(pairModel());

    assert.strictEqual(sections[1].badge, 'Calibrated');
    assert.ok(sections[1].rows.some(row => row.label === 'A→B→A RTT'));
    assert.ok(sections[1].rows.some(row => row.label === 'B→A→B processing'));
    assert.ok(sections[1].rows.some(row => row.label === 'Commissioning baseline'));
  });

  test('haSections_when_barrier_then_limiting_device_and_fault_row', () => {
    const sections = haSections(pairModel());

    const limiting = sections[2].rows.find(row => row.label === 'Limiting device');
    assert.strictEqual(limiting?.description, 'module 0');
    const faulted = sections[2].rows.find(row => row.id === 'barrier.module.1');
    assert.ok(faulted!.description!.includes('faulted'));
  });

  test('haSections_when_budget_then_qualified_badge_and_terms', () => {
    const sections = haSections(pairModel());

    assert.strictEqual(sections[3].badge, 'Qualified');
    assert.ok(sections[3].rows.some(row => row.label === 'T_peer-detect' && row.description!.startsWith('4 bound')));
    assert.ok(sections[3].rows.some(row => row.label === 'Predicted if now'));
  });

  test('haSections_when_budget_fails_then_minimum_demonstrated_badge', () => {
    const sections = haSections(pairModel({
      budget: { ...budget(), verdict: { qualified: false, minimumDemonstrated: 15 } },
    }));

    assert.strictEqual(sections[3].badge, 'Min demonstrated 15');
  });

  test('haSections_when_events_then_rows_oldest_first', () => {
    const sections = haSections(pairModel());

    assert.strictEqual(sections[4].badge, '2');
    assert.deepStrictEqual(
      sections[4].rows.map(row => row.label),
      ['forwardOpenReceived', 'ownerAccepted'],
    );
  });

  test('haSections_when_events_empty_then_empty_row', () => {
    const sections = haSections(pairModel({ events: { count: 0, events: [] } }));

    assert.strictEqual(sections[4].rows[0].label, 'No events recorded');
  });
});

suite('HA action legality', () => {
  test('canCommandSwap_when_sync_ready_pair_then_true', () => {
    assert.strictEqual(canCommandSwap(pairModel()), true);
  });

  test('canCommandSwap_when_not_sync_ready_then_false', () => {
    const model = pairModel({
      status: status({
        local: { role: 'primary', controllerId: 1, sync: 'deSync', syncReason: 'peerDeath', control: 'active', epoch: 2 },
      }),
    });

    assert.strictEqual(canCommandSwap(model), false);
  });

  test('canCommandSwap_when_standalone_or_disconnected_then_false', () => {
    assert.strictEqual(canCommandSwap(pairModel({ standalone: true })), false);
    assert.strictEqual(canCommandSwap(disconnectedHaModel()), false);
  });

  test('canRunCalibration_and_canSetTimingBudget_when_pair_then_true_standalone_false', () => {
    assert.strictEqual(canRunCalibration(pairModel()), true);
    assert.strictEqual(canSetTimingBudget(pairModel()), true);
    assert.strictEqual(canRunCalibration(pairModel({ standalone: true })), false);
    assert.strictEqual(canSetTimingBudget(disconnectedHaModel()), false);
  });
});

suite('HA command ids', () => {
  test('command_ids_when_registered_then_match_the_package_contribution', () => {
    // The literal ids the package.json contributes; the invariant check
    // greps test files for exactly these strings.
    assert.strictEqual(HA_REFRESH_COMMAND, 'ironplc.haRefresh');
    assert.strictEqual(HA_COMMANDED_SWAP_COMMAND, 'ironplc.haCommandedSwap');
    assert.strictEqual(HA_RUN_CALIBRATION_COMMAND, 'ironplc.haRunCalibration');
    assert.strictEqual(HA_SET_TIMING_BUDGET_COMMAND, 'ironplc.haSetTimingBudget');
  });
});
