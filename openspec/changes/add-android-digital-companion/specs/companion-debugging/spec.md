## ADDED Requirements

### Requirement: Local creator diagnostics
The application SHALL provide a creator-only debug surface showing active action, mood values, recent events, action score breakdown, and random seed. It MUST allow local simulation of supported events.

#### Scenario: Simulating a head pat
- **WHEN** a creator triggers a head-pat simulation
- **THEN** the same event path used by the overlay is processed and the resulting action is visible in diagnostics.

### Requirement: Privacy-preserving diagnostics
Diagnostics MUST remain local and MUST NOT collect notification contents, other-app contents, screen imagery, or network telemetry.

#### Scenario: Inspecting diagnostics
- **WHEN** a creator opens the debug surface
- **THEN** only companion state and locally generated behavior traces are displayed.
