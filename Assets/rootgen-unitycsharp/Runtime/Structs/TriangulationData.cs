using UnityEngine;

public struct TriangulationData {
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

    /// <summary>
    /// A vector 3 containing the cell indices relative to the source
    /// cell of the triangulation, where
    ///  x = source index,
    ///  y = left of source
    ///  z = right of source
    /// </summary>
    public Vector3 terrainSourceRelativeHexIndices;

    public int terrainSourceHexIndex {
        get {
            return (int)terrainSourceRelativeHexIndices.x; 
        } 

        set {
            terrainSourceRelativeHexIndices.x = value;
        }
    }

    public int terrainLeftHexIndex {
        get {
            return (int)terrainSourceRelativeHexIndices.y;
        }
        set {
            terrainSourceRelativeHexIndices.y = value;
        }
    }

    public int terrainRightHexIndex {
        get {
            return (int)terrainSourceRelativeHexIndices.z;
        }
        set {
            terrainSourceRelativeHexIndices.z = value;
        }
    }

    public Vector3 waterSourceRelativeHexIndices;

    public int waterSourceHexIndex {
        get {
            return (int)waterSourceRelativeHexIndices.x;
        }
        set {
            waterSourceRelativeHexIndices.x = value;
        }
    }
    public int waterLeftHexIndex {
        get {
            return (int)waterSourceRelativeHexIndices.y;
        }
        set {
            waterSourceRelativeHexIndices.y = value;
        }
    }
    public int waterRightHexIndex {
        get {
            return (int)waterSourceRelativeHexIndices.z;
        }
        set {
            waterSourceRelativeHexIndices.z = value;
        }
    }
}