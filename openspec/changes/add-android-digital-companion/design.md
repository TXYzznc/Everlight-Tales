## Context

This private-gift product is intentionally separate from the Unity framework baseline. It targets normal Android phones, with Xiaomi/Redmi used for validation but without manufacturer-specific runtime dependencies. The experience requires a user-authorized system overlay that remains local-only, responds immediately to touch, and can later render a Spine character instead of a placeholder.

## Goals / Non-Goals

**Goals:**

- Build a native Kotlin Android vertical slice: permission flow, small touchable overlay, drag/tap, persistent normalized position, and a user-visible foreground-service notification.
- Keep the decision engine pure Kotlin, deterministic under a stored random seed, data-driven, and unit-testable without Android.
- Define a stable boundary between renderer and character pack so art and runtime work can progress independently.
- Provide creator-only inspection and event simulation before adding final art, dialogue, or voice.

**Non-Goals:**

- Unity runtime integration, LLMs, internet access, accounts, cloud sync, accessibility, notification/content reading, screen capture, or automatic background startup.
- Guaranteed survival against every OEM battery-management policy, model-specific allowlists, a finished Spine rig, final voice content, or a Play Store release.

## Decisions

### Native Android runtime, separate product module

Use Kotlin and Android SDK for the overlay and lifecycle. Android's overlay and service restrictions are platform concerns; a Unity player would add lifecycle, latency, and package-size cost without simplifying them. Unity remains suitable for reference behavior experiments only, not the mobile runtime.

### Explicit user-controlled overlay lifecycle

Request the overlay setting before creating a small `TYPE_APPLICATION_OVERLAY` window. The user starts and stops the companion from the app; its foreground service presents the required notification. This avoids hidden background launches and makes the permission clear. A single touchable character window and a non-touchable bubble/effect window prevent transparent full-screen touch interception.

### Pure local behavior core

Model input as prioritized events. A reducer updates mood and memory; an action selector scores eligible actions from static configuration, current context, cooldowns, and seeded random jitter. UI observes actions but does not decide them. This is more testable and controllable than an Android-bound FSM or a generative model.

### Generated frame packs first, pack adapter rather than renderer coupling

The runtime consumes semantic action IDs such as `idle`, `tap_reaction`, and `dragged`. The first renderer maps them to generated transparent PNG/WebP frame sequences or sprite sheets. Krita can later redraw, revise, and extend those frames without changing the runtime contract. A future Spine adapter remains optional and maps the same IDs to skeletal animations. Art packs declare supported action IDs, frame timing, regions, dialogue/audio references, and version metadata.

### Standards-first Xiaomi/Redmi support

Persist only normalized coordinates inside current window bounds, respect insets, and expose a non-blocking help screen when an OEM suspends the service. Do not request accessibility or use undocumented OEM APIs. Test the defined vertical slice on at least one Xiaomi/Redmi device and one non-Xiaomi Android device before expanding interactions.

## Risks / Trade-offs

- [OEM battery management can stop a foreground overlay] → Provide clear recovery state and optional device-settings guidance; never promise permanent survival.
- [Overlay permission is declined] → Keep a functional settings/preview screen and explain the feature cannot run until explicitly enabled.
- [Generated frames are visually inconsistent] → Lock a selected character sheet and palette before generating animation frames; keep the first action set small and review each sequence against that sheet.
- [Behavior grows into opaque rules] → Store inputs, selected score breakdown, and seed in debug traces; cap early action inventory.
- [Overlay interrupts normal phone use] → Window bounds follow only the character; dialogue/effects remain non-touchable; user can stop the service instantly.

## Migration Plan

1. Create the product in a separate Android project or isolated product change; do not add product assets to `Assets/Game/ScriptsBuiltin/`.
2. Validate the placeholder vertical slice on target devices.
3. Introduce the behavior core and debug tooling behind the same placeholder renderer.
4. Add a Spine adapter only if later art needs skeletal deformation and the art pack satisfies the handoff contract.
5. If the overlay becomes unstable or unwelcome, stop the service and remove its windows; local state remains intact and no remote migration is needed.

## Open Questions

- Minimum supported Android API level and the first physical-device OS versions.
- Whether the delivery is private sideloading only or eventually Play-distributed.
- The selected visual direction after reference photos are supplied.
