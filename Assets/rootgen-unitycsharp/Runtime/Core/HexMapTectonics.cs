using RootLogging;
using System.Collections.Generic;
using UnityEngine;

public class HexMapTectonics {
    private HexMap _hexMap;
    private List<MapRegionRect> _regions;
    public HexMapTectonics(
        HexMap hexMap,
        int regionBorder,
        int mapBorderX,
        int mapBorderZ,
        int numRegions
    ) {
        _hexMap = hexMap;
        _regions = GenerateRegions(
            regionBorder,
            mapBorderX,
            mapBorderZ,
            numRegions
        );
    }

    public int Step(
        TectonicParameters parameters
    ) {
        // Determine whether this hex should be sunk            
        bool sink = Random.value < parameters.SinkProbability;

        int result = parameters.LandBudget;

        // For each region . . . 
        for (int i = 0; i < _regions.Count; i++) {
            
            MapRegionRect region = _regions[i];

            // Get a chunk size to use within the bounds of the region based
            // of the minimum and maximum chunk sizes.
            int regionDensity = Random.Range(
                parameters.RegionDensityMin,
                parameters.RegionDensityMax + 1
            );
            
            // If hex is to be sunk, sink hex and decrement decrement
            // land budget if sinking results in a hex below water
            // level.
            if (sink) {
                result = SinkTerrain(
                    result,
                    regionDensity,
                    region,
                    parameters.HighRiseProbability,
                    parameters.ElevationMin,
                    parameters.WaterLevelGlobal,
                    parameters.JitterProbability,
                    parameters.HexSize
                );
            }

            // Else, raise hex and increment land budget if raising
            // results in a hex above the water level.
            else {
                result = RaiseTerrain(
                    result,
                    regionDensity,
                    region,
                    parameters.HighRiseProbability,
                    parameters.ElevationMax,
                    parameters.WaterLevelGlobal,
                    parameters.JitterProbability,
                    parameters.HexSize
                );

                if (result == 0)
                    return result;
            }
        }

        return result;
    }

    private int SinkTerrain(
        int landBudget,
        int maximumRegionDensity,
        MapRegionRect region,
        float highRiseProbability,
        int elevationMin,
        int globalWaterLevel,
        float jitterProbability,
        float hexOuterRadius
    ) {
        int result = landBudget;
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
                originalElevation >= globalWaterLevel &&
                newElevation < globalWaterLevel
            ) {
                result++;
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

        return result;
    }

    private int RaiseTerrain(
        int landBudget,
        int maximumRegionDensity,
        MapRegionRect region,
        float highRiseProbability,
        int elevationMax,
        int globalWaterLevel,
        float jitterProbability,
        float hexOuterRadius
    ) {
        int result = landBudget;
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
                _hexMap.WrapSize
            );

            if (
                originalElevation < globalWaterLevel &&
                newElevation >= globalWaterLevel &&
                --result == 0
            ) {
                break;
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

                    open.Enqueue(neighbor, priority);
                }
            }
        }

        return result;
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
