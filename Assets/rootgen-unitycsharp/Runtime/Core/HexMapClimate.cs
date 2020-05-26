using System.Collections.Generic;
using UnityEngine;

public class HexMapClimate {

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

    private int _temperatureJitterChannel;

    public ClimateData this[int index] {
        get {
            return Array[index];
        }
    }
    
    public ClimateData[] Array {
        get; private set;
    }

    public List<ClimateData> List {
        get {
            List<ClimateData> result = new List<ClimateData>();

            foreach (ClimateData data in Array) {
                result.Add(data);
            }

            return result;
        }
    }

    private HexMap _hexMap;

    public HexMapClimate(
        HexMap hexMap,
        float startingMoisture
    ) {
        _hexMap = hexMap;
        _temperatureJitterChannel = Random.Range(0, 4);

        Array = new ClimateData[_hexMap.SizeSquared];

        for (int i = 0; i < Array.Length; i++) {
            ClimateData initial = new ClimateData(
                0,
                startingMoisture,
                0            
            );
        }
    }

    public void Step(
        ClimateParameters parameters
    ) {
        HexAdjacencyGraph adjacencyGraph =
            _hexMap.AdjacencyGraph;

        // Initialize the next climate step.
        ClimateData[] nextClimates = new ClimateData[Array.Length];

        // Populate the next climate step.
        for (int i = 0; i < Array.Length; i++) {
            nextClimates[i] = new ClimateData();
        }

        // For each hex cell...
        for (int i = 0; i < _hexMap.SizeSquared; i++) {
            Hex source = _hexMap.GetHex(i);
            List<HexEdge> adjacentEdges =
                adjacencyGraph.GetOutEdges(source);

            // Get the next climate step for that cell.
            StepClimate(
                Array,
                ref nextClimates,
                source,
                adjacentEdges,
                parameters
            );
        }

        Array = nextClimates;
    }

    private void StepClimate(
        ClimateData[] currentClimates,
        ref ClimateData[] nextClimates,
        Hex source,
        List<HexEdge> adjacentEdges,
        ClimateParameters parameters
    ) {
        ClimateData currentClimate = currentClimates[source.Index];

        // If the tile is water, add clouds equivalent to the evaporation
        // factor.
        if (source.IsUnderwater) {
            currentClimate.moisture = 1f;
            currentClimate.clouds += parameters.evaporationFactor;
        }
        // If the tile is not water, scale evaporation with the moisture
        // of the tile and create clouds from that.
        else {
            float evaporation =
                currentClimate.moisture * parameters.evaporationFactor;
            currentClimate.moisture -= evaporation;
            currentClimate.clouds += evaporation;
        }

        // Precipitation is scaled with the number of clouds.
        float precipitation =
            currentClimate.clouds * parameters.precipitationFactor;
        currentClimate.clouds -= precipitation;
        currentClimate.moisture += precipitation;

        // As the elevation of the hex approaches the maximum elevation,
        // the maximum number of clouds decreases.
        float cloudMaximum =
            1f - source.ViewElevation / (parameters.elevationMax + 1f);

        // If the number of clouds exceeds the cloud maximum, convert
        // excess clouds to moisture.
        if (currentClimate.clouds > cloudMaximum) {
            currentClimate.moisture +=
                currentClimate.clouds - cloudMaximum;
            currentClimate.clouds = cloudMaximum;
        }

        // Get the main cloud dispersal direction.
        HexDirections mainCloudDispersalDirection =
            parameters.windDirection.Opposite();

        // Get the cloud dispersal magnitude.
        float cloudDispersalMagnitude =
            currentClimate.clouds * (1f / (5f + parameters.windStrength));
        
        // Calculate the amount of runoff.
        float runoff =
            currentClimate.moisture * parameters.runoffFactor * (1f / 6f);
        
        // Calculate the amount of seepage.
        float seepage =
            currentClimate.moisture * parameters.seepageFactor * (1f / 6f);

        // Disperse clouds, runoff, and seepage to adjacent climates.
        foreach (HexEdge edge in adjacentEdges) {
            ClimateData neighborClimate =
                nextClimates[edge.Target.Index]; 
            
            if (edge.Direction == mainCloudDispersalDirection) {
                neighborClimate.clouds +=
                    cloudDispersalMagnitude * parameters.windStrength;
            }
            else {
                neighborClimate.clouds += cloudDispersalMagnitude;
            }

            int elevationDelta =
                edge.Target.ViewElevation - edge.Source.ViewElevation;

            if (elevationDelta < 0) {
                currentClimate.moisture -= runoff;
                neighborClimate.moisture += runoff;
            }
            else if (elevationDelta == 0) {
                currentClimate.moisture -= seepage;
                neighborClimate.moisture += seepage;
            }

            neighborClimate.moisture =
                Mathf.Clamp(
                    neighborClimate.moisture,
                    0f,
                    1f
                );

            nextClimates[edge.Target.Index] = neighborClimate;
        }

        ClimateData nextHexClimate = nextClimates[source.Index];
        nextHexClimate.moisture += currentClimate.moisture;

        // Ensure that no hex can have more moisture than a hex that is
        // underwater.
        nextHexClimate.moisture =
            Mathf.Clamp(
                nextHexClimate.moisture,
                0f, 
                1f
            );

        nextHexClimate.temperature =
            GenerateTemperature(
                source,
                parameters.hemisphere,
                parameters.waterLevel,
                parameters.elevationMax,
                _temperatureJitterChannel,
                parameters.temperatureJitter,
                parameters.lowTemperature,
                parameters.highTemperature,
                parameters.hexOuterRadius
            );

        //Store the data for the next climate.
        nextClimates[source.Index] = nextHexClimate;
    }

    private float GenerateTemperature(
        Hex hex,
        HemisphereMode hemisphere,
        int waterLevel,
        int elevationMax,
        int temperatureJitterChannel,
        float temperatureJitter,
        float lowTemperature,
        float highTemperature,
        float hexOuterRadius
    ) {
        float latitude =
            (float)hex.CubeCoordinates.Z / _hexMap.HexOffsetRows;

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
                _hexMap.WrapSize
            )[temperatureJitterChannel];

        temperature += (jitter * 2f - 1f) * temperatureJitter;

        return temperature;
    }
}

public struct ClimateParameters {
        public HemisphereMode hemisphere;
        public HexDirections windDirection;
        public float evaporationFactor;
        public float highTemperature;
        public float lowTemperature;
        public float precipitationFactor;
        public float runoffFactor;
        public float seepageFactor;
        public float temperatureJitter;
        public float windStrength;
        public float hexOuterRadius;
        public int elevationMax;
        public int waterLevel;

        public ClimateParameters(
            HemisphereMode hemisphere,
            HexDirections windDirection,
            float evaporationFactor,
            float highTemperature,
            float lowTemperature,
            float precipitationFactor,
            float runoffFactor,
            float seepageFactor,
            float temperatureJitter,
            float windStrength,
            float hexOuterRadius,
            int elevationMax,
            int waterLevel
        ) {
            this.hemisphere = hemisphere;
            this.windDirection = windDirection;
            this.evaporationFactor = evaporationFactor;
            this.highTemperature = highTemperature;
            this.lowTemperature = lowTemperature;
            this.precipitationFactor = precipitationFactor;
            this.runoffFactor = runoffFactor;
            this.seepageFactor = seepageFactor;
            this.temperatureJitter = temperatureJitter;
            this.windStrength = windStrength;
            this.hexOuterRadius = hexOuterRadius;
            this.elevationMax = elevationMax;
            this.waterLevel = waterLevel;
        }
    }