using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using RootLogging;
using RootCollections;
using RootUtils.Randomization;


/// <summary>
/// Class encapsulating the RootGen map generation algorithms.
/// </summary>
public class MapGenerator {
/// <summary>
/// The number of land cells in the map.
/// </summary>
    private int _landCells;

/// <summary>
/// A queue containing all the cells on the A* search frontier.
/// </summary>
    private CellPriorityQueue _searchFrontier;

/// <summary>
/// The current A* search phase.
/// </summary>
/// TODO:
///     This variable is reassigned unnecessarily.
    private int _searchFrontierPhase;

/// <summary>
/// A collection of all the map regions.
/// </summary>
    private List<MapRegionRect> _regions;

/// <summary>
/// A collection of structs defining the current climate data.
/// </summary>
/// TODO: 
///     Indirect coupling between _climate and HexGrid in GenerateClimate() and
///     StepClimate() requiring both the list of HexCells in HexGrid and _climate
///     to maintain the same order.
/// 
///     Need to either make this dependency explicit or elimnate the coupling.
/// 
///     This list is cleared and reused.
/// 
///     Might be possible to somehow elimate this list entirely by using functional
///     programming principles instead of reusing the same list.
    private List<ClimateData> _climate = new List<ClimateData>();

/// <summary>
/// A collection of structs defining the next climate data.
/// </summary>
/// TODO: 
///     Indirect coupling between _nextClimate and HexGrid in GenerateClimate() and
///     StepClimate() requiring both the list of HexCells in HexGrid and _nextClimate
///     to maintain the same order.
/// 
///     Need to either make this dependency explicit or elimnate the coupling.
///     
///     This list is cleared and reused.
/// 
///     Might be possible to somehow elimate this list entirely by using functional
///     programming principles instead of reusing the same list.
    private List<ClimateData> _nextClimate = new List<ClimateData>();

/// <summary>
/// A collection of HexDirections representing possible flow directions for
///     a given river at a particular growth step.
/// </summary>
/// TODO:
///     This list is cleared and reused.
///     
///     Might be possible to somehow eliminate this list entirely by using functional
///     programming principles instead of reusing the same list.
    private List<HexDirection> _flowDirections = new List<HexDirection>();

/// <summary>
/// An integer value representing the selected noise channel to use when
///     determining temperature jitter.
/// </summary>
/// TODO:
///     This variable creates an indirect dependency between SetTerrainTypes()
///     and GenerateTemperature. Need to either make this dependency explicit
///     or eliminate the coupling.
/// 
///     This variable is reassigned.
/// 
///     Might be possible to somehow eliminate this variable entirely by
///     using functional programming principles instead of reassigning
///     the variable.
/// 
///     Extract with other climate modeling data and algorithms into a separate
///     class.
    private int _temperatureJitterChannel;

/// <summary>
///     An array of floats representing thresholds for different temperature bands.
///     Used along with moisture bands to determine the index of biomes to be used
///     for the biome of a specific cell.
/// </summary>
/// TODO:
///     Extract with other climate modeling data and algorithms into a separate
///     class.
    private static float[] temperatureBands = { 0.1f, 0.3f, 0.6f };

/// <summary>
///     An array of floats representing thresholds for different moisture bands.
///     Used along with moisture bands to determine the index of biomes to be
///     used for the biome of a specific cell.
/// </summary>
/// TODO:
///     Extract with other climate modeling data and algorithms into a separate
///     class.
    private static float[] moistureBands = { 0.12f, 0.28f, 0.85f };

    /* Array representing a matrix of biomes along the temperature bands:
        *
        *  0.1 [desert][snow][snow][snow]
        *  0.3 [desert][mud][mud, sparse flora][mud, average flora]
        *  0.6 [desert][grass][grass, sparse flora ][grass, average flora]
        *      [desert][grass, sparse flora][grass, average flora][grass, dense flora]
        *  Temperature/Moisture | 0.12 | 0.28 | 0.85
        * */
/// <summary>
///     An array of Biome structs representing a matrix of possible biomes
///     for a particular cell, indexed by its temperature and moisture.
/// </summary>
/// TODO:
///     Extract with other climate modeling data and algoritms into a
///     separate class.
    private static Biome[] biomes = {
        new Biome(0, 0), new Biome(4, 0), new Biome(4, 0), new Biome(4, 0),
        new Biome(0, 0), new Biome(2, 0), new Biome(2, 1), new Biome(2, 2),
        new Biome(0, 0), new Biome(1, 0), new Biome(1, 1), new Biome(1, 2),
        new Biome(0, 0), new Biome(1, 1), new Biome(1, 2), new Biome(1, 3)
    };

    public HexGrid GenerateHistoricalBoard(
        int width,
        int height,
        int tileLimit
    ) {
        HexGrid result = HexGrid.GetGrid();
        result.Initialize(width, height, true, 10f);
        int _cellCount = result.WidthInCells * result.HeightInCells;

        for (int i = 0; i < _cellCount; i++) {
            result.GetCell(i).TerrainTypeIndex = 0;
        }

        List<HexCell> constellation = new List<HexCell>();

        int terrainIndex = 0;

        HexCell current = result.GetCell((height * width) / 2);
        constellation.Add(current);
        current.SetElevation(2, 10f);
        current.TerrainTypeIndex = terrainIndex;        

        foreach (HexCell neighbor in current.Neighbors) {
            constellation.Add(neighbor);
            neighbor.TerrainTypeIndex = terrainIndex;
            neighbor.SetElevation(2, 10f);
        }

        List<HexCell> toGrow = GrowConstellation(
            constellation, 
            tileLimit,
            ++terrainIndex
        );

        return result;
    }

    private List<HexCell> GrowConstellation(
        List<HexCell> constellation,
        int tileLimit,
        int currTerrain
    ) {
        if (currTerrain > 4) {
            currTerrain = 0;
        }
        if (constellation.Count >= tileLimit) {
            return constellation;
        }
        else {
            System.Random rand = new System.Random();
            double probability = rand.NextDouble();
            int growthThreshold;

            if (probability > 0.5f) {
                growthThreshold = 2;
            }
            else {
                growthThreshold = 3;
            }
            HexCell expansion = GetValidExpansion(constellation, growthThreshold);
            for (int i = 0; i < 6; ++i) {
                HexCell neighbor = expansion.GetNeighbor((HexDirection)i);
                TryAddToConstellation(constellation, neighbor, currTerrain);
            }
            return GrowConstellation(constellation, tileLimit, ++currTerrain);
        }
    }

    private HexCell GetValidExpansion(List<HexCell> constellation, int growthThreshold) {
        HexCell result = null;

        System.Random random = new System.Random();

        while(result == null) {
            HexCell expansionCandidate = constellation[random.Next(constellation.Count)];
            int numOutsideConstellation = 0;
            foreach (HexCell neighbor in expansionCandidate.Neighbors) {
                if (!constellation.Contains(neighbor)) {
                    numOutsideConstellation++;
                } 
            }

            if (numOutsideConstellation >= growthThreshold) {
                result = expansionCandidate;
            }
        }

        return result;
    }

    private void TryAddToConstellation(List<HexCell> constellation, HexCell cell, int terrainType) {
        if (constellation.Contains(cell)) {

        }
        else {
            constellation.Add(cell);
            cell.SetElevation(2, 10f);
            cell.TerrainTypeIndex = terrainType;
        }
    }

/// <summary>
/// Generate a HexGrid using the standard RootGen algorithm.
/// </summary>
/// <param name="config">
///     The configuration data for the map to be generated.
/// </param>
/// <returns>
///     A randomly generated HexGrid object.
/// </returns>
    public HexGrid GenerateMap(RootGenConfig config) {        
        HexGrid result = HexGrid.GetGrid();
        int seed;

        if (config.useFixedSeed) {
            seed = config.seed;
        } 
        else {
            config.seed = Random.Range(0, int.MaxValue);
            config.seed ^= (int)System.DateTime.Now.Ticks;
            config.seed ^= (int)Time.time;
            config.seed &= int.MaxValue;
            seed = config.seed;
        }

// Snapshot the initial random state before consuming the random sequence.
        Random.State snapshot = RandomState.Snapshot(seed);

        result.Initialize(
            config.width,
            config.height,
            config.wrapping,
            config.cellOuterRadius
        );

        if (_searchFrontier == null) {
            _searchFrontier = new CellPriorityQueue();
        }

        for (int i = 0; i < result.NumCells; i++) {
            result.GetCell(i).WaterLevel = config.waterLevel;
        }

        _regions = GenerateRegions(
            result,
            config.regionBorder,
            config.mapBorderX,
            config.mapBorderZ,
            config.numRegions
        );
        
        int numLandCells = GetNumLandCells(
            config.landPercentage,
            result.NumCells,
            config.sinkProbability,
            config.chunkSizeMin,
            config.chunkSizeMax,
            result,
            config.highRiseProbability,
            config.elevationMin,
            config.elevationMax,
            config.waterLevel,
            config.jitterProbability,
            _searchFrontier,
            _searchFrontierPhase,
            config.cellOuterRadius
        );

        GenerateErosion(
            result,
            config.erosionPercentage,
            result.NumCells,
            config.cellOuterRadius
        );

        GenerateClimate(
            result,
            config.startingMoisture,
            result.NumCells,
            config.evaporationFactor,
            config.precipitationFactor,
            config.elevationMax,
            config.windDirection,
            config.windStrength,
            config.runoffFactor,
            config.seepageFactor
        );

        GenerateRivers(
            result,
            numLandCells,
            config.waterLevel,
            config.elevationMax,
            config.riverPercentage,
            config.extraLakeProbability,
            config.cellOuterRadius
        );

        SetTerrainTypes(
            config.elevationMax,
            config.waterLevel,
            result.NumCells,
            config.hemisphere,
            config.temperatureJitter,
            config.lowTemperature,
            config.highTemperature,
            result,
            config.cellOuterRadius
        );

// Reset the search phase of the cells in the HexGrid to avoid search errors.
        for (int i = 0; i < result.NumCells; i++) {
            result.GetCell(i).SearchPhase = 0;
        }

// Restore the snapshot of the random state taken before consuming the
// random sequence.
        Random.state = snapshot;

        return result;
    }

    private List<MapRegionRect> GenerateRegions(
        HexGrid grid,
        int regionBorder,
        int mapBorderX,
        int mapBorderZ,
        int numRegions
    ) {
        return new List<MapRegionRect>(
            SubdivideRegions(
                grid.WidthInCells,
                grid.HeightInCells,
                mapBorderX,
                mapBorderZ,
                numRegions,
                regionBorder,
                grid.Wrapping
            )
        );
    }

    private List<MapRegionRect> SubdivideRegions(
        int cellCountX,
        int cellCountZ,
        int mapBorderX,
        int mapBorderZ,
        int numRegions,
        int regionBorder,
        bool wrapping
    ) {
        List<MapRegionRect> result = new List<MapRegionRect>();

        int borderX = wrapping ? regionBorder : mapBorderX;

        int rootXMin = cellCountX > (borderX * 2) ?
            borderX : 0;
        int rootZMin = cellCountZ > (borderX * 2) ?
            borderX : 0;
        int rootXMax = cellCountX > (borderX * 2) ?
            cellCountX - borderX  - 1 : cellCountX - 1;
        int rootZMax = cellCountZ > (borderX * 2) ?
            cellCountZ - mapBorderZ - 1 : cellCountZ - 1;


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
                    temp.AddRange(mapRect.SubdivideVertical(regionBorder));
                }
                else {
                    temp.AddRange(mapRect.SubdivideHorizontal(regionBorder));
                }
            }

            result.Clear();
            result.AddRange(temp);
            temp.Clear();
            i++;
        }

        return result;
    }

    private int GetNumLandCells(
        int percentLand,
        int numCells,
        float sinkProbability,
        int chunkSizeMin,
        int chunkSizeMax, 
        HexGrid grid,
        float highRiseProbability,
        int elevationMin,
        int elevationMax,
        int waterLevel,
        float jitterProbability,
        CellPriorityQueue searchFrontier,
        int searchFrontierPhase,
        float cellOuterRadius
    ) {
        int landBudget = Mathf.RoundToInt(
            numCells * percentLand * 0.01f
        );

        int result = landBudget;

        for (int guard = 0; guard < 10000; guard++) {
            bool sink = Random.value < sinkProbability;

            for (int i = 0; i < _regions.Count; i++) {
                MapRegionRect region = _regions[i];
                int chunkSize = Random.Range(chunkSizeMin, chunkSizeMax + 1);
                if (sink) {
                    landBudget = SinkTerrain(
                        grid,
                        chunkSize,
                        landBudget,
                        region,
                        highRiseProbability,
                        elevationMin,
                        waterLevel,
                        jitterProbability,
                        searchFrontier,
                        searchFrontierPhase,
                        cellOuterRadius
                    );
                }
                else {
                    landBudget = RaiseTerrain(
                        grid,
                        chunkSize,
                        landBudget,
                        region,
                        highRiseProbability,
                        elevationMax,
                        waterLevel,
                        jitterProbability,
                        cellOuterRadius
                    );
                    if (landBudget == 0) {
                        return result;
                    }
                }
            }
        }
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
        HexGrid grid,
        int chunkSize,
        int budget,
        MapRegionRect region,
        float highRiseProbability,
        int elevationMin,
        int waterLevel,
        float jitterProbability,
        CellPriorityQueue searchFrontier,
        int searchFrontierPhase,
        float cellOuterRadius
    ) {
        searchFrontierPhase += 1;
        
// Region dimensions are used to calcualte valid bounds for randomly
// selecting first cell to apply the sink algorithm to. This results
// in continent like formations loosely constrained by the size of
// a given region.
// TODO:
//  This algorithm could probably be improved by finding a real-world
//  model to implement for sinking terrain based on tectonic shift.
        HexCell firstCell = GetRandomCell(grid, region);
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        searchFrontier.Enqueue(firstCell);

        HexCoordinates center = firstCell.Coordinates;

        int sink = Random.value < highRiseProbability ? 2 : 1;
        int size = 0;

        while (size < chunkSize && searchFrontier.Count > 0) {
            HexCell current = searchFrontier.Dequeue();
            int originalElevation = current.Elevation;

            int newElevation = current.Elevation - sink;

            if (newElevation < elevationMin) {
                continue;
            }

            current.SetElevation(newElevation, cellOuterRadius);

            if (
                originalElevation >= waterLevel &&
                newElevation < waterLevel
            ) {
                budget += 1;
            }

            size += 1;

            for (
                HexDirection direction = HexDirection.Northeast;
                direction <= HexDirection.Northwest;
                direction++
            ) {
                HexCell neighbor = current.GetNeighbor(direction);
                if (neighbor && neighbor.SearchPhase < _searchFrontierPhase)
                {
                    neighbor.SearchPhase = _searchFrontierPhase;

                    /* Set the distance to be the distance from the center of the
                        * raised terrain chunk, so that cells closer to the center are
                        * prioritized when raising terrain.
                        */
                    neighbor.Distance = neighbor.Coordinates.DistanceTo(center);

                    /* Set the search heuristic to 1 or 0 based on a configurable
                        * jitter probability, to cause perturbation in the cells which
                        * are selected to be raised. This will make the chunks generated
                        * less uniform.
                        */
                    neighbor.SearchHeuristic =
                        Random.value < jitterProbability ? 1 : 0;
                    searchFrontier.Enqueue(neighbor);
                }
            }
        }

        searchFrontier.Clear();
        return budget;
    }

    private int RaiseTerrain(
        HexGrid grid, 
        int chunkSize, 
        int budget, 
        MapRegionRect region,
        float highRiseProbability,
        int elevationMax,
        int waterLevel,
        float jitterProbability,
        float cellOuterRadius
    ) {
        _searchFrontierPhase += 1;

// Region dimensions are used to calcualte valid bounds for randomly
// selecting first cell to apply the raise algorithm to. This results
// in continent like formations loosely constrained by the size of
// a given region.
// TODO:
//  This algorithm could probably be improved by finding a real-world
//  model to implement for raising terrain based on tectonic shift.
        HexCell firstCell = GetRandomCell(grid, region);
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        HexCoordinates center = firstCell.Coordinates;

        int rise = Random.value < highRiseProbability ? 2 : 1;
        int size = 0;

        while (size < chunkSize && _searchFrontier.Count > 0) {
            HexCell current = _searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;

            if (newElevation > elevationMax) {
                continue;
            }

            current.SetElevation(newElevation, cellOuterRadius);

            if (
                originalElevation < waterLevel &&
                newElevation >= waterLevel &&
                --budget == 0
            ) {
                break;
            }

            size += 1;

            for (
                HexDirection direction = HexDirection.Northeast;
                direction <= HexDirection.Northwest;
                direction++
            ) {
                HexCell neighbor = current.GetNeighbor(direction);
                if (neighbor && neighbor.SearchPhase < _searchFrontierPhase) {
                    neighbor.SearchPhase = _searchFrontierPhase;

                    /* Set the distance to be the distance from the center of the
                        * raised terrain chunk, so that cells closer to the center are
                        * prioritized when raising terrain.
                        */
                    neighbor.Distance = neighbor.Coordinates.DistanceTo(center);

                    /* Set the search heuristic to 1 or 0 based on a configurable
                        * jitter probability, to cause perturbation in the cells which
                        * are selected to be raised. This will make the chunks generated
                        * less uniform.
                        */
                    neighbor.SearchHeuristic = 
                        Random.value < jitterProbability ? 1 : 0;

                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();

        return budget;
    }

    private void GenerateErosion(
        HexGrid grid,
        int erosionPercentage,
        int numCells,
        float cellOuterRadius

    ) {
        List<HexCell> erodibleCells = ListPool<HexCell>.Get();

        for (int i = 0; i < numCells; i++) {
            HexCell cell = grid.GetCell(i);

            if (IsErodible(cell)) {
                erodibleCells.Add(cell);
            }
        }

        int targetErodibleCount =
            (int)(erodibleCells.Count * (100 - erosionPercentage) * 0.01f);

        while (erodibleCells.Count > targetErodibleCount) {
            int index = Random.Range(0, erodibleCells.Count);
            HexCell cell = erodibleCells[index];
            HexCell erosionRunoffTargetCell = GetErosionRunoffTarget(cell);

            cell.SetElevation(cell.Elevation - 1, cellOuterRadius);
            erosionRunoffTargetCell.SetElevation(
                erosionRunoffTargetCell.Elevation + 1,
                cellOuterRadius
            );

            if (!IsErodible(cell)) {
                erodibleCells[index] = erodibleCells[erodibleCells.Count - 1];
                erodibleCells.RemoveAt(erodibleCells.Count - 1);
            }

            for (
                HexDirection direction = HexDirection.Northeast;
                direction <= HexDirection.Northwest;
                direction++
            ) {
                HexCell neighbor = cell.GetNeighbor(direction);

                if (
                    neighbor && 
                    neighbor.Elevation == cell.Elevation + 2 &&
                    !erodibleCells.Contains(neighbor)
                ) {
                    erodibleCells.Add(neighbor);
                }
            }

            if (
                IsErodible(erosionRunoffTargetCell) &&
                !erodibleCells.Contains(erosionRunoffTargetCell)
            ) {
                erodibleCells.Add(erosionRunoffTargetCell);
            }

            for (
                HexDirection direction = HexDirection.Northeast;
                direction <= HexDirection.Northwest;
                direction++
            ) {
                HexCell neighbor = erosionRunoffTargetCell.GetNeighbor(direction);
                if (
                    neighbor && neighbor != cell &&
                    neighbor.Elevation == erosionRunoffTargetCell.Elevation + 1 &&
                    !IsErodible(neighbor)
                ) {
                    erodibleCells.Remove(neighbor);
                }
            }
        }

        ListPool<HexCell>.Add(erodibleCells);
    }

    private void GenerateClimate(
        HexGrid grid,
        float startingMoisture,
        int numCells,
        float evaporationFactor,
        float precipitationFactor,
        int elevationMax,    
        HexDirection windDirection,
        float windStrength,
        float runoffFactor,
        float seepageFactor

    ) {
        _climate.Clear();
        _nextClimate.Clear();

        ClimateData initialData = new ClimateData();
        initialData.moisture = startingMoisture;

        ClimateData clearData = new ClimateData();

        for (int i = 0; i < numCells; i++) {
            _climate.Add(initialData);
            _nextClimate.Add(clearData);
        }

        for (int cycle = 0; cycle < 40; cycle++) {
            for (int i = 0; i < numCells; i++) {
                StepClimate(
                    grid,
                    i,
                    evaporationFactor,
                    precipitationFactor,
                    elevationMax,
                    windDirection,
                    windStrength,
                    runoffFactor,
                    seepageFactor
                );
            }

            /* Make sure that the climate data being calculated in the current
                * cycle is always from the current climate and not the next climate.
                */

            // Store the modified climate data in swap.
            List<ClimateData> swap = _climate;

            // Store the cleared climate data in the current climate.
            _climate = _nextClimate;

            // Store the modified climate data in next climate
            _nextClimate = swap;
        }
    }

    private void StepClimate(
        HexGrid grid,
        int cellIndex,
        float evaporationFactor,
        float precipitationFactor,
        int elevationMax,
        HexDirection windDirection,
        float windStrength,
        float runoffFactor,
        float seepageFactor
    ) {
        HexCell cell = grid.GetCell(cellIndex);
        ClimateData cellClimate = _climate[cellIndex];

        if (cell.IsUnderwater) {
            cellClimate.moisture = 1f;
            cellClimate.clouds += evaporationFactor;
        }
        else {
            float evaporation = cellClimate.moisture * evaporationFactor;
            cellClimate.moisture -= evaporation;
            cellClimate.clouds += evaporation;
        }

        float precipitation = cellClimate.clouds * precipitationFactor;
        cellClimate.clouds -= precipitation;
        cellClimate.moisture += precipitation;

        // Cloud maximum has an inverse relationship with elevation maximum.
        float cloudMaximum = 1f - cell.ViewElevation / (elevationMax + 1f);

        if (cellClimate.clouds > cloudMaximum) {
            cellClimate.moisture += cellClimate.clouds - cloudMaximum;
            cellClimate.clouds = cloudMaximum;
        }

        HexDirection mainDispersalDirection = windDirection.Opposite();

        float cloudDispersal = cellClimate.clouds * (1f / (5f + windStrength));
        float runoff = cellClimate.moisture * runoffFactor * (1f / 6f);
        float seepage = cellClimate.moisture * seepageFactor * (1f / 6f);

        for (
            HexDirection direction = HexDirection.Northeast;
            direction <= HexDirection.Northwest;
            direction++
        ) {
            HexCell neighbor = cell.GetNeighbor(direction);

            if (!neighbor) {
                continue;
            }

            ClimateData neighborClimate = _climate[neighbor.Index];

            if (direction == mainDispersalDirection) {
                neighborClimate.clouds += cloudDispersal * windStrength;
            }
            else {
                neighborClimate.clouds += cloudDispersal;
            }

            int elevationDelta = neighbor.ViewElevation - cell.ViewElevation;

            if (elevationDelta < 0) {
                cellClimate.moisture -= runoff;
                neighborClimate.moisture += runoff;
            }
            else if (elevationDelta == 0) {
                cellClimate.moisture -= seepage;
                neighborClimate.moisture += seepage;
            }

            _climate[neighbor.Index] = neighborClimate;
        }

        // Create a cell for the next climate.
        ClimateData nextCellClimate = _nextClimate[cellIndex];

        // Modify the data for the next climate.
        nextCellClimate.moisture += cellClimate.moisture;

        /* Ensure that no cell can have more moisture than
            *a cell that is underwater.
            */
        if (nextCellClimate.moisture > 1f) {
            nextCellClimate.moisture = 1f;
        }

        //Store the data for the next climate.
        _nextClimate[cellIndex] = nextCellClimate;

        //Clear the current climate data.
        _climate[cellIndex] = new ClimateData();
    }

    private void GenerateRivers(
        HexGrid grid,
        int numLandCells,
        int waterLevel,
        int elevationMax,
        int riverPercentage,
        float extraLakeProbability,
        float cellOuterRadius
    ) {
        List<HexCell> riverOrigins = ListPool<HexCell>.Get();

        for (int i = 0; i < numLandCells; i++) {
            HexCell cell = grid.GetCell(i);

            if (cell.IsUnderwater) {
                continue;
            }

            ClimateData data = _climate[i];
            float weight =
                data.moisture * (cell.Elevation - waterLevel) /
                (elevationMax - waterLevel);

            if (weight > 0.75) {
                riverOrigins.Add(cell);
                riverOrigins.Add(cell);
            }

            if (weight > 0.5f) {
                riverOrigins.Add(cell);
            }

            if (weight > 0.25f) {
                riverOrigins.Add(cell);
            }
        }

        int riverBudget = Mathf.RoundToInt(numLandCells * riverPercentage * 0.01f);

        while (riverBudget > 0 && riverOrigins.Count > 0) {
            int index = Random.Range(0, riverOrigins.Count);
            int lastIndex = riverOrigins.Count - 1;
            HexCell origin = riverOrigins[index];
            riverOrigins[index] = riverOrigins[lastIndex];
            riverOrigins.RemoveAt(lastIndex);

            if (!origin.HasRiver) {
                bool isValidOrigin = true;

                for (
                    HexDirection direction = HexDirection.Northeast;
                    direction <= HexDirection.Northwest;
                    direction++
                ) {
                    HexCell neighbor = origin.GetNeighbor(direction);

                    if (neighbor && (neighbor.HasRiver || neighbor.IsUnderwater)) {
                        isValidOrigin = false;
                        break;
                    }
                }

                if (isValidOrigin) {
                    riverBudget -= GenerateRiver(
                        origin,
                        extraLakeProbability,
                        cellOuterRadius
                    );
                }
            }
        }

        if (riverBudget > 0) {
            Debug.LogWarning("Failed to use up river budget.");
        }

        ListPool<HexCell>.Add(riverOrigins);
    }

    private int GenerateRiver(
        HexCell origin,
        float extraLakeProbability,
        float cellOuterRadius
    ) {
        int length = 1;
        HexCell cell = origin;
        HexDirection direction = HexDirection.Northeast;

        while (!cell.IsUnderwater) {
            int minNeighborElevation = int.MaxValue;

            _flowDirections.Clear();

            for (
                HexDirection directionCandidate = HexDirection.Northeast;
                directionCandidate <= HexDirection.Northwest;
                directionCandidate++
            ) {
                HexCell neighbor = cell.GetNeighbor(directionCandidate);

                if (!neighbor) {
                    continue;
                }

                if (neighbor.Elevation < minNeighborElevation) {
                    minNeighborElevation = neighbor.Elevation;
                }

                if (neighbor == origin || neighbor.HasIncomingRiver) {
                    continue;
                }

                int delta = neighbor.Elevation - cell.Elevation;

                if (delta > 0) {
                    continue;
                }

                if (neighbor.HasOutgoingRiver) {
                    cell.SetOutgoingRiver(directionCandidate);
                    return length;
                }

                if (delta < 0) {
                    _flowDirections.Add(directionCandidate);
                    _flowDirections.Add(directionCandidate);
                    _flowDirections.Add(directionCandidate);
                }

                if (
                    length == 1 ||
                    (directionCandidate != direction.Next2() &&
                    directionCandidate != direction.Previous2())
                ) {
                    _flowDirections.Add(directionCandidate);
                }

                _flowDirections.Add(directionCandidate);
            }

            if (_flowDirections.Count == 0) {
                if (length == 1) {
                    return 0;
                }

                if (minNeighborElevation >= cell.Elevation) {
                    cell.WaterLevel = minNeighborElevation;

                    if (minNeighborElevation == cell.Elevation) {
                        cell.SetElevation(
                            minNeighborElevation - 1,
                            cellOuterRadius
                        );
                    }
                }

                break;
            }

            direction = _flowDirections[Random.Range(0, _flowDirections.Count)];
            cell.SetOutgoingRiver(direction);
            length += 1;

            if (
                minNeighborElevation >= cell.Elevation &&
                Random.value < extraLakeProbability
            ) {
                cell.WaterLevel = cell.Elevation;
                cell.SetElevation(cell.Elevation - 1, cellOuterRadius);
            }

            cell = cell.GetNeighbor(direction);
        }

        return length;
    }

    private bool IsErodible(HexCell cell) {
        int erodibleElevation = cell.Elevation - 2;
        for (
            HexDirection direction = HexDirection.Northeast;
            direction <= HexDirection.Northwest;
            direction++
        ) {
            HexCell neighbor = cell.GetNeighbor(direction);

            if (neighbor && neighbor.Elevation <= erodibleElevation) {
                return true;
            }
        }

        return false;
    }

    private HexCell GetErosionRunoffTarget(HexCell cell) {
        List<HexCell> candidates = ListPool<HexCell>.Get();
        int erodibleElevation = cell.Elevation - 2;

        for (
            HexDirection direction = HexDirection.Northeast;
            direction <= HexDirection.Northwest;
            direction++
        ) {
            HexCell neighbor = cell.GetNeighbor(direction);
            if (neighbor && neighbor.Elevation <= erodibleElevation) {
                candidates.Add(neighbor);
            }
        }

        HexCell target = candidates[Random.Range(0, candidates.Count)];
        ListPool<HexCell>.Add(candidates);
        return target;
    }

    private void SetTerrainTypes(
        int elevationMax,
        int waterLevel,
        int numCells,
        HemisphereMode hemisphereMode,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        HexGrid grid,
        float cellOuterRadius
    ) {
        _temperatureJitterChannel = Random.Range(0, 4);
        int rockDesertElevation =
            elevationMax - (elevationMax - waterLevel) / 2;

        for (int i = 0; i < numCells; i++) {
            HexCell cell = grid.GetCell(i);

            float temperature = GenerateTemperature(
                grid,
                cell,
                hemisphereMode,
                waterLevel,
                elevationMax,
                temperatureJitter,
                lowTemperature,
                highTemperature,
                cellOuterRadius
            );

            float moisture = _climate[i].moisture;

            if (!cell.IsUnderwater) {
                int temperatureBand = 0;

                for (; temperatureBand < temperatureBands.Length; temperatureBand++) {
                    if (temperature < temperatureBands[temperatureBand]) {
                        break;
                    }
                }

                int moistureBand = 0;

                for (; moistureBand < moistureBands.Length; moistureBand++) {
                    if (moisture < moistureBands[moistureBand]) {
                        break;
                    }
                }

                Biome cellBiome = biomes[temperatureBand * 4 + moistureBand];

                if (cellBiome.terrain == 0) {
                    if (cell.Elevation >= rockDesertElevation) {
                        cellBiome.terrain = 3;
                    }
                }
                else if (cell.Elevation == elevationMax) {
                    cellBiome.terrain = 4;
                }

                if (cellBiome.terrain == 4) {
                    cellBiome.plant = 0;
                }

                if (cellBiome.plant < 3 && cell.HasRiver) {
                    cellBiome.plant += 1;
                }

                cell.TerrainTypeIndex = cellBiome.terrain;
                cell.PlantLevel = cellBiome.plant;
            }
            else {
                int terrain;

                if (cell.Elevation == waterLevel - 1) {
                    int cliffs = 0;
                    int slopes = 0;

                    for
                    (
                        HexDirection direction = HexDirection.Northeast;
                        direction <= HexDirection.Northwest;
                        direction++
                    ) {
                        HexCell neighbor = cell.GetNeighbor(direction);

                        if (!neighbor) {
                            continue;
                        }

                        int delta = neighbor.Elevation - cell.WaterLevel;

                        if (delta == 0) {
                            slopes += 1;
                        }
                        else if (delta > 0) {
                            cliffs += 1;
                        }
                    }

                    /*More than half neighbors at same level.
                        *Inlet or lake, therefore terrain is grass.
                        */
                    if (cliffs + slopes > 3) {
                        terrain = 1;
                    }
                    // More than half cliffs, terrain is stone.
                    else if (cliffs > 0) {
                        terrain = 3;
                    }
                    // More than half slopes, terrain is beach.
                    else if (slopes > 0) {
                        terrain = 0;
                    }
                    // Shallow non-coast, terrain is grass.
                    else {
                        terrain = 1;
                    }
                }
                else if (cell.Elevation >= waterLevel) {
                    terrain = 1;
                }
                else if (cell.Elevation < 0) {
                    terrain = 3;
                }
                else {
                    terrain = 2;
                }

                /* Coldest temperature band produces mud instead of
                    * grass.
                    */
                if (terrain == 1 && temperature < temperatureBands[0]) {
                    terrain = 2;
                }

                cell.TerrainTypeIndex = terrain;
            }
        }
    }

    private float GenerateTemperature(
        HexGrid grid, 
        HexCell cell,
        HemisphereMode hemisphere,
        int waterLevel,
        int elevationMax,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        float cellOuterRadius
    ) {
        float latitude = (float)cell.Coordinates.Z / grid.HeightInCells;

        if (hemisphere == HemisphereMode.Both) {
            latitude *= 2f;

            if (latitude > 1f) {
                latitude = 2f - latitude;
            }
        }
        else if (hemisphere == HemisphereMode.North) {
            latitude = 1f - latitude;
        }

        float temperature =
            Mathf.LerpUnclamped(
                lowTemperature,
                highTemperature,
                latitude
            );

        temperature *= 
            1f - 
            (cell.ViewElevation - waterLevel) /
            (elevationMax - waterLevel + 1f);

        float jitter =
            HexagonPoint.SampleNoise(
                cell.Position * 0.1f,
                cellOuterRadius
            )[_temperatureJitterChannel];

        temperature += (jitter * 2f - 1f) * temperatureJitter;

        return temperature;
    }

    private HexCell GetRandomCell(HexGrid grid, MapRegionRect region)
    {
        return grid.GetCell(
            Random.Range(region.OffsetXMin, region.OffsetXMax),
            Random.Range(region.OffsetZMin, region.OffsetZMax)
        );
    }

    private void VisualizeRiverOrigins(
        HexGrid grid,
        int cellCount,
        int waterLevel,
        int elevationMax
    ) {
        for (int i = 0; i < cellCount; i++) {
            HexCell cell = grid.GetCell(i);

            float data = _climate[i].moisture * (cell.Elevation - waterLevel) /
                            (elevationMax - waterLevel);

            if (data > 0.75f) {
                cell.SetAndEnableMapVisualizationShaderData(1f);
            }
            else if (data > 0.5f) {
                cell.SetAndEnableMapVisualizationShaderData(0.5f);
            }
            else if (data > 0.25f) {
                cell.SetAndEnableMapVisualizationShaderData(0.25f);
            }
        }
    }

/// <summary>
/// Struct defining the bounds of a rectangular map region.
/// </summary>
    private struct MapRegionRect {
        private int _offsetXMin;
        private int _offsetXMax;
        private int _offsetZMin;
        private int _offsetZMax;

/// <summary>
/// The minimum X axis offset coordinate in the region.
/// </summary>
/// <value>
///     The provided value if <= OffsetXMax, otherwise
///     OffsetXMax.
/// </value>
        public int OffsetXMin {
            get { return _offsetXMin; }
            set {
                if (value < 0) {
                    _offsetXMin = 0;
                }
                else {
                    _offsetXMin = value <= _offsetXMax ?
                        value : _offsetXMax;
                }
            } 
        }

/// <summary>
/// The maximum X axis offset coordinate in the region.
/// </summary>
/// <value>
///     The provided value if >= OffsetXMin, otherwise
///     OffsetXMin.
/// </value>
        public int OffsetXMax {
            get { return _offsetXMax; }
            set {
                _offsetXMin = value >= _offsetXMin ?
                    value : _offsetXMin;
            }
        }

/// <summary>
/// The minimum Z axis offset coordinate in the region.
/// </summary>
/// <value>
///     The provided value if <= OffsetZMax,
///     otherwise OffsetZMax.
/// </value>
        public int OffsetZMin {
            get { return _offsetZMin; }
            set {
                if (value < 0) {
                    _offsetZMin = 0;
                }
                else {
                    _offsetZMin = value <= _offsetZMax ?
                        value : _offsetZMax;
                }
            }
        }

/// <summary>
/// The maximum Z axis offset coordinate in the region.
/// </summary>
/// <value>
///     The provided value if >= than OffsetZMin, otherwise
///     OffsetZMin.
/// </value>
        public int OffsetZMax {
            get { return _offsetZMax; }
            set {
                _offsetZMax = value >= _offsetZMin ?
                    value : _offsetZMin;
            }
        }

/// <summary>
/// The area of the region using offset coordinates.
/// </summary>
        public int OffsetArea {
            get {
                return OffsetSizeX * OffsetSizeZ;
            }
        }

/// <summary>
/// The size of the region along the x axis using offset coordinates.
/// </summary>
        public int OffsetSizeX {
            get {
                return (OffsetXMax - OffsetXMin);
            }
        }

/// <summary>
/// The size of the region along the z axis using offset coordinates.
/// </summary>
        public int OffsetSizeZ {
            get {
                return (OffsetZMax - OffsetZMin);
            }
        }

/// <summary>
/// The middle offset coordinate along the x axis.
/// </summary>
        public int OffsetXCenter {
            get {
                return
                    OffsetXMin + (OffsetSizeX / 2);
            }
        }

/// <summary>
/// The middle offset coordinate along the z axis.
/// </summary>
        public int OffsetZCenter {
            get {
                return
                    OffsetZMin + (OffsetSizeZ / 2);
            }
        }

        public override string ToString() {
            return 
                "xMin: " + _offsetXMin +
                ", xMax: " + _offsetXMax + 
                ", zMin: " + _offsetZMin +
                ", zMax: " + _offsetZMax; 
        }

/// <summary>
/// Constructor.
/// </summary>
/// <param name="offsetXMin">
///     The minimum X axis offset coordinate of the region.
///     Will be set to offsetXMax if greater than offsetXMax.
///     If set to a negative value, will be set to 0.
/// </param>
/// <param name="offsetXMax">
///     The maximum X axis offset coordinate of the region.
///     Will be set to offsetXMin if less than offsetXMin.
/// </param>
/// <param name="offsetZMin">
///     The minimum Z axis offset coordinate of the region.
///     Will be set ot offsetZMax is greater than offsetZMax.
///     if set to a negative value, will be set to 0.
/// </param>
/// <param name="offsetZMax">
///     The maximum Z axis offset coordinate of the region.
///     Will be set of offsetZMin if greater than offsetZMin.
/// </param>
        public MapRegionRect(
            int offsetXMin,
            int offsetXMax,
            int offsetZMin,
            int offsetZMax
        ) {
            offsetXMin = offsetXMin < 0 ? 0 : offsetXMin;
            offsetZMin = offsetXMin < 0 ? 0 : offsetZMin;

            _offsetXMin =
                offsetXMin <= offsetXMax ? 
                offsetXMin : offsetXMax;

            _offsetXMax =
                offsetXMax >= offsetXMin ?
                offsetXMax : offsetXMin;

            _offsetZMin =
                offsetZMin <= offsetZMax ?
                offsetZMin : offsetZMax;

            _offsetZMax =
                offsetZMax >= offsetZMin ?
                offsetZMax : offsetZMin;
        }

/// <summary>
/// Subdivide the border along the z axis.
/// </summary>
/// <param name="border">
///     (Optional) place a border between the two regions. If border is
///     greater than or equal to the size of the X dimension, border will be
///     set to the x dimension - 2.
/// </param>
/// <returns></returns>
        public List<MapRegionRect> SubdivideHorizontal(int border = 0) {
            if (this.OffsetSizeX - border < 3) {
                RootLog.Log(
                    "Border cannot reduce x dimension below 3 or divison will" +
                    " be impossible. Setting border to 0.",
                    Severity.Debug,
                    "MapGenerator"
                );

                border = 0;
            }

            List<MapRegionRect> result = new List<MapRegionRect>();

            result.Add(
                new MapRegionRect(
                    this.OffsetXMin,
                    this.OffsetXMax,
                    this.OffsetZMin,
                    this.OffsetZCenter - border
                )
            );

            result.Add(
                new MapRegionRect(
                    this.OffsetXMin,
                    this.OffsetXMax,
                    this.OffsetZCenter + border,
                    this.OffsetZMax
                )
            );

            return result;
        }

        public List<MapRegionRect> SubdivideVertical(int border = 0) {
            if (this.OffsetSizeZ - border < 3) {
                RootLog.Log(
                    "Border cannot reduce z dimension below 3 or divison will" +
                    " be impossible. Setting border to 0.",
                    Severity.Debug,
                    "MapGenerator"
                );

                border = 0;
            }

            List<MapRegionRect> result = new List<MapRegionRect>();

            result.Add(
                new MapRegionRect(
                    this.OffsetXMin,
                    this.OffsetXCenter - border,
                    this.OffsetZMin,
                    this.OffsetZMax
                )
            );

            result.Add(
                new MapRegionRect(
                    this.OffsetXCenter + border,
                    this.OffsetXMax,
                    this.OffsetZMin,
                    this.OffsetZMax
                )
            );

            return result;
        }
    }

        private struct ClimateData {
            public float clouds;
            public float moisture;
        }

        private struct Biome {
            public int terrain;
            public int plant;

            public Biome(int terrain, int plant) {
                this.terrain = terrain;
                this.plant = plant;
            }
        }
    }

    

