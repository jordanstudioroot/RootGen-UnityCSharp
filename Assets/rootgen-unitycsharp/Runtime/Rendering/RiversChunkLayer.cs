using System.Collections.Generic;
using UnityEngine;

public class RiversChunkLayer : MapMeshChunkLayer {
    public new static RiversChunkLayer CreateEmpty(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Rivers Chunk Layer");
        
        RiversChunkLayer resultMono =
            resultObj.AddComponent<RiversChunkLayer>();
        
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

    public TriangulationData TriangulateHexRiverEdge(
        Hex hex,
        Hex neighbor,
        HexDirections direction,
        Dictionary<HexDirections, bool> roadEdges,
        HexRiverData riverData,
        TriangulationData triangulationData,
        float hexOuterRadius,
        int wrapSize
    ) {
        if (riverData.HasRiver) {
            triangulationData = TriangulateCenterRiverSurface(
            riverData,
            direction,
            hex,
            triangulationData,
            hexOuterRadius,
            wrapSize,
            this,
            roadEdges
        );

        if (direction <= HexDirections.Southeast) {
            /*triangulationData =
                GetConnectionEdgeVertices(
                    hex,
                    neighbor,
                    direction,
                    triangulationData,
                    hexOuterRadius
                );*/
            
        // Adjust the other edge of the connection  if there is a river through
        // that edge.
                triangulationData = TriangulateRiverConnection(
                    hex,
                    neighbor,
                    triangulationData,
                    direction,
                    riverData,
                    hexOuterRadius,
                    wrapSize,
                    this
                );
            }
        }
        
            
        return triangulationData;
    }

    private TriangulationData TriangulateCenterRiverSurface(
        HexRiverData riverData,
        HexDirections direction,
        Hex source,
        TriangulationData data,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer rivers,
        Dictionary<HexDirections, bool> roadEdges
    ) {
        if (riverData.HasRiver) {
            if (riverData.HasRiverInDirection(direction)) {
// If the triangle has a river through the edge, lower center edge vertex
// to simulate stream bed.
                if (riverData.HasRiverStartOrEnd) {
                    if (!source.IsUnderwater) {
                        data = TriangulateRiverBeginOrEndRiver(
                            source,
                            data,
                            riverData,
                            hexOuterRadius,
                            wrapSize,
                            rivers
                        );
                    }
                }
                else if (!source.IsUnderwater) {
                    data = TriangulateCenterRiverQuads(
                        source,
                        data,
                        direction,
                        riverData,
                        hexOuterRadius,
                        wrapSize,
                        rivers
                    );
                }
            }
        }

        return data;
    }

    private TriangulationData TriangulateRiverBeginOrEndRiver(
        Hex source,
        TriangulationData triangulationData,
        HexRiverData riverData,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer rivers
    ) {
        Vector3 riverSurfaceCenter = triangulationData.terrainCenter;
//            bool reversed = hex.HasIncomingRiver;
        bool reversed = riverData.HasIncomingRiver;

        Vector3 indices = new Vector3(
            source.Index,
            source.Index,
            source.Index
        );
        
        TriangulateRiverQuad(
            triangulationData.middleEdgeVertices.vertex2,
            triangulationData.middleEdgeVertices.vertex4,
            triangulationData.centerEdgeVertices.vertex2,
            triangulationData.centerEdgeVertices.vertex4,
            source.RiverSurfaceY,
            0.6f,
            reversed,
            indices,
            hexOuterRadius,
            wrapSize,
            rivers
        );

        riverSurfaceCenter.y =
            triangulationData.middleEdgeVertices.vertex2.y =
            triangulationData.middleEdgeVertices.vertex4.y =
            source.RiverSurfaceY;

        rivers.AddTrianglePerturbed(
            riverSurfaceCenter,
            triangulationData.middleEdgeVertices.vertex2,
            triangulationData.middleEdgeVertices.vertex4,
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

        return triangulationData;
    }

    private TriangulationData TriangulateCenterRiverQuads(
        Hex source,
        TriangulationData triangulationData,
        HexDirections direction,
        HexRiverData riverData,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer rivers
    ) {
        bool reversed = riverData.HasIncomingRiverInDirection(
                direction
            );

        TriangulateRiverQuad(
            triangulationData.riverCenterLeft,
            triangulationData.riverCenterRight,
            triangulationData.middleEdgeVertices.vertex2,
            triangulationData.middleEdgeVertices.vertex4,
            source.RiverSurfaceY,
            0.4f,
            reversed,
            triangulationData.terrainSourceRelativeHexIndices,
            hexOuterRadius,
            wrapSize,
            rivers
        );

        TriangulateRiverQuad(
            triangulationData.middleEdgeVertices.vertex2,
            triangulationData.middleEdgeVertices.vertex4,
            triangulationData.centerEdgeVertices.vertex2,
            triangulationData.centerEdgeVertices.vertex4,
            source.RiverSurfaceY,
            0.6f,
            reversed,
            triangulationData.terrainSourceRelativeHexIndices,
            hexOuterRadius,
            wrapSize,
            rivers
        );

        return triangulationData;
    }

    private TriangulationData TriangulateRiverConnection(
        Hex source,
        Hex neighbor,
        TriangulationData data,
        HexDirections direction,
        HexRiverData riverData,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer rivers
    ) {
        if (riverData.HasRiverInDirection(direction)) {
            Vector3 indices;
            indices.x = indices.z = source.Index;
            indices.y = neighbor.Index;

            if (!source.IsUnderwater) {
                if (!neighbor.IsUnderwater) {
                    TriangulateRiverQuad(
                        data.centerEdgeVertices.vertex2,
                        data.centerEdgeVertices.vertex4,
                        data.connectionEdgeVertices.vertex2,
                        data.connectionEdgeVertices.vertex4,
                        source.RiverSurfaceY,
                        neighbor.RiverSurfaceY,
                        0.8f,
                        (
//                            hex.HasIncomingRiver &&
//                            hex.IncomingRiver == direction
                              riverData.HasIncomingRiverInDirection(
                                  direction
                              )
                        ),
                        indices,
                        hexOuterRadius,
                        wrapSize,
                        rivers
                    );
                }
                else if(source.elevation > neighbor.WaterLevel) {
                    TriangulateWaterfallInWater(
                        data.centerEdgeVertices.vertex2,
                        data.centerEdgeVertices.vertex4, 
                        data.connectionEdgeVertices.vertex2,
                        data.connectionEdgeVertices.vertex4, 
                        source.RiverSurfaceY, 
                        neighbor.RiverSurfaceY,
                        neighbor.WaterSurfaceY,
                        indices,
                        hexOuterRadius,
                        wrapSize,
                        rivers
                    );
                }
            }
            else if (
                !neighbor.IsUnderwater &&
                neighbor.elevation > source.WaterLevel
            ) {
                TriangulateWaterfallInWater(
                    data.connectionEdgeVertices.vertex4,
                    data.connectionEdgeVertices.vertex2,
                    data.centerEdgeVertices.vertex4,
                    data.centerEdgeVertices.vertex2,
                    neighbor.RiverSurfaceY,
                    source.RiverSurfaceY,
                    source.WaterSurfaceY,
                    indices,
                    hexOuterRadius,
                    wrapSize,
                    rivers
                );
            }
        }

        return data;
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
        int wrapSize,
        MapMeshChunkLayer rivers
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
            wrapSize,
            rivers
        );
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
        int wrapSize,
        MapMeshChunkLayer rivers
    ) {
        vertex1.y = vertex2.y = y1;
        vertex3.y = vertex4.y = y2;

        rivers.AddQuadPerturbed(
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
        int wrapSize,
        MapMeshChunkLayer rivers
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
}