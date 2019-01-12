# Pipeline Settings

## Internal Resolution
### Fixed Render Resolution
This controls how resolution scaling is applied to the renderer.
Values are:
- None: no resolution scaling is applied
- Vertical: uses the specified fixed resolution on the Y axis, and scales the X according to aspect ratio.
- Horizontal: the same as Vertical, but instead the X axis is fixed.
- Both: both X and Y have fixed resolutions. If the fixed resolution does not match the aspect ratio of the game, pixels will no longer be square. This mode is only recommended if you are controlling aspect ratio manually.

### Render Resolution
Controls the actual resolution according to the settings above.

## Shader Features
### Perspective Correction
Controls whether perspective correction is applied to UVs.

### Simulate Vertex Precision
Controls whether to simulate low precision vertex positions.

### Vertex Position
Controls the precision of the vertex precision simulation.