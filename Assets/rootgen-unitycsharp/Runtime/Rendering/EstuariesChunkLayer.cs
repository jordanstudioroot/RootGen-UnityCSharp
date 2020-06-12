using UnityEngine;

public class EstuariesChunkLayer : MapMeshChunkLayer {
    public new static EstuariesChunkLayer CreateEmpty(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Estuaries Chunk Layer");
        EstuariesChunkLayer resultMono =
            resultObj.AddComponent<EstuariesChunkLayer>();
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

    public TriangulationData TriangulateHexEstuaryEdge(
        Hex source,
        Hex neighbor,
        HexDirections direction,
        HexRiverData riverData,
        TriangulationData triangulationData,
        float hexOuterRadius,
        int wrapSize
    ) {
        if (source.IsUnderwater) {
            if (!neighbor.IsUnderwater) {
                if (riverData.HasRiverInDirection(direction)) {
                    TriangulateEstuary(
                        triangulationData.sourceWaterEdge,
                        triangulationData.neighborWaterEdge,
                        riverData.HasIncomingRiverInDirection(direction),
                        triangulationData.waterSourceRelativeHexIndices,
                        hexOuterRadius,
                        wrapSize,
                        this
                    );
                }
            }
        }

        return triangulationData;
    }

    private void TriangulateEstuary(
        EdgeVertices edge1,
        EdgeVertices edge2,
        bool incomingRiver,
        Vector3 indices,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer estuaries
    ) {
        estuaries.AddQuadPerturbed(
            edge2.vertex1, 
            edge1.vertex2, 
            edge2.vertex2, 
            edge1.vertex3,
            hexOuterRadius,
            wrapSize
        );

        estuaries.AddTrianglePerturbed(
            edge1.vertex3, 
            edge2.vertex2, 
            edge2.vertex4,
            hexOuterRadius,
            wrapSize
        );

        estuaries.AddQuadPerturbed(
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
}