using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RootLogging;

public class SaveConfigMenu : MonoBehaviour
{
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public
    public TMP_Dropdown hemisphere;
    public TMP_Dropdown windDirection;
    public TMP_InputField seed;
    public TMP_InputField fileNameInput;
    public Slider chunkSizeMax;
    public Slider chunkSizeMin;
    public Slider elevationMax;
    public Slider elevationMin;
    public Slider erosionPercentage;
    public Slider evaporationFactor;
    public Slider extraLakeProbability;
    public Slider height;
    public Slider highRiseProbability;
    public Slider highTemperature;
    public Slider jitterProbability;
    public Slider landPercentage;
    public Slider lowTemperature;
    public Slider mapBorderX;
    public Slider mapBorderZ;
    public Slider precipitationFactor;
    public Slider regionBorder;
    public Slider regionCount;
    public Slider riverPercentage;
    public Slider runoffFactor;
    public Slider seepageFactor;
    public Slider sinkProbability;
    public Slider startingMoisture;
    public Slider temperatureJitter;
    public Slider waterLevel;
    public Slider width;
    public Slider windStrength;
    public Toggle useFixedSeed;
    public Toggle wrapping;
    public Button saveConfig;


// ~~ private
    private bool _active;
    private RootGenConfigData _activeData;
    private string _fileName;

// CONSTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DESTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DELEGATES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// EVENTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// ENUMS

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INTERFACES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// PROPERTIES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INDEXERS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// METHODS ~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public
    public void ToggleShowHide() {
        if (_active) {
            Hide();
        }
        else {
            Show();
        }
    }

    public void Show() {
        _active = true;
        _activeData = new RootGenConfigData();
        this.gameObject.SetActive(true);
    }

    public void Hide() {
        _active = false;
        _activeData = new RootGenConfigData();
        _fileName = string.Empty;
        fileNameInput.text = string.Empty;
        this.gameObject.SetActive(false);
    }

    public void OnDisable() {
        _activeData = new RootGenConfigData();
    }

    public void OnEnable() {
        _activeData = new RootGenConfigData();
    }
    
    public void OnSaveConfig() {
        if (
            _fileName != null ||
            _fileName == string.Empty &&
            !Application.persistentDataPath.Contains(
                _fileName + ".json"
            )
        ) {
            _activeData.Save(_fileName);
        }
        else {
            RootLog.Log(
                "Blank or duplicate file name.",
                Severity.Warning,
                "SaveConfigMenu"
            );
        }

        _activeData = new RootGenConfigData();
        _fileName = string.Empty;
        fileNameInput.text = string.Empty;
        this.gameObject.SetActive(false);
    }

    public void OnFileName(string value) {
        _fileName = value;
    }
    public void OnSeed(string value) {
        _activeData.Seed = Int32.Parse(value);
    }

    public void OnChunkSizeMax(float value) {
        _activeData.ChunkSizeMax = (int)value;
    }

    public void OnChunkSizeMin(float value) {
        _activeData.ChunkSizeMin = (int)value;
    }

    public void OnElevationMax(float value) {
        _activeData.ElevationMax = (int)value;
    }

    public void OnElevationMin(float value) {
        _activeData.ElevationMin = (int)value;
    }

    public void OnErosionPercentage(float value) {
        _activeData.ErosionPercentage = (int)value;
    }

    public void OnEvaporationFactor(float value) {
        _activeData.EvaporationFactor = value;
    }

    public void OnExtraLakeProbability(float value) {
        _activeData.ExtraLakeProbability = value;
    }

    public void OnHeight(float value) {
        _activeData.Height = (int)value;
    }

    public void OnHighRiseProbability(float value) {
        _activeData.HighRiseProbability = value;
    }

    public void OnJitterProbability(float value) {
        _activeData.JitterProbability = value;
    }

    public void OnLandPercentage(float value) {
        _activeData.LandPercentage = (int)value;
    }

    public void OnLowTemperature(float value) {
        _activeData.LowTemperature = value;
    }

    public void OnMapBorderX(float value) {
        _activeData.MapBorderX = (int)value;
    }

    public void OnMapBorderZ(float value) {
        _activeData.MapBorderZ = (int)value;
    }

    public void OnPrecipitationFactor(float value) {
        _activeData.PrecipitationFactor = value;
    }
    public void OnRegionBorder(float value) {
        _activeData.RegionBorder = (int)value;
    }
    public void OnRegionCount(float value) {
        _activeData.RegionCount = (int)value;
    }
    public void OnHighTemperature(float value) {
        _activeData.HighTemperature = value;
    }
    public void OnRiverPercentage(float value) {
        _activeData.RiverPercentage = (int)value;
    }
    public void OnRunoffFactor(float value) {
        _activeData.RunoffFactor = value;
    }

    public void OnSeepageFactor(float value) {
        _activeData.SeepageFactor = value;
    }

    public void OnSinkProbability(float value) {
        _activeData.SinkProbability = value;
    }

    public void OnStartingMoisture(float value) {
        _activeData.StartingMoisture = value;
    }

    public void OnTemperatureJitter(float value) {
        _activeData.TemperatureJitter = value;
    }
    public void OnHemisphere(int value) {
        _activeData.Hemisphere = (HemisphereMode)value;
    }
    public void OnWaterLevel(float value) {
        _activeData.WaterLevel = (int)value;
    }
    public void OnWidth(float value) {
        _activeData.Width = (int)value;
    }
    public void OnWindStrength(float value) {
        _activeData.WindStrength = value;
    }
    public void OnUseFixedSeed(bool value) {
        _activeData.UseFixedSeed = value;
    }
    public void OnWrapping(bool value) {
        _activeData.Wrapping = value;
    }

// ~~ private
    private void Awake() {
        regionBorder.wholeNumbers = true;
        regionCount.wholeNumbers = true;         
        riverPercentage.wholeNumbers = true;
        waterLevel.wholeNumbers = true;
        width.wholeNumbers = true;

        hemisphere.onValueChanged.AddListener(OnHemisphere);
        seed.onValueChanged.AddListener(OnSeed);
        fileNameInput.onValueChanged.AddListener(OnFileName);
        chunkSizeMax.onValueChanged.AddListener(OnChunkSizeMax);
        chunkSizeMin.onValueChanged.AddListener(OnChunkSizeMin);
        elevationMax.onValueChanged.AddListener(OnElevationMax);
        elevationMin.onValueChanged.AddListener(OnElevationMin);
        erosionPercentage.onValueChanged.AddListener(OnErosionPercentage);
        evaporationFactor.onValueChanged.AddListener(OnEvaporationFactor);
        extraLakeProbability.onValueChanged.AddListener(OnExtraLakeProbability);
        height.onValueChanged.AddListener(OnHeight);
        highRiseProbability.onValueChanged.AddListener(OnHighRiseProbability);
        highTemperature.onValueChanged.AddListener(OnHighTemperature);
        jitterProbability.onValueChanged.AddListener(OnJitterProbability);
        landPercentage.onValueChanged.AddListener(OnLandPercentage);
        lowTemperature.onValueChanged.AddListener(OnLowTemperature);
        mapBorderX.onValueChanged.AddListener(OnMapBorderX);
        mapBorderZ.onValueChanged.AddListener(OnMapBorderZ);
        precipitationFactor.onValueChanged.AddListener(OnPrecipitationFactor);
        regionBorder.onValueChanged.AddListener(OnRegionBorder);
        regionCount.onValueChanged.AddListener(OnRegionCount);
        riverPercentage.onValueChanged.AddListener(OnRiverPercentage);
        runoffFactor.onValueChanged.AddListener(OnRunoffFactor);
        seepageFactor.onValueChanged.AddListener(OnSeepageFactor);
        sinkProbability.onValueChanged.AddListener(OnSinkProbability);
        startingMoisture.onValueChanged.AddListener(OnStartingMoisture);
        temperatureJitter.onValueChanged.AddListener(OnTemperatureJitter);
        waterLevel.onValueChanged.AddListener(OnWaterLevel);
        width.onValueChanged.AddListener(OnWidth);
        windStrength.onValueChanged.AddListener(OnWindStrength);
        useFixedSeed.onValueChanged.AddListener(OnUseFixedSeed);
        wrapping.onValueChanged.AddListener(OnWrapping);
        saveConfig.onClick.AddListener(OnSaveConfig);

    }
    private void Start() {
        LoadDefaults();

    }

    private void LoadDefaults() {
        hemisphere.value = (int)Defaults.HempisphereMode;
        chunkSizeMax.minValue = Defaults.ChunkSizeMaxMin;
        chunkSizeMax.maxValue = Defaults.ChunkSizeMaxMax;
        chunkSizeMax.value = Defaults.MaxChunkSize;
        chunkSizeMin.minValue = Defaults.ChunkSizeMinMin;
        chunkSizeMin.maxValue = Defaults.ChunkSizeMinMax;
        chunkSizeMin.value = Defaults.MinChunkSize;
        elevationMax.minValue = Defaults.ElevationMaxMin;
        elevationMax.maxValue = Defaults.ElevationMaxMax;
        elevationMax.value = Defaults.MaxElevation;
        elevationMin.minValue = Defaults.ElevationMinMin;
        elevationMin.maxValue = Defaults.ElevationMinMax;
        elevationMin.value = Defaults.ElevationMin;
        erosionPercentage.minValue = Defaults.ErosionPercentageMin;
        erosionPercentage.maxValue = Defaults.ErosionPercentageMax;
        erosionPercentage.value = Defaults.ErosionPercentage;
        evaporationFactor.minValue = Defaults.EvaporationFactorMin;
        evaporationFactor.maxValue = Defaults.EvaporationFactorMax;
        evaporationFactor.value = Defaults.EvaporationFactor;
        extraLakeProbability.minValue = Defaults.ExtraLakeProbabilityMin;
        extraLakeProbability.maxValue = Defaults.ExtraLakeProbabilityMax;
        extraLakeProbability.value = Defaults.ExtraLakeProbability;
        height.minValue = Defaults.SmallMapSize.y;
        height.maxValue = Defaults.LargeMapSize.y;
        height.value = Defaults.MediumMapSize.y;
        highRiseProbability.minValue = Defaults.HighRiseProbabilityMin;
        highRiseProbability.maxValue = Defaults.HighRiseProbabilityMax;
        highRiseProbability.value = Defaults.HighRiseProbability;
        highTemperature.minValue = Defaults.HighTemperatureMin;
        highTemperature.maxValue = Defaults.HighTemperatureMax;
        highTemperature.value = Defaults.HighTemperature;
        jitterProbability.minValue = Defaults.JitterProbabilityMin;
        jitterProbability.maxValue = Defaults.JitterProbabilityMax;
        jitterProbability.value = Defaults.JitterProbability;
        landPercentage.minValue = Defaults.LandPercentageMin;
        landPercentage.maxValue = Defaults.LandPercentageMax;
        landPercentage.value = Defaults.LandPercentage;
        lowTemperature.minValue = Defaults.LowTemperatureMin;
        lowTemperature.maxValue = Defaults.LowTemperatureMax;
        lowTemperature.value = Defaults.LowTemperature;
        mapBorderX.minValue = Defaults.MapBorderXMin;
        mapBorderX.maxValue = Defaults.MapBorderXMax;
        mapBorderX.value = Defaults.MapBorderX;
        mapBorderZ.minValue = Defaults.MapBorderZMin;
        mapBorderZ.maxValue = Defaults.MapBorderZMax;
        mapBorderZ.value = Defaults.MapBorderZ;
        precipitationFactor.minValue = Defaults.PrecipitationFactorMin;
        precipitationFactor.maxValue = Defaults.PrecipitationFactorMax;
        precipitationFactor.value = Defaults.PrecipitationFactor;
        regionBorder.minValue = Defaults.RegionBorderMin;
        regionBorder.maxValue = Defaults.RegionBorderMax;
        regionBorder.value = Defaults.RegionBorder;
        regionCount.minValue = Defaults.RegionCountMin;
        regionCount.maxValue = Defaults.RegionCountMax;
        regionCount.value = Defaults.RegionCount;
        riverPercentage.minValue = Defaults.RiverPercentageMin;
        riverPercentage.maxValue = Defaults.RiverPercentageMax;
        riverPercentage.value = Defaults.RiverPercentage;
        runoffFactor.minValue = Defaults.RunoffFactorMin;
        runoffFactor.maxValue = Defaults.RunoffFactorMax;
        runoffFactor.value = Defaults.RunoffFactor;
        seepageFactor.minValue = Defaults.SeepageFactorMin;
        seepageFactor.maxValue = Defaults.SeepageFactorMax;
        seepageFactor.value = Defaults.SeepageFactor;
        sinkProbability.minValue = Defaults.SinkProbabilityMin;
        sinkProbability.maxValue = Defaults.SinkProbabilityMax;
        sinkProbability.value = Defaults.SinkProbability;
        startingMoisture.minValue = Defaults.StartingMoistureMin;
        startingMoisture.maxValue = Defaults.StartingMoistureMax;
        startingMoisture.value = Defaults.StartingMoisture;
        temperatureJitter.minValue = Defaults.TemperatureJitterMin;
        temperatureJitter.maxValue = Defaults.TemperatureJitterMax;
        temperatureJitter.value = Defaults.TemperatureJitter;
        waterLevel.minValue = Defaults.WaterLevelMin;
        waterLevel.maxValue = Defaults.WaterLevelMax;
        waterLevel.value = Defaults.WaterLevel;
        width.minValue = Defaults.SmallMapSize.x;
        width.maxValue = Defaults.LargeMapSize.x;
        width.value = Defaults.MediumMapSize.x;
        windStrength.minValue = Defaults.WindStrengthMin;
        windStrength.maxValue = Defaults.WindStrengthMax;
        windStrength.value = Defaults.WindStrength;
        useFixedSeed.isOn = Defaults.UseFixedSeed;
        wrapping.isOn = Defaults.UseFixedSeed;

    }

// STRUCTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// CLASSES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
}
