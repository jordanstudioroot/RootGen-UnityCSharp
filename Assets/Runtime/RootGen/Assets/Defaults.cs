using UnityEngine;

public static class Defaults {
    // Default map generation settings
    public static bool UseFixedSeed {
        get {
            return false;
        }
    }
    public static bool MapWrapping {
        get {
            return true;
        }
    }
    public static Vector2 SmallMapSize {
        get {
            return new Vector2(15, 20);
        }
    }

    public static Vector2 MediumMapSize {
        get {
            return new Vector2(30, 40);
        }
    }

    public static Vector2 LargeMapSize {
        get {
            return new Vector2(60, 80);
        }
    }

    public static int WaterLevel {
        get {
            return 3;
        }
    }
    public static int WaterLevelMin {
        get {
            return 1;
        }
    }
    public static int WaterLevelMax {
        get {
            return 5;
        }
    }

    public static int LandPercentage {
        get {
            return 50;
        }
    }

    public static int LandPercentageMin {
        get {
            return 5;
        }
    }

    public static int LandPercentageMax {
        get {
            return 95;
        }
    }

    public static int MinChunkSize {
        get {
            return 30;
        }
    }

    public static int ChunkSizeMinMin {
        get {
            return 20;
        }
    }

    public static int ChunkSizeMaxMin {
        get {
            return 200;
        }
    }

    public static int MaxChunkSize {
        get {
            return 100;
        }
    }
    
    public static int ChunkSizeMinMax {
        get {
            return 20;
        }
    }

    public static int ChunkSizeMaxMax {
        get {
            return 200;
        }
    }

    public static float JitterProbability {
        get {
            return 0.25f;
        }
    }

    public static float JitterProbabilityMin {
        get {
            return 0f;
        }
    }

    public static float JitterProbabilityMax {
        get{
            return 0.5f;
        }
    }

    public static float HighRiseProbability {
        get {
            return 0.25f;
        }
    }

    public static float HighRiseProbabilityMin {
        get {
            return 0f;
        }
    }

    public static float HighRiseProbabilityMax {
        get {
            return 1f;
        }
    }

    public static float SinkProbability {
        get {
            return 0.2f;
        }
    }

    public static float SinkProbabilityMin {
        get {
            return 0f;
        }
    }

    public static float SinkProbabilityMax {
        get {
            return 0.4f;
        }
    }

    public static int ElevationMin {
        get {
            return -2;
        }
    }

    public static int ElevationMinMin {
        get {
            return -4;
        }
    }

    public static int ElevationMaxMin {
        get {
            return 0;
        }
    }

    public static int MaxElevation {
        get {
            return 8;
        }
    }

    public static int ElevationMinMax {
        get {
            return 6;
        }
    }

    public static int ElevationMaxMax {
        get {
            return 10;
        }
    }

    public static int MapBorderX {
        get {
            return 5;
        }
    }

    public static int MapBorderXMin {
        get {
            return 0;
        }
    }

    public static int MapBorderXMax {
        get {
            return 10;
        }
    }

    public static int MapBorderZ {
        get {
            return 5;
        }
    }

    public static int MapBorderZMin {
        get {
            return 0;
        }
    }

    public static int MapBorderZMax {
        get {
            return 10;
        }
    }

    public static int RegionBorder {
        get {
            return 5;
        }
    }

    public static int RegionBorderMin {
        get {
            return 0;
        }
    }

    public static int RegionBorderMax {
        get {
            return 10;
        }
    }

    public static int RegionCount {
        get {
            return 1;
        }
    }

    public static int RegionCountMin {
        get {
            return 1;
        }
    }

    public static int RegionCountMax {
        get {
            return 4;
        }
    }

    public static int ErosionPercentage {
        get {
            return 50;
        }
    }

    public static int ErosionPercentageMin {
        get {
            return 0;
        }
    }

    public static int ErosionPercentageMax {
        get {
            return 100;
        }
    }

    public static float EvaporationFactor {
        get {
            return 0.5f;
        }
    }

    public static float EvaporationFactorMin {
        get {
            return 0f;
        }
    }

    public static float EvaporationFactorMax {
        get {
            return 1f;
        }
    }

    public static float PrecipitationFactor {
        get {
            return 0.25f;
        }
    }

    public static float PrecipitationFactorMin {
        get {
             return 0f;
        }
    }

    public static float PrecipitationFactorMax {
        get {
            return 1f;
        }
    }

    public static float RunoffFactor {
        get {
            return 0.25f;
        }
    }

    public static float RunoffFactorMin {
        get {
            return 0f;
        }
    }

    public static float RunoffFactorMax {
        get {
            return 1f;
        }
    }

    public static float SeepageFactor {
        get {
            return 0.125f;
        }
    }

    public static float SeepageFactorMin {
        get {
            return 0f;
        }
    }

    public static float SeepageFactorMax {
        get {
            return 1f;
        }
    }

    public static HexDirection WindDirection {
        get {
            return HexDirection.Northwest;
        }
    }

    public static float WindStrength {
        get {
            return 4f;
        }
    }

    public static float WindStrengthMin {
        get {
            return 1f;
        }
    }

    public static float WindStrengthMax {
        get {
            return 10f;
        }
    }

    public static float StartingMoisture {
        get {
            return 0.1f;
        }
    }

    public static float StartingMoistureMin {
        get {
            return 1f;
        }
    }

    public static float StartingMoistureMax {
        get {
            return 1f;
        }
    }

    public static int RiverPercentage {
        get {
            return 10;
        }
    }

    public static int RiverPercentageMin {
        get {
            return 0;
        }
    }

    public static int RiverPercentageMax {
        get {
            return 20;
        }
    }

    public static float ExtraLakeProbability {
        get {
            return 0.25f;
        }
    }

    public static float ExtraLakeProbabilityMin {
        get {
            return 0f;
        }
    }

    public static float ExtraLakeProbabilityMax {
        get {
            return 1f;
        }
    }

    public static float LowTemperature {
        get {
            return 0f;
        }
    }

    public static float LowTemperatureMin {
        get {
            return 0f;
        }
    }

    public static float LowTemperatureMax {
        get {
            return 1f;
        }
    }

    public static float HighTemperature {
        get {
            return 1f;
        }
    }

    public static float HighTemperatureMin {
        get {
            return 0f;
        }
    }

    public static float HighTemperatureMax {
        get {
            return 1f;
        }
    }

    public static float TemperatureJitter {
        get {
            return 0.1f;
        }
    }

    public static float TemperatureJitterMin {
        get {
            return 0f;
        }
    }

    public static float TemperatureJitterMax {
        get {
            return 1f;
        }
    }

    public static HemisphereMode HempisphereMode {
        get {
            return HemisphereMode.Both;
        }
    }
}