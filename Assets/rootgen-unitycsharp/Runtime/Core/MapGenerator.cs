using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using RootLogging;
using RootCollections;
using RootUtils.Randomization;
using QuikGraph.Algorithms.Search;
using QuikGraph.Algorithms.Observers;


/// <summary>
/// Class encapsulating the RootGen map generation algorithms.
/// </summary>
public class MapGenerator {

/// <summary>
/// A constant representing the required difference in distance
/// between a cell and one, some, or all of its neighbors in order
/// for that cell to be considered erodible.
/// <summary>
    private const int DELTA_ERODIBLE_THRESHOLD = 2;
/// <summary>
/// A queue containing all the cells on the A* search frontier.
/// </summary>
//    private PriorityQueue _searchFrontier;

/// <summary>
/// The current A* search phase.
/// </summary>
/// TODO:
///     This variable is reassigned unnecessarily.
//    private int _searchFrontierPhase;

/// <summary>
/// A collection of all the map regions.
/// </summary>
    private List<MapRegionRect> _regions;

/// <summary>
/// A collection of structs defining the current climate data.
/// </summary>
/// TODO: 
///     Indirect coupling between _climate and HexMap in GenerateClimate() and
///     StepClimate() requiring both the list of HexCells in HexMap and _climate
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
///     Indirect coupling between _nextClimate and HexMap in GenerateClimate() and
///     StepClimate() requiring both the list of HexCells in HexMap and _nextClimate
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
    private List<HexDirections> _flowDirections = new List<HexDirections>();

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

/// <summary>
/// Generate a HexMap using the standard RootGen algorithm.
/// </summary>
/// <param name="config">
///     The configuration data for the map to be generated.
/// </param>
/// <returns>
///     A randomly generated HexMap object.
/// </returns>
    public HexMap GenerateMap(
        RootGenConfig config,
        bool editMode
    ) {        
        HexMap result = HexMap.CreateHexMapGameObject();
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
            new Rect(0, 0, config.width, config.height),
            seed,
            config.cellSize,
            config.wrapping,
            editMode
        );

//        if (_searchFrontier == null) {
//            _searchFrontier = new PriorityQueue();
//        }

        foreach (HexCell cell in result.Cells) {
            cell.WaterLevel = config.waterLevel;
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
            result.Columns * result.Rows,
            config.sinkProbability,
            config.minimumRegionDensity,
            config.maximumRegionDensity,
            result,
            config.highRiseProbability,
            config.elevationMin,
            config.elevationMax,
            config.waterLevel,
            config.jitterProbability,
            config.cellSize,
            result.WrapSize
        );

        GenerateErosion(
            result,
            config.erosionPercentage,
            config.cellSize
        );

        GenerateClimate(
            result,
            config.startingMoisture,
            result.SizeSquared,
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
            config.cellSize
        );

        SetTerrainTypes(
            config.elevationMax,
            config.waterLevel,
            result.SizeSquared,
            config.hemisphere,
            config.temperatureJitter,
            config.lowTemperature,
            config.highTemperature,
            result,
            config.cellSize
        );

// Restore the snapshot of the random state taken before consuming the
// random sequence.
        Random.state = snapshot;

        return result;
    }

    private List<MapRegionRect> GenerateRegions(
        HexMap hexMap,
        int regionBorder,
        int mapBorderX,
        int mapBorderZ,
        int numRegions
    ) {
        return new List<MapRegionRect>(
            SubdivideRegions(
                hexMap.Columns,
                hexMap.Rows,
                mapBorderX,
                mapBorderZ,
                numRegions,
                regionBorder,
                hexMap.IsWrapping
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
        int landPercentage,
        int numCells,
        float sinkProbability,
        int chunkSizeMin,
        int chunkSizeMax, 
        HexMap hexMap,
        float highRiseProbability,
        int elevationMin,
        int elevationMax,
        int waterLevel,
        float jitterProbability,
        float cellOuterRadius,
        int wrapSize
    ) {
// Set the land budget to the fraction of the overall cells
// as specified by the percentLand arguement.
        int landBudget = Mathf.RoundToInt(
            numCells * landPercentage * 0.01f
        );

// Initialize the result;
        int result = landBudget;

// Guard against permutations that result in an impossible map
// and by extension an infinite loop by including a guard clause
// that aborts the loop at 10,000 attempts to sink or raise the
// terrain.
        for (int guard = 0; guard < 10000; guard++) {
            
// Determine whether this cell should be sunk            
            bool sink = Random.value < sinkProbability;

// For each region . . . 
            for (int i = 0; i < _regions.Count; i++) {
                
                MapRegionRect region = _regions[i];

// Get a chunk size to use within the bounds of the region based
// of the minimum and maximum chunk sizes.
                int maximumRegionDensity = Random.Range(
                    chunkSizeMin,
                    chunkSizeMax + 1
                );
                
// If cell is to be sunk, sink cell and decrement decrement land
// budget if sinking results in a cell below water level.
                if (sink) {
                    landBudget = SinkTerrain(
                        hexMap,
                        maximumRegionDensity,
                        landBudget,
                        region,
                        highRiseProbability,
                        elevationMin,
                        waterLevel,
                        jitterProbability,
                        cellOuterRadius
                    );
                }

// Else, raise cell and increment land budget if raising results in
// a cell above the water level.
                else {
                    landBudget = RaiseTerrain(
                        hexMap,
                        maximumRegionDensity,
                        landBudget,
                        region,
                        highRiseProbability,
                        elevationMax,
                        waterLevel,
                        jitterProbability,
                        cellOuterRadius,
                        wrapSize
                    );

// If land budget is 0, return initial land budget value because all
// land cells specified to be allocated were allocated successfully. 
                    if (landBudget == 0) {
                        return result;
                    }
                }
            }
        }

// If land budget is greater than 0, all land cells specified to be
// allocated were not allocated successfully. Log a warning, decrement
// the remaining land budget from the result, and return the result
// as the number of land cells allocated.
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
        HexMap hexMap,
        int maximumRegionDensity,
        int landBudget,
        MapRegionRect region,
        float highRiseProbability,
        int elevationMin,
        int waterLevel,
        float jitterProbability,
//        int searchFrontierPhase,
        float cellOuterRadius
    ) {
        PriorityQueue<HexCell> open = new PriorityQueue<HexCell>();
        List<HexCell> closed = new List<HexCell>();
// Increment the search frontier phase to indicate a new search phase.
//        searchFrontierPhase += 1;
        
// Region dimensions are used to calcualte valid bounds for randomly
// selecting first cell to apply the sink algorithm to. This results
// in continent like formations loosely constrained by the size of
// a given region.
// TODO:
//  This algorithm could probably be improved by finding a real-world
//  model to implement for sinking terrain based on tectonic shift.

// Get a random cell within the region bounds to be the first cell
// searched.
        HexCell firstCell = GetRandomCell(hexMap, region);

// Set the search phase of the first cell to the current search phase.
//        firstCell.SearchPhase = _searchFrontierPhase;

// Initialize the distance of the selected cell to 0, since it is
// 0 cells away from the start of the search.
//        firstCell.Distance = 0;

// Search heuristic for first cell is 0 as it has a high search
// priority. Although, it doesnt really matter because it is the
// first to be dequeued.
//        firstCell.SearchHeuristic = 0;

//        searchFrontier.Enqueue(firstCell);

        open.Enqueue(firstCell, 0);

        CubeVector center = firstCell.Coordinates;

        int sink = Random.value < highRiseProbability ? 2 : 1;
        int regionDensity = 0;

//        while (size < chunkSize && searchFrontier > 0) {
            while (
              regionDensity < maximumRegionDensity &&
              open.Count > 0
            ) {
              
//            HexCell current = searchFrontier.Dequeue();
                HexCell current = open.Dequeue();
                closed.Add(current);
                
                int originalElevation = current.Elevation;

                int newElevation = current.Elevation - sink;

                if (newElevation < elevationMin) {
                    continue;
                }

                current.SetElevation(
                    newElevation,
                    cellOuterRadius,
                    hexMap.WrapSize
                );

                if (
                    originalElevation >= waterLevel &&
                    newElevation < waterLevel
                ) {
                    landBudget += 1;
                }

                regionDensity += 1;

    //            for (
    //                HexDirection direction = HexDirection.Northeast;
    //                direction <= HexDirection.Northwest;
    //                direction++
    //            ) {
                List<HexCell> neighbors;

                if (hexMap.TryGetNeighbors(current, out neighbors)) {
                    foreach(HexCell neighbor in neighbors) {
                        if (closed.Contains(neighbor))
                        continue;
    //                HexCell neighbor = current.GetNeighbor(direction);
                    
    //                if (
    //                    neighbor && neighbor.SearchPhase <
    //                    _searchFrontierPhase
    //                ) {
    //                    neighbor.SearchPhase = _searchFrontierPhase;

    /* Set the distance to be the distance from the center of the
    * raised terrain chunk, so that cells closer to the center are
    * prioritized when raising terrain.
    */
    //                    neighbor.Distance =
    //                        neighbor.Coordinates.DistanceTo(center);

    /* Set the search heuristic to 1 or 0 based on a configurable
    * jitter probability, to cause perturbation in the cells which
    * are selected to be raised. This will make the chunks generated
    * less uniform.
    */
    //                    neighbor.SearchHeuristic =
    //                        Random.value < jitterProbability ? 1 : 0;
                        
    //                    searchFrontier.Enqueue(neighbor);

                        int priority =
                            CubeVector.HexTileDistance(
                                neighbor.Coordinates,
                                center,
                                hexMap.WrapSize
                            ) +
    //                        neighbor.CubeCoordinates.DistanceTo(
    //                            center,
    //                            hexMap.WrapSize
    //                        ) +
                            Random.value < jitterProbability ? 1 : 0;

                        open.Enqueue(
                            neighbor,
                            priority
                        );
                    }
                }
            }
//        }

//        searchFrontier.Clear();
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
        float cellOuterRadius,
        int wrapSize
    ) {
//        _searchFrontierPhase += 1;

// Region dimensions are used to calcualte valid bounds for randomly
// selecting first cell to apply the raise algorithm to. This results
// in continent like formations loosely constrained by the size of
// a given region.
// TODO:
//  This algorithm could probably be improved by finding a real-world
//  model to implement for raising terrain based on tectonic shift.
        HexCell firstCell = GetRandomCell(hexMap, region);

        PriorityQueue<HexCell> open = new PriorityQueue<HexCell>();
        List<HexCell> closed = new List<HexCell>();

        open.Enqueue(firstCell, 0);

//        firstCell.SearchPhase = _searchFrontierPhase;
//        firstCell.Distance = 0;
//        firstCell.SearchHeuristic = 0;
//        _searchFrontier.Enqueue(firstCell);

        CubeVector center = firstCell.Coordinates;

        int rise = Random.value < highRiseProbability ? 2 : 1;
        int regionDensity = 0;

//        while (size < chunkSize && _searchFrontier.Count > 0) {
        while (
            regionDensity < maximumRegionDensity &&
            open.Count > 0
        ) {
            
            HexCell current = open.Dequeue();
            closed.Add(current);
            
            int originalElevation = current.Elevation;
            int newElevation = originalElevation + rise;

            if (newElevation > elevationMax) {
                continue;
            }

            current.SetElevation(
                newElevation,
                cellOuterRadius,
                hexMap.WrapSize
            );

            current.SetElevation(
                newElevation,
                cellOuterRadius,
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

//            for (
//                HexDirection direction = HexDirection.Northeast;
//                direction <= HexDirection.Northwest;
//                direction++
//            ) {
            List<HexCell> neighbors;

            if (hexMap.TryGetNeighbors(current, out neighbors)) {
                foreach(HexCell neighbor in neighbors) {
                    if (closed.Contains(neighbor))
                    continue;
//                HexCell neighbor = current.GetNeighbor(direction);

//                if (neighbor && neighbor.SearchPhase < _searchFrontierPhase) {
//                    neighbor.SearchPhase = _searchFrontierPhase;

                    /* Set the distance to be the distance from the center of the
                        * raised terrain chunk, so that cells closer to the center are
                        * prioritized when raising terrain.
                        */
//                    neighbor.Distance = neighbor.Coordinates.DistanceTo(center);

                    /* Set the search heuristic to 1 or 0 based on a configurable
                        * jitter probability, to cause perturbation in the cells which
                        * are selected to be raised. This will make the chunks generated
                        * less uniform.
                        */
//                    neighbor.SearchHeuristic = 
//                        Random.value < jitterProbability ? 1 : 0;

//                    _searchFrontier.Enqueue(neighbor);
                    int priority =
                        CubeVector.HexTileDistance(
                            neighbor.Coordinates,
                            center,
                            hexMap.WrapSize
                        ) +
//                        neighbor.CubeCoordinates.DistanceTo(
//                            center,
//                            hexMap.WrapSize
//                        ) +
                        Random.value < jitterProbability ? 1 : 0;

                    open.Enqueue(neighbor, priority);
                }
            }
        }

//        _searchFrontier.Clear();

        return budget;
    }

    private void GenerateErosion(
        HexMap hexMap,
        int erosionPercentage,
        float cellOuterRadius
    ) {
        List<HexCell> erodibleCells = ListPool<HexCell>.Get();

// For each cell in the hex map, check if the cell is erodible. If it is
// add it to the list of erodible cells.
        foreach (HexCell erosionCandidate in hexMap.Cells) {
            List<HexCell> erosionCandidateNeighbors;

            hexMap.TryGetNeighbors(
                erosionCandidate,
                out erosionCandidateNeighbors
            );

            if (
                IsErodible(
                    erosionCandidate,
                    erosionCandidateNeighbors
                )
            ) {
                erodibleCells.Add(erosionCandidate);
            }
        }

// Calculate the target number of cells to be eroded.
        int targetErodibleCount =
            (int)(
                erodibleCells.Count *
                (100 - erosionPercentage) *
                0.01f
            );

// While the number of cells erorded is less than the target number...
        while (erodibleCells.Count > targetErodibleCount) {

// Select a random cell from the erodible cells.
            int index = Random.Range(0, erodibleCells.Count);
            HexCell cell = erodibleCells[index];

// Get the candidates for erosion runoff for the selected cell.
            List<HexCell> cellNeighbors;

            if(
                hexMap.TryGetNeighbors(
                    cell,
                    out cellNeighbors
                )
            ) {
                HexCell targetCell =
                    GetErosionRunoffTarget(
                        cell,
                        cellNeighbors
                    );

//  Lower the elevation of the cell being eroded.
                cell.SetElevation(
                    cell.Elevation - 1,
                    cellOuterRadius,
                    hexMap.WrapSize
                );

// Raise the elevation of the cell selected for runoff.
                targetCell.SetElevation(
                    targetCell.Elevation + 1,
                    cellOuterRadius,
                    hexMap.WrapSize
                );

// If the cell is not erodible after this erosion step, remove
// it from the list of erodible cells.
                if (
                    !IsErodible(
                        cell,
                        cellNeighbors
                    )
                ) {
                    erodibleCells[index] =
                        erodibleCells[erodibleCells.Count - 1];

                    erodibleCells.RemoveAt(erodibleCells.Count - 1);
                }
                
// For each neighbor of the current cell...
                foreach(HexCell neighbor in cellNeighbors) {
// If the elevation of the cells neighbor is is exactly 2 steps
// higher than the current cell, and is not an erodible cell...
                    if (
                        neighbor.Elevation == cell.Elevation + 2 &&
                        !erodibleCells.Contains(neighbor)
                    ) {
// ...this erosion step has modified the map so that the cell is now
// erodible, so add it to the list of erodible cells.
                        erodibleCells.Add(neighbor);
                    }
                }

                List<HexCell> targetCellNeighbors;

// If the target of the runoff is now erodible due to the change in
// elevation, add it to the list of erodible cells.
                if (
                    hexMap.TryGetNeighbors(
                        targetCell,
                        out targetCellNeighbors
                    ) &&
                    IsErodible(targetCell, targetCellNeighbors) &&
                    !erodibleCells.Contains(targetCell)
                ) {
                    erodibleCells.Add(targetCell);

// If the neighbors of the targer cell are now not erodible due to the
// change in elevation, remove them from the list of erodible cells.
                    foreach (
                        HexCell targetCellNeighbor in
                        targetCellNeighbors
                    ) {
                        List<HexCell> targetCellNeighborNeighbors;

                        if (
                            hexMap.TryGetNeighbors(
                                targetCellNeighbor,
                                out targetCellNeighborNeighbors
                            ) &&
                            !IsErodible(
                                targetCellNeighbor,
                                targetCellNeighborNeighbors
                            )
                        ) {
                            erodibleCells.Remove(targetCellNeighbor);
                        }
                    }
                }
            }
        }

        ListPool<HexCell>.Add(erodibleCells);
    }

    private void GenerateClimate(
        HexMap HexMap,
        float startingMoisture,
        int numCells,
        float evaporationFactor,
        float precipitationFactor,
        int elevationMax,    
        HexDirections windDirection,
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
                    HexMap,
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
        HexMap hexMap,
        int cellIndex,
        float evaporationFactor,
        float precipitationFactor,
        int elevationMax,
        HexDirections windDirection,
        float windStrength,
        float runoffFactor,
        float seepageFactor
    ) {
        HexCell cell = hexMap.GetCell(cellIndex);
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

        HexDirections mainDispersalDirection = windDirection.Opposite();

        float cloudDispersal = cellClimate.clouds * (1f / (5f + windStrength));
        float runoff = cellClimate.moisture * runoffFactor * (1f / 6f);
        float seepage = cellClimate.moisture * seepageFactor * (1f / 6f);

//        for (
//            HexDirection direction = HexDirection.Northeast;
//            direction <= HexDirection.Northwest;
//            direction++
//        ) {

        foreach (HexEdge edge in hexMap.NeighborGraph.GetVertexEdges(cell)) {
//            HexCell neighbor = cell.GetNeighbor(direction);

//            if (!neighbor) {
//                continue;
//            }

            ClimateData neighborClimate = _climate[edge.Target.Index];

            if (edge.Direction == mainDispersalDirection) {
                neighborClimate.clouds += cloudDispersal * windStrength;
            }
            else {
                neighborClimate.clouds += cloudDispersal;
            }

            int elevationDelta = edge.Target.ViewElevation - cell.ViewElevation;

            if (elevationDelta < 0) {
                cellClimate.moisture -= runoff;
                neighborClimate.moisture += runoff;
            }
            else if (elevationDelta == 0) {
                cellClimate.moisture -= seepage;
                neighborClimate.moisture += seepage;
            }

            _climate[edge.Target.Index] = neighborClimate;
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
        HexMap hexMap,
        int numLandCells,
        int waterLevel,
        int elevationMax,
        int riverPercentage,
        float extraLakeProbability,
        float cellOuterRadius
    ) {

        RiverGraph riverGraph = hexMap.RiverGraph;

        List<HexCell> riverOrigins = ListPool<HexCell>.Get();

        for (int i = 0; i < numLandCells; i++) {
            HexCell cell = hexMap.GetCell(i);

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

//            if (!origin.HasRiver) {
            if (riverGraph.HasRiver(origin)) {
                bool isValidOrigin = true;
                
                List<HexCell> neighbors;
                if (hexMap.TryGetNeighbors(origin, out neighbors)) {
                    foreach(HexCell neighbor in neighbors) {
//                      HexCell neighbor =
//                      origin.GetNeighbor(direction);

                        if (
                            neighbor &&
                            (   
//                              neighbor.HasRiver ||
                                riverGraph.HasRiver(neighbor) ||
                                neighbor.IsUnderwater
                            )
                        ) {
                            isValidOrigin = false;
                            break;
                        }
                    }
                }

                if (isValidOrigin) {
                    riverBudget -= GenerateRiver(
                        hexMap,
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
        HexMap hexMap,
        HexCell origin,
        float extraLakeProbability,
        float cellOuterRadius
    ) {
        NeighborGraph neighborGraph = hexMap.NeighborGraph;
        RiverGraph riverGraph = hexMap.RiverGraph;

        int localRiverLength = 1;
        HexCell currentCell = origin;
        HexDirections direction = HexDirections.Northeast;

        while (!currentCell.IsUnderwater) {
            int minNeighborElevation = int.MaxValue;

            _flowDirections.Clear();

            for (
                HexDirections directionCandidate = HexDirections.Northeast;
                directionCandidate <= HexDirections.Northwest;
                directionCandidate++
            ) {
//            foreach(HexCell neighbor in neighborGraph.Neighbors(cell)) {
                HexCell neighbor =
//                    cell.GetNeighbor(directionCandidate);
                      neighborGraph.TryGetNeighborInDirection(
                          currentCell,
                          directionCandidate
                      );

                if (!neighbor) {
                    continue;
                }

                if (neighbor.Elevation < minNeighborElevation) {
                    minNeighborElevation = neighbor.Elevation;
                }

// If the direction points to the river origin, or to a neighbor
// which already has an incoming river, continue.
                if (
                    neighbor == origin ||
//                    neighbor.HasIncomingRiver
                    riverGraph.HasIncomingRiver(neighbor)
                ) {
                    continue;
                }

                int delta = neighbor.Elevation - currentCell.Elevation;

// If the elevation in the given direction is positive, continue.
                if (delta > 0) {
                    continue;
                }

// If the direction points away from the river origin and any
// neighbors which already have an incoming river, and the elevation
// in the given direction is negative or zero, and the neighbor
// has an outgoing river, branch river in this direction.
//                if (neighbor.HasOutgoingRiver) {
                if (riverGraph.HasOutgoingRiver(neighbor)) {
//                    cell.SetOutgoingRiver(directionCandidate);
                    RiverEdge mergeEdge = new RiverEdge(
                        currentCell,
                        neighbor,
                        directionCandidate
                    );

                    riverGraph.AddEdge(mergeEdge);
                    return localRiverLength;
                }

// If the direction points away from the river origin and any
// neighbors which already have an incoming river, and the elevation
// in the given direction is not positive, and the neighbor does
// not have an outgoing river in the given direction...

// If the direction is a decline, make the probability for the branch
// 4 / 5.
                if (delta < 0) {
                    _flowDirections.Add(directionCandidate);
                    _flowDirections.Add(directionCandidate);
                    _flowDirections.Add(directionCandidate);
                }

// If the rivers local length is 1, and the direction does not result
// in a slight river bend, but rather a straight river or a corner
// river, make the probability of the branch 2 / 5
                if (
                    localRiverLength == 1 ||
                    (directionCandidate != direction.NextClockwise2() &&
                    directionCandidate != direction.PreviousClockwise2())
                ) {
                    _flowDirections.Add(directionCandidate);
                }

                _flowDirections.Add(directionCandidate);
            }

// If there are no candidates for branching the river...
            if (_flowDirections.Count == 0) {
// If the river contains only the river origin...
                if (localRiverLength == 1) {
// Do nothing and return 0.
                    return 0;
                }

// If the cell is surrounded by cells at a higher elevation,
// set the water level of the cell to the minium elevation of
// all neighbors.
                if (minNeighborElevation >= currentCell.Elevation) {
                    currentCell.WaterLevel = minNeighborElevation;

// If the cell is of equal elevation to a neighbor with a minimum
// elevation, lower the current cells elevation to one below
// the minimum elevation of all of its neighbors so that it becomes
// a small lake that the river feeds into, and then break out of the
// while statement terminating the river in a lake rather than into
// the ocean.
                    if (minNeighborElevation == currentCell.Elevation) {
                        currentCell.SetElevation(
                            minNeighborElevation - 1,
                            cellOuterRadius,
                            hexMap.WrapSize
                        );
                    }
                }

                break;
            }

// If there are flow direction candidates, choose one at random
// based on the assigned probabilities and set an outgoing river
// in that direction.
            direction = _flowDirections[
                Random.Range(0, _flowDirections.Count)
            ];

//            cell.SetOutgoingRiver(direction);
            RiverEdge randomEdge = new RiverEdge(
                currentCell,
                neighborGraph.TryGetNeighborInDirection(
                    currentCell,
                    direction
                ),
                direction
            );

            riverGraph.AddEdge(randomEdge);

            localRiverLength += 1;

// If the cell is lower than the minimum elevation of its neighbors
// assign a lakes based on a specified probability.
            if (
                minNeighborElevation >= currentCell.Elevation &&
                Random.value < extraLakeProbability
            ) {
                currentCell.WaterLevel = currentCell.Elevation;
                currentCell.SetElevation(
                    currentCell.Elevation - 1,
                    cellOuterRadius,
                    hexMap.WrapSize
                );
            }
// Make the new current cell the cell which the river has branched
// into.
//            currentCell = currentCell.GetNeighbor(direction);
            currentCell = neighborGraph.TryGetNeighborInDirection(
                currentCell,
                direction
            );
        }

        return localRiverLength;
    }

/// <summary>
/// Gets a boolean value representing whether the specified cell is
/// erodible based on the state of its neighbors.
/// </summary>
/// <param name="candidate">
/// The provided cell.
/// </param>
/// <param name="candidateNeighbors">
/// The provided cells neighbors.
/// </param>
/// <returns>
/// A boolean value representing whether the specified cell is erodible
/// based on the state of its neighbors.
/// </returns>
    private bool IsErodible(
        HexCell candidate,
        List<HexCell> candidateNeighbors
    ) {
// For each neighbor of this cell...
        foreach (HexCell neighbor in candidateNeighbors) {

            if (
                neighbor &&
                (

// If the neighbors elevation is less than or equal to the erosion
// threshold value...
                    neighbor.Elevation <=
                    candidate.Elevation - DELTA_ERODIBLE_THRESHOLD
                )
            ) {
                return true;
            }
        }

        return false;
    }

    private HexCell GetErosionRunoffTarget(
        HexCell cell,
        List<HexCell> neighbors
    ) {
        List<HexCell> candidates = ListPool<HexCell>.Get();
        int erodibleElevation = cell.Elevation - 2;

//        for (
//            HexDirection direction = HexDirection.Northeast;
//            direction <= HexDirection.Northwest;
//            direction++
//        ) {
        foreach (HexCell neighbor in neighbors) {
//            HexCell neighbor = cell.GetNeighbor(direction);
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
        HexMap hexMap,
        float cellOuterRadius
    ) {
        RiverGraph riverGraph = hexMap.RiverGraph;

        _temperatureJitterChannel = Random.Range(0, 4);
        int rockDesertElevation =
            elevationMax - (elevationMax - waterLevel) / 2;

        for (int i = 0; i < numCells; i++) {
            HexCell cell = hexMap.GetCell(i);

            float temperature = GenerateTemperature(
                hexMap,
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

//                if (cellBiome.plant < 3 && cell.HasRiver) {
                if (cellBiome.plant < 3 && riverGraph.HasRiver(cell)) {
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
                    List<HexCell> neighbors;

                    if (hexMap.TryGetNeighbors(cell, out neighbors)) {
                        foreach (HexCell neighbor in neighbors) {
                            int delta =
                                neighbor.Elevation - cell.WaterLevel;

                            if (delta == 0) {
                                slopes += 1;
                            }
                            else if (delta > 0) {
                                cliffs += 1;
                            }
                        }
                    }

// More than half neighbors at same level.
// Inlet or lake, therefore terrain is grass.
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

// Coldest temperature band produces mud instead of
// grass.
//
                if (terrain == 1 && temperature < temperatureBands[0]) {
                    terrain = 2;
                }

                cell.TerrainTypeIndex = terrain;
            }
        }
    }

    private float GenerateTemperature(
        HexMap hexMap, 
        HexCell cell,
        HemisphereMode hemisphere,
        int waterLevel,
        int elevationMax,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        float cellOuterRadius
    ) {
        float latitude = (float)cell.Coordinates.Z / hexMap.Columns;

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
                cellOuterRadius,
                hexMap.WrapSize
            )[_temperatureJitterChannel];

        temperature += (jitter * 2f - 1f) * temperatureJitter;

        return temperature;
    }

    private HexCell GetRandomCell(HexMap hexMap, MapRegionRect region)
    {
        return hexMap.GetCell(
            Random.Range(region.OffsetXMin, region.OffsetXMax),
            Random.Range(region.OffsetZMin, region.OffsetZMax)
        );
    }

    private void VisualizeRiverOrigins(
        HexMap hexMap,
        int cellCount,
        int waterLevel,
        int elevationMax
    ) {
        for (int i = 0; i < cellCount; i++) {
            HexCell cell = hexMap.GetCell(i);

            float data = _climate[i].moisture * (cell.Elevation - waterLevel) /
                            (elevationMax - waterLevel);

            if (data > 0.75f) {
                cell.SetMapData(1f);
            }
            else if (data > 0.5f) {
                cell.SetMapData(0.5f);
            }
            else if (data > 0.25f) {
                cell.SetMapData(0.25f);
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

    

