## 1. Contract and resource pipeline

- [x] 1.1 Add formal Sprite mappings to Preparation, SaveSlot, Settings, and Settlement contracts
- [x] 1.2 Add project UI resolution/resource-difference record
- [x] 1.3 Add editor binding utility for MainPageShell and DialogView

## 2. BoardPage migration

- [x] 2.1 Add BoardPageForm contract with static operation controls
- [x] 2.2 Move rotate, arm, tap, settle, pause, and result controls into the Prefab
- [x] 2.3 Move pause menu buttons and backdrop into the Prefab
- [x] 2.4 Update runtime scripts to bind existing controls and avoid formal control creation

## 3. Validation

- [x] 3.1 Rebuild all contract Prefabs through the Editor generator
- [x] 3.2 Run missing-script inspection and console error checks
- [x] 3.3 Run PlayMode smoke after BoardPage migration
- [x] 3.4 Capture 1080×1920 PlayMode screenshot/observation evidence for BoardPage and MainPageShell controls

## 4. Follow-up pages

- [x] 4.1 Move MainPageShell map shell, map viewport, place panel, event list root, tracking label, and wait button into the MainPageShell Prefab
- [x] 4.2 Update MapPanel to bind the static map shell while retaining dynamic place nodes and event cards
- [x] 4.3 Create contracts for CityMap and location/event confirmation
- [x] 4.4 Create contracts for task, home, workbench, codex, archive, guest, service, dialogue, investigation, prologue, recovery, and feedback pages
- [x] 4.5 Register each Form in GF UI configuration and add page-specific lifecycle tests
