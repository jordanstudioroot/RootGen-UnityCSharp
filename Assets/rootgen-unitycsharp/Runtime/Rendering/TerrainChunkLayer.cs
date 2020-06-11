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
        GameObject resultObj = new GameObject("Map Mesh Chunk Layer");
        
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

    public TriangulationData TriangulateHexTerrainEdge(
        Hex hex,
        Hex neighbor,
        TriangulationData triangulationData,
        Dictionary<HexDirections, Hex> neighbors,
        HexDirections direction,
        HexRiverData riverData,
        MapMeshChunkLayer roads,
        FeatureContainer features,
        Dictionary<HexDirections, bool> roadEdges,
        Dictionary<HexDirections, ElevationEdgeTypes> elevationEdgeTypes,
        float hexOuterRadius,
        int wrapSize
    ) {          
        triangulationData.centerEdgeVertices =
            GetCenterEdgeVertices(
                direction,
                triangulationData,
                hexOuterRadius
            );

        triangulationData = TriangulateCenterRiverBed(
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

        triangulationData = TriangulateCenterRiverRoad(
            riverData,
            direction,
            hex,
            triangulationData,
            hexOuterRadius,
            wrapSize,
            roads,
            roadEdges,
            features
        );

        if (direction <= HexDirections.Southeast) {
            triangulationData.connectionEdgeVertices =
                GetConnectionEdgeVertices(
                    hex,
                    neighbor,
                    direction,
                    triangulationData.centerEdgeVertices,
                    hexOuterRadius
                );
            
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
                roads,
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

    private TriangulationData TriangulateCenterRiverRoad(
        HexRiverData riverData,
        HexDirections direction,
        Hex source,
        TriangulationData data,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads,
        Dictionary<HexDirections, bool> roadEdges,
        FeatureContainer features
    ) {
        if (riverData.HasRiver) {
            bool hasRoad = false;

            foreach (
                KeyValuePair<HexDirections, bool> pair in roadEdges
            ) {
                if (pair.Value) {
                    hasRoad = true;
                    break;
                }
            }

            if (hasRoad) {
                TriangulateRoadAdjacentToRiver(
                    source,
                    direction,
                    data.terrainCenter,
                    riverData,
                    roadEdges,
                    data.centerEdgeVertices,
                    hexOuterRadius,
                    wrapSize,
                    roads,
                    features
                );
            }
        }
        else {
            bool anyRoad = false;

            foreach (
                KeyValuePair<HexDirections, bool> pair in roadEdges
            ) {
                if (pair.Value) {
                    anyRoad = true;
                    break;
                }
            }
            
            if (anyRoad) {
                TriangulateRoadWithoutRiver(
                    source,
                    direction,
                    data.centerEdgeVertices,
                    roadEdges,
                    data.terrainCenter,
                    hexOuterRadius,
                    wrapSize,
                    roads
                );
            }
        }

        return data;
    }

    private TriangulationData TriangulateCenterRiverBed(
        HexRiverData riverData,
        HexDirections direction,
        Hex source,
        TriangulationData data,
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
                        data.terrainCenter,
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
                /*bool hasRoad = false;

                foreach (
                    KeyValuePair<HexDirections, bool> pair in roadEdges
                ) {
                    if (pair.Value) {
                        hasRoad = true;
                        break;
                    }
                }

                if (hasRoad) {
                    TriangulateRoadAdjacentToRiver(
                        source,
                        direction,
                        data.terrainCenter,
                        riverData,
                        roadEdges,
                        data.centerEdgeVertices,
                        hexOuterRadius,
                        wrapSize,
                        roads,
                        features
                    );
                }*/

                TriangulateTerrainAdjacentToRiver(
                    source,
                    direction,
                    data.terrainCenter,
                    data.centerEdgeVertices,
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
            /*bool anyRoad = false;

            foreach (
                KeyValuePair<HexDirections, bool> pair in roadEdges
            ) {
                if (pair.Value) {
                    anyRoad = true;
                    break;
                }
            }
            
            if (anyRoad) {
                TriangulateRoadWithoutRiver(
                    source,
                    direction,
                    data.centerEdgeVertices,
                    roadEdges,
                    data.terrainCenter,
                    hexOuterRadius,
                    wrapSize,
                    roads
                );
            }*/
            
            // Triangulate terrain center without river, basic edge fan.
            TriangulateEdgeFan(
                data.terrainCenter,
                data.centerEdgeVertices,
                source.Index,
                hexOuterRadius,
                wrapSize,
                terrain
            );

            /*TriangulateTerrainWithoutRiver(
                source,
                direction,
                data.centerEdgeVertices,
                roadEdges,
                data.terrainCenter,
                hexOuterRadius,
                wrapSize,
                terrain,
                roads
            );*/

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

    private TriangulationData TriangulateRiverBeginOrEndTerrain(
        Hex source,
        Vector3 center,
        TriangulationData data,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain
    ) {
        data.middleEdgeVertices = new EdgeVertices(
            Vector3.Lerp(center, data.centerEdgeVertices.vertex1, 0.5f),
            Vector3.Lerp(center, data.centerEdgeVertices.vertex5, 0.5f)
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
            center,
            data.middleEdgeVertices,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        return data;
    }

    private TriangulationData TriangulateTerrainConnection(
        Hex source,
        Hex neighbor,
        TriangulationData data,
        HexDirections direction,
        HexRiverData riverData,
        Dictionary<HexDirections, bool> roadEdges,
        Dictionary<HexDirections, ElevationEdgeTypes> elevationEdgeTypes,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        MapMeshChunkLayer roads,
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
                hasRoad,
                hexOuterRadius,
                wrapSize,
                terrain,
                roads
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

            if (hasRoad) {
                TriangulateEdgeStripRoads(
                    data.centerEdgeVertices,
                    _weights1,
                    source.Index,
                    data.connectionEdgeVertices,
                    _weights2,
                    neighbor.Index,
                    hexOuterRadius,
                    wrapSize,
                    roads
                );
            }
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

    private TriangulationData TriangulateRiverBanks(
        TriangulationData data,
        HexRiverData riverData,
        HexDirections direction,
        float hexOuterRadius
    ) {
        //        if (hex.HasRiverThroughEdge(direction.Opposite())) {
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
//        else if (hex.HasRiverThroughEdge(direction.Next())) {
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
//        else if (hex.HasRiverThroughEdge(direction.Previous())) {
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
//        else if (hex.HasRiverThroughEdge(direction.Next2())) {
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

    private TriangulationData TriangulateRiverTerrain(
        Hex source,
        TriangulationData triangulationData,
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

        triangulationData.terrainSourceHexIndex =
            triangulationData.terrainLeftHexIndex =
                triangulationData.terrainRightHexIndex =
                    source.Index;

        terrain.AddTriangleHexData(
            triangulationData.terrainSourceRelativeHexIndices,
            _weights1
        );

        terrain.AddQuadHexData(
            triangulationData.terrainSourceRelativeHexIndices,
            _weights1
        );

        terrain.AddQuadHexData(
            triangulationData.terrainSourceRelativeHexIndices,
            _weights1
        );

        terrain.AddTriangleHexData(
            triangulationData.terrainSourceRelativeHexIndices,
            _weights1
        );

        return triangulationData;
    }

    private void TriangulateTerrainAdjacentToRiver(
        Hex source,
        HexDirections direction,
        Vector3 center,
        EdgeVertices edgeVertices,
        Dictionary<HexDirections, bool> roadEdges,
        HexRiverData riverData,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
    ) {
//        if (hex.HasRiverThroughEdge(direction.Next())) {
        if (riverData.HasRiverInDirection(direction.NextClockwise())) {
/* If the direction has a river on either side, it has a slight curve. 
* The center vertex of river-adjacent triangle needs to be moved toward 
* the edge so they don't overlap the river.
*/
//            if (hex.HasRiverThroughEdge(direction.Previous())) {
            if (
                riverData.HasRiverInDirection(direction.PreviousClockwise())
            ) {
                center += HexagonPoint.GetSolidEdgeMiddle(
                    direction,
                    hexOuterRadius
                ) * (HexagonConstants.INNER_TO_OUTER_RATIO * 0.5f);
            }

/* If the hex has a river through the previous previous direction,
* it has a river flowing through the hex. Move the center vertex
* of the river-adjacent triangle so that it does not overlap the river.
*/
            else if (
//                hex.HasRiverThroughEdge(direction.Previous2())
                riverData.HasRiverInDirection(
                    direction.PreviousClockwise2()
                )
            ) {
                center +=
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
//            hex.HasRiverThroughEdge(direction.Previous()) &&
            riverData.HasRiverInDirection(direction.PreviousClockwise()) &&
//            hex.HasRiverThroughEdge(direction.Next2())
            riverData.HasRiverInDirection(direction.NextClockwise2())
        ) {
            center += HexagonPoint.GetSecondSolidCorner(
                direction,
                hexOuterRadius
            ) * 0.25f;
        }

        EdgeVertices middle = new EdgeVertices(
            Vector3.Lerp(center, edgeVertices.vertex1, 0.5f),
            Vector3.Lerp(center, edgeVertices.vertex5, 0.5f)
        );

        TriangulateEdgeStripTerrain(
            middle,
            _weights1,
            source.Index,
            edgeVertices,
            _weights1,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        TriangulateEdgeFan(
            center,
            middle,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

//            !hex.HasRoadThroughEdge(direction)
        if (!source.IsUnderwater && roadEdges[direction]) {
            features.AddFeature(
                source,
                (center + edgeVertices.vertex1 + edgeVertices.vertex5) * (1f / 3f),
                hexOuterRadius,
                wrapSize
            );
        }
    }

    private void TriangulateRoadAdjacentToRiver(
        Hex source,
        HexDirections direction, 
        Vector3 center,
        HexRiverData riverData,
        Dictionary<HexDirections, bool> roadEdges,
        EdgeVertices edgeVertices,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads,
        FeatureContainer features
    ) {
//        bool hasRoadThroughEdge = hex.HasRoadThroughEdge(direction);
        bool hasRoadThroughEdge = roadEdges[direction];

//          bool previousHasRiver = hex.HasRiverThroughEdge(
//              direction.Previous()
//          );
        bool previousHasRiver = riverData.HasIncomingRiverInDirection(
            direction.PreviousClockwise()
        );

//        bool nextHasRiver = hex.HasRiverThroughEdge(direction.Next());
        bool nextHasRiver = riverData.HasIncomingRiverInDirection(
            direction.NextClockwise()
        );

        Vector2 interpolators = GetRoadInterpolators(
            source,
            direction,
            roadEdges
        );

        Vector3 roadCenter = center;

//        if (hex.HasRiverBeginOrEnd) {
        if (riverData.HasRiverStartOrEnd) {
            roadCenter += 
                HexagonPoint.GetSolidEdgeMiddle(
//                    hex.RiverBeginOrEndDirection.Opposite(),
                    riverData.RiverStartOrEndDirection.Opposite(),
                    hexOuterRadius
                ) * 
                (1f / 3f);
        }
//        else if(hex.IncomingRiver == hex.OutgoingRiver.Opposite()) {
        else if (
            riverData.HasStraightRiver
        ) {
            Vector3 corner;

//  If the previous hex has a river, the corner the center will be
//  moved toward is equal to the current direction + 1.
            if (previousHasRiver) {
                if (
                    !hasRoadThroughEdge &&
//                    !hex.HasRoadThroughEdge(direction.Next())
                    !roadEdges[direction.NextClockwise()]
                ) {
                    return;
                }
                corner = HexagonPoint.GetSecondSolidCorner(
                    direction,
                    hexOuterRadius
                );
            }
// If the previous hex does not have a river, the corner the center will
// be moved toward is the same index as the current direction.
            else {
                if (
                    !hasRoadThroughEdge &&
//                    !hex.HasRoadThroughEdge(direction.Previous())
                    !roadEdges[direction.PreviousClockwise()]
                ) {
                    return;
                }

                corner = HexagonPoint.GetFirstSolidCorner(
                    direction,
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
                riverData.IncomingRivers[direction.NextClockwise()] &&
//                hex.HasRoadThroughEdge(direction.Next2()) ||
                roadEdges[direction.NextClockwise2()] ||
//                hex.HasRoadThroughEdge(direction.Opposite())
                roadEdges[direction.Opposite()]
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
          else if (riverData.HasPreviousClockwiseCornerRiver) {
            roadCenter -= HexagonPoint.GetSecondCorner(
//                hex.IncomingRiver,
                riverData.AnyIncomingRiver,
                hexOuterRadius
            ) * 0.2f;
        }
//        else if (hex.IncomingRiver == hex.OutgoingRiver.Next()) {
        else if (riverData.HasNextClockwiseCornerRiver) {
            roadCenter -= HexagonPoint.GetFirstCorner(
//                hex.IncomingRiver,
                riverData.AnyIncomingRiver,
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
                    direction,
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
                middle = direction.NextClockwise();
            }
            else if (nextHasRiver) {
//                middle = direction.Previous();
                middle = direction.PreviousClockwise();
            }
            else {
//                middle = direction;
                middle = direction;
            }

// If there is no road through any of the hexes on the outer side of the
// river bend, then the road center need not move and should instead be
// pruned.
            if (
//                !hex.HasRoadThroughEdge(middle) &&
                !roadEdges[middle] &&   
//                !hex.HasRoadThroughEdge(middle.Previous()) &&
                !roadEdges[middle.PreviousClockwise()] &&
//                !hex.HasRoadThroughEdge(middle.Next())
                !roadEdges[middle.NextClockwise()]
            ) {
                return;
            }

            Vector3 offset = HexagonPoint.GetSolidEdgeMiddle(
                middle,
                hexOuterRadius
            );

            roadCenter += offset * 0.25f;

            if (
                direction == middle &&
//                hex.HasRoadThroughEdge(direction.Opposite())
                roadEdges[direction.Opposite()]
            ) {
                features.AddBridge (
                    roadCenter,
                    center - offset * (
                        HexagonConstants.INNER_TO_OUTER_RATIO * 0.7f
                    ),
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
            source.Index,
            hexOuterRadius,
            wrapSize,
            roads
        );

        if (previousHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                center,
                middleLeft,
                source.Index,
                hexOuterRadius,
                wrapSize,
                roads
            );
        }

        if (nextHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                middleRight,
                center,
                source.Index,
                hexOuterRadius,
                wrapSize,
                roads
            );
        }
    }

    private Vector2 GetRoadInterpolators(
        Hex source,
        HexDirections direction,
        Dictionary<HexDirections, bool> roadEdges
    ) {
        Vector2 interpolators;

//        if (hex.HasRoadThroughEdge(direction)) {
        if (roadEdges[direction]) {
            interpolators.x = interpolators.y = 0.5f;
        }
        else {
            interpolators.x =
//              hex.HasRoadThroughEdge(direction.Previous()) ?
//                  0.5f : 0.25f;
                roadEdges[direction.PreviousClockwise()] ?
                0.5f : 0.25f;
            
            interpolators.y =
//              hex.HasRoadThroughEdge(direction.Next()) ?
//                  0.5f : 0.25f;
                roadEdges[direction.NextClockwise()] ?
                0.5f : 0.25f;
        }

        return interpolators;
    }

    private void TriangulateRoad(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight, 
        EdgeVertices edgeVertices,
        bool hasRoadThroughHexEdge,
        float index,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads
    ) {
        Vector3 indices;
        indices.x = indices.y = indices.z = index;

        if (hasRoadThroughHexEdge) {
            Vector3 middleCenter = Vector3.Lerp(middleLeft, middleRight, 0.5f);

            TriangulateRoadSegment(
                middleLeft,
                middleCenter,
                middleRight,
                edgeVertices.vertex2,
                edgeVertices.vertex3,
                edgeVertices.vertex4,
                _weights1,
                _weights1,
                indices,
                hexOuterRadius,
                wrapSize,
                roads
            );

            roads.AddTrianglePerturbed(
                center,
                middleLeft,
                middleCenter,
                hexOuterRadius,
                wrapSize
            );
            
            roads.AddTrianglePerturbed(
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
                wrapSize,
                roads
            );
        }
    }

    private void TriangulateRoadEdge(
        Vector3 center, 
        Vector3 middleLeft, 
        Vector3 middleRight,
        float index,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads
    ) {
        roads.AddTrianglePerturbed(
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

    private void TriangulateTerrainWithoutRiver(
        Hex source,
        HexDirections direction,
        EdgeVertices edgeVertices,
        Dictionary<HexDirections, bool> roadEdges,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        MapMeshChunkLayer roads
    ) {
        TriangulateEdgeFan(
            center,
            edgeVertices,
            source.Index,
            hexOuterRadius,
            wrapSize,
            terrain
        );

        /*bool anyRoad = false;

        foreach (KeyValuePair<HexDirections, bool> pair in roadEdges) {
            if (pair.Value) {
                anyRoad = true;
                break;
            }
        }

//        if (hex.HasRoads) {
        if (anyRoad) {
            Vector2 interpolators = GetRoadInterpolators(
                source,
                direction,
                roadEdges
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
                roadEdges[direction],
                source.Index,
                hexOuterRadius,
                wrapSize,
                roads
            );
        }*/
    }

    private void TriangulateRoadWithoutRiver(
        Hex source,
        HexDirections direction,
        EdgeVertices edgeVertices,
        Dictionary<HexDirections, bool> roadEdges,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads
    ) {
        Vector2 interpolators = GetRoadInterpolators(
            source,
            direction,
            roadEdges
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
            roadEdges[direction],
            source.Index,
            hexOuterRadius,
            wrapSize,
            roads
        );
    }

    private TriangulationData TryTriangulateNeighborTerrainCorner(
        Hex source,
        Hex neighbor,
        TriangulationData data,
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
                data.centerEdgeVertices,
                data.connectionEdgeVertices,
                hexOuterRadius,
                wrapSize,
                terrain,
                features
            );
        }

        return data;
    }

    private void TriangulateNeighborTerrainCorner(
        Hex source,
        Hex neighbor,
        Hex nextNeighbor,
        HexDirections direction,
        EdgeVertices centerEdgeVertices,
        EdgeVertices connectionEdgeVertices,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
    ) {
// Create a 5th vertex and assign it with the elevation of the neighbor
// under consideration. This will be used as the final vertex in the
// triangle which fills the gap between bridges.
        Vector3 vertex5 =
            centerEdgeVertices.vertex5 + HexagonPoint.GetBridge(
                direction.NextClockwise(),
                hexOuterRadius
            );

        vertex5.y = nextNeighbor.Position.y;

        if (source.elevation <= neighbor.elevation) {
            if (source.elevation <= nextNeighbor.elevation) {

// This hex has lowest elevation, no rotation.
                TriangulateTerrainCorner(
                    centerEdgeVertices.vertex5,
                    source,
                    connectionEdgeVertices.vertex5,
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
                    centerEdgeVertices.vertex5,
                    source,
                    connectionEdgeVertices.vertex5,
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
                connectionEdgeVertices.vertex5,
                neighbor,
                vertex5,
                nextNeighbor,
                centerEdgeVertices.vertex5,
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
                centerEdgeVertices.vertex5,
                source,
                connectionEdgeVertices.vertex5,
                neighbor,
                hexOuterRadius,
                wrapSize,
                terrain,
                features
            );
        }
    }

    private void TriangulateTerrainCorner(
        Vector3 bottom,
        Hex bottomHex,
        Vector3 left,
        Hex leftHex,
        Vector3 right,
        Hex rightHex,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        FeatureContainer features
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
                    bottom,
                    bottomHex,
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
                    bottom,
                    bottomHex,
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
                    bottom,
                    bottomHex,
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
                    bottom,
                    bottomHex,
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
                    bottom,
                    bottomHex,
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
                    bottom,
                    bottomHex,
                    hexOuterRadius,
                    wrapSize,
                    terrain
                );
            }
        }

// Else all edges are cliffs. Simply draw a triangle.
        else {
            terrain.AddTrianglePerturbed(
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
        bool hasRoad,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer terrain,
        MapMeshChunkLayer roads
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

        if (hasRoad) {
            TriangulateEdgeStripRoads(
                begin,
                _weights1,
                index1,
                edge2,
                weight2,
                index2,
                hexOuterRadius,
                wrapSize,
                roads
            );
        }

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

            if (hasRoad) {
                TriangulateEdgeStripRoads(
                    edge1,
                    weight1,
                    index1,
                    edge2,
                    weight2,
                    index2,
                    hexOuterRadius,
                    wrapSize,
                    roads
                );
            }
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

        if (hasRoad) {
            TriangulateEdgeStripRoads(
                edge2,
                weight2,
                index1,
                end,
                _weights2,
                index2,
                hexOuterRadius,
                wrapSize,
                roads
            );
        }
    }

    private void TriangulateEdgeTerracesRoad(
        EdgeVertices begin,
        Hex beginHex,
        EdgeVertices end,
        Hex endHex,
        bool hasRoad,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads
    ) {
        EdgeVertices edge2 = EdgeVertices.TerraceLerp(begin, end, 1);
        Color weight2 = HexagonPoint.TerraceLerp(_weights1, _weights2, 1);
        float index1 = beginHex.Index;
        float index2 = endHex.Index;

        TriangulateEdgeStripRoads(
            begin,
            _weights1,
            index1,
            edge2,
            weight2,
            index2,
            hexOuterRadius,
            wrapSize,
            roads
        );

        for (int i = 2; i < HexagonPoint.terraceSteps; i++) {
            EdgeVertices edge1 = edge2;
            Color weight1 = weight2;
            edge2 = EdgeVertices.TerraceLerp(begin, end, i);
            weight2 = HexagonPoint.TerraceLerp(_weights1, _weights2, i);

            TriangulateEdgeStripRoads(
                edge1, 
                weight1, 
                index1,
                edge2, 
                weight2, 
                index2,
                hexOuterRadius,
                wrapSize,
                roads
            );
        }

        TriangulateEdgeStripRoads(
            edge2, 
            weight2, 
            index1,
            end, 
            _weights2, 
            index2,
            hexOuterRadius,
            wrapSize,
            roads
        );
    }

    private void TriangulateEdgeStripRoads(
        EdgeVertices edge1,
        Color weight1,
        float index1,
        EdgeVertices edge2,
        Color weight2,
        float index2,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads
    ) {
        Vector3 indices;
        indices.x = indices.z = index1;
        indices.y = index2;

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
            wrapSize,
            roads
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

        /*if (hasRoad) {
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
                wrapSize,
                roads
            );
        }*/
    }
}