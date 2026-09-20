## 1. Product isolation and Android foundation

- [ ] 1.1 Create an isolated native Kotlin Android product workspace without modifying the framework baseline.
- [ ] 1.2 Set the minimum/target Android API levels and validate the build on a Xiaomi/Redmi and a non-Xiaomi physical device.
- [ ] 1.3 Implement the settings entry point, explicit start/stop control, overlay-permission guidance, and foreground-service notification.
- [ ] 1.4 Implement a character-sized overlay window and a separate non-touchable bubble/effect window.
- [ ] 1.5 Implement drag/tap input, normalized placement persistence, inset-aware restoration, and a safe disabled state.

## 2. Offline behavior core

- [ ] 2.1 Define pure Kotlin models for events, personality, mood, short-term memory, action definitions, cooldowns, and interruption rules.
- [ ] 2.2 Implement event prioritization, state reduction, deterministic seeded scoring, and semantic action selection.
- [ ] 2.3 Add unit tests proving a direct interaction preempts an interruptible idle action.
- [ ] 2.4 Add unit tests proving equal seed, inputs, and state reproduce an identical decision and score breakdown.
- [ ] 2.5 Add local persistence for normalized placement, seed, and minimal behavioral state without any network permission.

## 3. Renderer and art handoff

- [ ] 3.1 Implement a frame-animation renderer mapping semantic actions to generated transparent PNG/WebP sequences or sprite sheets.
- [ ] 3.2 Define the versioned character-pack manifest and action-mapping schema.
- [ ] 3.3 Produce a selected Q-version character sheet from supplied reference photos, including front and 3/4 reference, palette, and frame anchors.
- [ ] 3.4 Generate and review the initial frame sequences for idle, tap reaction, drag reaction, sleep, wake, and walk.
- [ ] 3.5 Import the generated sequences, validate timing and transparent bounds on a physical device, and prepare Krita source notes for later refinements.
- [ ] 3.6 Implement and test a future Spine renderer adapter only if skeletal animation becomes necessary.

## 4. Creator diagnostics and vertical-slice validation

- [ ] 4.1 Implement a creator-only debug surface for state, event trace, score breakdown, and seed.
- [ ] 4.2 Implement local event simulation and semantic-action preview controls.
- [ ] 4.3 Verify that the placeholder vertical slice starts only by user action, stays within bounds, persists position, and responds to tap/drag.
- [ ] 4.4 Verify that diagnostics contain no notification, other-app, screen, or network data.
- [ ] 4.5 Test the vertical slice after common Xiaomi/Redmi background-management conditions and publish best-effort recovery guidance.
