using System.Collections.Generic;
using UnityEngine;

public class OpenWaterChunkLayer : MapMeshChunkLayer {
    public new static OpenWaterChunkLayer CreateEmpty(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Open Water Chunk Layer");
        OpenWaterChunkLayer resultMono =
            resultObj.AddComponent<OpenWaterChunkLayer>();
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

    public TriangulationData TriangulateHexOpenWaterEdge(
        Hex source,
        Hex neighbor,
        Dictionary<HexDirections, Hex> neighbors,
        HexDirections direction,
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
                triangulationData = TriangulateShoreOpenWater(
                    source,
                    neighbor,
                    triangulationData.waterSurfaceCenter,
                    triangulationData.sourceWaterEdge,
                    hexOuterRadius,
                    wrapSize,
                    this,
                    triangulationData
                );
            }
            else {
                triangulationData = TriangulateOpenWaterCenter(
                    source,
                    triangulationData,
                    direction,
                    hexOuterRadius,
                    wrapSize,
                    this
                );

                TriangulateOpenWaterConnection(
                    source,
                    neighbor,
                    direction,
                    neighbors,
                    triangulationData.waterSurfaceCornerLeft,
                    triangulationData.waterSurfaceCornerRight,
                    triangulationData.terrainSourceRelativeHexIndices,
                    hexOuterRadius,
                    wrapSize,
                    this
                );
            }  
        }

        return triangulationData;
    }

    private TriangulationData TriangulateShoreOpenWater(
        Hex source,
        Hex target,
        Vector3 center,
        EdgeVertices edge1,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer water,
        TriangulationData triangulationData
    ) {
        water.AddTrianglePerturbed(
            center,
            edge1.vertex1,
            edge1.vertex2,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTrianglePerturbed(
            center,
            edge1.vertex2,
            edge1.vertex3,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTrianglePerturbed(
            center,
            edge1.vertex3,
            edge1.vertex4,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTrianglePerturbed(
            center,
            edge1.vertex4,
            edge1.vertex5,
            hexOuterRadius,
            wrapSize
        );

        triangulationData.waterSourceRelativeHexIndices = new Vector3(
            source.Index,
            target.Index,
            source.Index
        );

        water.AddTriangleHexData(
            triangulationData.waterSourceRelativeHexIndices,
            _weights1
        );
        
        water.AddTriangleHexData(
            triangulationData.waterSourceRelativeHexIndices,
            _weights1
        );
        
        water.AddTriangleHexData(
            triangulationData.waterSourceRelativeHexIndices,
            _weights1
        );
        
        water.AddTriangleHexData(
            triangulationData.waterSourceRelativeHexIndices,
            _weights1
        );

        return triangulationData;
    }

    private TriangulationData TriangulateOpenWaterCenter(
        Hex source,
        TriangulationData triangulationData,
        HexDirections direction,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer water
    ) {
        triangulationData.waterSurfaceCenter = source.Position;
        triangulationData.waterSurfaceCenter.y = source.WaterSurfaceY;

        triangulationData.waterSurfaceCornerLeft =
            triangulationData.waterSurfaceCenter +
            HexagonPoint.GetFirstWaterCorner(
                direction,
                hexOuterRadius
            );

        triangulationData.waterSurfaceCornerRight =
            triangulationData.waterSurfaceCenter +
            HexagonPoint.GetSecondWaterCorner(
                direction,
                hexOuterRadius
            );

        water.AddTrianglePerturbed(
            triangulationData.waterSurfaceCenter,
            triangulationData.waterSurfaceCornerLeft,
            triangulationData.waterSurfaceCornerRight,
            hexOuterRadius,
            wrapSize
        );

        triangulationData.waterSourceRelativeHexIndices = new Vector3(
            source.Index,
            source.Index,
            source.Index
        );

        water.AddTriangleHexData(
            triangulationData.waterSourceRelativeHexIndices,
            _weights1
        );

        return triangulationData;
    }

    private void TriangulateOpenWaterConnection(
        Hex source,
        Hex target,
        HexDirections direction,
        Dictionary<HexDirections, Hex> neighbors,
        Vector3 center1,
        Vector3 center2,
        Vector3 indices,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer water
    ) {
        if (
            direction <= HexDirections.Southeast
        ) {
            Vector3 bridge = HexagonPoint.GetWaterBridge(
                direction,
                hexOuterRadius
            );

            Vector3 edge1 = center1 + bridge;
            Vector3 edge2 = center2 + bridge;

            water.AddQuadPerturbed(
                center1,
                center2,
                edge1,
                edge2,
                hexOuterRadius,
                wrapSize
            );
            
            indices.y = target.Index;
            water.AddQuadHexData(indices, _weights1, _weights2);

            if (direction <= HexDirections.East) {
                Hex nextNeighbor;

//                    hex.GetNeighbor(direction.NextClockwise());
                if (
                    neighbors.TryGetValue(
                        direction.NextClockwise(),
                        out nextNeighbor
                    ) &&
                    nextNeighbor.IsUnderwater
                ) {
                    water.AddTrianglePerturbed(
                        center2, 
                        edge2, 
                        center2 + HexagonPoint.GetWaterBridge(
                            direction.NextClockwise(),
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
    }
}