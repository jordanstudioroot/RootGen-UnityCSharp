using UnityEngine;
using RootLogging;
using System.IO;
using System.Text.RegularExpressions;

[CreateAssetMenu(menuName = "RootGenConfig/Default Config")]
public class RootGenConfig : ScriptableObject
{
    public int width;

    public int height;

    public bool wrapping;

    public bool useFixedSeed;

    public int seed;

    [Range(10f, 9999f)]
    public int cellOuterRadius = 10;

    [Range(0f, 0.5f)]
    public float jitterProbability = 0.25f;

    [Range(20, 200)]
    public int minimumRegionDensity = 30;

    [Range(20, 200)]
    public int maximumRegionDensity = 100;

    [Range(5, 95)]
    public int landPercentage = 50;

    [Range(1, 5)]
    public int waterLevel = 3;

    [Range(0f, 1f)]
    public float highRiseProbability = 0.25f;

    [Range(0f, 0.4f)]
    public float sinkProbability = 0.2f;

    [Range(-4, 0)]
    public int elevationMin = -2;

    [Range(6, 10)]
    public int elevationMax = 8;

    [Range(0, 10)]
    public int mapBorderX = 5;

    [Range(0, 10)]
    public int mapBorderZ = 5;

    [Range(0, 10)]
    public int regionBorder = 5;

    [Range(1, 9999)]
    public int numRegions = 1;

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


/// <summary>
/// Serializes the instance to JSON and saves to Application.persistentDataPath.
/// </summary>
/// <param fileName="configName">The name of the config.</param>
    public void ToJson(string fileName) {
        if (HasDigits(fileName)) {
            RootLog.Log(
                "Attempted to serialize a RootGenConfig to JSON with a" +
                " file name containing digits. Removing digits from file name."
            );

            fileName = RemoveDigits(fileName);
        }
        
        string json = JsonUtility.ToJson(this, true);
        string path = Path.Combine(Application.persistentDataPath, fileName + ".json");
        int numDuplicate = 0;

        while (File.Exists(path)) {
            numDuplicate++;
            path = Path.Combine(Application.persistentDataPath, fileName + numDuplicate + ".json");

            RootLog.Log(
                "Serialized RootGenConfig data with file name " + fileName + " already exists. Attempting to " +
                " write as " + fileName + numDuplicate,
                Severity.Warning
            );
        }

        StreamWriter sw = new StreamWriter(path, false);
        sw.Write(json);

        string instances = numDuplicate > 0 ? numDuplicate.ToString() : "";

        RootLog.Log(
            "RootGenConfigData serialized to " + Application.persistentDataPath + " as " + fileName + instances,
            Severity.Information
        );

        sw.Close();
    }

/// <summary>
/// Instantiates a new instance of the RootGenConfig scriptable object with the default parameters, and attempts to
/// populate its fields with JSON data.
/// </summary>
/// <param name="path">The path of the JSON file containing the desired data.</param>
/// <returns>
///     A new instance of the RootGenConfig scriptable object, populated with the desired JSON data if the path exists or
///     the default parameters for RootGenConfig if it does not.
/// </returns>
    public static RootGenConfig FromJson(string configName) {
        RootGenConfig result = ScriptableObject.CreateInstance<RootGenConfig>();
        string path = Path.Combine(Application.persistentDataPath, configName + ".json");

        JsonUtility.FromJsonOverwrite(path, result);
        
        return result;
    }

    private bool HasDigits(string toCheck) {
        return Regex.Match(toCheck, @"[\d]").Length > 0 ? true : false;
    }

    private string RemoveDigits(string toRemove) {
        return Regex.Replace(toRemove, @"[\d]", string.Empty);
    }
}
