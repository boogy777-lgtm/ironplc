# Plan: VS Code Activity Bar Commands Panel

## Goal

Give the IronPLC extension a permanent presence in the VS Code activity bar
and expose its registered commands as clickable controls in a tree view, so
users no longer depend on the Command Palette for Run and Hot Edit actions.

## Architecture

Two contribution points and two new TypeScript modules; no command handler
is touched and no guard is re-implemented.

1. **Activity bar container + view** (`package.json`):
   `contributes.viewsContainers.activitybar` declares container id
   `ironplc` (title "IronPLC", icon `images/ironplc-activity.svg`), and
   `contributes.views.ironplc` declares view `ironplc.actions`
   ("Commands"). The icon is a single-color SVG using `currentColor`, the
   form VS Code themes correctly.

2. **Pure tree model** (`src/iplcViewLogic.ts`): a vscode-free module that
   builds the static model — groups "Program" and "Hot Edit", each item
   carrying `{ id, label, icon, command }` where `command` is an existing
   registered command id. One list of group descriptors is the single
   source; the unit test asserts every command id in the model exists in
   `package.json` `contributes.commands` (and vice versa for the exposed
   subset), so the panel cannot drift from the declarations.

3. **Provider glue** (`src/iplcViewProvider.ts`): a `TreeDataProvider`
   that maps the model to `vscode.TreeItem`s (label, `ThemeIcon` from the
   item's codicon, `command`), grouped under collapsible group nodes.
   Registered with `vscode.window.registerTreeDataProvider` in
   `extension.ts` next to the hot-edit registration, unconditionally like
   the other command surfaces. Existing handlers already validate session
   state, so the tree adds no guards.

**Activation.** `engines.vscode` is `^1.75.0`; since 1.74 VS Code
auto-generates `onView` activation events for contributed views, so no
explicit `activationEvents` entry is added.

**No refresh action.** The model is static (availability belongs to the
handlers), so a `view/title` refresh command would add a new command id
that the structural invariants require to be tested, for no behavior.
Omitted deliberately.

## Prefactoring

None. The change is additive: new modules, new contributions, one
registration line in `activate`. No existing module needs reshaping.

## File Map

| File | Change |
|------|--------|
| `integrations/vscode/images/ironplc-activity.svg` | New single-color activity-bar icon |
| `integrations/vscode/src/iplcViewLogic.ts` | New pure model builder |
| `integrations/vscode/src/iplcViewProvider.ts` | New TreeDataProvider glue |
| `integrations/vscode/src/test/unit/iplcViewLogic.test.ts` | New unit tests for the model |
| `integrations/vscode/src/extension.ts` | Register the view provider |
| `integrations/vscode/package.json` | Add `viewsContainers` and `views` contributions |

## Tasks

- [ ] Write plan (this commit)
- [ ] Add the activity-bar icon and both contribution points
- [ ] Add `iplcViewLogic.ts` + unit tests
- [ ] Add `iplcViewProvider.ts` and register it in `extension.ts`
- [ ] Run `npm run compile` && `npm run lint` && `npm run test:unit`
      (zero new failures; 80% line gate holds)
- [ ] Merge fast-forward into `lint-fences` and push the fork
