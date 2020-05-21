using UnityEngine;

public static class MeshConstants {
/// <summary>
/// The size of a mesh chunk along the x axis in offset coordinates.
/// </summary>
    public const int ChunkXMax = 5;

/// <summary>
/// The size of a mesh chunk along the z axis in offset coordinates.
/// </summary>
    public const int ChunkZMax = 5;

    public const int DefaulthexOuterRadius = 10;

    public static readonly Vector2 ChunkSize =
        new Vector2(
            ChunkXMax,
            ChunkZMax
        );
}