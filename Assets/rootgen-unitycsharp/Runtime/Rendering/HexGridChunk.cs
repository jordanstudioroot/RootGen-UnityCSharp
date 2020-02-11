using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    private static Color _weights1 = new Color(1f, 0f, 0f);
    private static Color _weights2 = new Color(0f, 1f, 0f);
    private static Color _weights3 = new Color(0f, 0f, 1f);
    private HexCell[] _cells;
    private Canvas _canvas;

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
    private void LateUpdate() {
        Triangulate();
        enabled = false;
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
    public static HexGridChunk GetChunk () {
        GameObject resultObj = new GameObject("Hex Grid Chunk");
        HexGridChunk resultMono = resultObj.AddComponent<HexGridChunk>();
        resultMono.Cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
        GameObject resultCanvasObj = new GameObject("Hex Grid Chunk Canvas");
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

    public void ShowUI(bool visible) {
        _canvas.gameObject.SetActive(visible);
    }

    public void Triangulate() {
        terrain.Clear();
        rivers.Clear();
        roads.Clear();
        water.Clear();
        waterShore.Clear();
        estuaries.Clear();
        features.Clear();

        for (int i = 0; i < _cells.Length; i++) {
            Triangulate(_cells[i]);
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
    private void Triangulate(HexCell cell) {

// For all directions around a given cell:
        List<Vector3> result = new List<Vector3>();
        for (
            HexDirection direction = HexDirection.Northeast;
            direction <= HexDirection.Northwest;
            direction++
        ) {
            
// Draw and color the triangles of the cells mesh.
            TriangulateDirection(direction, cell);
        }

        if (!cell.IsUnderwater) {
            if (!cell.HasRiver && !cell.HasRoads) {
                features.AddFeature(cell, cell.Position);
            }

            if (cell.IsSpecial) {
                features.AddSpecialFeature(cell, cell.Position);
            }
        }
    }

    private void TriangulateDirection(HexDirection direction, HexCell cell) {
// Cell center
        Vector3 center = cell.Position;

// Triangle edge.
        EdgeVertices edge = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        if (cell.HasRiver) {
            if (cell.HasRiverThroughEdge(direction)) {

/* If the triangle has a river through the edge, lower center edge vertex
*  to simulate stream bed.
*/
                edge.vertex3.y = cell.StreamBedY;

                if (cell.HasRiverBeginOrEnd) {
                    TriangulateWithRiverBeginOrEnd(
                        direction,
                        cell,
                        center,
                        edge
                    );
                }
                else {
                    TriangulateWithRiver(
                        direction,
                        cell,
                        center,
                        edge
                    );
                }
            }
            else {
                TriangulateAdjacentToRiver(
                    direction,
                    cell,
                    center,
                    edge
                );
            }
        }
        else {
            TriangulateWithoutRiver(
                direction,
                cell,
                center,
                edge
            );

            if (
                !cell.IsUnderwater &&
                !cell.HasRoadThroughEdge(direction)
            ) {
                features.AddFeature(
                    cell,
                    (center + edge.vertex1 + edge.vertex5) * (1f / 3f)
                );
            }
        }

/* If the direction of triangulation is between NE and SE, triangulate
*  the connection between cells. Otherwise, do not triangulate the connection.
*  Since the connections are triangulated from west to east, south to north,
*  the connection will already have been triangulated for SW, W, an NW.
*/
        if (direction <= HexDirection.SouthEast) {
            TriangulateConnection(direction, cell, edge);
        }

        if (cell.IsUnderwater) {
            TriangulateWater(direction, cell, center);
        }
    }

    private void TriangulateWithoutRiver(
        HexDirection direction, 
        HexCell cell, 
        Vector3 center, 
        EdgeVertices edge
    ) {
        TriangulateEdgeFan(center, edge, cell.Index);

        if (cell.HasRoads) {
            Vector2 interpolators = GetRoadInterpolators(direction, cell);

            TriangulateRoad(
                center,
                Vector3.Lerp(
                    center,
                    edge.vertex1,
                    interpolators.x
                ),
                Vector3.Lerp(
                    center,
                    edge.vertex5,
                    interpolators.y
                ),
                edge,
                cell.HasRoadThroughEdge(direction),
                cell.Index
            );
        }
    }

    private void TriangulateWithRiver(
        HexDirection direction,
        HexCell cell,
        Vector3 center,
        EdgeVertices edge
    ) {
        Vector3 centerLeft;
        Vector3 centerRight;

        if (cell.HasRiverThroughEdge(direction.Opposite())) {

/* Create a vertex 1/4th of the way from the center of the cell 
* to first solid corner of the previous edge, which is pointing
* straight "down" toward the bottom of the hexagon for a left facing
* edge.
*/
            centerLeft = center +
                HexMetrics.GetFirstSolidCorner(
                    direction.Previous()
                ) * 0.25f;

/* Create a vertex 1/4th of the way from the center of the cell
* to the second solid corner of the next edge, which is pointing
* straight "up" toward the top of the hexagon for a left facing edge.
*/
            centerRight = center +
                HexMetrics.GetSecondSolidCorner(
                    direction.Next()
                ) * 0.25f;
        }

/* If the next direction has a sharp turn, there will be a river through
* direction.Next() or direction.Previous(). Must align center line with
* center line with edge between this river and the adjacent river.
* Interpolate with an increased step to account for the rotation
* of the center line.
*/
        else if (cell.HasRiverThroughEdge(direction.Next())) {
            centerLeft = center;
            centerRight = 
                Vector3.Lerp(center, edge.vertex5, 2f / 3f);
        }
        else if (cell.HasRiverThroughEdge(direction.Previous())) {
            centerLeft =
                Vector3.Lerp(center, edge.vertex1, 2f / 3f);
            centerRight = center;
        }

/* If the cell has a river two directions next, or two directions
* previous, there is a slight bend in the river. Need to push
* the center line to the inside of the bend. Using
* HexMetrics.innerToOuter to adjust for the fact that
* the midpoint of a solid edge is closer to the center
* of a cell than a solid edge corner.
*/
        else if (cell.HasRiverThroughEdge(direction.Next2())) {
            centerLeft = center;

            centerRight = 
                center + 
                HexMetrics.GetSolidEdgeMiddle(direction.Next()) *
                (0.5f * HexMetrics.innerToOuter);
        }
        else {
            centerLeft = 
                center + 
                HexMetrics.GetSolidEdgeMiddle(direction.Previous()) *
                (0.5f * HexMetrics.innerToOuter);

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
            Vector3.Lerp(centerLeft, edge.vertex1, 0.5f),
            Vector3.Lerp(centerRight, edge.vertex5, 0.5f),
            1f / 6f
        );

/* Adjust the height of middle of the middle edge,
* as well as the height of the center of the hexagon, to 
* the height of the middle of the outer edge of the 
* hexagon. The given edge of the hexagon has already 
* been adjusted to the height of the river bed.
*/
        middle.vertex3.y = center.y = edge.vertex3.y;

// Create an edge strip between the middle and the given edge.
        TriangulateEdgeStrip(
            middle, _weights1, cell.Index,
            edge, _weights1, cell.Index
        );

        terrain.AddTriangle(centerLeft, middle.vertex1, middle.vertex2);
        terrain.AddQuad(centerLeft, center, middle.vertex2, middle.vertex3);
        terrain.AddQuad(center, centerRight, middle.vertex3, middle.vertex4);
        terrain.AddTriangle(centerRight, middle.vertex4, middle.vertex5);

        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;
        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddQuadCellData(indices, _weights1);
        terrain.AddQuadCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);

        if (!cell.IsUnderwater) {
            bool reversed = (cell.IncomingRiver == direction);

            TriangulateRiverQuad(
                centerLeft,
                centerRight,
                middle.
                vertex2,
                middle.vertex4,
                cell.RiverSurfaceY,
                0.4f,
                reversed,
                indices
            );

            TriangulateRiverQuad(
                middle.vertex2,
                middle.vertex4,
                edge.vertex2,
                edge.vertex4,
                cell.RiverSurfaceY,
                0.6f,
                reversed,
                indices
            );
        }
    }

    private void TriangulateWithRiverBeginOrEnd(
        HexDirection direction,
        HexCell cell,
        Vector3 center,
        EdgeVertices edge
    ) {
        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(center, edge.vertex1, 0.5f),
            Vector3.Lerp(center, edge.vertex5, 0.5f)
        );

        middle.vertex3.y = edge.vertex3.y;

        TriangulateEdgeStrip(
            middle, _weights1, cell.Index,
            edge, _weights1, cell.Index
        );

        TriangulateEdgeFan(center, middle, cell.Index);

        if (!cell.IsUnderwater) {
            bool reversed = cell.HasIncomingRiver;

            Vector3 indices;
            indices.x = indices.y = indices.z = cell.Index;
            
            TriangulateRiverQuad(
                middle.vertex2,
                middle.vertex4,
                edge.vertex2,
                edge.vertex4,
                cell.RiverSurfaceY,
                0.6f,
                reversed,
                indices
            );

            center.y =
                middle.vertex2.y = middle.vertex4.y = cell.RiverSurfaceY;

            rivers.AddTriangle(center, middle.vertex2, middle.vertex4);

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
        HexDirection direction,
        HexCell cell,
        Vector3 center,
        EdgeVertices edge
    ) {
        if (cell.HasRoads) {
            TriangulateRoadAdjacentToRiver(
                direction,
                cell,
                center,
                edge
            );
        }

        if (cell.HasRiverThroughEdge(direction.Next())) {

/* If the direction has a river on either side, it has a slight curve. 
* The center vertex of river-adjacent triangle needs to be moved toward 
* the edge so they don't overlap the river.
*/
            if (cell.HasRiverThroughEdge(direction.Previous())) {
                center += HexMetrics.GetSolidEdgeMiddle(direction) *
                    (HexMetrics.innerToOuter * 0.5f);
            }

/* If the cell has a river through the previous previous direction,
* it has a river flowing through the cell. Move the center vertex
* of the river-adjacent triangle so that it does not overlap the river.
*/
            else if (
                cell.HasRiverThroughEdge(direction.Previous2())
            ) {
                center +=
                    HexMetrics.GetFirstSolidCorner(direction) * 0.25f;
            }
        }

/* Second case of straight-river-adjacent triangle. Need to move center
* so it doesn't overlap the river.
*/
        else if (
            cell.HasRiverThroughEdge(direction.Previous()) &&
            cell.HasRiverThroughEdge(direction.Next2())
        ) {
            center += HexMetrics.GetSecondSolidCorner(direction) * 0.25f;
        }

        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(center, edge.vertex1, 0.5f),
            Vector3.Lerp(center, edge.vertex5, 0.5f)
        );

        TriangulateEdgeStrip(
            middle,
            _weights1,
            cell.Index,
            edge,
            _weights1,
            cell.Index
        );

        TriangulateEdgeFan(center, middle, cell.Index);

        if (!cell.IsUnderwater && !cell.HasRoadThroughEdge(direction)) {
            features.AddFeature(
                cell,
                (center + edge.vertex1 + edge.vertex5) * (1f / 3f)
            );
        }
    }

    private void TriangulateConnection(
        HexDirection direction,
        HexCell cell,
        EdgeVertices edge1
    ) {
        HexCell neighbor = cell.GetNeighbor(direction);

/* Some cells will not have neighbors. If this is the case, return out
* of the method.
*/
        if (neighbor == null) {
            return;
        }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        bridge.y = neighbor.Position.y - cell.Position.y;

        EdgeVertices edge2 = new EdgeVertices(
            edge1.vertex1 + bridge,
            edge1.vertex5 + bridge
        );

        bool hasRiver = cell.HasRiverThroughEdge(direction);
        bool hasRoad = cell.HasRoadThroughEdge(direction);

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
                        edge1.vertex2,
                        edge1.vertex4,
                        edge2.vertex2,
                        edge2.vertex4,
                        cell.RiverSurfaceY,
                        neighbor.RiverSurfaceY,
                        0.8f,
                        (cell.HasIncomingRiver &&
                        cell.IncomingRiver == direction),
                        indices
                    );
                }
                else if(cell.Elevation > neighbor.WaterLevel) {
                    TriangulateWaterfallInWater
                    (
                        edge1.vertex2, edge1.vertex4, 
                        edge2.vertex2, edge2.vertex4, 
                        cell.RiverSurfaceY, 
                        neighbor.RiverSurfaceY,
                        neighbor.WaterSurfaceY,
                        indices
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
                    edge1.vertex4,
                    edge1.vertex2,
                    neighbor.RiverSurfaceY,
                    cell.RiverSurfaceY,
                    cell.WaterSurfaceY,
                    indices
                );
            }
        }

        if (cell.GetEdgeType(direction) == EdgeType.Slope) {
            TriangulateEdgeTerraces(
                edge1, 
                cell, 
                edge2, 
                neighbor, 
                hasRoad
            );
        }
        else {
            TriangulateEdgeStrip(
                edge1,
                _weights1,
                cell.Index,
                edge2,
                _weights2,
                neighbor.Index, 
                hasRoad
            );
        }

        features.AddWall(
            edge1,
            cell,
            edge2,
            neighbor,
            hasRiver,
            hasRoad
        );

/* Drawn and color the triangle between the bridge of the current cell
* and its current neighbor and the bridge of the next cell and the
* current neighbor.*/
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

        if (direction <= HexDirection.East && nextNeighbor != null) {

/* Create a 5th vertex and assign it with the elevation of the neighbor
* under consideration. This will be used as the final vertex in the
* triangle which fills the gap between bridges.
*/
            Vector3 vertex5 =
                edge1.vertex5 + HexMetrics.GetBridge(direction.Next());

            vertex5.y = nextNeighbor.Position.y;

            if (cell.Elevation <= neighbor.Elevation) {
                if (cell.Elevation <= nextNeighbor.Elevation) {

//This cell has lowest elevation, no rotation.
                    TriangulateCorner(
                        edge1.vertex5,
                        cell,
                        edge2.vertex5,
                        neighbor,
                        vertex5,
                        nextNeighbor
                    );
                }
                else {
// Next neighbor has lowest elevation, rotate counter-clockwise.
                    TriangulateCorner(
                        vertex5,
                        nextNeighbor,
                        edge1.vertex5,
                        cell,
                        edge2.vertex5,
                        neighbor
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
                    edge1.vertex5,
                    cell
                );
            }
            else {

// Next neighbor has lowest elevation, rotate counter-clockwise.
                TriangulateCorner(
                    vertex5,
                    nextNeighbor,
                    edge1.vertex5,
                    cell,
                    edge2.vertex5,
                    neighbor
                );
            }
        }
    }

    private void TriangulateEdgeTerraces(
        EdgeVertices begin,
        HexCell beginCell,
        EdgeVertices end,
        HexCell endCell,
        bool hasRoad
    ) {
        EdgeVertices edge2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color weight2 = HexMetrics.TerraceLerp(_weights1, _weights2, 1);
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
            hasRoad
        );

        for (int i = 2; i < HexMetrics.terraceSteps; i++) {
            EdgeVertices edge1 = edge2;
            Color weight1 = weight2;
            edge2 = EdgeVertices.TerraceLerp(begin, end, i);
            weight2 = HexMetrics.TerraceLerp(_weights1, _weights2, i);

            TriangulateEdgeStrip(
                edge1, 
                weight1, 
                index1,
                edge2, 
                weight2, 
                index2,
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
            hasRoad
        );
    }

    private void TriangulateCorner(
        Vector3 bottom,
        HexCell bottomCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell
    ) {
        EdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        EdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        if (leftEdgeType == EdgeType.Slope) {
            if (rightEdgeType == EdgeType.Slope) {

// Corner is also a terrace. Slope-Slope-Flat.
                TriangulateCornerTerraces(
                    bottom,
                    bottomCell,
                    left,
                    leftCell,
                    right,
                    rightCell
                );
            }

// If the right edge is flat, must terrace from left instead of bottom. Slope-Flat-Slope
            else if (rightEdgeType == EdgeType.Flat) {
                TriangulateCornerTerraces (
                    left,
                    leftCell,
                    right,
                    rightCell,
                    bottom,
                    bottomCell
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
                    rightCell
                );
            }
        }
        else if (rightEdgeType == EdgeType.Slope) {
            if (leftEdgeType == EdgeType.Flat) {

/* If the right edge is a slope, and the left edge is flat, must terrace from right instead
* of bottom. Flat-Slope-Slope.
*/
                TriangulateCornerTerraces (
                    right,
                    rightCell,
                    bottom,
                    bottomCell,
                    left,
                    leftCell
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
                    rightCell
                );
            }
        }

/* Neither the left or right cell edge type is a slope. If the right cell type of the left cell
* is a slope, then terraces must be calculated for a corner between two cliff edges.
* Cliff-Cliff-Slope Right, or Cliff-Cliff-Slope Left.
*/
        else if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {

// If Cliff-Cliff-Slope-Left
            if (leftCell.Elevation < rightCell.Elevation) {
                TriangulateCornerCliffTerraces(
                    right,
                    rightCell,
                    bottom,
                    bottomCell,
                    left,
                    leftCell
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
                    bottomCell
                );
            }
        }

// Else all edges are cliffs. Simply draw a triangle.
        else {
            terrain.AddTriangle(
                bottom,
                left,
                right
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
            rightCell
        );
    }

    private void TriangulateCornerTerraces(
        Vector3 begin,
        HexCell beginCell,
        Vector3 left,
        HexCell leftCell,
        Vector3 right,
        HexCell rightCell
    ) {
        Vector3 vertex3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 vertex4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color weight3 = HexMetrics.TerraceLerp(_weights1, _weights2, 1);
        Color weight4 = HexMetrics.TerraceLerp(_weights1, _weights3, 1);

        Vector3 indices;

        indices.x = beginCell.Index;
        indices.y = leftCell.Index;
        indices.z = rightCell.Index;

        terrain.AddTriangle(
            begin,
            vertex3,
            vertex4
        );

        terrain.AddTriangleCellData(
            indices,
            _weights1,
            weight3,
            weight4
        );

        for (int i = 2; i < HexMetrics.terraceSteps; i++) {
            Vector3 vertex1 = vertex3;
            Vector3 vertex2 = vertex4;
            Color weight1 = weight3;
            Color weight2 = weight4;

            vertex3 = HexMetrics.TerraceLerp(begin, left, i);
            vertex4 = HexMetrics.TerraceLerp(begin, right, i);
            weight3 = HexMetrics.TerraceLerp(_weights1, _weights2, i);
            weight4 = HexMetrics.TerraceLerp(_weights1, _weights3, i);

            terrain.AddQuad(vertex1, vertex2, vertex3, vertex4);
            
            terrain.AddQuadCellData(
                indices,
                weight1,
                weight2,
                weight3,
                weight4
            );
        }

        terrain.AddQuad(vertex3, vertex4, left, right);

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
        HexCell rightCell
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
                HexMetrics.Perturb(begin),
                HexMetrics.Perturb(right),
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
            indices
        );

// Slope-Cliff-Slope. Triangulate a slope.
        if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {
            TriangulateBoundaryTriangle (
                left, 
                _weights2,
                right, 
                _weights3,
                boundary, 
                boundaryWeights,
                indices
            );
        }

// Slope-Cliff-Cliff. Triangulate a cliff.
        else {

/* Add perturbation for all vertices except the boundary
* vertex, to handle the Slope-Cliff-Cliff case of the 
* Cliff-Slope perturbation problem.
*/
            terrain.AddTriangleUnperturbed(
                HexMetrics.Perturb(left),
                HexMetrics.Perturb(right),
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
        HexCell rightCell
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
                HexMetrics.Perturb(begin),
                HexMetrics.Perturb(left),
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
            indices
        );

// Slope-Cliff-Slope. Triangulate a slope.
        if (leftCell.GetEdgeType(rightCell) == EdgeType.Slope) {
            TriangulateBoundaryTriangle(
                left, 
                _weights2,
                right, 
                _weights3,
                boundary, 
                boundaryWeights,
                indices
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
                HexMetrics.Perturb(left),
                HexMetrics.Perturb(right),
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
        Vector3 indices
    ) {

/* Immediately perturb vertex 2 as an optimization since it is not
* being used to derive any other point.
*/
        Vector3 vertex2 = 
            HexMetrics.Perturb(
                HexMetrics.TerraceLerp(begin, left, 1)
            );

        Color weight2 = 
            HexMetrics.TerraceLerp(
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
            HexMetrics.Perturb(begin),
            vertex2,
            boundary
        );

        terrain.AddTriangleCellData(indices, beginWeights, weight2, boundaryWeights);

        for (int i = 2; i < HexMetrics.terraceSteps; i++) {

/* vertex2 has already been perturbed, need not pertub
* vertex1 as it is derived from vertex2.
*/
            Vector3 vertex1 = vertex2;
            Color weight1 = weight2;

            vertex2 = HexMetrics.Perturb(
                HexMetrics.TerraceLerp(begin, left, i)
            );

            weight2 = HexMetrics.TerraceLerp(
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

        terrain.AddTriangleUnperturbed(vertex2, HexMetrics.Perturb(left), boundary);
        terrain.AddTriangleCellData(indices, weight2, leftWeights, boundaryWeights);
    }

    private void TriangulateEdgeFan(
        Vector3 center,
        EdgeVertices edge,
        float index
    ) {
        terrain.AddTriangle(center, edge.vertex1, edge.vertex2);
        terrain.AddTriangle(center, edge.vertex2, edge.vertex3);
        terrain.AddTriangle(center, edge.vertex3, edge.vertex4);
        terrain.AddTriangle(center, edge.vertex4, edge.vertex5);

        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);
        terrain.AddTriangleCellData(indices, _weights1);
    }

    private void TriangulateEdgeStrip
    (
        EdgeVertices edge1,
        Color weight1,
        float index1,
        EdgeVertices edge2,
        Color weight2,
        float index2,
        bool hasRoad = false
    ) {
        terrain.AddQuad(
            edge1.vertex1,
            edge1.vertex2,
            edge2.vertex1,
            edge2.vertex2
        );

        terrain.AddQuad(
            edge1.vertex2,
            edge1.vertex3,
            edge2.vertex2,
            edge2.vertex3
        );

        terrain.AddQuad(
            edge1.vertex3,
            edge1.vertex4,
            edge2.vertex3,
            edge2.vertex4
        );

        terrain.AddQuad(
            edge1.vertex4,
            edge1.vertex5,
            edge2.vertex4,
            edge2.vertex5
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
                indices
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
        Vector3 indices
    ) {
        vertex1.y = vertex2.y = y1;
        vertex3.y = vertex4.y = y2;

        rivers.AddQuad(vertex1, vertex2, vertex3, vertex4);

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
        Vector3 indices
    ) {
        TriangulateRiverQuad(
            vertex1,
            vertex2,
            vertex3,
            vertex4,
            y, y, v,
            reversed,
            indices
        );
    }

    private void TriangulateRoadSegment
    (
        Vector3 vertex1, Vector3 vertex2, Vector3 vertex3,
        Vector3 vertex4, Vector3 vertex5, Vector3 vertex6,
        Color weight1, Color weight2, Vector3 indices
    ) {
        roads.AddQuad(vertex1, vertex2, vertex4, vertex5);
        roads.AddQuad(vertex2, vertex3, vertex5, vertex6);

        roads.AddQuadUV(0f, 1f, 0f, 0f);
        roads.AddQuadUV(1f, 0f, 0f, 0f);

        roads.AddQuadCellData(indices, weight1, weight2);
        roads.AddQuadCellData(indices, weight1, weight2);
    }

    private void TriangulateRoadEdge(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight,
        float index
    ) {
        roads.AddTriangle(center, middleLeft, middleRight);

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
        HexDirection direction, 
        HexCell cell, 
        Vector3 center, 
        EdgeVertices edge
    ) {
        bool hasRoadThroughEdge = cell.HasRoadThroughEdge(direction);
        bool previousHasRiver = cell.HasRiverThroughEdge(direction.Previous());
        bool nextHasRiver = cell.HasRiverThroughEdge(direction.Next());
        Vector2 interpolators = GetRoadInterpolators(direction, cell);
        Vector3 roadCenter = center;

        if (cell.HasRiverBeginOrEnd) {
            roadCenter += 
                HexMetrics.GetSolidEdgeMiddle((cell.RiverBeginOrEndDirection.Opposite())) * 
                (1f / 3f);
        }
        else if(cell.IncomingRiver == cell.OutgoingRiver.Opposite()) {
            Vector3 corner;

/* If the previous cell has a river, the corner the center will be moved toward is
* equal to the current direction + 1.
*/
            if (previousHasRiver) {
                if (
                    !hasRoadThroughEdge &&
                    !cell.HasRoadThroughEdge(direction.Next())
                ) {
                    return;
                }
                corner = HexMetrics.GetSecondSolidCorner(direction);
            }
/* If the previous cell does not have a river, the corner the center will be moved
* toward is the same index as the current direction.
*/
            else {
                if (
                    !hasRoadThroughEdge &&
                    !cell.HasRoadThroughEdge(direction.Previous())
                ) {
                    return;
                }

                corner = HexMetrics.GetFirstSolidCorner(direction);
            }
/* Using the example of a river flowing from east to west or west to east, for all cases
* this will result in the river being pushed either directly "up" north away from the
* river or directly "down" south away from the river.
*/
            roadCenter += corner * 0.5f;

            if (
                cell.IncomingRiver == direction.Next() && 
                cell.HasRoadThroughEdge(direction.Next2()) ||
                cell.HasRoadThroughEdge(direction.Opposite())
            ) {
                features.AddBridge(roadCenter, center - corner * 0.5f);
            }
            
            center += corner * 0.25f;
        }

/* If the river has a zigzag, then the incoming river will be the on the edge previous
* from the outgoing river or the incoming river will be on the next edge of the outoing
* river. In the case of the former, the index of the corner whose vector is pointing
* away from the river is the index of the incoming river + 1. Otherwise it is the
* index of the incoming river. In both cases, subtracting the road center by that
* vector times 0.2f is sufficent to push the road center away from the river.
*/
        else if (cell.IncomingRiver == cell.OutgoingRiver.Previous()) {
            roadCenter -= HexMetrics.GetSecondCorner(cell.IncomingRiver) * 0.2f;
        }
        else if (cell.IncomingRiver == cell.OutgoingRiver.Next()) {
            roadCenter -= HexMetrics.GetFirstCorner(cell.IncomingRiver) * 0.2f;
        }

/* If there is a river on the previous and next edges, the river has a slight bend.
* Need to pull the road center toward the current cell edge, which will shorten the
* road back away from the river.
*/
        else if(previousHasRiver && nextHasRiver) { 
            if (!hasRoadThroughEdge) {
                return;
            }

/* Must account for difference in scale between corners and middles by using
* HexMetrics.innerToOuter.
*/
            Vector3 offset = 
                HexMetrics.GetSolidEdgeMiddle(direction) *
                HexMetrics.innerToOuter;
            
            roadCenter += offset * 0.7f;
            center += offset * 0.5f;
        }

/* The only remaining case is that the cell lies on the outside of a curving river.
* In this case, there are three edges pointing away from the river. The middle edge
* of these three edges must be obtained. Then, the center of the road is pushed
* toward the middle of this edge.
*/
        else {
            HexDirection middle;
            if (previousHasRiver) {
                middle = direction.Next();
            }
            else if (nextHasRiver) {
                middle = direction.Previous();
            }
            else {
                middle = direction;
            }

/* If there is no road through any of the cells on the outer side of the river
* bend, then the road center need not move and should instead be pruned.
*/
            if (
                !cell.HasRoadThroughEdge(middle) &&
                !cell.HasRoadThroughEdge(middle.Previous()) &&
                !cell.HasRoadThroughEdge(middle.Next())
            ) {
                return;
            }

            Vector3 offset = HexMetrics.GetSolidEdgeMiddle(middle);
            roadCenter += offset * 0.25f;

            if (
                direction == middle &&
                cell.HasRoadThroughEdge(direction.Opposite())
            ) {
                features.AddBridge (
                    roadCenter,
                    center - offset * (HexMetrics.innerToOuter * 0.7f)
                );
            }
        }

        Vector3 middleLeft = 
            Vector3.Lerp(roadCenter, edge.vertex1, interpolators.x);
        Vector3 middleRight =
            Vector3.Lerp(roadCenter, edge.vertex5, interpolators.y);

        TriangulateRoad(
            roadCenter,
            middleLeft,
            middleRight,
            edge,
            hasRoadThroughEdge,
            cell.Index
        );

        if (previousHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                center,
                middleLeft,
                cell.Index
            );
        }

        if (nextHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                middleRight,
                center,
                cell.Index
            );
        }
    }

    private void TriangulateRoad(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight, 
        EdgeVertices edge,
        bool hasRoadThroughCellEdge,
        float index
    ) {
        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        if (hasRoadThroughCellEdge) {
            Vector3 middleCenter = Vector3.Lerp(middleLeft, middleRight, 0.5f);

            TriangulateRoadSegment
            (
                middleLeft,
                middleCenter,
                middleRight,
                edge.vertex2,
                edge.vertex3,
                edge.vertex4,
                _weights1,
                _weights1,
                indices
            );

            roads.AddTriangle(center, middleLeft, middleCenter);
            roads.AddTriangle(center, middleCenter, middleRight);
            
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
            TriangulateRoadEdge(center, middleLeft, middleRight, index);
        }

    }

    private Vector2 GetRoadInterpolators(
        HexDirection direction,
        HexCell cell
    ) {
        Vector2 interpolators;

        if (cell.HasRoadThroughEdge(direction)) {
            interpolators.x = interpolators.y = 0.5f;
        }
        else {
            interpolators.x =
                cell.HasRoadThroughEdge(direction.Previous()) ? 0.5f : 0.25f;
            interpolators.y =
                cell.HasRoadThroughEdge(direction.Next()) ? 0.5f : 0.25f;
        }

        return interpolators;
    }

    private void TriangulateWater(
        HexDirection direction,
        HexCell cell,
        Vector3 center
    ) {
        center.y = cell.WaterSurfaceY;

        HexCell neighbor = cell.GetNeighbor(direction);

        if (neighbor != null && !neighbor.IsUnderwater) {
            TriangulateWaterShore(direction, cell, neighbor, center);
        }
        else {
            TriangulateOpenWater(direction, cell, neighbor, center);
        }            
    }

    private void TriangulateOpenWater(
        HexDirection direction,
        HexCell cell,
        HexCell neighbor,
        Vector3 center
    ) { 
        Vector3 center1 = center + HexMetrics.GetFirstWaterCorner(direction);
        Vector3 center2 = center + HexMetrics.GetSecondWaterCorner(direction);

        water.AddTriangle(center, center1, center2);

        Vector3 indices;
        indices.x = indices.y = indices.z = cell.Index;
        water.AddTriangleCellData(indices, _weights1);

        if (
            direction <= HexDirection.SouthEast && 
            neighbor != null
        ) {
            Vector3 bridge = HexMetrics.GetWaterBridge(direction);
            Vector3 edge1 = center1 + bridge;
            Vector3 edge2 = center2 + bridge;

            water.AddQuad(center1, center2, edge1, edge2);
            indices.y = neighbor.Index;
            water.AddQuadCellData(indices, _weights1, _weights2);

            if (direction <= HexDirection.East) {
                HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

                if (nextNeighbor == null || !nextNeighbor.IsUnderwater) {
                    return;
                }

                water.AddTriangle(
                    center2, 
                    edge2, 
                    center2 + HexMetrics.GetWaterBridge(direction.Next())
                );

                indices.z = nextNeighbor.Index;

                water.AddTriangleCellData(
                    indices, _weights1, _weights2, _weights3
                );
            }
        }
    }

    private void TriangulateWaterShore(
        HexDirection direction,
        HexCell cell,
        HexCell neighbor,
        Vector3 center
    ) {
        EdgeVertices edge1 = new EdgeVertices(
            center + HexMetrics.GetFirstWaterCorner(direction),
            center + HexMetrics.GetSecondWaterCorner(direction)
        );

        water.AddTriangle(center, edge1.vertex1, edge1.vertex2);
        water.AddTriangle(center, edge1.vertex2, edge1.vertex3);
        water.AddTriangle(center, edge1.vertex3, edge1.vertex4);
        water.AddTriangle(center, edge1.vertex4, edge1.vertex5);

        Vector3 indices = new Vector3();
        indices.x = indices.y = cell.Index;
        indices.y = neighbor.Index;

        water.AddTriangleCellData(indices, _weights1);
        water.AddTriangleCellData(indices, _weights1);
        water.AddTriangleCellData(indices, _weights1);
        water.AddTriangleCellData(indices, _weights1);

// Work backward from the solid shore to obtain the edge.
        Vector3 center2 = neighbor.Position;

// If the neighbor outside the wrap boundaries, adjust accordingly.
        if (neighbor.ColumnIndex < cell.ColumnIndex - 1) {
            center2.x += 
                HexMetrics.wrapSize * HexMetrics.innerDiameter;
        }
        else if (neighbor.ColumnIndex > cell.ColumnIndex + 1) {
            center2.x -=
                HexMetrics.wrapSize * HexMetrics.innerDiameter;
        }

        center2.y = center.y;

        EdgeVertices edge2 = new EdgeVertices(
            center2 + HexMetrics.GetSecondSolidCorner(direction.Opposite()),
            center2 + HexMetrics.GetFirstSolidCorner(direction.Opposite())
        );

        if (cell.HasRiverThroughEdge(direction)) {
            TriangulateEstuary(
                edge1,
                edge2,
                cell.HasIncomingRiver && cell.IncomingRiver == direction,
                indices
            );

        }
        else {
            waterShore.AddQuad(
                edge1.vertex1,
                edge1.vertex2,
                edge2.vertex1,
                edge2.vertex2
            );

            waterShore.AddQuad(
                edge1.vertex2,
                edge1.vertex3,
                edge2.vertex2,
                edge2.vertex3
            );

            waterShore.AddQuad(
                edge1.vertex3,
                edge1.vertex4,
                edge2.vertex3,
                edge2.vertex4
            );

            waterShore.AddQuad(
                edge1.vertex4,
                edge1.vertex5,
                edge2.vertex4,
                edge2.vertex5
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
        
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());

        if (nextNeighbor != null) {
            Vector3 center3 = nextNeighbor.Position;

            if (nextNeighbor.ColumnIndex < cell.ColumnIndex - 1) {
                center3.x += HexMetrics.wrapSize * HexMetrics.innerDiameter;
            }
            else if (nextNeighbor.ColumnIndex > cell.ColumnIndex + 1) {
                center3.x -= HexMetrics.wrapSize * HexMetrics.innerDiameter;
            }

/* Work backward from the shore to obtain the triangle if the neighbor is
* underwater, otherwise obtain normal triangle.
*/

            Vector3 vertex3 = 
                center3 + (
                    nextNeighbor.IsUnderwater ?
                    HexMetrics.GetFirstWaterCorner(direction.Previous()) :
                    HexMetrics.GetFirstSolidCorner(direction.Previous())
                );

            vertex3.y = center.y;

            waterShore.AddTriangle (
                edge1.vertex5,
                edge2.vertex5,
                vertex3
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
        Vector3 indices
    ) {
        waterShore.AddTriangle(
            edge2.vertex1,
            edge1.vertex2,
            edge1.vertex1
        );

        waterShore.AddTriangle(
            edge2.vertex5,
            edge1.vertex5,
            edge1.vertex4
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
            edge1.vertex3
        );

        estuaries.AddTriangle(
            edge1.vertex3, 
            edge2.vertex2, 
            edge2.vertex4
        );

        estuaries.AddQuad(
            edge1.vertex3, 
            edge1.vertex4, 
            edge2.vertex4, 
            edge2.vertex5
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
        Vector3 indices
    ) {
        vertex1.y = vertex2.y = y1;
        vertex3.y = vertex4.y = y2;
        vertex1 = HexMetrics.Perturb(vertex1);
        vertex2 = HexMetrics.Perturb(vertex2);
        vertex3 = HexMetrics.Perturb(vertex3);
        vertex4 = HexMetrics.Perturb(vertex4);
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
