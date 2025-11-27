# Changelog

## 1.0.0 (2025-08-19)

### Features
* **Throwable Enhancement**: Added configurable activation modes for grenades
  - Toggle mode: Enable/disable automatic grenade throwing with button press
  - Hold mode: Continuously auto-throw grenades while holding the throw button
  - None mode: Default MycoPunk behavior

* **Wingsuit Salvo Enhancement**: Added configurable activation modes for rocket salvo
  - Toggle mode: Enable/disable automatic salvo firing with button press
  - None mode: Default MycoPunk behavior

* **Visual Improvements**:
  - Optional suppression of 3D salvo launcher targeting model to reduce screen clutter
  - HUD notification suppression during automatic salvo firing
  - Debouncing logic to prevent spurious auto-fire triggers

### Configuration
* Added configuration options under "General" section:
  - `ThrowableActivationMode`: Controls grenade throwing behavior (None/Hold/Toggle)
  - `SalvoActivationMode`: Controls salvo firing behavior (None/Toggle)
  - `SuppressSalvoModelAlways`: Hides salvo targeting model when enabled

### Technical
* Implemented HarmonyLib patches for game modification
* Added proper logging for debugging and user feedback
* Client-side mod with no multiplayer conflicts
* BepInEx plugin framework integration

## 0.1.0 (2025-08-19)

### Tech
* Initial mod template setup with BepInEx framework
* Add MinVer for version management
* Add thunderstore.toml configuration for mod publishing
* Add LICENSE.md and CHANGELOG.md template files
* Basic plugin structure with HarmonyLib integration
* Placeholder for mod-specific functionality
