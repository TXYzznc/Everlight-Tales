## ADDED Requirements

### Requirement: User-controlled overlay lifecycle
The application SHALL create the companion overlay only after the user has granted overlay permission and explicitly started the companion. The application MUST display a foreground-service notification while the companion is running and MUST provide an in-app stop action.

#### Scenario: Permission is absent
- **WHEN** the user starts the companion without overlay permission
- **THEN** the application opens the system overlay-permission flow and does not create an overlay window.

#### Scenario: Companion starts
- **WHEN** permission is granted and the user starts the companion
- **THEN** the application creates a character-sized touchable overlay and starts its foreground service with a visible notification.

### Requirement: Non-intrusive interaction and placement
The character overlay SHALL receive tap and drag input only within its bounds. The application MUST persist its normalized location and restore it within the current usable screen bounds.

#### Scenario: Dragging the companion
- **WHEN** the user drags the character and releases it
- **THEN** the overlay moves with the gesture and its normalized position is saved.

#### Scenario: Device geometry changes
- **WHEN** the display size or insets change before the companion is restored
- **THEN** the application converts the saved normalized coordinates and clamps the overlay to usable bounds.
