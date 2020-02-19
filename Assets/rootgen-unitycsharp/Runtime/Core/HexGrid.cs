using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RootLogging;
using RootCollections;

public class HexGrid : MonoBehaviour
{
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
    private int _seed;
    private bool _wrapping;
    private bool _editMode;
    private int _cellCountX;
    private int _cellCountZ;
    private Transform[] _columns;
    private Text _cellLabelPrefab;
    private List<HexUnit> _units = new List<HexUnit>();
    private int _chunkCountX;
    private int _chunkCountZ;        
    private int _searchFrontierPhase;
//Init to -1 so new maps always get centered.
    private int _currentCenterColumnIndex = -1;
    private HexGridChunk[] _chunks;
    private CellShaderData _cellShaderData;
    private CellPriorityQueue _searchFrontier;
    private HexCell[] _hexCells;
    private HexCell _currentPathTo;
    private HexCell _currentPathFrom;
    private bool _currentPathExists;
    private Material _terrainMaterial;
    private bool _uiVisible;
    
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
    public HexCell[] HexCells {
        get {
            return _hexCells;
        }
    }
    public bool Wrapping {
        get {
            return _wrapping;
        }
    }
    public int CellCountX {
        get {
            return _cellCountX;
        }
    }

    public int CellCountZ {
        get {
             return _cellCountZ;
        }
    }
    
    public bool ShowGrid {
        set {
            ShaderKeyword.GridOn = value;
        }
    }

    public bool EditMode {
        set {
            ShaderKeyword.HexMapEditMode = value;
            ShowUI(!value);
            _editMode = value;
        }

        get {
            return _editMode;
        }
    }

    public HexCell Center2D {
        get {
            if (_hexCells != null) {
                return _hexCells[_hexCells.Length/2];
            }
            return null;
        }
    }

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
    public static HexGrid GetGrid(
        int x = 0, 
        int z = 0, 
        bool wrapping = false,
        bool editMode = true
    ) {
        GameObject resultObj = new GameObject("HexGrid");
        HexGrid resultMono = resultObj.AddComponent<HexGrid>();
        resultMono.EditMode = editMode;
        return resultMono;
    }

// ~~ private

// ~ Non-Static

// ~~ public
    
    public bool HasPath {
        get { return _currentPathExists; }
    }

    public bool Initialize(int x, int z, bool wrapping) {
        if
        (
            x <= 0 || x % HexMetrics.chunkSizeX != 0 ||
            z <= 0 || z % HexMetrics.chunkSizeZ != 0
        ) {
            RootLog.Log(
                "Unsupported or empty map size. Setting map size to minimum " +
                "chunk size.",
                Severity.Warning,
                "HexGrid"
            );
            x = HexMetrics.chunkSizeX;
            z = HexMetrics.chunkSizeZ;
        }

        ClearPath();
        ClearUnits();

        if (_columns != null) {
            for (int i = 0; i < _columns.Length; i++) {
                Destroy(_columns[i].gameObject);
            }
        }

        _cellCountX = x;
        _cellCountZ = z;

        this._wrapping = wrapping;

        // Set to -1 so new maps always gets centered.
        _currentCenterColumnIndex = -1;
        HexMetrics.wrapSize = wrapping ? _cellCountX : 0;

        _chunkCountX = _cellCountX / HexMetrics.chunkSizeX;
        _chunkCountZ = _cellCountZ / HexMetrics.chunkSizeZ;
        _cellShaderData.Initialize(_cellCountX, _cellCountZ);

        CreateChunks();
        InitializeCells(x, z);

        return true;
    }

    public HexCell GetCell(Vector3 position) {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        return GetCell(coordinates);
    }

    public HexCell GetCell(HexCoordinates coordinates) {
        int z = coordinates.Z;

        // Check for array index out of bounds.
        if (z < 0 || z >= _cellCountZ) {
            return null;
        }

        int x = coordinates.X + z / 2;
        
        // Check for array index out of bounds.
        if (x < 0 || x >= _cellCountX) {
            return null;
        }
        
        return _hexCells[x + z * _cellCountX];
    }

    public HexCell GetCell(Ray ray) {
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit)) {
            return GetCell(hit.point);
        }

        return null;
    }

    public HexCell GetCell(int xOffset, int zOffset) {
        return _hexCells[xOffset + zOffset * _cellCountX];
    }

    public HexCell GetCell(int cellIndex) {
        return _hexCells[cellIndex];
    }

    public void FindPath(HexCell fromCell, HexCell toCell, HexUnit unit) {
        ClearPath();
        _currentPathFrom = fromCell;
        _currentPathTo = toCell;
        _currentPathExists = Search(fromCell, toCell, unit);
        ShowPath(unit.Speed);
    }

    public void AddUnit(HexUnit unit, HexCell location, float orientation) {
        _units.Add(unit);
        unit.Grid = this;
        unit.Location = location;
        unit.Orientation = orientation;
    }

    public void RemoveUnit(HexUnit unit) {
        _units.Remove(unit);
        unit.Die();
    }

    public void ClearPath() {
        if (_currentPathExists) {
            HexCell current = _currentPathTo;
            while (current != _currentPathFrom) {
                current.SetLabel(null);
                current.DisableHighlight();
                current = current.PathFrom;
            }
            current.DisableHighlight();
            _currentPathExists = false;
        }
        else if (_currentPathFrom) {
            _currentPathFrom.DisableHighlight();
            _currentPathTo.DisableHighlight();
        }
    }

    public List<HexCell> GetPath() {
        if (!_currentPathExists) {
            return null;
        }

        List<HexCell> path = ListPool<HexCell>.Get();

        for (
            HexCell cell = _currentPathTo;
            cell != _currentPathFrom; 
            cell = cell.PathFrom
        ) {
            path.Add(cell);
        }

        path.Add(_currentPathFrom);
        path.Reverse();

        return path;
    }

    public void IncreaseVisibility(HexCell fromCell, int range) {
        List<HexCell> cells = GetVisibleCells(fromCell, range);

        for (int i = 0; i < cells.Count; i++) {
            cells[i].IncreaseVisibility();
        }

        ListPool<HexCell>.Add(cells);
    }

    public void DecreaseVisibility(HexCell fromCell, int range) {
        List<HexCell> cells = GetVisibleCells(fromCell, range);

        for (int i = 0; i < cells.Count; i++) {
            cells[i].DecreaseVisibility();
        }

        ListPool<HexCell>.Add(cells);
    }

    public void ResetVisibility() {
        for (int i = 0; i < _hexCells.Length; i++) {
            _hexCells[i].ResetVisibility();
        }

        for (int i = 0; i < _units.Count; i++) {
            HexUnit unit = _units[i];
            IncreaseVisibility(unit.Location, unit.VisionRange);
        }
    }


    public void CenterMap(float xPosition) {
        // Get the column index which the x axis coordinate is over.
        int centerColumnIndex =
            (int) (xPosition / (HexMetrics.innerDiameter * HexMetrics.chunkSizeX));

        if (centerColumnIndex == _currentCenterColumnIndex) {
            return;
        }

        _currentCenterColumnIndex = centerColumnIndex;

        int minColumnIndex = centerColumnIndex - _chunkCountX / 2;
        int maxColumnIndex = centerColumnIndex + _chunkCountX / 2;

        Vector3 position;
        position.y = position.z = 0f;

        for (int i = 0; i < _columns.Length; i++) {
            if (i < minColumnIndex) {
                position.x = _chunkCountX *
                                (HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
            }
            else if (i > maxColumnIndex) {
                position.x = _chunkCountX *
                                -(HexMetrics.innerDiameter * HexMetrics.chunkSizeX);
            }
            else {
                position.x = 0f;
            }

            _columns[i].localPosition = position;
        }
    }

    public void MakeChildOfColumn(Transform child, int columnIndex) {
        child.SetParent(_columns[columnIndex], false);
    }

    public HexGrid GetGrid(int width = 15, int height = 20) {
        GameObject resultObj = new GameObject("Hex Grid");
        HexGrid resultMono = resultObj.AddComponent<HexGrid>();
        return resultMono;
    }

// ~~ private
    private void ShowUI(bool visible) {
        for (int i = 0; i < _chunks.Length; i++) {
            _chunks[i].ShowUI(visible);
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

    private void ShowPath(int speed) {
        if (_currentPathExists) {
            HexCell current = _currentPathTo;

            while (current != _currentPathFrom) {
                int turn = (current.Distance - 1) / speed;
                current.SetLabel(turn.ToString());
                current.EnableHighlight(Color.white);
                current = current.PathFrom;
            }
        }
        _currentPathFrom.EnableHighlight(Color.blue);
        _currentPathTo.EnableHighlight(Color.red);
    }

    private void ClearUnits() {
        for (int i = 0; i < _units.Count; i++) {
            _units[i].Die();
        }

        _units.Clear();
    }

    private bool Search(HexCell fromCell, HexCell toCell, HexUnit unit)
    {
        int speed = unit.Speed;

        _searchFrontierPhase += 2;

        if (_searchFrontier == null)
        {
            _searchFrontier = new CellPriorityQueue();
        }
        else
        {
            _searchFrontier.Clear();
        }

        /* Temporarily using a list instead of a priority queue.
        * Should optimize this later.
        */
        fromCell.SearchPhase = _searchFrontierPhase;
        fromCell.Distance = 0;
        _searchFrontier.Enqueue(fromCell);

        while (_searchFrontier.Count > 0)
        {
            HexCell current = _searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == toCell)
            {
                return true;
            }

            int currentTurn = (current.Distance - 1) / speed;

            for (HexDirection direction = HexDirection.Northeast; direction <= HexDirection.Northwest; direction++)
            {
                HexCell neighbor = current.GetNeighbor(direction);

                if 
                (
                    neighbor == null ||
                    neighbor.SearchPhase > _searchFrontierPhase
                )
                {
                    continue;
                }

                if (!unit.IsValidDestination(neighbor))
                {
                    continue;
                }

                int moveCost = unit.GetMoveCost(current, neighbor, direction);

                if (moveCost < 0)
                {
                    continue;
                }

                /* Wasted movement points are factored into the cost of cells outside
                * the boundary of the first turn by adding the turn number multiplied
                * by the speed plus the cost to move into the cell outside the boundary
                * of the first turn. This method ensures that the the distances with
                * which the algorithm is using to calculate the best path take into
                * account wasted movement points.
                */
                int distance = current.Distance + moveCost;
                int turn = (distance - 1) / speed;

                if (turn > currentTurn)
                {
                    distance = turn * speed + moveCost;
                }

                if (neighbor.SearchPhase < _searchFrontierPhase)
                {
                    neighbor.SearchPhase = _searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic =
                        neighbor.Coordinates.DistanceTo(toCell.Coordinates);

                    _searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    _searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }

        return false;
    }

    private List<HexCell> GetVisibleCells(HexCell fromCell, int range)
    {
        List<HexCell> visibleCells = ListPool<HexCell>.Get();

        _searchFrontierPhase += 2;

        if (_searchFrontier == null)
        {
            _searchFrontier = new CellPriorityQueue();
        }
        else
        {
            _searchFrontier.Clear();
        }

        range += fromCell.ViewElevation;

        /* Temporarily using a list instead of a priority queue.
        * Should optimize this later.
        */
        fromCell.SearchPhase = _searchFrontierPhase;
        fromCell.Distance = 0;
        _searchFrontier.Enqueue(fromCell);

        HexCoordinates fromCoordinates = fromCell.Coordinates;

        while (_searchFrontier.Count > 0)
        {
            HexCell current = _searchFrontier.Dequeue();
            current.SearchPhase += 1;

            visibleCells.Add(current);

            for (HexDirection direction = HexDirection.Northeast; direction <= HexDirection.Northwest; direction++)
            {
                HexCell neighbor = current.GetNeighbor(direction);

                if
                (
                    neighbor == null ||
                    neighbor.SearchPhase > _searchFrontierPhase ||
                    !neighbor.IsExplorable
                )
                {
                    continue;
                }
                
                int distance = current.Distance + 1;

                if 
                (
                    distance + neighbor.ViewElevation > range ||
                    distance > fromCoordinates.DistanceTo(neighbor.Coordinates)
                )
                {
                    continue;
                }

                if (neighbor.SearchPhase < _searchFrontierPhase)
                {
                    neighbor.SearchPhase = _searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.SearchHeuristic = 0;
                    _searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance)
                {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    _searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }

        return visibleCells;
    }

    private void CreateChunks()
    {
        _columns = new Transform[_chunkCountX];

        for (int x = 0; x < _chunkCountX; x++)
        {
            _columns[x] = new GameObject("Column").transform;
            _columns[x].SetParent(transform, false);
        }

        _chunks = new HexGridChunk[_chunkCountX * _chunkCountZ];

        for (int z = 0, i = 0; z < _chunkCountZ; z++)
        {
            for (int x = 0; x < _chunkCountX; x++)
            {
                HexGridChunk chunk = _chunks[i++] = HexGridChunk.GetChunk();
                chunk.transform.SetParent(_columns[x], false);
            }
        }
    }

    private void InitializeCells(int width, int height)
    {
        _hexCells = new HexCell[width * height];

        for (int z = 0, i = 0; z < _cellCountZ; z++)
        {
            for (int x = 0; x < _cellCountX; x++)
            {
                InitializeCell(x, z, i++);
            }
        }
    }
    
    private void InitializeCell(int x, int z, int i)
    {
        Vector3 position;

        /*The distance between the center of two hexagons on the x axis is equal to twice the inner radius 
        * of a given hexagon. Additionally, half of the cells position on the z axis (cartesian y axis)
        * is added to its position on the x axis as an offset, and the integer division of the cells
        * position on the z axis is subtracted from that value. For even rows this negates the offset.
        * For odd rows, the integer is rounded down and the offset is retained.*/
        position.x = (x + z * 0.5f - z / 2) * HexMetrics.innerDiameter;
        position.y = 0f;

        /*The distance between the center of two hexagons on the z axis (cartesian y axis) is equal to
        * one and one half the outer radius of a given hexagon.*/
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = _hexCells[i] = HexCell.GetCell();
        cell.transform.localPosition = position;
        cell.Coordinates = HexCoordinates.AsAxialCoordinates(x, z);
        cell.Index = i;
        cell.ColumnIndex = x / HexMetrics.chunkSizeX;
        cell.ShaderData = _cellShaderData;

        if (_wrapping)
        {
            cell.IsExplorable = z > 0 && z < _cellCountZ - 1;
        }
        else
        {
            cell.IsExplorable =
                x > 0 && z > 0 && x < _cellCountX - 1 && z < _cellCountZ - 1;
        }
        
        /* At the beginning of each row, x == 0. Therefore, if x is greater than 0, set the east/west
        * connection of the cell between the current cell and the previous cell in the array
        */
        if (x > 0)
        {
            cell.SetNeighborPair(HexDirection.West, _hexCells[i - 1]);
            if (_wrapping && x == _cellCountX - 1)
            {
                cell.SetNeighborPair(HexDirection.East, _hexCells[i - x]);
            }
        }
        
        /* At the first row, z == 0. The first row has no southern neighbors. Therefore 
            *
            */
        if (z > 0)
        {
            /* Use the bitwise operator to mask off all but the first bit. If the result is 0,
                * the number is even:
                *      11 (3) & 1(1) == 1
                *       ^
                *       |
                *       AND only compares the length of the smallest binary sequence
                *       |
                *      10 (2) & 1(1) == 0
                *      
                * Because all  cells in even rows have a southeast neighbor, they can be connected.
                */

            if ((z & 1) == 0)
            {
                cell.SetNeighborPair(HexDirection.SouthEast, _hexCells[i - _cellCountX]);

                //All even cells except for the first cell in each row have a southwest neighbor.
                if (x > 0)
                {
                    cell.SetNeighborPair(HexDirection.SouthWest, _hexCells[i - _cellCountX - 1]);
                }
                else if (_wrapping)
                {
                    cell.SetNeighborPair(HexDirection.SouthWest, _hexCells[i - 1]);
                }
            }
            else
            {
                cell.SetNeighborPair(HexDirection.SouthWest, _hexCells[i - _cellCountX]);

                //All odd cells except the last cell in each row have a southeast neighbor
                if (x < _cellCountX - 1)
                {
                    cell.SetNeighborPair(HexDirection.SouthEast, _hexCells[i - _cellCountX + 1]);
                }
                else if (_wrapping)
                {
                    cell.SetNeighborPair(HexDirection.SouthEast, _hexCells[i - _cellCountX * 2 + 1]);
                }
            }
        }

        Text label = Instantiate<Text>(_cellLabelPrefab);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        cell.uiRect = label.rectTransform;
        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }

    private void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = _chunks[chunkX + chunkZ * _chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;

        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    private void Awake()
    {
        HexMetrics.InitializeHashGrid(_seed);
        _terrainMaterial = Resources.Load<Material>("Terrain");
        _cellShaderData = gameObject.AddComponent<CellShaderData>();
        _cellShaderData.HexGrid = this;
        _hexCells = new HexCell[0];
        _chunks = new HexGridChunk[0];
        _cellLabelPrefab = Resources.Load<Text>("Hex Cell Label");
    }

    private void OnEnable()
    {
        HexMetrics.InitializeHashGrid(_seed);
        HexMetrics.wrapSize = _wrapping ? _cellCountX : 0;
        ResetVisibility();
    }
}
