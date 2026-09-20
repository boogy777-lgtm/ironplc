import * as assert from 'assert';
import {
  ACTIVE_ICON,
  hotEditStatusBar,
  IDLE_ICON,
} from '../../hotEditStatusBarLogic';
import { HotEditStatus } from '../../hotEditSession';

function status(overrides: Partial<HotEditStatus> = {}): HotEditStatus {
  return {
    mode: 'normal',
    active: 3,
    normal: 3,
    candidate: null,
    application: 2,
    migration: false,
    rounds: 12,
    ...overrides,
  };
}

suite('hotEditStatusBar', () => {
  test('hotEditStatusBar_when_no_session_then_advertises_start_session', () => {
    const state = hotEditStatusBar(undefined);
    assert.strictEqual(state.text, `$(${IDLE_ICON}) Hot Edit: Start Session`);
    assert.strictEqual(state.command, 'ironplc.startHotEditSession');
    assert.strictEqual(state.visible, true);
    assert.strictEqual(state.warning, false);
  });

  test('hotEditStatusBar_when_session_normal_then_shows_mode_and_generation', () => {
    const state = hotEditStatusBar(status());
    assert.strictEqual(state.text, `$(${ACTIVE_ICON}) Hot Edit: Normal (gen 3)`);
    assert.strictEqual(state.command, 'ironplc.showHotEditStatus');
    assert.ok(state.tooltip.includes('Rounds: 12'));
    assert.strictEqual(state.warning, false);
  });

  test('hotEditStatusBar_when_candidate_staged_then_shows_pending_and_warns', () => {
    const state = hotEditStatusBar(status({ mode: 'testing', candidate: 4 }));
    assert.strictEqual(state.text, `$(${ACTIVE_ICON}) Hot Edit: 1 pending`);
    assert.ok(state.tooltip.includes('Candidate generation: 4'));
    assert.strictEqual(state.warning, true);
  });

  test('hotEditStatusBar_when_migration_candidate_then_warns', () => {
    const state = hotEditStatusBar(status({ candidate: 5, migration: true }));
    assert.strictEqual(state.warning, true);
    assert.ok(state.tooltip.includes('state migration'));
  });
});
