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
    /// A constant representing the maximum simulation steps that
    /// HexMapTectonics may simulate in order to get the land percentage
    /// specified by the config before it aborts.
    /// </summary>
    private const int MAX_TECTONIC_STEPS = 10000;
    
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

        string logString = "Tectonic Statistics\n";
        for (int i = 0; i < MAX_TECTONIC_STEPS; i++) {
            tectonicParameters.LandBudget = hexMapTectonics.Step(
                tectonicParameters
            );

            logString +=
                "Step " + i + ", Land Hexes: " +
                result.LandHexes.Count + " / Land Budget: " +
                tectonicParameters.LandBudget + " Total: " +
                (result.LandHexes.Count + tectonicParameters.LandBudget) + "\n";
            
            if (tectonicParameters.LandBudget == 0)
                break;
        }

        RootLog.Log(logString, Severity.Information, "Diagnostics");

        // If land budget is greater than 0, all land hexes specified to
        // be allocated were not allocated successfully. Log a warning,
        // decrement the remaining land budget from the result, and return
        // the result as the number of land hexes allocated.
        if (tectonicParameters.LandBudget > 0) {
            RootLog.Log(
                "Failed to use up " + tectonicParameters.LandBudget + " land budget.",
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

        hexMapClimate.RefreshTerrainTypes(
            climateParameters,
            result.RiverDigraph
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

    #endregion

    #endregion     
}

    

