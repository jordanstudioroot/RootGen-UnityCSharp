using RootCollections;
using RootLogging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Diagnostics;

public class HexMap : MonoBehaviour {

    #region Constant Fields
    #endregion

    #region Fields
    
    #region Private Fields

    private bool _editMode;
    private Text _hexLabelPrefab;
    private List<HexUnit> _units = new List<HexUnit>();

    //Init to -1 so new maps always get centered.
    /// <summary>
    /// The index of the MeshChunk column currently centered below the camera.
    /// </summary>
    private HexShaderData _hexShaderData;
    private Material _terrainMaterial;
    private bool _uiVisible;

    #endregion

    #endregion

    #region Finalizers (Destructors)
    #endregion

    #region Delegates
    #endregion

    #region Events
    #endregion

    #region Enums
    #endregion

    #region Interfaces
    #endregion

    #region Properties

    #region Public Properties

    /// <summary>
    /// Enables the edit mode for the hex map, disabling
    /// the fog of war shader and hiding the hex map ui.
    /// </summary>
    public bool EditMode {
        set {
            HexGridShaderKeywords.HexMapEditMode = value;
            ShowUIAllHexChunks(!value);
            _editMode = value;
        }

        get {
            return _editMode;
        }
    }

    #endregion

    #region Public Read Only Properties
    public List<Hex> LandHexes {
        get {
            IEnumerable<Hex> hexes = HexGrid.Hexes;
            List<Hex> result = new List<Hex>();
            foreach (Hex hex in hexes) {
                if (hex.elevation >= hex.WaterLevel) {
                    result.Add(hex);
                }
            }

            return result;
        }
    }

    public Transform[] HexMeshColumnTransforms {
        get; private set;
    }

    /// <summary>
    /// The hex mesh chunks contained in the hex map.
    /// </summary>
    /// <value></value>
    public MapMeshChunk[] HexMeshChunks {
        get; private set;
    }

    /// <summary>
    /// The number of columns of hex mesh chunks in the hex map.
    /// </summary>
    public int HexMeshChunkRows {
        get {
            return HexOffsetRows / HexMeshConstants.CHUNK_SIZE_X;
        }
    }

    /// <summary>
    /// The number of rows of hex mesh chunks in the hex map.
    /// </summary>
    public int HexMeshChunkColumns {
        get {
            return HexOffsetColumns / HexMeshConstants.CHUNK_SIZE_Z;
        }
    }

    /// <summary>
    /// The hex grid for the hex map. Contains positional data
    /// for each hex.   
    /// </summary>
    public HexGrid<Hex> HexGrid {
        get; private set;
    }

    /// <summary>
    /// The adjacency graph for the hex map. Contains all edges between
    /// adjacent hexes.
    /// </summary>
    public HexAdjacencyGraph AdjacencyGraph {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();
            return HexAdjacencyGraph.FromHexGrid(HexGrid);
        }
    }

    /// <summary>
    /// The river graph for the hex map. Contains bidirectional flow
    /// data for all rivers in the hex map.
    /// </summary>
    public RiverDigraph RiverDigraph {
        get; set;
    }

    /// <summary>
    /// The road graph for the hex map. Contains undirected flow data
    /// for all roads on the hex map.
    /// </summary>
    public RoadUndirectedGraph RoadUndirectedGraph {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();
            return RoadUndirectedGraph.FromHexGrid(HexGrid);
        }
    }

    /// <summary>
    /// The elevation graph for the hex map. Contains bidirectional flow
    /// data for all changes in elevation on the hex map.
    /// </summary>
    public ElevationDigraph CreateElevationDigraph {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();
            return ElevationDigraph.FromHexGrid(HexGrid);
        }
    }

    /// <summary>
    /// The number of rows in the hex map using an offset hex coordinate
    /// system.  
    /// </summary>
    /// <value></value>
    public int HexOffsetRows {
        get {
            return HexGrid.Rows;
        }
    }

    /// <summary>
    /// The number of columns in the hex map using an offset hex
    /// coordinate system.
    /// </summary>
    /// <value></value>
    public int HexOffsetColumns {
        get {
             return HexGrid.Columns;
        }
    }

    /// <summary>
    /// The hex tile at the absolute center of the hex grid.
    /// </summary>
    public Hex GridCenter {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();
            return HexGrid.Center;
        }
    }

    /// <summary>
    /// The index of the centermost column of hex mesh chunks.
    /// </summary>
    public int CenterHexMeshColumnIndex {
        get; private set;
    }

    /// <summary>
    /// The size of the hex map squared, using offset rows and columns.
    /// </summary>
    public int SizeSquared {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();
            return HexGrid.SizeSquared;
        }
    }

    /// <summary>
    /// Is the hex map wrapping?
    /// </summary>
    public bool IsWrapping {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();
            return HexGrid.IsWrapping;
        }
    }

    /// <summary>
    /// The wrap size of the hex map required to wrap coordinates around
    /// the X axis of an offset coordiante system.
    /// </summary>
    public int WrapSize {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();

            return HexGrid.WrapSize;
        }
    }

    /// <summary>
    /// An unordered collection of all hexes contained in the hex map.
    /// </summary>
    public IEnumerable<Hex> Hexes {
        get {
            if (HexGrid == null)
                throw new NullHexGridException();
            return HexGrid.Hexes;
        }
    }

    #endregion

    #region Public Write Only Properties
    
    /// <summary>
    /// Enables the hex grid overlay for the hex map.
    /// </summary>
    public bool ShowGrid {
        set {
            HexGridShaderKeywords.GridOn = value;
        }
    }

    #endregion

    #region Private Properties
    #endregion

    #endregion

    #region Indexers
    #endregion
    
    #region Methods

    #region Public Methods
    /// <summary>
    /// Initialize the hex map to an empty flat plain.
    /// </summary>
    /// <param name="bounds">
    ///     A bounds object representing the dimensions of the hex map. Will
    ///     be scaled to fit within a multiple of MeshConstants.ChunkSize.
    /// </param>
    /// <param name="wrapping">
    ///     Should the horizontal bounds of the grid wrap into their opposite
    ///     side?
    /// </param>
    /// <param name="editMode">
    ///     Should the map be editable immediately after being initialized?
    /// </param>
    /// <param name="hexOuterRadius">
    ///     The distance of each hex from its center to a circle 
    ///     intersecting each corner of the hexagon. Scales the size of all
    ///     other visual elements on the hex map.
    /// </param>
    /// <param name="seed">
    ///     The random seed used to initialize the hash grid for the map.
    /// </param>
    public HexMap Initialize(
        Rect bounds,
        int seed,
        float hexOuterRadius,
        bool wrapping,
        bool editMode
    ) {
        ClearHexUnits(_units);
        ClearColumnTransforms(HexMeshColumnTransforms);

        int columns, rows;

        if (
            bounds.x < 0 || bounds.y < 0 ||
            !bounds.size.IsFactorOf(HexMeshConstants.ChunkSize)
        ) {
            RootLog.Log(
                "Unsupported map size. Clamping dimensions to chunk size.",
                Severity.Warning,
                "HexMap"
            );

            Vector2 clamped =
                bounds.size.ClampToFactorOf(HexMeshConstants.ChunkSize);

            bounds = new Rect(
                0,
                0,
                clamped.x,
                clamped.y
            );
        }

        columns = (int)bounds.width;
        rows = (int)bounds.height;

        // Set to -1 so new maps always gets centered.
        CenterHexMeshColumnIndex = -1;

        HexagonPoint.InitializeHashGrid(seed);

        if (!_terrainMaterial)
            _terrainMaterial = Resources.Load<Material>("Terrain");

        HexGrid = new HexGrid<Hex>(
            rows,
            columns,
            wrapping
        );
        
        for (
            int index = 0, column = 0;
            column < HexOffsetColumns;
            column++
        ) {
            for (
                int row = 0;
                row < HexOffsetRows;
                row++
            ) {
                HexGrid[column, row] =
                    CreateHexFromOffsetCoordinates(
                        column, row, index++,
                        hexOuterRadius,
                        HexGrid
                    );
            }
        }

        HexMeshChunks = GetHexMeshChunks(
            HexGrid,
            hexOuterRadius
        );

        HexMeshColumnTransforms =
            GetHexMeshChunkColumns(HexMeshChunks);

        EditMode = editMode;

        return this;
    }

    /// <summary>
    /// Attempts to get the neighbors of the specified hex.
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="neighbors"></param>
    /// <returns></returns>
    public bool TryGetNeighbors(
        Hex hex,
        out List<Hex> neighbors
    ) {
        return HexGrid.TryGetNeighbors(hex, out neighbors);
    }

    public Hex GetHexAtIndex(int index) {
        return HexGrid.GetElement(index);
    }

    public Hex GetHexAtOffsetCoordinates(int x, int z) {
        return HexGrid.GetElement(x, z);
    }

    public static HexMap Empty(
        Rect bounds,
        int seed,
        float hexOuterRadius,
        bool wrapping,
        bool editMode
    ) {
        HexMap result = CreateHexMapGameObject();

        result.GetComponent<HexMap>().Initialize(
            bounds,
            seed,
            hexOuterRadius,
            wrapping,
            editMode
        );

        return result;
    }

    public void ClearColumnTransforms(Transform[] columns) {
         if (columns != null) {
            for (int i = 0; i < columns.Length; i++) {
                Destroy(columns[i].gameObject);
            }
        }
    }

    public Hex GetHex(
        Vector3 position,
        float hexOuterRadius
    ) {

        if (HexGrid == null)
            throw new System.NullReferenceException(
                "HexMap has not been initialized with a HexGrid."
            );

        position = transform.InverseTransformPoint(position);
        
        CubeVector coordinates =
            CubeVector.FromVector3(
                position,
                hexOuterRadius,
                HexGrid.WrapSize
            );
        
        return GetHex(coordinates);
    }

    public Hex GetHex(
        CubeVector coordinates
    ) {
        int z = coordinates.Z;

        // Check for array index out of bounds.
        if (z < 0 || z >= HexOffsetColumns) {
            return null;
        }

        int x = coordinates.X + z / 2;
        
        // Check for array index out of bounds.
        if (x < 0 || x >= HexOffsetColumns) {
            return null;
        }
        
        return HexGrid.GetElement(
            coordinates.X,
            coordinates.Y,
            coordinates.Z
        );
    }

    public Hex GetHex(
        Ray ray,
        float hexOuterRadius,
        int wrapSize
    ) {
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit)) {
            return GetHex(
                hit.point,
                hexOuterRadius
            );
        }

        return null;
    }

    public Hex GetHex(int xOffset, int zOffset) {
        return HexGrid.GetElement(xOffset, zOffset);
    }

    public Hex GetHex(int hexIndex) {
        return HexGrid.GetElement(hexIndex);
    }

    public IEnumerable<HexEdge> GetPath(
        Hex fromHex,
        Hex toHex,
        HexUnit hexUnit,
        HexAdjacencyGraph graph
    ) {
        return AStarSearch(fromHex, toHex, hexUnit, graph);

// Presentation concerns should not be in this method.        
//        SetPathDistanceLabelAndEnableHighlights(toHex, unit.Speed);
    }

    public void AddUnit(
        HexUnit hexUnit,
        Hex location,
        float orientation
    ) {
        _units.Add(hexUnit);
        
        hexUnit.Grid = this;
        hexUnit.Location = location;
        hexUnit.Orientation = orientation;
    }

    public void RemoveUnit(HexUnit unit) {
        _units.Remove(unit);
        unit.Die();
    }

// TODO: This is a presentation concern and should be moved out of
//       this class.
    public void IncreaseVisibility(
        Hex fromHex,
        int range
    ) {
        List<Hex> hexes =
        GetVisibleHexes(
            fromHex
//            range
        );

        for (int i = 0; i < hexes.Count; i++) {
            hexes[i].IncreaseVisibility();
        }

        ListPool<Hex>.Add(hexes);
    }

// TODO: This is a presentation concern and should be moved out of
//       this class.
    public void DecreaseVisibility(
        Hex fromHex,
        int range,
        ElevationDigraph elevationGraph
    ) {
        List<Hex> hexes =
            GetVisibleHexes(
                fromHex
//              range
            );

        for (int i = 0; i < hexes.Count; i++) {
            hexes[i].DecreaseVisibility();
        }

        ListPool<Hex>.Add(hexes);
    }

// TODO: This is a presentation concern and should be moved out of this
//       class
    public void ResetVisibility() {
        for (int i = 0; i < HexGrid.SizeSquared; i++) {
            HexGrid.GetElement(i).ResetVisibility();
        }

        for (int i = 0; i < _units.Count; i++) {
            HexUnit unit = _units[i];
            
            IncreaseVisibility(
                unit.Location,
                unit.VisionRange
            );
        }
    }


    public void CenterMap(
        float xPosition,
        float hexOuterRadius
    ) {
        float innerDiameter = 
            HexagonPoint.InnerDiameterFromOuterRadius(hexOuterRadius);
        // Get the column index which the x axis coordinate is over.
        int centerColumnIndex =
            (int) (xPosition / (innerDiameter * HexMeshConstants.CHUNK_SIZE_X));

        if (centerColumnIndex == CenterHexMeshColumnIndex) {
            return;
        }

        CenterHexMeshColumnIndex = centerColumnIndex;

        int minColumnIndex =
            centerColumnIndex - HexMeshChunkColumns / 2;
        
        int maxColumnIndex =
            centerColumnIndex + HexMeshChunkColumns / 2;

        Vector3 position = new Vector3(0, 0, 0);

        for (int i = 0; i < HexMeshColumnTransforms.Length; i++) {
            float posX =
                HexagonPoint.InnerDiameterFromOuterRadius(
                    hexOuterRadius
                ) * HexMeshConstants.CHUNK_SIZE_X;
            
            if (i < minColumnIndex) {
                position.x = HexMeshChunkColumns * posX;
            }
            else if (i > maxColumnIndex) {
                position.x = HexMeshChunkColumns * -posX;
            }
            else {
                position.x = 0f;
            }

            HexMeshColumnTransforms[i].localPosition = position;
        }
    }

    #endregion
    
    #region Public Static Methods

    /// <summary>
    ///     Creates an empty parentless GameObject and adds an uninitialized
    ///     HexMap component.
    /// </summary>
    /// <returns>
    ///     The unitialized HexMap component which has been added to the
    ///     GameObject.
    /// </returns>
    public static HexMap CreateHexMapGameObject() {
        GameObject resultObj = new GameObject("Hex Map");
        HexMap resultMono = resultObj.AddComponent<HexMap>();
        return resultMono;
    }
    
    #endregion

    #region Private Methods

    public void Draw(
        float hexOuterRadius
    ) {
        if (HexGrid == null)
            throw new NullHexGridException();

        _hexShaderData.Initialize(
            HexOffsetColumns,
            HexOffsetRows
        );

        foreach(Hex hex in HexGrid.Hexes) {
            _hexShaderData.RefreshTerrain(hex);
        }

        string diagnostic = "Mesh Triangulation Diagnostics\n\n";
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch currentStopwatch = new Stopwatch();
        stopwatch.Start();

        foreach(MapMeshChunk chunk in HexMeshChunks) {
            currentStopwatch.Restart();

            chunk.Triangulate(
                this,
                hexOuterRadius,
                AdjacencyGraph,
                RiverDigraph,
                RoadUndirectedGraph,
                CreateElevationDigraph            
            );

            currentStopwatch.Stop();
            diagnostic +=
                "Mesh chunk triangulated in: " +
                currentStopwatch.Elapsed + "\n";
        }

        stopwatch.Stop();
        diagnostic +=
            "All mesh chunks triangulated in: " + stopwatch.Elapsed;

        foreach(string toLog in SplitByChar(diagnostic, 30000)) {
            RootLog.Log(
                toLog,
                Severity.Information,
                "Diagnostic"
            );
        }
    }

    // TODO: Put this in a utility class or move it directly ito
    //       RootLogging.
    private List<string> SplitByChar(string toSplit, int charThreshold) {
        List<string> result = new List<string>();

        for (
            int i = 0, currLen = 1;
            i < toSplit.Length;
            i++, currLen++
        ) {
            if (currLen == charThreshold)
                result.Add(
                    toSplit.Substring(i, charThreshold)
                );
            else if (i == toSplit.Length - 1)
                result.Add(
                    toSplit.Substring(
                        i - (currLen - 1),
                        toSplit.Length - 1
                    )
                );
        }

        return result;
    }

    private MapMeshChunk[] GetHexMeshChunks(
        HexGrid<Hex> grid,
        float hexOuterRadius
    ) {
        MapMeshChunk[] result = new MapMeshChunk[
            HexMeshChunkRows * HexMeshChunkColumns
        ];

        for (int i = 0; i < result.Length; i++) {
            result[i] = MapMeshChunk.CreateEmpty(this.transform);
        }

        for (int hexRow = 0; hexRow < HexOffsetRows; hexRow++) {
            for (int hexColumn = 0; hexColumn < HexOffsetColumns; hexColumn++) {
                
                Hex hex = grid.GetElement(hexColumn, hexRow);

                int hexChunkColumn =
                    hexColumn / HexMeshConstants.CHUNK_SIZE_Z;
                
                int hexChunkRow =
                    hexRow / HexMeshConstants.CHUNK_SIZE_X;

                int hexLocalColumn =
                    hexColumn % HexMeshConstants.CHUNK_SIZE_X;

                int hexLocalRow =
                    hexRow % HexMeshConstants.CHUNK_SIZE_Z;

                result[
                    (hexChunkRow * HexMeshChunkColumns) +
                    hexChunkColumn
                ].AddHex(
                    (hexLocalRow * HexMeshConstants.CHUNK_SIZE_Z) +
                    hexLocalColumn,
                    hex
                );    
            }
        }

        return result;
    }

    private Transform[] GetHexMeshChunkColumns(
        MapMeshChunk[] chunks
    ) {
        Transform[] result = new Transform[HexMeshChunkColumns];

        for (int i = 0; i < result.Length; i++) {
            result[i] = new GameObject(
                "Map Mesh Chunk Column"
            ).transform;

            result[i].transform.SetParent(transform, false);
        }

        for (int row = 0; row < HexOffsetRows; row++) {
            for (int column = 0; column < HexOffsetColumns; column++) {
                int hexChunkRow =
                    row / HexMeshConstants.CHUNK_SIZE_X;
                int hexChunkColumn =
                    column / HexMeshConstants.CHUNK_SIZE_Z;

                chunks[(hexChunkRow * HexMeshChunkColumns) + hexChunkColumn]
                    .transform.SetParent(
                        result[hexChunkColumn]
                    );
            }
        }

        return result;
    }

    /// <summary>
    /// Switches the UI on and off for all HexGridChunks, enabling and
    /// disabling features such as the distance from the currently
    /// selected hex hex.
    /// </summary>
    /// <param name="visible">
    /// The visible state of all HexGridChunks.
    /// </param>
    private void ShowUIAllHexChunks(bool visible) {
        for (int i = 0; i < HexMeshChunks.Length; i++) {
            HexMeshChunks[i].ShowUI(visible);
        }
    }

    /// <summary>
    /// Destroy all HexUnits on this HexGrid.
    /// </summary>
    private void ClearHexUnits(List<HexUnit> units) {
        for (int i = 0; i < units.Count; i++) {
            units[i].Die();
        }

        units.Clear();
    }

    private double GetPathfindingEdgeWeight(HexEdge edge) {
        return 0d;
    }

    private double GetPathfindingHeursitic(Hex hex) {
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
        Hex start,
        Hex end,
        HexUnit unit,
        HexAdjacencyGraph graph
    ) {
        IEnumerable<HexEdge> result;
        
//        AlgorithmExtensions.ShortestPathsAStar<Hex, HexEdge>(
//            graph,
//            GetPathfindingEdgeWeight,
//            GetPathfindingHeursitic,
//            start
//        ).Invoke(end, out result);
        
        return null;

/*        int speed = unit.Speed;

        _searchFrontierPhase += 2;

        if (_searchFrontier == null)
        {
            _searchFrontier = new HexPriorityQueue();
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
            Hex current = _searchFrontier.Dequeue();
            current.SearchPhase += 1;

            if (current == end)
            {
                return true;
            }

            int currentTurn = (current.Distance - 1) / speed;

            for (HexDirection direction = HexDirection.Northeast; direction <= HexDirection.Northwest; direction++)
            {
                Hex neighbor = current.GetNeighbor(direction);

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

// Wasted movement points are factored into the cost of hexes outside
// the boundary of the first turn by adding the turn number multiplied
// by the speed plus the cost to move into the hex outside the boundary
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
    private List<Hex> GetVisibleHexes(
        Hex fromHex
//        int sightRange
    ) {
        ElevationDigraph elevationGraph = ElevationDigraph.FromHexGrid(
            HexGrid
        );

        List<Hex> visibleHexes = new List<Hex>();
        
        Queue<Hex> open = new Queue<Hex>();
        List<Hex> closed = new List<Hex>();

        Hex current = fromHex;
        open.Enqueue(current);
        visibleHexes.Add(current);
        
        List<HexEdge> visibleEdges = elevationGraph.GetVisibleEdges(
            current
        );

        while (visibleEdges.Count > 0) {
            foreach(HexEdge edge in visibleEdges) {
                if (!closed.Contains(edge.Target)) {
                    open.Enqueue(edge.Target);
                    visibleHexes.Add(edge.Target);
                    closed.Add(current);
                }
            }

            current = open.Dequeue();
        }

        return visibleHexes;

// USE OUT EDGES OF ADJACENCY GRAPH INSTEAD
// This method represents returning a breadth first list of the graph
// which terminates when an edge is encountered where the out edge
// target is higher in elevation than the out edge source.
/*        Queue<ElevationEdge> edgeQueue =
            (Queue<ElevationEdge>)graph.OutEdges(fromHex);

        List<Hex> result = new List<Hex>();
        result.Add(fromHex);

        while (edgeQueue.Count > 0) {
            ElevationEdge current = edgeQueue.Dequeue();

            if (
                current.Delta <= 0
            ) {
                result.Add(current.Target);
                
                foreach (
                    ElevationEdge edge in
                    (List<HexEdge>)graph.OutEdges(current.Target)
                ) {
                   edgeQueue.Enqueue(edge); 
                }
            }
        }

        return result;
*/
/*       
        List<Hex> visibleHexes = ListPool<Hex>.Get();

        _searchFrontierPhase += 2;

        if (_searchFrontier == null)
        {
            _searchFrontier = new HexPriorityQueue();
        }
        else
        {
            _searchFrontier.Clear();
        }

        sightRange += fromHex.ViewElevation;

// Temporarily using a list instead of a priority queue.
// Should optimize this later.
//
        fromHex.SearchPhase = _searchFrontierPhase;
        fromHex.Distance = 0;
        _searchFrontier.Enqueue(fromHex);

        HexCoordinates fromCoordinates = fromHex.Coordinates;

        while (_searchFrontier.Count > 0)
        {
            Hex current = _searchFrontier.Dequeue();
            current.SearchPhase += 1;

            visibleHexes.Add(current);

            for (HexDirection direction = HexDirection.Northeast; direction <= HexDirection.Northwest; direction++)
            {
                Hex neighbor = current.GetNeighbor(direction);

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

        return visibleHexes;
        */
    }

    private Vector3 CoordinateToLocalPosition(
        int x,
        int z,
        float innerDiameter,
        float hexOuterRadius
    ) {
        return new Vector3(
// The distance between the center of two hexagons on the x axis is equal to
// twice the inner radius of a given hexagon. Additionally, for half of the
// hexes, the position on the z axis (cartesian y axis) is added to its position
// on the x axis as an offset, and the integer division of the position of
// the hexes on the z axis is subtracted from that value. For even rows this
// negates the offset. For odd rows, the integer is rounded down and the offset
// is retained.
            (x + z * 0.5f - z / 2) * innerDiameter,
            0,
// The distance between the center of two hexagons on the z axis (cartesian y axis) is equal to
// one and one half the outer radius of a given hexagon.
            z * (hexOuterRadius * 1.5f)
        );
    }


/// <summary>
/// Create a hex representing the data 
/// </summary>
/// <param name="offsetX">
/// An x coordinate in the offset coordinate system.
/// </param>
/// <param name="offsetZ">
/// A z coordiante in the offset coordinate system.
/// </param>
/// <param name="rowMajorIndex">
/// The row-major index of the hex.
/// </param>
/// <param name="hexOuterRadius">
/// The outer radius of the hex.
/// </param>
/// <param name="hexGrid">
/// The hex grid to the hex to as an element.
/// </param>
/// <returns>
/// A hex, instantiated at world space coordinates cooresponding
/// to offsetX and offsetZ and assigned to the specified hex grid at
/// offsetX and offsetZ.
/// </returns>    
    private Hex CreateHexFromOffsetCoordinates(
        int offsetX,
        int offsetZ,
        int rowMajorIndex,
        float hexOuterRadius,
        HexGrid<Hex> hexGrid
    ) {
// metrics.
        float innerDiameter =
            HexagonPoint.OuterToInnerRadius(hexOuterRadius) * 2f;

// Create the Hexes object and monobehaviour.
        Hex result = Hex.Instantiate(offsetX, offsetZ, WrapSize);

// Set the Hexes transform.
        result.transform.localPosition = CoordinateToLocalPosition(
            offsetX,
            offsetZ,
            innerDiameter,
            hexOuterRadius
        );

        result.name = "Hex " + result.CubeCoordinates;

        result.Index = rowMajorIndex;
        result.ColumnIndex = offsetX / HexMeshConstants.CHUNK_SIZE_X;
        result.ShaderData = _hexShaderData;

// If wrapping is enabled, hex is not explorable if the hex is on the
// top or bottom border.
        if (IsWrapping) {
            result.IsExplorable = offsetZ > 0 && offsetZ < HexOffsetColumns - 1;
        }
// If wrapping is disabled, hex is not explorable if the hex is on
// any border.
        else {
            result.IsExplorable =
                offsetX > 0 &&
                offsetZ > 0 &&
                offsetX < HexOffsetColumns - 1 &&
                offsetZ < HexOffsetRows - 1;
        }

// THIS IS NOW HANDLED BY MAPPING THE DENSEARRAY TO AN ADJACENCY GRAP
// 
// At the beginning of each row, x == 0. Therefore, if x is greater than
// 0, set the east/west connection of the hex between the current hex
// and the previous hex in the array.
//        if (x > 0) {
//            result.SetNeighborPair(HexDirection.West, result[i - 1]);
//
//            if (_wrapping && x == _hexCountX - 1) {
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
// Because all hexes in even rows have a southeast neighbor, they can be connected.
//
//            if ((z & 1) == 0)
//            {
//                result.SetNeighborPair(HexDirection.SouthEast, result[i - _hexCountX]);
//
// All even hexess except for the first hex in each row have a southwest neighbor.
//                if (x > 0)
//                {
//                    result.SetNeighborPair(HexDirection.SouthWest, result[i - _hexCountX - 1]);
//                }
//                else if (_wrapping)
//                {
//                    result.SetNeighborPair(HexDireFtion.SouthWest, result[i - 1]);
//                }
//            }
//            else
//            {
//                result.SetNeighborPair(HexDirection.SouthWest, result[i - _hexCountX]);
//
//                //All odd hexess except the last hex in each row have a southeast neighbor
//                if (x < _hexCountX - 1)
//                {
//                    result.SetNeighborPair(HexDirection.SouthEast, result[i - _hexCountX + 1]);
//                }
//                else if (_wrapping)
//                {
//                    result.SetNeighborPair(HexDirection.SouthEast, result[i - _hexCountX * 2 + 1]);
//                }
//            }
//        }

// TODO: Presentation considerations should be moved to a separate class.
        Text label = new GameObject().AddComponent<Text>();
        
        label.rectTransform.anchoredPosition =
            new Vector2(
                result.transform.localPosition.x,
                result.transform.localPosition.z
            );

        result.uiRect = label.rectTransform;
        result.SetElevation(
            0,
            hexOuterRadius,
            hexGrid.WrapSize
        );
        
        return result;
    }

    private void Awake() {
//        ResetVisibility();

        // TODO: Is there a more transparent way to represent this dependency?
//       Right now it is buried in awake which makes it very hard to
//       tell that ShaderData depends on this class. Also, this
//       dependency is circular.
        _hexShaderData = gameObject.AddComponent<HexShaderData>();
        _hexShaderData.HexMap = this;
    
// TODO: This is a presentation concern and should not be in this class.
        _hexLabelPrefab = Resources.Load<Text>("Hex Label");
    }

    #endregion
    
    #endregion

    #region Structs
    #endregion
    
    #region Classes
    #endregion
}

