using UnityEngine;
using System.Collections.Generic;

public class HexClimate {
    public enum Terrains {
        Sand = 0,
        Grass = 1,
        Mud = 2,
        Stone = 3,
        Snow = 4
    }
/// <summary>
/// A collection of structs defining the current climate data.
/// </summary>
/// TODO: 
///     Indirect coupling between _climate and HexGrid in GenerateClimate() and
///     StepClimate() requiring both the list of HexCells in HexGrid and _climate
///     to maintain the same order.
/// 
///     Need to either make this dependency explicit or elimnate the coupling.
/// 
///     This list is cleared and reused.
/// 
///     Might be possible to somehow elimate this list entirely by using functional
///     programming principles instead of reusing the same list.
    private List<ClimateData> _climate = new List<ClimateData>();

/// <summary>
/// A collection of structs defining the next climate data.
/// </summary>
/// TODO: 
///     Indirect coupling between _nextClimate and HexGrid in GenerateClimate() and
///     StepClimate() requiring both the list of HexCells in HexGrid and _nextClimate
///     to maintain the same order.
/// 
///     Need to either make this dependency explicit or elimnate the coupling.
///     
///     This list is cleared and reused.
/// 
///     Might be possible to somehow elimate this list entirely by using functional
///     programming principles instead of reusing the same list.
    private List<ClimateData> _nextClimate = new List<ClimateData>();

/// <summary>
/// A collection of HexDirections representing possible flow directions for
///     a given river at a particular growth step.
/// </summary>
/// TODO:
///     This list is cleared and reused.
///     
///     Might be possible to somehow eliminate this list entirely by using functional
///     programming principles instead of reusing the same list.

/// <summary>
/// An integer value representing the selected noise channel to use when
///     determining temperature jitter.
/// </summary>
/// TODO:
///     This variable creates an indirect dependency between SetTerrainTypes()
///     and GenerateTemperature. Need to either make this dependency explicit
///     or eliminate the coupling.
/// 
///     This variable is reassigned.
/// 
///     Might be possible to somehow eliminate this variable entirely by
///     using functional programming principles instead of reassigning
///     the variable.
/// 
///     Extract with other climate modeling data and algorithms into a separate
///     class.
    private int _temperatureJitterChannel;

/// <summary>
///     An array of floats representing thresholds for different temperature bands.
///     Used along with moisture bands to determine the index of biomes to be used
///     for the biome of a specific cell.
/// </summary>
/// TODO:
///     Extract with other climate modeling data and algorithms into a separate
///     class.
    private static float[] temperatureBands = { 0.1f, 0.3f, 0.6f };

/// <summary>
///     An array of floats representing thresholds for different moisture bands.
///     Used along with moisture bands to determine the index of biomes to be
///     used for the biome of a specific cell.
/// </summary>
/// TODO:
///     Extract with other climate modeling data and algorithms into a separate
///     class.
    private static float[] moistureBands = { 0.12f, 0.28f, 0.85f };

    /* Array representing a matrix of biomes along the temperature bands:
        *
        *  0.1 [desert][snow][snow][snow]
        *  0.3 [desert][mud][mud, sparse flora][mud, average flora]
        *  0.6 [desert][grass][grass, sparse flora ][grass, average flora]
        *      [desert][grass, sparse flora][grass, average flora][grass, dense flora]
        *  Temperature/Moisture | 0.12 | 0.28 | 0.85
        * */
/// <summary>
///     An array of Biome structs representing a matrix of possible biomes
///     for a particular cell, indexed by its temperature and moisture.
/// </summary>
/// TODO:
///     Extract with other climate modeling data and algoritms into a
///     separate class.
    private static Biome[] biomes = {
        new Biome(Terrains.Sand, 0), new Biome(Terrains.Snow, 0), new Biome(Terrains.Snow, 0), new Biome(Terrains.Snow, 0),
        new Biome(Terrains.Sand, 0), new Biome(Terrains.Mud, 0), new Biome(Terrains.Mud, 1), new Biome(Terrains.Mud, 2),
        new Biome(Terrains.Sand, 0), new Biome(Terrains.Grass, 0), new Biome(Terrains.Grass, 1), new Biome(Terrains.Grass, 2),
        new Biome(Terrains.Sand, 0), new Biome(Terrains.Grass, 1), new Biome(Terrains.Grass, 2), new Biome(Terrains.Grass, 3)
    };

    private void SetTerrainTypes(
        RootGenConfig config,
        HexMap grid,
        NeighborGraph neighborGraph,
        RiverGraph riverGraph
    ) {
// Select the temperature jitter channel.
        _temperatureJitterChannel = Random.Range(0, 4);

// Declare the elevation threshold for rock or 
        int rockDesertElevation =
            config.elevationMax - (config.elevationMax - config.waterLevel) / 2;

        for (int i = 0; i < (grid.WidthInCells * grid.HeightInCells); i++) {
            HexCell cell = grid.GetCell(i);
            float temperature = GenerateTemperature(
                config,
                grid,
                cell,
                config.cellOuterRadius
            );

            float moisture = _climate[i].moisture;

            if (!cell.IsUnderwater) {
                int temperatureBand = 0;

                for (; temperatureBand < temperatureBands.Length; temperatureBand++) {
                    if (temperature < temperatureBands[temperatureBand]) {
                        break;
                    }
                }

                int moistureBand = 0;

                for (; moistureBand < moistureBands.Length; moistureBand++) {
                    if (moisture < moistureBands[moistureBand]) {
                        break;
                    }
                }

                Biome cellBiome = biomes[temperatureBand * 4 + moistureBand];

                if (cellBiome.terrain == 0) {
                    if (cell.Elevation >= rockDesertElevation) {
                        cellBiome.terrain = Terrains.Stone;
                    }
                }
                else if (cell.Elevation == config.elevationMax) {
                    cellBiome.terrain = Terrains.Snow;
                }

                if (cellBiome.terrain == Terrains.Snow) {
                    cellBiome.plant = 0;
                }

//                if (cellBiome.plant < 3 && cell.HasRiver) {
                if (riverGraph.HasRiver(cell)) {
                    cellBiome.plant += 1;
                }

                cell.TerrainTypeIndex = (int)cellBiome.terrain;
                cell.PlantLevel = cellBiome.plant;
            }
            else {
                Terrains terrain;

                if (cell.Elevation == config.waterLevel - 1) {
                    int cliffs = 0;
                    int slopes = 0;

//                    for
//                    (
//                        HexDirection direction = HexDirection.Northeast;
//                        direction <= HexDirection.Northwest;
//                        direction++
                    foreach(
                        HexCell neighbor in
                        neighborGraph.Neighbors(cell)
                    ) {
//                        HexCell neighbor = cell.GetNeighbor(direction);

//                        if (!neighbor) {
//                            continue;
//                        }

                        int delta = neighbor.Elevation - cell.WaterLevel;

                        if (delta == 0) {
                            slopes += 1;
                        }
                        else if (delta > 0) {
                            cliffs += 1;
                        }
                    }

                    /*More than half neighbors at same level.
                        *Inlet or lake, therefore terrain is grass.
                        */
                    if (cliffs + slopes > 3) {
                        terrain = Terrains.Grass;
                    }
                    // More than half cliffs, terrain is stone.
                    else if (cliffs > 0) {
                        terrain = Terrains.Stone;
                    }
                    // More than half slopes, terrain is beach.
                    else if (slopes > 0) {
                        terrain = Terrains.Sand;
                    }
                    // Shallow non-coast, terrain is grass.
                    else {
                        terrain = Terrains.Grass;
                    }
                }
                else if (cell.Elevation >= config.waterLevel) {
                    terrain = Terrains.Grass;
                }
                else if (cell.Elevation < 0) {
                    terrain = Terrains.Stone;
                }
                else {
                    terrain = Terrains.Mud;
                }

                /* Coldest temperature band produces mud instead of
                    * grass.
                    */
                if (terrain == Terrains.Grass && temperature < temperatureBands[0]) {
                    terrain = Terrains.Mud;
                }

                cell.TerrainTypeIndex = (int)terrain;
            }
        }
    }

    private void GenerateClimate(
        RootGenConfig config,
        HexMap grid,
        NeighborGraph neighborGraph
    ) {
        _climate.Clear();
        _nextClimate.Clear();

        ClimateData initialData = new ClimateData();
        initialData.moisture = config.startingMoisture;

        ClimateData clearData = new ClimateData();

        for (int i = 0; i < (grid.WidthInCells * grid.HeightInCells); i++) {
            _climate.Add(initialData);
            _nextClimate.Add(clearData);
        }

        for (int cycle = 0; cycle < 40; cycle++) {
            for (
                int i = 0;
                i < (grid.WidthInCells * grid.HeightInCells);
                i++
            ) {
                StepClimate(
                    config,
                    grid,
                    i,
                    neighborGraph
                );
            }

            /* Make sure that the climate data being calculated in the current
                * cycle is always from the current climate and not the next climate.
                */

            // Store the modified climate data in swap.
            List<ClimateData> swap = _climate;

            // Store the cleared climate data in the current climate.
            _climate = _nextClimate;

            // Store the modified climate data in next climate
            _nextClimate = swap;
        }
    }

    private void StepClimate(
        RootGenConfig config,
        HexMap grid,
        int cellIndex,
        NeighborGraph neighborGraph
    ) {
        HexCell cell = grid.GetCell(cellIndex);
        ClimateData cellClimate = _climate[cellIndex];

        if (cell.IsUnderwater) {
            cellClimate.moisture = 1f;
            cellClimate.clouds += config.evaporationFactor;
        }
        else {
            float evaporation = cellClimate.moisture * config.evaporationFactor;
            cellClimate.moisture -= evaporation;
            cellClimate.clouds += evaporation;
        }

        float precipitation = cellClimate.clouds * config.precipitationFactor;
        cellClimate.clouds -= precipitation;
        cellClimate.moisture += precipitation;

        // Cloud maximum has an inverse relationship with elevation maximum.
        float cloudMaximum = 1f - cell.ViewElevation / (config.elevationMax + 1f);

        if (cellClimate.clouds > cloudMaximum) {
            cellClimate.moisture += cellClimate.clouds - cloudMaximum;
            cellClimate.clouds = cloudMaximum;
        }

        HexDirection mainDispersalDirection = config.windDirection.Opposite();

        float cloudDispersal = cellClimate.clouds * (1f / (5f + config.windStrength));
        float runoff = cellClimate.moisture * config.runoffFactor * (1f / 6f);
        float seepage = cellClimate.moisture * config.seepageFactor * (1f / 6f);

//        for (
//            HexDirection direction = HexDirection.Northeast;
//            direction <= HexDirection.Northwest;
//            direction++
        foreach (HexEdge edge in neighborGraph.Edges(cell)) {
//            HexCell neighbor = cell.GetNeighbor(direction);

//            if (!neighbor) {
//                continue;
//            }

            ClimateData neighborClimate = _climate[edge.Target.Index];

            if (edge.Direction == mainDispersalDirection) {
                neighborClimate.clouds += cloudDispersal * config.windStrength;
            }
            else {
                neighborClimate.clouds += cloudDispersal;
            }

            int elevationDelta = edge.Target.ViewElevation - cell.ViewElevation;

            if (elevationDelta < 0) {
                cellClimate.moisture -= runoff;
                neighborClimate.moisture += runoff;
            }
            else if (elevationDelta == 0) {
                cellClimate.moisture -= seepage;
                neighborClimate.moisture += seepage;
            }

            _climate[edge.Target.Index] = neighborClimate;
        }

        // Create a cell for the next climate.
        ClimateData nextCellClimate = _nextClimate[cellIndex];

        // Modify the data for the next climate.
        nextCellClimate.moisture += cellClimate.moisture;

        /* Ensure that no cell can have more moisture than
            *a cell that is underwater.
            */
        if (nextCellClimate.moisture > 1f) {
            nextCellClimate.moisture = 1f;
        }

        //Store the data for the next climate.
        _nextClimate[cellIndex] = nextCellClimate;

        //Clear the current climate data.
        _climate[cellIndex] = new ClimateData();
    }

    private float GenerateTemperature(
        RootGenConfig config, 
        HexMap grid, 
        HexCell cell,
        float cellOuterRadius
    ) {
        float latitude = (float)cell.Coordinates.Z / grid.HeightInCells;

        if (config.hemisphere == HemisphereMode.Both) {
            latitude *= 2f;

            if (latitude > 1f) {
                latitude = 2f - latitude;
            }
        }
        else if (config.hemisphere == HemisphereMode.North) {
            latitude = 1f - latitude;
        }

        float temperature =
            Mathf.LerpUnclamped(
                config.lowTemperature,
                config.highTemperature,
                latitude
            );

        temperature *= 
            1f - 
            (cell.ViewElevation - config.waterLevel) /
            (config.elevationMax - config.waterLevel + 1f);

        float jitter =
            HexagonPoint.SampleNoise(
                cell.Position * 0.1f,
                cellOuterRadius,
                grid.IsWrapping,
                grid.WrapSize
            )[_temperatureJitterChannel];

        temperature += (jitter * 2f - 1f) * config.temperatureJitter;

        return temperature;
    }

    private struct ClimateData {
        public float clouds;
        public float moisture;
    }

    private struct Biome {
/// <summary>
/// The terrain type of the biome.
/// </summary>
        public Terrains terrain;

/// <summary>
/// The level of vegetation in the biome.
/// </summary>
        public int plant;

/// <summary>
/// Constructor.
/// </summary>
/// <param name="terrain">The terrain type of the biome.</param>
/// <param name="plant">The plant level of the biome.</param>
        public Biome(Terrains terrain, int plant) {
            this.terrain = terrain;
            this.plant = plant;
        }
    }
}