public struct TectonicParameters {
    public float HexSize { get; private set;}
    public float HighRiseProbability { get; private set;}
    public float JitterProbability { get; private set; }
    public float SinkProbability { get; private set; }
    public int ElevationMax { get; private set; }
    public int ElevationMin { get; private set; }
    public int WaterLevelGlobal { get; private set; }
    public int LandBudget { get; set; }
    public int RegionDensityMax { get; private set; }
    public int RegionDensityMin { get; private set; }

    public TectonicParameters(
        float hexSize,
        float highRiseProbability,
        float jitterProbability,
        float sinkProbability,
        int elevationMax,
        int elevationMin,
        int waterLevelGlobal,
        int landBudget,
        int regionDensityMax,
        int regionDensityMin
    )
     {
         this.HexSize = hexSize;
         this.HighRiseProbability = highRiseProbability;
         this.JitterProbability = jitterProbability;
         this.SinkProbability = sinkProbability;
         this.ElevationMax = elevationMax;
         this.ElevationMin = elevationMin;
         this.WaterLevelGlobal = waterLevelGlobal;
         this.LandBudget = landBudget;
         this.RegionDensityMax = regionDensityMax;
         this.RegionDensityMin = regionDensityMin;
     }
}