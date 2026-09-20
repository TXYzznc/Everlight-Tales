## Why

The intended private gift needs to feel like a responsive digital version of its creator while remaining offline and respectful of the recipient's device and privacy. A small Android overlay companion can deliver that experience only if the visual-production path and the native runtime are designed together from the first vertical slice.

## What Changes

- Define an offline Android companion product boundary: no account, network service, LLM, accessibility service, notification/content reading, or screen capture.
- Deliver a native Android vertical slice that a user explicitly starts: a permission-guided, movable, touchable overlay with persistent placement and a visible foreground-service notification.
- Add a data-driven, Android-independent behavior-core contract for events, personality, mood, short-term interaction memory, deterministic selection, and debug inspection.
- Define a character-art handoff contract for generated frame-animation packs now, with a future optional Spine-ready replacement that does not change behavioral logic.
- Add Xiaomi/Redmi-aware setup guidance as best-effort device guidance, while retaining standard Android behavior and avoiding device-model allowlists.

## Capabilities

### New Capabilities

- `android-overlay-companion`: User-authorized Android overlay lifecycle, touch interaction, foreground-service behavior, and placement persistence.
- `offline-behavior-core`: Deterministic, configurable local decision system that turns device and interaction events into character actions.
- `character-pack-handoff`: Spine-ready art-pack structure and renderer-facing contracts for replacing placeholder assets.
- `companion-debugging`: Creator-facing tools for observing and reproducing the companion's runtime decisions.

### Modified Capabilities

- None.

## Impact

This is a product-level change and must remain isolated from the framework baseline. It introduces a separate native Android build/runtime, local-only data contracts, Android permissions and foreground-service declarations, art-production assets, and device validation on Xiaomi/Redmi phones.
