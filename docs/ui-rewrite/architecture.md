# UI Rewrite Architecture

## Purpose
This document defines the target architecture for the UI rewrite and the boundaries between modules so implementation can be parallelized safely.

---

## Layer 1: Domain / Services

**Responsibility:**
Business logic only. This layer defines the application rules and state transitions independent of UI framework details.

**Contains:**
- Domain entities and value objects
- Use-case services (application services)
- Policy/decision logic
- Stable interfaces (ports) for external concerns (configuration, filesystem, IPC)

**Must not contain:**
- UI rendering code
- View-model shape decisions tied to components
- Framework event objects
- Direct filesystem/IPC calls (except via injected ports/interfaces)

---

## Layer 2: View-model / State

**Responsibility:**
Translate domain capabilities into UI-facing state and commands.

**Contains:**
- Screen/store state models
- Commands/actions and command orchestration
- Derived/computed state
- Input normalization and validation rules for user workflows
- Mapping between domain models and presentation models

**Must not contain:**
- Rendering implementation (KamiToolKit widgets/layout)
- Direct filesystem/IPC calls
- Domain rule duplication already owned by services

---

## Layer 3: KamiToolKit View

**Responsibility:**
Rendering and event binding only.

**Contains:**
- KamiToolKit component composition/layout
- Styling and visual hierarchy
- Binding user events to view-model commands
- Display-only formatting that does not alter business logic

**Must not contain:**
- Business logic decisions
- Cross-screen state orchestration
- Validation logic beyond immediate field-level affordances delegated to view-model
- Filesystem/IPC/configuration access

---

## Dependency Rules (One-way)

Dependency direction is strictly downward in abstraction:

1. **View (Layer 3) -> View-model (Layer 2)**
2. **View-model (Layer 2) -> Domain/Services (Layer 1)**
3. **Domain/Services (Layer 1) -> Ports/abstractions only**

No reverse imports are allowed.

### Allowed dependencies
- Layer 3 may reference Layer 2 public interfaces/types needed for rendering and event wiring.
- Layer 2 may reference Layer 1 services/entities/use-cases.
- Layer 1 may reference shared primitives and abstract ports (interfaces) only.

### Forbidden couplings
- Layer 1 importing any UI or KamiToolKit module.
- Layer 1 importing concrete filesystem/IPC/config implementations.
- Layer 2 importing Layer 3 components/widgets.
- Layer 3 importing Layer 1 directly (must go through Layer 2).
- Any layer reaching across boundaries through global singletons that bypass defined interfaces.

---

## Configuration, Filesystem, and IPC Placement

These concerns are **infrastructure** and must be isolated.

- **Configuration read/write:**
  - Concrete access allowed only in infrastructure adapters/composition root.
  - Domain receives configuration values via constructor/input parameters or domain-facing interfaces.

- **Filesystem access:**
  - Concrete file I/O allowed only in infrastructure adapters implementing domain/view-model ports.
  - View-model and domain must depend on interfaces, not direct file APIs.

- **IPC (process/app boundary):**
  - Concrete IPC clients/servers allowed only in infrastructure adapter modules.
  - View-model invokes IPC-backed behavior through service interfaces.
  - Views never call IPC directly.

---

## Migration Diagram

```text
Current (mixed responsibilities)

[Legacy UI + state + logic + IO]
              |
              v
Target (layered)

+-----------------------------+
| Layer 3: KamiToolKit View   |
| - Render                    |
| - Event binding             |
+-------------+---------------+
              |
              v
+-----------------------------+
| Layer 2: View-model/State   |
| - Commands                  |
| - Derived state             |
| - Validation                |
+-------------+---------------+
              |
              v
+-----------------------------+
| Layer 1: Domain/Services    |
| - Business rules            |
| - Use-cases                 |
| - Ports                     |
+-------------+---------------+
              |
              v
+-----------------------------+
| Infrastructure Adapters     |
| - Config                    |
| - Filesystem                |
| - IPC                       |
+-----------------------------+
```

Migration sequence:
1. Extract domain rules from legacy UI into Layer 1 services.
2. Introduce Layer 2 view-models that wrap Layer 1 use-cases.
3. Rebuild UI screens in Layer 3 using KamiToolKit against Layer 2 contracts.
4. Route config/filesystem/IPC through adapters and inject via composition root.
5. Remove legacy direct couplings once replacement paths are stable.

---

## Module Ownership (for Parallel Assignment)

To avoid overlap, assign work by module boundary:

### Team A: Domain/Services
- Owns domain entities, service APIs, and business rule tests.
- Defines required ports for external dependencies.
- Does **not** edit KamiToolKit view modules.

### Team B: View-model/State
- Owns command handlers, derived-state calculators, validators, and state store contracts.
- Integrates with domain service interfaces.
- Does **not** implement concrete filesystem/IPC/config adapters.

### Team C: KamiToolKit View
- Owns component layout, event wiring, and visual state presentation.
- Consumes view-model public APIs only.
- Does **not** encode business rules or persistence behavior.

### Team D: Infrastructure/Composition
- Owns concrete adapters for configuration, filesystem, IPC, and dependency wiring.
- Implements ports defined by Domain/Services.
- Exposes initialized services/view-model factories to app startup.

### Cross-team contract process
- Public interfaces are versioned and reviewed by owning team.
- Breaking interface changes require a migration note and staged rollout plan.
- Ownership map is authoritative for task creation to prevent file-level contention.
