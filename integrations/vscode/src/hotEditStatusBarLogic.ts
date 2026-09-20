/**
 * The state of the IronPLC hot-edit status-bar item, as a pure function so
 * the idle/active/pending/attention decisions are unit-testable without the
 * VS Code API. The caller (`hotEdit.ts`) owns the actual `StatusBarItem`
 * and maps `warning` onto a theme-safe `ThemeColor`.
 */

import { formatStatusDetail, formatStatusText, HotEditStatus } from './hotEditSession';

/** The icons the item uses; codicon ids without the `$(...)` wrapper. */
export const IDLE_ICON = 'debug-start';
export const ACTIVE_ICON = 'sync';

/** What the status-bar item should show and do right now. */
export interface HotEditStatusBarState {
  /** Codicon-prefixed label, e.g. `$(sync) Hot Edit: normal (gen 3)`. */
  text: string;
  tooltip: string;
  /** The command a click runs. */
  command: string;
  /** Whether the item is shown; always true so idle advertises Start Session. */
  visible: boolean;
  /** Whether the item should use the warning background (a candidate is staged). */
  warning: boolean;
}

/**
 * Maps the current session status to the item state. `undefined` means no
 * session: the item advertises Start Session. An active session shows the
 * pending candidate when one is staged, otherwise the mode and generation.
 */
export function hotEditStatusBar(state: HotEditStatus | undefined): HotEditStatusBarState {
  if (!state) {
    return {
      text: `$(${IDLE_ICON}) Hot Edit: Start Session`,
      tooltip: 'Start an IronPLC hot edit session',
      command: 'ironplc.startHotEditSession',
      visible: true,
      warning: false,
    };
  }
  const pending = state.candidate !== null;
  return {
    text: `$(${ACTIVE_ICON}) Hot Edit: ${pending ? '1 pending' : formatStatusText(state)}`,
    tooltip: formatStatusDetail(state),
    command: 'ironplc.showHotEditStatus',
    visible: true,
    warning: pending || state.migration,
  };
}
