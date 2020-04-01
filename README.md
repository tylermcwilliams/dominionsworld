# Dominions, World

Mod for Vintage Story used to create custom world. Maps need to be 2000x2000 pixels, and world in VS has to be 600k by 600k

Features:

- Climate
- Landforms
- Salt water in oceans

Changes from source:

- `GenMaps.cs` lines 13, 81 instance of WorldMap
- `GenMaps.cs` line 109 Climate mapgen
- `GenMaps.cs` line 156 Landform mapgen
- `GenBlockLayers.cs` - line 159 Abs of climate distortion, prevents bit overflow, - line 295 saltwater - line 245 saltwater
- `GenCaves.cs` line 547 saltwater
- `GenTerra.cs` line 310 saltwater

## Commands

- `/getpixelinfo` returns landform, temp and humidity of a given chunk

## Climate Map

Climate map generation is hijacked in `GenMaps.cs` by assigning an array of RGB values as int, directly read from the `climate.png` file.

Changes from source:

##### found by searching dominionsmod

- `GenMaps.cs` line 109
- `GenBlockLayers.cs` line 159

_For some reason, there is a bit overflow when lerping, on line 159 in `GenBlockLayers.cs`. As a temporary fix, I pass the arguments as absolutes, but this still causes very light inaccuracies._

## Landforms Map

Landforms map generation is hijacked in `GenMaps.cs` by assigning an array of RGB values as int, directly read from the `landform.png` file.

#### WARNING:

If the mod reads an R value that does not correspond to a landform directly, it might crash the server.
Make sure the landforms.png file is somehow sanitized for out of bounds R values.
Safety may be added later.

Changes from source:

- `GenMaps.cs` line 156

## Salt Water

Dominions world generates a new Salt Water block as sea water (below seaLevel).
Various parts of the worldgen that referred to water blocks (as defined in GlobalConfig) were changed to refer to saltWater block,
usually assigned at initWorldgen

Changes from source

- `GenBlockLayers.cs` lines 245, 295
- `GenCaves.cs` line 547
- `GenTerra.cs` line 310

Added

- `OldGenPartial.cs` Needed for GenCaves, Tyron keeps this part closed for the new versions
