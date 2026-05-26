# KamiToolKit Capability Map for Aetherment Parity Features

This document maps each parity feature ID in `feature-parity.md` to intended KamiToolKit patterns/components.

Legend:
- **direct**: KamiToolKit has a clear native pattern/component fit.
- **partial**: Feasible with adapter glue or custom composition around KamiToolKit primitives.
- **unsupported**: No reliable direct pattern in KamiToolKit for the required behavior; use fallback flow.

## Feature-by-feature mapping

| Feature ID | Capability | KamiToolKit patterns/components | Expected constraints (layout / modal / async) | Fallback if not direct |
|---|---|---|---|---|
| AETH-UI-001 | direct | `ComboBox`, `Label`, derived-state presenter module (`PresetResolverViewModel`) | **Layout:** detail header row must keep preset label visible at narrow widths. **Modal:** none. **Async:** none; recompute on settings mutation. | N/A |
| AETH-UI-002 | direct | `ComboBox` selection action, option-apply command (`ApplyPresetCommand`) | **Layout:** preset selector in persistent detail toolbar. **Modal:** none for normal apply. **Async:** may enqueue backend apply signal after local mutation. | N/A |
| AETH-UI-003 | direct | `ComboBox`/`MenuItem`, same command pipeline as built-in presets | **Layout:** unified preset list with type grouping. **Modal:** none. **Async:** same apply queue semantics as built-in. | N/A |
| AETH-UI-004 | partial | `TextInput`, `IconButton`, inline validation helper | **Layout:** editor row must coexist with selector in compact widths. **Modal:** optional confirm on overwrite. **Async:** save-to-store should be debounced or explicit submit. | Use explicit **Save as new / Overwrite selected** two-button flow when inline overwrite detection is ambiguous. |
| AETH-UI-005 | partial | `MenuItem` + destructive `IconButton` / contextual menu action | **Layout:** avoid accidental taps in dense popup menu. **Modal:** prefer confirm dialog/sheet for destructive action. **Async:** local store save after deletion. | If no reliable destructive affordance, move to dedicated “Manage Presets” panel with explicit delete controls. |
| AETH-UI-006 | partial | Command button + platform clipboard adapter module (`ClipboardService`) | **Layout:** action in preset action cluster. **Modal:** toast/snackbar for copied/failed states. **Async:** clipboard APIs may be async/fallible by host. | Show export payload in read-only text area with manual copy button/input focus fallback. |
| AETH-UI-007 | partial | Command button + clipboard decode/import service (`PresetImportService`) | **Layout:** action near preset controls. **Modal:** parse/replace confirmation dialog where name collision occurs. **Async:** clipboard read + decode off UI thread. | Provide “Paste payload” multiline dialog and import from text when clipboard access fails. |
| AETH-UI-008 | unsupported | Intended: host file-dialog bridge + task list/progress widgets | **Layout:** import action in left command bar. **Modal:** system file picker required. **Async:** multi-file install queue with progress reporting and cancellation. | Replace direct picker with host-provided “drop files here” surface or external “watched import folder” workflow. |
| AETH-UI-009 | direct | `Checkbox`, collection/mod state binding adapter | **Layout:** top-of-detail status row. **Modal:** none. **Async:** toggle may enqueue backend apply and temporary busy state. | N/A |
| AETH-UI-010 | direct | Conditional `Checkbox`, remote-origin badge component | **Layout:** show only when origin metadata exists; preserve row spacing stability. **Modal:** none. **Async:** persistence write can be background with optimistic UI. | N/A |
| AETH-UI-011 | partial | Dynamic form renderer over `RadioGroup`, `Select`, `CheckboxGroup`, `PathPicker` adapter, `Slider`, `ColorPicker` | **Layout:** schema-driven grid/list with category scoping. **Modal:** path picker and color picker may require popover/modal. **Async:** value changes may enqueue delayed apply; path resolution may involve IO. | For unsupported control subtype, render generic key/value editor row with textual value entry and reset action. |
| AETH-UI-012 | direct | `TabBar` / segmented control bound to option category filter | **Layout:** scrollable tabs for large category counts. **Modal:** none. **Async:** none aside from option list recomposition. | N/A |
| AETH-UI-013 | direct | Global `ComboBox` in sidebar command strip | **Layout:** must remain visible near import/reload actions. **Modal:** none. **Async:** collection switch can trigger state reload/apply indicators. | N/A |
| AETH-UI-014 | direct | `ListView` + selection model + detail-pane presenter | **Layout:** split-pane master/detail with persisted splitter ratio. **Modal:** none. **Async:** preview assets may lazy-load. | N/A |
| AETH-UI-015 | direct | `Button` triggering reload command + busy/disabled state | **Layout:** left action cluster. **Modal:** none. **Async:** long-running reload task with progress/error channel. | N/A |
| AETH-UI-016 | direct | Primary `Button`, queue badge, apply command dispatcher | **Layout:** prominent action when queue non-empty. **Modal:** optional confirm only if concurrent apply in progress. **Async:** required background task with status stream. | N/A |
| AETH-UI-017 | direct | Settings `Checkbox` bound to config store | **Layout:** settings form section. **Modal:** none. **Async:** persisted config write (can be fire-and-forget with retry queue). | N/A |
| AETH-UI-018 | direct | Settings `Checkbox` + redraw action toggle | **Layout:** colocated with auto-apply. **Modal:** none. **Async:** redraw call after successful apply; failure should downgrade to warning notification. | N/A |
| AETH-UI-019 | direct | Settings `Checkbox` bound to launch preference | **Layout:** startup behavior section. **Modal:** none. **Async:** persisted config only; host reads at next startup. | N/A |
| AETH-UI-020 | partial | `Button` + `Slider` + integration adapter (`DalamudStyleBridge`) | **Layout:** action block in mod detail. **Modal:** none. **Async:** bridge call may fail/not exist; needs capability probe. | If style bridge absent, expose copyable style summary and guidance text (“apply in host theme settings”). |
| AETH-UI-021 | direct | `ProgressBar` (main/sub) + status text | **Layout:** global top status rail with stable height. **Modal:** none. **Async:** subscribe to task progress stream and throttle UI updates. | N/A |
| AETH-UI-022 | direct | `AlertStack`/`MessageList`, acknowledge `Button` | **Layout:** global message panel with scroll for long stack. **Modal:** none (inline acknowledgements). **Async:** fed by async task/event bus. | N/A |
| AETH-UI-023 | direct | `Snackbar`/notification banner state machine | **Layout:** non-blocking notification area. **Modal:** none. **Async:** driven by apply lifecycle events (start/success/failure). | N/A |
| AETH-UI-024 | direct | Inline warning `Alert` component in detail pane | **Layout:** requirements block near top of mod page. **Modal:** none. **Async:** warning list recomputed on dependency/context changes. | N/A |
| AETH-UI-025 | unsupported | Intended: host-panel interception hooks + foreign draw-surface bridge | **Layout:** requires embedding inside external Penumbra panel region. **Modal:** host-controlled. **Async:** IPC timing/synchronization with host render lifecycle. | Run standalone Aetherment settings pane with deep-link from Penumbra panel instead of takeover rendering. |

## Unsupported items and ranked alternatives

Ranked by expected user impact (highest first):

1. **AETH-UI-025 — Penumbra settings panel takeover** (highest impact)
   - **Recommended alternative:** deep-link CTA in Penumbra panel (“Open Aetherment editor for this mod”), preserving selected mod/context handoff.
   - **Why:** Keeps core editing path available even without embedded host draw interception.

2. **AETH-UI-008 — Import `.aeth` mods from file dialog** (high impact)
   - **Recommended alternative:** drag-and-drop import zone in the Mods sidebar; secondary fallback is watched import directory.
   - **Why:** Maintains import capability without tight coupling to OS picker APIs.
