using UnityEngine;
using RootLogging;
using RootUtils.UnityLifecycle;

public class RootGen {
// FIELDS ~~~~~~~~~~
    private MapGenerator _mapGenerator;
    private RandomHashGrid _randomHashGrid;

// CONSTRUCTORS ~~~~~~~~~~
    public RootGen() {
        // Instantiate map generator.
        _mapGenerator = new MapGenerator();
    }

// DESTRUCTORS ~~~~~~~~~~

// DELEGATES ~~~~~~~~~~

// EVENTS ~~~~~~~~~~

// ENUMS ~~~~~~~~~~

// INTERFACES ~~~~~~~~~~

// PROPERTIES ~~~~~~~~~~

// INDEXERS ~~~~~~~~~~

// METHODS ~~~~~~~~~
/// <summary>
/// Generate a hex map using the provided configuration. 
/// </summary>
/// <param name="config">A RootGenConfig scriptable object.</param>
/// <returns>
///     A HexGrid generated according to the config settings.
/// </returns>
    public HexGrid GenerateMap(RootGenConfig config) {
        Destroy.DestroyAll<HexGrid>();
        HexGrid result = _mapGenerator.GenerateMap(config);
        HexGridCamera.AttachCamera(result, config.cellOuterRadius);
        return result;
    }

    public HexGrid GenerateHistoricalBoard(int width, int height) {
        Destroy.DestroyAll<HexGrid>();
        HexGrid result = _mapGenerator.GenerateHistoricalBoard(width, height, 16);
        HexGridCamera.AttachCamera(result, 10f);
        return result;
    }


/// <summary>
/// Generate a blank hex map.
/// </summary>
/// <param name="size">
///     A Vector2 specifying the width and heigth of the map.
/// </param>
/// <param name="wrapping">
///     Should the map wrap when panned by the camera?
/// </param>
/// <returns>
///     A blank hex map generated according to the specified dimensions.
/// </returns>
    public HexGrid GenerateEmptyMap(Vector2 size, bool wrapping) {
        Destroy.DestroyAll<HexGrid>();

        HexGrid result = HexGrid.GetGrid(
            (int)size.x,
            (int)size.y,
            wrapping
        );

        HexGridCamera.AttachCamera(result, 10f);
        return result;
    }

// STRUCTS ~~~~~~~~~~

// CLASSES ~~~~~~~~~~

}