using System.Collections.Generic;
using UnityEngine;

public class RoadsChunkLayer : MapMeshChunkLayer {
    public new static RoadsChunkLayer CreateEmpty(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Roads Chunk Layer");
        
        RoadsChunkLayer resultMono =
            resultObj.AddComponent<RoadsChunkLayer>();
        
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

    public TriangulationData TriangulateHexRoadEdge(
        Hex hex,
        Hex neighbor,
        TriangulationData triangulationData,
        HexDirections direction,
        HexRiverData riverData,
        FeatureContainer features,
        Dictionary<HexDirections, bool> roadEdges,
        Dictionary<HexDirections, ElevationEdgeTypes> elevationEdgeTypes,
        float hexOuterRadius,
        int wrapSize
    ) {
        triangulationData = TriangulateCenterRiverRoad(
            riverData,
            direction,
            hex,
            triangulationData,
            hexOuterRadius,
            wrapSize,
            this,
            roadEdges,
            features
        );

        if (direction <= HexDirections.Southeast) {
            triangulationData = TriangulateTerrainConnectionRoads(
                hex,
                neighbor,
                triangulationData,
                direction,
                riverData,
                roadEdges,
                elevationEdgeTypes,
                hexOuterRadius,
                wrapSize,
                this
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
                    data,
                    riverData,
                    roadEdges,
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

    private TriangulationData TriangulateTerrainConnectionRoads(
        Hex source,
        Hex neighbor,
        TriangulationData data,
        HexDirections direction,
        HexRiverData riverData,
        Dictionary<HexDirections, bool> roadEdges,
        Dictionary<HexDirections, ElevationEdgeTypes> elevationEdgeTypes,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads
    ) {
        bool hasRoad = roadEdges[direction];

        if (
//            hex.GetEdgeType(direction) == ElevationEdgeTypes.Slope
            elevationEdgeTypes[direction] == ElevationEdgeTypes.Slope
        ) {
            if (hasRoad) {
                TriangulateEdgeTerracesRoads(
                    data.centerEdgeVertices,
                    source,
                    data.connectionEdgeVertices,
                    neighbor,
                    hasRoad,
                    hexOuterRadius,
                    wrapSize,
                    roads
                );
            }
        }
        else {
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

        return data;
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

    private void TriangulateEdgeTerracesRoads(
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

    private TriangulationData TriangulateRoadAdjacentToRiver(
        Hex source,
        HexDirections direction, 
        TriangulationData data,
        HexRiverData riverData,
        Dictionary<HexDirections, bool> roadEdges,
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

        Vector3 roadCenter = data.terrainCenter;

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
                    return data;
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
                    return data;
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
                    data.terrainCenter - corner * 0.5f,
                    hexOuterRadius,
                    wrapSize
                );
            }
            
            data.terrainCenter += corner * 0.25f;
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
                return data;
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
            data.terrainCenter += offset * 0.5f;
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
                return data;
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
                    data.terrainCenter - offset * (
                        HexagonConstants.INNER_TO_OUTER_RATIO * 0.7f
                    ),
                    hexOuterRadius,
                    wrapSize
                );
            }
        }

        Vector3 middleLeft = 
            Vector3.Lerp(
                roadCenter,
                data.centerEdgeVertices.vertex1,
                interpolators.x
            );
        
        Vector3 middleRight =
            Vector3.Lerp(
                roadCenter,
                data.centerEdgeVertices.vertex5,
                interpolators.y
            );

        TriangulateRoad(
            roadCenter,
            middleLeft,
            middleRight,
            data.centerEdgeVertices,
            hasRoadThroughEdge,
            source.Index,
            hexOuterRadius,
            wrapSize,
            roads
        );

        if (previousHasRiver) {
            TriangulateRoadEdge(
                roadCenter,
                data.terrainCenter,
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
                data.terrainCenter,
                source.Index,
                hexOuterRadius,
                wrapSize,
                roads
            );
        }
        
        return data;
    }
}