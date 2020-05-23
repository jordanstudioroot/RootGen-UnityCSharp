using UnityEngine;

/// <summary>
/// Contains all constant values for the hex mesh. 
/// </summary>
public static class HexMeshConstants {
/// <summary>
/// The size of a mesh chunk along the x axis in offset coordinates.
/// </summary>
    public const int CHUNK_SIZE_X = 5;

/// <summary>
/// The size of a mesh chunk along the z axis in offset coordinates.
/// </summary>
    public const int CHUNK_SIZE_Z = 5;

/// <summary>
/// The default outer radius (distance from center to a corner) for
/// hexes in Unity-space coordinates.
/// </summary>
    public const int DEFAULT_HEX_OUTER_RADIUS = 10;

/// <summary>
/// A vector 2 containing the mesh chunk size along the x and z
/// axis in offset coordinates.
/// </summary>
/// <param name="CHUNK_SIZE_X">
/// The mesh chunk size along the x axis in offset coordinates.
/// </param>
/// <param name="CHUNK_SIZE_Z">
/// The mesh chunk size along the z axis in offset coordinates.
/// </param>
    public static readonly Vector2 ChunkSize =
        new Vector2(
            CHUNK_SIZE_X,
            CHUNK_SIZE_Z
        );
}