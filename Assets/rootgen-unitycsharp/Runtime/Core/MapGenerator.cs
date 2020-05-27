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

    /// <summary>
    /// A constant representing the maximum simulation steps that
    /// HexMapTectonics may simulate in order to get the land percentage
    /// specified by the config before it aborts.
    /// </summary>
    private const int MAX_TECTONIC_STEPS = 10000;
    
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

        HexMapTectonics hexMapTectonics = new HexMapTectonics(
            result,
            config.regionBorder,
            config.mapBorderX,
            config.mapBorderZ,
            config.numRegions
        );

        int landBudget = Mathf.RoundToInt(
            result.SizeSquared *
            config.landPercentage *
            0.01f
        );

        int newLandHexes = 0;

        TectonicParameters tectonicParameters =
            new TectonicParameters(
                config.hexSize,
                config.highRiseProbability,
                config.jitterProbability,
                config.sinkProbability,
                config.elevationMax,
                config.elevationMin,
                config.waterLevel,
                landBudget,
                config.maximumRegionDensity,
                config.minimumRegionDensity
            );

        for (int i = 0; i < MAX_TECTONIC_STEPS; i++) {
            newLandHexes = hexMapTectonics.Step(
                newLandHexes,
                tectonicParameters
            );

            if (newLandHexes == landBudget)
                break;
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
        }

        stopwatch.Stop();
        diagnostics += "Generate Tectonics: " + stopwatch.Elapsed + "\n";

        stopwatch.Start();

        HexMapErosion erosion = new HexMapErosion(result);
        int erodibleHexes = erosion.ErodibleHexes.Count;

        // Calculate the target number of uneroded hexes.
        int targetUnerodedHexes =
            (int)(
                erodibleHexes *
                (100 - config.erosionPercentage) *
                0.01f
            );
        
        while (erosion.ErodibleHexes.Count > targetUnerodedHexes) {
            erosion.Step(
                config.hexSize
            );
        }
        

        /*GenerateErosion(
            result,
            config.erosionPercentage,
            config.hexSize
        );*/

        stopwatch.Stop();
        diagnostics += "Generate Erosion: " + stopwatch.Elapsed + "\n";


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

        List<ClimateData> climates = hexMapClimate.List;
        
        stopwatch.Stop();
        diagnostics += "Generate Climate: " + stopwatch.Elapsed + "\n";

        stopwatch.Start();

        /*GenerateRivers(
            result,
            newLandHexes,
            config.waterLevel,
            config.elevationMax,
            config.riverPercentage,
            config.extraLakeProbability,
            config.hexSize,
            climate
        );*/

        HexMapRivers hexMapRivers = new HexMapRivers(
            result,
            climates,
            config.waterLevel,
            config.elevationMax
        );

        for (int i = 0; i < config.numInitialRivers; i++)
            hexMapRivers.StartRiver();

        for (int i = 0; i < config.numInitialRiverSteps; i++)
            hexMapRivers.Step(
                config.waterLevel,
                config.elevationMax,
                config.extraLakeProbability,
                config.hexSize,
                climates
            );

        result.RiverDigraph = hexMapRivers.RiverDigraph;

        stopwatch.Stop();
        diagnostics += "Generate Rivers: " + stopwatch.Elapsed + "\n";

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
            climates,
            hexMapRivers.RiverDigraph
        );

        stopwatch.Stop();
        diagnostics += "Assign Terrain Types: " + stopwatch.Elapsed + "\n";

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

    private void SetTerrainTypes(
        int elevationMax,
        int waterLevel,
        HemisphereMode hemisphereMode,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        HexMap hexMap,
        float hexOuterRadius,
        List<ClimateData> climates,
        RiverDigraph riverDigraph
    ) {
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

                if (hexBiome.plant < 3 && riverDigraph.HasRiver(hex)) {
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
}

    

