using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using RootLogging;
using RootCollections;

public class MapGenerator
{
    private int _cellCount;
    private int _landCells;
    private CellPriorityQueue _searchFrontier;
    private int _searchFrontierPhase;

    private List<MapRegion> _regions;

    private List<ClimateData> _climate = new List<ClimateData>();
    private List<ClimateData> _nextClimate = new List<ClimateData>();
    private List<HexDirection> _flowDirections = new List<HexDirection>();
    private int _temperatureJitterChannel;

    private static float[] temperatureBands = { 0.1f, 0.3f, 0.6f };
    private static float[] moistureBands = { 0.12f, 0.28f, 0.85f };

    /* Array representing a matrix of biomes along the temperature bands:
        *
        *  0.1 [desert][snow][snow][snow]
        *  0.3 [desert][mud][mud, sparse flora][mud, average flora]
        *  0.6 [desert][grass][grass, sparse flora ][grass, average flora]
        *      [desert][grass, sparse flora][grass, average flora][grass, dense flora]
        *  Temperature/Moisture | 0.12 | 0.28 | 0.85
        * */
    private static Biome[] biomes = {
        new Biome(0, 0), new Biome(4, 0), new Biome(4, 0), new Biome(4, 0),
        new Biome(0, 0), new Biome(2, 0), new Biome(2, 1), new Biome(2, 2),
        new Biome(0, 0), new Biome(1, 0), new Biome(1, 1), new Biome(1, 2),
        new Biome(0, 0), new Biome(1, 1), new Biome(1, 2), new Biome(1, 3)
    };

    public HexGrid GenerateMap(
        IRootGenConfigData config
    ) {
//      Get a blank grid.
        HexGrid result = HexGrid.GetGrid();

        Random.State originalRandomState = Random.state;

        if (!config.UseFixedSeed) {
            config.Seed = Random.Range(0, int.MaxValue);
            config.Seed ^= (int)System.DateTime.Now.Ticks;
            config.Seed ^= (int)Time.time;
            config.Seed &= int.MaxValue;
        }

        Random.InitState(config.Seed);

        _cellCount = config.Width * config.Height;
        result.Initialize(config.Width, config.Height, config.Wrapping);

        if (_searchFrontier == null) {
            _searchFrontier = new CellPriorityQueue();
        }

        for (int i = 0; i < _cellCount; i++) {
            result.GetCell(i).WaterLevel = config.WaterLevel;
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

        Random.state = originalRandomState;
        return result;
    }

    private void GenerateRegions(IRootGenConfigData config, HexGrid grid) {
        if (_regions == null) {
            _regions = new List<MapRegion>();
        }
        else {
            _regions.Clear();
        }

        int borderX = grid.Wrapping ? config.RegionBorder : config.MapBorderX;

        MapRegion region;
        switch (config.RegionCount) {
            default:
                if (grid.Wrapping)
                {
                    borderX = 0;
                }

                region.xMin = borderX;
                region.xMax = grid.CellCountX - borderX;
                region.zMin = config.MapBorderZ;
                region.zMax = grid.CellCountZ - config.MapBorderZ;
                _regions.Add(region);
                break;
            case 2:
                if (Random.value < 0.5f)
                {
                    region.xMin = borderX;
                    region.xMax = grid.CellCountX / 2 - config.RegionBorder;
                    region.zMin = config.MapBorderZ;
                    region.zMax = grid.CellCountZ - config.MapBorderZ;
                    _regions.Add(region);
                    region.xMin = grid.CellCountX / 2 + config.RegionBorder;
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
                    region.zMin = config.MapBorderZ;
                    region.zMax = grid.CellCountZ / 2 - config.RegionBorder;
                    _regions.Add(region);
                    region.zMin = grid.CellCountZ / 2 + config.RegionBorder;
                    region.zMax = grid.CellCountZ - config.MapBorderZ;
                    _regions.Add(region);
                }
                break;
            case 3:
                region.xMin = borderX;
                region.xMax = grid.CellCountX / 3 - config.RegionBorder;
                region.zMin = config.MapBorderZ;
                region.zMax = grid.CellCountZ - config.MapBorderZ;
                _regions.Add(region);
                region.xMin = grid.CellCountX / 3 + config.RegionBorder;
                region.xMax = grid.CellCountX * 2 / 3 - config.RegionBorder;
                _regions.Add(region);
                region.xMin = grid.CellCountX * 2 / 3 + config.RegionBorder;
                region.xMax = grid.CellCountX - borderX;
                _regions.Add(region);
                break;
            case 4:
                region.xMin = borderX;
                region.xMax = grid.CellCountX / 2 - config.RegionBorder;
                region.zMin = config.MapBorderZ;
                region.zMax = grid.CellCountZ / 2 - config.RegionBorder;
                _regions.Add(region);
                region.xMin = grid.CellCountX / 2 + config.RegionBorder;
                region.xMax = grid.CellCountX - borderX;
                _regions.Add(region);
                region.zMin = grid.CellCountZ / 2 + config.RegionBorder;
                region.zMax = grid.CellCountZ - config.MapBorderZ;
                _regions.Add(region);
                region.xMin = borderX;
                region.xMax = grid.CellCountX / 2 - config.RegionBorder;
                _regions.Add(region);
                break;
        }
    }

    private int GetNumLandCells(IRootGenConfigData config, HexGrid grid) {
        int landBudget = Mathf.RoundToInt(
            _cellCount * config.LandPercentage * 0.01f
        );

        int result = landBudget;

        for (int guard = 0; guard < 10000; guard++) {
            bool sink = Random.value < config.SinkProbability;

            for (int i = 0; i < _regions.Count; i++) {
                MapRegion region = _regions[i];
                int chunkSize = Random.Range(config.ChunkSizeMin, config.ChunkSizeMax + 1);
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

    private void GenerateErosion(IRootGenConfigData config, HexGrid grid) {
        List<HexCell> erodibleCells = ListPool<HexCell>.Get();

        for (int i = 0; i < _cellCount; i++) {
            HexCell cell = grid.GetCell(i);

            if (IsErodible(cell)) {
                erodibleCells.Add(cell);
            }
        }

        int targetErodibleCount =
            (int)(erodibleCells.Count * (100 - config.ErosionPercentage) * 0.01f);

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

    private void GenerateClimate(IRootGenConfigData config, HexGrid grid) {
        _climate.Clear();
        _nextClimate.Clear();

        ClimateData initialData = new ClimateData();

        initialData.moisture = config.StartingMoisture;
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

    private void GenerateRivers(
        IRootGenConfigData config,
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
                data.moisture * (cell.Elevation - config.WaterLevel) /
                (config.ElevationMax - config.WaterLevel);

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

        int riverBudget = Mathf.RoundToInt(numLandCells * config.RiverPercentage * 0.01f);

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

    private int GenerateRiver(IRootGenConfigData config, HexCell origin) {
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
                Random.value < config.ExtraLakeProbability
            ) {
                cell.WaterLevel = cell.Elevation;
                cell.Elevation -= 1;
            }

            cell = cell.GetNeighbor(direction);
        }

        return length;
    }

    private void StepClimate(IRootGenConfigData config, HexGrid grid, int cellIndex) {
        HexCell cell = grid.GetCell(cellIndex);
        ClimateData cellClimate = _climate[cellIndex];

        if (cell.IsUnderwater) {
            cellClimate.moisture = 1f;
            cellClimate.clouds += config.EvaporationFactor;
        }
        else {
            float evaporation = cellClimate.moisture * config.EvaporationFactor;
            cellClimate.moisture -= evaporation;
            cellClimate.clouds += evaporation;
        }

        float precipitation = cellClimate.clouds * config.PrecipitationFactor;
        cellClimate.clouds -= precipitation;
        cellClimate.moisture += precipitation;

        // Cloud maximum has an inverse relationship with elevation maximum.
        float cloudMaximum = 1f - cell.ViewElevation / (config.ElevationMax + 1f);

        if (cellClimate.clouds > cloudMaximum) {
            cellClimate.moisture += cellClimate.clouds - cloudMaximum;
            cellClimate.clouds = cloudMaximum;
        }

        HexDirection mainDispersalDirection = config.WindDirection.Opposite();

        float cloudDispersal = cellClimate.clouds * (1f / (5f + config.WindStrength));
        float runoff = cellClimate.moisture * config.RunoffFactor * (1f / 6f);
        float seepage = cellClimate.moisture * config.SeepageFactor * (1f / 6f);

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
                neighborClimate.clouds += cloudDispersal * config.WindStrength;
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
        IRootGenConfigData config, 
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

        int rise = Random.value < config.HighRiseProbability ? 2 : 1;
        int size = 0;

        while (size < chunkSize && _searchFrontier.Count > 0) {
            HexCell current = _searchFrontier.Dequeue();
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;

            if (newElevation > config.ElevationMax) {
                continue;
            }

            current.Elevation = newElevation;

            if (
                originalElevation < config.WaterLevel &&
                newElevation >= config.WaterLevel &&
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
                        Random.value < config.JitterProbability ? 1 : 0;

                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();

        return budget;
    }

    int SinkTerrain(
        IRootGenConfigData config,
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

        int sink = Random.value < config.HighRiseProbability ? 2 : 1;
        int size = 0;

        while (size < chunkSize && _searchFrontier.Count > 0) {
            HexCell current = _searchFrontier.Dequeue();
            int originalElevation = current.Elevation;

            int newElevation = current.Elevation - sink;

            if (newElevation < config.ElevationMin) {
                continue;
            }

            current.Elevation = newElevation;

            if (
                originalElevation >= config.WaterLevel &&
                newElevation < config.WaterLevel
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
                        Random.value < config.JitterProbability ? 1 : 0;
                    _searchFrontier.Enqueue(neighbor);
                }
            }
        }

        _searchFrontier.Clear();
        return budget;
    }

    private void SetTerrainTypes(IRootGenConfigData config, HexGrid grid) {
        _temperatureJitterChannel = Random.Range(0, 4);
        int rockDesertElevation =
            config.ElevationMax - (config.ElevationMax - config.WaterLevel) / 2;

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
                else if (cell.Elevation == config.ElevationMax) {
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

                if (cell.Elevation == config.WaterLevel - 1) {
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
                else if (cell.Elevation >= config.WaterLevel) {
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

    private HexCell GetRandomCell(HexGrid grid, MapRegion region)
    {
        return grid.GetCell(Random.Range(region.xMin, region.xMax), Random.Range(region.zMin, region.zMax));
    }

    private void VisualizeRiverOrigins(IRootGenConfigData config, HexGrid grid)
    {
        for (int i = 0; i < _cellCount; i++)
        {
            HexCell cell = grid.GetCell(i);

            float data = _climate[i].moisture * (cell.Elevation - config.WaterLevel) /
                            (config.ElevationMax - config.WaterLevel);

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

    private float GenerateTemperature(
        IRootGenConfigData config, 
        HexGrid grid, 
        HexCell cell
    ) {
        float latitude = (float)cell.Coordinates.Z / grid.CellCountZ;

        if (config.Hemisphere == HemisphereMode.Both) {
            latitude *= 2f;

            if (latitude > 1f) {
                latitude = 2f - latitude;
            }
        }
        else if (config.Hemisphere == HemisphereMode.North) {
            latitude = 1f - latitude;
        }

        float temperature =
            Mathf.LerpUnclamped(
                config.LowTemperature,
                config.HighTemperature,
                latitude
            );

        temperature *= 
            1f - 
            (cell.ViewElevation - config.WaterLevel) /
            (config.ElevationMax - config.WaterLevel + 1f);

        float jitter =
            HexMetrics.SampleNoise(cell.Position * 0.1f)[_temperatureJitterChannel];

        temperature += (jitter * 2f - 1f) * config.TemperatureJitter;

        return temperature;
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
