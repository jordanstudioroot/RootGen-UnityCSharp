using UnityEngine;
using System.IO;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField]
    private int x, z;

    public int X
    {
        get
        {
            return x;
        }
    }

    public int Z
    {
        get
        {
            return z;
        }
    }

    public int Y
    {
        get
        {
            /* Because having only an X and Z axis only allows movement in four directions,
                * a Y coordinate must be added. The defining property of the Y coordinate is that
                * adding it to the Z coordinate will always produce the same result, as the Y axis
                * is a mirror of the X axis. Therefore:
                *                  X + Y + Z = X + Z = -Y = -1(X + Z) = -1(-Y) = -X - Z = Y
                */

            return -X - Z;
        }
    }

    public HexCoordinates(int x, int z) {
        
        if (HexagonPoint.IsMapWrapping) {
            /* Get offset x coordinate back from axial coordinates, and
                * check if value is outside of the wrapping range and adjust
                * the coordinate accordingly.*/
            int offsetX = x + z / 2;

            if (offsetX < 0) {
                x += HexagonPoint.MapWrapSize;
            }
            else if (offsetX >= HexagonPoint.MapWrapSize) {
                x -= HexagonPoint.MapWrapSize;
            }
        }

        this.x = x;
        this.z = z;
    }

    public static HexCoordinates FromPosition(
        Vector3 position,
        float outerRadius
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

        return new HexCoordinates(
            integerX,
            integerZ
        );

    }

    public static HexCoordinates AsAxialCoordinates(
        int x,
        int z,
        int wrapSize
    ) {
        /* Return coordinates after subtracting the X coordinate with the Z coordinated integer
            * divided by 2. All cells will be offset on the X axis directly proportional to Z. As
            * Z grows larger, the magnitude of the offset increases bringing the X axis into alignment
            * with a proposed axis which is at a (roughly) 45 degree angle with the Z axis.*/
        return new HexCoordinates(
            x - z / 2,
            z
        );
    }

    public int DistanceTo(
        HexCoordinates other,
        int wrapSize
    ) {
        int xy =
            (x < other.x ? other.x - x : x - other.x) +
            (Y < other.Y ? other.Y - Y : Y - other.Y);

        if (HexagonPoint.IsMapWrapping) {
            other.x += wrapSize;
            int xyWrapped =
                (x < other.x ? other.x - x : x - other.x) +
                (Y < other.Y ? other.Y - Y : Y - other.Y);
            if (xyWrapped < xy) {
                xy = xyWrapped;
            }
            else {
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
            "(X: " +
            X.ToString() +
            ", Y: " +
            Y.ToString() +
            ", Z: " +
            Z.ToString() +
            ")";
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

    public static HexCoordinates Load(BinaryReader reader)
    {
        HexCoordinates coordinates;
        coordinates.x = reader.ReadInt32();
        coordinates.z = reader.ReadInt32();
        return coordinates;
    }
}
