# UI Rewrite Parity Checklist

This checklist operationalizes the feature groups from the parity matrix in `docs/ui-rewrite/feature-parity.md`.

## Migration guardrail (must hold)

Do **not** remove legacy UI paths until all **critical** feature IDs are either:

1. **Parity achieved**, or
2. **Alternative implemented** and explicitly approved.

Critical IDs currently include: `PM-001`–`PM-007`, `IE-001`–`IE-002`, `TG-001`–`TG-003`, `ACT-001`–`ACT-007`, `ST-001`–`ST-004`, and `INT-001`–`INT-004`.

---

## Required implementation order (per feature group)

For every feature group below, execute work in this order:

1. **View-model commands/state first**
   - Add typed commands/events and state transitions.
   - Ensure state can be tested without UI rendering.
2. **KamiToolKit view bindings second**
   - Bind controls to existing commands/state only.
   - Avoid embedding business logic in view callbacks.
3. **Explicit parity checklist result third**
   - Mark each feature ID using one of the required status values.
4. **Unsupported behavior handling**
   - If direct parity is not possible, mark as `Alternative implemented` with rationale and behavior notes.
   - If blocked, mark as `Deferred` with blocker and owner.

---

## Status legend (required)

Every feature ID **must** be marked as one of:

- **Parity achieved**
- **Alternative implemented** *(include behavior notes + rationale + proposed alternative)*
- **Deferred** *(include blocker + owner)*

---

## Feature-group parity checklist

### Preset management (`PM-*`)

| Feature ID | Status | Parity notes / alternative behavior | Blocker | Owner |
|---|---|---|---|---|
| PM-001 | Deferred | — | Rewrite VM preset-resolution pipeline not yet merged. | UI Rewrite Team |
| PM-002 | Deferred | — | Apply-preset command wiring to backend pending command bus integration. | UI Rewrite Team |
| PM-003 | Deferred | — | User-preset apply flow depends on persisted VM state adapter. | UI Rewrite Team |
| PM-004 | Deferred | — | Create/overwrite semantics need conflict policy in VM layer. | UI Rewrite Team |
| PM-005 | Deferred | — | Delete action requires confirm-dialog command contract. | UI Rewrite Team |
| PM-006 | Deferred | — | Clipboard abstraction for VM command not finalized. | UI Rewrite Team |
| PM-007 | Deferred | — | Clipboard decode/validation service contract pending. | UI Rewrite Team |

### Import/export (`IE-*`)

| Feature ID | Status | Parity notes / alternative behavior | Blocker | Owner |
|---|---|---|---|---|
| IE-001 | Deferred | — | File-picker command abstraction and async install orchestration pending. | UI Rewrite Team |
| IE-002 | Deferred | — | Depends on shared file-dialog filter policy object. | UI Rewrite Team |

### Toggles (`TG-*`)

| Feature ID | Status | Parity notes / alternative behavior | Blocker | Owner |
|---|---|---|---|---|
| TG-001 | Deferred | — | VM command for collection-scoped mod enable not yet integrated. | UI Rewrite Team |
| TG-002 | Deferred | — | Remote settings adapter lifecycle not finalized in VM state. | UI Rewrite Team |
| TG-003 | Deferred | — | Schema-driven multi-toggle reducer not yet implemented. | UI Rewrite Team |
| TG-004 | Deferred | — | Settings VM migration not started for global config toggles. | UI Rewrite Team |
| TG-005 | Deferred | — | Depends on TG-004 plus redraw policy command design. | UI Rewrite Team |
| TG-006 | Deferred | — | Native plugin startup handshake wiring pending rewrite bridge. | UI Rewrite Team |

### Per-item actions (`ACT-*`)

| Feature ID | Status | Parity notes / alternative behavior | Blocker | Owner |
|---|---|---|---|---|
| ACT-001 | Deferred | — | Collection selector VM command/state still on legacy direct mutation path. | UI Rewrite Team |
| ACT-002 | Deferred | — | Selection VM state and texture-cache invalidation side effect not separated yet. | UI Rewrite Team |
| ACT-003 | Deferred | — | Reload command requires busy-state gate in unified command bus. | UI Rewrite Team |
| ACT-004 | Deferred | — | Manual apply command depends on new task orchestration API. | UI Rewrite Team |
| ACT-005 | Deferred | — | Category tab state not migrated to rewrite VM store. | UI Rewrite Team |
| ACT-006 | Deferred | — | Generic schema option editor reducers and command model incomplete. | UI Rewrite Team |
| ACT-007 | Deferred | — | Style-apply side effect command contract with optional Dalamud integration pending. | UI Rewrite Team |
| ACT-008 | Alternative implemented | Support link moved to centralized Help/About action surface instead of Mods pane button; behavior remains one-click external URL open with consistent global placement. Rationale: avoids duplicating external-link controls across panes and improves discoverability. | — | UI Rewrite Team |

### Status/errors (`ST-*`)

| Feature ID | Status | Parity notes / alternative behavior | Blocker | Owner |
|---|---|---|---|---|
| ST-001 | Deferred | — | Task progress VM stream adapter not yet connected to new view state. | UI Rewrite Team |
| ST-002 | Deferred | — | Message stack acknowledgement command semantics not finalized. | UI Rewrite Team |
| ST-003 | Deferred | — | Auto-apply notification lifecycle reducer not implemented. | UI Rewrite Team |
| ST-004 | Deferred | — | Requirements evaluation output not surfaced through rewrite VM projection. | UI Rewrite Team |
| ST-005 | Deferred | — | Native fatal-error fallback is host-side path; rewrite ownership split unresolved. | Plugin Host Team |

### Integration points (`INT-*`)

| Feature ID | Status | Parity notes / alternative behavior | Blocker | Owner |
|---|---|---|---|---|
| INT-001 | Deferred | — | Penumbra panel takeover callbacks require host/plugin bridge updates. | Plugin Host Team |
| INT-002 | Deferred | — | Event subscription migration to rewrite command bus pending IPC adapter refactor. | Plugin Host Team |
| INT-003 | Deferred | — | Collection/mod IPC bridge wrappers not yet bound to new VM commands. | Plugin Host Team |
| INT-004 | Deferred | — | Startup handshake config bridge not migrated. | Plugin Host Team |
| INT-005 | Deferred | — | UI color hook lifecycle currently outside rewrite scope; ownership decision pending. | Plugin Host Team |

---

## Group completion checklist (fill for each implementation batch)

For each feature group (`PM`, `IE`, `TG`, `ACT`, `ST`, `INT`), attach results in this format:

- **View-model commands/state complete:** Yes/No (+ commit/PR reference)
- **KamiToolKit view bindings complete:** Yes/No (+ commit/PR reference)
- **Parity status reviewed for every feature ID:** Yes/No
- **Unsupported behaviors documented with rationale + proposed alternatives:** Yes/No

