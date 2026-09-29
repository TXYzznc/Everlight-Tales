## ADDED Requirements

### Requirement: Static UI resources are contract-bound
The UI build pipeline SHALL bind declared Sprite assets, Image types, and Form fields from page contracts when rebuilding a UIForm Prefab.

#### Scenario: Rebuild a page with formal sprites
- **WHEN** an approved contract declares a valid `spritePath`
- **THEN** the generated Image references that Sprite with the declared Image type and no missing reference

### Requirement: Board operation controls are prefab-owned
The BoardPage Prefab SHALL own its rotate, arm, tap, settle, pause, pause-menu, and result-text nodes; runtime code SHALL bind listeners to these nodes instead of creating formal controls.

#### Scenario: Open BoardPage with complete bindings
- **WHEN** BoardPageForm opens with generated bindings
- **THEN** operation listeners are attached and no formal operation GameObject is instantiated

#### Scenario: Open BoardPage with incomplete bindings
- **WHEN** one or more required operation references are missing
- **THEN** the Form logs a stable error and does not silently create replacement formal controls

### Requirement: UI integration is verified before delivery
Each completed UI batch SHALL pass compile, missing-script/reference inspection, PlayMode smoke, and visual comparison at the project’s 1080×1920 content baseline.

#### Scenario: Batch verification succeeds
- **WHEN** a batch is marked complete
- **THEN** Unity compilation and PlayMode smoke finish without errors and the resource/layout evidence is recorded
