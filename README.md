# Auto Kill Plugin
[![Push to nuget feed on release](https://github.com/TaleSpire-Modding/AutoKillPlugin/actions/workflows/release.yml/badge.svg)](https://github.com/TaleSpire-Modding/AutoKillPlugin/actions/workflows/release.yml)

A Simple plugin requested by Poap, that allows you to instantly kill multiple creatures in TaleSpire with a keybind.

## Install

Currently you need to either follow the build guide down below or use the R2ModMan. 

## Usage
This is a simple plugin that provides a keybind (right ctrl + del) to instantly kill selected creatures in TaleSpire.
It is currently setup so that in Turn Based mode, it will knock them prone and remove them on their turn.
Outside of Turn Based mode, it will just remove them instantly.
This plugin also supports multiselection, so if you have multiple creatures selected, it will kill them all.

## How to Compile / Modify

Open ```AutoKillPlugin.sln``` in Visual Studio.

Build the project (We now use Nuget).

Browse to the newly created ```bin/Debug``` or ```bin/Release``` folders and copy the ```.dll``` to ```Steam\steamapps\common\TaleSpire\BepInEx\plugins```

## Changelog
- 1.0.3: Bump SetInjectionFlag package version, migrate to DependencyUnityPlugin and include Cleanup Logic
- 1.0.2: Updated to handle mode switching during combat more gracefully
- 1.0.1: Set Hard dependency on Set Injection Flags to ensure compatibility
- 1.0.0: Initial release

## Shoutouts
<!-- CONTRIBUTORS-START -->
Shoutout to my past [Patreons](https://www.patreon.com/HolloFox) and [Discord](https://discord.gg/bxgZvBRVGf) members, recognising your mighty support and contribution to my caffeine addiction:
- [Demongund](https://www.twitch.tv/demongund) - Introduced me to TaleSpire
- [Tales Tavern/MadWizard](https://talestavern.com/)
- Joaqim Planstedt
<!-- CONTRIBUTORS-END -->
