# Silence Please

A configurable sound muting mod for Valheim.

If you're enjoying the mod, perhaps consider buying me a coffee.

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/vismeneer)

## Overview

**Silence Please** is a Valheim mod that allows you to selectively mute specific in-game sound effects. It provides both granular control (mute any sound by name) and convenient quick-mute options for common annoyances like chickens, shield generators, and wolf howls.

## Features

- **Realtime mute/unmute when editing with configuration manager**
- **Mute Any Sound:** Specify a comma-separated list of sound effect names to mute.
- **Quick Mute Options:**
  - **Chickens:** Instantly mute chicken sounds (`sfx_love`).
  - **Shield Generators:** Instantly mute shield generator hum (`sfx_shieldgenerator_powered_loop`).
  - **Wolves:** Mute wolf howls, with options to mute only when cubs are nearby or always.
- **Wolf Howl Silencing Modes:**
  - **Off:** No wolf howls are muted.
  - **CubsInRange:** Only mute howls if a wolf cub is within a configurable range.
  - **On:** Always mute wolf howls.
- **Log Sound Events:** Optionally log all sound effect names as they play, to help identify which sounds to mute.
- **Enable/Disable Mod:** Toggle the mod on or off without uninstalling.

## Configuration

All settings for Silence Please are available in the BepInEx configuration file, located at `BepInEx/config/org.bepinex.plugins.silenceplease.cfg`. Here’s what you can configure:

- **Enable Mod**  
  This option allows you to enable or disable the mod entirely. Set to `true` to activate the mod, or `false` to turn it off without uninstalling.

- **Mute Sounds**  
  Enter a comma-separated list of sound effect names you want to mute. For example: `sfx_love,sfx_shieldgenerator_powered_loop`. Use the logging feature (see below) to discover sound names.

- **Silence Chickens**  
  If enabled, this will mute the chicken sound effect (`sfx_love`). Set to `true` to silence chickens.

- **Silence Shield Generators**  
  If enabled, this will mute the shield generator sound effect (`sfx_shieldgenerator_powered_loop`). Set to `true` to silence shield generators.

- **Silence Wolves**  
  Controls how wolf howls are muted.  
  - `Off`: No wolf howls are muted.  
  - `CubsInRange`: Only mutes howls if a wolf cub is within a certain range.  
  - `On`: Always mutes wolf howls.

- **Wolf Silence Range**  
  When using the `CubsInRange` mode for wolf howls, this setting determines the range (in meters) to check for nearby wolf cubs. Default is `30`.

- **Log Sounds**  
  When enabled, the name of every sound effect played will be logged to the console. This is useful for identifying which sound names to add to the mute list.

To change these settings, edit the config file with your preferred values and save. The mod will automatically apply your changes the next time you launch the game.

## Compatibility

- **Incompatible with:** `valheim_plus` (org.bepinex.plugins.valheim_plus)
- **Requires:** BepInEx

## Identifying Sound Names

Enable the `Log Sounds` option in the config to print the name of every sound effect played. Use these names in the `Mute Sounds` list to mute specific effects.
