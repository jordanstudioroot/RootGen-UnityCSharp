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
    #region Constant Fields

    /// <summary>
    /// A constant representing the required difference in distance
    /// between a hex and one, some, or all of its neighbors in order
    /// for that hex to be considered erodible.
    /// <summary>
    private const int DELTA_ERODIBLE_THRESHOLD = 2;
    
    #endregion

    #region Fields

    #region Constructors
    #endregion;

    #region Private Fields
    /// <summary>
    ///     An array of floats representing thresholds for different temperature bands.
    ///     Used along with moisture bands to determine the index of biomes to be used
    ///     for the biome of a specific hex.
    /// </summary>
    private static float[] temperatureBands = { 0.1f, 0.3f, 0.6f };

    /// <summary>
    ///     An array of floats representing thresholds for different moisture bands.
    ///     Used along with moisture bands to determine the index of biomes to be
    ///     used for the biome of a specific hex.
    /// </summary>
    private static float[] moistureBands = { 0.12f, 0.28f, 0.85f };

    /// <summary>
    ///     An array of Biome structs representing a matrix of possible biomes
    ///     for a particular hex, indexed by its temperature and moisture.
    /// </summary>
    private static Biome[] biomes = {
        // Temperature <= .1 Freezing
        new Biome(Terrains.Desert, 0),   // Moisture <= .12 Freez. Dry
        new Biome(Terrains.Snow, 0),     //          <= .28 Freez. Moist
        new Biome(Terrains.Snow, 0),     //          <= .85 Freez. Wet
        new Biome(Terrains.Snow, 0),     //           > .85 Freez. Drenched 
        
        // Temperature <= .3 Cold
        new Biome(Terrains.Desert, 0),   // Moisture <= .12 Cold Dry
        new Biome(Terrains.Mud, 0),      //          <= .28 Cold Moist
        new Biome(Terrains.Mud, 1),      //          <= .85 Cold Wet
        new Biome(Terrains.Mud, 2),      //           > .85 Cold Drenched

        // Temperature <= .6 Warm
        new Biome(Terrains.Desert, 0),   // Moisture <= .12 Warm Dry
        new Biome(Terrains.Grassland, 0),// Moisture <= .28 Warm Moist
        new Biome(Terrains.Grassland, 1),// Moisture <= .85 Warm Wet
        new Biome(Terrains.Grassland, 2),// Moisture  > .85 Warm Drenched

        // Temperature > .6 Hot
        new Biome(Terrains.Desert, 0),   // Moisture <= .12 Hot Dry
        new Biome(Terrains.Grassland, 1),// Moisture <= .28 Hot Moist
        new Biome(Terrains.Grassland, 2),// Moisture <= .85 Hot Wet
        new Biome(Terrains.Grassland, 3) // Moisture  > .85 Hot Drenched
    };
    #endregion

    #endregion

    #region Methods

    #region Public Methods

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

        // Snapshot the initial random state before consuming the random
        // sequence.
        Random.State snapshot = RandomState.Snapshot(seed);

        result.Initialize(
            new Rect(0, 0, config.width, config.height),
            seed,
            config.hexSize,
            config.wrapping,
            editMode
        );

        foreach (Hex hex in result.Hexes) {
            hex.WaterLevel = config.waterLevel;
        }

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        List<MapRegionRect> regions = GenerateRegions(
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
            result.WrapSize,
            regions
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

        HexMapClimate hexMapClimate = new HexMapClimate(
            result,
            config.startingMoisture
        );

        
        ClimateParameters climateParameters = new ClimateParameters(
            config.hemisphere,
            config.windDirection,
            config.evaporationFactor,
            config.highTemperature,
            config.lowTemperature,
            config.precipitationFactor,
            config.runoffFactor,
            config.seepageFactor,
            config.temperatureJitter,
            config.windStrength,
            config.hexSize,
            config.elevationMax,
            config.waterLevel
        );

        for (int i = 0; i < config.initialClimateSteps; i++) {
            hexMapClimate.Step(climateParameters);
        }

        List<ClimateData> climate = hexMapClimate.List;
        
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
            config.hexSize,
            climate
        );

        stopwatch.Stop();
        diagnostics += "GenerateRivers: " + stopwatch.Elapsed + "\n";

        stopwatch.Start();

        SetTerrainTypes(
            config.elevationMax,
            config.waterLevel,
            config.hemisphere,
            config.temperatureJitter,
            config.lowTemperature,
            config.highTemperature,
            result,
            config.hexSize,
            climate
        );

        stopwatch.Stop();
        diagnostics += "SetTerrainTypes: " + stopwatch.Elapsed + "\n";

        RootLog.Log(
            diagnostics,
            Severity.Information,
            "Diagonstics"
        );

        // Restore the snapshot of the random state taken before consuming
        // the random sequence.
        Random.state = snapshot;
        return result;
    }

    #endregion

    #region Private Methods

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
        int wrapSize,
        List<MapRegionRect> regions
    ) {
        // Set the land budget to the fraction of the overall hexes as
        // specified by the percentLand arguement.
        int landBudget = Mathf.RoundToInt(
            numHexes * landPercentage * 0.01f
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

                // Else, raise hex and increment land budget if raising
                // results in a hex above the water level.
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
        HexMap hexMap,
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
        Hex firstHex = GetRandomHex(hexMap, region);
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
                hexMap.WrapSize
            );

            if (
                originalElevation >= waterLevel &&
                newElevation < waterLevel
            ) {
                landBudget += 1;
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
        Hex firstHex = GetRandomHex(hexMap, region);

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

    private void GenerateErosion(
        HexMap hexMap,
        int erosionPercentage,
        float hexOuterRadius
    ) {
        List<Hex> erodibleHexes = ListPool<Hex>.Get();

        // For each hex in the hex map, check if the hex is erodible.
        // If it is add it to the list of erodible hexes.
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

        // Calculate the target number of uneroded hexes.
        int targetUnerodedHexes =
            (int)(
                erodibleHexes.Count *
                (100 - erosionPercentage) *
                0.01f
            );

        // While the number of erodible hexes is greater than the target
        // number of uneroded hexes...
        while (erodibleHexes.Count > targetUnerodedHexes) {

            // Select a random hex from the erodible hexes.
            int index = Random.Range(0, erodibleHexes.Count);
            Hex originHex = erodibleHexes[index];

            // Get the candidates for erosion runoff for the selected hex.
            List<Hex> originNeighborHexes;

            if(
                hexMap.TryGetNeighbors(
                    originHex,
                    out originNeighborHexes
                )
            ) {
                Hex runoffHex =
                    GetErosionRunoffTarget(
                        originHex,
                        originNeighborHexes
                    );

                // Lower the elevation of the hex being eroded.
                originHex.SetElevation(
                    originHex.elevation - 1,
                    hexOuterRadius,
                    hexMap.WrapSize
                );

                // Raise the elevation of the hex selected for runoff.
                runoffHex.SetElevation(
                    runoffHex.elevation + 1,
                    hexOuterRadius,
                    hexMap.WrapSize
                );

                // If the hex is not erodible after this erosion step,
                // remove it from the list of erodible hexes.
                if (
                    !IsErodible(
                        originHex,
                        originNeighborHexes
                    )
                ) {
                    erodibleHexes[index] =
                        erodibleHexes[erodibleHexes.Count - 1];

                    erodibleHexes.RemoveAt(erodibleHexes.Count - 1);
                }
                
                // For each neighbor of the current hex...
                foreach(Hex originNeighbor in originNeighborHexes) {
                    // If the elevation of the hexes neighbor is is exactly
                    // 2 steps higher than the current hex, and is not an
                    // erodible hex...
                    if (
                        (
                            originNeighbor.elevation ==
                            originHex.elevation + DELTA_ERODIBLE_THRESHOLD
                        ) &&
                        !erodibleHexes.Contains(originNeighbor)
                    ) {
                        // ...this erosion step has modified the map so
                        // that the hex is now erodible, so add it to the
                        // list of erodible hexes.
                        erodibleHexes.Add(originNeighbor);
                    }
                }

                List<Hex> runoffNeighborHexes;

                // If the target of the runoff is now erodible due to the
                // change in elevation, add it to the list of erodible
                // hexes.
                if (
                    hexMap.TryGetNeighbors(
                        runoffHex,
                        out runoffNeighborHexes
                    )
                ) {
                    if (
                        IsErodible(
                            runoffHex,
                            runoffNeighborHexes
                        ) && 
                        !erodibleHexes.Contains(runoffHex)
                    ) {
                        erodibleHexes.Add(runoffHex);
                    }

                    foreach (
                        Hex runoffNeighbor in
                        runoffNeighborHexes
                    ) {
                        List<Hex> runoffNeighborNeighborHexes;

                        if (
                            hexMap.TryGetNeighbors(
                                runoffNeighbor,
                                out runoffNeighborNeighborHexes
                            ) &&
                            runoffNeighbor != originHex &&
                            (
                                runoffNeighbor.elevation ==
                                runoffHex.elevation + 1 
                            ) &&
                            !IsErodible(
                                runoffNeighbor,
                                runoffNeighborNeighborHexes
                            )
                        ) {
                            erodibleHexes.Remove(runoffNeighbor);
                        }   
                    }
                }
            }
        }

        ListPool<Hex>.Add(erodibleHexes);
    }

    private void GenerateRivers(
        HexMap hexMap,
        int numLandHexes,
        int waterLevel,
        int elevationMax,
        int riverPercentage,
        float extraLakeProbability,
        float hexOuterRadius,
        List<ClimateData> climate
    ) {

        RiverDigraph riverGraph =
            hexMap.RiverDigraph = 
            new RiverDigraph();

        HexAdjacencyGraph adjacencyGraph =
            hexMap.AdjacencyGraph;

        List<Hex> riverOrigins = ListPool<Hex>.Get();

        for (int i = 0; i < numLandHexes; i++) {
            Hex hex = hexMap.GetHex(i);

            if (hex.IsUnderwater) {
                continue;
            }

            ClimateData data = climate[i];
            float weight =
                data.moisture * (hex.elevation - waterLevel) /
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

            if (!riverGraph.HasRiver(origin)) {
                bool isValidOrigin = true;
                
                List<Hex> neighbors;
                if (
                    hexMap.TryGetNeighbors(origin, out neighbors)
                ) {
                    foreach(Hex neighbor in neighbors) {
                        if (
                            neighbor &&
                            (   
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
                        ref riverGraph,
                        adjacencyGraph
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
        ref RiverDigraph riverGraph,
        HexAdjacencyGraph adjacencyGraph
    ) {
        int localRiverLength = 1;
        Hex currentHex = origin;
        HexDirections direction = HexDirections.Northeast;

        while (!currentHex.IsUnderwater) {
            int minNeighborElevation = int.MaxValue;

            List<HexDirections> flowDirections =
                new List<HexDirections>();

            for (
                HexDirections directionCandidate = HexDirections.Northeast;
                directionCandidate <= HexDirections.Northwest;
                directionCandidate++
            ) {
                Hex neighbor =
                      adjacencyGraph.TryGetNeighborInDirection(
                          currentHex,
                          directionCandidate
                      );

                if (!neighbor) {
                    continue;
                }

                if (neighbor.elevation < minNeighborElevation) {
                    minNeighborElevation = neighbor.elevation;
                }

                // If the direction points to the river origin, or to a
                // neighbor which already has an incoming river, continue.
                if (
                    neighbor == origin ||
                    riverGraph.HasIncomingRiver(neighbor)
                ) {
                    continue;
                }

                int delta = neighbor.elevation - currentHex.elevation;

                // If the elevation in the given direction is positive,
                // continue.
                if (delta > 0) {
                    continue;
                }

                // If the direction points away from the river origin and
                // any neighbors which already have an incoming river, and
                // the elevation in the given direction is negative or
                // zero, and the neighbor has an outgoing river, branch
                // river in this direction.
                if (riverGraph.HasOutgoingRiver(neighbor)) {
                    RiverEdge mergeEdge = new RiverEdge(
                        currentHex,
                        neighbor,
                        directionCandidate
                    );

                    riverGraph.AddVerticesAndEdge(mergeEdge);
                    return localRiverLength;
                }

                // If the direction points away from the river origin and
                // any neighbors which already have an incoming river, and
                // the elevation in the given direction is not positive,
                // and the neighbor does not have an outgoing river in the
                // given direction...

                // If the direction is a decline, make the probability for
                // the branch 4 / 5.
                if (delta < 0) {
                    flowDirections.Add(directionCandidate);
                    flowDirections.Add(directionCandidate);
                    flowDirections.Add(directionCandidate);
                }

                // If the rivers local length is 1, and the direction does
                // not result in a slight river bend, but rather a straight
                // river or a corner river, make the probability of the
                // branch 2 / 5
                if (
                    localRiverLength == 1 ||
                    (directionCandidate != direction.NextClockwise2() &&
                    directionCandidate != direction.PreviousClockwise2())
                ) {
                    flowDirections.Add(directionCandidate);
                }

                flowDirections.Add(directionCandidate);
            }

            // If there are no candidates for branching the river...
            if (flowDirections.Count == 0) {
                // If the river contains only the river origin...
                if (localRiverLength == 1) {
                    // Do nothing and return 0.
                    return 0;
                }

                // If the hex is surrounded by hexes at a higher elevation,
                // set the water level of the hex to the minium elevation
                // of all neighbors.
                if (minNeighborElevation >= currentHex.elevation) {
                    currentHex.WaterLevel = minNeighborElevation;

                    // If the hex is of equal elevation to a neighbor with
                    // a minimum elevation, lower the current hexes
                    // elevation to one below the minimum elevation of all
                    // of its neighbors so that it becomes a small lake
                    // that the river feeds into, and then break out of the
                    // while statement terminating the river in a lake
                    // rather than into the ocean.
                    if (minNeighborElevation == currentHex.elevation) {
                        currentHex.SetElevation(
                            minNeighborElevation - 1,
                            hexOuterRadius,
                            hexMap.WrapSize
                        );
                    }
                }

                break;
            }

            // If there are flow direction candidates, choose one at
            // random based on the assigned probabilities and set an\
            // outgoing river in that direction.
            direction = flowDirections[
                Random.Range(0, flowDirections.Count)
            ];

            RiverEdge randomEdge = new RiverEdge(
                currentHex,
                adjacencyGraph.TryGetNeighborInDirection(
                    currentHex,
                    direction
                ),
                direction
            );

            riverGraph.AddVerticesAndEdge(randomEdge);
            localRiverLength += 1;

            // If the hex is lower than the minimum elevation of its
            // neighbors assign a lakes based on a specified probability.
            if (
                minNeighborElevation >= currentHex.elevation &&
                Random.value < extraLakeProbability
            ) {
                currentHex.WaterLevel = currentHex.elevation;
                currentHex.SetElevation(
                    currentHex.elevation - 1,
                    hexOuterRadius,
                    hexMap.WrapSize
                );
            }
            // Make the new current hex the hex which the river has
            // branched into.
            currentHex = adjacencyGraph.TryGetNeighborInDirection(
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

                    // If the neighbors elevation is less than or equal to
                    // the erosion threshold value...
                    neighbor.elevation <=
                    candidate.elevation - DELTA_ERODIBLE_THRESHOLD
                )
            ) {
                return true;
            }
        }

        return false;
    }

    private Hex GetErosionRunoffTarget(
        Hex origin,
        List<Hex> neighbors
    ) {
        List<Hex> candidates = ListPool<Hex>.Get();
        int erodibleElevation =
            origin.elevation - DELTA_ERODIBLE_THRESHOLD;

        foreach (Hex neighbor in neighbors) {
            if (neighbor && neighbor.elevation <= erodibleElevation) {
                candidates.Add(neighbor);
            }
        }

        Hex target = candidates[Random.Range(0, candidates.Count)];
            
        ListPool<Hex>.Add(candidates);
        return target;
    }

    private void SetTerrainTypes(
        int elevationMax,
        int waterLevel,
        HemisphereMode hemisphereMode,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        HexMap hexMap,
        float hexOuterRadius,
        List<ClimateData> climates
    ) {
        RiverDigraph riverGraph = hexMap.RiverDigraph;

        int temperatureJitterChannel = Random.Range(0, 4);

        int rockDesertElevation =
            elevationMax - (elevationMax - waterLevel) / 2;

        foreach (Hex hex in hexMap.Hexes) {
            float temperature = climates[hex.Index].temperature;

            float moisture = climates[hex.Index].moisture;

            if (!hex.IsUnderwater) {
                int temperatureBand = 0;

                for (
                    ;
                    temperatureBand < temperatureBands.Length;
                    temperatureBand++
                ) {
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

                if (hexBiome.terrain == Terrains.Desert) {
                    if (hex.elevation >= rockDesertElevation) {
                        hexBiome.terrain = Terrains.Stone;
                    }
                }
                else if (hex.elevation == elevationMax) {
                    hexBiome.terrain = Terrains.Snow;
                }

                if (hexBiome.terrain == Terrains.Snow) {
                    hexBiome.plant = 0;
                }

                if (hexBiome.plant < 3 && riverGraph.HasRiver(hex)) {
                    hexBiome.plant += 1;
                }

                hex.Biome = hexBiome;
                hex.ClimateData = climates[hex.Index];
            }
            else {
                Terrains terrain;

                if (hex.elevation == waterLevel - 1) {
                    int cliffs = 0;
                    int slopes = 0;
                    List<Hex> neighbors;

                    if (hexMap.TryGetNeighbors(hex, out neighbors)) {
                        foreach (Hex neighbor in neighbors) {
                            int delta =
                                neighbor.elevation - hex.WaterLevel;

                            if (delta == 0) {
                                slopes += 1;
                            }
                            else if (delta > 0) {
                                cliffs += 1;
                            }
                        }
                    }

                    // More than half neighbors at same level. Inlet or
                    // lake, therefore terrain is grass.
                    if (cliffs + slopes > 3) {
                        terrain = Terrains.Grassland;
                    }

                    // More than half cliffs, terrain is stone.
                    else if (cliffs > 0) {
                        terrain = Terrains.Stone;
                    }

                    // More than half slopes, terrain is beach.
                    else if (slopes > 0) {
                        terrain = Terrains.Desert;
                    }

                    // Shallow non-coast, terrain is grass.
                    else {
                        terrain = Terrains.Grassland;
                    }
                }
                else if (hex.elevation >= waterLevel) {
                    terrain = Terrains.Grassland;
                }
                else if (hex.elevation < 0) {
                    terrain = Terrains.Desert;
                }
                else {
                    terrain = Terrains.Mud;
                }

                // Coldest temperature band produces mud instead of grass.
                if (
                    terrain == Terrains.Grassland &&
                    temperature < temperatureBands[0]
                ) {
                    terrain = Terrains.Mud;
                }

                hex.Biome = new Biome(terrain, 0);
                hex.ClimateData = climates[hex.Index];
            }
        }
    }

    private void VisualizeRiverOrigins(
        HexMap hexMap,
        int hexCount,
        int waterLevel,
        int elevationMax,
        List<ClimateData> climate
    ) {
        for (int i = 0; i < hexCount; i++) {
            Hex hex = hexMap.GetHex(i);

            float data = climate[i].moisture * (hex.elevation - waterLevel) /
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

    #endregion

    #endregion

    

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
}

    

