import * as assert from 'assert';
import {
  DASHBOARD_ACTIONS,
  DashboardAction,
  OPEN_DASHBOARD_COMMAND,
  PairDashboardState,
  UNITS,
  applyAction,
  buildDashboardViewModel,
  canApply,
  demoPairState,
  emptyPairState,
  enabledActions,
  ownerOf,
  standbyOf,
} from '../../redundantPairLogic';

const AT = '12:00:00';

function apply(state: PairDashboardState, ...actions: DashboardAction[]): PairDashboardState {
  return actions.reduce((current, action) => applyAction(current, action, AT), state);
}

function killBoth(state: PairDashboardState): PairDashboardState {
  return apply(state, 'kill-p', 'kill-i');
}

suite('empty (real) pair state', () => {
  test('emptyPairState_then_disconnected_and_all_actions_disabled', () => {
    const state = emptyPairState();
    assert.strictEqual(state.connected, false);
    assert.strictEqual(state.demo, false);
    for (const action of DASHBOARD_ACTIONS) {
      assert.strictEqual(canApply(state, action), false, action);
    }
    const view = buildDashboardViewModel(state);
    assert.ok(view.emptyMessage);
    assert.strictEqual(view.connected, false);
    assert.deepStrictEqual(view.units, []);
  });

  test('openDashboardCommand_then_is_the_registered_id', () => {
    assert.strictEqual(OPEN_DASHBOARD_COMMAND, 'ironplc.openRedundantPairDashboard');
  });
});

suite('demo truth table', () => {
  test('demoPairState_then_plc1_owner_armed_and_synced', () => {
    const state = demoPairState();
    assert.strictEqual(ownerOf(state), 'PLC-1');
    assert.strictEqual(standbyOf(state), 'PLC-2');
    assert.strictEqual(state.epoch, 41);
    assert.ok(state.modules.every(module => module.armed));
    const view = buildDashboardViewModel(state);
    assert.strictEqual(view.takeoverReady, true);
    assert.strictEqual(view.barrier.limitingDevice, 'DO-07');
    assert.strictEqual(view.budget.ok, true);
  });

  test('killP_whenIoAlive_then_observer_redundancy_lost_and_no_promotion', () => {
    const state = apply(demoPairState(), 'kill-p');
    assert.strictEqual(state.control['PLC-2'], 'REDUNDANCY_LOST');
    assert.strictEqual(state.control['PLC-1'], 'ACTIVE');
    assert.strictEqual(canApply(state, 'swap'), false);
    assert.strictEqual(state.counters.ch1.penalty, 1000);
    assert.strictEqual(state.counters.ch1.missing, 1);
    const view = buildDashboardViewModel(state);
    assert.strictEqual(view.channels.p, false);
    assert.strictEqual(view.channels.i, true);
    assert.strictEqual(view.takeoverReady, false);
  });

  test('killI_whenPairLinkAlive_then_owner_degraded_and_no_promotion', () => {
    const state = apply(demoPairState(), 'kill-i');
    assert.strictEqual(state.control['PLC-1'], 'ACTIVE_DEGRADED');
    assert.strictEqual(state.control['PLC-2'], 'IDLE');
    assert.strictEqual(canApply(state, 'swap'), false);
    assert.strictEqual(state.counters.ch2.penalty, 1000);
    const view = buildDashboardViewModel(state);
    assert.strictEqual(view.channels.i, false);
    assert.strictEqual(view.takeoverReady, false);
  });

  test('killBothWithLiveOwner_then_partition_is_the_only_promotion_path', () => {
    const state = killBoth(demoPairState());
    assert.strictEqual(state.control['PLC-1'], 'ACTIVE_DEGRADED');
    assert.strictEqual(state.control['PLC-2'], 'IDLE');
    assert.strictEqual(canApply(state, 'swap'), false);
    assert.strictEqual(canApply(state, 'partition'), true);
  });

  test('restoreChannels_then_normal_state_returns', () => {
    const state = apply(killBoth(demoPairState()), 'restore-channels');
    assert.strictEqual(state.control['PLC-1'], 'ACTIVE');
    assert.strictEqual(state.control['PLC-2'], 'IDLE');
    assert.strictEqual(state.channels.P, true);
    assert.strictEqual(state.channels.I, true);
    assert.strictEqual(state.counters.ch1.penalty, 0);
    assert.strictEqual(state.counters.ch2.penalty, 0);
  });
});

suite('demo failure and resurrection', () => {
  test('primaryDeath_then_standby_promotes_with_epoch_bump_and_owner_stays_dead', () => {
    const state = apply(demoPairState(), 'primary-death');
    assert.strictEqual(state.dead['PLC-1'], true);
    assert.strictEqual(state.deadRole['PLC-1'], 'owner');
    assert.strictEqual(state.control['PLC-1'], 'DEAD');
    assert.strictEqual(state.control['PLC-2'], 'ACTIVE');
    assert.strictEqual(state.permitOwner, 'PLC-2');
    assert.strictEqual(state.epoch, 42);
    assert.ok(state.modules.every(module => module.armed && module.owner === 'PLC-2'));
    assert.strictEqual(buildDashboardViewModel(state).takeoverReady, false);
  });

  test('resurrectPrimary_then_boots_as_secondary_never_primary', () => {
    const state = apply(demoPairState(), 'primary-death', 'resurrect-primary');
    assert.strictEqual(state.dead['PLC-1'], false);
    assert.strictEqual(state.sync['PLC-1'], 'SYNC_READY');
    assert.strictEqual(state.control['PLC-1'], 'IDLE');
    assert.strictEqual(state.control['PLC-2'], 'ACTIVE');
    assert.strictEqual(state.ownerUnreachable, false);
    assert.strictEqual(state.channels.P, true);
    assert.ok(state.events.some(entry => entry.message.includes('zombie rule')));
  });

  test('secondaryDeath_then_owner_continues_and_pair_degrades', () => {
    const state = apply(demoPairState(), 'secondary-death');
    assert.strictEqual(state.dead['PLC-2'], true);
    assert.strictEqual(state.control['PLC-2'], 'DEAD');
    assert.strictEqual(state.control['PLC-1'], 'ACTIVE');
    assert.strictEqual(canApply(state, 'secondary-death'), false);
    assert.deepStrictEqual(buildDashboardViewModel(state).alarms.deadUnits, ['PLC-2']);
  });

  test('resurrectSecondary_then_rejoins_synced', () => {
    const state = apply(demoPairState(), 'secondary-death', 'resurrect-secondary');
    assert.strictEqual(state.dead['PLC-2'], false);
    assert.strictEqual(state.sync['PLC-2'], 'SYNC_READY');
    assert.strictEqual(state.control['PLC-2'], 'IDLE');
  });

  test('partition_when_owner_alive_then_claim_rejected_and_redundancy_lost', () => {
    const state = apply(demoPairState(), 'kill-p', 'kill-i', 'partition');
    assert.strictEqual(state.redundancyLost, true);
    assert.strictEqual(state.control['PLC-1'], 'REDUNDANCY_LOST');
    assert.strictEqual(state.control['PLC-2'], 'REDUNDANCY_LOST');
    assert.ok(state.modules.every(module => module.ownerState === 'SAFE' && !module.armed));
    assert.ok(state.events.some(entry => entry.message.includes('OWNERSHIP_CONFLICT')));
    assert.strictEqual(canApply(state, 'partition'), false);
    const view = buildDashboardViewModel(state);
    assert.strictEqual(view.alarms.redundancyLost, true);
    assert.strictEqual(view.barrier.passed, false);
  });
});

suite('demo commissioning commands', () => {
  test('swap_then_roles_exchange_and_epoch_bumps', () => {
    const state = apply(demoPairState(), 'swap');
    assert.strictEqual(state.control['PLC-2'], 'ACTIVE');
    assert.strictEqual(state.control['PLC-1'], 'IDLE');
    assert.strictEqual(state.epoch, 42);
    assert.strictEqual(state.roles['PLC-2'], 'Primary');
    assert.strictEqual(state.roles['PLC-1'], 'Secondary');
    assert.strictEqual(state.sync['PLC-1'], 'SYNC_READY');
    assert.ok(state.modules.every(module => module.owner === 'PLC-2' && module.armed));
  });

  test('invalidate_then_swap_and_takeover_are_blocked_until_calibration', () => {
    const invalid = apply(demoPairState(), 'invalidate');
    assert.strictEqual(invalid.calibration, 'UNQUALIFIED');
    assert.strictEqual(canApply(invalid, 'swap'), false);
    assert.strictEqual(buildDashboardViewModel(invalid).takeoverReady, false);
    const recalibrated = apply(invalid, 'calibrate');
    assert.strictEqual(recalibrated.calibration, 'CALIBRATED');
    assert.strictEqual(canApply(recalibrated, 'swap'), true);
  });

  test('degrade_then_performance_alarm_and_guarantee_lost', () => {
    const state = apply(demoPairState(), 'degrade');
    assert.strictEqual(state.degraded, true);
    const view = buildDashboardViewModel(state);
    assert.strictEqual(view.alarms.performanceDegraded, true);
    assert.strictEqual(state.timingLost, true);
    assert.strictEqual(view.budget.ok, false);
  });

  test('reset_then_returns_to_the_commissioning_baseline', () => {
    const state = apply(demoPairState(), 'primary-death', 'reset');
    assert.strictEqual(state.epoch, 41);
    assert.strictEqual(state.control['PLC-1'], 'ACTIVE');
    assert.strictEqual(state.dead['PLC-1'], false);
  });

  test('enabledActions_then_reflects_guards_and_counters_tick_with_actions', () => {
    const state = demoPairState();
    const enabled = enabledActions(state);
    assert.strictEqual(enabled['swap'], true);
    assert.strictEqual(enabled['resurrect-primary'], false);
    assert.strictEqual(enabled['restore-channels'], false);
    const after = apply(state, 'kill-p');
    assert.strictEqual(after.counters.ch1.ping, 1);
    assert.strictEqual(after.counters.ch2.ping, 1);
    assert.strictEqual(UNITS.length, 2);
  });
});
