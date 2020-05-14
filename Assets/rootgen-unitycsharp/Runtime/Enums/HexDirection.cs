using UnityEngine;

public enum HexDirection
{
    Northeast = 0,
    East = 1,
    SouthEast = 2,
    SouthWest = 3,
    West = 4,
    Northwest = 5
}

public static class DirectionAtClockwiseAngle
{
    /* 'this' keyword is required to add the functionality of this method to
        * the 'HexDirections' enum. The class referenced after 'this' is the
        * target of the added functionality, and the method provided can be
        * executed by calling the method on the class predicated by 'this'
        * in the arguement list.*/

    public static HexDirection Opposite(this HexDirection direction)
    {
        /* If the direction is less than 3, returns the opposite direction at
            * HexDirections + 3. If the direction is greater than 3, loops back 
            * around the HexDirections enum and returns the opposite direction.*/

        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection PreviousClockwise(this HexDirection direction)
    {
        /* If the hex direction is the first in the enum, wrap back and return the last enum
            * value, otherwise return the preceding enum value.
            */

        return direction == HexDirection.Northeast ? HexDirection.Northwest : (direction - 1);
    }

    public static HexDirection PreviousClockwise2(this HexDirection direction)
    {
        direction -= 2;
        return direction >= HexDirection.Northeast ? direction : direction + 6;
    }

    public static HexDirection NextClockwise(this HexDirection direction)
    {
        /* If the hex direction is the last in the enu, wrap forward and return the last enum
            * value, otherwise return the next enum value.
            */

        return direction == HexDirection.Northwest ? HexDirection.Northeast : (direction + 1);
    }

    public static HexDirection NextClockwise2(this HexDirection direction)
    {
        direction += 2;
        return direction <= HexDirection.Northwest ? direction : (direction - 6);
    }

/// <summary>
///     Returns a cardinal direction the specified number of clockwise
///     rotations from this direction. If the specified rotation is
///     negative, returns a counter clockwise rotation.
/// </summary>
/// <param name="direction">
///     This direction.
/// </param>
/// <param name="rotation">
///     The specified number of rotations. If negative, rotates
///     counter-clockwise instead.
/// </param>
/// <returns>
///     A cartinal direction the specified number of clockwise
///     rotations from this direction.
/// </returns>
    public static HexDirection ClockwiseRotation(
        this HexDirection direction,
        int rotation
    ) {
        if (rotation < 0) {
            int result = (int)direction + rotation;

            while (result < 0)
                result = (int)HexDirection.Northwest + 1 - result;
            
            return (HexDirection)result;
        }

        return direction + rotation % ((int)HexDirection.Northwest + 1);
    }
}

