using UnityEngine;

public class Holdridge {
// e = potential evapotranspiration ratio
// p = annual precipitation      
//          p|[0,.125)|[.125,250)|[250,500)|[500,1k)|[1k,2k) |[2k,4k) |[4k, 8k)| >=8k   |
// -------------------------------------------------------------------|--------|--------|
// e[0, .25) | DesCol | DesCol   | DesCol  | RaiTun | RaiFor | RaiFor | RaiFor | RaiFor |
//           | Alvar  | Alvar    | Alvar   | Alpine | SubAlp | Montan | LowMon | PreMon |
//           | Polar  | Polar    | Polar   | SubPol | Boreal | CooTem | WarTem | Tropic |
// -------------------------------------------------------------------|--------|--------|
// e[.25, .5)| DesCol | DesCol   | WetTun  | WetFor | WetFor | WetFor | WetFor | WetFor |
//           | Alvar  | Alvar    | Alpine  | SubAlp | Montan | LowMon | PreMon | PreMon |
//           | Polar  | Polar    | SubPol  | Boreal | CooTem | WarTem | Tropic | Tropic |
// -------------------------------------------------------------------|--------|--------|
// e[.5, 1)  | DesCol | MoiTun   | MoiFor  | MoiFor | MoiFor | MoiFor | MoiFor | MoiFor |
//           | Alvar  | Alpine   | SubAlp  | Montan | LowMon | PreMon | PreMon | PreMon |
//           | Polar  | SubPol   | Boreal  | CooTem | WarTem | Tropic | Tropic | Tropic |
// -------------------------------------------------------------------|--------|--------|
// e[1, 2)   | DryTun | DryScr   | Steppe  | DryFor | DryFor | DryFor | DryFor | DryFor |
//           | Alpine | SubAlp   | Montan  | LowMon | PreMon | PreMon | PreMon | PreMon |
//           | SubPol | Boreal   | CooTem  | WarTem | Tropic | Tropic | Tropic | Tropic |
// -------------------------------------------------------------------|--------|--------|
// e[2, 4)   | DesHot | DesScr   | ThStWo  | VeDrFo | VeDrFo | VeDrFo | VeDrFo | VeDrFo |
//           | SubAlp | Montan   | LowMon  | PreMon | PreMon | PreMon | PreMon | PreMon |
//           | Boreal | CooTem   | WarTem  | Tropic | Tropic | Tropic | Tropic | Tropic |
// -------------------------------------------------------------------|--------|--------|
// e[4, 8)   | DesHot | DesScr   | ThoWoo  | ThoWoo | ThoWoo | ThoWoo | ThoWoo | ThoWoo |
//           | Montan | LowMon   | PreMon  | PreMon | PreMon | PreMon | PreMon | PreMon |
//           | CooTem | WarTem   | Tropic  | Tropic | Tropic | Tropic | Tropic | Tropic |
// -------------------------------------------------------------------|--------|--------|
// e[8, 16)  | DesHot | DesScr   | DesScr  | DesScr | DesScr | DesScr | DesScr | DesScr |
//           | LowMon | PreMon   | PreMon  | PreMon | PreMon | PreMon | PreMon | PreMon |
//           | WarTem | Tropic   | Tropic  | Tropic | Tropic | Tropic | Tropic | Tropic |
// -------------------------------------------------------------------|--------|--------|
// e >= 16   | DesHot | DesHot   | DesHot  | DesHot | DesHot | DesHot | DesHot | DesHot |
//           | PreMon | PreMon   | PreMon  | PreMon | PreMon | PreMon | PreMon | PreMon |
//           | Tropic | Tropic   | Tropic  | Tropic | Tropic | Tropic | Tropic | Tropic |
// -------------------------------------------------------------------------------------|
    public HoldridgeZone[][] holdridgeZones = {
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DesertC, ABelt.Alvar,    LRegion.Polar),
            new HoldridgeZone(LZone.DesertC, ABelt.Alvar,    LRegion.Polar),
            new HoldridgeZone(LZone.DesertC, ABelt.Alvar,    LRegion.Polar),
            new HoldridgeZone(LZone.RTundra, ABelt.Alpine,   LRegion.SPolar),
            new HoldridgeZone(LZone.RForest, ABelt.SAlpine,  LRegion.Boreal),
            new HoldridgeZone(LZone.RForest, ABelt.Montane,  LRegion.CTemp),
            new HoldridgeZone(LZone.RForest, ABelt.LMontane, LRegion.WTemp),
            new HoldridgeZone(LZone.RForest, ABelt.PMontane, LRegion.Tropic)
        },
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DesertC, ABelt.Alvar,    LRegion.Polar),
            new HoldridgeZone(LZone.DesertC, ABelt.Alvar,    LRegion.Polar),
            new HoldridgeZone(LZone.WTundra, ABelt.Alpine,   LRegion.SPolar),
            new HoldridgeZone(LZone.WForest, ABelt.SAlpine,  LRegion.Boreal),
            new HoldridgeZone(LZone.WForest, ABelt.Montane,  LRegion.CTemp),
            new HoldridgeZone(LZone.WForest, ABelt.Montane,  LRegion.CTemp),
            new HoldridgeZone(LZone.WForest, ABelt.LMontane, LRegion.WTemp),
            new HoldridgeZone(LZone.WForest, ABelt.PMontane, LRegion.Tropic)
        },
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DesertC, ABelt.Alvar,    LRegion.Polar),
            new HoldridgeZone(LZone.MTundra, ABelt.Alpine,   LRegion.SPolar),
            new HoldridgeZone(LZone.MForest, ABelt.SAlpine,  LRegion.Boreal),
            new HoldridgeZone(LZone.MForest, ABelt.Montane,  LRegion.CTemp),
            new HoldridgeZone(LZone.MForest, ABelt.LMontane, LRegion.WTemp),
            new HoldridgeZone(LZone.MForest, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.MForest, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.MForest, ABelt.PMontane, LRegion.Tropic)
        },
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DTundra,  ABelt.Alpine,   LRegion.SPolar),
            new HoldridgeZone(LZone.DryScb,      ABelt.SAlpine,  LRegion.Boreal),
            new HoldridgeZone(LZone.Steppe,   ABelt.Montane,  LRegion.CTemp),
            new HoldridgeZone(LZone.DForest,  ABelt.LMontane, LRegion.WTemp),
            new HoldridgeZone(LZone.DForest,  ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DForest,  ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DForest,  ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DForest,  ABelt.PMontane, LRegion.Tropic)
        },
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DesertH,   ABelt.SAlpine,  LRegion.Boreal),
            new HoldridgeZone(LZone.DesertScb, ABelt.Montane,  LRegion.CTemp),
            new HoldridgeZone(LZone.TSWL,      ABelt.LMontane, LRegion.WTemp),
            new HoldridgeZone(LZone.VDForest,  ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.VDForest,  ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.VDForest,  ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.VDForest,  ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.VDForest,  ABelt.PMontane, LRegion.Tropic)
        },
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DesertH,   ABelt.Montane,  LRegion.CTemp),
            new HoldridgeZone(LZone.DesertScb, ABelt.LMontane, LRegion.WTemp),
            new HoldridgeZone(LZone.TWL,       ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.TWL,       ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.TWL,       ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.TWL,       ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.TWL,       ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.TWL,       ABelt.PMontane, LRegion.Tropic)
        },
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DesertH,   ABelt.LMontane, LRegion.WTemp),
            new HoldridgeZone(LZone.DesertScb, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertScb, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertScb, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertScb, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertScb, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertScb, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertScb, ABelt.PMontane, LRegion.Tropic)
        },
        new HoldridgeZone[] {
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic),
            new HoldridgeZone(LZone.DesertH, ABelt.PMontane, LRegion.Tropic)
        }
    };

    public HoldridgeZone GetHoldridgeZone(
        float temperature,
        float moisture
    ) {
        temperature = temperature * 7f; // normalize the temperature value
                                        // to the length of the temperature
                                        // axis


        moisture = moisture * 7f; // normalize the moisture value to the 
                                  // length of the moisture axis

        // Get temperature and moisture indexes by flooring the normalized
        // values.
        int moistureIndex = (int)Mathf.Floor(moisture);
        int temperatureIndex = (int)Mathf.Floor(temperature);

        // provide temperature and moisture as approximations of
        // evaportation and precipitation
        return holdridgeZones[temperatureIndex][moistureIndex];
    }
}

[System.Serializable]
public struct HoldridgeZone {
    public LZone lifeZone;
    public ABelt altitudinalBelt;
    public LRegion latitudinalRegion;
    public HoldridgeZone(
        LZone lifeZone,
        ABelt altitudinalBelt,
        LRegion latitudinalRegion
    ) {
        this.lifeZone = lifeZone;
        this.altitudinalBelt = altitudinalBelt;
        this.latitudinalRegion = latitudinalRegion;
    }

    public override string ToString() {
        return
            "Life Zone: " + lifeZone +
            ", Alt. Belt: " + altitudinalBelt +
            ", Lat. Region: " + latitudinalRegion; 
    }
}

public enum LZone {
    /// <summary>
    /// Hot Desert
    /// </summary>
    DesertH = 0,

    /// <summary>
    /// Cold Desert
    /// </summary>
    DesertC = 1,
    
    /// <summary>
    /// Dry Tundra
    /// </summary>
    DTundra = 2,
    
    /// <summary>
    /// Desert Scrub
    /// </summary>
    DesertScb = 3,
    
    /// <summary>
    /// DryScrub
    /// </summary>
    DryScb = 4,
    
    /// <summary>
    /// Moist Tundra
    /// </summary>
    MTundra = 5,
    
    /// <summary>
    /// Thorn Woodland
    /// </summary>
    TWL = 6,
    
    /// <summary>
    /// Thorn-Steppe Woodland
    /// </summary>
    TSWL = 7,
    
    /// <summary>
    /// Steppe
    /// </summary>
    Steppe = 8,
    
    /// <summary>
    /// Moist Forest
    /// </summary>
    MForest = 9,
    
    /// <summary>
    /// Wet Tundra
    /// </summary>
    WTundra = 10,
    
    /// <summary>
    /// Very Dry Forest
    /// </summary>
    VDForest = 11,
    
    /// <summary>
    /// Dry Forest
    /// </summary>
    DForest = 12,
    
    /// <summary>
    /// Wet Forest
    /// </summary>
    WForest = 13,
    
    /// <summary>
    /// Rain Tundra
    /// </summary>
    RTundra = 14,
    
    /// <summary>
    /// Rain Forest
    /// </summary>
    RForest = 15
}

public enum ABelt {
    /// <summary>
    /// Pre-Montane
    /// </summary>
    PMontane = 0,
    
    /// <summary>
    /// Lower Montane
    /// </summary>
    LMontane = 1,

    /// <summary>
    /// Montane
    /// </summary>
    Montane = 2,

    /// <summary>
    /// Sub-Alpine
    /// </summary>
    SAlpine = 3,

    /// <summary>
    /// Alpine
    /// </summary>
    Alpine = 4,

    /// <summary>
    /// Alvar
    /// </summary>
    Alvar = 5
};

public enum LRegion {
    ///<summary>
    /// Polar
    ///</summary>
    Polar = 0,
    ///<summary>
    /// Sub-Polar
    ///</summary>
    SPolar = 1,
     ///<summary>
    /// Boreal
    ///</summary>
    Boreal = 2,
    ///<summary>
    /// Cool Temperate
    ///</summary>
    CTemp = 3,
    ///<summary>
    /// Warm Temperate
    ///</summary>
    WTemp = 4,
    ///<summary>
    /// Sub-Tropical
    ///</summary>
    STropical = 5,
    ///<summary>
    /// Tropical
    ///</summary>
    Tropic = 6
}