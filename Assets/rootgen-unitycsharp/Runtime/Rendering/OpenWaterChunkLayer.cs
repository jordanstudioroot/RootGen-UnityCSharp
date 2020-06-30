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

    public WaterTriangulationData TriangulateHexOpenWaterEdge(
        Hex source,
        Hex neighbor,
        Dictionary<HexDirections, Hex> neighbors,
        HexDirections direction,
        WaterTriangulationData waterTriData,
        TerrainTriangulationData terrainTriData,
        float hexOuterRadius,
        int wrapSize
    ) {
        if (source.IsUnderwater) {
            if (
                !neighbor.IsUnderwater
            ) {
                waterTriData = TriangulateShoreOpenWater(
                    source,
                    neighbor,
                    waterTriData.waterSurfaceCenter,
                    waterTriData.sourceWaterEdge,
                    hexOuterRadius,
                    wrapSize,
                    this,
                    waterTriData
                );
            }
            else {
                waterTriData = TriangulateOpenWaterCenter(
                    source,
                    waterTriData,
                    direction,
                    hexOuterRadius,
                    wrapSize,
                    this
                );

                waterTriData = TriangulateOpenWaterConnection(
                    source,
                    neighbor,
                    direction,
                    waterTriData,
                    terrainTriData,
                    neighbors,
                    hexOuterRadius,
                    wrapSize,
                    this
                );
            }  
        }

        return waterTriData;
    }

    private WaterTriangulationData TriangulateShoreOpenWater(
        Hex source,
        Hex target,
        Vector3 waterSurfaceCenter,
        EdgeVertices sourceWaterEdge,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer water,
        WaterTriangulationData triangulationData
    ) {
        water.AddTrianglePerturbed(
            waterSurfaceCenter,
            sourceWaterEdge.vertex1,
            sourceWaterEdge.vertex2,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTrianglePerturbed(
            waterSurfaceCenter,
            sourceWaterEdge.vertex2,
            sourceWaterEdge.vertex3,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTrianglePerturbed(
            waterSurfaceCenter,
            sourceWaterEdge.vertex3,
            sourceWaterEdge.vertex4,
            hexOuterRadius,
            wrapSize
        );
        
        water.AddTrianglePerturbed(
            waterSurfaceCenter,
            sourceWaterEdge.vertex4,
            sourceWaterEdge.vertex5,
            hexOuterRadius,
            wrapSize
        );

        //            / | y
        //           /  |
        //           |  |
        //source x/z |  | target
        //           |  |
        //           \  |
        //            \ | y
        
        Vector3 waterShoreHexIndices;
        
        waterShoreHexIndices.x =
            waterShoreHexIndices.z = source.Index;

        waterShoreHexIndices.y = target.Index;

        water.AddTriangleHexData(
            waterShoreHexIndices,
            _weights1
        );
        
        water.AddTriangleHexData(
            waterShoreHexIndices,
            _weights1
        );
        
        water.AddTriangleHexData(
            waterShoreHexIndices,
            _weights1
        );
        
        water.AddTriangleHexData(
            waterShoreHexIndices,
            _weights1
        );

        return triangulationData;
    }

    private WaterTriangulationData TriangulateOpenWaterCenter(
        Hex source,
        WaterTriangulationData triangulationData,
        HexDirections direction,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer water
    ) {
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

        Vector3 openWaterCenterIndices;

        openWaterCenterIndices.x =
            openWaterCenterIndices.y =
                openWaterCenterIndices.z =
                    source.Index;

        water.AddTriangleHexData(
            openWaterCenterIndices,
            _weights1
        );

        return triangulationData;
    }

    private WaterTriangulationData TriangulateOpenWaterConnection(
        Hex source,
        Hex target,
        HexDirections direction,
        WaterTriangulationData waterTriData,
        TerrainTriangulationData terrainTriData,
        Dictionary<HexDirections, Hex> neighbors,
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

            Vector3 edge1 = waterTriData.waterSurfaceCornerLeft + bridge;
            Vector3 edge2 = waterTriData.waterSurfaceCornerRight + bridge;

            water.AddQuadPerturbed(
                waterTriData.waterSurfaceCornerLeft,
                waterTriData.waterSurfaceCornerRight,
                edge1,
                edge2,
                hexOuterRadius,
                wrapSize
            );

            Vector3 openWaterIndices;
            openWaterIndices.x =
                openWaterIndices.z =
                    source.Index;

            openWaterIndices.y = target.Index;
                        
            water.AddQuadHexData(
                openWaterIndices,
                _weights1,
                _weights2
            );

            if (direction <= HexDirections.East) {
                Hex nextNeighbor;

                if (
                    neighbors.TryGetValue(
                        direction.NextClockwise(),
                        out nextNeighbor
                    ) &&
                    nextNeighbor.IsUnderwater
                ) {
                    water.AddTrianglePerturbed(
                        waterTriData.waterSurfaceCornerRight, 
                        edge2, 
                        waterTriData.waterSurfaceCornerRight +
                        HexagonPoint.GetWaterBridge(
                            direction.NextClockwise(),
                            hexOuterRadius
                        ),
                        hexOuterRadius,
                        wrapSize
                    );

                    openWaterIndices.z =
                        nextNeighbor.Index;

                    water.AddTriangleHexData(
                        openWaterIndices,
                        _weights1,
                        _weights2,
                        _weights3
                    );
                }
            }
        }

        return waterTriData;
    }
}