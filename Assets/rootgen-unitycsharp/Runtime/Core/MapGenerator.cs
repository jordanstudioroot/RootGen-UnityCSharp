using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using RootCollections;
using RootLogging;
using RootUtils.Randomization;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// Class encapsulating the RootGen map generation algorithms.
/// </summary>
public class MapGenerator {

/// <summary>
/// A constant representing the required difference in distance
/// between a hex and one, some, or all of its neighbors in order
/// for that hex to be considered erodible.
/// <summary>
    private const int DELTA_ERODIBLE_THRESHOLD = 2;
/// <summary>
/// A queue containing all the hexes on the A* search frontier.
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
///     StepClimate() requiring both the list of hexs in HexMap and _climate
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
///     StepClimate() requiring both the list of hexs in HexMap and _nextClimate
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
///     for the biome of a specific hex.
/// </summary>
/// TODO:
///     Extract with other climate modeling data and algorithms into a separate
///     class.
    private static float[] temperatureBands = { 0.1f, 0.3f, 0.6f };

/// <summary>
///     An array of floats representing thresholds for different moisture bands.
///     Used along with moisture bands to determine the index of biomes to be
///     used for the biome of a specific hex.
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
///     for a particular hex, indexed by its temperature and moisture.
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
/// The configuration data for the map to be generated.
/// </param>
/// <returns>
///A randomly generated HexMap object.
/// </returns>
    public HexMap GenerateMap(
        RootGenConfig config,
        bool editMode
    ) {
        string diagnostics = "Generate Map Performance Diagnostics\n\n";

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
            config.hexSize,
            config.wrapping,
            editMode
        );

//        if (_searchFrontier == null) {
//            _searchFrontier = new PriorityQueue();
//        }

        foreach (Hex hex in result.Hexes) {
            hex.WaterLevel = config.waterLevel;
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        _regions = GenerateRegions(
            result,
            config.regionBorder,
            config.mapBorderX,
            config.mapBorderZ,
            config.numRegions
        );

        stopwatch.Stop();
        diagnostics += "Generate Regions: " + stopwatch.Elapsed + "\n";
        
        
        stopwatch.Start();

        int numLandHexes = GetNumLandHexes(
            config.landPercentage,
            result.HexOffsetColumns * result.HexOffsetRows,
            config.sinkProbability,
            config.minimumRegionDensity,
            config.maximumRegionDensity,
            result,
            config.highRiseProbability,
            config.elevationMin,
            config.elevationMax,
            config.waterLevel,
            config.jitterProbability,
            config.hexSize,
            result.WrapSize
        );

        stopwatch.Stop();
        diagnostics += "GetNumLandHexes: " + stopwatch.Elapsed + "\n";

        stopwatch.Start();

        GenerateErosion(
            result,
            config.erosionPercentage,
            config.hexSize
        );

        stopwatch.Stop();
        diagnostics += "GenerateErosion: " + stopwatch.Elapsed + "\n";


        stopwatch.Start();

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
        
        stopwatch.Stop();
        diagnostics += "Generate Climate: " + stopwatch.Elapsed + "\n";

        stopwatch.Start();

        GenerateRivers(
            result,
            numLandHexes,
            config.waterLevel,
            config.elevationMax,
            config.riverPercentage,
            config.extraLakeProbability,
            config.hexSize
        );

        stopwatch.Stop();
        diagnostics += "GenerateRivers: " + stopwatch.Elapsed + "\n";

        stopwatch.Start();

        SetTerrainTypes(
            config.elevationMax,
            config.waterLevel,
            result.SizeSquared,
            config.hemisphere,
            config.temperatureJitter,
            config.lowTemperature,
            config.highTemperature,
            result,
            config.hexSize
        );

        stopwatch.Stop();
        diagnostics += "SetTerrainTypes: " + stopwatch.Elapsed + "\n";

        RootLog.Log(
            diagnostics,
            Severity.Information,
            "Diagonstics"
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
                hexMap.HexOffsetColumns,
                hexMap.HexOffsetRows,
                mapBorderX,
                mapBorderZ,
                numRegions,
                regionBorder,
                hexMap.IsWrapping
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

        int borderX = wrapping ? regionBorder : mapBorderX;

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

    private int GetNumLandHexes(
        int landPercentage,
        int numHexes,
        float sinkProbability,
        int chunkSizeMin,
        int chunkSizeMax, 
        HexMap hexMap,
        float highRiseProbability,
        int elevationMin,
        int elevationMax,
        int waterLevel,
        float jitterProbability,
        float hexOuterRadius,
        int wrapSize
    ) {
// Set the land budget to the fraction of the overall hexes
// as specified by the percentLand arguement.
        int landBudget = Mathf.RoundToInt(
            numHexes * landPercentage * 0.01f
        );

// Initialize the result;
        int result = landBudget;

// Guard against permutations that result in an impossible map
// and by extension an infinite loop by including a guard clause
// that aborts the loop at 10,000 attempts to sink or raise the
// terrain.
        for (int guard = 0; guard < 10000; guard++) {
            
// Determine whether this hex should be sunk            
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
                
// If hex is to be sunk, sink hex and decrement decrement land
// budget if sinking results in a hex below water level.
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
                        hexOuterRadius
                    );
                }

// Else, raise hex and increment land budget if raising results in
// a hex above the water level.
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
                        hexOuterRadius,
                        wrapSize
                    );

// If land budget is 0, return initial land budget value because all
// land hexes specified to be allocated were allocated successfully. 
                    if (landBudget == 0) {
                        return result;
                    }
                }
            }
        }

// If land budget is greater than 0, all land hexes specified to be
// allocated were not allocated successfully. Log a warning, decrement
// the remaining land budget from the result, and return the result
// as the number of land hexes allocated.
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
        float hexOuterRadius
    ) {
        PriorityQueue<Hex> open = new PriorityQueue<Hex>();
        List<Hex> closed = new List<Hex>();
// Increment the search frontier phase to indicate a new search phase.
//        searchFrontierPhase += 1;
        
// Region dimensions are used to calcualte valid bounds for randomly
// selecting first hex to apply the sink algorithm to. This results
// in continent like formations loosely constrained by the size of
// a given region.
// TODO:
//  This algorithm could probably be improved by finding a real-world
//  model to implement for sinking terrain based on tectonic shift.

// Get a random hex within the region bounds to be the first hex
// searched.
        Hex firstHex = GetRandomHex(hexMap, region);

// Set the search phase of the first hex to the current search phase.
//        firstHex.SearchPhase = _searchFrontierPhase;

// Initialize the distance of the selected hex to 0, since it is
// 0 hexes away from the start of the search.
//        firstHex.Distance = 0;

// Search heuristic for first hex is 0 as it has a high search
// priority. Although, it doesnt really matter because it is the
// first to be dequeued.
//        firstHex.SearchHeuristic = 0;

//        searchFrontier.Enqueue(firstHex);

        open.Enqueue(firstHex, 0);

        CubeVector center = firstHex.Coordinates;

        int sink = Random.value < highRiseProbability ? 2 : 1;
        int regionDensity = 0;

//        while (size < chunkSize && searchFrontier > 0) {
            while (
              regionDensity < maximumRegionDensity &&
              open.Count > 0
            ) {
              
//            hex current = searchFrontier.Dequeue();
                Hex current = open.Dequeue();
                closed.Add(current);
                
                int originalElevation = current.Elevation;

                int newElevation = current.Elevation - sink;

                if (newElevation < elevationMin) {
                    continue;
                }

                current.SetElevation(
                    newElevation,
                    hexOuterRadius,
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
                List<Hex> neighbors;

                if (hexMap.TryGetNeighbors(current, out neighbors)) {
                    foreach(Hex neighbor in neighbors) {
                        if (closed.Contains(neighbor))
                        continue;
    //                hex neighbor = current.GetNeighbor(direction);
                    
    //                if (
    //                    neighbor && neighbor.SearchPhase <
    //                    _searchFrontierPhase
    //                ) {
    //                    neighbor.SearchPhase = _searchFrontierPhase;

    /* Set the distance to be the distance from the center of the
    * raised terrain chunk, so that hexes closer to the center are
    * prioritized when raising terrain.
    */
    //                    neighbor.Distance =
    //                        neighbor.Coordinates.DistanceTo(center);

    /* Set the search heuristic to 1 or 0 based on a configurable
    * jitter probability, to cause perturbation in the hexes which
    * are selected to be raised. This will make the chunks generated
    * less uniform.
    */
    //                    neighbor.SearchHeuristic =
    //                        Random.value < jitterProbability ? 1 : 0;
                        
    //                    searchFrontier.Enqueue(neighbor);

                        int priority =
                            CubeVector.WrappedHexTileDistance(
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
        float hexOuterRadius,
        int wrapSize
    ) {
//        _searchFrontierPhase += 1;

// Region dimensions are used to calcualte valid bounds for randomly
// selecting first hex to apply the raise algorithm to. This results
// in continent like formations loosely constrained by the size of
// a given region.
// TODO:
//  This algorithm could probably be improved by finding a real-world
//  model to implement for raising terrain based on tectonic shift.
        Hex firstHex = GetRandomHex(hexMap, region);

        PriorityQueue<Hex> open = new PriorityQueue<Hex>();
        List<Hex> closed = new List<Hex>();

        open.Enqueue(firstHex, 0);

//        firstHex.SearchPhase = _searchFrontierPhase;
//        firstHex.Distance = 0;
//        firstHex.SearchHeuristic = 0;
//        _searchFrontier.Enqueue(firstHex);

        CubeVector center = firstHex.Coordinates;

        int rise = Random.value < highRiseProbability ? 2 : 1;
        int regionDensity = 0;

//        while (size < chunkSize && _searchFrontier.Count > 0) {
        while (
            regionDensity < maximumRegionDensity &&
            open.Count > 0
        ) {
            
            Hex current = open.Dequeue();
            closed.Add(current);
            
            int originalElevation = current.Elevation;
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

//            for (
//                HexDirection direction = HexDirection.Northeast;
//                direction <= HexDirection.Northwest;
//                direction++
//            ) {
            List<Hex> neighbors;

            if (hexMap.TryGetNeighbors(current, out neighbors)) {
                foreach(Hex neighbor in neighbors) {
                    if (closed.Contains(neighbor))
                    continue;
//                hex neighbor = current.GetNeighbor(direction);

//                if (neighbor && neighbor.SearchPhase < _searchFrontierPhase) {
//                    neighbor.SearchPhase = _searchFrontierPhase;

                    /* Set the distance to be the distance from the center of the
                        * raised terrain chunk, so that hexes closer to the center are
                        * prioritized when raising terrain.
                        */
//                    neighbor.Distance = neighbor.Coordinates.DistanceTo(center);

                    /* Set the search heuristic to 1 or 0 based on a configurable
                        * jitter probability, to cause perturbation in the hexes which
                        * are selected to be raised. This will make the chunks generated
                        * less uniform.
                        */
//                    neighbor.SearchHeuristic = 
//                        Random.value < jitterProbability ? 1 : 0;

//                    _searchFrontier.Enqueue(neighbor);
                    int priority =
                        CubeVector.WrappedHexTileDistance(
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
        float hexOuterRadius
    ) {
        List<Hex> erodibleHexes = ListPool<Hex>.Get();

// For each hex in the hex map, check if the hex is erodible. If it is
// add it to the list of erodible hexes.
        foreach (Hex erosionCandidate in hexMap.Hexes) {
            List<Hex> erosionCandidateNeighbors;

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
                erodibleHexes.Add(erosionCandidate);
            }
        }

// Calculate the target number of hexes to be eroded.
        int targetErodibleCount =
            (int)(
                erodibleHexes.Count *
                (100 - erosionPercentage) *
                0.01f
            );

// While the number of hexes erorded is less than the target number...
        while (erodibleHexes.Count > targetErodibleCount) {

// Select a random hex from the erodible hexes.
            int index = Random.Range(0, erodibleHexes.Count);
            Hex hex = erodibleHexes[index];

// Get the candidates for erosion runoff for the selected hex.
            List<Hex> hexNeighbors;

            if(
                hexMap.TryGetNeighbors(
                    hex,
                    out hexNeighbors
                ) &&
                IsErodible(hex, hexNeighbors)
            ) {
                Hex targetHex =
                    GetErosionRunoffTarget(
                        hex,
                        hexNeighbors
                    );

//  Lower the elevation of the hex being eroded.
                hex.SetElevation(
                    hex.Elevation - 1,
                    hexOuterRadius,
                    hexMap.WrapSize
                );

// Raise the elevation of the hex selected for runoff.
                targetHex.SetElevation(
                    targetHex.Elevation + 1,
                    hexOuterRadius,
                    hexMap.WrapSize
                );

// If the hex is not erodible after this erosion step, remove
// it from the list of erodible hexes.
                if (
                    !IsErodible(
                        hex,
                        hexNeighbors
                    )
                ) {
                    erodibleHexes[index] =
                        erodibleHexes[erodibleHexes.Count - 1];

                    erodibleHexes.RemoveAt(erodibleHexes.Count - 1);
                }
                
// For each neighbor of the current hex...
                foreach(Hex neighbor in hexNeighbors) {
// If the elevation of the hexes neighbor is is exactly 2 steps
// higher than the current hex, and is not an erodible hex...
                    if (
                        neighbor.Elevation == hex.Elevation + 2 &&
                        !erodibleHexes.Contains(neighbor)
                    ) {
// ...this erosion step has modified the map so that the hex is now
// erodible, so add it to the list of erodible hexes.
                        erodibleHexes.Add(neighbor);
                    }
                }

                List<Hex> targetHexNeighbors;

// If the target of the runoff is now erodible due to the change in
// elevation, add it to the list of erodible hexes.
                if (
                    hexMap.TryGetNeighbors(
                        targetHex,
                        out targetHexNeighbors
                    ) &&
                    IsErodible(targetHex, targetHexNeighbors) &&
                    !erodibleHexes.Contains(targetHex)
                ) {
                    erodibleHexes.Add(targetHex);

// If the neighbors of the targer hex are now not erodible due to the
// change in elevation, remove them from the list of erodible hexes.
                    foreach (
                        Hex targetHexNeighbor in
                        targetHexNeighbors
                    ) {
                        List<Hex> targetHexNeighborNeighbors;

                        if (
                            hexMap.TryGetNeighbors(
                                targetHexNeighbor,
                                out targetHexNeighborNeighbors
                            ) &&
                            !IsErodible(
                                targetHexNeighbor,
                                targetHexNeighborNeighbors
                            )
                        ) {
                            erodibleHexes.Remove(targetHexNeighbor);
                        }
                    }
                }
            }
        }

        ListPool<Hex>.Add(erodibleHexes);
    }

    private void GenerateClimate(
        HexMap hexMap,
        float startingMoisture,
        int numHexes,
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

        for (int i = 0; i < numHexes; i++) {
            _climate.Add(initialData);
            _nextClimate.Add(clearData);
        }
        HexAdjacencyGraph adjacencyGraph =
            hexMap.AdjacencyGraph;

        for (int cycle = 0; cycle < 40; cycle++) {
            for (int i = 0; i < numHexes; i++) {
                Hex source = hexMap.GetHex(i);
                List<HexEdge> edges =
                    adjacencyGraph.GetOutEdges(source);
                StepClimate(
                    hexMap.GetHex(i),
                    edges,
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
        Hex source,
        List<HexEdge> outEdges,
        int hexIndex,
        float evaporationFactor,
        float precipitationFactor,
        int elevationMax,
        HexDirections windDirection,
        float windStrength,
        float runoffFactor,
        float seepageFactor
    ) {
        ClimateData hexClimate = _climate[hexIndex];

        if (source.IsUnderwater) {
            hexClimate.moisture = 1f;
            hexClimate.clouds += evaporationFactor;
        }
        else {
            float evaporation = hexClimate.moisture * evaporationFactor;
            hexClimate.moisture -= evaporation;
            hexClimate.clouds += evaporation;
        }

        float precipitation = hexClimate.clouds * precipitationFactor;
        hexClimate.clouds -= precipitation;
        hexClimate.moisture += precipitation;

        // Cloud maximum has an inverse relationship with elevation maximum.
        float cloudMaximum = 1f - source.ViewElevation / (elevationMax + 1f);

        if (hexClimate.clouds > cloudMaximum) {
            hexClimate.moisture += hexClimate.clouds - cloudMaximum;
            hexClimate.clouds = cloudMaximum;
        }

        HexDirections mainDispersalDirection = windDirection.Opposite();

        float cloudDispersal = hexClimate.clouds * (1f / (5f + windStrength));
        float runoff = hexClimate.moisture * runoffFactor * (1f / 6f);
        float seepage = hexClimate.moisture * seepageFactor * (1f / 6f);

//        for (
//            HexDirection direction = HexDirection.Northeast;
//            direction <= HexDirection.Northwest;
//            direction++
//        ) {

        foreach (HexEdge edge in outEdges) {
//            hex neighbor = hex.GetNeighbor(direction);

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

            int elevationDelta = edge.Target.ViewElevation - source.ViewElevation;

            if (elevationDelta < 0) {
                hexClimate.moisture -= runoff;
                neighborClimate.moisture += runoff;
            }
            else if (elevationDelta == 0) {
                hexClimate.moisture -= seepage;
                neighborClimate.moisture += seepage;
            }

            _climate[edge.Target.Index] = neighborClimate;
        }

        // Create a hex for the next climate.
        ClimateData nextHexClimate = _nextClimate[hexIndex];

        // Modify the data for the next climate.
        nextHexClimate.moisture += hexClimate.moisture;

        /* Ensure that no hex can have more moisture than
            *a hex that is underwater.
            */
        if (nextHexClimate.moisture > 1f) {
            nextHexClimate.moisture = 1f;
        }

        //Store the data for the next climate.
        _nextClimate[hexIndex] = nextHexClimate;

        //Clear the current climate data.
        _climate[hexIndex] = new ClimateData();
    }

    private void GenerateRivers(
        HexMap hexMap,
        int numLandHexes,
        int waterLevel,
        int elevationMax,
        int riverPercentage,
        float extraLakeProbability,
        float hexOuterRadius
    ) {

        RiverDigraph riverGraph =
            hexMap.RiverDigraph = 
            new RiverDigraph();

        List<Hex> riverOrigins = ListPool<Hex>.Get();

        for (int i = 0; i < numLandHexes; i++) {
            Hex hex = hexMap.GetHex(i);

            if (hex.IsUnderwater) {
                continue;
            }

            ClimateData data = _climate[i];
            float weight =
                data.moisture * (hex.Elevation - waterLevel) /
                (elevationMax - waterLevel);

            if (weight > 0.75) {
                riverOrigins.Add(hex);
                riverOrigins.Add(hex);
            }

            if (weight > 0.5f) {
                riverOrigins.Add(hex);
            }

            if (weight > 0.25f) {
                riverOrigins.Add(hex);
            }
        }

        int riverBudget = Mathf.RoundToInt(
            numLandHexes *
            riverPercentage *
            0.01f
        );

        while (riverBudget > 0 && riverOrigins.Count > 0) {
            int index = Random.Range(0, riverOrigins.Count);
            int lastIndex = riverOrigins.Count - 1;
            Hex origin = riverOrigins[index];
            riverOrigins[index] = riverOrigins[lastIndex];
            riverOrigins.RemoveAt(lastIndex);

//            if (!origin.HasRiver) {
            if (!riverGraph.HasRiver(origin)) {
                bool isValidOrigin = true;
                
                List<Hex> neighbors;
                if (
                    hexMap.TryGetNeighbors(origin, out neighbors)
                ) {
                    foreach(Hex neighbor in neighbors) {
//                      hex neighbor =
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
                        hexOuterRadius,
                        ref riverGraph
                    );
                }
            }
        }

        if (riverBudget > 0) {
            Debug.LogWarning("Failed to use up river budget.");
        }

        ListPool<Hex>.Add(riverOrigins);
    }

    private int GenerateRiver(
        HexMap hexMap,
        Hex origin,
        float extraLakeProbability,
        float hexOuterRadius,
        ref RiverDigraph riverGraph
    ) {
        HexAdjacencyGraph neighborGraph = hexMap.AdjacencyGraph;

        int localRiverLength = 1;
        Hex currentHex = origin;
        HexDirections direction = HexDirections.Northeast;

        while (!currentHex.IsUnderwater) {
            int minNeighborElevation = int.MaxValue;

            _flowDirections.Clear();

            for (
                HexDirections directionCandidate = HexDirections.Northeast;
                directionCandidate <= HexDirections.Northwest;
                directionCandidate++
            ) {
//            foreach(hex neighbor in neighborGraph.Neighbors(hex)) {
                Hex neighbor =
//                    hex.GetNeighbor(directionCandidate);
                      neighborGraph.TryGetNeighborInDirection(
                          currentHex,
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

                int delta = neighbor.Elevation - currentHex.Elevation;

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
//                    hex.SetOutgoingRiver(directionCandidate);
                    RiverEdge mergeEdge = new RiverEdge(
                        currentHex,
                        neighbor,
                        directionCandidate
                    );

                    riverGraph.AddVerticesAndEdge(mergeEdge);
                    Debug.Log(mergeEdge);
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

// If the hex is surrounded by hexes at a higher elevation,
// set the water level of the hex to the minium elevation of
// all neighbors.
                if (minNeighborElevation >= currentHex.Elevation) {
                    currentHex.WaterLevel = minNeighborElevation;

// If the hex is of equal elevation to a neighbor with a minimum
// elevation, lower the current hexes elevation to one below
// the minimum elevation of all of its neighbors so that it becomes
// a small lake that the river feeds into, and then break out of the
// while statement terminating the river in a lake rather than into
// the ocean.
                    if (minNeighborElevation == currentHex.Elevation) {
                        currentHex.SetElevation(
                            minNeighborElevation - 1,
                            hexOuterRadius,
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

//            hex.SetOutgoingRiver(direction);
            RiverEdge randomEdge = new RiverEdge(
                currentHex,
                neighborGraph.TryGetNeighborInDirection(
                    currentHex,
                    direction
                ),
                direction
            );

            riverGraph.AddVerticesAndEdge(randomEdge);
            Debug.Log(randomEdge);

            localRiverLength += 1;

// If the hex is lower than the minimum elevation of its neighbors
// assign a lakes based on a specified probability.
            if (
                minNeighborElevation >= currentHex.Elevation &&
                Random.value < extraLakeProbability
            ) {
                currentHex.WaterLevel = currentHex.Elevation;
                currentHex.SetElevation(
                    currentHex.Elevation - 1,
                    hexOuterRadius,
                    hexMap.WrapSize
                );
            }
// Make the new current hex the hex which the river has branched
// into.
//            currentHex = currentHex.GetNeighbor(direction);
            currentHex = neighborGraph.TryGetNeighborInDirection(
                currentHex,
                direction
            );
        }

        return localRiverLength;
    }

/// <summary>
/// Gets a boolean value representing whether the specified hex is
/// erodible based on the state of its neighbors.
/// </summary>
/// <param name="candidate">
/// The provided hex.
/// </param>
/// <param name="candidateNeighbors">
/// The provided hexes neighbors.
/// </param>
/// <returns>
/// A boolean value representing whether the specified hex is erodible
/// based on the state of its neighbors.
/// </returns>
    private bool IsErodible(
        Hex candidate,
        List<Hex> candidateNeighbors
    ) {
// For each neighbor of this hex...
        foreach (Hex neighbor in candidateNeighbors) {

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

    private Hex GetErosionRunoffTarget(
        Hex hex,
        List<Hex> neighbors
    ) {
        List<Hex> candidates = ListPool<Hex>.Get();
        int erodibleElevation =
            hex.Elevation - DELTA_ERODIBLE_THRESHOLD;

//        for (
//            HexDirection direction = HexDirection.Northeast;
//            direction <= HexDirection.Northwest;
//            direction++
//        ) {
        foreach (Hex neighbor in neighbors) {
//            hex neighbor = hex.GetNeighbor(direction);
            if (neighbor && neighbor.Elevation <= erodibleElevation) {
                candidates.Add(neighbor);
            }
        }

        Hex target = null;

        if (candidates.Count != 0) {
            target = candidates[
                Random.Range(0, candidates.Count)
            ];
        }
            
        ListPool<Hex>.Add(candidates);
        return target;
    }

    private void SetTerrainTypes(
        int elevationMax,
        int waterLevel,
        int numHexes,
        HemisphereMode hemisphereMode,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        HexMap hexMap,
        float hexOuterRadius
    ) {
        RiverDigraph riverGraph = hexMap.RiverDigraph;

        _temperatureJitterChannel = Random.Range(0, 4);
        int rockDesertElevation =
            elevationMax - (elevationMax - waterLevel) / 2;

        for (int i = 0; i < numHexes; i++) {
            Hex hex = hexMap.GetHex(i);

            float temperature = GenerateTemperature(
                hexMap,
                hex,
                hemisphereMode,
                waterLevel,
                elevationMax,
                temperatureJitter,
                lowTemperature,
                highTemperature,
                hexOuterRadius
            );

            float moisture = _climate[i].moisture;

            if (!hex.IsUnderwater) {
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

                Biome hexBiome = biomes[temperatureBand * 4 + moistureBand];

                if (hexBiome.terrain == 0) {
                    if (hex.Elevation >= rockDesertElevation) {
                        hexBiome.terrain = 3;
                    }
                }
                else if (hex.Elevation == elevationMax) {
                    hexBiome.terrain = 4;
                }

                if (hexBiome.terrain == 4) {
                    hexBiome.plant = 0;
                }

//                if (hexBiome.plant < 3 && hex.HasRiver) {
                if (hexBiome.plant < 3 && riverGraph.HasRiver(hex)) {
                    hexBiome.plant += 1;
                }

                hex.terrainType = (TerrainTypes)hexBiome.terrain;
                hex.PlantLevel = hexBiome.plant;
            }
            else {
                int terrain;

                if (hex.Elevation == waterLevel - 1) {
                    int cliffs = 0;
                    int slopes = 0;
                    List<Hex> neighbors;

                    if (hexMap.TryGetNeighbors(hex, out neighbors)) {
                        foreach (Hex neighbor in neighbors) {
                            int delta =
                                neighbor.Elevation - hex.WaterLevel;

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
                else if (hex.Elevation >= waterLevel) {
                    terrain = 1;
                }
                else if (hex.Elevation < 0) {
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

                hex.terrainType = (TerrainTypes)terrain;
            }
        }
    }

    private float GenerateTemperature(
        HexMap hexMap, 
        Hex hex,
        HemisphereMode hemisphere,
        int waterLevel,
        int elevationMax,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        float hexOuterRadius
    ) {
        float latitude =
            (float)hex.Coordinates.Z / hexMap.HexOffsetRows;

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
            (hex.ViewElevation - waterLevel) /
            (elevationMax - waterLevel + 1f);

        float jitter =
            HexagonPoint.SampleNoise(
                hex.Position * 0.1f,
                hexOuterRadius,
                hexMap.WrapSize
            )[_temperatureJitterChannel];

        temperature += (jitter * 2f - 1f) * temperatureJitter;

        return temperature;
    }

    private Hex GetRandomHex(HexMap hexMap, MapRegionRect region)
    {
        return hexMap.GetHex(
            Random.Range(region.OffsetXMin, region.OffsetXMax),
            Random.Range(region.OffsetZMin, region.OffsetZMax)
        );
    }

    private void VisualizeRiverOrigins(
        HexMap hexMap,
        int hexCount,
        int waterLevel,
        int elevationMax
    ) {
        for (int i = 0; i < hexCount; i++) {
            Hex hex = hexMap.GetHex(i);

            float data = _climate[i].moisture * (hex.Elevation - waterLevel) /
                            (elevationMax - waterLevel);

            if (data > 0.75f) {
                hex.SetMapData(1f);
            }
            else if (data > 0.5f) {
                hex.SetMapData(0.5f);
            }
            else if (data > 0.25f) {
                hex.SetMapData(0.25f);
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
                    "Border cannot reduce z dimension below 3 or divison " +
                    "will be impossible. Setting border to 0.",
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

    

