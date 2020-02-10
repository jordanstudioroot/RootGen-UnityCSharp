using UnityEngine;

/* Should only be used when triangulating as it is not serializeable.
* Should not be used to store edge vertices.
*/
public struct EdgeVertices
{
    public Vector3 vertex1;
    public Vector3 vertex2;
    public Vector3 vertex3;
    public Vector3 vertex4;
    public Vector3 vertex5;

    public EdgeVertices(Vector3 corner1, Vector3 corner2)
    {
        vertex1 = corner1;
        vertex2 = Vector3.Lerp(corner1, corner2, 0.25f);
        vertex3 = Vector3.Lerp(corner1, corner2, 0.5f);
        vertex4 = Vector3.Lerp(corner1, corner2, 0.75f);
        vertex5 = corner2;
    }

    public EdgeVertices(Vector3 corner1, Vector3 corner2, float outerStep)
    {
        vertex1 = corner1;
        vertex2 = Vector3.Lerp(corner1, corner2, outerStep);
        vertex3 = Vector3.Lerp(corner1, corner2, 0.5f);
        vertex4 = Vector3.Lerp(corner1, corner2, 1f - outerStep);
        vertex5 = corner2;
    }

    public static EdgeVertices TerraceLerp
    (
        EdgeVertices edgeA,
        EdgeVertices edgeB,
        int step
    )
    {
        EdgeVertices result = new EdgeVertices();

        // Note that HexMetrics.TerraceLerp is being called. This method is not recursive.
        result.vertex1 = HexMetrics.TerraceLerp(edgeA.vertex1, edgeB.vertex1, step);
        result.vertex2 = HexMetrics.TerraceLerp(edgeA.vertex2, edgeB.vertex2, step);
        result.vertex3 = HexMetrics.TerraceLerp(edgeA.vertex3, edgeB.vertex3, step);
        result.vertex4 = HexMetrics.TerraceLerp(edgeA.vertex4, edgeB.vertex4, step);
        result.vertex5 = HexMetrics.TerraceLerp(edgeA.vertex5, edgeB.vertex5, step);

        return result;
    }
}