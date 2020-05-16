using UnityEngine;

public enum HexDirections
{
    Northeast = 0,
    East = 1,
    Southeast = 2,
    Southwest = 3,
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

    public static HexDirections Opposite(this HexDirections direction)
    {
        /* If the direction is less than 3, returns the opposite direction at
            * HexDirections + 3. If the direction is greater than 3, loops back 
            * around the HexDirections enum and returns the opposite direction.*/

        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirections PreviousClockwise(this HexDirections direction)
    {
        /* If the hex direction is the first in the enum, wrap back and return the last enum
            * value, otherwise return the preceding enum value.
            */

        return direction == HexDirections.Northeast ? HexDirections.Northwest : (direction - 1);
    }

    public static HexDirections PreviousClockwise2(this HexDirections direction)
    {
        direction -= 2;
        return direction >= HexDirections.Northeast ? direction : direction + 6;
    }

    public static HexDirections NextClockwise(this HexDirections direction)
    {
        /* If the hex direction is the last in the enu, wrap forward and return the last enum
            * value, otherwise return the next enum value.
            */

        return direction == HexDirections.Northwest ? HexDirections.Northeast : (direction + 1);
    }

    public static HexDirections NextClockwise2(this HexDirections direction)
    {
        direction += 2;
        return direction <= HexDirections.Northwest ? direction : (direction - 6);
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
    public static HexDirections ClockwiseRotation(
        this HexDirections direction,
        int rotation
    ) {
        if (rotation < 0) {
            int result = (int)direction + rotation;

            while (result < 0)
                result = (int)HexDirections.Northwest + 1 - result;
            
            return (HexDirections)result;
        }

        return direction + rotation % ((int)HexDirections.Northwest + 1);
    }
} 

