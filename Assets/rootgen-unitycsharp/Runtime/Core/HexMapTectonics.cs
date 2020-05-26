using RootLogging;
using System.Collections.Generic;
using UnityEngine;

public class HexMapTectonics {
    private HexMap _hexMap;
    private List<MapRegionRect> regions;
    public HexMapTectonics(
        HexMap hexMap,
        int regions
    ) {
        _hexMap = hexMap;
    }

    private int Step(
        int landPercentage,
        float sinkProbability,
        int chunkSizeMin,
        int chunkSizeMax,
        float highRiseProbability,
        int elevationMin,
        int elevationMax,
        int waterLevel,
        float jitterProbability,
        float hexOuterRadius,
        int wrapSize,
        List<MapRegionRect> regions
    ) {
        // Set the land budget to the fraction of the overall hexes as
        // specified by the percentLand arguement.
        int landBudget = Mathf.RoundToInt(
            _hexMap.SizeSquared * landPercentage * 0.01f
        );

        // Initialize the result;
        int result = landBudget;

        // Guard against permutations that result in an impossible map and
        // by extension an infinite loop by including a guard clause that
        // aborts the loop at 10,000 attempts to sink or raise the terrain.
        for (int guard = 0; guard < 10000; guard++) {
            
            // Determine whether this hex should be sunk            
            bool sink = Random.value < sinkProbability;

            // For each region . . . 
            for (int i = 0; i < regions.Count; i++) {
                
                MapRegionRect region = regions[i];

                // Get a chunk size to use within the bounds of the region based
                // of the minimum and maximum chunk sizes.
                int maximumRegionDensity = Random.Range(
                    chunkSizeMin,
                    chunkSizeMax + 1
                );
                
                // If hex is to be sunk, sink hex and decrement decrement
                // land budget if sinking results in a hex below water
                // level.
                if (sink) {
                    landBudget = SinkTerrain(
                        maximumRegionDensity,
                        landBudget,
                        region,
                        highRiseProbability,
                        elevationMin,
                        waterLevel,
                        jitterProbability,
                        hexOuterRadius
                    );
                }

                // Else, raise hex and increment land budget if raising
                // results in a hex above the water level.
                else {
                    landBudget = RaiseTerrain(
                        maximumRegionDensity,
                        landBudget,
                        region,
                        highRiseProbability,
                        elevationMax,
                        waterLevel,
                        jitterProbability,
                        hexOuterRadius,
                        wrapSize
                    );

                    // If land budget is 0, return initial land budget
                    // value because all land hexes specified to be
                    // allocated were allocated successfully. 
                    if (landBudget == 0) {
                        return result;
                    }
                }
            }
        }

        // If land budget is greater than 0, all land hexes specified to
        // be allocated were not allocated successfully. Log a warning,
        // decrement the remaining land budget from the result, and return
        // the result as the number of land hexes allocated.
        if (landBudget > 0) {
            RootLog.Log(
                "Failed to use up " + landBudget + " land budget.",
                Severity.Warning,
                "MapGenerator"
            );
            result -= landBudget;
        }

        return result;
    }

    int SinkTerrain(
            int maximumRegionDensity,
            int landBudget,
            MapRegionRect region,
            float highRiseProbability,
            int elevationMin,
            int waterLevel,
            float jitterProbability,
            float hexOuterRadius
    ) {
        PriorityQueue<Hex> open = new PriorityQueue<Hex>();
        List<Hex> closed = new List<Hex>();
        // Get a random hex within the region bounds to be the first hex
        // searched.
        Hex firstHex = GetRandomHex(region);
        open.Enqueue(firstHex, 0);
        CubeVector center = firstHex.CubeCoordinates;
        int sink = Random.value < highRiseProbability ? 2 : 1;
        int regionDensity = 0;

        while (
            regionDensity < maximumRegionDensity &&
            open.Count > 0
        ) {
            Hex current = open.Dequeue();
            closed.Add(current);
            
            int originalElevation = current.elevation;

            int newElevation = current.elevation - sink;

            if (newElevation < elevationMin) {
                continue;
            }

            current.SetElevation(
                newElevation,
                hexOuterRadius,
                _hexMap.WrapSize
            );

            if (
                originalElevation >= waterLevel &&
                newElevation < waterLevel
            ) {
                landBudget += 1;
            }

            regionDensity += 1;

            List<Hex> neighbors;

            if (_hexMap.TryGetNeighbors(current, out neighbors)) {
                foreach(Hex neighbor in neighbors) {
                    if (closed.Contains(neighbor))
                    continue;

                int priority =
                    CubeVector.WrappedHexTileDistance(
                        neighbor.CubeCoordinates,
                        center,
                        _hexMap.WrapSize
                    ) +
                    Random.value < jitterProbability ? 1 : 0;

                    open.Enqueue(
                        neighbor,
                        priority
                    );
                }
            }
        }

        return landBudget;
    }

    private int RaiseTerrain(
        HexMap hexMap,
        int maximumRegionDensity, 
        int budget, 
        MapRegionRect region,
        float highRiseProbability,
        int elevationMax,
        int waterLevel,
        float jitterProbability,
        float hexOuterRadius,
        int wrapSize
    ) {
        Hex firstHex = GetRandomHex(region);

        PriorityQueue<Hex> open = new PriorityQueue<Hex>();
        List<Hex> closed = new List<Hex>();

        open.Enqueue(firstHex, 0);

        CubeVector center = firstHex.CubeCoordinates;

        int rise = Random.value < highRiseProbability ? 2 : 1;
        int regionDensity = 0;

        while (
            regionDensity < maximumRegionDensity &&
            open.Count > 0
        ) {
            
            Hex current = open.Dequeue();
            closed.Add(current);
            
            int originalElevation = current.elevation;
            int newElevation = originalElevation + rise;

            if (newElevation > elevationMax) {
                continue;
            }

            current.SetElevation(
                newElevation,
                hexOuterRadius,
                hexMap.WrapSize
            );

            current.SetElevation(
                newElevation,
                hexOuterRadius,
                hexMap.WrapSize
            );

            if (
                originalElevation < waterLevel &&
                newElevation >= waterLevel &&
                --budget == 0
            ) {
                break;
            }

            regionDensity += 1;

            List<Hex> neighbors;

            if (hexMap.TryGetNeighbors(current, out neighbors)) {
                foreach(Hex neighbor in neighbors) {
                    if (closed.Contains(neighbor))
                    continue;

                    int priority =
                        CubeVector.WrappedHexTileDistance(
                            neighbor.CubeCoordinates,
                            center,
                            hexMap.WrapSize
                        ) +
                        Random.value < jitterProbability ? 1 : 0;

                    open.Enqueue(neighbor, priority);
                }
            }
        }

        return budget;
    }

    private Hex GetRandomHex(MapRegionRect region) {
        return _hexMap.GetHex(
            Random.Range(region.OffsetXMin, region.OffsetXMax),
            Random.Range(region.OffsetZMin, region.OffsetZMax)
        );
    }

    private List<MapRegionRect> GenerateRegions(
        int regionBorder,
        int mapBorderX,
        int mapBorderZ,
        int numRegions
    ) {
        return new List<MapRegionRect>(
            SubdivideRegions(
                _hexMap.HexOffsetColumns,
                _hexMap.HexOffsetRows,
                mapBorderX,
                mapBorderZ,
                numRegions,
                regionBorder,
                _hexMap.IsWrapping
            )
        );
    }

    private List<MapRegionRect> SubdivideRegions(
        int hexCountX,
        int hexCountZ,
        int mapBorderX,
        int mapBorderZ,
        int numRegions,
        int regionBorder,
        bool wrapping
    ) {
        List<MapRegionRect> result = new List<MapRegionRect>();

        int borderX = _hexMap.IsWrapping ? regionBorder : mapBorderX;

        int rootXMin = hexCountX > (borderX * 2) ?
            borderX : 0;
        int rootZMin = hexCountZ > (borderX * 2) ?
            borderX : 0;
        int rootXMax = hexCountX > (borderX * 2) ?
            hexCountX - borderX  - 1 : hexCountX - 1;
        int rootZMax = hexCountZ > (borderX * 2) ?
            hexCountZ - mapBorderZ - 1 : hexCountZ - 1;


        MapRegionRect root = new MapRegionRect(
            rootXMin,
            rootXMax,
            rootZMin,
            rootZMax
        );

        result.Add(root);

        List<MapRegionRect> temp = new List<MapRegionRect>();

        int i = 0;

        while (result.Count < numRegions) {
            foreach (MapRegionRect mapRect in result) {
                if (i % 2 == 0) {
                    temp.AddRange(
                        mapRect.SubdivideVertical(regionBorder)
                    );
                }
                else {
                    temp.AddRange(
                        mapRect.SubdivideHorizontal(regionBorder)
                    );
                }
            }

            result.Clear();
            result.AddRange(temp);
            temp.Clear();
            i++;
        }

        return result;
    }
}
