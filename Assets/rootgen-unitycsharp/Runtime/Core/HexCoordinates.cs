using UnityEngine;
using System;
using System.IO;
using RootLogging;

[System.Serializable]
/// <summary>
/// A three dimensional vector representing X, Z, and Y coodinates
/// commonly used to represent the coordinates on a hex grid.
/// </summary>
public struct CubeVector {
    [SerializeField]
    private int x, z;

/// <summary>
/// The longitudinal axis of the vector in the cube coordinate system.
/// </summary>
/// <value></value>
    public int X
    {
        get
        {
            return x;
        }
    }

/// <summary>
/// The first laditudinal axis of the vector in the cube coordinate
/// system.
/// </summary>
/// <value></value>
    public int Z
    {
        get
        {
            return z;
        }
    }

/// <summary>
/// The second laditudinal axis of the vector in the cube coordinate
/// system and the inverse of the Z axis.
/// </summary>
/// <value></value>
    public int Y
    {
        get
        {

//  X + Y + Z = 0->
//  X + Z = -Y ->
//  (X + Z) / -1 = -Y / -1 ->
//  -X - Z = Y
            return -X - Z;
        }
    }

//  [x, z, y]
//
//     NW                   NE
//      [ 0,-1,+1][ 1, -1, 0]
// W [-1, 0, 1][0, 0, 0][1, 0, 1] E
//      [-1, 1, 0][ 0, 1, -1]
//     SW                   SE
    public static CubeVector Northwest(int wrapSize) {
        return FromAxialCoordinates(0, -1, wrapSize);
    }

    public static CubeVector Northeast(int wrapSize) {
        return FromAxialCoordinates(1, -1, wrapSize);
    }

    public static CubeVector East(int wrapSize) {
        return FromAxialCoordinates(1, 0, wrapSize);
    }

    public static CubeVector Southeast(int wrapSize) {
        return FromAxialCoordinates(0, 1, wrapSize);
    }

    public static CubeVector Southwest(int wrapSize) {
        return FromAxialCoordinates(-1, 1, wrapSize);
    }

    public static CubeVector West(int wrapSize) {
        return FromAxialCoordinates(-1, 0, wrapSize);
    }

    public CubeVector(int x, int z, int wrapSize) {
        if (wrapSize > 0) {
            int offsetX = x + z / 2;

            if (offsetX < 0) {
                x += wrapSize;
            }
            else if (offsetX >= wrapSize) {
                x -= wrapSize;
            }
        }

        this.x = x;
        this.z = z;
    }

    public bool IsNeighborOf(
        CubeVector other,
        int wrapSize
    ) {
        RootLog.Log(
            DistanceTo(
                other,
                wrapSize
            ).ToString(),
            Severity.Information,
            "Distance"
        );

        return (
            DistanceTo(
                other,
                wrapSize
            ) == 1
        );
    }

    public CubeVector Minus(CubeVector other, int wrapSize) {
        return new CubeVector(
            X - other.X,
            Z - other.Z,
            wrapSize
        );
    }

    public CubeVector Plus(CubeVector other, int wrapSize) {
        return new CubeVector(
            x + other.X,
            Z + other.Z,
            wrapSize
        );
    }

    public CubeVector Normalized(int wrapSize) {
        return FromAxialCoordinates(
            Mathf.Clamp(X, -1, 1),
            Mathf.Clamp(Z, -1, 1),
            wrapSize
        );
    }

    public HexDirections DirectionTo(
        CubeVector other,
        int wrapSize
    ) {
        CubeVector dir = Plus(other, wrapSize).Normalized(wrapSize);
        Debug.Log(this + " + " + other + " normalized = " + dir);

        if (dir.Equals(Northeast(wrapSize)))
            return HexDirections.Northeast;
        
        if (dir.Equals(Northwest(wrapSize)))
            return HexDirections.Northwest;
        
        if (dir.Equals(Southeast(wrapSize)))
            return HexDirections.Southeast;

        if (dir.Equals(Southwest(wrapSize)))
            return HexDirections.Southwest;

        if (dir.Equals(East(wrapSize)))
            return HexDirections.East;
        
        if (dir.Equals(West(wrapSize)))
            return HexDirections.West;

        throw new NotImplementedException();
    }

    public static CubeVector FromPosition(
        Vector3 position,
        float outerRadius,
        int wrapSize
    ) {
        float innerDiameter =
            HexagonPoint.GetOuterToInnerRadius(outerRadius) * 2f;
            
        // Divide X by the horizontal width of a hexagon.
        float x = position.x / innerDiameter;

        // The y axis is just the inverse of the x axis.
        float y = -x;

        //Shift every two rows one unit to the left.
        float offset = position.z / (outerRadius * 3f);
        x -= offset;
        y -= offset;

        int integerX = Mathf.RoundToInt(x);
        int integerY = Mathf.RoundToInt(y);

        // X + Y + Z = 0 = X + Y = -Z = -X - Y = Z

        int integerZ = Mathf.RoundToInt(-x - y);

        if (integerX + integerY + integerZ != 0)
        {
            /* As a coordinate gets further away from the center, the likelihood
                * of it producing a rounding error increases. Therefore, find the
                * largest rounding delta:
                */
            
            float deltaX = Mathf.Abs(x - integerX);
            float deltaY = Mathf.Abs(y - integerY);
            float deltaZ = Mathf.Abs(-x - y - integerZ);

            // If X has the largest rounding delta, reconstruct X from Y and Z
            if (deltaX > deltaY && deltaX > deltaZ)
            {
                integerX = -integerY - integerZ;
            }
            //If Z has the largest rounding delta, reconstruct Z from X and Y
            else if (deltaZ > deltaY)
            {
                integerZ = -integerX - integerY;
            }
        }

        return new CubeVector(
            integerX,
            integerZ,
            wrapSize
        );

    }

    public static CubeVector FromAxialCoordinates(
        int x,
        int z,
        int wrapSize
    ) {
/* Return coordinates after subtracting the X coordinate with the Z coordinated integer
* divided by 2. All cells will be offset on the X axis directly proportional to Z. As
* Z grows larger, the magnitude of the offset increases bringing the X axis into alignment
* with a proposed axis which is at a (roughly) 45 degree angle with the Z axis.*/
        return new CubeVector(
            x - z / 2,
            z,
            wrapSize
        );
    }

    public int DistanceTo(
        CubeVector other,
        int wrapSize
    ) {
// Get the cumulative absolute value of the deltas for the x and y
// hex axes 
        int xy =
            (
                x < other.x ?
                other.x - x : x - other.x
            ) +
            (
                Y < other.Y ?
                other.Y - Y : Y - other.Y
            );
// If the map is wrapping...
        if (wrapSize > 0) {

// Add the wrap size to the x axis coordinate of other hex vector. 
            other.x += wrapSize;

// Get the cumulative absolute value of the deltas for the x and y
// hex axes if the coordinates are wrapping.
            int xyWrapped =
                (
                    x < other.x ?
                    other.x - x : x - other.x
                ) +
                (
                    Y < other.Y ?
                    other.Y - Y : Y - other.Y
                );
// If the wrapping distance is less than the regular distance,
// the wrapped coordinate is closer than the regular coordinate
// so use that coordiante instead.
            if (xyWrapped < xy) {
                xy = xyWrapped;
            }
// If the wrapping distance is greater than the regular distance...
            else {
// Negate the 
                other.x -= 2 * wrapSize;
                xyWrapped =
                    (x < other.x ? other.x - x : x - other.x) +
                    (Y < other.Y ? other.Y - Y : Y - other.Y);

                if (xyWrapped < xy)
                {
                    xy = xyWrapped;
                }
            }
        }
        
        return (xy + (z < other.z ? other.z - z : z - other.z)) / 2;
    }

    public override string ToString() {
        return
            "[X: " +
            X.ToString() +
            ", Y: " +
            Y.ToString() +
            ", Z: " +
            Z.ToString() +
            "]";
    }

    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(x);
        writer.Write(z);
    }

    public static CubeVector Load(BinaryReader reader)
    {
        CubeVector coordinates;
        coordinates.x = reader.ReadInt32();
        coordinates.z = reader.ReadInt32();
        return coordinates;
    }

    public override bool Equals(object other) {
        if (other is CubeVector) {
            CubeVector otherVec = (CubeVector)other;
            if (
                otherVec.X == X &&
                otherVec.Z == Z &&
                otherVec.Y == Y
            )
                return true;
        }

        return false;
    }
}
