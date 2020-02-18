using System;
using System.Collections.Generic;
using UnityEngine;
using RootEvents;
using RootLogging;

public class RootEdit : MonoBehaviour {
    // FIELDS ~~~~~~~~~~
    
    // ~ Static
    
    // ~~ public
    
    // ~~ private
    
    // ~ Non-Static
    
    // ~~ public
    
    // ~~ private
    private HexGrid _hexGrid;
    private HexGridEditorUI _hexGridEditorUI;

    private Queue<CustomEventArgs<GameObject, string>>
        _registerUnitRequestCache;
    
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
    private static event 
        EventHandler<CustomEventArgs<GameObject, string>> RaiseRegisterUnit;
    
    // ~ Non-Static
    
    // ~~ public
    
    // ~~ private
    
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
    public static string RegisterUnit(object source, GameObject toRegister) {
        CustomEventArgs<GameObject, string> args = 
            new CustomEventArgs<GameObject, string>(toRegister);
        
        if (RaiseRegisterUnit != null) {
            RaiseRegisterUnit(source, args);
        }

        return args.Response;
    }
    
    // ~~ private
    
    // ~ Non-Static
    
    // ~~ public
    
    // ~~ private
    private void Awake() {
        _registerUnitRequestCache = 
            new Queue<CustomEventArgs<GameObject, string>>();

        RaiseRegisterUnit += HandleRegisterUnit;
    }

    private void Start() {
        if(!(_hexGrid = ValidateSingleInstance<HexGrid>())) {
            _hexGrid = HexGrid.GetGrid();
        }

        _hexGridEditorUI = HexGridEditorUI.GetUI(_hexGrid);
    }

    private void Update() {
        if(
            _hexGrid && 
            _hexGridEditorUI && 
            _registerUnitRequestCache.Count > 0
        ) {
            ClearRegisterUnitRequestCache();
        }
    }
    
    private T[] GetExisting<T>() where T : UnityEngine.Object {
        T[] result = GameObject.FindObjectsOfType<T>();
        return result;
    }

    private T ValidateSingleInstance<T>() where T : UnityEngine.Object {
        T[] existing = GetExisting<T>();

        if (existing.Length > 1) {
            RootLog.Log(
                "Single instance validation of " + typeof(T) + " failed. " + 
                    "Multiple instances.", 
                Severity.Critical,
                "RootEdit"
            );
        }

        if(existing.Length > 0) {
            RootLog.Log(
                "Single instance validation of " + typeof(T) + " succeeded.",
                Severity.Information,
                "RootEdit"
            );
            return existing[0];
        }

        RootLog.Log(
            "Single instance validation of " + typeof(T) + " failed. " +
                "No instances.",
            Severity.Warning,
            "RootEdit"
        );

        return null;
    }

    private void HandleRegisterUnit(
        object source,
        CustomEventArgs<GameObject, string> args
    ) {
        if (_hexGridEditorUI) {
            _hexGridEditorUI.RegisterUnitType(args.Argument);
            args.Response = "Unit registered.";
        }
        else {
            _registerUnitRequestCache.Enqueue(args);
        }
    }

    private void ClearRegisterUnitRequestCache() {
        while (_registerUnitRequestCache.Count > 0) {
            CustomEventArgs<GameObject, string> dequeue = 
                _registerUnitRequestCache.Dequeue();
            HandleRegisterUnit(null, dequeue);
        }
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