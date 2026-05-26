# Desktop Sync Integration Decision Criteria

This document defines a **go / no-go decision gate** for desktop sync integration in the native UI rewrite.

## Goal

Encapsulate desktop sync behind a dedicated adapter/service boundary so the native UI rewrite can continue independently from desktop-integration details.

- **Boundary requirement:** Native UI must consume a `DesktopSyncService` interface (or equivalent) owned by the rewrite layer.
- **Implementation requirement:** Desktop-specific sync logic must live in an adapter implementation behind that interface.
- **Plugin-only compatibility requirement:** If adapter is absent/disabled, plugin-only workflows must remain functional with predictable degraded behavior.

---

## Decision outcomes

- **GO (keep integration):** Keep desktop sync in scope for rewrite, delivered only through the adapter boundary.
- **NO-GO (approve removal from rewrite scope):** Temporarily remove desktop sync from rewrite scope **only** when criteria below show disproportionate rewrite risk **and** a migration path is in place.

> Removal is not approved by default. It requires explicit evidence across all criteria.

---

## Required evaluation criteria

Assess each criterion using the rubric and record evidence in the Decision Record section.

### 1) Integration complexity (estimated implementation effort)

Estimate effort for implementing adapter boundary + first functional desktop adapter in the rewrite.

Suggested scoring:
- **Low (1):** Small interface, low coupling, straightforward event/data mapping, no blocking unknowns.
- **Medium (2):** Moderate mapping/refactor complexity, some asynchronous/state coordination, limited unknowns.
- **High (3):** Significant coupling to legacy internals, substantial refactor, multi-system coordination, unresolved unknowns.

Evidence to capture:
- Affected modules and seams.
- Unknowns/spikes required.
- Rough estimate (engineer-days or sprint fraction).

### 2) Ongoing maintenance burden

Estimate steady-state cost of keeping desktop sync integrated post-rewrite.

Suggested scoring:
- **Low (1):** Stable contracts, low change frequency, minimal platform-specific drift.
- **Medium (2):** Occasional breakage from upstream/UI changes; manageable adapter updates.
- **High (3):** Frequent breakage, brittle coupling, high test/support overhead.

Evidence to capture:
- Historical churn or incident patterns (if available).
- Test coverage strategy and ownership.
- Platform/version drift risks.

### 3) Failure-mode severity in plugin-only usage

Assess impact if desktop sync fails while users run plugin-only flows.

Suggested scoring:
- **Low (1):** Failures are isolated; core plugin operations continue; clear recoverability.
- **Medium (2):** Partial workflow degradation; recoverable with user action/workarounds.
- **High (3):** Failures block key plugin flows, risk data loss/corruption, or produce confusing silent failure.

Evidence to capture:
- Failure scenarios and expected fallback behavior.
- User-visible error messaging quality.
- Data safety guarantees.

### 4) User impact if removed

Assess impact of removing desktop sync from rewrite scope.

Suggested scoring:
- **Low (1):** Small user segment affected; acceptable short-term workarounds.
- **Medium (2):** Noticeable friction for a meaningful segment; mitigations available.
- **High (3):** Major workflow loss, high support burden, or trust/reliability concerns.

Evidence to capture:
- Affected personas/use-cases.
- Support/docs load expected.
- Compatibility behavior and migration friction.

---

## Approval rule for removal (NO-GO)

You may approve removal of desktop sync from rewrite scope **only if all conditions are true**:

1. **Disproportionate rewrite risk is demonstrated**
   - Integration complexity is **High (3)**, and
   - Ongoing maintenance burden is **High (3)** *or* credibly trending High.
2. **Plugin-only safety is acceptable without desktop sync in rewrite path**
   - Failure-mode severity in plugin-only usage is **not High** after defined fallback behavior.
3. **User migration path is fully defined and ready**
   - Clear user messaging, compatibility behavior, and support plan are documented (see next section).

If any condition is unmet, decision defaults to **GO (keep integration via adapter boundary)**.

---

## Mandatory migration path requirements (for removal approval)

If removal is approved, all of the following must be delivered before or with rollout:

1. **Clear messaging**
   - Release notes and in-app notice stating what changed, who is affected, and timeline.
   - Specific alternatives/workarounds for impacted users.
2. **Compatibility behavior**
   - Deterministic behavior when desktop sync is unavailable (no silent failures).
   - Graceful degradation in UI: disabled states, actionable error/help text, and non-blocking plugin-only core actions.
3. **Operational support**
   - Support playbook/FAQ updates.
   - Telemetry/monitoring for migration pain signals.
4. **Re-entry criteria**
   - Conditions under which desktop sync is reconsidered for reintegration.

Without this migration package, removal cannot be approved.

---

## Decision record template

Copy and complete for each decision review.

```md
### Desktop Sync Decision Review — <YYYY-MM-DD>

- Integration complexity: <Low/Medium/High> (<1/2/3>)
  - Evidence: ...
- Ongoing maintenance burden: <Low/Medium/High> (<1/2/3>)
  - Evidence: ...
- Failure-mode severity in plugin-only usage: <Low/Medium/High> (<1/2/3>)
  - Evidence: ...
- User impact if removed: <Low/Medium/High> (<1/2/3>)
  - Evidence: ...

**Decision:** <GO keep integration | NO-GO approve removal>

**Rationale:**
- ...

**If NO-GO (removal approved), migration path checklist:**
- [ ] Messaging ready (release notes + in-app copy)
- [ ] Compatibility behavior validated
- [ ] Support docs/playbook updated
- [ ] Monitoring/telemetry plan active
- [ ] Re-entry criteria documented
```
