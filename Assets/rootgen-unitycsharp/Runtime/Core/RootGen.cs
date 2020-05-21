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
    public HexMap GenerateMap(
        RootGenConfig config,
        bool editMode
    ) {
        Destroy.DestroyAll<HexMap>();

        HexMap result = _mapGenerator.GenerateMap(
            config,
            editMode
        );

        HexGridCamera.AttachCamera(result, config.hexSize);
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
    public HexMap GenerateEmptyMap(
        Vector2 size,
        int seed,
        float hexOuterRadius,
        bool wrapping,
        bool editMode
    ) {
        Destroy.DestroyAll<HexMap>();

        HexMap result = HexMap.Empty(
            new Rect(0, 0, (int)size.x, (int)size.y),
            seed,
            hexOuterRadius,
            wrapping,
            editMode
        );

        HexGridCamera.AttachCamera(result, hexOuterRadius);
        return result;
    }

// STRUCTS ~~~~~~~~~~

// CLASSES ~~~~~~~~~~

}