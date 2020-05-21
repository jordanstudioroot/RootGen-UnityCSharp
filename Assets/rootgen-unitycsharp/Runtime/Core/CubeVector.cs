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
    public int X {
        get
        {
            return x;
        }
    }

    public int XOffset {
        get {
            return (x + z) * 2;
        }
    }

/// <summary>
/// The first laditudinal axis of the vector in the cube coordinate
/// system.
/// </summary>
/// <value></value>
    public int Z {
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
    public int Y {
        get {
//  X + Y + Z = 0->
//  X + Z = -Y ->
//  (X + Z) / -1 = -Y / -1 ->
//  -X - Z = Y
            return -x - z;
        }
    }

//  [x, z, y]
//
//     NW                   NE
//      [-1, 0, 1][ 0, -1, 1]
// W [-1, 1, 0][0, 0, 0][1, -1, 0] E
//      [0, 1, -1][1, 0, -1]
//     SW                   SE
    public static CubeVector Northeast {
        get {
            return new CubeVector(0, 1);
        }
    }

    public static CubeVector East {
        get {
            return new CubeVector(1, 0);
        }
    }

    public static CubeVector Southeast {
        get {
            return new CubeVector(1, -1);
        }
    }

    public static CubeVector Southwest {
        get {
            return new CubeVector(0, -1);
        }
    }

    public static CubeVector West {
        get {
            return new CubeVector(-1, 0);
        }
    }

    public static CubeVector Northwest {
        get {
            return new CubeVector(-1, 1);
        }
    }

    public CubeVector(int x, int z) {
        this.x = x;
        this.z = z;
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

    public CubeVector Minus(CubeVector other) {
        CubeVector result = new CubeVector(
            X - other.X,
            Z - other.Z
        );
        
        return result;
    }

    public CubeVector Plus(CubeVector other) {
        CubeVector result =  new CubeVector(
            x + other.X,
            Z + other.Z
        );

        return result;
    }

    public CubeVector Cross(CubeVector other) {
        throw new NotImplementedException();
    }

    public CubeVector Dot(CubeVector other) {
        throw new NotImplementedException();
    }

    public CubeVector Scale(int scalar) {
        return new CubeVector(
            x * scalar,
            z * scalar
        );
    }

    public static CubeVector operator - (CubeVector a, CubeVector b) {
        return a.Minus(b);
    }

    public static CubeVector operator + (CubeVector a, CubeVector b) {
        return a.Plus(b);
    }

    public static CubeVector operator * (CubeVector a, CubeVector b) {
        return a.Dot(b);
    }

    public static CubeVector CrossProduct (CubeVector a, CubeVector b) {
        return a.Cross(b);
    }

    public static CubeVector Scale(CubeVector vector, int scalar) {
        return vector.Scale(scalar);
    }

    public CubeVector Normalized {
        get {
            int magnitude = (int)Mathf.Sqrt(
                Mathf.Pow(x, 2) +
                Mathf.Pow(Y, 2) +
                Mathf.Pow(z, 2) 
            );

            int xNorm = ClampToHighestAbsoluteValue(x) / magnitude;
            int zNorm = ClampToHighestAbsoluteValue(z) / magnitude;

            return new CubeVector(
                xNorm,
                zNorm
            );
        }
    }

    public int ClampToHighestAbsoluteValue(float toClamp) {
        return 
            toClamp > 0 ?
                Mathf.CeilToInt(toClamp) :
                Mathf.FloorToInt(toClamp);
    }

    public static CubeVector Direction(
        CubeVector from,
        CubeVector to
    ) {
        return (to - from).Normalized;
    }

    public static CubeVector Direction(
        CubeVector from,
        CubeVector to,
        int wrapping
    ) {
        if (
            IsWrappingRight(
                from, to, wrapping
            )
        ) {
            CubeVector toWrapRight =
                new CubeVector(
                    to.X + wrapping,
                    to.Z
                );

            return Direction(from, toWrapRight);
        }
        else if (
            IsWrappingLeft(
                from, to, wrapping
            )
        ) {
            CubeVector toWrapLeft =
                new CubeVector(
                    to.X - wrapping,
                    to.Z
                );
            
            return Direction(from, toWrapLeft);
        }

        return Direction(from, to);
    }

    private static HexDirections HexDirectionInternal(CubeVector vector) {
        if (vector.Equals(Northeast))
            return HexDirections.Northeast;
        
        if (vector.Equals(Northwest))
            return HexDirections.Northwest;
        
        if (vector.Equals(Southeast))
            return HexDirections.Southeast;

        if (vector.Equals(Southwest))
            return HexDirections.Southwest;

        if (vector.Equals(East))
            return HexDirections.East;
        
        if (vector.Equals(West))
            return HexDirections.West;

        throw new NotImplementedException(
            "There is no matching hex direction for the specified " +
            "cube vector:\n\t" + vector
        );
    }

    public static HexDirections HexDirection(
        CubeVector from,
        CubeVector to
    ) {
        CubeVector direction = Direction(from, to);
        return HexDirectionInternal(direction);        
    }

    public static HexDirections HexDirection(
        CubeVector from,
        CubeVector to,
        int wrapping
    ) {
        CubeVector direction = Direction(from, to, wrapping);
        return HexDirectionInternal(direction);
    }

    public static CubeVector FromPosition(
        Vector3 position,
        float outerRadius,
        int wrapSize
    ) {
        float innerDiameter =
            HexagonPoint.OuterToInnerRadius(outerRadius) * 2f;
            
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

/// <summary>
///     Create cube vector from offset coordinates.
/// </summary>
/// <param name="x">
///     The x axis.
/// </param>
/// <param name="z">
///     The z (offset) axis.
/// </param>
/// <param name="wrapSize">
///     The wrap size for the coordinates if the coordinate system
///     is wrapping.
/// </param>
/// <returns></returns>
    public static CubeVector FromOffsetCoordinates(
        int x,
        int z,
        int wrapSize
    ) {
// Return coordinates after subtracting the X coordinate with the Z
// coordinate integer divided by 2.
// 
// All hexes will be offset on the X axis directly proportional to Z. As
// Z grows larger, the magnitude of the offset increases bringing the X
// axis into alignment with a proposed axis which is at a (roughly) 45
// degree angle with the Z axis.
        return new CubeVector(
            x - z / 2,
            z,
            wrapSize
        );
    }

/// <summary>
/// Get the distance in hex tiles between two cube vectors representing
/// tiles on a hex grid.
/// </summary>
/// <param name="source">
/// A cube vector representing the source tile.
/// </param>
/// <param name="target">
/// A cube vector representing the target tile.
/// </param>
/// <returns>
/// The distance in hex tiles between the source tile and the target tile.
/// </returns>
    public static int HexTileDistance(CubeVector source, CubeVector target) {
        int result =  
            (int)Mathf.Floor(
                CubicDistance(source, target)
            );

        return result;
    }

    private static bool IsWrappingRight(
        CubeVector source,
        CubeVector target,
        int wrapSize
    ) {
        if (wrapSize < 0)
            throw new ArgumentException(
                "The wrap size must be greater than or equal to 0."
            );

        int unwrappedDistance = HexTileDistance(source, target);

        CubeVector rightWrapped =
            new CubeVector(
                target.x + wrapSize,
                target.z
            );

        int rightWrappedDistance = HexTileDistance(source, rightWrapped);

        if (rightWrappedDistance < unwrappedDistance)
            return true;

        return false;
    }

    private static bool IsWrappingLeft(
        CubeVector source,
        CubeVector target,
        int wrapSize
    ) {
        if (wrapSize < 0)
            throw new ArgumentException(
                "The wrap size must be greater than or equal to 0."
            );

        int unwrappedDistance = HexTileDistance(source, target);

        CubeVector leftWrapped =
            new CubeVector(
                target.x - wrapSize,
                target.z
            );

        int rightWrappedDistance = HexTileDistance(source, leftWrapped);

        if (rightWrappedDistance < unwrappedDistance)
            return true;

        return false;
    }

    public static int HexTileDistance(
        CubeVector source,
        CubeVector target,
        int wrapSize
    ) {
        if (wrapSize < 0)
            throw new ArgumentException(
                "The wrap size must be greater than or equal to 0."
            );

        int unwrappedDistance = HexTileDistance(source, target);

        CubeVector rightWrapped =
            new CubeVector(
                target.x + wrapSize,
                target.z
            );

        int rightWrappedDistance = HexTileDistance(source, rightWrapped);

        if (rightWrappedDistance < unwrappedDistance)
            return rightWrappedDistance;

        CubeVector leftWrapped =
            new CubeVector(
                target.x - wrapSize,
                target.z
            );

        int leftWrappedDistance = HexTileDistance(source, leftWrapped);

        if (leftWrappedDistance < unwrappedDistance)
            return leftWrappedDistance;

        return unwrappedDistance;
    }

/// <summary>
/// Get the cubic distance between two cube vectors.
/// </summary>
/// <param name="source">
///     The source cube vector.
/// </param>
/// <param name="target">
///     The target cube vector.
/// </param>
/// <returns>
///     The raw distance between two cube vectors using cube coordinates.
/// </returns>
    public static float CubicDistance(CubeVector source, CubeVector target) {
        return Mathf.Sqrt(
            Mathf.Pow(source.x - target.x, 2f) +
            Mathf.Pow(source.Y - target.Y, 2f) +
            Mathf.Pow(source.z - target.z, 2f)
        );
    }

/// <summary>
/// Get the axial distance between two cube vectors.
/// </summary>
/// <param name="source">
/// The source cube vector.
/// </param>
/// <param name="target">
/// The target cube vector.
/// </param>
/// <returns>
/// The axial distance between two cube vectors.
/// </returns>
    public float OffsetDistance(CubeVector source, CubeVector target) {
        throw new NotImplementedException();
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

    public static Vector2 CubeToOffset(int cubeX, int cubeY, int cubeZ) {
        return new Vector2(
            cubeX + (cubeZ - (cubeZ % 2)) / 2,
            cubeZ
        );
    }
 }
