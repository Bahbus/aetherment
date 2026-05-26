# UI Rewrite Fallback Decisions

This document records **intentional behavior changes** where the native KamiToolKit-based UI cannot (or should not) mirror the legacy ImGui behavior exactly.

## Scope and QA usage

- Source of feature IDs: `docs/ui-rewrite/feature-parity.md`.
- Only feature IDs marked `Alternative implemented` (or otherwise explicitly approved as non-parity) belong here.
- QA should validate both:
  1. Legacy behavior is intentionally replaced.
  2. Acceptance criteria below pass in the rewrite UI.

---

## ACT-008 — Open support link from Mods pane

### Legacy behavior

- **Control type:** Per-pane action button in the Mods view (`Support this project`).
- **Layout:** Button lived in the Mods pane action area.
- **Event model:** Direct click event opened an external URL.
- **Lifecycle:** Available only while the Mods pane was active.

### Limitation preventing exact mirroring

The rewrite consolidates external links into a global Help/About action surface. Keeping a duplicate pane-local support button would create redundant controls and inconsistent placement across panes.

- **Control-type mismatch:** Pane-local button vs. global menu/action item.
- **Layout mismatch:** Legacy contextual placement is replaced by persistent global placement.
- **Lifecycle mismatch:** Action is now available across the app, not only in Mods.

### Alternative interaction designs considered

1. **Global Help/About action only** (chosen)
   - Single location for all support/documentation links.
2. **Dual-entry design** (global action + Mods shortcut)
   - Preserves muscle memory but duplicates action surface.
3. **Contextual inline card in Mods header**
   - Keeps contextuality, but adds layout noise and one-off component behavior.

### User impact evaluation

- **Extra clicks:**
  - Users already in Mods may need one additional click (open Help/About surface first).
- **Discoverability:**
  - Improves for non-Mods contexts due to always-available global entry point.
  - Slightly reduced for users expecting the old Mods-local location.
- **Performance:**
  - No meaningful runtime/perf impact.
- **Error risk:**
  - Lower long-term UI consistency risk (single maintained entry point).
  - Minimal user error risk; action semantics (open external URL) unchanged.

### Default fallback decision

Use **Global Help/About action only** as the default fallback for `ACT-008`.

### Acceptance criteria (QA)

1. From any primary pane (including non-Mods panes), user can open Help/About and trigger the Support link.
2. Triggering the action opens the same external destination used by legacy behavior.
3. Mods pane no longer contains a duplicate Support button.
4. Keyboard/controller navigation can reach and activate the global Support action.
5. If URL open fails, user receives visible non-crashing error feedback consistent with rewrite notification patterns.

---

## Current decision inventory status

At time of writing, `ACT-008` is the only feature ID with an approved non-parity fallback in the parity checklist. Add new sections in this file whenever another feature is marked `Alternative implemented`.
