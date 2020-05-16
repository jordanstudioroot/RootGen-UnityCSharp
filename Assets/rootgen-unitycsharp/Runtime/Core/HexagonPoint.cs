using UnityEngine;
using RootUtils.Randomization;

public static class HexagonPoint {

//The percentage of a hexagon which should be of a single color or texture.
    public const float solidFactor = 0.8f;

// The precentage of a hexagon which should be blended with the color
// or texture of a neighboring hexagon.
    public const float blendFactor = 1f - solidFactor;

// The percentage of a hexagon which should not be submerged by an
// adjacent water hexgon.
    public const float waterFactor = 0.6f;
    public const float waterBlendFactor = 1f - waterFactor;
    public const int terracesPerSlope = 2;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);
    public const float cellPerturbStrength = 4f; //0f;
    public const float noiseScale = 0.003f;
    public const float wallThickness = 0.75f;
    public const float wallElevationOffset = verticalTerraceStepSize;
    public const float wallYOffset = -1;
    public const float elevationStep = 3f;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float elevationPerturbStrength = 1.5f;
    public const float streamBedElevationOffset = -1.75f;
    public const float waterElevationOffset = -0.5f;
    public const float wallHeight = 4f;
    public const float wallTowerThreshold = 0.5f;
    public const float bridgeDesignLength = 7f;
    public static Texture2D noiseSource = Resources.Load<Texture2D>("noise");

/// <summary>
/// The square root of the size of the hash grid to be sampled.
/// </summary>
    private const int _sqrtHashGridSize = 256;
    private const float _hashGridScale = 0.25f;
    private static RandomHash[] _hashGrid;

    public static Vector3 WallLerp(Vector3 near, Vector3 far) {
        near.x += (far.x - near.x) * 0.5f;
        near.z += (far.z - near.z) * 0.5f;

        float vertical =
            near.y < far.y ?
                wallElevationOffset :
                (1f - wallElevationOffset);

        near.y +=
            (far.y - near.y) * vertical +
            wallYOffset;
        return near;
    }

    /// <summary>
/// Return a point at a given clockwise corner of a hexagon.
/// </summary>
/// <param name="corner">
///     The desired clockwise corner.
/// </param>
/// <returns>
///     A point at the desired clockwise corner if <param name="corner">
///     is less than or equal to 6, otherwise a point at
///     <param name="corner"> % 6.
/// </returns>
    public static Vector3 GetCorner(int corner, float radius) {
        return HexagonConstants.REFERENCE_CORNERS[(int)corner % 6] * radius;
    }

/// <summary>
/// Get a point at the first clockwise corner of a given hex direction.
/// </summary>
/// <param name="direction">
///     The hex direction to be used as a frame of reference.
/// </param>
/// <returns>
///     A point corresponding to the first clockwise corner of a given
///     hex direction.
/// </returns>
    public static Vector3 GetFirstCorner(HexDirections direction, float radius) {
        return GetCorner((int)direction, radius);
    }

/// <summary>
/// Get a point at the second clockwise corner of a given hex direction.
/// </summary>
/// <param name="direction">
///     The hex direction to be used as a frame of reference.
/// </param>
/// <returns>
///     A point corresponding to the second clockwise corner of a given
///     hex direction.
/// </returns>
    public static Vector3 GetSecondCorner(HexDirections direction, float radius) {
        return GetCorner((int)direction + 1, radius);
    }


    public static Vector3 GetFirstSolidCorner(
        HexDirections direction,
        float outerRadius
    ) {
        return GetCorner((int)direction, outerRadius) * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(
        HexDirections direction,
        float outerRadius
    ) {
        return GetCorner((int)direction + 1, outerRadius) * solidFactor;
    }

    public static Vector3 GetSolidEdgeMiddle(
        HexDirections direction,
        float outerRadius
    ) {
        return (
            GetCorner((int)direction, outerRadius) +
            GetCorner((int)direction + 1, outerRadius)
        ) * (0.5f * solidFactor);
    }

    public static Vector3 GetBridge(HexDirections direction, float outerRadius) {
// Get a vector pointing from the edge of the solid region of one hexagon to the 
// edge of the solid region of another hexagon.
        return (
            GetCorner((int)direction, outerRadius) +
            GetCorner((int)direction + 1, outerRadius)
        ) * blendFactor;
    }

    public static Vector3 TerraceLerp (
        Vector3 vertexA, 
        Vector3 vertexB, 
        int step
    ) {

// Set the horizontal magnitude of the step to value of the nth step,
// multiplied by the horizontal step size.
        float horiztonal =
            step * horizontalTerraceStepSize;

// Increment the x component of vertexA by the distance between the x component of
// vertexB and vertexA multiplied by the height of the step. Repeat for the 
// z component.
        vertexA.x += (vertexB.x - vertexA.x) * horiztonal;
        vertexA.z += (vertexB.z - vertexA.z) * horiztonal;

// Increment the vertical component of vertexA only at odd steps by using
// integer divison by 2.
        float vertical =
            ((step + 1) / 2) * verticalTerraceStepSize;
        vertexA.y += (vertexB.y - vertexA.y) * vertical;

        return vertexA;
    }

    public static Color TerraceLerp (Color colorA, Color colorB, int step) {

// Interpolate the color as if the terrace is actually flat.
        float horizontal =
            step * horizontalTerraceStepSize;
        return Color.Lerp(colorA, colorB, horizontal);
    }

    public static ElevationEdgeTypes GetEdgeType(int elevationA, int elevationB) {
        if (elevationA == elevationB) {
            return ElevationEdgeTypes.Flat;
        }

        int delta = elevationB - elevationA;

        if (delta == 1 || delta == -1) {
            return ElevationEdgeTypes.Slope;
        }

        return ElevationEdgeTypes.Cliff;
    }

    public static Vector4 SampleNoise(
        Vector3 position,
        float outerRadius,
        int wrapSize
    ) {
        float innerDiameter =
            GetOuterToInnerRadius(outerRadius) * 2f;

// Texture is small, so will need to interpolate extra texels to avoid distortion.
        Vector4 sample = noiseSource.GetPixelBilinear (
            position.x * noiseScale, 
            position.z * noiseScale
        );

// Scale the wrapping transition up by half, and then move it half
// a cell width to the left to avoid seams where the cell coordinates
// are negative.
        if (
            wrapSize > 0 &&
            position.x < (outerRadius * 2f) * 1.5f
        ) {
            Vector4 sample2 = noiseSource.GetPixelBilinear (
                (position.x + wrapSize * innerDiameter) *
                noiseScale,
                position.z * noiseScale
            );

            sample = Vector4.Lerp (
                sample2, sample, position.x *
                (1f / innerDiameter) - 0.5f
            );
        }

        return sample;
    }


    public static Vector3 Perturb(
        Vector3 position,
        float outerRadius,
        int wrapSize
    ) {
        Vector4 sample = SampleNoise(
            position,
            outerRadius,
            wrapSize
        );

// Set the range of the perturbation between -1 and 1. Because the value of the
// noise will be between 0 and 1, sample * 2f - 1f can be no less than -1 and no
// no greater than 1.
        position.x +=
            (sample.x * 2f - 1f) * cellPerturbStrength;
        position.z +=
            (sample.z * 2f - 1f) * cellPerturbStrength;
        return position;
    }

    public static Vector3 GetFirstWaterCorner(HexDirections direction, float radius) {
        return GetCorner((int)direction, radius) * waterFactor;
    }

    public static Vector3 GetSecondWaterCorner(HexDirections direction, float radius) {
        return GetCorner((int)direction + 1, radius) * waterFactor;
    } 

    public static Vector3 GetWaterBridge(HexDirections direction, float radius) {
        return (
            GetCorner((int)direction, radius) +
            GetCorner((int)direction + 1, radius)
        ) * waterBlendFactor;
    }

    public static void InitializeHashGrid(int seed) {
        _hashGrid = new RandomHash[_sqrtHashGridSize * _sqrtHashGridSize];

// Store the default state of Random
        Random.State currentState = Random.state;

        Random.InitState(seed);

// Get random values
        for (int i = 0; i < _hashGrid.Length; i++)
        {
            _hashGrid[i] = new RandomHash(5);
        }

// Restore the default state of random to ensure randomness
        Random.state = currentState;
    }

    public static RandomHash SampleHashGrid(Vector3 position) {

// Modulo the input values to make them wrap around
// the indices of the hash World. The smaller the
// hashGridScale value is, the less dense the
// unique values. For example, a value of 0.25f produces
// a unique value every 4 units square.
        int x = (int)(position.x * _hashGridScale) % _sqrtHashGridSize;
        if (x < 0)
        {
            x += _sqrtHashGridSize;
        }
        int z = (int) (position.z * _hashGridScale) % _sqrtHashGridSize;
        if (z < 0)
        {
            z += _sqrtHashGridSize;
        }

        return _hashGrid[x + z * _sqrtHashGridSize];
    }

    public static Vector3 WallThicknessOffset(Vector3 near, Vector3 far) {
        Vector3 offset;
        offset.x = far.x - near.x;
        offset.y = 0f;
        offset.z = far.z - near.z;
        return offset.normalized * (wallThickness * 0.5f);
    }

    public static float GetOuterToInnerRadius(float outerRadius) {
        return HexagonConstants.OUTER_TO_INNER_RATIO * outerRadius;
    }

    public static float GetInnerToOuterRadius(float innerRadius) {
        return HexagonConstants.INNER_TO_OUTER_RATIO * innerRadius;
    }
}