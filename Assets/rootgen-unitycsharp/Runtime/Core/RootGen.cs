using UnityEngine;
using RootEvents;
using RootLogging;

/// <summary>
/// Facade for the RootGen library encapsulating the
/// process of generating a map according to a RootGenConfig scriptable
/// object or an object implementing the IRootGenConfigData interface.
/// </summary>
public class RootGen {
// FIELDS ~~~~~~~~~~
    private MapGenerator _mapGenerator;

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
    /// <param name="source">The instance that requested the map.</param>
    /// <param name="config">A RootGenConfig scriptable object.</param>
    /// <returns>
    ///     A HexGrid generated according to the config settings.
    /// </returns>
    public HexGrid GenerateMap(object source, RootGenConfig config) {
        Clear<HexGrid>();
        Clear<HexGridCamera>();

        HexGrid result = _mapGenerator.GenerateMap(config.GetData());
        HexGridCamera camera = HexGridCamera.GetCamera(result);

        RootLog.Log(
            "Map request from " + source + " processed.",
            Severity.Information,
            "RootGen"
        );

        return result;
    }

    /// <summary>
    /// Generate a hex map using the provided configuration data.
    /// </summary>
    /// <param name="source">The instance that requested the map.</param>
    /// <param name="data">A class implementing IRootGenConfigData.</param>
    /// <returns>
    ///     A HexGrid generated according to the config settings.
    /// </returns>

    public HexGrid GenerateMap(
        object source,
        IRootGenConfigData configData
    ) {
        Clear<HexGrid>();
        Clear<HexGridCamera>();

        HexGrid result = _mapGenerator.GenerateMap(configData);
        HexGridCamera camera = HexGridCamera.GetCamera(result);

        RootLog.Log(
            "Map request from " + source + " processed.",
            Severity.Information,
            "RootGen"
        );

        return result;
    }

    /// <summary>
    /// Generate a blank hex map.
    /// </summary>
    /// <param name="source">The instance that requested the map.</param>
    /// <param name="size">
    ///     A Vector2 specifying the width and heigth of the map.
    /// </param>
    /// <param name="wrapping">
    ///     Should the map wrap when panned by the camera?
    /// </param>
    /// <returns>
    ///     A blank hex map generated according to the specified dimensions.
    /// </returns>
    public HexGrid GenerateEmptyMap(
        object source,
        Vector2 size,
        bool wrapping
    ) {
        Clear<HexGrid>();
        Clear<HexGridCamera>();

        HexGrid response = HexGrid.GetGrid(
            (int)size.x,
            (int)size.y,
            wrapping
        );

        HexGridCamera camera = HexGridCamera.GetCamera(response);

        RootLog.Log(
            "Map request from " + source + " processed.",
            Severity.Information,
            "RootGen"
        );
        
        return response;
    }

    private void Clear<T>() where T : UnityEngine.MonoBehaviour {
        foreach(T t in GameObject.FindObjectsOfType<T>()) {
            GameObject.Destroy(t.transform.gameObject);
        }
    }

// STRUCTS ~~~~~~~~~~

// CLASSES ~~~~~~~~~~

}