# KamiToolKit Pattern Notes for Aetherment UI Rewrite

> Scope note: this is a **structure-first** pattern inventory intended to map KamiToolKit-style UI composition patterns to Aetherment parity requirements. It focuses on reusable interaction patterns and integration caveats rather than exact API signatures.

## Pattern matrix

| Pattern area | KamiToolKit module/type candidates | Minimal usage pattern | Constraints / caveats for Aetherment parity | Parity mapping |
|---|---|---|---|---|
| Multi-pane layouts | `Window`/`Panel` composition primitives, splitter/dock helpers, child-region containers | Create a root window, split into persistent left-nav + right-detail panes, and bind selected item state into right pane renderer. Keep pane ratio in persisted config. | Aetherment already has custom splitter behavior and selection-driven pages; parity requires preserving collection/mod selection context when pane layout changes or reloads. | `ACT-001`, `ACT-002`, `ACT-005` |
| Item collections with filtering/search | List/collection view widgets, search input, filter chips/tabs, virtualized row drawing | Keep source list immutable per frame, apply search text + category filter to derive visible rows, and draw selectable rows with stable item IDs. | Must preserve Aetherment’s sorted mod ordering and category-tab option filtering semantics; dynamic option schemas must stay data-driven (no hardcoded per-mod UI). | `ACT-002`, `ACT-005`, `ACT-006`, `IE-002` |
| Modal workflows (confirm/import/export) | Modal dialog abstractions, popup confirmation helpers, file-picker bridge components, clipboard action wrappers | Trigger modal from row/action button, stage pending action payload in local state, confirm/cancel to commit side effect, and close modal deterministically. | Preset import/export in Aetherment is clipboard-based today; if adopting toolkit modal APIs, preserve base64 JSON compatibility and add explicit invalid-payload error UX. Import-mod flow is file-dialog + async task, so modal shouldn’t block background progress updates. | `PM-006`, `PM-007`, `PM-005`, `IE-001` |
| Async operation feedback | Busy overlays/spinners, progress bar widgets, toast/notice queue, error banner components | Start async op with progress handle, render non-blocking progress region (main + subtask), push completion/error messages to dismissible queue, and expose retry/ack actions. | Must match current two-level progress semantics and auto-apply notification lifecycle; backend apply queue and plugin bridge errors need surfaced without deadlocking UI thread. | `ST-001`, `ST-002`, `ST-003`, `ST-005`, `ACT-004` |
| Persistent view-state + config bindings | Observable/bindable settings model, serialized view-state store, key/value config adapter | Bind controls directly to state model with dirty tracking; persist on change or debounced flush; restore on window reopen/startup. | Needs strict separation between per-user UI preferences and per-collection/per-mod settings. Plugin-side open-on-launch handshake and redraw flags must remain compatible with existing config + native bridge calls. | `TG-004`, `TG-005`, `TG-006`, `INT-004`, `PM-001` |

## Integration guidance for Aetherment

### 1) Favor adapter layer over direct rewrite
Use thin adapters that translate Aetherment domain state (`mod_manager`, collection settings, remote settings, task progress) into KamiToolKit view models, then translate user intents back into existing mutation APIs.

Why: this preserves current backend side effects (Penumbra IPC calls, save/apply behavior, notification timing) while still allowing UI component reuse.

### 2) Keep schema-driven option rendering as first-class
The highest-risk parity gap is option editing (`single/grouped/path/slider/color/multi-toggle`) because Aetherment renders from metadata. Any KamiToolKit componentization should expose a generic “option descriptor -> control renderer” path and avoid static form definitions.

### 3) Model modal actions as explicit state machines
For preset delete/import/export and mod import, use explicit states (`idle -> staged -> running -> success/error`) to avoid accidental double-execution when frames redraw.

### 4) Separate ephemeral UI state from persisted config
- Ephemeral: selected tab, active modal, search text, spinner visibility.
- Persisted: open-on-launch, auto-apply flags, split ratios if adopted, collection selection defaults.

Persisting too much ephemeral state can create surprising behavior after plugin reloads.

## Suggested next validation pass
Before implementation, verify concrete KamiToolKit type names and signatures in the upstream repository and replace placeholder module/type labels above with exact references (namespace + file path), then add code snippets for each pattern.
