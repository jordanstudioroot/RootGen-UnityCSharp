using UnityEngine;

[CreateAssetMenu(menuName = "RootGenConfig/Default Config")]
public class RootGenConfig : ScriptableObject
{
    public int width;
    public int height;
    public bool wrapping;
    public bool useFixedSeed;
    public int seed;

    [Range(0f, 0.5f)]
    public float jitterProbability = 0.25f;

    [Range(20, 200)]
    public int chunkSizeMin = 30;

    [Range(20, 200)]
    public int chunkSizeMax = 100;

    [Range(5, 95)]
    public int landPercentage = 50;

    [Range(1, 5)]
    public int waterLevel = 3;

    [Range(0f, 1f)]
    public float highRiseProbability = 0.25f;

    [Range(0f, 0.4f)]
    public float sinkProbability = 0.2f;

    [Range(-4, 0)]
    public int elevationMinimum = -2;

    [Range(6, 10)]
    public int elevationMax = 8;

    [Range(0, 10)]
    public int mapBorderX = 5;

    [Range(0, 10)]
    public int mapBorderZ = 5;

    [Range(0, 10)]
    public int regionBorder = 5;

    [Range(1, 4)]
    public int regionCount = 1;

    [Range(0, 100)]
    public int erosionPercentage = 50;

    [Range(0f, 1f)]
    public float evaporationFactor = 0.5f;

    [Range(0f, 1f)]
    public float precipitationFactor = 0.25f;

    [Range(0f, 1f)]
    public float runoffFactor = 0.25f;

    [Range(0f, 1f)]
    public float seepageFactor = 0.125f;

    public HexDirection windDirection = HexDirection.Northwest;

    [Range(1f, 10f)]
    public float windStrength = 4f;

    [Range(0f, 1f)]
    public float startingMoisture = 0.1f;

    [Range(0, 20)]
    public int riverPercentage = 10;

    [Range(0f, 1f)]
    public float extraLakeProbability = 0.25f;

    [Range(0f, 1f)]
    public float lowTemperature = 0f;

    [Range(0f, 1f)]
    public float highTemperature = 1f;

    [Range(0f, 1f)]
    public float temperatureJitter = 0.1f;

    public HemisphereMode hemisphere;

    public RootGenConfigData GetData() {
        RootGenConfigData result = new RootGenConfigData();
        result.ChunkSizeMax = chunkSizeMax;
        result.ChunkSizeMin = chunkSizeMin;
        result.ElevationMax = elevationMax;
        result.ElevationMin = elevationMinimum;
        result.ErosionPercentage = erosionPercentage;
        result.EvaporationFactor = evaporationFactor;
        result.ExtraLakeProbability = extraLakeProbability;
        result.Height = height;
        result.Hemisphere = hemisphere;
        result.HighRiseProbability = highRiseProbability;
        result.HighTemperature = highTemperature;
        result.JitterProbability = jitterProbability;
        result.LandPercentage = landPercentage;
        result.LowTemperature = lowTemperature;
        result.MapBorderX = mapBorderX;
        result.MapBorderZ = mapBorderZ;
        result.PrecipitationFactor = precipitationFactor;
        result.RegionBorder = regionBorder;
        result.RegionCount = regionCount;
        result.RiverPercentage = riverPercentage;
        result.RunoffFactor = runoffFactor;
        result.Seed = seed;
        result.SeepageFactor = seepageFactor;
        result.SinkProbability = sinkProbability;
        result.StartingMoisture = startingMoisture;
        result.TemperatureJitter = temperatureJitter;
        result.UseFixedSeed = useFixedSeed;
        result.WaterLevel = waterLevel;
        result.Width = width;
        result.WindDirection = windDirection;
        result.WindStrength = windStrength;
        result.Wrapping = wrapping;

        return result;
    }
}
