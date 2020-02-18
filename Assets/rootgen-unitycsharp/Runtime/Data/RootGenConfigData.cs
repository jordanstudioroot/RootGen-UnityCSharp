using System;
using System.IO;
using UnityEngine;
using RootLogging;

[Serializable]
public class RootGenConfigData : IRootGenConfigData {
    [SerializeField]
    private int _width = (int)Defaults.SmallMapSize.x;
    public int Width {
        get {
            return _width;
        }
        set {
            _width = (int)Mathf.Clamp(
                value,
                Defaults.SmallMapSize.x,
                Defaults.LargeMapSize.x
            );
        }
    }

    [SerializeField]
    private int _height = (int)Defaults.SmallMapSize.y;
    public int Height {
        get {
            return _height;
        }
        set {
            _height = (int)Mathf.Clamp(
                value,
                Defaults.SmallMapSize.y,
                Defaults.LargeMapSize.y
            );
        }
    }

    [SerializeField]
    private bool _wrapping = Defaults.MapWrapping;
    public bool Wrapping {
        get {
            return _wrapping;
        }
        set {
            _wrapping = value;
        }
    }

    [SerializeField]
    private bool _useFixedSeed = Defaults.UseFixedSeed;
    public bool UseFixedSeed {
        get {
            return _useFixedSeed;
        }
        set {
            _useFixedSeed = value;
        }
    }

    [SerializeField]
    private int _seed;
    public int Seed { get { return _seed; } set { _seed = value; } }
    
    [SerializeField]
    private float _jitterProbability = Defaults.JitterProbability;
    public float JitterProbability {
        get {
            return _jitterProbability;
        }
        set {
            _jitterProbability = Mathf.Clamp(
                value,
                Defaults.JitterProbabilityMin,
                Defaults.JitterProbabilityMax
            );
        }
    }

    [SerializeField]
    private int _chunkSizeMin = Defaults.MinChunkSize;
    public int ChunkSizeMin {
        get {
            return _chunkSizeMin;
        }
        set {
            _chunkSizeMin = Mathf.Clamp(
                value,
                Defaults.MaxChunkSize,
                Defaults.ChunkSizeMaxMin
            );
        }
    }

    [SerializeField]
    private int _chunkSizeMax = Defaults.MaxChunkSize;
    public int ChunkSizeMax {
        get { 
            return _chunkSizeMax;
        }
        set {
            _chunkSizeMax = Mathf.Clamp(
                value,
                Defaults.ChunkSizeMinMax,
                Defaults.ChunkSizeMaxMax
            );
        }
    }
    
    [SerializeField]
    private int _landPercentage = Defaults.LandPercentage;
    public int LandPercentage {
        get {
            return _landPercentage;
        }
        set {
            _landPercentage = Mathf.Clamp(
                value,
                Defaults.LandPercentageMin,
                Defaults.LandPercentageMax
            );
        }
    }

    [SerializeField]
    private int _waterLevel = Defaults.WaterLevel;
    public int WaterLevel {
        get {
            return _waterLevel;
        }
        set {
            _waterLevel = Mathf.Clamp(
                value, 
                Defaults.WaterLevelMin, 
                Defaults.WaterLevelMax
            );
        }
    }

    [SerializeField]
    private float _highRiseProbability = 
        Defaults.HighRiseProbability;
    public float HighRiseProbability {
        get {
            return _highRiseProbability;
        }
        set {
            _highRiseProbability = Mathf.Clamp(
                value,
                Defaults.HighRiseProbabilityMin,
                Defaults.HighRiseProbabilityMax
            );
        }
    }

    [SerializeField]
    private float _sinkProbablity = Defaults.SinkProbability;
    public float SinkProbability {
        get {
            return _sinkProbablity;
        }
        set {
            _sinkProbablity = Mathf.Clamp(
                value,
                Defaults.SinkProbabilityMin,
                Defaults.SinkProbabilityMax
            );
        }
    }

    [SerializeField]
    private int _elevationMin = Defaults.ElevationMin;
    public int ElevationMin {
        get {
            return _elevationMin;
        }
        set {
            _elevationMin = Mathf.Clamp(
                value,
                Defaults.ElevationMinMin,
                Defaults.ElevationMaxMin
            );
        }
    }

    [SerializeField]
    private int _elevationMax = Defaults.MaxElevation;
    public int ElevationMax {
        get {
            return _elevationMax;
        }
        set {
            _elevationMax = Mathf.Clamp(
                value,
                Defaults.ElevationMinMax,
                Defaults.ElevationMaxMax
            );
        }
    }

    [SerializeField]
    private int _mapBorderX = Defaults.MapBorderX;
    public int MapBorderX {
        get {
            return _mapBorderX;
        }
        set {
            _mapBorderX = Mathf.Clamp(
                value,
                Defaults.MapBorderXMin,
                Defaults.MapBorderXMax
            );
        }
    }

    [SerializeField]
    private int _mapBorderZ = Defaults.MapBorderZ;
    public int MapBorderZ {
        get {
            return _mapBorderZ;
        }

        set {
            _mapBorderZ = Mathf.Clamp(
                value,
                Defaults.MapBorderZMin,
                Defaults.MapBorderZMax
            );
        }
    }

    [SerializeField]
    private int _regionBorder = Defaults.RegionBorder;
    public int RegionBorder {
        get {
            return _regionBorder;
        }
        set {
            _regionBorder = Mathf.Clamp(
                value,
                Defaults.RegionBorderMin,
                Defaults.RegionBorderMax
            );
        }
    }

    [SerializeField]
    private int _regionCount = Defaults.RegionCount;
    public int RegionCount {
        get {
            return _regionCount;
        }
        set {
            _regionCount = Mathf.Clamp(
                value,
                Defaults.RegionCountMin,
                Defaults.RegionCountMax
            );
        }
    }
    
    [SerializeField]
    private int _erosionPercentage  = Defaults.ErosionPercentage;
    public int ErosionPercentage {
        get {
            return _erosionPercentage;
        }
        set {
            _erosionPercentage = Mathf.Clamp(
                value,
                Defaults.ErosionPercentageMin,
                Defaults.ErosionPercentageMax
            );
        }
    }

    [SerializeField]
    private float _evaporationFactor = Defaults.EvaporationFactor;
    public float EvaporationFactor {
        get {
            return _evaporationFactor;
        }

        set {
            _evaporationFactor = Mathf.Clamp(
                value,
                Defaults.EvaporationFactorMin,
                Defaults.EvaporationFactorMax
            );
        }
    }

    [SerializeField]
    private float _precipitationFactor = Defaults.PrecipitationFactor;
    public float PrecipitationFactor {
        get {
            return _precipitationFactor;
        }
        set {
            _precipitationFactor = Mathf.Clamp(
                value,
                Defaults.PrecipitationFactorMin,
                Defaults.PrecipitationFactorMax
            );
        }
    }

    [SerializeField]
    private float _runoffFactor = Defaults.RunoffFactor;
    public float RunoffFactor {
        get {
            return _runoffFactor;
        }
        set {
            _runoffFactor = Mathf.Clamp(
                value,
                Defaults.RunoffFactorMin,
                Defaults.RunoffFactorMax
            );
        }
    }

    [SerializeField]
    private float _seepageFactor = Defaults.SeepageFactor;
    public float SeepageFactor {
        get {
            return _seepageFactor;
        }

        set {
            _seepageFactor = Mathf.Clamp(
                value,
                Defaults.SeepageFactorMin,
                Defaults.SeepageFactorMax
            );
        }
    }

    [SerializeField]
    private HexDirection _windDirection = Defaults.WindDirection;
    public HexDirection WindDirection {
        get {
            return _windDirection;
        }
        set {
            _windDirection = value;
        }
    }

    [SerializeField]
    private float _windStrength = Defaults.WindStrength;
    public float WindStrength {
        get {
            return _windStrength;
        }

        set {
            _windStrength = Mathf.Clamp(
                value,
                Defaults.WindStrengthMin,
                Defaults.WindStrengthMax
            );
        }
    }
    
    [SerializeField]
    private float _startingMoisture = Defaults.StartingMoisture;
    public float StartingMoisture {
        get {
            return _startingMoisture;
        }
        set {
            _startingMoisture = Mathf.Clamp(
                value,
                Defaults.StartingMoistureMin,
                Defaults.StartingMoistureMax
            );
        }
    }

    [SerializeField]
    private int _riverPercentage = Defaults.RiverPercentage;
    public int RiverPercentage {
        get {
            return _riverPercentage;
        }

        set {
            _riverPercentage = Mathf.Clamp(
                value,
                Defaults.RiverPercentageMin,
                Defaults.RiverPercentageMax
            );
        }
    }

    [SerializeField]
    private float _extraLakeProbability = Defaults.ExtraLakeProbability;
    public float ExtraLakeProbability {
        get {
            return _extraLakeProbability;
        }
        set {
            _extraLakeProbability = Mathf.Clamp(
                value,
                Defaults.ExtraLakeProbabilityMin,
                Defaults.ExtraLakeProbabilityMax
            );
        }
    }

    [SerializeField]
    private float _lowTemperature = Defaults.LowTemperature;
    public float LowTemperature {
        get {
            return _lowTemperature;
        }

        set {
            _lowTemperature = Mathf.Clamp(
                value,
                Defaults.LowTemperatureMin,
                Defaults.LowTemperatureMax
            );
        }
    }
    
    [SerializeField]
    private float _highTemperature = Defaults.HighTemperature;
    public float HighTemperature {
        get {
            return _highTemperature;
        }

        set {
            _highTemperature = Mathf.Clamp(
                value,
                Defaults.HighTemperatureMin,
                Defaults.HighTemperatureMax
            );
        }
    }

    [SerializeField]
    private float _temperatureJitter = Defaults.TemperatureJitter;
    public float TemperatureJitter {
        get {
            return _temperatureJitter;
        }
        set {
            _temperatureJitter = Mathf.Clamp(
                value,
                Defaults.TemperatureJitterMin,
                Defaults.TemperatureJitterMax
            );
        }
    }

    [SerializeField]
    private HemisphereMode _hemisphere = Defaults.HempisphereMode;
    public HemisphereMode Hemisphere {
        get {
            return _hemisphere;
        }
        set {
            _hemisphere = value;
        }
    }

    public void Save(string name) {
        string json = JsonUtility.ToJson(this, true);
        string path = Path.Combine(Application.persistentDataPath, name + ".json");
        RootLog.Log("RootGenConfig saved: " + path);
        RootLog.Log(json);
        StreamWriter sw = new StreamWriter(path, false);
        sw.Write(json);
        sw.Close();
    }

    public static RootGenConfigData Load(string name) {
        StreamReader sr = new StreamReader(
            Path.Combine(Application.persistentDataPath, name + ".json")
        );
        string json = sr.ReadToEnd();
        sr.Close();
        RootGenConfigData result = JsonUtility.FromJson<RootGenConfigData>(json);
        return result;
    }
}