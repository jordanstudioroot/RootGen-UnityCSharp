using UnityEngine;

public class TerrainTriangulationData {
    /// <summary>
    /// The center of the terrain surface for the hex being triangulated.
    /// </summary>
    public Vector3 terrainCenter;

    /// <summary>
    /// The center of the water surface for hex being triangulated.
    /// </summary>
    public Vector3 waterSurfaceCenter;
    
    /// <summary>
    /// The left corner of the edge being triangulated relative to the
    /// center of the water surface.
    /// </summary>
    public Vector3 waterSurfaceCornerLeft;

    /// <summary>
    /// The right corner of the edge being triangulated relative to the
    /// center of the water surface.
    /// </summary>
    public Vector3 waterSurfaceCornerRight;

    /// <summary>
    /// The left river bank boundary for the edge.
    /// </summary>
    public Vector3 riverCenterLeft;

    /// <summary>
    /// The right river bank boundary for the edge.
    /// </summary>
    public Vector3 riverCenterRight;

    /// <summary>
    /// The edge vertices for the center of the hex edge.
    /// </summary>
    public EdgeVertices centerEdgeVertices;

    /// <summary>
    /// The edge vertices for the connection the hex makes
    /// with its neighbor for the given edge.
    /// </summary>
    public EdgeVertices connectionEdgeVertices;

    /// <summary>
    /// The edge vertices between the center and the connection.
    /// </summary>
    public EdgeVertices middleEdgeVertices;
}