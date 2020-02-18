using UnityEngine;
using RootEvents;
using RootLogging;

// Main runtime monobehavior for RootGen.
public class RootGen : MonoBehaviour {
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
    private MapGenerator _mapGenerator;

// CONSTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DESTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DELEGATES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// EVENTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private
    
// ~ Non-Static

// ~~ public

// ~~ private
    private static RootEvent<IRootGenConfigData, HexGrid> raiseGenerateMapEvent;
    private static RootEvent<Vector2, bool, HexGrid> raiseGenerateEmptyMapEvent;

// ENUMS

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INTERFACES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// PROPERTIES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INDEXERS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// METHODS ~~~~~~~~~

// ~ Static

// ~~ public

    public static HexGrid GenerateMap(object source, RootGenConfig config) {
        return raiseGenerateMapEvent.Publish(source, config.GetData()).Response;
    }

    public static HexGrid GenerateMap(object source, IRootGenConfigData configData) {
        return raiseGenerateMapEvent.Publish(source, configData).Response;
    }

    public static HexGrid GenerateEmptyMap(object source, Vector2 size, bool wrapping) {
        return raiseGenerateEmptyMapEvent.Publish(source, size, wrapping).Response;
    }

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
    private void Clear<T>() where T : UnityEngine.MonoBehaviour {
        foreach(T t in GameObject.FindObjectsOfType<T>()) {
            Destroy(t.transform.gameObject);
        }
    }

    private void HandleGenerateMap(
        object source, 
        CustomEventArgs<IRootGenConfigData, HexGrid> args
    ) {
        Clear<HexGrid>();
        Clear<HexGridCamera>();

        HexGrid response = _mapGenerator.GenerateMap(args.Argument);
        HexGridCamera camera = HexGridCamera.GetCamera(response);

        args.Response = response;

        RootLog.Log(
            "Map generated.",
            Severity.Information,
            "RootGen"
        );
    }

    private void HandleGenerateEmptyMap(
        object source,
        CustomEventArgs<Vector2, bool, HexGrid> args
    ) {
        Clear<HexGrid>();
        Clear<HexGridCamera>();

        HexGrid response = HexGrid.GetGrid(
            (int)args.Argument1.x, 
            (int)args.Argument1.y, 
            args.Argument2
        );

        HexGridCamera camera = HexGridCamera.GetCamera(response);

        args.Response = response;
        
        RootLog.Log(
            "Empty map generated.",
            Severity.Information,
            "RootGen"
        );
    }

    private void PublishGenerateMap(Vector2 size, bool wrapping) {
        
    }

    private void Awake() {
        _mapGenerator = MapGenerator.GetMapGenerator();
        raiseGenerateMapEvent = new RootEvent<IRootGenConfigData, HexGrid>();
        raiseGenerateEmptyMapEvent = new RootEvent<Vector2, bool, HexGrid>();
        raiseGenerateMapEvent.Subscribe(HandleGenerateMap);
        raiseGenerateEmptyMapEvent.Subscribe(HandleGenerateEmptyMap);
    }
    
    private void OnEnable() {
    
    }
    
    private void Reset() {
    
    }
    
    private void Start() {
        
    }
    
    private void FixedUpdate() {
    
    }
    
    private void Update() {
    
    }
    
    private void LateUpdate() {
    
    }
    
    private void OnDisable() {
    
    }
    
    private void OnDestroy() {
    
    }

// STRUCTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// CLASSES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
}