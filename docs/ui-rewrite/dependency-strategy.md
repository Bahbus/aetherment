# UI Rewrite Dependency Strategy

This document compares two ways to consume the planned UI dependency for the rewrite:

1. **External dependency reference** (consume upstream package/repository directly).
2. **Vendored source subset** (copy a pinned subset of upstream sources into this repository).

## Decision summary

- **Default strategy:** **External dependency reference**.
- **Why:** Lowest maintenance cost and upgrade burden while keeping compatibility risk manageable through pinning and lockfiles.
- **Rollback trigger:** Move to a vendored subset only if reproducibility or compatibility incidents exceed the rollback criteria in [Rollback criteria](#rollback-criteria).

---

## Option A — External dependency reference

### What it means

Use package-manager/repository references (NuGet/`PackageReference`, Cargo crate/git dependency, etc.) and pin versions/commits in project manifests and lockfiles.

### Evaluation

| Dimension | Assessment |
|---|---|
| Build complexity | **Low to Medium.** Minimal repository churn; existing package restore flows do most work. CI must guarantee deterministic restore and cache behavior. |
| Upgrade burden | **Low.** Upgrade by changing version/commit and validating parity; no manual source re-sync. |
| Compatibility risk | **Medium.** API or behavior drift can appear on upgrade, but can be controlled by strict pinning and staged update tests. |
| Maintenance cost | **Low.** No local fork surface to own; mostly version governance and regression testing. |

### Pros

- Fastest adoption path.
- Smaller repo footprint and fewer locally owned copies.
- Security fixes and upstream improvements are easier to pull.

### Cons

- Requires stable restore tooling in CI/offline scenarios.
- Upstream packaging choices can affect integration timing.

---

## Option B — Vendored source subset

### What it means

Copy only the required upstream files into a local `vendor/` (or equivalent) tree and build against those local sources.

### Evaluation

| Dimension | Assessment |
|---|---|
| Build complexity | **Medium to High.** Requires local layout conventions, include/path rewrites, and possibly custom build steps. |
| Upgrade burden | **High.** Every upstream update becomes a manual import/reconcile task with re-validation. |
| Compatibility risk | **Low to Medium (runtime), High (integration drift).** Runtime can be stable due to full pinning, but local divergence from upstream can accumulate and create merge pain. |
| Maintenance cost | **High.** Team owns provenance tracking, conflict resolution, and security patch cherry-picks. |

### Pros

- Strong reproducibility and offline build behavior.
- Full control over exactly what is compiled.

### Cons

- Highest long-term ownership burden.
- Harder to keep aligned with upstream bug/security fixes.

---

## Recommended default strategy

Adopt **External dependency reference** as the default.

### Guardrails for the default

1. Pin exact versions/commits (no floating ranges).
2. Commit lockfiles and fail CI when lock drift appears unexpectedly.
3. Add compatibility smoke tests for UI bootstrap and critical interaction flows.
4. Track upstream release notes for breaking changes before upgrades.

---

## Rollback criteria

Switch from external reference to vendored subset only when one or more of the following are true across a rolling 30-day window:

1. **Reproducibility incidents:** 3 or more CI failures attributable to upstream unavailability, yanked artifacts, or non-deterministic restore behavior.
2. **Compatibility incidents:** 2 or more production- or release-blocking regressions directly caused by upstream packaging/version changes despite pinning.
3. **Policy/compliance blocker:** A legal/security requirement mandates source vendoring or provenance controls not achievable through external references.
4. **Release risk threshold breach:** A scheduled release is blocked for more than 2 business days by dependency retrieval/packaging issues.

If any criterion is met, open a migration task to vendor a minimal subset and freeze external upgrades until parity is re-established.

---

## Exact project files that must change for adoption

The following files are the expected touch points to adopt the **external dependency reference** strategy for the UI rewrite in this repository.

### 1) Rust workspace / crate dependency manifests

- `Cargo.toml` (workspace-level dependency policy, shared versions/features where applicable).
- `plugin/Cargo.toml` (plugin crate dependency additions/feature flags).
- `aetherment/Cargo.toml` (core crate dependency additions if shared runtime UI pieces are needed).
- `renderer/Cargo.toml` (only if renderer-side UI integration requires direct dependency linkage).
- `Cargo.lock` (pinned resolved graph update).

### 2) .NET plugin project dependency references

- `plugin/plugin/Aetherment.csproj` (`PackageReference` additions/updates and related build props/targets).
- `plugin/plugin/Aetherment.json` (if plugin metadata/version compatibility constraints must be updated with the dependency shift).

### 3) Native/build glue

- `plugin/build.rs` (only if dependency introduces/generated native bindings or build-time codegen path changes).

### 4) Source-level include/import migration (as needed)

- `plugin/src/penumbradraw/imgui_bindings.rs` (if binding entry points change).
- `plugin/src/penumbradraw/imgui.rs` (if API usage changes).
- `plugin/plugin/Native.cs` (if managed/native interop signatures change).
- `plugin/plugin/Dalamud.cs` (if service registration/bootstrap changes with new dependency wiring).

### 5) CI/build scripts (if restore/build invocation changes)

- `build_windows.sh` (if explicit restore/build arguments must be added for pinned source resolution).

## If rollback to vendored subset is activated

Additional files/directories likely required:

- New vendor tree (proposed): `plugin/vendor/<dependency-name>/...`.
- `plugin/Cargo.toml` and/or `plugin/plugin/Aetherment.csproj` updated to reference local path/project sources instead of external references.
- `plugin/build.rs` updated for vendored include/lib paths.

Keep vendored footprint minimal and document upstream commit provenance in a `README` inside the vendor directory.
