using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkLayer : MapMeshChunkLayer {
    public new static TerrainChunkLayer CreateEmpty(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Terrain Chunk Layer");
        
        TerrainChunkLayer resultMono =
            resultObj.AddComponent<TerrainChunkLayer>();
        
        resultMono.GetComponent<MeshRenderer>().material = material;
        resultMono._useCollider = useCollider;

        if (useCollider)
            resultMono._meshCollider =
                resultObj.AddComponent<MeshCollider>();

        resultMono._useHexData = useHexData;
        resultMono._useUVCoordinates = useUVCoordinates;
        resultMono._useUV2Coordinates = useUV2Coordinates;

        return resultMono;
    }
 
    public TerrainTriangulationData TriangulateHexTerrainEdge(
        Hex hex,
        Hex neighbor,
        TerrainTriangulationData triangulationData,
        Dictionary<HexDirections, Hex> neighbors,
        HexDirections direction,
        HexRiverData riverData,
        FeatureContainer features,
        Dictionary<HexDirections, bool> roadEdges,
        Dictionary<HexDirections, ElevationEdgeTypes> elevationEdgeTypes,
        float hexOuterRadius,
        int wrapSize
    ) {          
        triangulationData = TriangulateTerrainCenter(
            riverData,
            direction,
            hex,
            triangulationData,
            hexOuterRadius,
            wrapSize,
            this,
            features,
            roadEdges
        );

        if (direction <= HexDirections.Southeast) {
            triangulationData = TriangulateTerrainConnection(
                hex,
                neighbor,
                triangulationData,
                direction,
                riverData,
                roadEdges,
                elevationEdgeTypes,
                hexOuterRadius,
                wrapSize,
                this,
                features
            );
            
    // Adjust the other edge of the connection  if there is a river through
    // that edge.
            triangulationData = TryTriangulateNeighborTerrainCorner(
                hex,
                neighbor,
                triangulationData,
                direction,
                neighbors,
                hexOuterRadius,
                wrapSize,
                this,
                features
            );
        }

        return triangulationData;
    }

    private TerrainTriangulationData TriangulateTerrainCenter(
        HexRiverData riverData,
        HexDirections direction,
        Hex source,
        TerrainTriangulationData data,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features,
        Dictionary<HexDirections, bool> roadEdges
    ) {
        if (riverData.HasRiver) {
            if (riverData.HasRiverInDirection(direction)) {

                data.centerEdgeVertices.vertex3.y = source.StreamBedY;

                if (riverData.HasRiverStartOrEnd) {
                    data = TriangulateRiverBeginOrEndTerrain(
                        source,
                        data,
                        hexOuterRadius,
                        wrapSize,
                        terrain
                    );
                }
                else {
                    data = TriangulateRiverBanks(
                        data,
                        riverData,
                        direction,
                        hexOuterRadius
                    );

                    data = TriangulateRiverTerrain(
                        source,
                        data,
                        hexOuterRadius,
                        wrapSize,
                        terrain
                    );
                }
            }
            else {
                data = TriangulateTerrainAdjacentToRiver(
                    source,
                    direction,
                    data,
                    roadEdges,
                    riverData,
                    hexOuterRadius,
                    wrapSize,
                    terrain,
                    features
                );
            }
        }
        else {            
            // Triangulate terrain center without river, basic edge fan.
            TriangulateEdgeFan(
                data.terrainCenter,
                data.centerEdgeVertices,
                source.Index,
                hexOuterRadius,
                wrapSize,
                terrain
            );

            if (
                !source.IsUnderwater &&
                !roadEdges[direction]
            ) {
                features.AddFeature(
                    source,
                    (
                        data.terrainCenter +
                        data.centerEdgeVertices.vertex1 +
                        data.centerEdgeVertices.vertex5
                    ) * (1f / 3f),
                    hexOuterRadius,
                    wrapSize
                );
            }
        }

        return data;
    }

    private TerrainTriangulationData TriangulateRiverBeginOrEndTerrain(
        Hex source,
        TerrainTriangulationData data,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
        data.middleEdgeVertices = new EdgeVertices(
            Vector3.Lerp(
                data.terrainCenter,
                data.centerEdgeVertices.vertex1,
                0.5f
            ),
            Vector3.Lerp(
                data.terrainCenter,
                data.centerEdgeVertices.vertex5,
                0.5f
            )
        );

        data.middleEdgeVertices.vertex3.y = source.StreamBedY;

        TriangulateEdgeStripTerrain(
            data.middleEdgeVertices,
            _weights1,
            source.Index,
            data.centerEdgeVertices,
            _weights1,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        TriangulateEdgeFan(
            data.terrainCenter,
            data.middleEdgeVertices,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        return data;
    }

    private TerrainTriangulationData TriangulateTerrainConnection(
        Hex source,
        Hex neighbor,
        TerrainTriangulationData data,
        HexDirections direction,
        HexRiverData riverData,
        Dictionary<HexDirections, bool> roadEdges,
        Dictionary<HexDirections, ElevationEdgeTypes> elevationEdgeTypes,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
    ) {
        if (riverData.HasRiverInDirection(direction)) {
            data.connectionEdgeVertices.vertex3.y = neighbor.StreamBedY;
        }

        bool hasRoad = roadEdges[direction];

        if (
//            hex.GetEdgeType(direction) == ElevationEdgeTypes.Slope
            elevationEdgeTypes[direction] == ElevationEdgeTypes.Slope
        ) {
            TriangulateEdgeTerracesTerrain(
                data.centerEdgeVertices, 
                source, 
                data.connectionEdgeVertices, 
                neighbor,
                hexOuterRadius,
                wrapSize,
                terrain
            );
        }
        else {
            TriangulateEdgeStripTerrain(
                data.centerEdgeVertices,
                _weights1,
                source.Index,
                data.connectionEdgeVertices,
                _weights2,
                neighbor.Index,
                hexOuterRadius,
                wrapSize,
                terrain
            );
        }

        features.AddWall(
            data.centerEdgeVertices,
            source,
            data.connectionEdgeVertices,
            neighbor,
            riverData.HasRiverInDirection(direction),
            hasRoad,
            hexOuterRadius,
            wrapSize
        );

        return data;
    }

    private TerrainTriangulationData TriangulateRiverBanks(
        TerrainTriangulationData data,
        HexRiverData riverData,
        HexDirections direction,
        float hexOuterRadius
    ) {
        if (riverData.HasRiverInDirection(direction.Opposite())) {
/* Create a vertex 1/4th of the way from the center of the hex 
* to first solid corner of the previous edge, which is pointing
* straight "down" toward the bottom of the hexagon for a left facing
* edge.
*/
            data.riverCenterLeft = data.terrainCenter +
                HexagonPoint.GetFirstSolidCorner(
                    direction.PreviousClockwise(),
                    hexOuterRadius
                ) * 0.25f;

/* Create a vertex 1/4th of the way from the center of the hex
* to the second solid corner of the next edge, which is pointing
* straight "up" toward the top of the hexagon for a left facing edge.
*/
            data.riverCenterRight = data.terrainCenter +
                HexagonPoint.GetSecondSolidCorner(
                    direction.NextClockwise(),
                    hexOuterRadius
                ) * 0.25f;
        }

/* If the next direction has a sharp turn, there will be a river through
* direction.Next() or direction.Previous(). Must align center line with
* center line with edge between this river and the adjacent river.
* Interpolate with an increased step to account for the rotation
* of the center line.
*/
        else if (
            riverData.HasRiverInDirection(direction.NextClockwise())
        ) {
            data.riverCenterLeft = data.terrainCenter;
            data.riverCenterRight = 
                Vector3.Lerp(
                    data.terrainCenter,
                    data.centerEdgeVertices.vertex5,
                    2f / 3f
                );
        }
        else if (
            riverData.HasRiverInDirection(direction.PreviousClockwise())
        ) {
            data.riverCenterLeft =
                Vector3.Lerp(
                    data.terrainCenter,
                    data.centerEdgeVertices.vertex1,
                    2f / 3f
                );

            data.riverCenterRight = data.terrainCenter;
        }

/* If the hex has a river two directions next, or two directions
* previous, there is a slight bend in the river. Need to push
* the center line to the inside of the bend. Using
* HexMetrics.innerToOuter to adjust for the fact that
* the midpoint of a solid edge is closer to the center
* of a hex than a solid edge corner.
*/
        else if (
            riverData.HasRiverInDirection(direction.NextClockwise2())
        ) {
            data.riverCenterLeft = data.terrainCenter;

            data.riverCenterRight = 
                data.terrainCenter + 
                HexagonPoint.GetSolidEdgeMiddle(
                    direction.NextClockwise(),
                    hexOuterRadius
                ) * (0.5f * HexagonConstants.INNER_TO_OUTER_RATIO);
        }
// Previous 2
        else {
            data.riverCenterLeft = 
                data.terrainCenter + 
                HexagonPoint.GetSolidEdgeMiddle(
                    direction.PreviousClockwise(),
                    hexOuterRadius
                ) * (0.5f * HexagonConstants.INNER_TO_OUTER_RATIO);

            data.riverCenterRight = data.terrainCenter;
        }

/* Get the final location of the center by averaging
* centerLeft and centerRight. For a straight through
* river this average is the same as the center
* of the hex. For a bend this moves the center
* appropriately. Otherwise, all points are the same
* and the center also remains at the center of the hex.
*/
        data.terrainCenter = Vector3.Lerp(
            data.riverCenterLeft,
            data.riverCenterRight,
            0.5f
        );

/* Create the middle edge vertices using points halfway between
* centerLeft/centerRight and the 1st and 5th vertices of the
* hexagons edge vertices for the given direction. Must use an
* alternate constructor for the middle edge vertices object
* because the length of the edge is 3/4ths rather than 1. To
* keep the 2nd and 4th vertex in line with the rivers edges,
* must interpolate by 1/6th instead of 1/3rd.
*/
        EdgeVertices middleEdgeVertices = new EdgeVertices(
            Vector3.Lerp(
                data.riverCenterLeft,
                data.centerEdgeVertices.vertex1,
                0.5f
            ),
            Vector3.Lerp(
                data.riverCenterRight,
                data.centerEdgeVertices.vertex5,
                0.5f
            ),
            1f / 6f
        );

/* Adjust the height of middle of the middle edge,
* as well as the height of the center of the hexagon, to 
* the height of the middle of the outer edge of the 
* hexagon. The given edge of the hexagon has already 
* been adjusted to the height of the river bed.
*/
        middleEdgeVertices.vertex3.y =
            data.terrainCenter.y =
                data.centerEdgeVertices.vertex3.y;
        
        data.middleEdgeVertices = middleEdgeVertices;
        return data;
    }

    private TerrainTriangulationData TriangulateRiverTerrain(
        Hex source,
        TerrainTriangulationData triangulationData,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
        TriangulateEdgeStripTerrain(
            triangulationData.middleEdgeVertices,
            _weights1,
            source.Index,
            triangulationData.centerEdgeVertices,
            _weights1,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        terrain.AddTrianglePerturbed(
            triangulationData.riverCenterLeft,
            triangulationData.middleEdgeVertices.vertex1,
            triangulationData.middleEdgeVertices.vertex2,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuadPerturbed(
            triangulationData.riverCenterLeft,
            triangulationData.terrainCenter,
            triangulationData.middleEdgeVertices.vertex2,
            triangulationData.middleEdgeVertices.vertex3,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuadPerturbed(
            triangulationData.terrainCenter,
            triangulationData.riverCenterRight,
            triangulationData.middleEdgeVertices.vertex3,
            triangulationData.middleEdgeVertices.vertex4,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddTrianglePerturbed(
            triangulationData.riverCenterRight,
            triangulationData.middleEdgeVertices.vertex4,
            triangulationData.middleEdgeVertices.vertex5,
            hexOuterRadius,
            wrapSize
        );

        Vector3 centerHexIndices;

        centerHexIndices.x =
            centerHexIndices.y =
                centerHexIndices.z =
                    source.Index;

        terrain.AddTriangleHexData(
            centerHexIndices,
            _weights1
        );

        terrain.AddQuadHexData(
            centerHexIndices,
            _weights1
        );

        terrain.AddQuadHexData(
            centerHexIndices,
            _weights1
        );

        terrain.AddTriangleHexData(
            centerHexIndices,
            _weights1
        );

        return triangulationData;
    }

    private TerrainTriangulationData TriangulateTerrainAdjacentToRiver(
        Hex source,
        HexDirections direction,
        TerrainTriangulationData triangulationData,
        Dictionary<HexDirections, bool> roadEdges,
        HexRiverData riverData,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
    ) {
        if (riverData.HasRiverInDirection(direction.NextClockwise())) {
/* If the direction has a river on either side, it has a slight curve. 
* The center vertex of river-adjacent triangle needs to be moved toward 
* the edge so they don't overlap the river.
*/
//            if (hex.HasRiverThroughEdge(direction.Previous())) {
            if (
                riverData.HasRiverInDirection(direction.PreviousClockwise())
            ) {
                triangulationData.terrainCenter += HexagonPoint.GetSolidEdgeMiddle(
                    direction,
                    hexOuterRadius
                ) * (HexagonConstants.INNER_TO_OUTER_RATIO * 0.5f);
            }

/* If the hex has a river through the previous previous direction,
* it has a river flowing through the hex. Move the center vertex
* of the river-adjacent triangle so that it does not overlap the river.
*/
            else if (
                riverData.HasRiverInDirection(
                    direction.PreviousClockwise2()
                )
            ) {
                triangulationData.terrainCenter +=
                    HexagonPoint.GetFirstSolidCorner(
                        direction,
                        hexOuterRadius
                    ) * 0.25f;
            }
        }

/* Second case of straight-river-adjacent triangle. Need to move center
* so it doesn't overlap the river.
*/
        else if (
            riverData.HasRiverInDirection(direction.PreviousClockwise()) &&
            riverData.HasRiverInDirection(direction.NextClockwise2())
        ) {
            triangulationData.terrainCenter += HexagonPoint.GetSecondSolidCorner(
                direction,
                hexOuterRadius
            ) * 0.25f;
        }

        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(
                triangulationData.terrainCenter,
                triangulationData.centerEdgeVertices.vertex1,
                0.5f
            ),
            Vector3.Lerp(
                triangulationData.terrainCenter,
                triangulationData.centerEdgeVertices.vertex5,
                0.5f
            )
        );

        TriangulateEdgeStripTerrain(
            middle,
            _weights1,
            source.Index,
            triangulationData.centerEdgeVertices,
            _weights1,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        TriangulateEdgeFan(
            triangulationData.terrainCenter,
            middle,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        if (!source.IsUnderwater && roadEdges[direction]) {
            features.AddFeature(
                source,
                (
                    triangulationData.terrainCenter +
                    triangulationData.centerEdgeVertices.vertex1 +
                    triangulationData.centerEdgeVertices.vertex5
                ) * (1f / 3f),
                hexOuterRadius,
                wrapSize
            );
        }

        return triangulationData;
    }

    private TerrainTriangulationData TryTriangulateNeighborTerrainCorner(
        Hex source,
        Hex neighbor,
        TerrainTriangulationData data,
        HexDirections direction,
        Dictionary<HexDirections, Hex> neighbors,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
    ) {
        Hex nextNeighbor;
        
        if (
            neighbors.TryGetValue(
                direction.NextClockwise(),
                out nextNeighbor
            ) &&
            direction <= HexDirections.East
        ) {
            TriangulateNeighborTerrainCorner(
                source,
                neighbor,
                nextNeighbor,
                direction,
                data,
                hexOuterRadius,
                wrapSize,
                terrain,
                features
            );
        }

        return data;
    }

    private TerrainTriangulationData TriangulateNeighborTerrainCorner(
        Hex source,
        Hex neighbor,
        Hex nextNeighbor,
        HexDirections direction,
        TerrainTriangulationData data,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
    ) {
// Create a 5th vertex and assign it with the elevation of the neighbor
// under consideration. This will be used as the final vertex in the
// triangle which fills the gap between bridges.
        Vector3 vertex5 =
            data.centerEdgeVertices.vertex5 + HexagonPoint.GetBridge(
                direction.NextClockwise(),
                hexOuterRadius
            );

        vertex5.y = nextNeighbor.Position.y;

        if (source.elevation <= neighbor.elevation) {
            if (source.elevation <= nextNeighbor.elevation) {

// This hex has lowest elevation, no rotation.
                TriangulateTerrainCorner(
                    data.centerEdgeVertices.vertex5,
                    source,
                    data.connectionEdgeVertices.vertex5,
                    neighbor,
                    vertex5,
                    nextNeighbor,
                    hexOuterRadius,
                    wrapSize,
                    terrain,
                    features
                );
            }
            else {
// Next neighbor has lowest elevation, rotate counter-clockwise.
                TriangulateTerrainCorner(
                    vertex5,
                    nextNeighbor,
                    data.centerEdgeVertices.vertex5,
                    source,
                    data.connectionEdgeVertices.vertex5,
                    neighbor,
                    hexOuterRadius,
                    wrapSize,
                    terrain,
                    features
                );
            }
        }
        else if (neighbor.elevation <= nextNeighbor.elevation) {
// Neighbor is lowest hex, rotate triangle clockwise.
            TriangulateTerrainCorner(
                data.connectionEdgeVertices.vertex5,
                neighbor,
                vertex5,
                nextNeighbor,
                data.centerEdgeVertices.vertex5,
                source,
                hexOuterRadius,
                wrapSize,
                terrain,
                features
            );
        }
        else {

// Next neighbor has lowest elevation, rotate counter-clockwise.
            TriangulateTerrainCorner(
                vertex5,
                nextNeighbor,
                data.centerEdgeVertices.vertex5,
                source,
                data.connectionEdgeVertices.vertex5,
                neighbor,
                hexOuterRadius,
                wrapSize,
                terrain,
                features
            );
        }

        return data;
    }

    private void TriangulateTerrainCorner(
        Vector3 begin,
        Hex beginHex,
        Vector3 left,
        Hex leftHex,
        Vector3 right,
        Hex rightHex,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
    ) {
        ElevationEdgeTypes leftEdgeType = beginHex.GetEdgeType(leftHex);
        ElevationEdgeTypes rightEdgeType = beginHex.GetEdgeType(rightHex);

        if (leftEdgeType == ElevationEdgeTypes.Slope) {
            if (rightEdgeType == ElevationEdgeTypes.Slope) {

// Corner is also a terrace. Slope-Slope-Flat.
                TriangulateCornerTerraces(
                    begin,
                    beginHex,
                    left,
                    leftHex,
                    right,
                    rightHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
                );
            }

// If the right edge is flat, must terrace from left instead of bottom.
// Slope-Flat-Slope
            else if (rightEdgeType == ElevationEdgeTypes.Flat) {
                TriangulateCornerTerraces (
                    left,
                    leftHex,
                    right,
                    rightHex,
                    begin,
                    beginHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
                );
            }
            else {

/* At least one edge is a cliff. Slope-Cliff-Slope or Slope-Cliff-Cliff. Standard case
* because slope on left and flat on right.
*/
                TriangulateCornerTerracesCliff (
                    begin,
                    beginHex,
                    left,
                    leftHex,
                    right,
                    rightHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
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
                    begin,
                    beginHex,
                    left,
                    leftHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
                );
            }
            else {

/* At least one edge is a cliff. Slope-Cliff-Slope or Slope-Cliff-Cliff. Mirror case because
* slope on right and flat on left.
*/
                TriangulateCornerCliffTerraces(
                    begin,
                    beginHex,
                    left,
                    leftHex,
                    right,
                    rightHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
                );
            }
        }

/* Neither the left or right hex edge type is a slope. If the right hex type of the left hex
* is a slope, then terraces must be calculated for a corner between two cliff edges.
* Cliff-Cliff-Slope Right, or Cliff-Cliff-Slope Left.
*/
        else if (leftHex.GetEdgeType(rightHex) == ElevationEdgeTypes.Slope) {

// If Cliff-Cliff-Slope-Left
            if (leftHex.elevation < rightHex.elevation) {
                TriangulateCornerCliffTerraces(
                    right,
                    rightHex,
                    begin,
                    beginHex,
                    left,
                    leftHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
                );
            }

// If Cliff-Cliff-Slope-Right
            else {
                TriangulateCornerTerracesCliff(
                    left,
                    leftHex,
                    right,
                    rightHex,
                    begin,
                    beginHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
                );
            }
        }

// Else all edges are cliffs. Simply draw a triangle.
        else {
            terrain.AddTrianglePerturbed(
                begin,
                left,
                right,
                hexOuterRadius,
                wrapSize
            );

            Vector3 indices;
            indices.x = beginHex.Index;
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
            begin,
            beginHex,
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
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
        Vector3 vertex3 = HexagonPoint.TerraceLerp(begin, left, 1);
        Vector3 vertex4 = HexagonPoint.TerraceLerp(begin, right, 1);
        Color weight3 = HexagonPoint.TerraceLerp(_weights1, _weights2, 1);
        Color weight4 = HexagonPoint.TerraceLerp(_weights1, _weights3, 1);

        Vector3 indices;

        indices.x = beginHex.Index;
        indices.y = leftHex.Index;
        indices.z = rightHex.Index;

        terrain.AddTrianglePerturbed(
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

            terrain.AddQuadPerturbed(
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

        terrain.AddQuadPerturbed(
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
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
/* Set boundary distance to 1 elevation level above the bottom-most hex
* in the case.
*/
        float boundaryDistance =
            1f / (rightHex.elevation - beginHex.elevation);

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

        //      begin hex
        //        | x
        //      vertex
        //    /y      \z
        // left hex   right hex
        indices.x = beginHex.Index;
        indices.y = leftHex.Index;
        indices.z = rightHex.Index;

        TriangulateBoundaryTriangle(
            begin,
            _weights1,
            left,
            _weights2,
            boundary,
            boundaryWeights,
            indices,
            hexOuterRadius,
            wrapSize,
            terrain
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
                wrapSize,
                terrain
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
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
/* Set boundary distance to 1 elevation level above the bottom-most hex
* in the case.
*/
        float boundaryDistance =
            1f / (leftHex.elevation - beginHex.elevation);

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
            wrapSize,
            terrain
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
                wrapSize,
                terrain
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

    private void TriangulateEdgeTerracesTerrain(
        EdgeVertices begin,
        Hex beginHex,
        EdgeVertices end,
        Hex endHex,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
        EdgeVertices edge2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color weight2 = HexagonPoint.TerraceLerp(_weights1, _weights2, 1);
        float index1 = beginHex.Index;
        float index2 = endHex.Index;

        TriangulateEdgeStripTerrain(
            begin,
            _weights1, 
            index1, 
            edge2, 
            weight2, 
            index2,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        for (int i = 2; i < HexagonPoint.terraceSteps; i++) {
            EdgeVertices edge1 = edge2;
            Color weight1 = weight2;
            edge2 = EdgeVertices.TerraceLerp(begin, end, i);
            weight2 = HexagonPoint.TerraceLerp(_weights1, _weights2, i);

            TriangulateEdgeStripTerrain(
                edge1, 
                weight1, 
                index1,
                edge2, 
                weight2, 
                index2,
                hexOuterRadius,
                wrapSize,
                terrain
            );
        }

        TriangulateEdgeStripTerrain(
            edge2, 
            weight2, 
            index1,
            end, 
            _weights2, 
            index2,
            hexOuterRadius,
            wrapSize,
            terrain
        );
    }

    private void TriangulateEdgeStripTerrain(
        EdgeVertices edge1,
        Color weight1,
        float index1,
        EdgeVertices edge2,
        Color weight2,
        float index2,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
        terrain.AddQuadPerturbed(
            edge1.vertex1,
            edge1.vertex2,
            edge2.vertex1,
            edge2.vertex2,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuadPerturbed(
            edge1.vertex2,
            edge1.vertex3,
            edge2.vertex2,
            edge2.vertex3,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuadPerturbed(
            edge1.vertex3,
            edge1.vertex4,
            edge2.vertex3,
            edge2.vertex4,
            hexOuterRadius,
            wrapSize
        );

        terrain.AddQuadPerturbed(
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
    }
}