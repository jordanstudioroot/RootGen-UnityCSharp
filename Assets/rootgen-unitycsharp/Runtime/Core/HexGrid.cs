using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RootLogging;
using RootCollections;
using QuikGraph;
using QuikGraph.Algorithms;
using DenseArray;

public class HexGrid : MonoBehaviour {
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
    private List<HexEdge> _adjacencyEdges;
    private HexAdjacencyGraph _adjacencyGraph;
    private DenseArray<HexCell> _cellMatrix;
    private int _seed;
    private bool _wrapping;
    private bool _editMode;

/// <summary>
/// A Rect representing the latitudinal and longitudinal bounds of the HexGrid.
/// </summary>
    private Rect _gridBounds;
    private Rect _bounds;
    private Transform[] _columns;
    private Text _cellLabelPrefab;
    private List<HexUnit> _units = new List<HexUnit>();
    private int _chunkWidth;
    private int _chunkHeight;        
    private int _searchFrontierPhase;
//Init to -1 so new maps always get centered.
/// <summary>
/// The index of the MeshChunk column currently centered below the camera.
/// </summary>
    private int _currentCenterColumnIndex = -1;
    private HexGridChunk[] _chunks;
    private CellShaderData _cellShaderData;
    private CellPriorityQueue _searchFrontier;
    private HexCell _currentPathFrom;
    private bool _currentPathExists;
    private Material _terrainMaterial;
    private bool _uiVisible;
    private int _numCells;

    public DenseArray<HexCell> CellMatrix {
        get {
            return _cellMatrix;
        }
    }
    public bool Wrapping {
        get {
            return _wrapping;
        }
    }
    public int WidthInCells {
        get {
            return (int)_gridBounds.width;
        }
    }

    public int HeightInCells {
        get {
             return (int)_gridBounds.height;
        }
    }

    public int WidthInChunks {
        get {
            return (int)_gridBounds.width / MeshConstants.ChunkSizeX;
        }
    }

    public int HeightInChunks {
        get {
            return (int)_gridBounds.height / MeshConstants.ChunkSizeZ;
        }
    }

    public bool ShowGrid {
        set {
            HexGridShaderKeywords.GridOn = value;
        }
    }

    public bool EditMode {
        set {
            HexGridShaderKeywords.HexMapEditMode = value;
            ShowUIAllChunks(!value);
            _editMode = value;
        }

        get {
            return _editMode;
        }
    }

    public HexCell Center2D {
        get {
            if (_cellMatrix != null) {
                return _cellMatrix[
                    (_cellMatrix.Rows / 2) - 1,
                    (_cellMatrix.Columns / 2) - 1
                ];
            }
            return null;
        }
    }

    public int NumCells {
        get {
            if (_cellMatrix == null) {
                return 0;
            }

            return _cellMatrix.Count;
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

/// <summary>
///     Add and edge to the HexGrid.
/// </summary>
/// <param name="source">
///     The source of the edge.
/// </param>
/// <param name="target">
///     The target of the edge.
/// </param>
    public void AddEdge(
        HexCell source,
        HexCell target,
        ElevationEdgeType edgeType,
        bool hasRoad = false,
        bool hasRiver = false
    ) {
        HexEdge newEdge = new HexEdge(
            source,
            target
        );

        _adjacencyEdges.Add(newEdge);
        _adjacencyGraph.AddEdge(newEdge);
    }
    
/// <summary>
///     Get a list of direct neighbors of the specified hex cell.
/// </summary>
/// <param name="cell">
///     The cell adjacent to the desired cells.
/// </param>
/// <param name="graph">
///     A bidirectional graph containing HexCells.
/// </param>
/// <returns>
///     A list containing the targets of all out edge sof the specified
///     cell.
/// </returns>
    public List<HexCell> GetNeighbors(
        HexCell cell,
        IBidirectionalGraph<HexCell, IEdge<HexCell>> graph
    ) {
        IEnumerable<IEdge<HexCell>> outEdges;
        graph.TryGetOutEdges(cell, out outEdges);

        List<HexCell> result = new List<HexCell>();

        foreach(HexEdge edge in outEdges) {
            result.Add(edge.Target);
        }

        return result;
    }

    public void ClearColumns() {
         if (_columns != null) {
            for (int i = 0; i < _columns.Length; i++) {
                Destroy(_columns[i].gameObject);
            }
        }
    }

/// <summary>
///     Initializes the grid with a 2d cell matrix and mesh chunk data based on the
///     provided width, height, cell outer radius, and wrapping..
/// </summary>
/// <param name="widthInCells">
///     The width of the grid in cells.
/// </param>
/// <param name="heightInCells">
///     The height of the grid in cells.
/// </param>
/// <param name="horizontalWrapping">
///     Should the horizontal bounds of the grid wrap into their opposite side? If enabled,
///     the cells on one horizontal bound are connected to their corresponding cell on
///     the opposite horizontal bound. 
/// </param>
/// <param name="cellOuterRadius">
///     The distance of each hex cell from its center to a circle intersecting each
///     corner of the hexagon. Controls the size of each hex cell.
/// </param>
    public void Initialize(
        int widthInCells,
        int heightInCells,
        bool horizontalWrapping,
        float cellOuterRadius
    ) {
        Vector2 proposedBounds = new Vector2(
            widthInCells,
            heightInCells
        );

        Vector2 chunkSize = new Vector2(
            MeshConstants.ChunkSizeX,
            MeshConstants.ChunkSizeZ
        );

        if (
            !proposedBounds.IsFactorOf(chunkSize)
        ) {
            RootLog.Log(
                "Unsupported or empty map size. Clamping dimensions to chunk size."
            );
            Vector2 clamped =
                proposedBounds.ClampToFactorOf(chunkSize);

            _gridBounds = new Rect(
                chunkSize.x,
                chunkSize.y,
                clamped.x,
                clamped.y
            );
        }
        else {
            _gridBounds = new Rect(
                chunkSize.x,
                chunkSize.y,
                proposedBounds.x,
                proposedBounds.y
            );
        }

        ClearUnits();
        ClearColumns();

        this._wrapping = horizontalWrapping;

// Set to -1 so new maps always gets centered.
        _currentCenterColumnIndex = -1;

        HexagonPoint.wrapSize = horizontalWrapping ? WidthInCells : 0;

        _cellShaderData.Initialize(
            WidthInCells,
            HeightInCells
        );

        _cellMatrix = CreateCellDenseArray(
            _gridBounds,
            cellOuterRadius,
            WidthInChunks
        );

        CreateChunks(
            cellOuterRadius,
            ref _columns,
            WidthInChunks,
            HeightInChunks
        );
    }

/// <summary>
/// Returns true if the provided dimensions 
/// </summary>
/// <param name="sizeX"></param>
/// <param name="sizeZ"></param>
/// <param name="meshChunkSizeX"></param>
/// <param name="meshChunkSizeZ"></param>
/// <returns></returns>
    public bool Is2DFactorOf(
        int sizeX,
        int sizeZ,
        int meshChunkSizeX,
        int meshChunkSizeZ
    ) {
        if (
            sizeX < meshChunkSizeX ||
            sizeX % meshChunkSizeX != 0 ||
            sizeZ < meshChunkSizeZ ||
            sizeZ % meshChunkSizeZ != 0
        ) {
            return false;
        }

        return true;
    }

    public Rect ClampToChunkSize(
        int x,
        int z,
        int chunkSizeX,
        int chunkSizeZ
    ) {
        int xClamped = Mathf.Clamp(
            x,
            chunkSizeX,
            x - (x % chunkSizeX)
        );

        int zClamped = Mathf.Clamp(
            z,
            chunkSizeZ,
            z - (z % chunkSizeZ)
        );

        return new Rect(0, 0, xClamped, zClamped);
    }

    public HexCell GetCell(
        Vector3 position,
        float outerCellRadius
    ) {
        position = transform.InverseTransformPoint(position);
        
        HexCoordinates coordinates =
            HexCoordinates.FromPosition(position, outerCellRadius);
        
        return GetCell(coordinates);
    }

    public HexCell GetCell(
        HexCoordinates coordinates
    ) {
        int z = coordinates.Z;

        // Check for array index out of bounds.
        if (z < 0 || z >= HeightInCells) {
            return null;
        }

        int x = coordinates.X + z / 2;
        
        // Check for array index out of bounds.
        if (x < 0 || x >= WidthInCells) {
            return null;
        }
        
        return _cellMatrix[x + z * WidthInCells];
    }

    public HexCell GetCell(Ray ray, float outerRadius) {
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit)) {
            return GetCell(hit.point, outerRadius);
        }

        return null;
    }

    public HexCell GetCell(int xOffset, int zOffset) {
        return _cellMatrix[xOffset + zOffset * WidthInCells];
    }

    public HexCell GetCell(int cellIndex) {
        return _cellMatrix[cellIndex];
    }

    public IEnumerable<HexEdge> GetPath(
        HexCell fromCell,
        HexCell toCell,
        HexUnit unit,
        HexAdjacencyGraph graph
    ) {
        ClearPath(toCell); 
        return AStarSearch(fromCell, toCell, unit, graph);

// Presentation concerns should not be in this method.        
//        SetPathDistanceLabelAndEnableHighlights(toCell, unit.Speed);
    }

    public void AddUnit(
        HexUnit unit,
        HexCell location,
        float orientation
    ) {
        _units.Add(unit);
        
        unit.Grid = this;
        unit.Location = location;
        unit.Orientation = orientation;
    }

    public void RemoveUnit(HexUnit unit) {
        _units.Remove(unit);
        unit.Die();
    }

    public void ClearPath(
        HexCell currentPathTo
    ) {
        if (_currentPathExists) {
            HexCell current = currentPathTo;
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
            currentPathTo.DisableHighlight();
        }
    }

    public List<HexCell> GetPath(
        HexCell currentPathTo
    ) {
        if (!_currentPathExists) {
            return null;
        }

        List<HexCell> path = ListPool<HexCell>.Get();

        for (
            HexCell cell = currentPathTo;
            cell != _currentPathFrom; 
            cell = cell.PathFrom
        ) {
            path.Add(cell);
        }

        path.Add(_currentPathFrom);
        path.Reverse();

        return path;
    }

// TODO: This is a presentation concern and should be moved out of
//       this class.
    public void IncreaseVisibility(
        HexCell fromCell,
        int range,
        HexAdjacencyGraph graph
    ) {
        List<HexCell> cells =
        GetVisibleCells(
            fromCell,
            range,
            graph
        );

        for (int i = 0; i < cells.Count; i++) {
            cells[i].IncreaseVisibility();
        }

        ListPool<HexCell>.Add(cells);
    }

// TODO: This is a presentation concern and should be moved out of
//       this class.
    public void DecreaseVisibility(
        HexCell fromCell,
        int range,
        HexAdjacencyGraph graph
    ) {
        List<HexCell> cells =
            GetVisibleCells(
                fromCell,
                range,
                graph
            );

        for (int i = 0; i < cells.Count; i++) {
            cells[i].DecreaseVisibility();
        }

        ListPool<HexCell>.Add(cells);
    }

// TODO: This is a presentation concern and should be moved out of this
//       class
    public void ResetVisibility() {
        for (int i = 0; i < _cellMatrix.Count; i++) {
            _cellMatrix[i].ResetVisibility();
        }

        for (int i = 0; i < _units.Count; i++) {
            HexUnit unit = _units[i];
            
            IncreaseVisibility(
                unit.Location,
                unit.VisionRange,
                _adjacencyGraph
            );
        }
    }


    public void CenterMap(
        float xPosition,
        float cellOuterRadius
    ) {
        float innerDiameter = 
            HexagonPoint.GetOuterToInnerRadius(cellOuterRadius) * 2f;
        // Get the column index which the x axis coordinate is over.
        int centerColumnIndex =
            (int) (xPosition / (innerDiameter * MeshConstants.ChunkSizeX));

        if (centerColumnIndex == _currentCenterColumnIndex) {
            return;
        }

        _currentCenterColumnIndex = centerColumnIndex;

        int minColumnIndex = centerColumnIndex - _chunkWidth / 2;
        int maxColumnIndex = centerColumnIndex + _chunkWidth / 2;

        Vector3 position;
        position.y = position.z = 0f;

        for (int i = 0; i < _columns.Length; i++) {
            if (i < minColumnIndex) {
                position.x = _chunkWidth *
                                (innerDiameter * MeshConstants.ChunkSizeX);
            }
            else if (i > maxColumnIndex) {
                position.x = _chunkWidth *
                                -(innerDiameter * MeshConstants.ChunkSizeX);
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

/// <summary>
///     Switches the UI on and off for all HexGridChunks, enabling and disabling
///     features such as the distance from the currently selected hex cell.
/// </summary>
/// <param name="visible">
///     The visible state of all HexGridChunks.
/// </param>
    private void ShowUIAllChunks(bool visible) {
        for (int i = 0; i < _chunks.Length; i++) {
            _chunks[i].ShowUI(visible);
        }
    }

/// <summary>
/// Destroy all HexUnits on this HexGrid.
/// </summary>
    private void ClearUnits() {
        for (int i = 0; i < _units.Count; i++) {
            _units[i].Die();
        }

        _units.Clear();
    }

    private double GetPathfindingEdgeWeight(HexEdge edge) {
        return 0d;
    }

    private double GetPathfindingHeursitic(HexCell cell) {
        return 0d;
    }

/// <summary>
/// Search this HexGrid.
/// </summary>
/// <param name="start"></param>
/// <param name="end"></param>
/// <param name="unit"></param>
/// <returns></returns>
    private IEnumerable<HexEdge> AStarSearch(
        HexCell start,
        HexCell end,
        HexUnit unit,
        HexAdjacencyGraph graph
    ) {
        IEnumerable<HexEdge> result;
        
        AlgorithmExtensions.ShortestPathsAStar<HexCell, HexEdge>(
            graph,
            GetPathfindingEdgeWeight,
            GetPathfindingHeursitic,
            start
        ).Invoke(end, out result);
        
        return result;

/*        int speed = unit.Speed;

        _searchFrontierPhase += 2;

        if (_searchFrontier == null)
        {
            _searchFrontier = new CellPriorityQueue();
        }
        else
        {
            _searchFrontier.Clear();
        }

// Temporarily using a list instead of a priority queue.
// Should optimize this later.
//
        start.SearchPhase = _searchFrontierPhase;
        start.Distance = 0;
        _searchFrontier.Enqueue(start);

        while (_searchFrontier.Count > 0)
        {
            HexCell current = _searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == end)
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

// Wasted movement points are factored into the cost of cells outside
// the boundary of the first turn by adding the turn number multiplied
// by the speed plus the cost to move into the cell outside the boundary
// of the first turn. This method ensures that the the distances with
// which the algorithm is using to calculate the best path take into
// account wasted movement points.
//
                int distance = current.Distance + moveCost;
                int turn = (distance - 1) / speed;

                if (turn > currentTurn) {
                    distance = turn * speed + moveCost;
                }

                if (neighbor.SearchPhase < _searchFrontierPhase) {
                    neighbor.SearchPhase = _searchFrontierPhase;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    neighbor.SearchHeuristic =
                        neighbor.Coordinates.DistanceTo(end.Coordinates);

                    _searchFrontier.Enqueue(neighbor);
                }
                else if (distance < neighbor.Distance) {
                    int oldPriority = neighbor.SearchPriority;
                    neighbor.Distance = distance;
                    neighbor.PathFrom = current;
                    _searchFrontier.Change(neighbor, oldPriority);
                }
            }
        }

        return false;
*/   
    }

// This should be a simple graph traversal which stops when it hits an
// edge which has a higher elevation.
    private List<HexCell> GetVisibleCells(
        HexCell fromCell,
        int sightRange,
        HexAdjacencyGraph graph
    ) {


// USE OUT EDGES OF ADJACENCY GRAPH INSTEAD
// This method represents returning a breadth first list of the graph
// which terminates when an edge is encountered where the out edge
// target is higher in elevation than the out edge source.
        Queue<HexEdge> edgeQueue = (Queue<HexEdge>)graph.OutEdges(fromCell);
        List<HexCell> result = new List<HexCell>();
        result.Add(fromCell);

        while (edgeQueue.Count > 0) {
            HexEdge current = edgeQueue.Dequeue();

            if (
                current.Source.Elevation >= current.Target.Elevation
            ) {
                result.Add(current.Target);
                
                foreach (
                    HexEdge edge in
                    (List<HexEdge>)graph.OutEdges(current.Target)
                ) {
                   edgeQueue.Enqueue(edge); 
                }
            }
        }

        return result;

/*       
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

        sightRange += fromCell.ViewElevation;

// Temporarily using a list instead of a priority queue.
// Should optimize this later.
//
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
                    distance + neighbor.ViewElevation > sightRange ||
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
        */
    }

    private HexGridChunk[] CreateChunks(
        float cellOuterRadius,
        ref Transform[] columns,
        int chunkCountX,
        int chunkCountZ
    ) {
        HexGridChunk[] result = new HexGridChunk[chunkCountX * chunkCountZ];
        columns = new Transform[_chunkWidth];

        for (int x = 0; x < chunkCountX; x++) {
            columns[x] = new GameObject("Column").transform;
            columns[x].SetParent(transform, false);
        }

        for (int z = 0, i = 0; z < chunkCountZ; z++) {
            for (int x = 0; x < chunkCountX; x++) {
                HexGridChunk chunk = result[i++] =
                    HexGridChunk.GetChunk(cellOuterRadius);

                chunk.transform.SetParent(columns[x], false);
            }
        }

        return result;
    }

/// <summary>
/// Get a new cell matrix represented as a DenseArray.
/// </summary>
/// <param name="gridBounds">
///     A rect representing the dimensions of the HexGrid to be represented
///     by the matrix.
/// </param>
/// <param name="cellOuterRadius">
///     The distance of each hex cell from its center to a circle intersecting each
///     corner of the hexagon. Controls the size of each hex cell.
/// </param>
/// <param name="chunkSizeX">
///     The size of a mesh chunk along the x axis.
/// </param>
/// <returns>
///     A DenseArray cooresponding to the specified bounds containing HexCells
///     corresponding to the specified radius and chunkSizeX.
/// </returns>
    private DenseArray<HexCell> CreateCellDenseArray(
        Rect gridBounds,
        float cellOuterRadius,
        int chunkSizeX
    ) {
        int matrixRows = (int) gridBounds.height;
        int matrixColumns = (int) gridBounds.width;
        
        DenseArray<HexCell> result = new DenseArray<HexCell>(
            matrixRows,
            matrixColumns
        );

        for (int row = 0, i = 0; row < matrixRows; row++) {
            for (int column = 0; column < matrixColumns; column++) {
                result[row, column] = CreateCell(
                    column,
                    row,
                    i++,
                    cellOuterRadius,
                    chunkSizeX
                );
            }
        }
        
        return result;
    }

    private HexDigraph CreateAdjacencyGraph(
        int width,
        int height,
        float cellOuterRadius,
        int chunkSizeX,
        DenseArray<HexCell> cells
    ) {        
        HexDigraph result = new HexDigraph();
        
        List<HexEdge> edges = CreateAdjacencyEdges(cells);
        result.AddEdgeRange(edges);
        
        return result;
    }

    private List<HexEdge> CreateAdjacencyEdges(DenseArray<HexCell> cells) {
        List<HexEdge> result = new List<HexEdge>();

        int numCells = cells.Columns * cells.Rows;

        for (int i = 0; i < numCells; i++) {
            for (int dz = -1; dz <= 1; dz++) {
                for (int dx = -1; dx <= 1; dx++) {
                    if (cells[dz, dx] && cells[dz, dx] != cells[i]) {
                        result.Add(
                            new HexEdge(
                                cells[i],
                                cells[dz, dx]
                            )
                        );
                    }
                }
            }
        }

        return result;
    }    

    private Vector3 CoordinateToLocalPosition(
        int x,
        int z,
        float innerDiameter,
        float cellOuterRadius
    ) {
        return new Vector3(
// The distance between the center of two hexagons on the x axis is equal to
// twice the inner radius of a given hexagon. Additionally, for half of the
// cells, the position on the z axis (cartesian y axis) is added to its position
// on the x axis as an offset, and the integer division of the position of
// the cell on the z axis is subtracted from that value. For even rows this
// negates the offset. For odd rows, the integer is rounded down and the offset
// is retained.
            (x + z * 0.5f - z / 2) * innerDiameter,
            0,
// The distance between the center of two hexagons on the z axis (cartesian y axis) is equal to
// one and one half the outer radius of a given hexagon.
            z * (cellOuterRadius * 1.5f)
        );
    }


/// <summary>
/// Create a Cell representing the data 
/// </summary>
/// <param name="x"></param>
/// <param name="z"></param>
/// <param name="i"></param>
/// <param name="cellOuterRadius"></param>
/// <param name="chunkSizeX"></param>
/// <returns></returns>    
    private HexCell CreateCell(
        int x,
        int z,
        int i,
        float cellOuterRadius,
        int chunkSizeX
    ) {
// Get the inner diameter of the cell as a scaling value for all other
// metrics.
        float innerDiameter =
            HexagonPoint.GetOuterToInnerRadius(cellOuterRadius) * 2f;

// Create the HexCell's object and monobehaviour.
        HexCell result = HexCell.GetCell();

// Set the HexCell's transform.
        result.transform.localPosition = CoordinateToLocalPosition(
            x,
            z,
            innerDiameter,
            cellOuterRadius
        );

// Set the HexCell's monobehaviour properties.
        result.Coordinates = HexCoordinates.AsAxialCoordinates(x, z);
        result.Index = i;
        result.ColumnIndex = x / MeshConstants.ChunkSizeX;
        result.ShaderData = _cellShaderData;

// If wrapping is enabled, cell is not explorable if the cell is on the
// top or bottom border.
        if (_wrapping) {
            result.IsExplorable = z > 0 && z < HeightInCells - 1;
        }
// If wrapping is disabled, cell is not explorable if the cell is on
// any border.
        else {
            result.IsExplorable =
                x > 0 &&
                z > 0 &&
                x < WidthInCells - 1 &&
                z < HeightInCells - 1;
        }

// THIS IS NOW HANDLED BY MAPPING THE DENSEARRAY TO AN ADJACENCY GRAP
// 
// At the beginning of each row, x == 0. Therefore, if x is greater than
// 0, set the east/west connection of the cell between the current cell
// and the previous cell in the array.
//        if (x > 0) {
//            result.SetNeighborPair(HexDirection.West, result[i - 1]);
//
//            if (_wrapping && x == _cellCountX - 1) {
//                result.SetNeighborPair(HexDirection.East, result[i - x]);
//            }
//        }
//        
// At the first row, z == 0. The first row has no southern neighbors. Therefore
//
//        if (z > 0)
//        {
// Use the bitwise operator to mask off all but the first bit. If the result is 0,
// the number is even:
//      11 (3) & 1(1) == 1
//       ^
//       |
//       AND only compares the length of the smallest binary sequence
//       |
//      10 (2) & 1(1) == 0
//      
// Because all  cells in even rows have a southeast neighbor, they can be connected.
//
//            if ((z & 1) == 0)
//            {
//                result.SetNeighborPair(HexDirection.SouthEast, result[i - _cellCountX]);
//
//                //All even cells except for the first cell in each row have a southwest neighbor.
//                if (x > 0)
//                {
//                    result.SetNeighborPair(HexDirection.SouthWest, result[i - _cellCountX - 1]);
//                }
//                else if (_wrapping)
//                {
//                    result.SetNeighborPair(HexDirection.SouthWest, result[i - 1]);
//                }
//            }
//            else
//            {
//                result.SetNeighborPair(HexDirection.SouthWest, result[i - _cellCountX]);
//
//                //All odd cells except the last cell in each row have a southeast neighbor
//                if (x < _cellCountX - 1)
//                {
//                    result.SetNeighborPair(HexDirection.SouthEast, result[i - _cellCountX + 1]);
//                }
//                else if (_wrapping)
//                {
//                    result.SetNeighborPair(HexDirection.SouthEast, result[i - _cellCountX * 2 + 1]);
//                }
//            }
//        }

// TODO: Presentation considerations should be moved to a separate class.
        Text label = Instantiate<Text>(_cellLabelPrefab);
        label.rectTransform.anchoredPosition =
            new Vector2(
                result.transform.localPosition.x,
                result.transform.localPosition.z
            );

        result.uiRect = label.rectTransform;
        result.SetElevation(0, cellOuterRadius);

        AddCellToChunk(x, z, result);
        
        return result;
    }

    private void AddCellToChunk(int x, int z, HexCell cell) {
        int chunkX = x / MeshConstants.ChunkSizeX;
        int chunkZ = z / MeshConstants.ChunkSizeZ;
        HexGridChunk chunk = _chunks[chunkX + chunkZ * _chunkWidth];

        int localX = x - chunkX * MeshConstants.ChunkSizeX;
        int localZ = z - chunkZ * MeshConstants.ChunkSizeZ;

        chunk.AddCell(localX + localZ * MeshConstants.ChunkSizeX, cell);
    }

    private void Awake() {
        HexagonPoint.InitializeHashGrid(_seed);
        _terrainMaterial = Resources.Load<Material>("Terrain");

// TODO: Is there a more transparent way to represent this dependency?
//       Right now it is buried in awake which makes it very hard to
//       tell that ShaderData depends on this class. Also, this
//       dependency is circular.
        _cellShaderData = gameObject.AddComponent<CellShaderData>();
        _cellShaderData.HexGrid = this;

// Rather leave null and know that it is uninitialized.
//        _cellMatrix = new DenseArray<HexCell>(0, 0);
        
        _chunks = new HexGridChunk[0];
    
// TODO: This is a presentation concern and should not be in this class.
        _cellLabelPrefab = Resources.Load<Text>("Hex Cell Label");
    }

    private void OnEnable() {
        HexagonPoint.InitializeHashGrid(_seed);
        HexagonPoint.wrapSize = _wrapping ? WidthInCells : 0;
        ResetVisibility();
    }
}

