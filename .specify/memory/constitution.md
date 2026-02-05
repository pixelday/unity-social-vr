# MWorldsTech Constitution

## Core Principles

### I. Scripts Over Inspector Magic
Behavior lives in C#.

Inspector values configure, they do not encode logic.

No hidden behavior via serialized booleans, event chains, or UnityEvents.

If behavior matters, it should be readable in code.

Rule of thumb: if you cannot grep for it, it is suspect.

### II. Data Is Separate From Behavior
Data is dumb. Code is smart.

ScriptableObjects are data containers, not mini-services.

No logic in ScriptableObjects beyond validation (OnValidate and basic guards).

Runtime state does not live in assets.

This keeps state flow explicit and testable.

### III. Explicit Wiring Beats Implicit Discovery
Avoid magic hookups.

No `FindObjectOfType` (or similar) in production code.

No reliance on scene hierarchy order.

Dependencies are passed explicitly (constructors where possible) or assigned once at startup by a composition root.

If something breaks because a GameObject moved, the design is wrong.

### IV. Composition Over Inheritance
Prefer small components with clear responsibility.

Shallow inheritance trees only.

Interfaces and composition over base classes.

One component, one job.

If a class name needs “ManagerManager,” stop and refactor.

### V. Scenes Are Configuration, Not Logic
Scenes assemble systems, they do not define them.

No gameplay logic in scene callbacks beyond delegating to explicit system methods.

Scenes describe what exists, not how it behaves.

Scene load order should not encode rules.

A scene should be replaceable without rewriting systems.

### VI. Lifecycle Is Explicit
Unity lifecycle methods are an implementation detail.

Do not spread logic across `Awake`, `Start`, `OnEnable`, `Update` casually.

Prefer explicit `Initialize`, `Tick`, `Shutdown` methods.

Centralize update loops where possible (a small number of tick drivers instead of many `Update`s).

If timing matters, make it obvious.

### VII. Single Source of Truth
State lives in one place.

No mirrored state across components.

UI reflects state, it does not own it.

Derive, do not duplicate.

Bugs thrive on redundant state.

### VIII. Names Should Explain the System
Optimize for reading, not typing.

No clever abbreviations.

Component names should answer “what does this do?”

Folder structure mirrors architectural boundaries (domain and feature boundaries first, Unity glue last).

If a new dev cannot guess where something lives, the structure failed.

### IX. Failure Should Be Loud
Silent failure is worse than a crash.

Assert invariants early.

Throw when assumptions are violated (or fail the build/test).

Log with intent, not spam.

Fail fast, fail where the mistake is made.

### X. Modularity Over Convenience
Short-term convenience is technical debt.

No god objects.

No cross-cutting singletons without justification and documented scope.

Systems communicate through narrow contracts.

If everything can see everything, nothing is understandable.

### XI. Optimize for Change, Not Perfection
The code will change. Design for that.

Make deletion easy.

Prefer clarity over cleverness.

Refactor early and often.

The best Unity projects are boring to read.

### XII. Readonly Folder
The following folders are readonly. Do not edit them.
`Assets/Asset Packs`
`Assest/TextMesh Pro`
`Packages`

## Architecture Constraints

- Prefer a layered structure:
  - **Core**: plain C# domain logic (no `MonoBehaviour`, no scene knowledge)
  - **Game**: gameplay orchestration and state (still minimal Unity)
  - **UnityAdapters**: input, physics, audio, rendering, scene and prefab glue
- New gameplay logic should start in Core, then be wired into Unity via adapters.
- Avoid runtime reflection and stringly-typed lookups unless unavoidable and documented.
- Do not introduce global service locators. If you must, it needs a written rationale and a plan to contain it.

## Development Workflow

- Every change ships with:
  - A clear intent in the PR description
  - The smallest reasonable test coverage (unit tests in Core, Play Mode tests only when necessary)
  - No unrelated refactors bundled with feature work
- Definition of done for a feature:
  - State flow is explicit
  - Dependencies are wired explicitly
  - No inspector-driven behavior
  - No hidden coupling to scene order or hierarchy
- If you add a system:
  - It must have a clear owner type (state owner)
  - It must define its public contract (interfaces, events, commands)
  - It must be easy to delete

## Governance

- This constitution supersedes local preferences and convenience.
- Any exception requires:
  1. A short written rationale in the PR
  2. The smallest possible scope
  3. A follow-up task to remove or contain the exception, if applicable
- Reviews must explicitly check:
  - No inspector-encoded behavior
  - No implicit discovery (`Find*`, scene-order assumptions)
  - State ownership is singular and obvious
  - Unity remains a host layer, not the architecture

**Version**: 1.0.0 | **Ratified**: 2026-01-27 | **Last Amended**: 2026-01-27
