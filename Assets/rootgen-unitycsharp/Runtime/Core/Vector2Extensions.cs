using UnityEngine;

public static class Vector2Extensions {

/// <summary>
///     Returns a boolean value representing whether this Vector2 is a factor
///     of the provided Vector2. 
/// </summary>
/// <param name="subject">
///     This Vector2.    
/// </param>
/// <param name="target">
///     The Vector2 providing the dimensions representing the factor.
/// </param>
/// <returns></returns>
    public static bool IsFactorOf(
        this Vector2 subject,
        Vector2 target
    ) {
        if (
            subject.x < target.x ||
            subject.x % target.x != 0 ||
            subject.y < target.y ||
            subject.y % target.y != 0
        ) {
            return false;
        }

        return true;
    }

/// <summary>
///     Returns a new Vector2 clamped to a factor of the provided Vector2.
/// </summary>
/// <param name="subject">
///     This Vector2.
/// </param>
/// <param name="target">
///     The Vector2 providing the dimensions representing the factor.
/// </param>
/// <returns></returns>
    public static Vector2 ClampToFactorOf(
        this Vector2 subject,
        Vector2 target
    ) {
        float xClamped = Mathf.Clamp(
            subject.x,
            target.x, 
            subject.x - (subject.x % target.x)
        );

        float zClamped = Mathf.Clamp(
            subject.y,
            target.y,
            subject.y - (subject.y % target.y)
        );

        return new Vector2(xClamped, zClamped);
    }
}