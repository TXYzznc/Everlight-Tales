## ADDED Requirements

### Requirement: Semantic renderer contract
The runtime SHALL request semantic action identifiers rather than renderer-specific animation names. A character pack MUST declare the actions it supports and map them to its renderer assets.

#### Scenario: Placeholder pack is active
- **WHEN** the behavior core requests `tap_reaction`
- **THEN** the placeholder renderer displays its configured feedback without requiring Spine assets.

### Requirement: Generated frame-animation handoff
The first art handoff MUST support generated transparent PNG or WebP frame sequences or sprite sheets. Each animation MUST declare its semantic action ID, ordered frames, duration or frame rate, anchor point, and loop behavior. The handoff SHALL preserve a character sheet, palette, and layer/source notes so that Krita can later refine the frames.

#### Scenario: Playing a generated animation
- **WHEN** the behavior core requests `tap_reaction`
- **THEN** the renderer plays the generated sequence mapped to that action using its declared timing and anchor.

### Requirement: Optional skeletal-animation upgrade
The art handoff SHOULD document source-layer names, pivot/attachment expectations, draw order, supported expressions, and hidden-area completion needed for a future Spine-compatible pack. The product MUST NOT require Spine files for the first vertical slice.

#### Scenario: Replacing a generated pack
- **WHEN** a later skeletal pack supplies the same semantic actions as the generated pack
- **THEN** the renderer adapter can replace the generated renderer without changing behavior-core logic.
