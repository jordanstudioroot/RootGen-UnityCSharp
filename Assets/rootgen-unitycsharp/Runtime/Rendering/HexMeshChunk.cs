using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RootLogging;

public class HexMeshChunk : MonoBehaviour {
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
    private Canvas _canvas;

// TODO: Temporarily reusing this variable as this value
//       must be present in LateUpdate. Should refactor class
//       so that this is not necessary.
    private float _hexOuterRadius;
    
    private HexAdjacencyGraph _graph;

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
    public Hex[] Hexes {
        get; set;
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
    public static HexMeshChunk CreateEmpty() {
        GameObject resultObj = new GameObject("Hex Mesh Chunk");
        HexMeshChunk resultMono = resultObj.AddComponent<HexMeshChunk>();
        
        resultMono.Hexes = new Hex[
            MeshConstants.ChunkSizeX *
            MeshConstants.ChunkSizeZ
        ];
        
        GameObject resultCanvasObj = new GameObject(
            "Hex Mesh Chunk Canvas"
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

    public static HexMeshChunk CreateEmpty(Transform parent) {
        HexMeshChunk result = CreateEmpty();
        result.transform.SetParent(parent, false);
        return result;
    }

// ~~ private

// ~ Non-Static

// ~~ public
    public void Refresh() {
        enabled = true;
    }

    public bool AddHex(int index, Hex hex) {
        try {
            Hexes[index] = hex;
            hex.chunk = this;

    /* Set WorldPositionStays to false for both the hexes transform
    * and ui rect or they will not move initally to be oriented with
    * the chunk.
    */
            hex.transform.SetParent(transform, false);
            hex.uiRect.SetParent(_canvas.transform, false);
            return true;
        }
        catch (System.IndexOutOfRangeException) {
            RootLog.Log(
                "The specified hex " + hex + " could not be added to " +
                " the mesh chunk because the specified index " + index +
                " was outside the bounds of the chunks hex array."
            );
            return false;
        }
    }

/// <summary>
///     Switches the UI on and off for this chunk, enabling and disabling
///     features such as the distance from the currently selected hex hex.
/// </summary>
/// <param name="visible">
///     The visible state of the hex grid chunk.
/// </param>
    public void ShowUI(bool visible) {
        _canvas.gameObject.SetActive(visible);
    }

    public void Triangulate(
        HexMap hexMap,
        float hexOuterRadius,
        HexAdjacencyGraph neighborGraph,
        RiverDigraph riverGraph,
        RoadUndirectedGraph roadGraph,
        ElevationBidirectionalGraph elevationGraph
    ) {
        /*RootLog.Log(
            "Rendering hex grid: " + "\n" + hexMap.HexGrid + "\n" + neighborGraph,
            Severity.Information,
            "NeighborGraph.FromHexGrid"
        );*/
        terrain.Clear();
        rivers.Clear();
        roads.Clear();
        water.Clear();
        waterShore.Clear();
        estuaries.Clear();
        features.Clear();

        for(int i = 0; i < Hexes.Length; i++) {
            if (Hexes[i]) {
                TriangulateHex(
                    Hexes[i],
                    hexOuterRadius,
                    neighborGraph,
                    riverGraph,
                    roadGraph,
                    elevationGraph,
                    hexMap.WrapSize
                );
            }
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
    private void TriangulateHex(
        Hex hex,
        float hexOuterRadius,
        HexAdjacencyGraph neighborGraph,
        RiverDigraph riverGraph,
        RoadUndirectedGraph roadGraph,
        ElevationBidirectionalGraph elevationGraph,
        int wrapSize
    ) {
        _hexOuterRadius = hexOuterRadius;

        List<HexEdge> edges =
            neighborGraph.GetOutEdges(hex);        

        foreach(HexEdge edge in edges) {
            TriangulateEdge(
                hex,
                edge.Target,
                edge.Direction,
                hexOuterRadius,
                neighborGraph,
                riverGraph,
                roadGraph,
                elevationGraph,
                wrapSize
            );
        }

        foreach(
            HexDirections borderDirection in
            neighborGraph.GetBorderDirections(hex)
        ) {
            TriangulateEdge(
                hex,
                null,
                borderDirection,
                hexOuterRadius,
                neighborGraph,
                riverGraph,
                roadGraph,
                elevationGraph,
                wrapSize
            );
        }

        if (!hex.IsUnderwater) {
            if (
                    !riverGraph.HasRiver(hex) &&
                    !roadGraph.HasRoad(hex)
            ) {
                features.AddFeature(
                    hex,
                    hex.Position,
                    hexOuterRadius,
                    wrapSize
                );
            }

            if (hex.IsSpecial) {
                features.AddSpecialFeature(
                    hex,
                    hex.Position,
                    hexOuterRadius,
                    wrapSize
                );
            }
        }
    }

    private void TriangulateEdge(
        Hex source,
        Hex target,
        HexDirections direction,
        float hexOuterRadius,
        HexAdjacencyGraph neighborGraph,
        RiverDigraph riverGraph,
        RoadUndirectedGraph roadGraph,
        ElevationBidirectionalGraph elevationGraph,
        int wrapSize
    ) {
// Hex center
        Vector3 center = source.Position;

// Triangle edge.
        EdgeVertices edgeVertices = new EdgeVertices(
            center + HexagonPoint.GetFirstSolidCorner(
                direction,
                hexOuterRadius
            ),
            center +
            HexagonPoint.GetSecondSolidCorner(
                direction,
                hexOuterRadius
            )
        );

//        if (hex.HasRiver) {
        if (riverGraph.HasRiver(source)) {

// SHOULD ITERATE OVER EACH EDGE FOR THE FOLLOWING ALGORITHM
//            if (hex.HasRiverThroughEdge(direction)) {
            if (
                riverGraph.HasRiverInDirection(
                    source,
                    direction
                )
            ) {
/* If the triangle has a river through the edge, lower center edge vertex
*  to simulate stream bed.
*/
                edgeVertices.vertex3.y = source.StreamBedY;

//                if (hex.HasRiverBeginOrEnd) {
                if (riverGraph.HasRiverEnd(source)) {
                    TriangulateWithRiverBeginOrEnd(
                        source,
                        center,
                        edgeVertices,
                        neighborEdge,
                        riverGraph,
                        hexOuterRadius,
                        wrapSize
                    );
                }
                else {
                    TriangulateWithRiver(
                        source,
                        center,
                        edgeVertices,
                        neighborEdge,
                        riverGraph,
                        hexOuterRadius,
                        wrapSize
                    );
                }
            }
            else {
                TriangulateAdjacentToRiver(
                    source,
                    center,
                    edgeVertices,
                    neighborEdge,
                    roadGraph,
                    riverGraph,
                    hexOuterRadius,
                    wrapSize
                );
            }
        }
        else {
            TriangulateWithoutRiver(
                source,
                neighborEdge,
                edgeVertices,
                riverGraph,
                roadGraph,
                center,
                hexOuterRadius,
                wrapSize
            );

            if (
                !source.IsUnderwater &&
// SHOULD ITERATE THOURGH EACH EDGE FOR THIS ALGORITHM
//                !hex.HasRoadThroughEdge(direction)
                  !roadGraph.HasRoad(source)
            ) {
                features.AddFeature(
                    source,
                    (center + edgeVertices.vertex1 + edgeVertices.vertex5) * (1f / 3f),
                    hexOuterRadius,
                    wrapSize
                );
            }
        }

/* If the direction of triangulation is between NE and SE, triangulate
*  the connection between hexes. Otherwise, do not triangulate the connection.
*  Since the connections are triangulated from west to east, south to north,
*  the connection will already have been triangulated for SW, W, an NW.
*/
        if (neighborEdge.Direction <= HexDirections.Southeast) {
            TriangulateConnection(
                source,
                neighborEdge,
                neighborGraph,
                riverGraph,
                roadGraph,
                elevationGraph,
                edgeVertices,
                hexOuterRadius,
                wrapSize
            );
        }

        if (source.IsUnderwater) {
            TriangulateWater(
                source,
                neighborEdge,
                neighborGraph,
                riverGraph,
                center,
                hexOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateWithoutRiver(
        Hex hex,
        HexEdge neighborEdge,
        EdgeVertices edgeVertices,
        RiverDigraph riverGraph,
        RoadUndirectedGraph roadGraph,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize
    ) {
        TriangulateEdgeFan(
            center,
            edgeVertices,
            hex.Index,
            hexOuterRadius,
            wrapSize
        );

//        if (hex.HasRoads) {
          if (roadGraph.HasRoad(hex)) {
            Vector2 interpolators = GetRoadInterpolators(
                hex,
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
//                hex.HasRoadThroughEdge(direction),
                roadGraph.HasRoad(hex),
                hex.Index,
                hexOuterRadius,
                wrapSize
            );
        }
    }
    

    private void TriangulateWithRiver(
        Hex hex,
        Vector3 center,
        EdgeVertices edgeVertices,
        HexEdge neighborEdge,
        RiverDigraph riverGraph,
        float hexOuterRadius,
        int wrapSize
    ) {
        Vector3 centerLeft;
        Vector3 centerRight;

//        if (hex.HasRiverThroughEdge(direction.Opposite())) {
        if (
            riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction.Opposite()
            )
        ) {
/* Create a vertex 1/4th of the way from the center of the hex 
* to first solid corner of the previous edge, which is pointing
* straight "down" toward the bottom of the hexagon for a left facing
* edge.
*/
            centerLeft = center +
                HexagonPoint.GetFirstSolidCorner(
                    neighborEdge.Direction.PreviousClockwise(),
                    hexOuterRadius
                ) * 0.25f;

/* Create a vertex 1/4th of the way from the center of the hex
* to the second solid corner of the next edge, which is pointing
* straight "up" toward the top of the hexagon for a left facing edge.
*/
            centerRight = center +
                HexagonPoint.GetSecondSolidCorner(
                    neighborEdge.Direction.NextClockwise(),
                    hexOuterRadius
                ) * 0.25f;
        }

/* If the next direction has a sharp turn, there will be a river through
* direction.Next() or direction.Previous(). Must align center line with
* center line with edge between this river and the adjacent river.
* Interpolate with an increased step to account for the rotation
* of the center line.
*/
//        else if (hex.HasRiverThroughEdge(direction.Next())) {
        else if (
            riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction.NextClockwise()
            )
        ) {
            centerLeft = center;
            centerRight = 
                Vector3.Lerp(center, edgeVertices.vertex5, 2f / 3f);
        }
//        else if (hex.HasRiverThroughEdge(direction.Previous())) {
        else if (
            riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction.PreviousClockwise()
            )
        ) {
            centerLeft =
                Vector3.Lerp(center, edgeVertices.vertex1, 2f / 3f);
            centerRight = center;
        }

/* If the hex has a river two directions next, or two directions
* previous, there is a slight bend in the river. Need to push
* the center line to the inside of the bend. Using
* HexMetrics.innerToOuter to adjust for the fact that
* the midpoint of a solid edge is closer to the center
* of a hex than a solid edge corner.
*/
//        else if (hex.HasRiverThroughEdge(direction.Next2())) {
        else if (
            riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction.NextClockwise2()
            )
        ) {
            centerLeft = center;

            centerRight = 
                center + 
                HexagonPoint.GetSolidEdgeMiddle(
                    neighborEdge.Direction.NextClockwise(),
                    hexOuterRadius
                ) * (0.5f * HexagonConstants.INNER_TO_OUTER_RATIO);
        }
// Previous 2
        else {
            centerLeft = 
                center + 
                HexagonPoint.GetSolidEdgeMiddle(
                    neighborEdge.Direction.PreviousClockwise(),
                    hexOuterRadius
                ) * (0.5f * HexagonConstants.INNER_TO_OUTER_RATIO);

            centerRight = center;
        }

/* Get the final location of the center by averaging
* centerLeft and centerRight. For a straight through
* river this average is the same as the center
* of the hex. For a bend this moves the center
* appropriately. Otherwise, all points are the same
* and the center also remains at the center of the hex.
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
            hex.Index,
            edgeVertices,
            _weights1,
            hex.Index,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            centerLeft,
            middle.vertex1,
            middle.vertex2,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            centerLeft,
            center,
            middle.vertex2,
            middle.vertex3,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            center,
            centerRight,
            middle.vertex3,
            middle.vertex4,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            centerRight,
            middle.vertex4,
            middle.vertex5,
            hexOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = hex.Index;
        terrain.AddTriangleHexData(indices, _weights1);
        terrain.AddQuadHexData(indices, _weights1);
        terrain.AddQuadHexData(indices, _weights1);
        terrain.AddTriangleHexData(indices, _weights1);

        if (!hex.IsUnderwater) {
//            bool reversed = (hex.IncomingRiver == direction);
            bool reversed = riverGraph.HasIncomingRiverInDirection(
                hex,
                neighborEdge.Direction
            );

            TriangulateRiverQuad(
                centerLeft,
                centerRight,
                middle.
                vertex2,
                middle.vertex4,
                hex.RiverSurfaceY,
                0.4f,
                reversed,
                indices,
                hexOuterRadius,
                wrapSize
            );

            TriangulateRiverQuad(
                middle.vertex2,
                middle.vertex4,
                edgeVertices.vertex2,
                edgeVertices.vertex4,
                hex.RiverSurfaceY,
                0.6f,
                reversed,
                indices,
                hexOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateWithRiverBeginOrEnd(
        Hex hex,
        Vector3 center,
        EdgeVertices edgeVertices,
        HexEdge neighborEdge,
        RiverDigraph riverGraph,
        float hexOuterRadius,
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
            hex.Index,
            edgeVertices,
            _weights1,
            hex.Index,
            hexOuterRadius,
            wrapSize
        );

        TriangulateEdgeFan(
            center,
            middle,
            hex.Index,
            hexOuterRadius,
            wrapSize
        );

        if (!hex.IsUnderwater) {
//            bool reversed = hex.HasIncomingRiver;
            bool reversed = riverGraph.HasIncomingRiverInDirection(
                hex,
                neighborEdge.Direction
            );

            Vector3 indices;
            indices.x = indices.y = indices.z = hex.Index;
            
            TriangulateRiverQuad(
                middle.vertex2,
                middle.vertex4,
                edgeVertices.vertex2,
                edgeVertices.vertex4,
                hex.RiverSurfaceY,
                0.6f,
                reversed,
                indices,
                hexOuterRadius,
                wrapSize
            );

            center.y =
                middle.vertex2.y = middle.vertex4.y = hex.RiverSurfaceY;

            rivers.AddTriangle(
                center,
                middle.vertex2,
                middle.vertex4,
                hexOuterRadius,
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

            rivers.AddTriangleHexData(indices, _weights1);
        }
    }

    private void TriangulateAdjacentToRiver(
        Hex hex,
        Vector3 center,
        EdgeVertices edgeVertices,
        HexEdge neighborEdge,
        RoadUndirectedGraph roadGraph,
        RiverDigraph riverGraph,
        float hexOuterRadius,
        int wrapSize
    ) {
//        if (hex.HasRoads) {
        if (roadGraph.HasRoad(hex)) {
            TriangulateRoadAdjacentToRiver(
                hex,
                center,
                neighborEdge,
                riverGraph,
                roadGraph,
                edgeVertices,
                hexOuterRadius,
                wrapSize
            );
        }

//        if (hex.HasRiverThroughEdge(direction.Next())) {
        if (riverGraph.HasRiverInDirection(
            hex,
            neighborEdge.Direction
        )) {
/* If the direction has a river on either side, it has a slight curve. 
* The center vertex of river-adjacent triangle needs to be moved toward 
* the edge so they don't overlap the river.
*/
//            if (hex.HasRiverThroughEdge(direction.Previous())) {
            if (riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction.PreviousClockwise()
            )) {
                center += HexagonPoint.GetSolidEdgeMiddle(
                    neighborEdge.Direction,
                    hexOuterRadius
                ) * (HexagonConstants.INNER_TO_OUTER_RATIO * 0.5f);
            }

/* If the hex has a river through the previous previous direction,
* it has a river flowing through the hex. Move the center vertex
* of the river-adjacent triangle so that it does not overlap the river.
*/
            else if (
//                hex.HasRiverThroughEdge(direction.Previous2())
                riverGraph.HasRiverInDirection(
                    hex,
                    neighborEdge.Direction.PreviousClockwise2()
                )
            ) {
                center +=
                    HexagonPoint.GetFirstSolidCorner(
                        neighborEdge.Direction,
                        hexOuterRadius
                    ) * 0.25f;
            }
        }

/* Second case of straight-river-adjacent triangle. Need to move center
* so it doesn't overlap the river.
*/
        else if (
//            hex.HasRiverThroughEdge(direction.Previous()) &&
            riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction.PreviousClockwise()
            ) &&
//            hex.HasRiverThroughEdge(direction.Next2())
            riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction.NextClockwise2()
            )
        ) {
            center += HexagonPoint.GetSecondSolidCorner(
                neighborEdge.Direction,
                hexOuterRadius
            ) * 0.25f;
        }

        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(center, edgeVertices.vertex1, 0.5f),
            Vector3.Lerp(center, edgeVertices.vertex5, 0.5f)
        );

        TriangulateEdgeStrip(
            middle,
            _weights1,
            hex.Index,
            edgeVertices,
            _weights1,
            hex.Index,
            hexOuterRadius,
            wrapSize
        );

        TriangulateEdgeFan(
            center,
            middle,
            hex.Index,
            hexOuterRadius,
            wrapSize
        );

        if (
            !hex.IsUnderwater &&
//            !hex.HasRoadThroughEdge(direction)
            roadGraph.HasRoadInDirection(
                hex,
                neighborEdge.Direction
            )
        ) {
            features.AddFeature(
                hex,
                (center + edgeVertices.vertex1 + edgeVertices.vertex5) * (1f / 3f),
                hexOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateConnection(
        Hex hex,
        HexEdge neighborEdge,
        HexAdjacencyGraph neighborGraph,
        RiverDigraph riverGraph,
        RoadUndirectedGraph roadGraph,
        ElevationBidirectionalGraph elevationGraph,
        EdgeVertices edgeVertices1,
        float hexOuterRadius,
        int wrapSize
    ) {
//        hex neighbor = hex.GetNeighbor(direction);
        Hex neighbor = neighborGraph.TryGetNeighborInDirection(
            hex,
            neighborEdge.Direction
        );

/* Some hexes will not have neighbors. If this is the case, return out
* of the method.
*/
        if (neighbor == null) {
            return;
        }

        Vector3 bridge = HexagonPoint.GetBridge(
            neighborEdge.Direction, hexOuterRadius
        );

        bridge.y = neighbor.Position.y - hex.Position.y;

        EdgeVertices edge2 = new EdgeVertices(
            edgeVertices1.vertex1 + bridge,
            edgeVertices1.vertex5 + bridge
        );

//        bool hasRiver = hex.HasRiverThroughEdge(direction);
        bool hasRiver = riverGraph.HasRiverInDirection(
            hex,
            neighborEdge.Direction
        );

//        bool hasRoad = hex.HasRoadThroughEdge(direction);
        bool hasRoad = roadGraph.HasRoadInDirection(
            hex,
            neighborEdge.Direction
        );
        
/* Adjust the other edge of the connection
* if there is a river through that edge.
*/
        if (hasRiver) {
            edge2.vertex3.y = neighbor.StreamBedY;

            Vector3 indices;
            indices.x = indices.z = hex.Index;
            indices.y = neighbor.Index;

            if (!hex.IsUnderwater) {
                if (!neighbor.IsUnderwater) {
                    TriangulateRiverQuad(
                        edgeVertices1.vertex2,
                        edgeVertices1.vertex4,
                        edge2.vertex2,
                        edge2.vertex4,
                        hex.RiverSurfaceY,
                        neighbor.RiverSurfaceY,
                        0.8f,
                        (
//                            hex.HasIncomingRiver &&
//                            hex.IncomingRiver == direction
                              riverGraph.HasIncomingRiverInDirection(
                                  hex,
                                  neighborEdge.Direction
                              )
                        ),
                        indices,
                        hexOuterRadius,
                        wrapSize
                    );
                }
                else if(hex.Elevation > neighbor.WaterLevel) {
                    TriangulateWaterfallInWater(
                        edgeVertices1.vertex2, edgeVertices1.vertex4, 
                        edge2.vertex2, edge2.vertex4, 
                        hex.RiverSurfaceY, 
                        neighbor.RiverSurfaceY,
                        neighbor.WaterSurfaceY,
                        indices,
                        hexOuterRadius,
                        wrapSize
                    );
                }
            }
            else if (
                !neighbor.IsUnderwater &&
                neighbor.Elevation > hex.WaterLevel
            ) {
                TriangulateWaterfallInWater(
                    edge2.vertex4,
                    edge2.vertex2,
                    edgeVertices1.vertex4,
                    edgeVertices1.vertex2,
                    neighbor.RiverSurfaceY,
                    hex.RiverSurfaceY,
                    hex.WaterSurfaceY,
                    indices,
                    hexOuterRadius,
                    wrapSize
                );
            }
        }

        if (
//            hex.GetEdgeType(direction) == ElevationEdgeTypes.Slope
            elevationGraph.GetEdgeTypeInDirection(
                hex,
                neighborEdge.Direction
            ) == ElevationEdgeTypes.Slope
        ) {
            TriangulateEdgeTerraces(
                edgeVertices1, 
                hex, 
                edge2, 
                neighbor,
                hasRoad,
                hexOuterRadius,
                wrapSize
            );
        }
        else {
            TriangulateEdgeStrip(
                edgeVertices1,
                _weights1,
                hex.Index,
                edge2,
                _weights2,
                neighbor.Index,
                hexOuterRadius,
                wrapSize, 
                hasRoad
            );
        }

        features.AddWall(
            edgeVertices1,
            hex,
            edge2,
            neighbor,
            hasRiver,
            hasRoad,
            hexOuterRadius,
            wrapSize
        );

/* Drawn and color the triangle between the bridge of the current hex
* and its current neighbor and the bridge of the next hex and the
* current neighbor.*/
//        hex nextNeighbor = hex.GetNeighbor(direction.Next());
        Hex nextNeighbor = neighborGraph.TryGetNeighborInDirection(
            hex,
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
                    hexOuterRadius
                );

            vertex5.y = nextNeighbor.Position.y;

            if (hex.Elevation <= neighbor.Elevation) {
                if (hex.Elevation <= nextNeighbor.Elevation) {

//This hex has lowest elevation, no rotation.
                    TriangulateCorner(
                        edgeVertices1.vertex5,
                        hex,
                        edge2.vertex5,
                        neighbor,
                        vertex5,
                        nextNeighbor,
                        hexOuterRadius,
                        wrapSize
                    );
                }
                else {
// Next neighbor has lowest elevation, rotate counter-clockwise.
                    TriangulateCorner(
                        vertex5,
                        nextNeighbor,
                        edgeVertices1.vertex5,
                        hex,
                        edge2.vertex5,
                        neighbor,
                        hexOuterRadius,
                        wrapSize
                    );
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation) {
// Neighbor is lowest hex, rotate triangle clockwise.
                TriangulateCorner(
                    edge2.vertex5,
                    neighbor,
                    vertex5,
                    nextNeighbor,
                    edgeVertices1.vertex5,
                    hex,
                    hexOuterRadius,
                    wrapSize
                );
            }
            else {

// Next neighbor has lowest elevation, rotate counter-clockwise.
                TriangulateCorner(
                    vertex5,
                    nextNeighbor,
                    edgeVertices1.vertex5,
                    hex,
                    edge2.vertex5,
                    neighbor,
                    hexOuterRadius,
                    wrapSize
                );
            }
        }
    }

    private void TriangulateEdgeTerraces(
        EdgeVertices begin,
        Hex beginHex,
        EdgeVertices end,
        Hex endHex,
        bool hasRoad,
        float hexOuterRadius,
        int wrapSize
    ) {
        EdgeVertices edge2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color weight2 = HexagonPoint.TerraceLerp(_weights1, _weights2, 1);
        float index1 = beginHex.Index;
        float index2 = endHex.Index;

        TriangulateEdgeStrip
        (
            begin,
            _weights1, 
            index1, 
            edge2, 
            weight2, 
            index2,
            hexOuterRadius,
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
                hexOuterRadius,
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
            hexOuterRadius,
            wrapSize,
            hasRoad
        );
    }

    private void TriangulateCorner(
        Vector3 bottom,
        Hex bottomHex,
        Vector3 left,
        Hex leftHex,
        Vector3 right,
        Hex rightHex,
        float hexOuterRadius,
        int wrapSize
    ) {
        ElevationEdgeTypes leftEdgeType = bottomHex.GetEdgeType(leftHex);
        ElevationEdgeTypes rightEdgeType = bottomHex.GetEdgeType(rightHex);

        if (leftEdgeType == ElevationEdgeTypes.Slope) {
            if (rightEdgeType == ElevationEdgeTypes.Slope) {

// Corner is also a terrace. Slope-Slope-Flat.
                TriangulateCornerTerraces(
                    bottom,
                    bottomHex,
                    left,
                    leftHex,
                    right,
                    rightHex,
                    hexOuterRadius,
                    wrapSize
                );
            }

// If the right edge is flat, must terrace from left instead of bottom. Slope-Flat-Slope
            else if (rightEdgeType == ElevationEdgeTypes.Flat) {
                TriangulateCornerTerraces (
                    left,
                    leftHex,
                    right,
                    rightHex,
                    bottom,
                    bottomHex,
                    hexOuterRadius,
                    wrapSize
                );
            }
            else {

/* At least one edge is a cliff. Slope-Cliff-Slope or Slope-Cliff-Cliff. Standard case
* because slope on left and flat on right.
*/
                TriangulateCornerTerracesCliff (
                    bottom,
                    bottomHex,
                    left,
                    leftHex,
                    right,
                    rightHex,
                    hexOuterRadius,
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
                    rightHex,
                    bottom,
                    bottomHex,
                    left,
                    leftHex,
                    hexOuterRadius,
                    wrapSize
                );
            }
            else {

/* At least one edge is a cliff. Slope-Cliff-Slope or Slope-Cliff-Cliff. Mirror case because
* slope on right and flat on left.
*/
                TriangulateCornerCliffTerraces(
                    bottom,
                    bottomHex,
                    left,
                    leftHex,
                    right,
                    rightHex,
                    hexOuterRadius,
                    wrapSize
                );
            }
        }

/* Neither the left or right hex edge type is a slope. If the right hex type of the left hex
* is a slope, then terraces must be calculated for a corner between two cliff edges.
* Cliff-Cliff-Slope Right, or Cliff-Cliff-Slope Left.
*/
        else if (leftHex.GetEdgeType(rightHex) == ElevationEdgeTypes.Slope) {

// If Cliff-Cliff-Slope-Left
            if (leftHex.Elevation < rightHex.Elevation) {
                TriangulateCornerCliffTerraces(
                    right,
                    rightHex,
                    bottom,
                    bottomHex,
                    left,
                    leftHex,
                    hexOuterRadius,
                    wrapSize
                );
            }

// If Cliff-Cliff-Slope-Right
            else {
                TriangulateCornerTerracesCliff(
                    left,
                    leftHex,
                    right,
                    rightHex,
                    bottom,
                    bottomHex,
                    hexOuterRadius,
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
                hexOuterRadius,
                wrapSize
            );

            Vector3 indices;
            indices.x = bottomHex.Index;
            indices.y = leftHex.Index;
            indices.z = rightHex.Index;

            terrain.AddTriangleHexData(
                indices,
                _weights1,
                _weights2,
                _weights3
            );
        }

        features.AddWall(
            bottom,
            bottomHex,
            left,
            leftHex,
            right,
            rightHex,
            hexOuterRadius,
            wrapSize
        );
    }

    private void TriangulateCornerTerraces(
        Vector3 begin,
        Hex beginHex,
        Vector3 left,
        Hex leftHex,
        Vector3 right,
        Hex rightHex,
        float hexOuterRadius,
        int wrapSize
    ) {
        Vector3 vertex3 = HexagonPoint.TerraceLerp(begin, left, 1);
        Vector3 vertex4 = HexagonPoint.TerraceLerp(begin, right, 1);
        Color weight3 = HexagonPoint.TerraceLerp(_weights1, _weights2, 1);
        Color weight4 = HexagonPoint.TerraceLerp(_weights1, _weights3, 1);

        Vector3 indices;

        indices.x = beginHex.Index;
        indices.y = leftHex.Index;
        indices.z = rightHex.Index;

        terrain.AddTriangle(
            begin,
            vertex3,
            vertex4,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddTriangleHexData(
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
                hexOuterRadius,
                wrapSize
            );
            
            terrain.AddQuadHexData(
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
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuadHexData(
            indices,
            weight3,
            weight4,
            _weights2,
            _weights3
        );
    }

    private void TriangulateCornerTerracesCliff(
        Vector3 begin,
        Hex beginHex,
        Vector3 left,
        Hex leftHex,
        Vector3 right,
        Hex rightHex,
        float hexOuterRadius,
        int wrapSize
    ) {
/* Set boundary distance to 1 elevation level above the bottom-most hex
* in the case.
*/
        float boundaryDistance =
            1f / (rightHex.Elevation - beginHex.Elevation);

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
                    hexOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    right,
                    hexOuterRadius,
                    wrapSize
                ),
                boundaryDistance
            );

        Color boundaryWeights =
            Color.Lerp(_weights1, _weights2, boundaryDistance);

        Vector3 indices;

        indices.x = beginHex.Index;
        indices.y = leftHex.Index;
        indices.z = rightHex.Index;

        TriangulateBoundaryTriangle (
            begin,
            _weights1,
            left,
            _weights2,
            boundary,
            boundaryWeights,
            indices,
            hexOuterRadius,
            wrapSize
        );

// Slope-Cliff-Slope. Triangulate a slope.
        if (leftHex.GetEdgeType(rightHex) == ElevationEdgeTypes.Slope) {
            TriangulateBoundaryTriangle (
                left, 
                _weights2,
                right, 
                _weights3,
                boundary, 
                boundaryWeights,
                indices,
                hexOuterRadius,
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
                    hexOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    right,
                    hexOuterRadius,
                    wrapSize
                ),
                boundary
            );

            terrain.AddTriangleHexData(
                indices,
                _weights2,
                _weights3,
                boundaryWeights
            );
        }
    }

    private void TriangulateCornerCliffTerraces(
        Vector3 begin,
        Hex beginHex,
        Vector3 left,
        Hex leftHex,
        Vector3 right,
        Hex rightHex,
        float hexOuterRadius,
        int wrapSize
    ) {
/* Set boundary distance to 1 elevation level above the bottom-most hex
* in the case.
*/
        float boundaryDistance =
            1f / (leftHex.Elevation - beginHex.Elevation);

// If boundary distance becomes negative, CCSR and CCSL case will have strange behavior.
        if (boundaryDistance < 0) {
            boundaryDistance = -boundaryDistance;
        }

// Must interpolate between the perturbed points, not the original points.
        Vector3 boundary = 
            Vector3.Lerp(
                HexagonPoint.Perturb(
                    begin,
                    hexOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    left,
                    hexOuterRadius,
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

        indices.x = beginHex.Index;
        indices.y = leftHex.Index;
        indices.z = rightHex.Index;

        TriangulateBoundaryTriangle(
            right, 
            _weights3,
            begin,
            _weights1,
            boundary,
            boundaryWeights,
            indices,
            hexOuterRadius,
            wrapSize
        );

// Slope-Cliff-Slope. Triangulate a slope.
        if (leftHex.GetEdgeType(rightHex) == ElevationEdgeTypes.Slope) {
            TriangulateBoundaryTriangle(
                left, 
                _weights2,
                right, 
                _weights3,
                boundary, 
                boundaryWeights,
                indices,
                hexOuterRadius,
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
                    hexOuterRadius,
                    wrapSize
                ),
                HexagonPoint.Perturb(
                    right,
                    hexOuterRadius,
                    wrapSize
                ),
                boundary
            );

            terrain.AddTriangleHexData(
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
        float hexOuterRadius,
        int wrapSize
    ) {

/* Immediately perturb vertex 2 as an optimization since it is not
* being used to derive any other point.
*/
        Vector3 vertex2 = 
            HexagonPoint.Perturb(
                HexagonPoint.TerraceLerp(begin, left, 1),
                hexOuterRadius,
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
                hexOuterRadius,
                wrapSize
            ),
            vertex2,
            boundary
        );

        terrain.AddTriangleHexData(indices, beginWeights, weight2, boundaryWeights);

        for (int i = 2; i < HexagonPoint.terraceSteps; i++) {

/* vertex2 has already been perturbed, need not pertub
* vertex1 as it is derived from vertex2.
*/
            Vector3 vertex1 = vertex2;
            Color weight1 = weight2;

            vertex2 = HexagonPoint.Perturb(
                HexagonPoint.TerraceLerp(begin, left, i),
                hexOuterRadius,
                wrapSize
            );

            weight2 = HexagonPoint.TerraceLerp(
                beginWeights,
                leftWeights,
                i
            );

            terrain.AddTriangleUnperturbed(vertex1, vertex2, boundary);

            terrain.AddTriangleHexData(
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
                hexOuterRadius,
                wrapSize
            ),
            boundary
        );
        terrain.AddTriangleHexData(indices, weight2, leftWeights, boundaryWeights);
    }

    private void TriangulateEdgeFan(
        Vector3 center,
        EdgeVertices edge,
        float index,
        float hexOuterRadius,
        int wrapSize
    ) {
        terrain.AddTriangle(
            center,
            edge.vertex1,
            edge.vertex2,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            center,
            edge.vertex2,
            edge.vertex3,
            hexOuterRadius,
            wrapSize
        );
        
        terrain.AddTriangle(
            center,
            edge.vertex3,
            edge.vertex4,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddTriangle(
            center,
            edge.vertex4,
            edge.vertex5,
            hexOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        terrain.AddTriangleHexData(indices, _weights1);
        terrain.AddTriangleHexData(indices, _weights1);
        terrain.AddTriangleHexData(indices, _weights1);
        terrain.AddTriangleHexData(indices, _weights1);
    }

    private void TriangulateEdgeStrip(
        EdgeVertices edge1,
        Color weight1,
        float index1,
        EdgeVertices edge2,
        Color weight2,
        float index2,
        float hexOuterRadius,
        int wrapSize,
        bool hasRoad = false
    ) {
        terrain.AddQuad(
            edge1.vertex1,
            edge1.vertex2,
            edge2.vertex1,
            edge2.vertex2,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            edge1.vertex2,
            edge1.vertex3,
            edge2.vertex2,
            edge2.vertex3,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            edge1.vertex3,
            edge1.vertex4,
            edge2.vertex3,
            edge2.vertex4,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuad(
            edge1.vertex4,
            edge1.vertex5,
            edge2.vertex4,
            edge2.vertex5,
            hexOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.z = index1;
        indices.y = index2;

        terrain.AddQuadHexData(indices, weight1, weight2);
        terrain.AddQuadHexData(indices, weight1, weight2);
        terrain.AddQuadHexData(indices, weight1, weight2);
        terrain.AddQuadHexData(indices, weight1, weight2);

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
                hexOuterRadius,
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
        float hexOuterRadius,
        int wrapSize
    ) {
        vertex1.y = vertex2.y = y1;
        vertex3.y = vertex4.y = y2;

        rivers.AddQuad(
            vertex1,
            vertex2,
            vertex3,
            vertex4,
            hexOuterRadius,
            wrapSize
        );

        if (reversed) {
            rivers.AddQuadUV(1f, 0f, 0.8f - v, 0.6f - v);
        }
        else {
            rivers.AddQuadUV(0f, 1f, v, v + 0.2f);
        }

        rivers.AddQuadHexData(indices, _weights1, _weights2);
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
        float hexOuterRadius,
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
            hexOuterRadius,
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
        float hexOuterRadius,
        int wrapSize
    ) {
        roads.AddQuad(
            vertex1,
            vertex2,
            vertex4,
            vertex5,
            hexOuterRadius,
            wrapSize
        );

        roads.AddQuad(
            vertex2,
            vertex3,
            vertex5,
            vertex6,
            hexOuterRadius,
            wrapSize
        );

        roads.AddQuadUV(0f, 1f, 0f, 0f);
        roads.AddQuadUV(1f, 0f, 0f, 0f);

        roads.AddQuadHexData(indices, weight1, weight2);
        roads.AddQuadHexData(indices, weight1, weight2);
    }

    private void TriangulateRoadEdge(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight,
        float index,
        float hexOuterRadius,
        int wrapSize
    ) {
        roads.AddTriangle(
            center,
            middleLeft,
            middleRight,
            hexOuterRadius,
            wrapSize
        );

        roads.AddTriangleUV(
            new Vector2(1f, 0f), 
            new Vector2(0f, 0f), 
            new Vector2(0f, 0f)
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = index;
        roads.AddTriangleHexData(indices, _weights1);
    }

    private void TriangulateRoadAdjacentToRiver(
        Hex hex, 
        Vector3 center,
        HexEdge neighborEdge,
        RiverDigraph riverGraph,
        RoadUndirectedGraph roadGraph,
        EdgeVertices edgeVertices,
        float hexOuterRadius,
        int wrapSize
    ) {
//        bool hasRoadThroughEdge = hex.HasRoadThroughEdge(direction);
        bool hasRoadThroughEdge = roadGraph.HasRoadInDirection(
            hex,
            neighborEdge.Direction
        );

//          bool previousHasRiver = hex.HasRiverThroughEdge(
//              direction.Previous()
//          );
        bool previousHasRiver = riverGraph.HasRiverInDirection(
            hex,
            neighborEdge.Direction.PreviousClockwise()
        );

//        bool nextHasRiver = hex.HasRiverThroughEdge(direction.Next());
        bool nextHasRiver = riverGraph.HasIncomingRiverInDirection(
            hex,
            neighborEdge.Direction.NextClockwise()
        );

        Vector2 interpolators = GetRoadInterpolators(
            hex,
            neighborEdge,
            roadGraph
        );

        Vector3 roadCenter = center;

//        if (hex.HasRiverBeginOrEnd) {
        if (riverGraph.HasRiverStartOrEnd(hex)) {
            roadCenter += 
                HexagonPoint.GetSolidEdgeMiddle(
//                    hex.RiverBeginOrEndDirection.Opposite(),
                    riverGraph.RiverStartOrEndDirection(hex).Opposite(),
                    hexOuterRadius
                ) * 
                (1f / 3f);
        }
//        else if(hex.IncomingRiver == hex.OutgoingRiver.Opposite()) {
        else if (
            riverGraph.HasStraightRiver(hex)
        ) {
            Vector3 corner;

//  If the previous hex has a river, the corner the center will be moved
//  toward is equal to the current direction + 1.
            if (previousHasRiver) {
                if (
                    !hasRoadThroughEdge &&
//                    !hex.HasRoadThroughEdge(direction.Next())
                    !roadGraph.HasRoadInDirection(
                        hex,
                        neighborEdge.Direction.NextClockwise()
                    )
                ) {
                    return;
                }
                corner = HexagonPoint.GetSecondSolidCorner(
                    neighborEdge.Direction,
                    hexOuterRadius
                );
            }
// If the previous hex does not have a river, the corner the center will
// be moved toward is the same index as the current direction.
            else {
                if (
                    !hasRoadThroughEdge &&
//                    !hex.HasRoadThroughEdge(direction.Previous())
                    !roadGraph.HasRoadInDirection(
                        hex,
                        neighborEdge.Direction.PreviousClockwise()
                    )
                ) {
                    return;
                }

                corner = HexagonPoint.GetFirstSolidCorner(
                    neighborEdge.Direction,
                    hexOuterRadius
                );
            }
/* Using the example of a river flowing from east to west or west to east, for all cases
* this will result in the river being pushed either directly "up" north away from the
* river or directly "down" south away from the river.
*/
            roadCenter += corner * 0.5f;

            if (
//                hex.IncomingRiver == direction.Next() && 
                riverGraph.IncomingRiverDirections(hex)[0] ==
                neighborEdge.Direction.NextClockwise() &&
//                hex.HasRoadThroughEdge(direction.Next2()) ||
                roadGraph.HasRoadInDirection(
                    hex,
                    neighborEdge.Direction.NextClockwise2()
                ) ||
//                hex.HasRoadThroughEdge(direction.Opposite())
                roadGraph.HasRoadInDirection(
                    hex,
                    neighborEdge.Direction.Opposite()
                )
            ) {
                features.AddBridge(
                    roadCenter,
                    center - corner * 0.5f,
                    hexOuterRadius,
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

//        else if (hex.IncomingRiver == hex.OutgoingRiver.Previous()) {
          else if (riverGraph.HasStraightRiver(hex)) {
            roadCenter -= HexagonPoint.GetSecondCorner(
//                hex.IncomingRiver,
                riverGraph.IncomingRiverDirections(hex)[0],
                hexOuterRadius
            ) * 0.2f;
        }
//        else if (hex.IncomingRiver == hex.OutgoingRiver.Next()) {
        else if (riverGraph.HasClockwiseCornerRiver(hex)) {
            roadCenter -= HexagonPoint.GetFirstCorner(
//                hex.IncomingRiver,
                riverGraph.IncomingRiverDirections(hex)[0],
                hexOuterRadius
            ) * 0.2f;
        }

// If there is a river on the previous and next edges, the river has a
// slight bend. Need to pull the road center toward the current hex edge,
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
                    hexOuterRadius
                ) *
                HexagonConstants.INNER_TO_OUTER_RATIO;
            
            roadCenter += offset * 0.7f;
            center += offset * 0.5f;
        }

// The only remaining case is that the hex lies on the outside of a
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

// If there is no road through any of the hexes on the outer side of the
// river bend, then the road center need not move and should instead be
// pruned.
            if (
//                !hex.HasRoadThroughEdge(middle) &&
                !roadGraph.HasRoadInDirection(
                    hex,
                    middle
                ) &&   
//                !hex.HasRoadThroughEdge(middle.Previous()) &&
                !roadGraph.HasRoadInDirection(
                    hex,
                    middle.PreviousClockwise()
                ) &&
//                !hex.HasRoadThroughEdge(middle.Next())
                !roadGraph.HasRoadInDirection(
                    hex,
                    middle.NextClockwise()
                )
            ) {
                return;
            }

            Vector3 offset = HexagonPoint.GetSolidEdgeMiddle(middle, hexOuterRadius);
            roadCenter += offset * 0.25f;

            if (
                neighborEdge.Direction == middle &&
//                hex.HasRoadThroughEdge(direction.Opposite())
                roadGraph.HasRoadInDirection(
                    hex,
                    neighborEdge.Direction.Opposite()
                )
            ) {
                features.AddBridge (
                    roadCenter,
                    center - offset * (HexagonConstants.INNER_TO_OUTER_RATIO * 0.7f),
                    hexOuterRadius,
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
            hex.Index,
            hexOuterRadius,
            wrapSize
        );

        if (previousHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                center,
                middleLeft,
                hex.Index,
                hexOuterRadius,
                wrapSize
            );
        }

        if (nextHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                middleRight,
                center,
                hex.Index,
                hexOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateRoad(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight, 
        EdgeVertices edge,
        bool hasRoadThroughHexEdge,
        float index,
        float hexOuterRadius,
        int wrapSize
    ) {
        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        if (hasRoadThroughHexEdge) {
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
                hexOuterRadius,
                wrapSize
            );

            roads.AddTriangle(
                center,
                middleLeft,
                middleCenter,
                hexOuterRadius,
                wrapSize
            );
            
            roads.AddTriangle(
                center,
                middleCenter,
                middleRight,
                hexOuterRadius,
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

            roads.AddTriangleHexData(indices, _weights1);
            roads.AddTriangleHexData(indices, _weights1);
        }
        else {
            TriangulateRoadEdge(
                center,
                middleLeft,
                middleRight,
                index,
                hexOuterRadius,
                wrapSize
            );
        }

    }

    private Vector2 GetRoadInterpolators(
        Hex hex,
        HexEdge neighborEdge,
        RoadUndirectedGraph roadGraph
    ) {
        Vector2 interpolators;

//        if (hex.HasRoadThroughEdge(direction)) {
        if (
            roadGraph.HasRoadInDirection(
                hex,
                neighborEdge.Direction
            )
        ) {
            interpolators.x = interpolators.y = 0.5f;
        }
        else {
            interpolators.x =
//              hex.HasRoadThroughEdge(direction.Previous()) ?
//                  0.5f : 0.25f;
                roadGraph.HasRoadInDirection(
                    hex,
                    neighborEdge.Direction.PreviousClockwise()
                ) ?
                0.5f : 0.25f;
            
            interpolators.y =
//              hex.HasRoadThroughEdge(direction.Next()) ?
//                  0.5f : 0.25f;
                roadGraph.HasRoadInDirection(
                    hex,
                    neighborEdge.Direction.NextClockwise()
                ) ?
                0.5f : 0.25f;
        }

        return interpolators;
    }

    private void TriangulateWater(
        Hex hex,
        HexEdge neighborEdge,
        HexAdjacencyGraph neighborGraph,
        RiverDigraph riverGraph,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize
    ) {
        center.y = hex.WaterSurfaceY;

        if (
            neighborEdge.Target != null &&
            !neighborEdge.Target.IsUnderwater
        ) {
            TriangulateWaterShore(
                hex,
                neighborEdge,
                neighborGraph,
                riverGraph,
                center,
                hexOuterRadius,
                wrapSize
            );
        }
        else {
            TriangulateOpenWater(
                hex,
                neighborEdge,
                neighborGraph,
                riverGraph,
                center,
                hexOuterRadius,
                wrapSize
            );
        }            
    }

    private void TriangulateOpenWater(
        Hex hex,
        HexEdge neighborEdge,
        HexAdjacencyGraph neighborGraph,
        RiverDigraph riverGraph,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize
    ) { 
        Vector3 center1 =
            center +
            HexagonPoint.GetFirstWaterCorner(
                neighborEdge.Direction,
                hexOuterRadius
            );

        Vector3 center2 =
            center +
            HexagonPoint.GetSecondWaterCorner(
                neighborEdge.Direction,
                hexOuterRadius
            );

        water.AddTriangle(
            center,
            center1,
            center2,
            hexOuterRadius,
            wrapSize
        );

        Vector3 indices;
        indices.x = indices.y = indices.z = hex.Index;
        water.AddTriangleHexData(indices, _weights1);

        if (
            neighborEdge.Direction <= HexDirections.Southeast && 
            neighborEdge.Target != null
        ) {
            Vector3 bridge = HexagonPoint.GetWaterBridge(
                neighborEdge.Direction,
                hexOuterRadius
            );

            Vector3 edge1 = center1 + bridge;
            Vector3 edge2 = center2 + bridge;

            water.AddQuad(
                center1,
                center2,
                edge1,
                edge2,
                hexOuterRadius,
                wrapSize
            );
            
            indices.y = neighborEdge.Target.Index;
            water.AddQuadHexData(indices, _weights1, _weights2);

            if (neighborEdge.Direction <= HexDirections.East) {
                Hex nextNeighbor =
//                    hex.GetNeighbor(direction.NextClockwise());
                    neighborGraph.TryGetNeighborInDirection(
                        hex,
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
                        hexOuterRadius
                    ),
                    hexOuterRadius,
                    wrapSize
                );

                indices.z = nextNeighbor.Index;

                water.AddTriangleHexData(
                    indices, _weights1, _weights2, _weights3
                );
            }
        }
    }

    private void TriangulateWaterShore(
        Hex hex,
        HexEdge neighborEdge,
        HexAdjacencyGraph neighborGraph,
        RiverDigraph riverGraph,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize
    ) {
        EdgeVertices edge1 = new EdgeVertices(
            center + HexagonPoint.GetFirstWaterCorner(
                neighborEdge.Direction,
                hexOuterRadius
            ),
            center + HexagonPoint.GetSecondWaterCorner(
                neighborEdge.Direction,
                hexOuterRadius
            )
        );

        water.AddTriangle(
            center,
            edge1.vertex1,
            edge1.vertex2,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTriangle(
            center,
            edge1.vertex2,
            edge1.vertex3,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTriangle(
            center,
            edge1.vertex3,
            edge1.vertex4,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTriangle(
            center,
            edge1.vertex4,
            edge1.vertex5,
            hexOuterRadius,
            wrapSize
        );

        Vector3 indices = new Vector3();
        indices.x = indices.y = hex.Index;
        indices.y = neighborEdge.Target.Index;

        water.AddTriangleHexData(indices, _weights1);
        water.AddTriangleHexData(indices, _weights1);
        water.AddTriangleHexData(indices, _weights1);
        water.AddTriangleHexData(indices, _weights1);

// Work backward from the solid shore to obtain the edge.
        Vector3 center2 = neighborEdge.Target.Position;

        float hexInnerRadius =
            HexagonPoint.OuterToInnerRadius(hexOuterRadius);
        float hexInnerDiameter = hexInnerRadius * 2f;

// If the neighbor outside the wrap boundaries, adjust accordingly.
        if (neighborEdge.Target.ColumnIndex < hex.ColumnIndex - 1) {
            center2.x += 
                wrapSize * hexInnerDiameter;
        }
        else if (neighborEdge.Target.ColumnIndex > hex.ColumnIndex + 1) {
            center2.x -=
                wrapSize * hexInnerDiameter;
        }

        center2.y = center.y;

        EdgeVertices edge2 = new EdgeVertices(
            center2 + HexagonPoint.GetSecondSolidCorner(
                neighborEdge.Direction.Opposite(),
                hexOuterRadius
            ),
            center2 + HexagonPoint.GetFirstSolidCorner(
                neighborEdge.Direction.Opposite(),
                hexOuterRadius
            )
        );

        if (
//          hex.HasRiverThroughEdge(direction)
            riverGraph.HasRiverInDirection(
                hex,
                neighborEdge.Direction
            )
        ) {
            TriangulateEstuary(
                edge1,
                edge2,
//                (hex.HasIncomingRiver &&
//                hex.IncomingRiver == direction),
                (
                    riverGraph.HasIncomingRiver(hex) &&
                    riverGraph.IncomingRiverDirections(hex)[0] ==
                    neighborEdge.Direction
                ),
                indices,
                hexOuterRadius,
                wrapSize
            );

        }
        else {
            waterShore.AddQuad(
                edge1.vertex1,
                edge1.vertex2,
                edge2.vertex1,
                edge2.vertex2,
                hexOuterRadius,
                wrapSize
            );

            waterShore.AddQuad(
                edge1.vertex2,
                edge1.vertex3,
                edge2.vertex2,
                edge2.vertex3,
                hexOuterRadius,
                wrapSize
            );

            waterShore.AddQuad(
                edge1.vertex3,
                edge1.vertex4,
                edge2.vertex3,
                edge2.vertex4,
                hexOuterRadius,
                wrapSize
            );

            waterShore.AddQuad(
                edge1.vertex4,
                edge1.vertex5,
                edge2.vertex4,
                edge2.vertex5,
                hexOuterRadius,
                wrapSize
            );

            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);
            waterShore.AddQuadUV(0f, 0f, 0f, 1f);

            waterShore.AddQuadHexData(indices, _weights1, _weights2);
            waterShore.AddQuadHexData(indices, _weights1, _weights2);
            waterShore.AddQuadHexData(indices, _weights1, _weights2);
            waterShore.AddQuadHexData(indices, _weights1, _weights2);
        }
        
        Hex nextNeighbor =
//            hex.GetNeighbor(direction.NextClockwise());
            neighborGraph.TryGetNeighborInDirection(
                hex,
                neighborEdge.Direction.NextClockwise()
            );

        if (nextNeighbor != null) {
            Vector3 center3 = nextNeighbor.Position;

            if (nextNeighbor.ColumnIndex < hex.ColumnIndex - 1) {
                center3.x += wrapSize * hexInnerDiameter;
            }
            else if (nextNeighbor.ColumnIndex > hex.ColumnIndex + 1) {
                center3.x -= wrapSize * hexInnerDiameter;
            }

// Work backward from the shore to obtain the triangle if the neighbor is
// underwater, otherwise obtain normal triangle.

            Vector3 vertex3 = 
                center3 + (
                    nextNeighbor.IsUnderwater ?
                    HexagonPoint.GetFirstWaterCorner(
                        neighborEdge.Direction.PreviousClockwise(),
                        hexOuterRadius
                    ) :
                    HexagonPoint.GetFirstSolidCorner(
                        neighborEdge.Direction.PreviousClockwise(),
                        hexOuterRadius
                    )
                );

            vertex3.y = center.y;

            waterShore.AddTriangle (
                edge1.vertex5,
                edge2.vertex5,
                vertex3,
                hexOuterRadius,
                wrapSize
            );

            indices.z = nextNeighbor.Index;

            waterShore.AddTriangleHexData (
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
        float hexOuterRadius,
        int wrapSize
    ) {
        waterShore.AddTriangle(
            edge2.vertex1,
            edge1.vertex2,
            edge1.vertex1,
            hexOuterRadius,
            wrapSize
        );

        waterShore.AddTriangle(
            edge2.vertex5,
            edge1.vertex5,
            edge1.vertex4,
            hexOuterRadius,
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

        waterShore.AddTriangleHexData(
            indices, 
            _weights2, 
            _weights1, 
            _weights1
        );

        waterShore.AddTriangleHexData(
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
            hexOuterRadius,
            wrapSize
        );

        estuaries.AddTriangle(
            edge1.vertex3, 
            edge2.vertex2, 
            edge2.vertex4,
            hexOuterRadius,
            wrapSize
        );

        estuaries.AddQuad(
            edge1.vertex3, 
            edge1.vertex4, 
            edge2.vertex4, 
            edge2.vertex5,
            hexOuterRadius,
            wrapSize
        );

        estuaries.AddQuadUV(
            new Vector2(0f, 1f), 
            new Vector2(0f, 0f), 
            new Vector2(1f, 1f), 
            new Vector2(0f, 0f)
        );

        estuaries.AddQuadHexData(
            indices, _weights2, _weights1, _weights2, _weights1
        );

        estuaries.AddTriangleHexData(indices, _weights1, _weights2, _weights2);
        estuaries.AddQuadHexData(indices, _weights1, _weights2);

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
        float hexOuterRadius,
        int wrapSize
    ) {
        vertex1.y = vertex2.y = y1;
        vertex3.y = vertex4.y = y2;
        
        vertex1 = HexagonPoint.Perturb(
            vertex1,
            hexOuterRadius,
            wrapSize
        );

        vertex2 = HexagonPoint.Perturb(
            vertex2,
            hexOuterRadius,
            wrapSize
        );

        vertex3 = HexagonPoint.Perturb(
            vertex3,
            hexOuterRadius,
            wrapSize
        );

        vertex4 = HexagonPoint.Perturb(
            vertex4,
            hexOuterRadius,
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
        rivers.AddQuadHexData(indices, _weights1, _weights2);
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
