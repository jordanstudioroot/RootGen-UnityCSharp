using UnityEngine;

public static class HexMetrics
{
    public const float outerToInner = 0.866025404f;
    public const float innerToOuter = 1f / outerToInner;

    /* A hexagon is composed of equilateral triangles, therefore the outer radius of any
        * hexagon is equal to the length of its edges.*/
    public const float outerRadius = edgeLength;

    /* Take one of the six triangles of a hexagon. The inner radius is equal to the height
        * of this triangle. You get this height by splitting the triangle into two right
        * triangles, then you can use the Pythagorean theorem:
        *          (len_adjacent^2 * len_opposite^2 = len_hypotenuse^2)
        * to derive the following equation:
        *          let e be edge length
        *          let ri be inner radius
        *          
        *          ri = sqrt(e^2 - (e/2)^2 = sqrt(3(e^2/4)) = e(sqrt(3)/2 ~= 0.886e*/
    public const float innerRadius = outerRadius * 0.866025404f;

    public const float innerDiameter = innerRadius * 2f;

    //The percentage of a hexagon which should not have its colors blended
    public const float solidFactor = 0.8f;
    //The precentage of a hexagon which should have its colors blended
    public const float blendFactor = 1f - solidFactor;
    public const float waterFactor = 0.6f;
    public const float waterBlendFactor = 1f - waterFactor;

    public const float edgeLength = 10f;
    public const float elevationStep = 3f;

    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    public const float cellPerturbStrength = 4f; //0f;
    public const float noiseScale = 0.003f;
    public const float elevationPerturbStrength = 1.5f;

    public const int chunkSizeX = 5;
    public const int chunkSizeZ = 5;

    public const float streamBedElevationOffset = -1.75f;
    public const float waterElevationOffset = -0.5f;

    public const int hashGridSize = 256;
    public const float hashGridScale = 0.25f;

    public const float wallHeight = 4f;
    public const float wallThickness = 0.75f;
    public const float wallElevationOffset = verticalTerraceStepSize;
    public const float wallTowerThreshold = 0.5f;
    public const float wallYOffset = -1;

    public const float bridgeDesignLength = 7f;

    public static Texture2D noiseSource = Resources.Load<Texture2D>("noise");

    public static int wrapSize;

    private static RootGenHash[] _hashGrid;

    public static bool Wrapping
    {
        get { return wrapSize > 0; }
    }

    /* _featureThresholds represents different thresholds for
        * different levels of development of a given feature. For a
        * given featureThresholds[n] the appearance of more pronounced
        * features increases as n decreases. Specifically,
        * _featureThresholds represents a range of values for which
        * certain features will be selected. For _featureThresholds[2],
        * the range of the most developed feature appearing is between
        * 0.4f and 0.6f.
        */
    private static float[][] _featureThresholds =
    {
        new float[] { 0.0f, 0.0f, 0.4f},
        new float[] { 0.0f, 0.4f, 0.6f},
        new float[] { 0.4f, 0.6f, 0.8f}
    };

    public static readonly Vector3[] corners =
    {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),

        /* An identical vector to corners[0] is added to prevent an IndexOutOfBounds error
            * when methods are looping over the array to draw the triangles of a hexagon. Could
            * alternatively use a circular array.*/
        new Vector3(0f, 0f, outerRadius)
    };

    public static Vector3 GetFirstCorner (HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner (HexDirection direction)
    {
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[(int)direction + 1] * solidFactor;
    }

    public static Vector3 GetSolidEdgeMiddle(HexDirection direction)
    {
        return
            (corners[(int)direction] + corners[(int)direction + 1]) *
            (0.5f * solidFactor);
    }

    public static Vector3 GetBridge (HexDirection direction)
    {
        /* Get a vector pointing from the edge of the solid region of one hexagon to the 
            * edge of the solid region of another hexagon.
            */
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
    }

    public static Vector3 TerraceLerp (Vector3 vertexA, Vector3 vertexB, int step)
    {
        /* Set the horizontal magnitude of the step to value of the nth step, multiplied by the
            * horizontal step size
            */
        float horiztonal = step * HexMetrics.horizontalTerraceStepSize;

        /* Increment the x component of vertexA by the distance between the x component of
            * vertexB and vertexA multiplied by the height of the step. Repeat for the 
            * z component.
            */
        vertexA.x += (vertexB.x - vertexA.x) * horiztonal;
        vertexA.z += (vertexB.z - vertexA.z) * horiztonal;

        /* Increment the vertical component of vertexA only at odd steps by using
            * integer divison by 2.
            */
        float vertical = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
        vertexA.y += (vertexB.y - vertexA.y) * vertical;
        return vertexA;
    }

    public static Color TerraceLerp (Color colorA, Color colorB, int step)
    {
        // Interpolate the color as if the terrace is actually flat.
        float horizontal = step * HexMetrics.horizontalTerraceStepSize;
        return Color.Lerp(colorA, colorB, horizontal);
    }

    public static EdgeType GetEdgeType(int elevationA, int elevationB)
    {
        if (elevationA == elevationB)
        {
            return EdgeType.Flat;
        }

        int delta = elevationB - elevationA;

        if (delta == 1 || delta == -1)
        {
            return EdgeType.Slope;
        }

        return EdgeType.Cliff;
    }

    public static Vector4 SampleNoise (Vector3 position)
    {
        // Texture is small, so will need to interpolate extra texels to avoid distortion.
        Vector4 sample = noiseSource.GetPixelBilinear
        (
            position.x * noiseScale, 
            position.z * noiseScale
        );

        /* Scale the wrapping transition up by half, and then move it half
            * a cell width to the left to avoid seams where the cell coordinates
            * are negative.
            */
        if (Wrapping && position.x < innerDiameter * 1.5f)
        {
            Vector4 sample2 = noiseSource.GetPixelBilinear
            (
                (position.x + wrapSize * innerDiameter) * noiseScale,
                position.z * noiseScale
            );

            sample = Vector4.Lerp
            (
                sample2, sample, position.x * (1f / innerDiameter) - 0.5f
            );
        }

        return sample;
    }


    public static Vector3 Perturb(Vector3 position)
    {
        Vector4 sample = HexMetrics.SampleNoise(position);

        /* Set the range of the perturbation between -1 and 1. Because the value of the
            * noise will be between 0 and 1, sample * 2f - 1f can be no less than -1 and no
            * no greater than 1.
            */
        position.x += (sample.x * 2f - 1f) * cellPerturbStrength;
        position.z += (sample.z * 2f - 1f) * cellPerturbStrength;
        return position;
    }

    public static Vector3 GetFirstWaterCorner(HexDirection direction)
    {
        return corners[(int)direction] * waterFactor;
    }

    public static Vector3 GetSecondWaterCorner(HexDirection direction)
    {
        return corners[(int)direction + 1] * waterFactor;
    } 

    public static Vector3 GetWaterBridge(HexDirection direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) *
            waterBlendFactor;
    }

    public static void InitializeHashGrid(int seed)
    {
        _hashGrid = new RootGenHash[hashGridSize * hashGridSize];

        // Store the default state of Random
        Random.State currentState = Random.state;

        Random.InitState(seed);

        // Get random values
        for (int i = 0; i < _hashGrid.Length; i++)
        {
            _hashGrid[i] = RootGenHash.Create();
        }

        // Restore the default state of random to ensure randomness
        Random.state = currentState;
    }

    public static RootGenHash SampleHashGrid(Vector3 position)
    {
        /* Modulo the input values to make them wrap around
            * the indices of the hash World. The smaller the
            * hashGridScale value is, the less dense the
            * unique values. For example, a value of 0.25f produces
            * a unique value every 4 units square.
            */
        int x = (int)(position.x * hashGridScale) % hashGridSize;
        if (x < 0)
        {
            x += hashGridSize;
        }
        int z = (int) (position.z * hashGridScale) % hashGridSize;
        if (z < 0)
        {
            z += hashGridSize;
        }

        return _hashGrid[x + z * hashGridSize];
    }

    public static float[] GetFeatureThresholds(int level)
    {
        return _featureThresholds[level];
    }

    public static Vector3 WallThicknessOffset(Vector3 near, Vector3 far)
    {
        Vector3 offset;
        offset.x = far.x - near.x;
        offset.y = 0f;
        offset.z = far.z - near.z;
        return offset.normalized * (wallThickness * 0.5f);
    }

    public static Vector3 WallLerp(Vector3 near, Vector3 far)
    {
        near.x += (far.x - near.x) * 0.5f;
        near.z += (far.z - near.z) * 0.5f;

        float vertical =
            near.y < far.y ? wallElevationOffset : (1f - wallElevationOffset);

        near.y += (far.y - near.y) * vertical + wallYOffset;
        return near;
    }
}

