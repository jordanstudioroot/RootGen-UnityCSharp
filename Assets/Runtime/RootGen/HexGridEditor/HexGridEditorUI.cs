using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public enum OptionalToggle {
    Ignore,
    Yes,
    No
}

public enum PaintTypes {
    Sand,
    Grass,
    Mud,
    Stone,
    Snow,
    Water,
    Elevation,
    Urban,
    Farm,
    Plant,
    Road,
    River,
    Unit
}

public enum Modes {
    Editor,
    Runtime
}

public enum EditStates {
    Select,
    DragApply,
    DragDelete,
    Apply,
    Delete,
    WaitingForInput
}

public class HexGridEditorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
    private Modes _mode;
    private int _brushSize;
    private OptionalToggle _riverMode;
    private OptionalToggle _roadMode;
    private bool _isDrag;
    private HexDirection _dragDirection;
    private HexCell _previousDragCell;
    private List<GameObject> _unitObjects;
    private GameObject _unitToInstantiate;
    private PaintTypes _paintType;
    private DropdownAdapter _brushDropdownAdapter;
    private DropdownAdapter _modeDropdownAdapter;
    private HexUnit _selectedUnit;
    private bool _pointerOverUI;

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
    public HexGrid ActiveGrid {
        set; private get;
    }
    public TMP_Dropdown BrushDropdown {
        set {
            _brushDropdownAdapter =
                new DropdownAdapter(value);
            RefreshPaintOptions(_brushDropdownAdapter);
        }
    }

    public TMP_Dropdown ModeDropdown {
        set {
            _modeDropdownAdapter =
                new DropdownAdapter(value);
            RefreshModeOptions(_modeDropdownAdapter, ActiveGrid);
        }
    }

// ~~ private
    private EditStates EditState {
        get {
            if (Input.GetMouseButtonDown(0) || 
                Input.GetMouseButtonDown(1)
            ) {
                return EditStates.Select;
            }
            else if (Input.GetMouseButton(0)) {
                return EditStates.DragApply;
            }
            else if (Input.GetMouseButton(1)) {
                return EditStates.DragDelete;
            }
            else if (Input.GetMouseButtonUp(0)) {
                return EditStates.Apply;
            }
            else if (Input.GetMouseButtonUp(1)) {
                return EditStates.Delete;
            }
            else {
                return EditStates.WaitingForInput;
            }
        }
    }

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

// ~~ private

// ~ Non-Static

// ~~ public
    public static HexGridEditorUI GetUI(HexGrid grid) {
        GameObject resultObj = Instantiate(
            Resources.Load<GameObject>("Hex Grid Editor UI")
        );
        HexGridEditorUI resultMono = resultObj.GetComponent<HexGridEditorUI>();
        resultMono.ActiveGrid = grid;
        resultMono.BrushDropdown =
            GameObject.Find("BrushDropdown").GetComponent<TMP_Dropdown>();
        resultMono.ModeDropdown =
           GameObject.Find("ModeDropdown").GetComponent<TMP_Dropdown>();

        return resultObj.GetComponent<HexGridEditorUI>();
    }

    public void SetShowGrid(bool show) {
        CheckGrid();
        ActiveGrid.ShowGrid = show;
    }

    public void RegisterUnitType(GameObject obj) {
        _unitObjects.Add(obj);
        RefreshPaintOptions(_brushDropdownAdapter);
    }

    public void OnPointerEnter(PointerEventData pData) {
        _pointerOverUI = true;
    }

    public void OnPointerExit(PointerEventData pData) {
        _pointerOverUI = false;
    }

// ~~ private
    private void RefreshPaintOptions(DropdownAdapter adapter) {
        adapter.Clear();
        RegisterTerrainPaintType(adapter, PaintTypes.Sand);
        RegisterTerrainPaintType(adapter, PaintTypes.Grass);
        RegisterTerrainPaintType(adapter, PaintTypes.Mud);
        RegisterTerrainPaintType(adapter, PaintTypes.Stone);
        RegisterTerrainPaintType(adapter, PaintTypes.Snow);
        RegisterTerrainPaintType(adapter, PaintTypes.Elevation);
        RegisterTerrainPaintType(adapter, PaintTypes.River);
        RegisterTerrainPaintType(adapter, PaintTypes.Water);
        RegisterTerrainPaintType(adapter, PaintTypes.Road);
        RegisterTerrainPaintType(adapter, PaintTypes.Plant);
        RegisterTerrainPaintType(adapter, PaintTypes.Urban);
        RegisterTerrainPaintType(adapter, PaintTypes.Farm);
        
        foreach (GameObject obj in _unitObjects) {
            RegisterGameObjectPaintType(adapter, obj);
        }
    }

    private void RefreshModeOptions(DropdownAdapter adapter, HexGrid grid) {
        adapter.Clear();
        RegisterMode(adapter, Modes.Editor, grid);
        RegisterMode(adapter, Modes.Runtime, grid);
    }

    private void RegisterTerrainPaintType(DropdownAdapter adapter, PaintTypes type) {
        adapter.AddOption(
            type.ToString(),
            () => {
                _paintType = type;
            }
        );   
    }

    private void RegisterGameObjectPaintType(DropdownAdapter adapter, GameObject obj) {
        adapter.AddOption(
            obj.name,
            () => {
                _paintType = PaintTypes.Unit;
                _unitToInstantiate = obj;
            }
        );
    }

    private void RegisterMode(DropdownAdapter adapter, Modes mode, HexGrid grid) {
        adapter.AddOption(
            mode.ToString(),
            () => {
                UpdateMode(grid, mode);
            }
        );
    }

    private void HandleSelect(HexGrid grid, HexCell cell, int magnitude) {
        cell = GetCellUnderCursor(grid);
        switch (_paintType) {
            case PaintTypes.Sand:
                cell.TerrainTypeIndex = (int)_paintType;
                break;
            case PaintTypes.Grass:
                cell.TerrainTypeIndex = (int)_paintType;
                break;
            case PaintTypes.Mud:
                cell.TerrainTypeIndex = (int)_paintType;
                break;
            case PaintTypes.Stone:
                cell.TerrainTypeIndex = (int)_paintType;
                break;
            case PaintTypes.Snow:
                cell.TerrainTypeIndex = (int)_paintType;
                break;
            case PaintTypes.Water:
                cell.WaterLevel += magnitude;
                break;
            case PaintTypes.Elevation:
                cell.Elevation += magnitude;
                break;
            case PaintTypes.Urban:
                cell.UrbanLevel += magnitude;
                break;
            case PaintTypes.Farm:
                cell.FarmLevel += magnitude;
                break;
            case PaintTypes.Plant:
                cell.PlantLevel += magnitude;
                break;
            default:
                break;
        }
    }

    private void HandleApply(HexGrid grid, HexCell cell) {
        switch (_paintType) {
            case PaintTypes.Unit:
                HexUnit unit = 
                    Instantiate(_unitToInstantiate).AddComponent<HexUnit>();
                grid.AddUnit(unit, cell, 0);
                break;
            default:
                break;
        }
    }

    private void HandleDrag(
        HexGrid grid, 
        HexCell previousCell, 
        HexCell currentCell,
        HexDirection dragDirection,
        bool add
    ) {
        switch(_paintType) {
            case PaintTypes.Road:
                if (add)
                    previousCell.AddRoad(dragDirection);
                else
                    previousCell.RemoveRoads();
                break;
            case PaintTypes.River:
                if (add)
                    previousCell.SetOutgoingRiver(dragDirection);
                else
                    previousCell.RemoveIncomingRiver();
                break;
            default:
                break;
        }
    }

    private HexCell GetCellUnderCursor(HexGrid grid) {
        return
            grid.GetCell(UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition));
    }

    private void EditCells(HexGrid grid, HexCell center) {
        int centerX = center.Coordinates.X;
        int centerZ = center.Coordinates.Z;

        for (int radius = 0, z = centerZ - _brushSize; z <= centerZ; z++, radius++) {
            for (int x = centerX - radius; x <= centerX + _brushSize; x++) {
                EditCell(grid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int radius = 0, z = centerZ + _brushSize; z > centerZ; z--, radius++) {
            for (int x = centerX - _brushSize; x <= centerX + radius; x++) {
                EditCell(grid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    private void EditCell(HexCell cell) {
/* Check for null reference returned by array out of
* bounds check in World class.
*/
        if (cell) {
            if (_isDrag) {
/* Work relative to the cell being edited to support different
* brush sizes.
*/
                HexCell otherCell = cell.GetNeighbor(_dragDirection.Opposite());
                if (otherCell) {
                    if (_riverMode == OptionalToggle.Yes) {
                        otherCell.SetOutgoingRiver(_dragDirection);
                    }
                    if (_roadMode == OptionalToggle.Yes) {
                        otherCell.AddRoad(_dragDirection);
                    }
                }
            }
        }
    }

    private void Awake() {
//_terrainMaterial.DisableKeyword("GRID_ON");
        _unitObjects = new List<GameObject>();
    }

    private void Update() {
        if(!_pointerOverUI) {
            CheckGrid();
            ProcessUpdate(ActiveGrid, _mode);
        }
    }

    private void ProcessUpdate(HexGrid grid, Modes mode) {
        HexCell cellUnderCursor = GetCellUnderCursor(grid);

        switch (mode) {
            case Modes.Editor:
                ProcessEditMode(grid, cellUnderCursor, EditState);
                break;
            case Modes.Runtime:
                ProcessRuntimeMode(grid, cellUnderCursor);
                break;
            default:
                throw new System.NotImplementedException();
        }
    }

    private void UpdateMode(HexGrid grid, Modes mode) {
        _mode = mode;
        if (mode == Modes.Editor) {
            grid.EditMode = true;
        }
        else if (mode == Modes.Runtime) {
            grid.EditMode = false;
        }     
    }

    private bool CheckGrid() {
        if (!GameObject.FindObjectOfType<HexGrid>())
            return false;
        else
            ActiveGrid = GameObject.FindObjectOfType<HexGrid>();

        return true;
    }

    private void ProcessEditMode(
        HexGrid grid,
        HexCell cellUnderCursor,
        EditStates editState
    ) {
        if (grid.HasPath) {
            grid.ClearPath();
        }

        if (!cellUnderCursor) {
            return;
        }

        HexCell selectedCell = null;

        int magnitude = 0;

        switch (editState) {
            case EditStates.Select:
                _previousDragCell = cellUnderCursor;
                selectedCell = cellUnderCursor;
                break;
            case EditStates.DragApply:
                if (cellUnderCursor != _previousDragCell) {
                    HexDirection dragDirection = GetDragDirection(
                        _previousDragCell, 
                        cellUnderCursor
                    );
                    HandleDrag(
                        grid, 
                        _previousDragCell, 
                        cellUnderCursor,
                        dragDirection,
                        true
                    );
                    _previousDragCell = cellUnderCursor;
                    selectedCell = cellUnderCursor;
                    magnitude = 1;
                }
                break;
            case EditStates.DragDelete:
                if (cellUnderCursor != _previousDragCell) {
                    HexDirection dragDirection = GetDragDirection(
                        _previousDragCell, 
                        cellUnderCursor
                    );
                    HandleDrag(
                        grid, 
                        _previousDragCell, 
                        cellUnderCursor,
                        dragDirection,
                        false
                    );
                    _previousDragCell = cellUnderCursor;
                    selectedCell = cellUnderCursor;
                    magnitude = -1;
                }
                break;
            case EditStates.Apply:
                _previousDragCell = null;
                selectedCell = cellUnderCursor;
                magnitude = 1;
                HandleApply(grid, selectedCell);
                break;
            case EditStates.Delete:
                _previousDragCell = null;
                selectedCell = cellUnderCursor;
                magnitude = -1;
                break;
            case EditStates.WaitingForInput:
                return;
            default:
                return;
        }

        if(selectedCell) {
            HandleSelect(grid, selectedCell, magnitude);
        }
    }

    private void ProcessRuntimeMode(
        HexGrid grid, 
        HexCell cellUnderCursor
    ) {
        HexCell selectedCell = null;

        if (Input.GetMouseButtonUp(0))
        {
            grid.ClearPath();
            selectedCell = cellUnderCursor;
            if (cellUnderCursor.Unit)
                _selectedUnit = cellUnderCursor.Unit;
            else
                _selectedUnit = null;            
        }
        else if (_selectedUnit)
        {
            if (Input.GetMouseButtonUp(1))
            {
                if (grid.HasPath) {
                    _selectedUnit.Travel(grid.GetPath());
                    grid.ClearPath();
                }
            }
            else
            {
                if(cellUnderCursor) {
                    if (cellUnderCursor && 
                        _selectedUnit.IsValidDestination(cellUnderCursor)
                    ) {
                        grid.FindPath(
                            _selectedUnit.Location, 
                            cellUnderCursor, 
                            _selectedUnit
                        );
                    }
                    else {
                        grid.ClearPath();
                    }
                }
            }
        }
    }

    private HexDirection GetDragDirection(HexCell previousCell, HexCell currentCell) {
        for (
            HexDirection result = HexDirection.Northeast;
            result <= HexDirection.Northwest;
            result++
        ) {
            if (previousCell.GetNeighbor(result) == currentCell) {
                return result;
            }
        }
        throw new System.Exception(previousCell + " -> " + currentCell);
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
