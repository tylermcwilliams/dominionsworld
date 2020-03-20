# Dominions, World

Mod for Vintage Story used to create custom world

Features:

- Climate map is generated according to pixels on a file `VintagestoryData/Maps/climate.png`

### Climate

Climate map generation is hijacked in `GenMaps.cs` by assigning an array of RGB values as int, directly read from the climate.png file.

Changes from source:

##### found by searching dominionsmodification

- `GenMaps.cs` line 115
- `GenBlockLayers.cs` line 159

_For some reason, there is a bit overflow when lerping, on line 159 in `GenBlockLayers.cs`. As a temporary fix, I pass the arguments as absolutes, but this still causes very light inaccuracies._

### Landforms

Landforms map generation is hijacked in `GenMaps.cs` by assigning an array of RGB values as int, directly read from the landform.png file.

#### WARNING: 
If the mod reads an R value that does not correspond to a landform directly, it might crash the server.
Make sure the landforms.png file is somehow sanitized for out of bounds R values.
Safety may be added later.

Changes from source:

- `GenMaps.cs` line 150