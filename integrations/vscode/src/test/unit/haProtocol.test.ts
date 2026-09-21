import * as assert from 'assert';
import {
  encodeHaCommand,
  HaResponse,
  parseHaResponseLine,
} from '../../haProtocol';
import { HotEditProtocolError } from '../../hotEditSession';

const HA_STATUS_LINE = '{"response":"haStatus","standalone":false,"pairId":"7",'
  + '"local":{"role":"primary","controllerId":1,"sync":"syncReady","syncReason":"boot",'
  + '"control":"active","epoch":2},'
  + '"peer":{"role":"secondary","controllerId":2,"sync":"syncReady","control":"idle","epoch":2},'
  + '"epoch":2,"applicationGeneration":1,"stateGeneration":0,'
  + '"takeoverReady":true,"syncReady":true,"ioReady":true,"linkValid":true,'
  + '"alarms":{"performanceDegraded":false,"timingGuaranteeLost":false,"redundancyLost":false}}';

const TERM = '"current":2,"min":1,"ema10":2,"ema100":2,"max":3,"count":12';

const HA_CALIBRATION_LINE = '{"response":"haCalibration","state":"calibrated","linkValid":true,'
  + '"takeoverReady":true,'
  + '"ab":{"rtt":{' + TERM + '},"processing":{' + TERM + '},"lossRatePercent":0,'
  + '"maxConsecutiveLoss":0,"pingsSent":12,"pongsReceived":12},'
  + '"ba":{"rtt":{' + TERM + '},"processing":{' + TERM + '},"lossRatePercent":0,'
  + '"maxConsecutiveLoss":0,"pingsSent":12,"pongsReceived":12},'
  + '"peerDetect":{' + TERM + '},"leaseExpiry":3,"claimStart":4,"scan":{' + TERM + '},'
  + '"baselineRttMax":3,"recalibrations":1,"lastInvalidation":"linkChanged"}';

const HA_BARRIER_LINE = '{"response":"haBarrier","modules":['
  + '{"module":0,"profile":"reconnect","online":true,"ownerState":"armed","owner":1,"ownerEpoch":2,'
  + '"ownerArmed":true,"claim":{' + TERM + '},"arm":{' + TERM + '},"outputApply":{' + TERM + '}}'
  + '],"limitingDevice":0,"worstOwnershipRecovery":15}';

const HA_IO_READY_LINE = '{"response":"haIoReady","requiredInputsObservable":true,'
  + '"standbyConnectionsValid":true,"configsMatch":true,"epochsValid":true}';

const HA_BUDGET_LINE = '{"response":"haTimingBudget","peerFailureConfirmation":2,"recoveryBudget":100,'
  + '"peerDetect":{"bound":4,"current":4,"ema10":4},"claim":{"bound":7,"current":7,"ema10":7},'
  + '"arm":{"bound":2,"current":2,"ema10":2},'
  + '"scan":{"bound":1,"current":1,"ema10":1,"ema100":1,"min":1,"max":1,"phase":0},'
  + '"outputApply":{"bound":1,"current":1,"ema10":1},'
  + '"calculatedWorstCase":15,"predictedIfNow":15,"verdict":{"qualified":true}}';

const HA_EVENTS_LINE = '{"response":"haEvents","count":4,"events":['
  + '{"tick":0,"kind":"forwardOpenReceived","detail":"claiming unit 1"},'
  + '{"tick":1,"kind":"ownerAccepted"}]}';

suite('encodeHaCommand', () => {
  test('encodeHaCommand_when_query_then_tag_only_line', () => {
    assert.strictEqual(encodeHaCommand('haStatus'), '{"command":"haStatus"}');
    assert.strictEqual(encodeHaCommand('haEvents'), '{"command":"haEvents"}');
    assert.strictEqual(encodeHaCommand('haCommandedSwap'), '{"command":"haCommandedSwap"}');
    assert.strictEqual(encodeHaCommand('haRunCalibration'), '{"command":"haRunCalibration"}');
  });

  test('encodeHaCommand_when_set_timing_budget_then_carries_parameters', () => {
    assert.strictEqual(
      encodeHaCommand('haSetTimingBudget', { peerFailureConfirmation: 4, recoveryBudget: 100 }),
      '{"command":"haSetTimingBudget","peerFailureConfirmation":4,"recoveryBudget":100}',
    );
  });
});

suite('parseHaResponseLine', () => {
  test('parseHaResponseLine_when_status_then_every_field', () => {
    const response = parseHaResponseLine(HA_STATUS_LINE);

    assert.strictEqual(response.kind, 'haStatus');
    if (response.kind === 'haStatus') {
      const status = response.status;
      assert.strictEqual(status.standalone, false);
      assert.strictEqual(status.pairId, '7');
      assert.strictEqual(status.local.role, 'primary');
      assert.strictEqual(status.local.sync, 'syncReady');
      assert.strictEqual(status.local.syncReason, 'boot');
      assert.strictEqual(status.local.control, 'active');
      assert.strictEqual(status.local.controllerId, 1);
      assert.strictEqual(status.peer?.control, 'idle');
      assert.strictEqual(status.epoch, 2);
      assert.strictEqual(status.takeoverReady, true);
      assert.strictEqual(status.alarms.redundancyLost, false);
    }
  });

  test('parseHaResponseLine_when_calibration_then_directions_and_baseline', () => {
    const response = parseHaResponseLine(HA_CALIBRATION_LINE);

    assert.strictEqual(response.kind, 'haCalibration');
    if (response.kind === 'haCalibration') {
      assert.strictEqual(response.calibration.state, 'calibrated');
      assert.strictEqual(response.calibration.ab.rtt.max, 3);
      assert.strictEqual(response.calibration.ab.processing.count, 12);
      assert.strictEqual(response.calibration.ba.lossRatePercent, 0);
      assert.strictEqual(response.calibration.peerDetect.current, 2);
      assert.strictEqual(response.calibration.leaseExpiry, 3);
      assert.strictEqual(response.calibration.claimStart, 4);
      assert.strictEqual(response.calibration.baselineRttMax, 3);
      assert.strictEqual(response.calibration.recalibrations, 1);
      assert.strictEqual(response.calibration.lastInvalidation, 'linkChanged');
    }
  });

  test('parseHaResponseLine_when_barrier_then_module_row', () => {
    const response = parseHaResponseLine(HA_BARRIER_LINE);

    assert.strictEqual(response.kind, 'haBarrier');
    if (response.kind === 'haBarrier') {
      const row = response.barrier.modules[0];
      assert.strictEqual(row.module, 0);
      assert.strictEqual(row.profile, 'reconnect');
      assert.strictEqual(row.ownerState, 'armed');
      assert.strictEqual(row.owner, 1);
      assert.strictEqual(row.ownerEpoch, 2);
      assert.strictEqual(row.ownerArmed, true);
      assert.strictEqual(row.claim.max, 3);
      assert.strictEqual(response.barrier.limitingDevice, 0);
      assert.strictEqual(response.barrier.worstOwnershipRecovery, 15);
    }
  });

  test('parseHaResponseLine_when_io_ready_then_breakdown', () => {
    const response = parseHaResponseLine(HA_IO_READY_LINE);

    assert.strictEqual(response.kind, 'haIoReady');
    if (response.kind === 'haIoReady') {
      assert.strictEqual(response.ioReady.requiredInputsObservable, true);
      assert.strictEqual(response.ioReady.standbyConnectionsValid, true);
      assert.strictEqual(response.ioReady.configsMatch, true);
      assert.strictEqual(response.ioReady.epochsValid, true);
      assert.strictEqual(response.ioReady.failingItem, undefined);
    }
  });

  test('parseHaResponseLine_when_budget_then_terms_and_verdict', () => {
    const response = parseHaResponseLine(HA_BUDGET_LINE);

    assert.strictEqual(response.kind, 'haTimingBudget');
    if (response.kind === 'haTimingBudget') {
      const budget = response.budget;
      assert.strictEqual(budget.peerFailureConfirmation, 2);
      assert.strictEqual(budget.recoveryBudget, 100);
      assert.strictEqual(budget.peerDetect.bound, 4);
      assert.strictEqual(budget.claim.bound, 7);
      assert.strictEqual(budget.arm.bound, 2);
      assert.strictEqual(budget.scan.phase, 0);
      assert.strictEqual(budget.outputApply.bound, 1);
      assert.strictEqual(budget.calculatedWorstCase, 15);
      assert.strictEqual(budget.predictedIfNow, 15);
      assert.strictEqual(budget.verdict?.qualified, true);
    }
  });

  test('parseHaResponseLine_when_events_then_ring_and_count', () => {
    const response = parseHaResponseLine(HA_EVENTS_LINE);

    assert.strictEqual(response.kind, 'haEvents');
    if (response.kind === 'haEvents') {
      assert.strictEqual(response.events.count, 4);
      assert.strictEqual(response.events.events.length, 2);
      assert.strictEqual(response.events.events[0].kind, 'forwardOpenReceived');
      assert.strictEqual(response.events.events[0].detail, 'claiming unit 1');
      assert.strictEqual(response.events.events[1].detail, undefined);
    }
  });

  test('parseHaResponseLine_when_ack_then_ack', () => {
    assert.deepStrictEqual(parseHaResponseLine('{"response":"ack"}'), { kind: 'ack' });
  });

  test('parseHaResponseLine_when_error_then_coded_error', () => {
    const response = parseHaResponseLine('{"response":"error","vCode":"V4108","message":"swap refused"}');

    assert.strictEqual(response.kind, 'error');
    if (response.kind === 'error') {
      assert.strictEqual(response.error.vCode, 'V4108');
      assert.strictEqual(response.error.message, 'swap refused');
    }
  });

  test('parseHaResponseLine_when_v4111_then_carries_minimum_demonstrated', () => {
    const response = parseHaResponseLine(
      '{"response":"error","vCode":"V4111","message":"budget cannot be honored","minimumDemonstrated":15}',
    );

    assert.strictEqual(response.kind, 'error');
    if (response.kind === 'error') {
      assert.strictEqual(response.error.minimumDemonstrated, 15);
    }
  });

  test('parseHaResponseLine_when_status_missing_alarms_then_throws', () => {
    assert.throws(() => parseHaResponseLine('{"response":"haStatus","standalone":false}'), HotEditProtocolError);
  });

  test('parseHaResponseLine_when_barrier_module_malformed_then_throws', () => {
    assert.throws(
      () => parseHaResponseLine('{"response":"haBarrier","modules":[{"module":0}]}'),
      HotEditProtocolError,
    );
  });

  test('parseHaResponseLine_when_not_json_or_unknown_then_throws', () => {
    assert.throws(() => parseHaResponseLine('not json'), HotEditProtocolError);
    assert.throws(() => parseHaResponseLine('{"response":"huh"}'), HotEditProtocolError);
  });
});
