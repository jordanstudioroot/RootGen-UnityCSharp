using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using RootLogging;
using RootCollections;


/// <summary>
/// Class encapsulating the RootGen map generation algorithms.
/// </summary>
public class MapGenerator
{
/// <summary>
/// The number of cells in the map.
/// </summary>
    private int _cellCount;

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
    private List<MapRegion> _regions;

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
        
        _cellCount = width * height;
        result.Initialize(width, height, true);

        for (int i = 0; i < _cellCount; i++) {
            result.GetCell(i).TerrainTypeIndex = 0;
        }

        List<HexCell> region = new List<HexCell>();

        int terrainIndex = 0;

        HexCell current = result.GetCell((height * width) / 2);
        region.Add(current);
        current.Elevation = 2;
        current.TerrainTypeIndex = terrainIndex;        

        foreach (HexCell neighbor in current.Neighbors) {
            region.Add(neighbor);
            neighbor.TerrainTypeIndex = terrainIndex;
            neighbor.Elevation = 2;
        }

        List<HexCell> toGrow = GrowRegion(
            region, 
            tileLimit,
            ++terrainIndex
        );

        return result;
    }

    private List<HexCell> GrowRegion(
        List<HexCell> region,
        int tileLimit,
        int currTerrain
    ) {
        if (currTerrain > 4) {
            currTerrain = 0;
        }
        if (region.Count >= tileLimit) {
            return region;
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
            HexCell expansion = GetValidExpansion(region, growthThreshold);
            for (int i = 0; i < 6; ++i) {
                HexCell neighbor = expansion.GetNeighbor((HexDirection)i);
                TryAddToRegion(region, neighbor, currTerrain);
            }
            return GrowRegion(region, tileLimit, ++currTerrain);
        }
    }

    private HexCell GetValidExpansion(List<HexCell> region, int growthThreshold) {
        HexCell result = null;

        System.Random random = new System.Random();

        while(result == null) {
            HexCell expansionCandidate = region[random.Next(region.Count)];
            int numOutsideRegion = 0;
            foreach (HexCell neighbor in expansionCandidate.Neighbors) {
                if (!region.Contains(neighbor)) {
                    numOutsideRegion++;
                } 
            }

            if (numOutsideRegion >= growthThreshold) {
                result = expansionCandidate;
            }
        }

        return result;
    }

    private void TryAddToRegion(List<HexCell> region, HexCell cell, int terrainType) {
        if (region.Contains(cell)) {

        }
        else {
            region.Add(cell);
            cell.Elevation = 2;
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
    public HexGrid GenerateMap(
        RootGenConfig config
    ) {
        HexGrid result = HexGrid.GetGrid();

// Store the current random state to later restore it so that the
// algorithm does not affect anything else using Random.
// TODO:
//      Create a new class or extension method that encapsulates this
//      process.
        Random.State originalRandomState = Random.state;

        if (!config.useFixedSeed) {
            config.seed = Random.Range(0, int.MaxValue);
            config.seed ^= (int)System.DateTime.Now.Ticks;
            config.seed ^= (int)Time.time;
            config.seed &= int.MaxValue;
        }

        Random.InitState(config.seed);
        _cellCount = config.width * config.height;
        result.Initialize(config.width, config.height, config.wrapping);

        if (_searchFrontier == null) {
            _searchFrontier = new CellPriorityQueue();
        }

        for (int i = 0; i < _cellCount; i++) {
            result.GetCell(i).WaterLevel = config.waterLevel;
        }

        GenerateRegions(config, result);
        int numLandCells = GetNumLandCells(config, result);
        GenerateErosion(config, result);
        GenerateClimate(config, result);
        GenerateRivers(config ,result, numLandCells);
        SetTerrainTypes(config, result);

        /* Reset the search phase of all cells to avoid collisions
            * with the World search algorithms.
            */
        for (int i = 0; i < _cellCount; i++) {
            result.GetCell(i).SearchPhase = 0;
        }

// Restore the original random state.
        Random.state = originalRandomState;
        return result;
    }

    private void GenerateRegions(RootGenConfig config, HexGrid grid) {
        if (_regions == null) {
            _regions = new List<MapRegion>();
        }
        else {
            _regions.Clear();
        }

        int borderX = grid.Wrapping ? config.regionBorder : config.mapBorderX;

        MapRegion region;
        switch (config.regionCount) {
            default:
                if (grid.Wrapping)
                {
                    borderX = 0;
                }

                region.xMin = borderX;
                region.xMax = grid.CellCountX - borderX;
                region.zMin = config.mapBorderZ;
                region.zMax = grid.CellCountZ - config.mapBorderZ;
                _regions.Add(region);
                break;
            case 2:
                if (Random.value < 0.5f)
                {
                    region.xMin = borderX;
                    region.xMax = grid.CellCountX / 2 - config.regionBorder;
                    region.zMin = config.mapBorderZ;
                    region.zMax = grid.CellCountZ - config.mapBorderZ;
                    _regions.Add(region);
                    region.xMin = grid.CellCountX / 2 + config.regionBorder;
                    region.xMax = grid.CellCountX - borderX;
                    _regions.Add(region);
                }
                else
                {
                    if (grid.Wrapping)
                    {
                        borderX = 0;
                    }

                    region.xMin = borderX;
                    region.xMax = grid.CellCountX - borderX;
                    region.zMin = config.mapBorderZ;
                    region.zMax = grid.CellCountZ / 2 - config.regionBorder;
                    _regions.Add(region);
                    region.zMin = grid.CellCountZ / 2 + config.regionBorder;
                    region.zMax = grid.CellCountZ - config.mapBorderZ;
                    _regions.Add(region);
                }
                break;
            case 3:
                region.xMin = borderX;
                region.xMax = grid.CellCountX / 3 - config.regionBorder;
                region.zMin = config.mapBorderZ;
                region.zMax = grid.CellCountZ - config.mapBorderZ;
                _regions.Add(region);
                region.xMin = grid.CellCountX / 3 + config.regionBorder;
                region.xMax = grid.CellCountX * 2 / 3 - config.regionBorder;
                _regions.Add(region);
                region.xMin = grid.CellCountX * 2 / 3 + config.regionBorder;
                region.xMax = grid.CellCountX - borderX;
                _regions.Add(region);
                break;
            case 4:
                region.xMin = borderX;
                region.xMax = grid.CellCountX / 2 - config.regionBorder;
                region.zMin = config.mapBorderZ;
                region.zMax = grid.CellCountZ / 2 - config.regionBorder;
                _regions.Add(region);
                region.xMin = grid.CellCountX / 2 + config.regionBorder;
                region.xMax = grid.CellCountX - borderX;
                _regions.Add(region);
                region.zMin = grid.CellCountZ / 2 + config.regionBorder;
                region.zMax = grid.CellCountZ - config.mapBorderZ;
                _regions.Add(region);
                region.xMin = borderX;
                region.xMax = grid.CellCountX / 2 - config.regionBorder;
                _regions.Add(region);
                break;
        }
    }

    private int GetNumLandCells(RootGenConfig config, HexGrid grid) {
        int landBudget = Mathf.RoundToInt(
            _cellCount * config.landPercentage * 0.01f
        );

        int result = landBudget;

        for (int guard = 0; guard < 10000; guard++) {
            bool sink = Random.value < config.sinkProbability;

            for (int i = 0; i < _regions.Count; i++) {
                MapRegion region = _regions[i];
                int chunkSize = Random.Range(config.chunkSizeMin, config.chunkSizeMax + 1);
                if (sink) {
                    landBudget = SinkTerrain(config, grid, chunkSize, landBudget, region);
                }
                else {
                    landBudget = RaiseTerrain(config, grid, chunkSize, landBudget, region);
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

    private void GenerateErosion(RootGenConfig config, HexGrid grid) {
        List<HexCell> erodibleCells = ListPool<HexCell>.Get();

        for (int i = 0; i < _cellCount; i++) {
            HexCell cell = grid.GetCell(i);

            if (IsErodible(cell)) {
                erodibleCells.Add(cell);
            }
        }

        int targetErodibleCount =
            (int)(erodibleCells.Count * (100 - config.erosionPercentage) * 0.01f);

        while (erodibleCells.Count > targetErodibleCount) {
            int index = Random.Range(0, erodibleCells.Count);
            HexCell cell = erodibleCells[index];
            HexCell erosionRunoffTargetCell = GetErosionRunoffTarget(cell);

            cell.Elevation -= 1;
            erosionRunoffTargetCell.Elevation += 1;

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

    private void GenerateClimate(RootGenConfig config, HexGrid grid) {
        _climate.Clear();
        _nextClimate.Clear();

        ClimateData initialData = new ClimateData();
        initialData.moisture = config.startingMoisture;

        ClimateData clearData = new ClimateData();

        for (int i = 0; i < _cellCount; i++) {
            _climate.Add(initialData);
            _nextClimate.Add(clearData);
        }

        for (int cycle = 0; cycle < 40; cycle++) {
            for (int i = 0; i < _cellCount; i++) {
                StepClimate(config, grid, i);
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

    private void StepClimate(RootGenConfig config, HexGrid grid, int cellIndex) {
        HexCell cell = grid.GetCell(cellIndex);
        ClimateData cellClimate = _climate[cellIndex];

        if (cell.IsUnderwater) {
            cellClimate.moisture = 1f;
            cellClimate.clouds += config.evaporationFactor;
        }
        else {
            float evaporation = cellClimate.moisture * config.evaporationFactor;
            cellClimate.moisture -= evaporation;
            cellClimate.clouds += evaporation;
        }

        float precipitation = cellClimate.clouds * config.precipitationFactor;
        cellClimate.clouds -= precipitation;
        cellClimate.moisture += precipitation;

        // Cloud maximum has an inverse relationship with elevation maximum.
        float cloudMaximum = 1f - cell.ViewElevation / (config.elevationMax + 1f);

        if (cellClimate.clouds > cloudMaximum) {
            cellClimate.moisture += cellClimate.clouds - cloudMaximum;
            cellClimate.clouds = cloudMaximum;
        }

        HexDirection mainDispersalDirection = config.windDirection.Opposite();

        float cloudDispersal = cellClimate.clouds * (1f / (5f + config.windStrength));
        float runoff = cellClimate.moisture * config.runoffFactor * (1f / 6f);
        float seepage = cellClimate.moisture * config.seepageFactor * (1f / 6f);

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
                neighborClimate.clouds += cloudDispersal * config.windStrength;
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
        RootGenConfig config,
        HexGrid grid,
        int numLandCells
    ) {
        List<HexCell> riverOrigins = ListPool<HexCell>.Get();

        for (int i = 0; i < _cellCount; i++) {
            HexCell cell = grid.GetCell(i);

            if (cell.IsUnderwater) {
                continue;
            }

            ClimateData data = _climate[i];
            float weight =
                data.moisture * (cell.Elevation - config.waterLevel) /
                (config.elevationMax - config.waterLevel);

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

        int riverBudget = Mathf.RoundToInt(numLandCells * config.riverPercentage * 0.01f);

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
                    riverBudget -= GenerateRiver(config, origin);
                }
            }
        }

        if (riverBudget > 0) {
            Debug.LogWarning("Failed to use up river budget.");
        }

        ListPool<HexCell>.Add(riverOrigins);
    }

    private int GenerateRiver(RootGenConfig config, HexCell origin) {
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
                        cell.Elevation = minNeighborElevation - 1;
                    }
                }

                break;
            }

            direction = _flowDirections[Random.Range(0, _flowDirections.Count)];
            cell.SetOutgoingRiver(direction);
            length += 1;

            if (
                minNeighborElevation >= cell.Elevation &&
                Random.value < config.extraLakeProbability
            ) {
                cell.WaterLevel = cell.Elevation;
                cell.Elevation -= 1;
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

    private int RaiseTerrain(
        RootGenConfig config, 
        HexGrid grid, 
        int chunkSize, 
        int budget, 
        MapRegion region
    ) {
        _searchFrontierPhase += 1;
        HexCell firstCell = GetRandomCell(grid, region);
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        HexCoordinates center = firstCell.Coordinates;

        int rise = Random.value < config.highRiseProbability ? 2 : 1;
        int size = 0;

        while (size < chunkSize && _searchFrontier.Count > 0) {
            HexCell current = _searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;

            if (newElevation > config.elevationMax) {
                continue;
            }

            current.Elevation = newElevation;

            if (
                originalElevation < config.waterLevel &&
                newElevation >= config.waterLevel &&
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
                        Random.value < config.jitterProbability ? 1 : 0;

                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();

        return budget;
    }

    int SinkTerrain(
        RootGenConfig config,
        HexGrid grid,
        int chunkSize,
        int budget,
        MapRegion region
    ) {
        _searchFrontierPhase += 1;
        HexCell firstCell = GetRandomCell(grid, region);
        firstCell.SearchPhase = _searchFrontierPhase;
        firstCell.Distance = 0;
        firstCell.SearchHeuristic = 0;
        _searchFrontier.Enqueue(firstCell);

        HexCoordinates center = firstCell.Coordinates;

        int sink = Random.value < config.highRiseProbability ? 2 : 1;
        int size = 0;

        while (size < chunkSize && _searchFrontier.Count > 0) {
            HexCell current = _searchFrontier.Dequeue();
            int originalElevation = current.Elevation;

            int newElevation = current.Elevation - sink;

            if (newElevation < config.elevationMin) {
                continue;
            }

            current.Elevation = newElevation;

            if (
                originalElevation >= config.waterLevel &&
                newElevation < config.waterLevel
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
                        Random.value < config.jitterProbability ? 1 : 0;
                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();
        return budget;
    }

    private void SetTerrainTypes(RootGenConfig config, HexGrid grid) {
        _temperatureJitterChannel = Random.Range(0, 4);
        int rockDesertElevation =
            config.elevationMax - (config.elevationMax - config.waterLevel) / 2;

        for (int i = 0; i < _cellCount; i++) {
            HexCell cell = grid.GetCell(i);
            float temperature = GenerateTemperature(config, grid, cell);
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
                else if (cell.Elevation == config.elevationMax) {
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

                if (cell.Elevation == config.waterLevel - 1) {
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
                else if (cell.Elevation >= config.waterLevel) {
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
        RootGenConfig config, 
        HexGrid grid, 
        HexCell cell
    ) {
        float latitude = (float)cell.Coordinates.Z / grid.CellCountZ;

        if (config.hemisphere == HemisphereMode.Both) {
            latitude *= 2f;

            if (latitude > 1f) {
                latitude = 2f - latitude;
            }
        }
        else if (config.hemisphere == HemisphereMode.North) {
            latitude = 1f - latitude;
        }

        float temperature =
            Mathf.LerpUnclamped(
                config.lowTemperature,
                config.highTemperature,
                latitude
            );

        temperature *= 
            1f - 
            (cell.ViewElevation - config.waterLevel) /
            (config.elevationMax - config.waterLevel + 1f);

        float jitter =
            HexMetrics.SampleNoise(cell.Position * 0.1f)[_temperatureJitterChannel];

        temperature += (jitter * 2f - 1f) * config.temperatureJitter;

        return temperature;
    }

    private HexCell GetRandomCell(HexGrid grid, MapRegion region)
    {
        return grid.GetCell(Random.Range(region.xMin, region.xMax), Random.Range(region.zMin, region.zMax));
    }

    private void VisualizeRiverOrigins(RootGenConfig config, HexGrid grid)
    {
        for (int i = 0; i < _cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);

            float data = _climate[i].moisture * (cell.Elevation - config.waterLevel) /
                            (config.elevationMax - config.waterLevel);

            if (data > 0.75f)
            {
                cell.SetMapData(1f);
            }
            else if (data > 0.5f)
            {
                cell.SetMapData(0.5f);
            }
            else if (data > 0.25f)
            {
                cell.SetMapData(0.25f);
            }
        }
    }

    private struct MapRegion {
        public int xMin;
        public int xMax;
        public int zMin;
        public int zMax;
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
