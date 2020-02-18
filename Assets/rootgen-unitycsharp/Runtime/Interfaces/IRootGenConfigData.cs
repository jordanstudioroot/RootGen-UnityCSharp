using System;

public interface IRootGenConfigData {
    int Width { get; set; }
    int Height { get; set; }
    bool Wrapping { get; set; }
    bool UseFixedSeed { get; set; }
    int Seed { get; set; }
    float JitterProbability { get; set; }
    int ChunkSizeMin { get; set; }
    int ChunkSizeMax { get; set; }
    int LandPercentage { get; set; }
    int WaterLevel { get; set; }
    float HighRiseProbability { get; set; }
    float SinkProbability { get; set; }
    int ElevationMin { get; set; }
    int ElevationMax { get; set; }
    int MapBorderX { get; set; }
    int MapBorderZ { get; set; }
    int RegionBorder { get; set; }
    int RegionCount { get; set; }
    int ErosionPercentage { get; set; }
    float EvaporationFactor { get; set; }
    float PrecipitationFactor { get; set; }
    float RunoffFactor { get; set; }
    float SeepageFactor { get; set; }
    HexDirection WindDirection { get; set; }
    float WindStrength { get; set; }
    float StartingMoisture { get; set; }
    int RiverPercentage { get; set; }
    float ExtraLakeProbability { get; set; }
    float LowTemperature { get; set; }
    float HighTemperature { get; set; }
    float TemperatureJitter { get; set; }
    HemisphereMode Hemisphere { get; set;}
}
