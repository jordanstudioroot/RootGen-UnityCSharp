using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RootLogging;

public class MapMeshChunk : MonoBehaviour {
    private TerrainChunkLayer _terrainLayer;
    private RiversChunkLayer _riversLayer;
    private RoadsChunkLayer _roadsLayer;
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
        resultMono._terrainLayer.transform.SetParent(resultObj.transform, false);
        
        resultMono._riversLayer = RiversChunkLayer.CreateEmpty(
            Resources.Load<Material>("River"), false, true, true, false
        );
        resultMono._riversLayer.transform.SetParent(resultObj.transform, false);

        resultMono._roadsLayer = RoadsChunkLayer.CreateEmpty(
            Resources.Load<Material>("Road"), false, true, true, false
        );
        resultMono._roadsLayer.transform.SetParent(resultObj.transform, false);

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
        _riversLayer.Clear();
        _roadsLayer.Clear();
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
                    _riversLayer,
                    _roadsLayer,
                    _water,
                    _waterShore,
                    _estuaries,
                    _features
                );
            }
        }

        _terrainLayer.Draw();
        _riversLayer.Draw();
        _roadsLayer.Draw();
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
        TerrainChunkLayer terrainLayer,
        RiversChunkLayer riversLayer,
        RoadsChunkLayer roadsLayer,
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
                terrainLayer.TriangulateHexTerrainEdge(
                    hex,
                    neighbor,
                    triangulationData,
                    neighbors,
                    direction,
                    riverData,
                    features,
                    roadEdges,
                    elevationEdgeTypes,
                    hexOuterRadius,
                    wrapSize
                );

            triangulationData =
                roadsLayer.TriangulateHexRoadEdge(
                    hex,
                    neighbor,
                    triangulationData,
                    direction,
                    riverData,
                    features,
                    roadEdges,
                    elevationEdgeTypes,
                    hexOuterRadius,
                    wrapSize
                );

            triangulationData =
                riversLayer.TriangulateHexRiverEdge(
                    hex,
                    neighbor,
                    direction,
                    roadEdges,
                    riverData,
                    triangulationData,
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

    private TriangulationData TriangulateHexWater(
        Hex source,
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
        if (source.IsUnderwater) {
            triangulationData.waterSurfaceCenter = source.Position;
            triangulationData.waterSurfaceCenter.y = source.WaterSurfaceY;

            if (
                !neighbor.IsUnderwater
            ) {
                EdgeVertices edge1 = new EdgeVertices(
                    triangulationData.waterSurfaceCenter +
                    HexagonPoint.GetFirstWaterCorner(
                        direction,
                        hexOuterRadius
                    ),
                    triangulationData.waterSurfaceCenter +
                    HexagonPoint.GetSecondWaterCorner(
                        direction,
                        hexOuterRadius
                    )
                );

                triangulationData = TriangulateWaterShoreWater(
                    source,
                    neighbor,
                    triangulationData.waterSurfaceCenter,
                    edge1,
                    hexOuterRadius,
                    wrapSize,
                    water,
                    triangulationData
                );

                Vector3 center2 = neighbor.Position;

                float hexInnerRadius =
                    HexagonPoint.OuterToInnerRadius(hexOuterRadius);
                
                float hexInnerDiameter = hexInnerRadius * 2f;
        // TODO: This will not work once the column index is removed from
        //       Hex class.
        // If the neighbor outside the wrap boundaries, adjust accordingly.
                if (neighbor.ColumnIndex < source.ColumnIndex - 1) {
                    center2.x += 
                        wrapSize * hexInnerDiameter;
                }
                else if (neighbor.ColumnIndex > source.ColumnIndex + 1) {
                    center2.x -=
                        wrapSize * hexInnerDiameter;
                }

                center2.y = triangulationData.waterSurfaceCenter.y;

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
                    waterShore,
                    edge1,
                    edge2,
                    hexInnerDiameter
                );

                if (riverData.HasRiverInDirection(direction)) {
                    TriangulateEstuary(
                        edge1,
                        edge2,
                        riverData.HasIncomingRiverInDirection(direction),
                        triangulationData.waterSourceRelativeHexIndices,
                        hexOuterRadius,
                        wrapSize,
                        estuaries
                    );
                }
            }
            else {
                triangulationData = TriangulateWaterCenter(
                    source,
                    triangulationData,
                    direction,
                    hexOuterRadius,
                    wrapSize,
                    water
                );

                TriangulateWaterConnection(
                    source,
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

    private TriangulationData TriangulateWaterShoreWater(
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
