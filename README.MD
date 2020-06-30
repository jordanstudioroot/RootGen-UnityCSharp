## RootGen

A procedural 3D landscape generator for Unity3D, based with permission on Jasper Flick's [hex map](https://catlikecoding.com/unity/tutorials/hex-map/) series on [Catlike Coding](https://catlikecoding.com/).

The goal of RootGen is to provide an implementation of hex map generation in Unity3D that is as implementation-agnostic as possible, allowing developers to provide a set of generation parameters in whichever format that they choose, render the resulting hex map, and retrieve data about the map in whichever format they find to be the most useful (graph, matrix, list, etc).

Currently, RootGen only supports providing parameters using a scriptable object, and retrieving the map as a custom matrix class. See todo for a prioritized list of planned features.

## Implementation

### Climate

RootGen uses a simplified version of the [holdridge life zones](https://en.wikipedia.org/wiki/Holdridge_life_zones) to generate the climate of individual hexes.

### Topography

RootGen creates elevation regions based on provided parameters, using these overlapping elevation regions in combination with other elevation parameters such as maximum and minimum elevation to generate a semi-realistic simulation of tectonic shift.

### Weather

RootGen uses a simplified model of atmospheric moisture propagation to determine the movement of clouds and precipitation.

## Requirements

Initialize new or existing Unity3D project as npm package.

## Installation and Usage

cd to new or existing project directory initialized with NPM and `npm install jordanstudioroot/RootGen-UnityCSharp`, or clone this repository into the Assets folder and link the root assembly definition in your projects assembly definition. If you don't want to use the assembly definitions, simply delete all .asmdef files in the directory.

### Instantiating RootGen.

Once RootGen has been added to the project, instantiate an instance: `RootGen rootGen = new RootGen()`

### Creating a RootGenConfig

Create a new RootGenConfig scriptable object by right clicking in the project pane and clicking Create -> RootGenConfig -> Default Config.

### Description Of RootGenConfigParameters

- **Width:** The width of the map.

- **Height:** The height of the map.

- **Wrapping:** Should the map wrap?

- **Use Fixed Seed:** Checking this box will keep the random seed the same when regenerating maps using the same instance of RootGen. Check this if you would not like the randomized values of the map to change when it is regenerated.  

- **Seed:** Allows the specification of a custom random seed. Use in combination with "Use Fixed Seed" to keep the provided value from changing when regenerating the map with a single instance of RootGen.

- **Initial Climate Steps:** The number of iterations that the climate generation algorithm with run through before stopping. Providing higher values results in a longer period of climate simulation.

- **Hex Size:** The size of the individual hexes on the map. Providing larger values makes the individual hexes larger. Useful for scaling the map to fit the scale of assets such as characters and buildings.

- **Jitter Probability:** The probability that the region generation algorithm will "jitter". A higher Jitter Probability will result in more abnormal region formations.

- **Minimum Region Density:** The minimum density of a elevation region. Higher values will result in the smallest regions having more broad variations in elevation.

- **Maximum Region Density:** The maximum density of an elevation region. Higher values will result in the largest regions having more broad variations in elevation.

- **Land Percentage:** The percentage overall land in the map. Higher values will result in smaller bodies of water and larger landmasses.

- **Water Level:** The global water level.

- **High Rise Probability:** The probability that a cliff will be generated. Higher values will result in more mountainous terrain.

- **Sink Probability:** The probability that terrain will "sink". Higher values will result in more valleys.

- **Elevation Min:** The lowest elevation permitted in the map.

- **Elevation Max:** The highest elevation permitted in the map.

- **Map Border X:** The size of the longitudinal water border of the map. Larger values will result in terrain being "squeezed in" along the longitudinal axis.

- **Map Border Z:** The size of the latitudinal water border of the map. Larger values will result in terrain being "squeezed in" along the latitudinal axis.

- **Region Border:** The border between elevation regions. Larger values will result in larger gaps between elevation regions.

- **Num Regions:** The number of elevation regions. The provided value will be clamped to an even value if odd. Larger values will result in a larger grid of regions.

- **Erosion Percentage:** The percent of wind erosion to be applied to the map. Larger values will result in smoother terrain globally.

- **Evaporation Factor:** The percent of evaporation to be applied to the map. Larger values will result in drier terrain and a larger incidence of desert regions.

- **Precipitation Factor:** The percent of precipitation to be applied to the map. Larger values will result in more rivers and lakes.

- **Runoff Factor:** The percent of runoff to be applied to the map. Larger values will result in more rivers occurring as a result of runoff calculations.

- **Seepage Factor:** The percent of seepage to be applied to the map. Larger values will result in hexes obtaining more moisture from their neighbors as a result of moisture seepage from adjacent tiles with a high amount of moisture.

- **Wind Direction:** The global direction of the wind. Erosion will occur in this direction, and precipitation will propagate around mountains in the opposite direction.

- **Wind Strength:** The strength of the wind. Determines how far precipitation propagates and how much erosion occurs.

- **Starting Moisture:** The global starting moisture for all hex tiles. The higher this value is, the "wetter" the map will be.

- **Extra Lake Probability:** The probability that rivers will generate lakes.

- **Low Temperature:** The minimum temperature of the map. Higher values will result in the coldest hexes being hotter.

- **High Temperature:** The maximum temperature of the map. Lower values will result in the hottest hexes being colder.

- **Temperature Jitter:** The amount that the temperature algorithm will "jitter". Higher values will result in more chaotic variations in climate.

- **Num Initial Rivers:** The number of initial rivers.

- **Num Initial River Steps:** The number of steps that the river generation algorithm will initially take to generate the rivers. Higher values will result in rivers being longer initially.

- **Hemisphere:** The hemisphere of the map, either north, south, or both. North and south will simulate their corresponding hemisphere, with a cold pole on either the top of bottom of the map. Both will result in both hemispheres being simulated, with a north an south pole.

## TODO

- [x] Finish extracting RootEdit to separate package.
- [ ] Separate data generation and rendering into separate libraries or packages.
- [ ] Tests

## Criticisms, Suggestions, Pull Requests

Email: [jordannelson@protonmail.com](mailto:jordannelson@protonmail.com)

## Bugs

Report on [issues](https://github.com/jordanstudioroot/PROJECT_NAME/issues).
