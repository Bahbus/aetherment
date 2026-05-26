# Aetherment UI Rewrite Architecture

## Module boundaries

The plugin UI internals are now organized into three layers:

1. **Domain / services** (`aetherment/src/view/mods_layers/domain.rs`)
   - Owns operations that mutate external systems or shared backend state.
   - Exposes `ModsDomainOps` as an explicit interface for UI-triggered actions (apply queue, enable/disable, import, finalize apply, redraw).
   - Default implementation `BackendModsDomain` adapts existing backend calls.

2. **View-model / state orchestration** (`aetherment/src/view/mods_layers/state.rs`)
   - Owns UI-independent state for the Mods workflow (`ModsViewModel`).
   - Owns validation and derived-state helpers (e.g. preset name validation, category selection normalization).
   - Contains no rendering calls and no direct backend calls.

3. **KamiToolKit views (render + binding)** (`aetherment/src/view/mods_layers/views.rs` and `aetherment/src/view/mods.rs`)
   - Performs rendering and binds user input to view-model/domain interfaces.
   - Delegates side effects through `ModsDomainOps` instead of calling backend APIs directly for UI-triggered operations.

## Dependency direction rules

Dependencies must remain one-directional:

- `views` -> `state`
- `views` -> `domain` (via `ModsDomainOps`)
- `state` -X-> `views`
- `domain` -X-> `views`
- `state` -X-> direct backend / filesystem / remote integration

Practical rule: any code that needs `egui` widgets stays in views; any code that needs irreversible side effects must be represented in domain interfaces.

## Side-effect boundaries

### Plugin config access

- **Allowed in view-model orchestration only for local selection/state derivation**.
- Persisting user configuration should be mediated through explicit domain/service interfaces in future migrations.
- Current transitional usage in `mods.rs` should be gradually migrated behind interfaces.

### Filesystem access

- **Allowed only in domain/services**.
- UI may collect file paths (input binding), but actual file processing/import execution must happen through domain methods (`import_mods`).

### External integrations (backend, IPC, notifications)

- **Allowed only in domain/services** for operations initiated by UI.
- Views call abstract operations (`ModsDomainOps`) and avoid direct integration calls for these triggers.
- Future UIs (web, CLI, tests) can reuse behavior by swapping `ModsDomainOps` implementations.

## Migration notes

- This change introduces a first vertical slice for the Mods feature and is intentionally incremental.
- Additional features (presets, notifications, remote settings, collection management) should follow the same separation pattern.
- During migration, preserve functional parity by moving logic behind interfaces before changing behavior.
