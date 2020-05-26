using UnityEngine;

[System.Serializable]
public struct ClimateData {
    [SerializeField]
    public float clouds;

    [SerializeField]
    public float moisture;

    [SerializeField]
    public float temperature;

    public ClimateData(
        float clouds,
        float moisture,
        float temperature
    ) {
        this.clouds = clouds;
        this.moisture = moisture;
        this.temperature = temperature;
    }

    public override string ToString() {
        return "Cloud Level: " + clouds +
            ", Moisture Level: " + moisture +
            ", Temperature: " + temperature;
    }
}