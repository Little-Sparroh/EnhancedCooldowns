# Changelog

## 1.1.0 (2025-12-24)

### Features
* **Individual Grenade Activation Modes**: Separated grenade activation modes into independent configurations for each grenade type
  - Incendiary Grenade: Toggle, Hold, or None modes
  - Voltaic Grenade: Toggle, Hold, or None modes
  - Acid Grenade: Toggle, Hold, or None modes
* **Hold Mode Fix**: Corrected hold mode behavior for proper continuous auto-throw while holding the throw button

### Configuration
* Updated grenade configurations under "General" section:
  - `IncendiaryGrenadeActivationMode`: Controls incendiary grenade behavior (None/Hold/Toggle)
  - `VoltaicGrenadeActivationMode`: Controls voltaic grenade behavior (None/Hold/Toggle)
  - `AcidGrenadeActivationMode`: Controls acid grenade behavior (None/Hold/Toggle)

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
