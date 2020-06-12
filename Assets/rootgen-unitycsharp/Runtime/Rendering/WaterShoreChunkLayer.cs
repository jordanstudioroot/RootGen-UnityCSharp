using System.Collections.Generic;
using UnityEngine;

public class WaterShoreChunkLayer : MapMeshChunkLayer {
    public new static WaterShoreChunkLayer CreateEmpty(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Water Shore Chunk Layer");
        WaterShoreChunkLayer resultMono =
            resultObj.AddComponent<WaterShoreChunkLayer>();
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

    public TriangulationData TriangulateHexWaterShoreEdge(
        Hex source,
        Hex neighbor,
        Dictionary<HexDirections, Hex> neighbors,
        HexDirections direction,
        HexRiverData riverData,
        TriangulationData triangulationData,
        float hexOuterRadius,
        int wrapSize
    ) {
        if (source.IsUnderwater) {
            triangulationData.waterSurfaceCenter = source.Position;
            triangulationData.waterSurfaceCenter.y = source.WaterSurfaceY;

            if (
                !neighbor.IsUnderwater
            ) {
                Vector3 center2 = neighbor.Position;

                float hexInnerRadius =
                    HexagonPoint.OuterToInnerRadius(hexOuterRadius);
                
                float hexInnerDiameter = hexInnerRadius * 2f;


                TriangulateWaterShore(
                    source,
                    neighbor,
                    triangulationData.waterSourceRelativeHexIndices,
                    direction,
                    neighbors,
                    riverData,
                    triangulationData.waterSurfaceCenter,
                    hexOuterRadius,
                    wrapSize,
                    this,
                    triangulationData.sourceWaterEdge,
                    triangulationData.neighborWaterEdge,
                    hexInnerDiameter
                );
            }  
        }

        return triangulationData;
    }

    private void TriangulateWaterShore(
        Hex source,
        Hex target,
        Vector3 indices,
        HexDirections direction,
        Dictionary<HexDirections, Hex> neighbors,
        HexRiverData riverData,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer waterShore,
        EdgeVertices edge1,
        EdgeVertices edge2,
        float hexInnerDiameter
    ) {
//          hex.HasRiverThroughEdge(direction)
        if (riverData.HasRiverInDirection(direction)) {
            TriangulateWaterShoreWithRiver(
                edge1,
                edge2,
                riverData.HasIncomingRiverInDirection(direction),
                indices,
                hexOuterRadius,
                wrapSize,
                waterShore
            );
        }
        else {
            waterShore.AddQuadPerturbed(
                edge1.vertex1,
                edge1.vertex2,
                edge2.vertex1,
                edge2.vertex2,
                hexOuterRadius,
                wrapSize
            );

            waterShore.AddQuadPerturbed(
                edge1.vertex2,
                edge1.vertex3,
                edge2.vertex2,
                edge2.vertex3,
                hexOuterRadius,
                wrapSize
            );

            waterShore.AddQuadPerturbed(
                edge1.vertex3,
                edge1.vertex4,
                edge2.vertex3,
                edge2.vertex4,
                hexOuterRadius,
                wrapSize
            );

            waterShore.AddQuadPerturbed(
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

            waterShore.AddQuadHexData(
                indices,
                _weights1,
                _weights2
            );
            
            waterShore.AddQuadHexData(
                indices,
                _weights1,
                _weights2
            );
            
            waterShore.AddQuadHexData(
                indices,
                _weights1,
                _weights2
            );

            waterShore.AddQuadHexData(
                indices,
                _weights1,
                _weights2
            );
        }
        
        Hex nextNeighbor;
//            hex.GetNeighbor(direction.NextClockwise());

        if (
            neighbors.TryGetValue(
                direction.NextClockwise(),
                out nextNeighbor
            )
        ) {
            Vector3 center3 = nextNeighbor.Position;

            if (nextNeighbor.ColumnIndex < source.ColumnIndex - 1) {
                center3.x += wrapSize * hexInnerDiameter;
            }
            else if (nextNeighbor.ColumnIndex > source.ColumnIndex + 1) {
                center3.x -= wrapSize * hexInnerDiameter;
            }

// Work backward from the shore to obtain the triangle if the neighbor is
// underwater, otherwise obtain normal triangle.

            Vector3 vertex3 = 
                center3 + (
                    nextNeighbor.IsUnderwater ?
                    HexagonPoint.GetFirstWaterCorner(
                        direction.PreviousClockwise(),
                        hexOuterRadius
                    ) :
                    HexagonPoint.GetFirstSolidCorner(
                        direction.PreviousClockwise(),
                        hexOuterRadius
                    )
                );

            vertex3.y = center.y;

            waterShore.AddTrianglePerturbed (
                edge1.vertex5,
                edge2.vertex5,
                vertex3,
                hexOuterRadius,
                wrapSize
            );

            indices.z = nextNeighbor.Index;

            waterShore.AddTriangleHexData (
                indices,
                _weights1,
                _weights2,
                _weights3
            );

            waterShore.AddTriangleUV (
                new Vector2(0f, 0f),
                new Vector2(0f, 1f),
                new Vector2(0f, nextNeighbor.IsUnderwater ? 0f : 1f)
            );
        }
    }

    private void TriangulateWaterShoreWithRiver(
        EdgeVertices edge1,
        EdgeVertices edge2,
        bool incomingRiver,
        Vector3 indices,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer waterShore
    ) {
        waterShore.AddTrianglePerturbed(
            edge2.vertex1,
            edge1.vertex2,
            edge1.vertex1,
            hexOuterRadius,
            wrapSize
        );

        waterShore.AddTrianglePerturbed(
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
    }
}