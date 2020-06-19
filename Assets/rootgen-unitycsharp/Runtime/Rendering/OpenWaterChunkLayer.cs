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

                triangulationData = TriangulateOpenWaterConnection(
                    source,
                    neighbor,
                    direction,
                    triangulationData,
                    neighbors,
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
        Vector3 waterSurfaceCenter,
        EdgeVertices sourceWaterEdge,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer water,
        TriangulationData triangulationData
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

    private TriangulationData TriangulateOpenWaterConnection(
        Hex source,
        Hex target,
        HexDirections direction,
        TriangulationData data,
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

            Vector3 edge1 = data.waterSurfaceCornerLeft + bridge;
            Vector3 edge2 = data.waterSurfaceCornerRight + bridge;

            water.AddQuadPerturbed(
                data.waterSurfaceCornerLeft,
                data.waterSurfaceCornerRight,
                edge1,
                edge2,
                hexOuterRadius,
                wrapSize
            );
            
            data.terrainSourceRelativeHexIndices.y = target.Index;
            water.AddQuadHexData(
                data.terrainSourceRelativeHexIndices,
                _weights1,
                _weights2
            );

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
                        data.waterSurfaceCornerRight, 
                        edge2, 
                        data.waterSurfaceCornerRight +
                        HexagonPoint.GetWaterBridge(
                            direction.NextClockwise(),
                            hexOuterRadius
                        ),
                        hexOuterRadius,
                        wrapSize
                    );

                    data.terrainSourceRelativeHexIndices.z =
                        nextNeighbor.Index;

                    water.AddTriangleHexData(
                        data.terrainSourceRelativeHexIndices,
                        _weights1,
                        _weights2,
                        _weights3
                    );
                }
            }
        }

        return data;
    }
}