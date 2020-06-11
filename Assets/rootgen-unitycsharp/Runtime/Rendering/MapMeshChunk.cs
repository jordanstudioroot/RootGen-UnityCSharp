using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RootLogging;

public class MapMeshChunk : MonoBehaviour {
    private TerrainChunkLayer _terrainLayer;
    private MapMeshChunkLayer _rivers;
    private MapMeshChunkLayer _roads;
    private MapMeshChunkLayer _water;
    private MapMeshChunkLayer _waterShore;
    private MapMeshChunkLayer _estuaries;
    private FeatureContainer _features;

/// <summary>
/// Splat map vector representing an entirely red channel.
/// </summary>
    private static Color _weights1 = new Color(1f, 0f, 0f);
/// <summary>
///  Splat map vector representing an entirely green channel.
/// </summary>
    private static Color _weights2 = new Color(0f, 1f, 0f);
/// <summary>
/// Splat map vector representing an entirely blue channel.
/// </summary>
    private static Color _weights3 = new Color(0f, 0f, 1f);
        
    private HexAdjacencyGraph _graph;

    public Hex[] Hexes {
        get; set;
    }

    public Canvas WorldSpaceUICanvas {
        get; private set;
    }

    public static MapMeshChunk CreateEmpty() {
        GameObject resultObj = new GameObject("Map Mesh Chunk");
        MapMeshChunk resultMono = resultObj.AddComponent<MapMeshChunk>();
        
        resultMono.Hexes = new Hex[
            HexMeshConstants.CHUNK_SIZE_X *
            HexMeshConstants.CHUNK_SIZE_Z
        ];
        
        GameObject resultCanvasObj = new GameObject(
            "World Space UI Canvas"
        );
        
        Canvas resultCanvasMono = resultCanvasObj.AddComponent<Canvas>();

        CanvasScaler resultCanvasScalerMono =
            resultCanvasObj.AddComponent<CanvasScaler>();

        resultCanvasObj.transform.SetParent(resultObj.transform, false);
        resultMono.WorldSpaceUICanvas = resultCanvasMono;
        resultCanvasScalerMono.dynamicPixelsPerUnit = 10f;
        resultCanvasObj.transform.rotation = Quaternion.Euler(90, 0, 0);
        resultCanvasObj.transform.position += Vector3.up * .005f;

        resultMono._terrainLayer = TerrainChunkLayer.CreateEmpty(
            Resources.Load<Material>("Terrain"), true, true, false, false
        );
        resultMono._terrainLayer.name = "Terrain Layer";
        resultMono._terrainLayer.transform.SetParent(resultObj.transform, false);
        
        resultMono._rivers = MapMeshChunkLayer.CreateEmpty(
            Resources.Load<Material>("River"), false, true, true, false
        );
        resultMono._rivers.name = "Rivers Layer";
        resultMono._rivers.transform.SetParent(resultObj.transform, false);

        resultMono._roads = MapMeshChunkLayer.CreateEmpty(
            Resources.Load<Material>("Road"), false, true, true, false
        );
        resultMono._roads.name = "Roads Layer";
        resultMono._roads.transform.SetParent(resultObj.transform, false);

        resultMono._water = MapMeshChunkLayer.CreateEmpty(
            Resources.Load<Material>("Water"), false, true, false, false
        );
        resultMono._water.name = "Water Layer";
        resultMono._water.transform.SetParent(resultObj.transform, false);

        resultMono._waterShore = MapMeshChunkLayer.CreateEmpty(
            Resources.Load<Material>("WaterShore"), false, true, true, false
        );
        resultMono._waterShore.name = "Water Shore Layer";
        resultMono._waterShore.transform.SetParent(resultObj.transform, false);

        resultMono._estuaries = MapMeshChunkLayer.CreateEmpty(
            Resources.Load<Material>("Estuary"), false, true, true, true
        );
        resultMono._estuaries.name = "Estuaries Layer";
        resultMono._estuaries.transform.SetParent(resultObj.transform, false);

        MapMeshChunkLayer walls = MapMeshChunkLayer.CreateEmpty(
            Resources.Load<Material>("Urban"), false, false, false, false
        );
        walls.transform.SetParent(resultObj.transform, false);
        walls.name = "Walls Layer";

        resultMono._features = FeatureContainer.GetFeatureContainer(walls);
        resultMono._features.transform.SetParent(resultObj.transform, false);
        resultMono._features.name = "Features Layer";

        return resultMono;
    }

    public static MapMeshChunk CreateEmpty(Transform parent) {
        MapMeshChunk result = CreateEmpty();
        result.transform.SetParent(parent, false);
        return result;
    }

    public void Refresh() {
        enabled = true;
    }

    public bool AddHex(int index, Hex hex) {
        try {
            Hexes[index] = hex;
            hex.chunk = this;

// Set WorldPositionStays to false for both the hexes transform and
// ui rect or they will not move initally to be oriented with the
// chunk.
            hex.transform.SetParent(transform, false);
            hex.uiRect.SetParent(WorldSpaceUICanvas.transform, false);
            return true;
        }
        catch (System.IndexOutOfRangeException) {
            RootLog.Log(
                "The specified hex " + hex + " could not be added to " +
                " the mesh chunk because the specified index " + index +
                " was outside the bounds of the chunks hex array."
            );
            return false;
        }
    }

/// <summary>
/// Switches the UI on and off for this chunk, enabling and
/// disabling features such as the distance from the currently
/// selected hex hex.
/// </summary>
/// <param name="visible">
/// The visible state of the hex grid chunk.
/// </param>
    public void ShowUI(bool visible) {
        WorldSpaceUICanvas.gameObject.SetActive(visible);
    }

    public void Triangulate(
        HexMap hexMap,
        float hexOuterRadius,
        HexAdjacencyGraph adjacencyGraph,
        RiverDigraph riverGraph,
        RoadUndirectedGraph roadGraph,
        ElevationDigraph elevationGraph
    ) {
        _terrainLayer.Clear();
        _rivers.Clear();
        _roads.Clear();
        _water.Clear();
        _waterShore.Clear();
        _estuaries.Clear();
        _features.Clear();

        for(int i = 0; i < Hexes.Length; i++) {
            Hex current = Hexes[i];

            if (current) {
                Dictionary<HexDirections, Hex> neighbors =
                    adjacencyGraph.GetNeighborByDirection(current);

                Dictionary<HexDirections, ElevationEdgeTypes> edgeTypes =
                    elevationGraph.GetNeighborEdgeTypes(current);

                Dictionary<HexDirections, bool> roadEdges =
                    roadGraph.GetNeighborRoads(current);
                    
                List<HexDirections> borderDirections =
                    adjacencyGraph.GetBorderDirectionsList(current);

                HexRiverData riverData = riverGraph.GetRiverData(current);

                TriangulateHex(
                    current,
                    neighbors,
                    borderDirections,
                    hexOuterRadius,
                    riverData,
                    roadEdges,
                    edgeTypes,
                    hexMap.WrapSize,
                    _terrainLayer,
                    _rivers,
                    _roads,
                    _water,
                    _waterShore,
                    _estuaries,
                    _features
                );
            }
        }

        _terrainLayer.Draw();
        _rivers.Draw();
        _roads.Draw();
        _water.Draw();
        _waterShore.Draw();
        _estuaries.Draw();
        _features.Apply();
    }
    
    /// <summary>
    /// Triangulate the mesh geometry of an individual hex.
    /// </summary>
    /// <param name="hex">
    /// The hex to whose mesh geometry is to be triangluated.
    /// </param>
    /// <param name="hexOuterRadius">
    /// The outer radius of the hex to be triangulated.
    /// </param>
    /// <param name="adjacencyGraph">
    /// 
    /// </param>
    /// <param name="riverDigraph"></param>
    /// <param name="roadUndirectedGraph"></param>
    /// <param name="elevationDigraph"></param>
    /// <param name="wrapSize"></param>
    private void TriangulateHex(
        Hex hex,
        Dictionary<HexDirections, Hex> neighbors,
        List<HexDirections> borderDirections,
        float hexOuterRadius,
        HexRiverData riverData,
        Dictionary<HexDirections, bool> roadEdges,
        Dictionary<HexDirections, ElevationEdgeTypes> elevationEdgeTypes,
        int wrapSize,
        TerrainChunkLayer terrainChunkLayer,
        MapMeshChunkLayer rivers,
        MapMeshChunkLayer roads,
        MapMeshChunkLayer water,
        MapMeshChunkLayer waterShore,
        MapMeshChunkLayer estuaries,
        FeatureContainer features
    ) { 
        foreach (
            KeyValuePair<HexDirections, Hex> pair in neighbors
        ) {
            HexDirections direction = pair.Key;
            Hex neighbor = pair.Value;
            
            TriangulationData triangulationData = new TriangulationData();
            triangulationData.terrainCenter = hex.Position;

            triangulationData =
                terrainChunkLayer.TriangulateHexTerrainEdge(
                    hex,
                    neighbor,
                    triangulationData,
                    neighbors,
                    direction,
                    riverData,
                    roads,
                    features,
                    roadEdges,
                    elevationEdgeTypes,
                    hexOuterRadius,
                    wrapSize
                );

            triangulationData = TriangulateHexRivers(
                hex,
                neighbor,
                direction,
                roadEdges,
                riverData,
                triangulationData,
                rivers,
                hexOuterRadius,
                wrapSize
            );

            triangulationData = TriangulateHexWater(
                hex,
                neighbor,
                neighbors,
                direction,
                riverData,
                triangulationData,
                water,
                waterShore,
                estuaries,
                hexOuterRadius,
                wrapSize
            );
        }

        for (int i = 0; i < borderDirections.Count; i++) {
            TriangulationData triangulationData = new TriangulationData();
            triangulationData.terrainCenter = hex.Position;

            EdgeVertices centerEdgeVertices = GetCenterEdgeVertices(
                borderDirections[i],
                triangulationData,
                hexOuterRadius
            );

            triangulationData = TriangulateWaterCenter(
                hex,
                triangulationData,
                borderDirections[i],
                hexOuterRadius,
                wrapSize,
                water
            );

            TriangulateBorderConnection(
                hex,
                borderDirections[i],
                centerEdgeVertices,
                hexOuterRadius
            );
        }

        bool anyEdge = false;

        foreach (KeyValuePair<HexDirections, bool> pair in roadEdges) {
            if (pair.Value) {
                anyEdge = true;
                break;
            }
        }

        if (!hex.IsUnderwater) {
            if (
                !riverData.HasRiver &&
                !anyEdge
            ) {
                features.AddFeature(
                    hex,
                    hex.Position,
                    hexOuterRadius,
                    wrapSize
                );
            }

            if (hex.IsSpecial) {
                features.AddSpecialFeature(
                    hex,
                    hex.Position,
                    hexOuterRadius,
                    wrapSize
                );
            }
        }
    }

    private TriangulationData TriangulateHexRivers(
        Hex hex,
        Hex neighbor,
        HexDirections direction,
        Dictionary<HexDirections, bool> roadEdges,
        HexRiverData riverData,
        TriangulationData triangulationData,
        MapMeshChunkLayer rivers,
        float hexOuterRadius,
        int wrapSize
    ) {
        triangulationData = TriangulateCenterRiverSurface(
            riverData,
            direction,
            hex,
            triangulationData,
            hexOuterRadius,
            wrapSize,
            rivers,
            roadEdges
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
                rivers
            );
        }
            
        return triangulationData;
    }

    private TriangulationData TriangulateHexWater(
        Hex hex,
        Hex neighbor,
        Dictionary<HexDirections, Hex> neighbors,
        HexDirections direction,
        HexRiverData riverData,
        TriangulationData triangulationData,
        MapMeshChunkLayer water,
        MapMeshChunkLayer waterShore,
        MapMeshChunkLayer estuaries,
        float hexOuterRadius,
        int wrapSize
    ) {
        if (hex.IsUnderwater) {
            triangulationData.waterSurfaceCenter = hex.Position;
            triangulationData.waterSurfaceCenter.y = hex.WaterSurfaceY;

            if (
                !neighbor.IsUnderwater
            ) {
                TriangulateWaterShore(
                    hex,
                    neighbor,
                    direction,
                    neighbors,
                    riverData,
                    triangulationData.waterSurfaceCenter,
                    hexOuterRadius,
                    wrapSize,
                    water,
                    waterShore,
                    estuaries
                );
            }
            else {
                triangulationData = TriangulateWaterCenter(
                    hex,
                    triangulationData,
                    direction,
                    hexOuterRadius,
                    wrapSize,
                    water
                );

                TriangulateWaterConnection(
                    hex,
                    neighbor,
                    direction,
                    neighbors,
                    triangulationData.waterSurfaceCornerLeft,
                    triangulationData.waterSurfaceCornerRight,
                    triangulationData.terrainSourceRelativeHexIndices,
                    hexOuterRadius,
                    wrapSize,
                    water
                );
            }      
        }

        return triangulationData;
    }

    private EdgeVertices GetCenterEdgeVertices(
        HexDirections direction,
        TriangulationData data,
        float hexOuterRadius
    ) {
// Triangle edge.
        EdgeVertices edgeVertices = new EdgeVertices(
            data.terrainCenter + HexagonPoint.GetFirstSolidCorner(
                direction,
                hexOuterRadius
            ),
            data.terrainCenter +
            HexagonPoint.GetSecondSolidCorner(
                direction,
                hexOuterRadius
            )
        );

        return edgeVertices;
    }

    private TriangulationData TriangulateRiverBeginOrEndRiver(
        Hex source,
        Vector3 center,
        TriangulationData triangulationData,
        HexRiverData riverData,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer rivers
    ) {
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

        center.y =
            triangulationData.middleEdgeVertices.vertex2.y =
            triangulationData.middleEdgeVertices.vertex4.y =
            source.RiverSurfaceY;

        rivers.AddTrianglePerturbed(
            center,
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
                            data.terrainCenter,
                            data,
                            riverData,
                            hexOuterRadius,
                            wrapSize,
                            rivers
                        );
                    }
                }
                else if (!source.IsUnderwater) {
                    data = TriangulateCenterRiverSurface(
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

    private TriangulationData TriangulateCenterRiverSurface(
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

    private EdgeVertices GetConnectionEdgeVertices(
        Hex source,
        Hex neighbor,
        HexDirections direction,
        EdgeVertices centerEdgeVertices,
        float hexOuterRadius
    ) {

        Vector3 bridge = HexagonPoint.GetBridge(
            direction,
            hexOuterRadius
        );

        bridge.y = neighbor.Position.y - source.Position.y;

        EdgeVertices result = new EdgeVertices(
            centerEdgeVertices.vertex1 + bridge,
            centerEdgeVertices.vertex5 + bridge
        );

        return result;
    }

    private EdgeVertices TriangulateBorderConnection(
        Hex source,
        HexDirections direction,
        EdgeVertices centerEdgeVertices,
        float hexOuterRadius
    ) {
        Vector3 bridge = HexagonPoint.GetBridge(
            direction,
            hexOuterRadius
        );

        bridge.y = source.Position.y;

        EdgeVertices result = new EdgeVertices(
            centerEdgeVertices.vertex1 + bridge,
            centerEdgeVertices.vertex5 + bridge
        );

        return result;
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

    private void TriangulateRoadSegment (
        Vector3 vertex1,
        Vector3 vertex2,
        Vector3 vertex3,
        Vector3 vertex4,
        Vector3 vertex5,
        Vector3 vertex6,
        Color weight1,
        Color weight2,
        Vector3 indices,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer roads
    ) {
        roads.AddQuadPerturbed(
            vertex1,
            vertex2,
            vertex4,
            vertex5,
            hexOuterRadius,
            wrapSize
        );

        roads.AddQuadPerturbed(
            vertex2,
            vertex3,
            vertex5,
            vertex6,
            hexOuterRadius,
            wrapSize
        );

        roads.AddQuadUV(0f, 1f, 0f, 0f);
        roads.AddQuadUV(1f, 0f, 0f, 0f);

        roads.AddQuadHexData(indices, weight1, weight2);
        roads.AddQuadHexData(indices, weight1, weight2);
    }

    private TriangulationData TriangulateWaterCenter(
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

    private void TriangulateWaterConnection(
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

    private void TriangulateWaterShore(
        Hex source,
        Hex target,
        HexDirections direction,
        Dictionary<HexDirections, Hex> neighbors,
        HexRiverData riverData,
        Vector3 center,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer water,
        MapMeshChunkLayer waterShore,
        MapMeshChunkLayer estuaries
    ) {
        EdgeVertices edge1 = new EdgeVertices(
            center + HexagonPoint.GetFirstWaterCorner(
                direction,
                hexOuterRadius
            ),
            center + HexagonPoint.GetSecondWaterCorner(
                direction,
                hexOuterRadius
            )
        );

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

        Vector3 indices = new Vector3(
            source.Index,
            target.Index,
            source.Index
        );

        water.AddTriangleHexData(indices, _weights1);
        water.AddTriangleHexData(indices, _weights1);
        water.AddTriangleHexData(indices, _weights1);
        water.AddTriangleHexData(indices, _weights1);

// Work backward from the solid shore to obtain the edge.
        Vector3 center2 = target.Position;

        float hexInnerRadius =
            HexagonPoint.OuterToInnerRadius(hexOuterRadius);
        
        float hexInnerDiameter = hexInnerRadius * 2f;
// TODO: This will not work once the column index is removed from
//       Hex class.
// If the neighbor outside the wrap boundaries, adjust accordingly.
        if (target.ColumnIndex < source.ColumnIndex - 1) {
            center2.x += 
                wrapSize * hexInnerDiameter;
        }
        else if (target.ColumnIndex > source.ColumnIndex + 1) {
            center2.x -=
                wrapSize * hexInnerDiameter;
        }

        center2.y = center.y;

        EdgeVertices edge2 = new EdgeVertices(
            center2 + HexagonPoint.GetSecondSolidCorner(
                direction.Opposite(),
                hexOuterRadius
            ),
            center2 + HexagonPoint.GetFirstSolidCorner(
                direction.Opposite(),
                hexOuterRadius
            )
        );

//          hex.HasRiverThroughEdge(direction)
        if (riverData.HasRiverInDirection(direction)) {
            TriangulateEstuary(
                edge1,
                edge2,
//                (hex.HasIncomingRiver &&
//                hex.IncomingRiver == direction),
                riverData.HasIncomingRiverInDirection(direction),
                indices,
                hexOuterRadius,
                wrapSize,
                waterShore,
                estuaries
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

            waterShore.AddQuadHexData(indices, _weights1, _weights2);
            waterShore.AddQuadHexData(indices, _weights1, _weights2);
            waterShore.AddQuadHexData(indices, _weights1, _weights2);
            waterShore.AddQuadHexData(indices, _weights1, _weights2);
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
        Vector3 indices,
        float hexOuterRadius,
        int wrapSize,
        MapMeshChunkLayer waterShore,
        MapMeshChunkLayer estuaries
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
