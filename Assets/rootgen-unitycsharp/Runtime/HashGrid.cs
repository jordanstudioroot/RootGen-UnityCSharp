using RootUtils.Randomization;
using UnityEngine;

/// <summary>
///     A class encapsulating the process of generating reproduceable
///     collections of random values for specific coordinate values.
/// </summary>
public class RandomHashGrid {
    private RandomHash[] _grid;
    private int _hashGridSize;
    private float _hashGridScale;

/// <summary>
/// Constructor.
/// </summary>
/// <param name="hashGridSize">
///     The size of the hash grid. The larger this value, the
///     less reptition of random values across a given range
///     of coordinates. To eliminate repitition this value should
///     be equal to the maximum x or y coordinate, which ever is
///     larger.
/// </param>
/// <param name="hashGridScale">
///     Density of the the unique values per unit square. A value
///     of 1 produces a unique value for every unique coordinate,
///     while a value of .25f produces a unique value every 4 unique
///     coordinates square.
/// </param>
/// <param name="seed">
///     The random seed value used to create the values in the
///     hash grid.
/// </param>
/// <param name="randomValuesPerCooridnate">
///     The number of random values stored per coordinate.
/// </param>
    public RandomHashGrid(
        int hashGridSize,
        float hashGridScale,
        int seed,
        int randomValuesPerCooridnate
    ) {
        _hashGridSize = hashGridSize;
        _hashGridScale = hashGridScale;

        _grid = new RandomHash[hashGridSize * hashGridSize];

// Snapshot the current random state and initialize new random state with seed.
        Random.State snapshot = RandomState.Snapshot(seed);

// Populate grid with RandomHash structs.
        for (int i = 0; i < _grid.Length; i++)
        {
            _grid[i] = new RandomHash(randomValuesPerCooridnate);
        }

// Restore random state from snapshot.
        Random.state = snapshot;
    }

    public RandomHash Sample(float x, float z) {

// Modulo the input values to make them wrap around
// the indices of the list of random values.
        int sampleX = (int)(x * _hashGridScale) % _hashGridSize;
        if (sampleX < 0) {
            sampleX += _hashGridSize;
        }

        int sampleZ = (int)(z * _hashGridScale) % _hashGridSize;
        if (sampleZ < 0) {
            sampleZ += _hashGridSize;
        }

        return _grid[sampleX + sampleZ * _hashGridSize];
    }
}