# EnhancedCooldowns

A BepInEx mod for MycoPunk that enhances cooldown mechanics for throwables and wingsuit salvo with configurable activation modes.

## Description

This mod modifies the game's cooldown system for grenades and the wingsuit's rocket salvo, allowing players to customize how these abilities activate. It uses Harmony patches to intercept and alter the default behavior without affecting core game mechanics.

The mod supports individual configuration for each grenade type and provides options for both manual and automatic activation modes.

## Features

- **Grenade Activation Modes**: Separate settings for incendiary, voltaic, and acid grenades
  - **None**: Default MycoPunk behavior (automatic activation when charged)
  - **Hold**: Requires holding the throw button to activate when charged
  - **Toggle**: Toggle on/off with a button press; automatically throws when charged and enabled

- **Wingsuit Salvo Activation Modes**:
  - **None**: Default behavior (manual firing)
  - **Toggle**: Toggle auto-fire on/off; automatically fires salvo when recharged and enabled

- **Visual Options**:
  - Suppress the 3D salvo launcher targeting model to reduce screen clutter

- **Configuration**: Settings are stored in a config file that can be edited while the game is running; changes reload automatically

## Configuration

The mod's settings are located in `BepInEx/config/sparroh.enhancedcooldowns.cfg`. Each option is under the "General" section:

- `IncendiaryGrenadeActivationMode` (default: Toggle)
  - Controls activation mode for incendiary grenades

- `VoltaicGrenadeActivationMode` (default: Toggle)
  - Controls activation mode for voltaic grenades

- `AcidGrenadeActivationMode` (default: Toggle)
  - Controls activation mode for acid grenades

- `SalvoActivationMode` (default: Toggle)
  - Controls activation mode for wingsuit salvo

- `SuppressSalvoModelAlways` (default: false)
  - When true, always hides the 3D salvo launcher model

Activation mode options: None, Hold, Toggle

## Installation

**Recommended: Via Thunderstore Mod Manager**
1. Install Thunderstore Mod Manager
2. Search for "EnhancedCooldowns" by Sparroh
3. Download and install the mod
4. Launch MycoPunk; the mod loads automatically

**Manual Installation**
1. Download the mod package from Thunderstore or GitHub releases
2. Extract the contents to your MycoPunk game directory
3. Place `EnhancedCooldowns.dll` in `<MycoPunk Directory>/BepInEx/plugins/`
4. Ensure BepInEx is installed and configured

## Dependencies

- MycoPunk (base game)
- [BepInEx](https://github.com/BepInEx/BepInEx) - Version 5.4.2403 or compatible
- .NET Framework 4.8
- [HarmonyLib](https://github.com/pardeike/Harmony) (included via NuGet)

## Usage

After installation, the mod loads automatically when MycoPunk starts. Check the BepInEx console for confirmation messages.

- For grenades in Toggle mode: Press the throw button to toggle activation on/off
- For grenades in Hold mode: Hold the throw button to activate when charged
- For salvo in Toggle mode: Press the salvo button to toggle auto-fire on/off

Configuration changes take effect immediately without restarting the game.

## Troubleshooting

- **Mod not loading**: Verify BepInEx is properly installed and the DLL is in the correct plugins folder
- **Settings not applying**: Check the config file syntax and ensure the game has write permissions
- **Conflicts**: This is a client-side mod and should not affect multiplayer

## Changelog

See CHANGELOG.md for detailed version history.

## Authors

- Sparroh
- funlennysub (BepInEx template)
- [@DomPizzie](https://twitter.com/dompizzie) (README template)

## License

This project is licensed under the MIT License - see the LICENSE file for details
