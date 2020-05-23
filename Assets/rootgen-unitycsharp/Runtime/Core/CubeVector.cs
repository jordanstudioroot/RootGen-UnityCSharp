using UnityEngine;
using System;
using System.IO;
using RootLogging;

[System.Serializable]
/// <summary>
/// A three dimensional vector representing X, Z, and Y coodinates
/// commonly used to represent a position on a hex grid.
/// </summary>
public struct CubeVector {
    #region ConstantFields
    #endregion

    #region Fields

    [SerializeField]
    private int _x, _z;
    
    #endregion

    #region Constuctors
    
    /// <summary>
    /// Constructor for a non-wrapping cube coordinate.
    /// </summary>
    /// <param name="x">
    /// The right diagonal longitudinal cube coordinate.
    /// </param>
    /// <param name="z">
    /// The latitudinal cube coordinate.
    /// </param>
    public CubeVector(int x, int z) {
        this._x = x;
        this._z = z;
    }

    /// <summary>
    /// Constructor for a wrapping cube coordinate.
    /// </summary>
    /// <param name="x">
    /// The right diagonal longitudinal cube coordinate.
    /// </param>
    /// <param name="z">
    /// The latitudinal cube coordinate.
    /// </param>
    /// <param name="wrapOffsetX">
    /// The offset required to wrap cube coordinates around the
    /// x axis. Should be set to the width of the bounds of the
    /// plane which the coordinate is located on.
    /// </param>
    public CubeVector(int x, int z, int wrapOffsetX) {
        if (wrapOffsetX > 0) {
            int offsetX = x + z / 2;

            if (offsetX < 0) {
                x += wrapOffsetX;
            }
            else if (offsetX >= wrapOffsetX) {
                x -= wrapOffsetX;
            }
        }

        this._x = x;
        this._z = z;
    }
    
    #endregion

    #region Finalizers(Destructors)
    #endregion

    #region Delegates
    #endregion
    
    #region Events
    #endregion

    #region Enums
    #endregion

    #region Interfaces
    #endregion

    #region Properties

    /// <summary>
    /// The right diagonal longitudinal axis of the vector in the
    /// cube coordinate system.
    /// </summary>
    public int X { get { return _x; } }

    /// <summary>
    /// The longitudinal axis in offset coordinates.
    /// </summary>
    public int XOffset {
        get {
            return (_x + _z) * 2;
        }
    }

    /// <summary>
    /// The latitudinal axis of the coordinate system.
    /// </summary>
    public int Z {
        get
        {
            return _z;
        }
    }

    /// <summary>
    /// The left diagonal longitudinal axis of the vector in the
    /// cube coordinate system. Equal to the inverse of the x axis
    /// minus the z axis.
    /// </summary>
    public int Y {
        get {
            //  X + Y + Z = 0->
            //  X + Z = -Y ->
            //  (X + Z) / -1 = -Y / -1 ->
            //  -X - Z = Y
            return -_x - _z;
        }
    }

    //  [x, z, y]
    //
    //     NW                   NE
    //      [-1, 0, 1][ 0, -1, 1]
    // W [-1, 1, 0][0, 0, 0][1, -1, 0] E
    //      [0, 1, -1][1, 0, -1]
    //     SW                   SE

    /// <summary>
    /// A cube vector pointing northeast, an alias for 
    /// new CubeVector(0, 1).
    /// </summary>
    public static CubeVector Northeast {
        get {
            return new CubeVector(0, 1);
        }
    }

    /// <summary>
    /// A cube vector pointing east, an alias for new
    /// CubeVector(1, 0).
    /// </summary>
    public static CubeVector East {
        get {
            return new CubeVector(1, 0);
        }
    }

    /// <summary>
    /// A cube vector pointing southeast, an alias for
    /// new CubeVector(1, -1).
    /// </summary>
    public static CubeVector Southeast {
        get {
            return new CubeVector(1, -1);
        }
    }

    /// <summary>
    /// A cube vector pointing southwest, an alias for
    /// new CubeVector(0, -1).
    /// </summary>
    public static CubeVector Southwest {
        get {
            return new CubeVector(0, -1);
        }
    }

    /// <summary>
    /// A cube vector pointing west, an alias for new 
    /// CubeVector(-1, 0).
    /// </summary>
    /// <value></value>
    public static CubeVector West {
        get {
            return new CubeVector(-1, 0);
        }
    }

    /// <summary>
    /// A cube vector pointing northwest, an alias for new
    /// CubeVector(-1, 1).
    /// </summary>
    /// <value></value>
    public static CubeVector Northwest {
        get {
            return new CubeVector(-1, 1);
        }
    }

    #endregion

    #region Indexers
    #endregion

    #region Operators
    
    public static CubeVector operator - (
        CubeVector a,
        CubeVector b
    ) {
        return a.Minus(b);
    }

    public static CubeVector operator + (
        CubeVector a,
        CubeVector b
    ) {
        return a.Plus(b);
    }

    public static CubeVector operator * (
        CubeVector a,
        CubeVector b
    ) {
        return a.Dot(b);
    }

    #endregion

    #region Methods

    #region Public Methods
    
    public CubeVector Minus(CubeVector other) {
        CubeVector result = new CubeVector(
            X - other.X,
            Z - other.Z
        );
        
        return result;
    }

    public CubeVector Plus(CubeVector other) {
        CubeVector result =  new CubeVector(
            _x + other.X,
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
            _x * scalar,
            _z * scalar
        );
    }

    public CubeVector Normalized {
        get {
            int magnitude = (int)Mathf.Sqrt(
                Mathf.Pow(_x, 2) +
                Mathf.Pow(Y, 2) +
                Mathf.Pow(_z, 2) 
            );

            int xNorm = ClampToHighestAbsoluteValue(_x) / magnitude;
            int zNorm = ClampToHighestAbsoluteValue(_z) / magnitude;

            return new CubeVector(
                xNorm,
                zNorm
            );
        }
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
    public float OffsetDistance(
        CubeVector source,
        CubeVector target
    ) {
        throw new NotImplementedException();
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

    public override int GetHashCode() {
    // From .NET Core Documentation:
    // Frequently, a type has multiple data fields that can
    // participate in generating the hash code. One way to generate
    // a hash code is to combine these fields using an XOR
    // operation.
        return X ^ Y ^ Z;
    }

    #endregion

    #region Public Static Methods

    /// <summary>
    /// Returns a Vector2 representing the offset coordinates
    /// corresponding to provided cube coordinates.
    /// </summary>
    /// <param name="cubeX">
    /// The right diagonal longitudinal axis of the cube coordinate.
    /// </param>
    /// <param name="cubeZ">
    /// The latitudinal axis of the cube coordinate.
    /// </param>
    /// <returns></returns>
    public static Vector2 CubeToOffset(
        int cubeX,
        int cubeZ
    ) {
        return new Vector2(
            cubeX + (cubeZ - (cubeZ % 2)) / 2,
            cubeZ
        );
    }

    /// <summary>
    /// Gets the cross product of CubeVector a with CubeVector b.
    /// </summary>
    /// <param name="a">
    /// A vector to be used as the multiplicand.
    /// </param>
    /// <param name="b">
    /// A vector to be used as the multiplier.
    /// </param>
    /// <returns></returns>
    public static CubeVector CrossProduct (
        CubeVector a,
        CubeVector b
    ) {
        return a.Cross(b);
    }

    /// <summary>
    /// Scale the size of a CubeVector by the provided value.
    /// </summary>
    /// <param name="cubeVector"></param>
    /// <param name="scalar"></param>
    /// <returns></returns>
    public static CubeVector Scale(
        CubeVector cubeVector,
        int scalar
    ) {
        return cubeVector.Scale(scalar);
    }

    /// <summary>
    /// Get a CubeVector of length 1 representing the direction from
    /// one cube vector to antoher, where the provided cube vectors
    /// are considered to be coordinates on a plane.
    /// </summary>
    /// <param name="from">
    /// The origin of the direction.
    /// </param>
    /// <param name="to">
    /// The destination of the direction.
    /// </param>
    /// <returns>
    /// A CubeVector of length 1 representing the direction from
    /// one cube vector to antoher, where the provided cube vectors
    /// are considered to be coordinates on a plane.
    /// </returns>
    public static CubeVector CubeDirection(
        CubeVector from,
        CubeVector to
    ) {
        return (to - from).Normalized;
    }

    /// <summary>
    /// Get a CubeVector of length 1 representing the direction from
    /// one cube vector to antoher, where the provided cube vectors
    /// are considered to be coordinates on a plane.
    /// </summary>
    /// <param name="from">
    /// The origin of the direction.
    /// </param>
    /// <param name="to">
    /// The destination of the direction.
    /// </param>
    /// <param name="wrapOffsetX">
    /// The offset required to wrap cube coordinates around the
    /// x axis. Should be set to the width of the bounds of the
    /// plane which the coordinate is located on.
    /// </param>
    /// <returns>
    /// A CubeVector of length 1 representing the direction from
    /// one cube vector to antoher, where the provided cube vectors
    /// are considered to be coordinates on a plane.
    /// </returns>
    public static CubeVector CubeDirectionWrapping(
        CubeVector from,
        CubeVector to,
        int wrapOffsetX
    ) {
        if (
            IsWrappingRight(
                from, to, wrapOffsetX
            )
        ) {
            CubeVector toWrapRight =
                new CubeVector(
                    to.X + wrapOffsetX,
                    to.Z
                );

            return CubeDirection(from, toWrapRight);
        }
        else if (
            IsWrappingLeft(
                from, to, wrapOffsetX
            )
        ) {
            CubeVector toWrapLeft =
                new CubeVector(
                    to.X - wrapOffsetX,
                    to.Z
                );
            
            return CubeDirection(from, toWrapLeft);
        }

        return CubeDirection(from, to);
    }

    /// <summary>
    /// Get a hex direction pointing from one CubeVector to another,
    /// where the provided CubeVectors are considered to be
    /// coordinates on a plane.
    /// </summary>
    /// <param name="from">
    /// The origin of the direction.
    /// </param>
    /// <param name="to">
    /// The destination of the direction.
    /// </param>
    /// <returns>
    /// A hex direction pointing from one CubeVector to another,
    /// where the provided CubeVectors are considered to be
    /// coordinates on a plane.
    /// </returns>
    public static HexDirections HexDirection(
        CubeVector from,
        CubeVector to
    ) {
        CubeVector direction = CubeDirection(from, to);
        return HexDirectionInternal(direction);        
    }

    /// <summary>
    /// Get a hex direction pointing from one CubeVector to another,
    /// where the provided CubeVectors are considered to be
    /// coordinates on a plane.
    /// </summary>
    /// <param name="from">
    /// The origin of the direction.
    /// </param>
    /// <param name="to">
    /// The destination of the direction.
    /// </param>
    /// <param name="wrapOffsetX">
    /// The offset required to wrap cube coordinates around the
    /// x axis. Should be set to the width of the bounds of the
    /// plane which the coordinate is located on.
    /// </param>
    /// <returns>
    /// A hex direction pointing from one CubeVector to another,
    /// where the provided CubeVectors are considered to be
    /// coordinates on a plane.
    /// </returns>
    public static HexDirections HexDirectionWrapping(
        CubeVector from,
        CubeVector to,
        int wrapOffsetX
    ) {
        CubeVector direction = CubeDirectionWrapping(from, to, wrapOffsetX);
        return HexDirectionInternal(direction);
    }

    /// <summary>
    /// Get a CubeVector corresponding to a Vector3.
    /// </summary>
    /// <param name="vector3">
    /// The specified Vector3, where the x and z coordinates are
    /// considered to be the corresponding x and z coordinates of
    /// the resulting cube vector.
    /// </param>
    /// <param name="hexOuterRadius">
    /// The distance from the center of a hex to one of its corners.
    /// </param>
    /// <param name="wrapOffsetX">
    /// The offset required to wrap cube coordinates around the x
    /// axis. Should be set to the width of the bounds of the plane
    /// which the coordinate is located on.
    /// </param>
    /// <returns>
    /// A cube vector corresponding to the position of the Vector3.
    /// </returns>
    public static CubeVector FromVector3(
        Vector3 vector3,
        float hexOuterRadius,
        int wrapOffsetX
    ) {
        float innerDiameter =
            HexagonPoint.OuterToInnerRadius(hexOuterRadius) * 2f;
            
        // Divide X by the horizontal width of a hexagon.
        float x = vector3.x / innerDiameter;

        // The y axis is just the inverse of the x axis.
        float y = -x;

        //Shift every two rows one unit to the left.
        float offset = vector3.z / (hexOuterRadius * 3f);
        x -= offset;
        y -= offset;

        int integerX = Mathf.RoundToInt(x);
        int integerY = Mathf.RoundToInt(y);

        //  X + Y + Z = 0->
        //  X + Z = -Y ->
        //  (X + Z) / -1 = -Y / -1 ->
        //  -X - Z = Y
        int integerZ = Mathf.RoundToInt(-x - y);

        if (integerX + integerY + integerZ != 0)
        {
            // As a coordinate gets further away from the center,
            // the likelihood of it producing a rounding error
            // increases. Therefore, find the largest rounding
            // delta.
            
            float deltaX = Mathf.Abs(x - integerX);
            float deltaY = Mathf.Abs(y - integerY);
            float deltaZ = Mathf.Abs(-x - y - integerZ);

            // If X has the largest rounding delta, reconstruct X
            // from Y and Z
            if (deltaX > deltaY && deltaX > deltaZ)
            {
                integerX = -integerY - integerZ;
            }

            //If Z has the largest rounding delta, reconstruct Z
            // from X and Y
            else if (deltaZ > deltaY)
            {
                integerZ = -integerX - integerY;
            }
        }

        return new CubeVector(
            integerX,
            integerZ,
            wrapOffsetX
        );

    }

/// <summary>
/// Create cube vector from offset coordinates.
/// </summary>
/// <param name="xOffset">
/// The longitudinal offset coordinate.
/// </param>
/// <param name="zOffset">
/// The the latitudinal offsetCoordinate.
/// </param>
/// <param name="wrapSize">
/// The wrap size for the coordinates if the coordinate system is
/// wrapping.
/// </param>
/// <returns></returns>
    public static CubeVector FromOffsetCoordinates(
        int xOffset,
        int zOffset,
        int wrapSize
    ) {

// Return coordinates after subtracting the X coordinate with the Z
// coordinate integer divided by 2.
// 
// All hexes will be offset on the X axis directly proportional to Z.
// As Z grows larger, the magnitude of the offset increases bringing
// the X axis into alignment with a proposed axis which is at a
// (roughly) 45 degree angle with the Z axis.
        return new CubeVector(
            xOffset - zOffset / 2,
            zOffset,
            wrapSize
        );
    }

    /// <summary>
    /// Get the distance in hex tiles between two cube vectors
    /// representing tiles on a hex grid.
    /// </summary>
    /// <param name="source">
    /// A cube vector representing the source tile.
    /// </param>
    /// <param name="target">
    /// A cube vector representing the target tile.
    /// </param>
    /// <returns>
    /// The distance in hex tiles between the source tile and the
    /// target tile.
    /// </returns>
    public static int UnwrappedHexTileDistance(
        CubeVector source,
        CubeVector target
    ) {
        int result =  
            (int)Mathf.Floor(
                CubicDistance(source, target)
            );

        return result;
    }    

    /// <summary>
    /// Gets the distance between to hex tiles. 
    /// </summary>
    /// <param name="a">
    /// A cube vector representing the first hex tile.
    /// </param>
    /// <param name="b">
    /// A cube vector representing the second hex tile.
    /// </param>
    /// <param name="wrapOffsetX">
    /// The offset required to wrap cube coordinates around the x
    /// axis. Should be set to the width of the bounds of the plane
    /// which the coordinate is located on.
    /// </param>
    /// <returns></returns>
    public static int WrappedHexTileDistance(
        CubeVector a,
        CubeVector b,
        int wrapOffsetX
    ) {
        if (wrapOffsetX < 0)
            throw new ArgumentException(
                "The wrap size must be greater than 0."
            );

        int unwrappedDistance = UnwrappedHexTileDistance(a, b);

        CubeVector rightWrapped =
            new CubeVector(
                b._x + wrapOffsetX,
                b._z
            );

        int rightWrappedDistance = UnwrappedHexTileDistance(
            a,
            rightWrapped
        );

        if (rightWrappedDistance < unwrappedDistance)
            return rightWrappedDistance;

        CubeVector leftWrapped =
            new CubeVector(
                b._x - wrapOffsetX,
                b._z
            );

        int leftWrappedDistance = UnwrappedHexTileDistance(
            a,
            leftWrapped
        );

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
            Mathf.Pow(source._x - target._x, 2f) +
            Mathf.Pow(source.Y - target.Y, 2f) +
            Mathf.Pow(source._z - target._z, 2f)
        );
    }

    #endregion

    #region Private Methods
    
    private int ClampToHighestAbsoluteValue(float toClamp) {
        return 
            toClamp > 0 ?
                Mathf.CeilToInt(toClamp) :
                Mathf.FloorToInt(toClamp);
    }

    #endregion

    #region Private Static Methods
    
    private static HexDirections HexDirectionInternal(
        CubeVector vector
    ) {
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

    private static bool IsWrappingLeft(
        CubeVector source,
        CubeVector target,
        int wrapSize
    ) {
        if (wrapSize < 0)
            throw new ArgumentException(
                "The wrap size must be greater than or equal to 0."
            );

        int unwrappedDistance = UnwrappedHexTileDistance(source, target);

        CubeVector leftWrapped =
            new CubeVector(
                target._x - wrapSize,
                target._z
            );

        int rightWrappedDistance = UnwrappedHexTileDistance(source, leftWrapped);

        if (rightWrappedDistance < unwrappedDistance)
            return true;

        return false;
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

        int unwrappedDistance = UnwrappedHexTileDistance(source, target);

        CubeVector rightWrapped =
            new CubeVector(
                target._x + wrapSize,
                target._z
            );

        int rightWrappedDistance = UnwrappedHexTileDistance(source, rightWrapped);

        if (rightWrappedDistance < unwrappedDistance)
            return true;

        return false;
    }

    #endregion

    #endregion

    #region Structs
    #endregion

    #region Classes
    #endregion

 }
