# Transmog
TLDR for those familiar with WoW: This is a transmogrification mod for Single Player Tarkov.

This mod allows you to change the appearance of your PMC/Scav equipment (for equippable items, not tactical clothing).

## Runtime Requirements:

[Custom Interactions](https://hub.sp-tarkov.com/files/file/1278-custom-interactions/)

## How to build:
1. Clone this repository.
2. Duplicate *UserSettings.props.template* and rename the copy to *UserSettings.props*
3. Open *UserSettings.props* and change the value of *SPTTarkovDirectory* to your game directory.
4. Open the solution with Visual Studio.
5. After building, the output assembly will be copied to the plugin folder of your game, and a packaged zip file will be in *bin* directory.