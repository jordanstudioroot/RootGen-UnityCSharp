using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using RootLogging;

public class MapMeshChunk : MonoBehaviour {
    private TerrainChunkLayer _terrainLayer;
    private RiversChunkLayer _riversLayer;
    private RoadsChunkLayer _roadsLayer;
    private OpenWaterChunkLayer _openWaterLayer;
    private WaterShoreChunkLayer _waterShoreLayer;
    private EstuariesChunkLayer _estuariesLayer;
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

    protected TriangulationData GetConnectionEdgeVertices(
        Hex source,
        Hex neighbor,
        HexDirections direction,
        TriangulationData data,
        float hexOuterRadius
    ) {

        Vector3 bridge = HexagonPoint.GetBridge(
            direction,
            hexOuterRadius
        );

        bridge.y = neighbor.Position.y - source.Position.y;

        data.connectionEdgeVertices = new EdgeVertices(
            data.centerEdgeVertices.vertex1 + bridge,
            data.centerEdgeVertices.vertex5 + bridge
        );

        return data;
    }

    protected TriangulationData GetCenterEdgeVertices(
        HexDirections direction,
        TriangulationData data,
        float hexOuterRadius
    ) {
    // Triangle edge.
        data.centerEdgeVertices = new EdgeVertices(
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

        return data;
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
        
        Canvas resultCanvasMono =
            resultCanvasObj.AddComponent<Canvas>();

        CanvasScaler resultCanvasScalerMono =
            resultCanvasObj.AddComponent<CanvasScaler>();

        resultCanvasObj.transform.SetParent(
            resultObj.transform,
            false
        );

        resultMono.WorldSpaceUICanvas = resultCanvasMono;
        resultCanvasScalerMono.dynamicPixelsPerUnit = 10f;
        
        resultCanvasObj.transform.rotation = Quaternion.Euler(
            90,
            0,
            0
        );
        
        resultCanvasObj.transform.position += Vector3.up * .005f;

        resultMono._terrainLayer = TerrainChunkLayer.CreateEmpty(
            Resources.Load<Material>("Terrain"),
            true,
            true,
            false,
            false
        );

        resultMono._terrainLayer.transform.SetParent(
            resultObj.transform,
            false
        );
        
        resultMono._riversLayer = RiversChunkLayer.CreateEmpty(
            Resources.Load<Material>("River"),
            false,
            true,
            true,
            false
        );

        resultMono._riversLayer.transform.SetParent(
            resultObj.transform,
            false
        );

        resultMono._roadsLayer = RoadsChunkLayer.CreateEmpty(
            Resources.Load<Material>("Road"),
            false,
            true,
            true,
            false
        );

        resultMono._roadsLayer.transform.SetParent(
            resultObj.transform,
            false
        );

        resultMono._openWaterLayer =
            OpenWaterChunkLayer.CreateEmpty(
                Resources.Load<Material>("Water"),
                false,
                true,
                false,
                false
        );

        resultMono._openWaterLayer.transform.SetParent(
            resultObj.transform,
            false
        );

        resultMono._waterShoreLayer =
            WaterShoreChunkLayer.CreateEmpty(
                Resources.Load<Material>("WaterShore"),
                false,
                true,
                true,
                false
            );

        resultMono._waterShoreLayer.transform.SetParent(
            resultObj.transform,
            false
        );

        resultMono._estuariesLayer = EstuariesChunkLayer.CreateEmpty(
            Resources.Load<Material>("Estuary"),
            false,
            true,
            true,
            true
        );

        resultMono._estuariesLayer.transform.SetParent(
            resultObj.transform,
            false
        );

        MapMeshChunkLayer walls = MapMeshChunkLayer.CreateEmpty(
            Resources.Load<Material>("Urban"),
            false,
            false,
            false,
            false
        );

        walls.name = "Walls Layer";
        walls.transform.SetParent(resultObj.transform, false);

        resultMono._features = FeatureContainer.GetFeatureContainer(
            walls
        );
        
        resultMono._features.transform.SetParent(
            resultObj.transform,
            false
        );
        
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
        _openWaterLayer.Clear();
        _waterShoreLayer.Clear();
        _estuariesLayer.Clear();
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
                    _openWaterLayer,
                    _waterShoreLayer,
                    _estuariesLayer,
                    _features
                );
            }
        }

        _terrainLayer.Draw();
        _riversLayer.Draw();
        _roadsLayer.Draw();
        _openWaterLayer.Draw();
        _waterShoreLayer.Draw();
        _estuariesLayer.Draw();
        _features.Apply();
    }
    
    /// <summary>
    /// Triangulate the mesh geometry of an individual hex.
    /// </summary>
    /// <param name="source">
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
        Hex source,
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
        OpenWaterChunkLayer openWaterLayer,
        WaterShoreChunkLayer waterShoreLayer,
        EstuariesChunkLayer estuariesLayer,
        FeatureContainer features
    ) { 
        foreach (
            KeyValuePair<HexDirections, Hex> pair in neighbors
        ) {
            // Initialize triangulation data.
            HexDirections direction = pair.Key;
            Hex neighbor = pair.Value;
            
            TriangulationData triangulationData =
                new TriangulationData();

            triangulationData.terrainCenter =
                source.Position;

            triangulationData = GetCenterEdgeVertices(
                direction,
                triangulationData,
                hexOuterRadius
            );

            if (direction <= HexDirections.Southeast) {
                triangulationData = GetConnectionEdgeVertices(
                    source,
                    neighbor,
                    direction,
                    triangulationData,
                    hexOuterRadius
                );
            }

            triangulationData = GetWaterData(
                source,
                neighbor,
                triangulationData,
                direction,
                hexOuterRadius,
                wrapSize
            );

            // Triangulate layers for non-border edge.
            triangulationData =
                terrainLayer.TriangulateHexTerrainEdge(
                    source,
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
                    source,
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
                    source,
                    neighbor,
                    direction,
                    roadEdges,
                    riverData,
                    triangulationData,
                    hexOuterRadius,
                    wrapSize
                );

            triangulationData =
                openWaterLayer.TriangulateHexOpenWaterEdge(
                    source,
                    neighbor,
                    neighbors,
                    direction,
                    triangulationData,
                    hexOuterRadius,
                    wrapSize
                );

            triangulationData =
                waterShoreLayer.TriangulateHexWaterShoreEdge(
                    source,
                    neighbor,
                    neighbors,
                    direction,
                    riverData,
                    triangulationData,
                    hexOuterRadius,
                    wrapSize
                );
            
            triangulationData =
                estuariesLayer.TriangulateHexEstuaryEdge(
                    source,
                    neighbor,
                    direction,
                    riverData,
                    triangulationData,
                    hexOuterRadius,
                    wrapSize
                );
        }

        bool anyEdge = false;

        foreach (KeyValuePair<HexDirections, bool> pair in roadEdges) {
            if (pair.Value) {
                anyEdge = true;
                break;
            }
        }

        // Add feature or special to hex.
        if (!source.IsUnderwater) {
            if (
                !riverData.HasRiver &&
                !anyEdge
            ) {
                features.AddFeature(
                    source,
                    source.Position,
                    hexOuterRadius,
                    wrapSize
                );
            }

            if (source.IsSpecial) {
                features.AddSpecialFeature(
                    source,
                    source.Position,
                    hexOuterRadius,
                    wrapSize
                );
            }
        }
    }

    private TriangulationData GetWaterData(
        Hex source,
        Hex neighbor,
        TriangulationData triangulationData,
        HexDirections direction,
        float hexOuterRadius,
        int wrapSize
    ) {
        triangulationData.waterSurfaceCenter = source.Position;
        triangulationData.waterSurfaceCenter.y = source.WaterSurfaceY;

        triangulationData.sourceWaterEdge = new EdgeVertices(
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

        Vector3 neighborCenter = neighbor.Position;

        float hexInnerRadius =
            HexagonPoint.OuterToInnerRadius(hexOuterRadius);
        
        float hexInnerDiameter = hexInnerRadius * 2f;
// TODO: This will not work once the column index is removed from
//       Hex class.
// If the neighbor outside the wrap boundaries, adjust accordingly.
        if (neighbor.ColumnIndex < source.ColumnIndex - 1) {
            neighborCenter.x += 
                wrapSize * hexInnerDiameter;
        }
        else if (neighbor.ColumnIndex > source.ColumnIndex + 1) {
            neighborCenter.x -=
                wrapSize * hexInnerDiameter;
        }

        neighborCenter.y = triangulationData.waterSurfaceCenter.y;

        triangulationData.neighborWaterEdge = new EdgeVertices(
            neighborCenter + HexagonPoint.GetSecondSolidCorner(
                direction.Opposite(),
                hexOuterRadius
            ),
            neighborCenter + HexagonPoint.GetFirstSolidCorner(
                direction.Opposite(),
                hexOuterRadius
            )
        );

        return triangulationData;
    }
}
