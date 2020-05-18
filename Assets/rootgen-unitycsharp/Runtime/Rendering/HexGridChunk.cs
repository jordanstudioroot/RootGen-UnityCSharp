using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RootLogging;

public class HexGridChunk : MonoBehaviour {
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public
    public HexMesh terrain;
    public HexMesh rivers;
    public HexMesh roads;
    public HexMesh water;
    public HexMesh waterShore;
    public HexMesh estuaries;
    public FeatureContainer features;

// ~~ private
/// <summary>
/// Splat map vector representing an entirely red channel.
/// </summary>
/// <returns></returns>
    private static Color _weights1 = new Color(1f, 0f, 0f);
/// <summary>
///  Splat map vector representing an entirely green channel.
/// </summary>
/// <returns></returns>
    private static Color _weights2 = new Color(0f, 1f, 0f);
/// <summary>
/// Splat map vector representing an entirely blue channel.
/// </summary>
/// <returns></returns>
    private static Color _weights3 = new Color(0f, 0f, 1f);
    private HexCell[] _cells;
    private Canvas _canvas;

// TODO: Temporarily reusing this variable as this value
//       must be present in LateUpdate. Should refactor class
//       so that this is not necessary.
    private float _cellOuterRadius;
    
    private NeighborGraph _graph;

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
    public HexCell[] Cells {
        set {
            _cells = value;
        }
    }

    public Canvas Canvas {
        set {
            _canvas = value;
        }
    }

// ~~ private
    private void LateUpdate() { }

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
// TODO: This method is a flimsy form of dependency injection and a bad
//       fake constructor. Should refactor HexGridChunk class so that this
//       approach is not necessary.
    public static HexGridChunk CreateAndRenderChunk(
        float cellOuterRadius,
        HexGrid<HexCell> hexGrid,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        RoadGraph roadGraph,
        ElevationGraph elevationGraph
    ) {
        GameObject resultObj = new GameObject("Hex Grid Chunk");
        HexGridChunk resultMono = resultObj.AddComponent<HexGridChunk>();

// TODO: Why was this here?      
//        resultMono.Initialize(
//            cellOuterRadius,
//            neighborGraph
//        );
        
        resultMono.Cells = new HexCell[
            MeshConstants.ChunkSizeX *
            MeshConstants.ChunkSizeZ
        ];
        
        GameObject resultCanvasObj = new GameObject(
            "Hex Grid Chunk Canvas"
        );
        
        Canvas resultCanvasMono = resultCanvasObj.AddComponent<Canvas>();

        CanvasScaler resultCanvasScalerMono =
            resultCanvasObj.AddComponent<CanvasScaler>();

        resultCanvasObj.transform.SetParent(resultObj.transform, false);
        resultMono.Canvas = resultCanvasMono;
        resultCanvasScalerMono.dynamicPixelsPerUnit = 10f;
        resultCanvasObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        resultCanvasObj.transform.position += Vector3.up * .005f;

        resultMono.terrain = HexMesh.GetMesh(
            Resources.Load<Material>("Terrain"), true, true, false, false
        );
        resultMono.terrain.name = "Terrain";
        resultMono.terrain.transform.SetParent(resultObj.transform, false);
        
        resultMono.rivers = HexMesh.GetMesh(
            Resources.Load<Material>("River"), false, true, true, false
        );
        resultMono.rivers.name = "Rivers";
        resultMono.rivers.transform.SetParent(resultObj.transform, false);

        resultMono.roads = HexMesh.GetMesh(
            Resources.Load<Material>("Road"), false, true, true, false
        );
        resultMono.roads.name = "Roads";
        resultMono.roads.transform.SetParent(resultObj.transform, false);

        resultMono.water = HexMesh.GetMesh(
            Resources.Load<Material>("Water"), false, true, false, false
        );
        resultMono.water.name = "Water";
        resultMono.water.transform.SetParent(resultObj.transform, false);

        resultMono.waterShore = HexMesh.GetMesh(
            Resources.Load<Material>("WaterShore"), false, true, true, false
        );
        resultMono.waterShore.name = "Water Shore";
        resultMono.waterShore.transform.SetParent(resultObj.transform, false);

        resultMono.estuaries = HexMesh.GetMesh(
            Resources.Load<Material>("Estuary"), false, true, true, true
        );
        resultMono.estuaries.name = "Estuaries";
        resultMono.estuaries.transform.SetParent(resultObj.transform, false);

        HexMesh walls = HexMesh.GetMesh(
            Resources.Load<Material>("Urban"), false, false, false, false
        );
        walls.transform.SetParent(resultObj.transform, false);
        walls.name = "Walls";

        resultMono.features = FeatureContainer.GetFeatureContainer(walls);
        resultMono.features.transform.SetParent(resultObj.transform, false);
        resultMono.features.name = "Features";

        resultMono.Triangulate(
            hexGrid,
            cellOuterRadius,
            neighborGraph,
            riverGraph,
            roadGraph,
            elevationGraph
        );

        resultMono.enabled = false;

        return resultMono;
    }

    public static HexGridChunk CreateChunk() {
        GameObject resultObj = new GameObject("Hex Grid Chunk");
        HexGridChunk resultMono = resultObj.AddComponent<HexGridChunk>();
        
        resultMono.Cells = new HexCell[
            MeshConstants.ChunkSizeX *
            MeshConstants.ChunkSizeZ
        ];
        
        GameObject resultCanvasObj = new GameObject(
            "Hex Grid Chunk Canvas"
        );
        
        Canvas resultCanvasMono = resultCanvasObj.AddComponent<Canvas>();

        CanvasScaler resultCanvasScalerMono =
            resultCanvasObj.AddComponent<CanvasScaler>();

        resultCanvasObj.transform.SetParent(resultObj.transform, false);
        resultMono.Canvas = resultCanvasMono;
        resultCanvasScalerMono.dynamicPixelsPerUnit = 10f;
        resultCanvasObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        resultCanvasObj.transform.position += Vector3.up * .005f;

        resultMono.terrain = HexMesh.GetMesh(
            Resources.Load<Material>("Terrain"), true, true, false, false
        );
        resultMono.terrain.name = "Terrain";
        resultMono.terrain.transform.SetParent(resultObj.transform, false);
        
        resultMono.rivers = HexMesh.GetMesh(
            Resources.Load<Material>("River"), false, true, true, false
        );
        resultMono.rivers.name = "Rivers";
        resultMono.rivers.transform.SetParent(resultObj.transform, false);

        resultMono.roads = HexMesh.GetMesh(
            Resources.Load<Material>("Road"), false, true, true, false
        );
        resultMono.roads.name = "Roads";
        resultMono.roads.transform.SetParent(resultObj.transform, false);

        resultMono.water = HexMesh.GetMesh(
            Resources.Load<Material>("Water"), false, true, false, false
        );
        resultMono.water.name = "Water";
        resultMono.water.transform.SetParent(resultObj.transform, false);

        resultMono.waterShore = HexMesh.GetMesh(
            Resources.Load<Material>("WaterShore"), false, true, true, false
        );
        resultMono.waterShore.name = "Water Shore";
        resultMono.waterShore.transform.SetParent(resultObj.transform, false);

        resultMono.estuaries = HexMesh.GetMesh(
            Resources.Load<Material>("Estuary"), false, true, true, true
        );
        resultMono.estuaries.name = "Estuaries";
        resultMono.estuaries.transform.SetParent(resultObj.transform, false);

        HexMesh walls = HexMesh.GetMesh(
            Resources.Load<Material>("Urban"), false, false, false, false
        );
        walls.transform.SetParent(resultObj.transform, false);
        walls.name = "Walls";

        resultMono.features = FeatureContainer.GetFeatureContainer(walls);
        resultMono.features.transform.SetParent(resultObj.transform, false);
        resultMono.features.name = "Features";

        return resultMono;
    }

// ~~ private

// ~ Non-Static

// ~~ public
    public void Refresh() {
        enabled = true;
    }

    public void AddCell(int index, HexCell cell) {
        _cells[index] = cell;
        cell.chunk = this;

/* Set WorldPositionStays to false for both the cells transform
* and ui rect or they will not move initally to be oriented with
* the chunk.
*/
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(_canvas.transform, false);
    }

/// <summary>
///     Switches the UI on and off for this chunk, enabling and disabling
///     features such as the distance from the currently selected hex cell.
/// </summary>
/// <param name="visible">
///     The visible state of the hex grid chunk.
/// </param>
    public void ShowUI(bool visible) {
        _canvas.gameObject.SetActive(visible);
    }

    public void Triangulate(
        HexGrid<HexCell> grid,
        float cellOuterRadius,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        RoadGraph roadGraph,
        ElevationGraph elevationGraph
    ) {
        RootLog.Log(
            "Rendering hex grid: " + "\n" + grid + "\n" + neighborGraph,
            Severity.Information,
            "NeighborGraph.FromHexGrid"
        );
        terrain.Clear();
        rivers.Clear();
        roads.Clear();
        water.Clear();
        waterShore.Clear();
        estuaries.Clear();
        features.Clear();

        for (int i = 0; i < grid.Size; i++) {
            TriangulateCell(
                grid.GetElement(i),
                cellOuterRadius,
                neighborGraph,
                riverGraph,
                roadGraph,
                elevationGraph,
                grid.WrapSize
            );
        }

        terrain.Apply();
        rivers.Apply();
        roads.Apply();
        water.Apply();
        waterShore.Apply();
        estuaries.Apply();
        features.Apply();
    }

// ~~ private
    private void TriangulateCell(
        HexCell cell,
        float cellOuterRadius,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        RoadGraph roadGraph,
        ElevationGraph elevationGraph,
        int wrapSize
    ) {
        _cellOuterRadius = cellOuterRadius;

        List<HexEdge> edges =
            neighborGraph.CellEdges(cell);

// USING GRAPH EDGES INSTEAD OF HEX DIRECTIONS
//
//        List<Vector3> result = new List<Vector3>();
//        for (
//            HexDirection direction = HexDirection.Northeast;
//            direction <= HexDirection.Northwest;
//            direction++
//        ) {
            
// Draw and color the triangles of the cells mesh.
//            TriangulateDirection(
//                direction,
//                cell,
//                cellOuterRadius,
//                graph
//            );
//        }

        

        foreach(HexEdge neighborEdge in edges) {
            TriangulateEdge(
                cell,
                cellOuterRadius,
                neighborEdge,
                neighborGraph,
                riverGraph,
                roadGraph,
                elevationGraph,
                wrapSize
            );
        }

        if (!cell.IsUnderwater) {
//            if (!cell.HasRiver && !cell.HasRoads) {
            if (
                    !riverGraph.HasRiver(cell) &&
                    !roadGraph.HasRoad(cell)
            ) {
                features.AddFeature(
                    cell,
                    cell.Position,
                    cellOuterRadius,
                    wrapSize
                );
            }

            if (cell.IsSpecial) {
                features.AddSpecialFeature(
                    cell,
                    cell.Position,
                    cellOuterRadius,
                    wrapSize
                );
            }
        }
    }

    private void TriangulateEdge(
        HexCell cell,
        float cellOuterRadius,
        HexEdge neighborEdge,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        RoadGraph roadGraph,
        ElevationGraph elevationGraph,
        int wrapSize
    ) {
// Cell center
        Vector3 center = cell.Position;

// Triangle edge.
        EdgeVertices edgeVertices = new EdgeVertices(
            center + HexagonPoint.GetFirstSolidCorner(
                neighborEdge.Direction,
                cellOuterRadius
            ),
            center +
            HexagonPoint.GetSecondSolidCorner(
                neighborEdge.Direction,
                cellOuterRadius
            )
        );

//        if (cell.HasRiver) {
        if (riverGraph.HasRiver(cell)) {

// SHOULD ITERATE OVER EACH EDGE FOR THE FOLLOWING ALGORITHM
//            if (cell.HasRiverThroughEdge(direction)) {
            if (
                riverGraph.HasRiverInDirection(
                    cell,
                    neighborEdge.Direction
                )
            ) {
/* If the triangle has a river through the edge, lower center edge vertex
*  to simulate stream bed.
*/
                edgeVertices.vertex3.y = cell.StreamBedY;

//                if (cell.HasRiverBeginOrEnd) {
                if (riverGraph.HasRiverEnd(cell)) {
                    TriangulateWithRiverBeginOrEnd(
                        cell,
                        center,
                        edgeVertices,
                        neighborEdge,
                        riverGraph,
                        cellOuterRadius,
                        wrapSize
                    );
                }
                else {
                    TriangulateWithRiver(
                        cell,
                        center,
                        edgeVertices,
                        neighborEdge,
                        riverGraph,
                        cellOuterRadius,
                        wrapSize
                    );
                }
            }
            else {
                TriangulateAdjacentToRiver(
                    cell,
                    center,
                    edgeVertices,
                    neighborEdge,
                    roadGraph,
                    riverGraph,
                    cellOuterRadius,
                    wrapSize
                );
            }
        }
        else {
            TriangulateWithoutRiver(
                cell,
                neighborEdge,
                edgeVertices,
                neighborEdge,
                riverGraph,
                roadGraph,
                center,
                cellOuterRadius,
                wrapSize
            );

            if (
                !cell.IsUnderwater &&
// SHOULD ITERATE THOURGH EACH EDGE FOR THIS ALGORITHM
//                !cell.HasRoadThroughEdge(direction)
                  !roadGraph.HasRoad(cell)
            ) {
                features.AddFeature(
                    cell,
                    (center + edgeVertices.vertex1 + edgeVertices.vertex5) * (1f / 3f),
                    cellOuterRadius,
                    wrapSize
                );
            }
        }

/* If the direction of triangulation is between NE and SE, triangulate
*  the connection between cells. Otherwise, do not triangulate the connection.
*  Since the connections are triangulated from west to east, south to north,
*  the connection will already have been triangulated for SW, W, an NW.
*/
        if (neighborEdge.Direction <= HexDirections.Southeast) {
            TriangulateConnection(
                cell,
                neighborEdge,
                neighborGraph,
                riverGraph,
                roadGraph,
                elevationGraph,
                edgeVertices,
                cellOuterRadius,
                wrapSize
            );
        }

        if (cell.IsUnderwater) {
            TriangulateWater(
                cell,
                neighborEdge,
                neighborGraph,
                riverGraph,
                center,
                cellOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateWithoutRiver(
        HexCell cell,
        HexEdge neighborEdge,
        EdgeVertices edgeVertices,
        HexEdge hexEdge,
        RiverGraph riverGraph,
        RoadGraph roadGraph,
        Vector3 center,
        float cellOuterRadius,
        int wrapSize
    ) {
        TriangulateEdgeFan(
            center,
            edgeVertices,
            cell.Index,
            cellOuterRadius,
            wrapSize
        );

//        if (cell.HasRoads) {
          if (roadGraph.HasRoad(cell)) {
            Vector2 interpolators = GetRoadInterpolators(
                cell,
                neighborEdge,
                roadGraph
            );

            TriangulateRoad(
                center,
                Vector3.Lerp(
                    center,
                    edgeVertices.vertex1,
                    interpolators.x
                ),
                Vector3.Lerp(
                    center,
                    edgeVertices.vertex5,
                    interpolators.y
                ),
                edgeVertices,
//                cell.HasRoadThroughEdge(direction),
                roadGraph.HasRoad(cell),
                cell.Index,
                cellOuterRadius,
                wrapSize
            );
        }
    }
    

    private void TriangulateWithRiver(
        HexCell cell,
        Vector3 center,
        EdgeVertices edgeVertices,
        HexEdge neighborEdge,
        RiverGraph riverGraph,
        float cellOuterRadius,
        int wrapSize
    ) {
        Vector3 centerLeft;
        Vector3 centerRight;

//        if (cell.HasRiverThroughEdge(direction.Opposite())) {
        if (
            riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction.Opposite()
            )
        ) {
/* Create a vertex 1/4th of the way from the center of the cell 
* to first solid corner of the previous edge, which is pointing
* straight "down" toward the bottom of the hexagon for a left facing
* edge.
*/
            centerLeft = center +
                HexagonPoint.GetFirstSolidCorner(
                    neighborEdge.Direction.PreviousClockwise(),
                    cellOuterRadius
                ) * 0.25f;

/* Create a vertex 1/4th of the way from the center of the cell
* to the second solid corner of the next edge, which is pointing
* straight "up" toward the top of the hexagon for a left facing edge.
*/
            centerRight = center +
                HexagonPoint.GetSecondSolidCorner(
                    neighborEdge.Direction.NextClockwise(),
                    cellOuterRadius
                ) * 0.25f;
        }

/* If the next direction has a sharp turn, there will be a river through
* direction.Next() or direction.Previous(). Must align center line with
* center line with edge between this river and the adjacent river.
* Interpolate with an increased step to account for the rotation
* of the center line.
*/
//        else if (cell.HasRiverThroughEdge(direction.Next())) {
        else if (
            riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction.NextClockwise()
            )
        ) {
            centerLeft = center;
            centerRight = 
                Vector3.Lerp(center, edgeVertices.vertex5, 2f / 3f);
        }
//        else if (cell.HasRiverThroughEdge(direction.Previous())) {
        else if (
            riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction.PreviousClockwise()
            )
        ) {
            centerLeft =
                Vector3.Lerp(center, edgeVertices.vertex1, 2f / 3f);
            centerRight = center;
        }

/* If the cell has a river two directions next, or two directions
* previous, there is a slight bend in the river. Need to push
* the center line to the inside of the bend. Using
* HexMetrics.innerToOuter to adjust for the fact that
* the midpoint of a solid edge is closer to the center
* of a cell than a solid edge corner.
*/
//        else if (cell.HasRiverThroughEdge(direction.Next2())) {
        else if (
            riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction.NextClockwise2()
            )
        ) {
            centerLeft = center;

            centerRight = 
                center + 
                HexagonPoint.GetSolidEdgeMiddle(
                    neighborEdge.Direction.NextClockwise(),
                    cellOuterRadius
                ) * (0.5f * HexagonConstants.INNER_TO_OUTER_RATIO);
        }
// Previous 2
        else {
            centerLeft = 
                center + 
                HexagonPoint.GetSolidEdgeMiddle(
                    neighborEdge.Direction.PreviousClockwise(),
                    cellOuterRadius
                ) * (0.5f * HexagonConstants.INNER_TO_OUTER_RATIO);

            centerRight = center;
        }

/* Get the final location of the center by averaging
* centerLeft and centerRight. For a straight through
* river this average is the same as the center
* of the cell. For a bend this moves the center
* appropriately. Otherwise, all points are the same
* and the center also remains at the center of the cell.
*/
        center = Vector3.Lerp(centerLeft, centerRight, 0.5f);

/* Create the middle edge vertices using points halfway between
* centerLeft/centerRight and the 1st and 5th vertices of the
* hexagons edge vertices for the given direction. Must use an
* alternate constructor for the middle edge vertices object
* because the length of the edge is 3/4ths rather than 1. To
* keep the 2nd and 4th vertex in line with the rivers edges,
* must interpolate by 1/6th instead of 1/3rd.
*/
        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(centerLeft, edgeVertices.vertex1, 0.5f),
            Vector3.Lerp(centerRight, edgeVertices.vertex5, 0.5f),
            1f / 6f
        );

/* Adjust the height of middle of the middle edge,
* as well as the height of the center of the hexagon, to 
* the height of the middle of the outer edge of the 
* hexagon. The given edge of the hexagon has already 
* been adjusted to the height of the river bed.
*/
        middle.vertex3.y = center.y = edgeVertices.vertex3.y;

// Create an edge strip between the middle and the given edge.
        TriangulateEdgeStrip(
            middle,
            _weights1,
            cell.Index,
            edgeVertices,
            _weights1,
            cell.Index,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            centerLeft,
            middle.vertex1,
            middle.vertex2,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            centerLeft,
            center,
            middle.vertex2,
            middle.vertex3,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            center,
            centerRight,
            middle.vertex3,
            middle.vertex4,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            centerRight,
            middle.vertex4,
            middle.vertex5,
            cellOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;
        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddQuadCellData(indices, _weights1);
        terrain.AddQuadCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);

        if (!cell.IsUnderwater) {
//            bool reversed = (cell.IncomingRiver == direction);
            bool reversed = riverGraph.HasIncomingRiverInDirection(
                cell,
                neighborEdge.Direction
            );

            TriangulateRiverQuad(
                centerLeft,
                centerRight,
                middle.
                vertex2,
                middle.vertex4,
                cell.RiverSurfaceY,
                0.4f,
                reversed,
                indices,
                cellOuterRadius,
                wrapSize
            );

            TriangulateRiverQuad(
                middle.vertex2,
                middle.vertex4,
                edgeVertices.vertex2,
                edgeVertices.vertex4,
                cell.RiverSurfaceY,
                0.6f,
                reversed,
                indices,
                cellOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateWithRiverBeginOrEnd(
        HexCell cell,
        Vector3 center,
        EdgeVertices edgeVertices,
        HexEdge neighborEdge,
        RiverGraph riverGraph,
        float cellOuterRadius,
        int wrapSize
    ) {
        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(center, edgeVertices.vertex1, 0.5f),
            Vector3.Lerp(center, edgeVertices.vertex5, 0.5f)
        );

        middle.vertex3.y = edgeVertices.vertex3.y;

        TriangulateEdgeStrip(
            middle,
            _weights1,
            cell.Index,
            edgeVertices,
            _weights1,
            cell.Index,
            cellOuterRadius,
            wrapSize
        );

        TriangulateEdgeFan(
            center,
            middle,
            cell.Index,
            cellOuterRadius,
            wrapSize
        );

        if (!cell.IsUnderwater) {
//            bool reversed = cell.HasIncomingRiver;
            bool reversed = riverGraph.HasIncomingRiverInDirection(
                cell,
                neighborEdge.Direction
            );

            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            
            TriangulateRiverQuad(
                middle.vertex2,
                middle.vertex4,
                edgeVertices.vertex2,
                edgeVertices.vertex4,
                cell.RiverSurfaceY,
                0.6f,
                reversed,
                indices,
                cellOuterRadius,
                wrapSize
            );

            center.y =
                middle.vertex2.y = middle.vertex4.y = cell.RiverSurfaceY;

            rivers.AddTriangle(
                center,
                middle.vertex2,
                middle.vertex4,
                cellOuterRadius,
                wrapSize
            );

            if (reversed) {
                rivers.AddTriangleUV(
                    new Vector2(0.5f, 0.4f),
                    new Vector2(1f, 0.2f),
                    new Vector2(0f, 0.2f)
                );
            }
            else {
                rivers.AddTriangleUV(
                    new Vector2(0.5f, 0.4f),
                    new Vector2(0f, 0.6f),
                    new Vector2(1f, 0.6f)
                );
            }

            rivers.AddTriangleCellData(indices, _weights1);
        }
    }

    private void TriangulateAdjacentToRiver(
        HexCell cell,
        Vector3 center,
        EdgeVertices edgeVertices,
        HexEdge neighborEdge,
        RoadGraph roadGraph,
        RiverGraph riverGraph,
        float cellOuterRadius,
        int wrapSize
    ) {
//        if (cell.HasRoads) {
        if (roadGraph.HasRoad(cell)) {
            TriangulateRoadAdjacentToRiver(
                cell,
                center,
                neighborEdge,
                riverGraph,
                roadGraph,
                edgeVertices,
                cellOuterRadius,
                wrapSize
            );
        }

//        if (cell.HasRiverThroughEdge(direction.Next())) {
        if (riverGraph.HasRiverInDirection(
            cell,
            neighborEdge.Direction
        )) {
/* If the direction has a river on either side, it has a slight curve. 
* The center vertex of river-adjacent triangle needs to be moved toward 
* the edge so they don't overlap the river.
*/
//            if (cell.HasRiverThroughEdge(direction.Previous())) {
            if (riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction.PreviousClockwise()
            )) {
                center += HexagonPoint.GetSolidEdgeMiddle(
                    neighborEdge.Direction,
                    cellOuterRadius
                ) * (HexagonConstants.INNER_TO_OUTER_RATIO * 0.5f);
            }

/* If the cell has a river through the previous previous direction,
* it has a river flowing through the cell. Move the center vertex
* of the river-adjacent triangle so that it does not overlap the river.
*/
            else if (
//                cell.HasRiverThroughEdge(direction.Previous2())
                riverGraph.HasRiverInDirection(
                    cell,
                    neighborEdge.Direction.PreviousClockwise2()
                )
            ) {
                center +=
                    HexagonPoint.GetFirstSolidCorner(
                        neighborEdge.Direction,
                        cellOuterRadius
                    ) * 0.25f;
            }
        }

/* Second case of straight-river-adjacent triangle. Need to move center
* so it doesn't overlap the river.
*/
        else if (
//            cell.HasRiverThroughEdge(direction.Previous()) &&
            riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction.PreviousClockwise()
            ) &&
//            cell.HasRiverThroughEdge(direction.Next2())
            riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction.NextClockwise2()
            )
        ) {
            center += HexagonPoint.GetSecondSolidCorner(
                neighborEdge.Direction,
                cellOuterRadius
            ) * 0.25f;
        }

        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(center, edgeVertices.vertex1, 0.5f),
            Vector3.Lerp(center, edgeVertices.vertex5, 0.5f)
        );

        TriangulateEdgeStrip(
            middle,
            _weights1,
            cell.Index,
            edgeVertices,
            _weights1,
            cell.Index,
            cellOuterRadius,
            wrapSize
        );

        TriangulateEdgeFan(
            center,
            middle,
            cell.Index,
            cellOuterRadius,
            wrapSize
        );

        if (
            !cell.IsUnderwater &&
//            !cell.HasRoadThroughEdge(direction)
            roadGraph.HasRoadInDirection(
                cell,
                neighborEdge.Direction
            )
        ) {
            features.AddFeature(
                cell,
                (center + edgeVertices.vertex1 + edgeVertices.vertex5) * (1f / 3f),
                cellOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateConnection(
        HexCell cell,
        HexEdge neighborEdge,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        RoadGraph roadGraph,
        ElevationGraph elevationGraph,
        EdgeVertices edgeVertices1,
        float cellOuterRadius,
        int wrapSize
    ) {
//        HexCell neighbor = cell.GetNeighbor(direction);
        HexCell neighbor = neighborGraph.TryGetNeighborInDirection(
            cell,
            neighborEdge.Direction
        );

/* Some cells will not have neighbors. If this is the case, return out
* of the method.
*/
        if (neighbor == null) {
            return;
        }

        Vector3 bridge = HexagonPoint.GetBridge(
            neighborEdge.Direction, cellOuterRadius
        );

        bridge.y = neighbor.Position.y - cell.Position.y;

        EdgeVertices edge2 = new EdgeVertices(
            edgeVertices1.vertex1 + bridge,
            edgeVertices1.vertex5 + bridge
        );

//        bool hasRiver = cell.HasRiverThroughEdge(direction);
        bool hasRiver = riverGraph.HasRiverInDirection(
            cell,
            neighborEdge.Direction
        );

//        bool hasRoad = cell.HasRoadThroughEdge(direction);
        bool hasRoad = roadGraph.HasRoadInDirection(
            cell,
            neighborEdge.Direction
        );
        
/* Adjust the other edge of the connection
* if there is a river through that edge.
*/
        if (hasRiver) {
            edge2.vertex3.y = neighbor.StreamBedY;

            Vector3 indices;
            indices.x = indices.z = cell.Index;
            indices.y = neighbor.Index;

            if (!cell.IsUnderwater) {
                if (!neighbor.IsUnderwater) {
                    TriangulateRiverQuad(
                        edgeVertices1.vertex2,
                        edgeVertices1.vertex4,
                        edge2.vertex2,
                        edge2.vertex4,
                        cell.RiverSurfaceY,
                        neighbor.RiverSurfaceY,
                        0.8f,
                        (
//                            cell.HasIncomingRiver &&
//                            cell.IncomingRiver == direction
                              riverGraph.HasIncomingRiverInDirection(
                                  cell,
                                  neighborEdge.Direction
                              )
                        ),
                        indices,
                        cellOuterRadius,
                        wrapSize
                    );
                }
                else if(cell.Elevation > neighbor.WaterLevel) {
                    TriangulateWaterfallInWater(
                        edgeVertices1.vertex2, edgeVertices1.vertex4, 
                        edge2.vertex2, edge2.vertex4, 
                        cell.RiverSurfaceY, 
                        neighbor.RiverSurfaceY,
                        neighbor.WaterSurfaceY,
                        indices,
                        cellOuterRadius,
                        wrapSize
                    );
                }
            }
            else if (
                !neighbor.IsUnderwater &&
                neighbor.Elevation > cell.WaterLevel
            ) {
                TriangulateWaterfallInWater(
                    edge2.vertex4,
                    edge2.vertex2,
                    edgeVertices1.vertex4,
                    edgeVertices1.vertex2,
                    neighbor.RiverSurfaceY,
                    cell.RiverSurfaceY,
                    cell.WaterSurfaceY,
                    indices,
                    cellOuterRadius,
                    wrapSize
                );
            }
        }

        if (
//            cell.GetEdgeType(direction) == ElevationEdgeTypes.Slope
            elevationGraph.GetEdgeTypeInDirection(
                cell,
                neighborEdge.Direction
            ) == ElevationEdgeTypes.Slope
        ) {
            TriangulateEdgeTerraces(
                edgeVertices1, 
                cell, 
                edge2, 
                neighbor,
                hasRoad,
                cellOuterRadius,
                wrapSize
            );
        }
        else {
            TriangulateEdgeStrip(
                edgeVertices1,
                _weights1,
                cell.Index,
                edge2,
                _weights2,
                neighbor.Index,
                cellOuterRadius,
                wrapSize, 
                hasRoad
            );
        }

        features.AddWall(
            edgeVertices1,
            cell,
            edge2,
            neighbor,
            hasRiver,
            hasRoad,
            cellOuterRadius,
            wrapSize
        );

/* Drawn and color the triangle between the bridge of the current cell
* and its current neighbor and the bridge of the next cell and the
* current neighbor.*/
//        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        HexCell nextNeighbor = neighborGraph.TryGetNeighborInDirection(
            cell,
            neighborEdge.Direction.NextClockwise()
        );

        if (
            neighborEdge.Direction <=
            HexDirections.East && nextNeighbor != null
        ) {

/* Create a 5th vertex and assign it with the elevation of the neighbor
* under consideration. This will be used as the final vertex in the
* triangle which fills the gap between bridges.
*/
            Vector3 vertex5 =
                edgeVertices1.vertex5 + HexagonPoint.GetBridge(
                    neighborEdge.Direction.NextClockwise(),
                    cellOuterRadius
                );

            vertex5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation) {
                if (cell.Elevation <= nextNeighbor.Elevation) {

//This cell has lowest elevation, no rotation.
                    TriangulateCorner(
                        edgeVertices1.vertex5,
                        cell,
                        edge2.vertex5,
                        neighbor,
                        vertex5,
                        nextNeighbor,
                        cellOuterRadius,
                        wrapSize
                    );
                }
                else {
// Next neighbor has lowest elevation, rotate counter-clockwise.
                    TriangulateCorner(
                        vertex5,
                        nextNeighbor,
                        edgeVertices1.vertex5,
                        cell,
                        edge2.vertex5,
                        neighbor,
                        cellOuterRadius,
                        wrapSize
                    );
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation) {
// Neighbor is lowest cell, rotate triangle clockwise.
                TriangulateCorner(
                    edge2.vertex5,
                    neighbor,
                    vertex5,
                    nextNeighbor,
                    edgeVertices1.vertex5,
                    cell,
                    cellOuterRadius,
                    wrapSize
                );
            }
            else {

// Next neighbor has lowest elevation, rotate counter-clockwise.
                TriangulateCorner(
                    vertex5,
                    nextNeighbor,
                    edgeVertices1.vertex5,
                    cell,
                    edge2.vertex5,
                    neighbor,
                    cellOuterRadius,
                    wrapSize
                );
            }
        }
    }

    private void TriangulateEdgeTerraces(
        EdgeVertices begin,
        HexCell beginCell,
        EdgeVertices end,
        HexCell endCell,
        bool hasRoad,
        float cellOuterRadius,
        int wrapSize
    ) {
        EdgeVertices edge2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color weight2 = HexagonPoint.TerraceLerp(_weights1, _weights2, 1);
        float index1 = beginCell.Index;
        float index2 = endCell.Index;

        TriangulateEdgeStrip
        (
            begin,
            _weights1, 
            index1, 
            edge2, 
            weight2, 
            index2,
            cellOuterRadius,
            wrapSize,
            hasRoad
        );

        for (int i = 2; i < HexagonPoint.terraceSteps; i++) {
            EdgeVertices edge1 = edge2;
            Color weight1 = weight2;
            edge2 = EdgeVertices.TerraceLerp(begin, end, i);
            weight2 = HexagonPoint.TerraceLerp(_weights1, _weights2, i);

            TriangulateEdgeStrip(
                edge1, 
                weight1, 
                index1,
                edge2, 
                weight2, 
                index2,
                cellOuterRadius,
                wrapSize,
                hasRoad
            );
        }

        TriangulateEdgeStrip(
            edge2, 
            weight2, 
            index1,
            end, 
            _weights2, 
            index2,
            cellOuterRadius,
            wrapSize,
            hasRoad
        );
    }

    private void TriangulateCorner(
        Vector3 bottom,
        HexCell bottomCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell,
        float cellOuterRadius,
        int wrapSize
    ) {
        ElevationEdgeTypes leftEdgeType = bottomCell.GetEdgeType(leftCell);
        ElevationEdgeTypes rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == ElevationEdgeTypes.Slope) {
            if (rightEdgeType == ElevationEdgeTypes.Slope) {

// Corner is also a terrace. Slope-Slope-Flat.
                TriangulateCornerTerraces(
                    bottom,
                    bottomCell,
                    left,
                    leftCell,
                    right,
                    rightCell,
                    cellOuterRadius,
                    wrapSize
                );
            }

// If the right edge is flat, must terrace from left instead of bottom. Slope-Flat-Slope
            else if (rightEdgeType == ElevationEdgeTypes.Flat) {
                TriangulateCornerTerraces (
                    left,
                    leftCell,
                    right,
                    rightCell,
                    bottom,
                    bottomCell,
                    cellOuterRadius,
                    wrapSize
                );
            }
            else {

/* At least one edge is a cliff. Slope-Cliff-Slope or Slope-Cliff-Cliff. Standard case
* because slope on left and flat on right.
*/
                TriangulateCornerTerracesCliff (
                    bottom,
                    bottomCell,
                    left,
                    leftCell,
                    right,
                    rightCell,
                    cellOuterRadius,
                    wrapSize
                );
            }
        }
        else if (rightEdgeType == ElevationEdgeTypes.Slope) {
            if (leftEdgeType == ElevationEdgeTypes.Flat) {

/* If the right edge is a slope, and the left edge is flat, must terrace from right instead
* of bottom. Flat-Slope-Slope.
*/
                TriangulateCornerTerraces (
                    right,
                    rightCell,
                    bottom,
                    bottomCell,
                    left,
                    leftCell,
                    cellOuterRadius,
                    wrapSize
                );
            }
            else {

/* At least one edge is a cliff. Slope-Cliff-Slope or Slope-Cliff-Cliff. Mirror case because
* slope on right and flat on left.
*/
                TriangulateCornerCliffTerraces(
                    bottom,
                    bottomCell,
                    left,
                    leftCell,
                    right,
                    rightCell,
                    cellOuterRadius,
                    wrapSize
                );
            }
        }

/* Neither the left or right cell edge type is a slope. If the right cell type of the left cell
* is a slope, then terraces must be calculated for a corner between two cliff edges.
* Cliff-Cliff-Slope Right, or Cliff-Cliff-Slope Left.
*/
        else if (leftCell.GetEdgeType(rightCell) == ElevationEdgeTypes.Slope) {

// If Cliff-Cliff-Slope-Left
            if (leftCell.Elevation < rightCell.Elevation) {
                TriangulateCornerCliffTerraces(
                    right,
                    rightCell,
                    bottom,
                    bottomCell,
                    left,
                    leftCell,
                    cellOuterRadius,
                    wrapSize
                );
            }

// If Cliff-Cliff-Slope-Right
            else {
                TriangulateCornerTerracesCliff(
                    left,
                    leftCell,
                    right,
                    rightCell,
                    bottom,
                    bottomCell,
                    cellOuterRadius,
                    wrapSize
                );
            }
        }

// Else all edges are cliffs. Simply draw a triangle.
        else {
            terrain.AddTriangle(
                bottom,
                left,
                right,
                cellOuterRadius,
                wrapSize
            );

            Vector3 indices;
            indices.x = bottomCell.Index;
            indices.y = leftCell.Index;
            indices.z = rightCell.Index;

            terrain.AddTriangleCellData(
                indices,
                _weights1,
                _weights2,
                _weights3
            );
        }

        features.AddWall(
            bottom,
            bottomCell,
            left,
            leftCell,
            right,
            rightCell,
            cellOuterRadius,
            wrapSize
        );
    }

    private void TriangulateCornerTerraces(
        Vector3 begin,
        HexCell beginCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell,
        float cellOuterRadius,
        int wrapSize
    ) {
        Vector3 vertex3 = HexagonPoint.TerraceLerp(begin, left, 1);
        Vector3 vertex4 = HexagonPoint.TerraceLerp(begin, right, 1);
        Color weight3 = HexagonPoint.TerraceLerp(_weights1, _weights2, 1);
        Color weight4 = HexagonPoint.TerraceLerp(_weights1, _weights3, 1);

        Vector3 indices;

        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        terrain.AddTriangle(
            begin,
            vertex3,
            vertex4,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddTriangleCellData(
            indices,
            _weights1,
            weight3,
            weight4
        );

        for (int i = 2; i < HexagonPoint.terraceSteps; i++) {
            Vector3 vertex1 = vertex3;
            Vector3 vertex2 = vertex4;
            Color weight1 = weight3;
            Color weight2 = weight4;

            vertex3 = HexagonPoint.TerraceLerp(begin, left, i);
            vertex4 = HexagonPoint.TerraceLerp(begin, right, i);
            weight3 = HexagonPoint.TerraceLerp(_weights1, _weights2, i);
            weight4 = HexagonPoint.TerraceLerp(_weights1, _weights3, i);

            terrain.AddQuad(
                vertex1,
                vertex2,
                vertex3,
                vertex4,
                cellOuterRadius,
                wrapSize
            );
            
            terrain.AddQuadCellData(
                indices,
                weight1,
                weight2,
                weight3,
                weight4
            );
        }

        terrain.AddQuad(
            vertex3,
            vertex4,
            left,
            right,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddQuadCellData(
            indices,
            weight3,
            weight4,
            _weights2,
            _weights3
        );
    }

    private void TriangulateCornerTerracesCliff(
        Vector3 begin,
        HexCell beginCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell,
        float cellOuterRadius,
        int wrapSize
    ) {
/* Set boundary distance to 1 elevation level above the bottom-most cell
* in the case.
*/
        float boundaryDistance =
            1f / (rightCell.Elevation - beginCell.Elevation);

/*If boundary distance becomes negative, CCSR and CCSL case will have
*strange behavior.
*/
        if (boundaryDistance < 0) {
            boundaryDistance = -boundaryDistance;
        }

// Must interpolate the perturbed points, not the original points.
        Vector3 boundary =
            Vector3.Lerp(
                HexagonPoint.Perturb(
                    begin,
                    cellOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    right,
                    cellOuterRadius,
                    wrapSize
                ),
                boundaryDistance
            );

        Color boundaryWeights =
            Color.Lerp(_weights1, _weights2, boundaryDistance);

        Vector3 indices;

        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle (
            begin,
            _weights1,
            left,
            _weights2,
            boundary,
            boundaryWeights,
            indices,
            cellOuterRadius,
            wrapSize
        );

// Slope-Cliff-Slope. Triangulate a slope.
        if (leftCell.GetEdgeType(rightCell) == ElevationEdgeTypes.Slope) {
            TriangulateBoundaryTriangle (
                left, 
                _weights2,
                right, 
                _weights3,
                boundary, 
                boundaryWeights,
                indices,
                cellOuterRadius,
                wrapSize
            );
        }

// Slope-Cliff-Cliff. Triangulate a cliff.
        else {

/* Add perturbation for all vertices except the boundary
* vertex, to handle the Slope-Cliff-Cliff case of the 
* Cliff-Slope perturbation problem.
*/
            terrain.AddTriangleUnperturbed(
                HexagonPoint.Perturb(
                    left,
                    cellOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    right,
                    cellOuterRadius,
                    wrapSize
                ),
                boundary
            );

            terrain.AddTriangleCellData(
                indices,
                _weights2,
                _weights3,
                boundaryWeights
            );
        }
    }

    private void TriangulateCornerCliffTerraces(
        Vector3 begin,
        HexCell beginCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell,
        float cellOuterRadius,
        int wrapSize
    ) {
/* Set boundary distance to 1 elevation level above the bottom-most cell
* in the case.
*/
        float boundaryDistance =
            1f / (leftCell.Elevation - beginCell.Elevation);

// If boundary distance becomes negative, CCSR and CCSL case will have strange behavior.
        if (boundaryDistance < 0) {
            boundaryDistance = -boundaryDistance;
        }

// Must interpolate between the perturbed points, not the original points.
        Vector3 boundary = 
            Vector3.Lerp(
                HexagonPoint.Perturb(
                    begin,
                    cellOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    left,
                    cellOuterRadius,
                    wrapSize
                ),
                boundaryDistance
            );

        Color boundaryWeights = 
            Color.Lerp(
                _weights1,
                _weights2,
                boundaryDistance
            );

        Vector3 indices;

        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        TriangulateBoundaryTriangle(
            right, 
            _weights3,
            begin,
            _weights1,
            boundary,
            boundaryWeights,
            indices,
            cellOuterRadius,
            wrapSize
        );

// Slope-Cliff-Slope. Triangulate a slope.
        if (leftCell.GetEdgeType(rightCell) == ElevationEdgeTypes.Slope) {
            TriangulateBoundaryTriangle(
                left, 
                _weights2,
                right, 
                _weights3,
                boundary, 
                boundaryWeights,
                indices,
                cellOuterRadius,
                wrapSize
            );
        }

// Slope-Cliff-Cliff. Triangulate a cliff.
        else
        {

/* Add perturbation to all vertices except the boundary vertex
* to handle the Slope-Cliff-Cliff case of the Cliff-Slope perturbation
* problem.
*/
            terrain.AddTriangleUnperturbed(
                HexagonPoint.Perturb(
                    left,
                    cellOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    right,
                    cellOuterRadius,
                    wrapSize
                ),
                boundary
            );

            terrain.AddTriangleCellData(
                indices, 
                _weights2, 
                _weights3, 
                boundaryWeights
            );
        }
    }

    private void TriangulateBoundaryTriangle(
        Vector3 begin,
        Color beginWeights,
        Vector3 left, 
        Color leftWeights,
        Vector3 boundary,
        Color boundaryWeights,
        Vector3 indices,
        float cellOuterRadius,
        int wrapSize
    ) {

/* Immediately perturb vertex 2 as an optimization since it is not
* being used to derive any other point.
*/
        Vector3 vertex2 = 
            HexagonPoint.Perturb(
                HexagonPoint.TerraceLerp(begin, left, 1),
                cellOuterRadius,
                wrapSize
            );

        Color weight2 = 
            HexagonPoint.TerraceLerp(
                beginWeights,
                leftWeights,
                1
            );

/* Perturb all vertices except the boundary vertex, to avoid moving
* the vertex out of alignment with a cliff. vertex2 has already been
* perturbed. Handles the Cliff-Slope-Slope and Slope-Cliff-Slope cases
* of the Cliff-Slope perturbation problem.
*/
        terrain.AddTriangleUnperturbed(
            HexagonPoint.Perturb(
                begin,
                cellOuterRadius,
                wrapSize
            ),
            vertex2,
            boundary
        );

        terrain.AddTriangleCellData(indices, beginWeights, weight2, boundaryWeights);

        for (int i = 2; i < HexagonPoint.terraceSteps; i++) {

/* vertex2 has already been perturbed, need not pertub
* vertex1 as it is derived from vertex2.
*/
            Vector3 vertex1 = vertex2;
            Color weight1 = weight2;

            vertex2 = HexagonPoint.Perturb(
                HexagonPoint.TerraceLerp(begin, left, i),
                cellOuterRadius,
                wrapSize
            );

            weight2 = HexagonPoint.TerraceLerp(
                beginWeights,
                leftWeights,
                i
            );

            terrain.AddTriangleUnperturbed(vertex1, vertex2, boundary);

            terrain.AddTriangleCellData(
                indices,
                weight1,
                weight2,
                boundaryWeights
            );
        }

        terrain.AddTriangleUnperturbed(
            vertex2,
            HexagonPoint.Perturb(
                left,
                cellOuterRadius,
                wrapSize
            ),
            boundary
        );
        terrain.AddTriangleCellData(indices, weight2, leftWeights, boundaryWeights);
    }

    private void TriangulateEdgeFan(
        Vector3 center,
        EdgeVertices edge,
        float index,
        float cellOuterRadius,
        int wrapSize
    ) {
        terrain.AddTriangle(
            center,
            edge.vertex1,
            edge.vertex2,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            center,
            edge.vertex2,
            edge.vertex3,
            cellOuterRadius,
            wrapSize
        );
        
        terrain.AddTriangle(
            center,
            edge.vertex3,
            edge.vertex4,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            center,
            edge.vertex4,
            edge.vertex5,
            cellOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);
    }

    private void TriangulateEdgeStrip(
        EdgeVertices edge1,
        Color weight1,
        float index1,
        EdgeVertices edge2,
        Color weight2,
        float index2,
        float cellOuterRadius,
        int wrapSize,
        bool hasRoad = false
    ) {
        terrain.AddQuad(
            edge1.vertex1,
            edge1.vertex2,
            edge2.vertex1,
            edge2.vertex2,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            edge1.vertex2,
            edge1.vertex3,
            edge2.vertex2,
            edge2.vertex3,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            edge1.vertex3,
            edge1.vertex4,
            edge2.vertex3,
            edge2.vertex4,
            cellOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            edge1.vertex4,
            edge1.vertex5,
            edge2.vertex4,
            edge2.vertex5,
            cellOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.z = index1;
        indices.y = index2;

        terrain.AddQuadCellData(indices, weight1, weight2);
        terrain.AddQuadCellData(indices, weight1, weight2);
        terrain.AddQuadCellData(indices, weight1, weight2);
        terrain.AddQuadCellData(indices, weight1, weight2);

        if (hasRoad) {
            TriangulateRoadSegment(
                edge1.vertex2, 
                edge1.vertex3, 
                edge1.vertex4, 
                edge2.vertex2, 
                edge2.vertex3, 
                edge2.vertex4,
                weight1, 
                weight2, 
                indices,
                cellOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateRiverQuad(
        Vector3 vertex1,
        Vector3 vertex2,
        Vector3 vertex3,
        Vector3 vertex4,
        float y1, float y2,
        float v,
        bool reversed,
        Vector3 indices,
        float cellOuterRadius,
        int wrapSize
    ) {
        vertex1.y = vertex2.y = y1;
        vertex3.y = vertex4.y = y2;

        rivers.AddQuad(
            vertex1,
            vertex2,
            vertex3,
            vertex4,
            cellOuterRadius,
            wrapSize
        );

        if (reversed) {
            rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
        }
        else {
            rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
        }

        rivers.AddQuadCellData(indices, _weights1, _weights2);
    }

    private void TriangulateRiverQuad(
        Vector3 vertex1, 
        Vector3 vertex2, 
        Vector3 vertex3, 
        Vector3 vertex4, 
        float y, 
        float v, 
        bool reversed,
        Vector3 indices,
        float cellOuterRadius,
        int wrapSize
    ) {
        TriangulateRiverQuad(
            vertex1,
            vertex2,
            vertex3,
            vertex4,
            y, y, v,
            reversed,
            indices,
            cellOuterRadius,
            wrapSize
        );
    }

    private void TriangulateRoadSegment (
        Vector3 vertex1,
        Vector3 vertex2,
        Vector3 vertex3,
        Vector3 vertex4,
        Vector3 vertex5,
        Vector3 vertex6,
        Color weight1,
        Color weight2,
        Vector3 indices,
        float cellOuterRadius,
        int wrapSize
    ) {
        roads.AddQuad(
            vertex1,
            vertex2,
            vertex4,
            vertex5,
            cellOuterRadius,
            wrapSize
        );

        roads.AddQuad(
            vertex2,
            vertex3,
            vertex5,
            vertex6,
            cellOuterRadius,
            wrapSize
        );

        roads.AddQuadUV(0f, 1f, 0f, 0f);
        roads.AddQuadUV(1f, 0f, 0f, 0f);

        roads.AddQuadCellData(indices, weight1, weight2);
        roads.AddQuadCellData(indices, weight1, weight2);
    }

    private void TriangulateRoadEdge(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight,
        float index,
        float cellOuterRadius,
        int wrapSize
    ) {
        roads.AddTriangle(
            center,
            middleLeft,
            middleRight,
            cellOuterRadius,
            wrapSize
        );

        roads.AddTriangleUV(
            new Vector2(1f, 0f), 
            new Vector2(0f, 0f), 
            new Vector2(0f, 0f)
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = index;
        roads.AddTriangleCellData(indices, _weights1);
    }

    private void TriangulateRoadAdjacentToRiver(
        HexCell cell, 
        Vector3 center,
        HexEdge neighborEdge,
        RiverGraph riverGraph,
        RoadGraph roadGraph,
        EdgeVertices edgeVertices,
        float cellOuterRadius,
        int wrapSize
    ) {
//        bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
        bool hasRoadThroughEdge = roadGraph.HasRoadInDirection(
            cell,
            neighborEdge.Direction
        );

//          bool previousHasRiver = cell.HasRiverThroughEdge(
//              direction.Previous()
//          );
        bool previousHasRiver = riverGraph.HasRiverInDirection(
            cell,
            neighborEdge.Direction.PreviousClockwise()
        );

//        bool nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
        bool nextHasRiver = riverGraph.HasIncomingRiverInDirection(
            cell,
            neighborEdge.Direction.NextClockwise()
        );

        Vector2 interpolators = GetRoadInterpolators(
            cell,
            neighborEdge,
            roadGraph
        );

        Vector3 roadCenter = center;

//        if (cell.HasRiverBeginOrEnd) {
        if (riverGraph.HasRiverStartOrEnd(cell)) {
            roadCenter += 
                HexagonPoint.GetSolidEdgeMiddle(
//                    cell.RiverBeginOrEndDirection.Opposite(),
                    riverGraph.RiverStartOrEndDirection(cell).Opposite(),
                    cellOuterRadius
                ) * 
                (1f / 3f);
        }
//        else if(cell.IncomingRiver == cell.OutgoingRiver.Opposite()) {
        else if (
            riverGraph.HasStraightRiver(cell)
        ) {
            Vector3 corner;

//  If the previous cell has a river, the corner the center will be moved
//  toward is equal to the current direction + 1.
            if (previousHasRiver) {
                if (
                    !hasRoadThroughEdge &&
//                    !cell.HasRoadThroughEdge(direction.Next())
                    !roadGraph.HasRoadInDirection(
                        cell,
                        neighborEdge.Direction.NextClockwise()
                    )
                ) {
                    return;
                }
                corner = HexagonPoint.GetSecondSolidCorner(
                    neighborEdge.Direction,
                    cellOuterRadius
                );
            }
// If the previous cell does not have a river, the corner the center will
// be moved toward is the same index as the current direction.
            else {
                if (
                    !hasRoadThroughEdge &&
//                    !cell.HasRoadThroughEdge(direction.Previous())
                    !roadGraph.HasRoadInDirection(
                        cell,
                        neighborEdge.Direction.PreviousClockwise()
                    )
                ) {
                    return;
                }

                corner = HexagonPoint.GetFirstSolidCorner(
                    neighborEdge.Direction,
                    cellOuterRadius
                );
            }
/* Using the example of a river flowing from east to west or west to east, for all cases
* this will result in the river being pushed either directly "up" north away from the
* river or directly "down" south away from the river.
*/
            roadCenter += corner * 0.5f;

            if (
//                cell.IncomingRiver == direction.Next() && 
                riverGraph.IncomingRiverDirections(cell)[0] ==
                neighborEdge.Direction.NextClockwise() &&
//                cell.HasRoadThroughEdge(direction.Next2()) ||
                roadGraph.HasRoadInDirection(
                    cell,
                    neighborEdge.Direction.NextClockwise2()
                ) ||
//                cell.HasRoadThroughEdge(direction.Opposite())
                roadGraph.HasRoadInDirection(
                    cell,
                    neighborEdge.Direction.Opposite()
                )
            ) {
                features.AddBridge(
                    roadCenter,
                    center - corner * 0.5f,
                    cellOuterRadius,
                    wrapSize
                );
            }
            
            center += corner * 0.25f;
        }

// If the river has a zigzag, then the incoming river will be the on the
// edge previous from the outgoing river or the incoming river will be on
// the next edge of the outoing river. In the case of the former, the
// index of the corner whose vector is pointing away from the river is the
// index of the incoming river + 1. Otherwise it is the index of the
// incoming river. In both cases, subtracting the road center by that 
// vector times 0.2f is sufficent to push the road center away from the
// river.

//        else if (cell.IncomingRiver == cell.OutgoingRiver.Previous()) {
          else if (riverGraph.HasStraightRiver(cell)) {
            roadCenter -= HexagonPoint.GetSecondCorner(
//                cell.IncomingRiver,
                riverGraph.IncomingRiverDirections(cell)[0],
                cellOuterRadius
            ) * 0.2f;
        }
//        else if (cell.IncomingRiver == cell.OutgoingRiver.Next()) {
        else if (riverGraph.HasClockwiseCornerRiver(cell)) {
            roadCenter -= HexagonPoint.GetFirstCorner(
//                cell.IncomingRiver,
                riverGraph.IncomingRiverDirections(cell)[0],
                cellOuterRadius
            ) * 0.2f;
        }

// If there is a river on the previous and next edges, the river has a
// slight bend. Need to pull the road center toward the current cell edge,
// which will shorten the road back away from the river.

        else if(previousHasRiver && nextHasRiver) { 
            if (!hasRoadThroughEdge) {
                return;
            }

// Must account for difference in scale between corners and middles by
// using HexMetrics.innerToOuter.

            Vector3 offset = 
                HexagonPoint.GetSolidEdgeMiddle(
                    neighborEdge.Direction,
                    cellOuterRadius
                ) *
                HexagonConstants.INNER_TO_OUTER_RATIO;
            
            roadCenter += offset * 0.7f;
            center += offset * 0.5f;
        }

// The only remaining case is that the cell lies on the outside of a
// curving river. In this case, there are three edges pointing away from
// the river. The middle edge of these three edges must be obtained.
// Then, the center of the road is pushed toward the middle of this edge.
        else {
            HexDirections middle;
            if (previousHasRiver) {
//                middle = direction.Next();
                middle = neighborEdge.Direction.NextClockwise();
            }
            else if (nextHasRiver) {
//                middle = direction.Previous();
                middle = neighborEdge.Direction.PreviousClockwise();
            }
            else {
//                middle = direction;
                middle = neighborEdge.Direction;
            }

// If there is no road through any of the cells on the outer side of the
// river bend, then the road center need not move and should instead be
// pruned.
            if (
//                !cell.HasRoadThroughEdge(middle) &&
                !roadGraph.HasRoadInDirection(
                    cell,
                    middle
                ) &&   
//                !cell.HasRoadThroughEdge(middle.Previous()) &&
                !roadGraph.HasRoadInDirection(
                    cell,
                    middle.PreviousClockwise()
                ) &&
//                !cell.HasRoadThroughEdge(middle.Next())
                !roadGraph.HasRoadInDirection(
                    cell,
                    middle.NextClockwise()
                )
            ) {
                return;
            }

            Vector3 offset = HexagonPoint.GetSolidEdgeMiddle(middle, cellOuterRadius);
            roadCenter += offset * 0.25f;

            if (
                neighborEdge.Direction == middle &&
//                cell.HasRoadThroughEdge(direction.Opposite())
                roadGraph.HasRoadInDirection(
                    cell,
                    neighborEdge.Direction.Opposite()
                )
            ) {
                features.AddBridge (
                    roadCenter,
                    center - offset * (HexagonConstants.INNER_TO_OUTER_RATIO * 0.7f),
                    cellOuterRadius,
                    wrapSize
                );
            }
        }

        Vector3 middleLeft = 
            Vector3.Lerp(roadCenter, edgeVertices.vertex1, interpolators.x);
        Vector3 middleRight =
            Vector3.Lerp(roadCenter, edgeVertices.vertex5, interpolators.y);

        TriangulateRoad(
            roadCenter,
            middleLeft,
            middleRight,
            edgeVertices,
            hasRoadThroughEdge,
            cell.Index,
            cellOuterRadius,
            wrapSize
        );

        if (previousHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                center,
                middleLeft,
                cell.Index,
                cellOuterRadius,
                wrapSize
            );
        }

        if (nextHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                middleRight,
                center,
                cell.Index,
                cellOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateRoad(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight, 
        EdgeVertices edge,
        bool hasRoadThroughCellEdge,
        float index,
        float cellOuterRadius,
        int wrapSize
    ) {
        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        if (hasRoadThroughCellEdge) {
            Vector3 middleCenter = Vector3.Lerp(middleLeft, middleRight, 0.5f);

            TriangulateRoadSegment(
                middleLeft,
                middleCenter,
                middleRight,
                edge.vertex2,
                edge.vertex3,
                edge.vertex4,
                _weights1,
                _weights1,
                indices,
                cellOuterRadius,
                wrapSize
            );

            roads.AddTriangle(
                center,
                middleLeft,
                middleCenter,
                cellOuterRadius,
                wrapSize
            );
            
            roads.AddTriangle(
                center,
                middleCenter,
                middleRight,
                cellOuterRadius,
                wrapSize
            );
            
            roads.AddTriangleUV(
                new Vector2(1f, 0f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f)
            );

            roads.AddTriangleUV(
                new Vector2(1f, 0f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f)
            );

            roads.AddTriangleCellData(indices, _weights1);
            roads.AddTriangleCellData(indices, _weights1);
        }
        else {
            TriangulateRoadEdge(
                center,
                middleLeft,
                middleRight,
                index,
                cellOuterRadius,
                wrapSize
            );
        }

    }

    private Vector2 GetRoadInterpolators(
        HexCell cell,
        HexEdge neighborEdge,
        RoadGraph roadGraph
    ) {
        Vector2 interpolators;

//        if (cell.HasRoadThroughEdge(direction)) {
        if (
            roadGraph.HasRoadInDirection(
                cell,
                neighborEdge.Direction
            )
        ) {
            interpolators.x = interpolators.y = 0.5f;
        }
        else {
            interpolators.x =
//              cell.HasRoadThroughEdge(direction.Previous()) ?
//                  0.5f : 0.25f;
                roadGraph.HasRoadInDirection(
                    cell,
                    neighborEdge.Direction.PreviousClockwise()
                ) ?
                0.5f : 0.25f;
            
            interpolators.y =
//              cell.HasRoadThroughEdge(direction.Next()) ?
//                  0.5f : 0.25f;
                roadGraph.HasRoadInDirection(
                    cell,
                    neighborEdge.Direction.NextClockwise()
                ) ?
                0.5f : 0.25f;
        }

        return interpolators;
    }

    private void TriangulateWater(
        HexCell cell,
        HexEdge neighborEdge,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        Vector3 center,
        float cellOuterRadius,
        int wrapSize
    ) {
        center.y = cell.WaterSurfaceY;

        if (
            neighborEdge.Target != null &&
            !neighborEdge.Target.IsUnderwater
        ) {
            TriangulateWaterShore(
                cell,
                neighborEdge,
                neighborGraph,
                riverGraph,
                center,
                cellOuterRadius,
                wrapSize
            );
        }
        else {
            TriangulateOpenWater(
                cell,
                neighborEdge,
                neighborGraph,
                riverGraph,
                center,
                cellOuterRadius,
                wrapSize
            );
        }            
    }

    private void TriangulateOpenWater(
        HexCell cell,
        HexEdge neighborEdge,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        Vector3 center,
        float cellOuterRadius,
        int wrapSize
    ) { 
        Vector3 center1 =
            center +
            HexagonPoint.GetFirstWaterCorner(
                neighborEdge.Direction,
                cellOuterRadius
            );

        Vector3 center2 =
            center +
            HexagonPoint.GetSecondWaterCorner(
                neighborEdge.Direction,
                cellOuterRadius
            );

        water.AddTriangle(
            center,
            center1,
            center2,
            cellOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;
        water.AddTriangleCellData(indices, _weights1);

        if (
            neighborEdge.Direction <= HexDirections.Southeast && 
            neighborEdge.Target != null
        ) {
            Vector3 bridge = HexagonPoint.GetWaterBridge(
                neighborEdge.Direction,
                cellOuterRadius
            );

            Vector3 edge1 = center1 + bridge;
            Vector3 edge2 = center2 + bridge;

            water.AddQuad(
                center1,
                center2,
                edge1,
                edge2,
                cellOuterRadius,
                wrapSize
            );
            
            indices.y = neighborEdge.Target.Index;
            water.AddQuadCellData(indices, _weights1, _weights2);

            if (neighborEdge.Direction <= HexDirections.East) {
                HexCell nextNeighbor =
//                    cell.GetNeighbor(direction.NextClockwise());
                    neighborGraph.TryGetNeighborInDirection(
                        cell,
                        neighborEdge.Direction.NextClockwise()
                    );

                if (nextNeighbor == null || !nextNeighbor.IsUnderwater) {
                    return;
                }

                water.AddTriangle(
                    center2, 
                    edge2, 
                    center2 + HexagonPoint.GetWaterBridge(
                        neighborEdge.Direction.NextClockwise(),
                        cellOuterRadius
                    ),
                    cellOuterRadius,
                    wrapSize
                );

                indices.z = nextNeighbor.Index;

                water.AddTriangleCellData(
                    indices, _weights1, _weights2, _weights3
                );
            }
        }
    }

    private void TriangulateWaterShore(
        HexCell cell,
        HexEdge neighborEdge,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph,
        Vector3 center,
        float cellOuterRadius,
        int wrapSize
    ) {
        EdgeVertices edge1 = new EdgeVertices(
            center + HexagonPoint.GetFirstWaterCorner(
                neighborEdge.Direction,
                cellOuterRadius
            ),
            center + HexagonPoint.GetSecondWaterCorner(
                neighborEdge.Direction,
                cellOuterRadius
            )
        );

        water.AddTriangle(
            center,
            edge1.vertex1,
            edge1.vertex2,
            cellOuterRadius,
            wrapSize
        );
        
        water.AddTriangle(
            center,
            edge1.vertex2,
            edge1.vertex3,
            cellOuterRadius,
            wrapSize
        );
        
        water.AddTriangle(
            center,
            edge1.vertex3,
            edge1.vertex4,
            cellOuterRadius,
            wrapSize
        );
        
        water.AddTriangle(
            center,
            edge1.vertex4,
            edge1.vertex5,
            cellOuterRadius,
            wrapSize
        );

        Vector3 indices = new Vector3();
        indices.x = indices.y = cell.Index;
        indices.y = neighborEdge.Target.Index;

        water.AddTriangleCellData(indices, _weights1);
        water.AddTriangleCellData(indices, _weights1);
        water.AddTriangleCellData(indices, _weights1);
        water.AddTriangleCellData(indices, _weights1);

// Work backward from the solid shore to obtain the edge.
        Vector3 center2 = neighborEdge.Target.Position;

        float cellInnerRadius =
            HexagonPoint.GetOuterToInnerRadius(cellOuterRadius);
        float cellInnerDiameter = cellInnerRadius * 2f;

// If the neighbor outside the wrap boundaries, adjust accordingly.
        if (neighborEdge.Target.ColumnIndex < cell.ColumnIndex - 1) {
            center2.x += 
                wrapSize * cellInnerDiameter;
        }
        else if (neighborEdge.Target.ColumnIndex > cell.ColumnIndex + 1) {
            center2.x -=
                wrapSize * cellInnerDiameter;
        }

        center2.y = center.y;

        EdgeVertices edge2 = new EdgeVertices(
            center2 + HexagonPoint.GetSecondSolidCorner(
                neighborEdge.Direction.Opposite(),
                cellOuterRadius
            ),
            center2 + HexagonPoint.GetFirstSolidCorner(
                neighborEdge.Direction.Opposite(),
                cellOuterRadius
            )
        );

        if (
//          cell.HasRiverThroughEdge(direction)
            riverGraph.HasRiverInDirection(
                cell,
                neighborEdge.Direction
            )
        ) {
            TriangulateEstuary(
                edge1,
                edge2,
//                (cell.HasIncomingRiver &&
//                cell.IncomingRiver == direction),
                (
                    riverGraph.HasIncomingRiver(cell) &&
                    riverGraph.IncomingRiverDirections(cell)[0] ==
                    neighborEdge.Direction
                ),
                indices,
                cellOuterRadius,
                wrapSize
            );

        }
        else {
            waterShore.AddQuad(
                edge1.vertex1,
                edge1.vertex2,
                edge2.vertex1,
                edge2.vertex2,
                cellOuterRadius,
                wrapSize
            );

            waterShore.AddQuad(
                edge1.vertex2,
                edge1.vertex3,
                edge2.vertex2,
                edge2.vertex3,
                cellOuterRadius,
                wrapSize
            );

            waterShore.AddQuad(
                edge1.vertex3,
                edge1.vertex4,
                edge2.vertex3,
                edge2.vertex4,
                cellOuterRadius,
                wrapSize
            );

            waterShore.AddQuad(
                edge1.vertex4,
                edge1.vertex5,
                edge2.vertex4,
                edge2.vertex5,
                cellOuterRadius,
                wrapSize
            );

            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);

            waterShore.AddQuadCellData(indices, _weights1, _weights2);
            waterShore.AddQuadCellData(indices, _weights1, _weights2);
            waterShore.AddQuadCellData(indices, _weights1, _weights2);
            waterShore.AddQuadCellData(indices, _weights1, _weights2);
        }
        
        HexCell nextNeighbor =
//            cell.GetNeighbor(direction.NextClockwise());
            neighborGraph.TryGetNeighborInDirection(
                cell,
                neighborEdge.Direction.NextClockwise()
            );

        if (nextNeighbor != null) {
            Vector3 center3 = nextNeighbor.Position;

            if (nextNeighbor.ColumnIndex < cell.ColumnIndex - 1) {
                center3.x += wrapSize * cellInnerDiameter;
            }
            else if (nextNeighbor.ColumnIndex > cell.ColumnIndex + 1) {
                center3.x -= wrapSize * cellInnerDiameter;
            }

// Work backward from the shore to obtain the triangle if the neighbor is
// underwater, otherwise obtain normal triangle.

            Vector3 vertex3 = 
                center3 + (
                    nextNeighbor.IsUnderwater ?
                    HexagonPoint.GetFirstWaterCorner(
                        neighborEdge.Direction.PreviousClockwise(),
                        cellOuterRadius
                    ) :
                    HexagonPoint.GetFirstSolidCorner(
                        neighborEdge.Direction.PreviousClockwise(),
                        cellOuterRadius
                    )
                );

            vertex3.y = center.y;

            waterShore.AddTriangle (
                edge1.vertex5,
                edge2.vertex5,
                vertex3,
                cellOuterRadius,
                wrapSize
            );

            indices.z = nextNeighbor.Index;

            waterShore.AddTriangleCellData (
                indices, _weights1, _weights2, _weights3
            );

            waterShore.AddTriangleUV (
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
            );
        }
    }

    private void TriangulateEstuary(
        EdgeVertices edge1,
        EdgeVertices edge2,
        bool incomingRiver,
        Vector3 indices,
        float cellOuterRadius,
        int wrapSize
    ) {
        waterShore.AddTriangle(
            edge2.vertex1,
            edge1.vertex2,
            edge1.vertex1,
            cellOuterRadius,
            wrapSize
        );

        waterShore.AddTriangle(
            edge2.vertex5,
            edge1.vertex5,
            edge1.vertex4,
            cellOuterRadius,
            wrapSize
        );

        waterShore.AddTriangleUV
        (
            new Vector2(0f, 1f), 
            new Vector2(0f, 0f), 
            new Vector2(0f, 0f)
        );

        waterShore.AddTriangleUV
        (
            new Vector2(0f, 1f),
            new Vector2(0f, 0f),
            new Vector2(0f, 0f)
        );

        waterShore.AddTriangleCellData(
            indices, 
            _weights2, 
            _weights1, 
            _weights1
        );

        waterShore.AddTriangleCellData(
            indices,
            _weights2,
            _weights1,
            _weights1
        );


        estuaries.AddQuad(
            edge2.vertex1, 
            edge1.vertex2, 
            edge2.vertex2, 
            edge1.vertex3,
            cellOuterRadius,
            wrapSize
        );

        estuaries.AddTriangle(
            edge1.vertex3, 
            edge2.vertex2, 
            edge2.vertex4,
            cellOuterRadius,
            wrapSize
        );

        estuaries.AddQuad(
            edge1.vertex3, 
            edge1.vertex4, 
            edge2.vertex4, 
            edge2.vertex5,
            cellOuterRadius,
            wrapSize
        );

        estuaries.AddQuadUV(
            new Vector2(0f, 1f), 
            new Vector2(0f, 0f), 
            new Vector2(1f, 1f), 
            new Vector2(0f, 0f)
        );

        estuaries.AddQuadCellData(
            indices, _weights2, _weights1, _weights2, _weights1
        );

        estuaries.AddTriangleCellData(indices, _weights1, _weights2, _weights2);
        estuaries.AddQuadCellData(indices, _weights1, _weights2);

        estuaries.AddTriangleUV(
            new Vector2(0f, 0f),
            new Vector2(1f, 1f),
            new Vector2(1f, 1f)
        );

        estuaries.AddQuadUV(
            new Vector2(0f, 0f), 
            new Vector2(0f, 0f),
            new Vector2(1f, 1f), 
            new Vector2(0f, 1f)
        );

        if (incomingRiver) {
            estuaries.AddQuadUV2(
                new Vector2(1.5f, 1f),
                new Vector2(0.7f, 1.15f),
                new Vector2(1f, 0.8f),
                new Vector2(0.5f, 1.1f)
            );

            estuaries.AddTriangleUV2(
                new Vector2(0.5f, 1.1f),
                new Vector2(1f, 0.8f),
                new Vector2(0f, 0.8f)
            );
            
            estuaries.AddQuadUV2(
                new Vector2(0.5f, 1.1f),
                new Vector2(0.3f, 1.15f),
                new Vector2(0f, 0.8f),
                new Vector2(-0.5f, 1f)
            );

        }
        else {
            estuaries.AddQuadUV2(
                new Vector2(-0.5f, -0.2f), 
                new Vector2(0.3f, -0.35f),
                new Vector2(0f, 0f), 
                new Vector2(0.5f, -0.3f)
            );

            estuaries.AddTriangleUV2(
                new Vector2(0.5f, -0.3f),
                new Vector2(0f, 0f),
                new Vector2(1f, 0f)
            );

            estuaries.AddQuadUV2(
                new Vector2(0.5f, -0.3f), 
                new Vector2(0.7f, -0.35f),
                new Vector2(1f, 0f), 
                new Vector2(1.5f, -0.2f)
            );

        }
    }

    private void TriangulateWaterfallInWater(
        Vector3 vertex1,
        Vector3 vertex2, 
        Vector3 vertex3,
        Vector3 vertex4,
        float y1,
        float y2, 
        float waterY,
        Vector3 indices,
        float cellOuterRadius,
        int wrapSize
    ) {
        vertex1.y = vertex2.y = y1;
        vertex3.y = vertex4.y = y2;
        
        vertex1 = HexagonPoint.Perturb(
            vertex1,
            cellOuterRadius,
            wrapSize
        );

        vertex2 = HexagonPoint.Perturb(
            vertex2,
            cellOuterRadius,
            wrapSize
        );

        vertex3 = HexagonPoint.Perturb(
            vertex3,
            cellOuterRadius,
            wrapSize
        );

        vertex4 = HexagonPoint.Perturb(
            vertex4,
            cellOuterRadius,
            wrapSize
        );

        float t = (waterY - y2) / (y1 - y2);
        vertex3 = Vector3.Lerp(vertex3, vertex1, t);
        vertex4 = Vector3.Lerp(vertex4, vertex2, t);

        rivers.AddQuadUnperturbed(
            vertex1,
            vertex2,
            vertex3,
            vertex4
        );

        rivers.AddQuadUV(0f, 1f, 0.8f, 1f);
        rivers.AddQuadCellData(indices, _weights1, _weights2);
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
