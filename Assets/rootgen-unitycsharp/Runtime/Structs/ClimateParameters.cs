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