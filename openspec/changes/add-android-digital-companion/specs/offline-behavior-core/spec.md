## ADDED Requirements

### Requirement: Offline deterministic action selection
The behavior core SHALL operate without network access and independently of Android UI. Given identical pack configuration, persisted state, ordered events, and random seed, it MUST choose identical actions.

#### Scenario: Reproducing a decision
- **WHEN** a creator replays an event sequence with the same seed and state
- **THEN** the core returns the same selected action and score breakdown.

### Requirement: Interaction priority and configurable behavior
The core MUST process direct user-interaction events ahead of autonomous idle events and SHALL select only actions permitted by data-driven configuration, cooldowns, and interruption rules.

#### Scenario: Interaction interrupts idle behavior
- **WHEN** an interruptible idle action is active and a tap event arrives
- **THEN** the core selects an eligible interaction response before the next autonomous action.
