using System;
using System.Collections.Generic;
using UnityEngine;
using RootCollections;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

/// <summary>
/// Boolean value representing whether the mesh collider is used.
/// </summary>
    public bool useCollider;

/// <summary>
/// Boolean value representing whether the hexData is used in rendering.
/// </summary>
    public bool useHexData;

/// <summary>
/// Boolean value representing whether the UV coordinates are used in rendering.
/// </summary>
    public bool useUVCoordinates;
/// <summary>
/// Boolean value representing whether the UV2 coordinates are used in rendering.
/// </summary>
    public bool useUV2Coordinates;

/// <summary>
/// The list of UV coordinates used to render the HexMesh.
/// </summary>
    [NonSerialized] private List<Vector2> _uvs;
/// <summary>
/// The list of the UV2 coordinates used to render the HexMesh.
/// </summary>
    [NonSerialized] private List<Vector2> _uv2s;
/// <summary>
/// The list of vertices used to render the HexMesh.
/// </summary>
    [NonSerialized] private List<Vector3> _vertices;
/// <summary>
/// The list of triangles used to render the HexMesh.
/// </summary>
    [NonSerialized] private List<int> _triangles;

/// <summary>
/// A list of Color values used as weights to blend the mesh data between hexes. 
/// </summary>
    [NonSerialized] private List<Color> _hexWeights;
    
/// <summary>
///     A list of Vector3s used to map the hex positions to the UV map.
/// </summary>
    [NonSerialized] private List<Vector3> _hexIndices;

    private UnityEngine.Mesh _hexMesh;
    private MeshCollider meshCollider;
    

    protected void Awake()
    {
        GetComponent<MeshFilter>().mesh = _hexMesh = new Mesh();
        _hexMesh.name = "Hex Mesh";
    }

    public static HexMesh GetMesh(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Hex Mesh");
        HexMesh resultMono = resultObj.AddComponent<HexMesh>();
        resultMono.GetComponent<MeshRenderer>().material = material;
        resultMono.useCollider = useCollider;

        if (useCollider)
            resultMono.meshCollider = resultObj.AddComponent<MeshCollider>();

        resultMono.useHexData = useHexData;
        resultMono.useUVCoordinates = useUVCoordinates;
        resultMono.useUV2Coordinates = useUV2Coordinates;

        return resultMono;
    }

    public void AddTriangle(
        Vector3 vertex1, 
        Vector3 vertex2, 
        Vector3 vertex3,
        float hexOuterRadius,
        int wrapSize
    ) {
        /* The vertex index is equal to the length of the vertices list before
            * before adding the new vertices to it. */
        int vertexIndex = _vertices.Count;

        _vertices.Add(
            HexagonPoint.Perturb(
                vertex1,
                hexOuterRadius,
                wrapSize
            )
        );

        _vertices.Add(
            HexagonPoint.Perturb(
                vertex2,
                hexOuterRadius,
                wrapSize
            )
        );
        
        _vertices.Add(
            HexagonPoint.Perturb(
                vertex3,
                hexOuterRadius,
                wrapSize
            )
        );
        
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
    }

    public void AddTriangleUnperturbed(
        Vector3 vertex1, 
        Vector3 vertex2, 
        Vector3 vertex3
    ) {
        int vertexIndex = _vertices.Count;

        _vertices.Add(vertex1);
        _vertices.Add(vertex2);
        _vertices.Add(vertex3);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
    }

    public void AddQuad(
        Vector3 vertex1, 
        Vector3 vertex2, 
        Vector3 vertex3, 
        Vector3 vertex4,
        float hexOuterRadius,
        int wrapSize
    ) {

        int vertexIndex = _vertices.Count;
        _vertices.Add(
            HexagonPoint.Perturb(
                vertex1,
                hexOuterRadius,
                wrapSize
            )
        );

        _vertices.Add(
            HexagonPoint.Perturb(
                vertex2,
                hexOuterRadius,
                wrapSize
            )
        );
        
        _vertices.Add(
            HexagonPoint.Perturb(
                vertex3,
                hexOuterRadius,
                wrapSize
            )
        );

        _vertices.Add(
            HexagonPoint.Perturb(
                vertex4,
                hexOuterRadius,
                wrapSize
            )
        );
        
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 3);
    }

    public void AddQuadUnperturbed(
        Vector3 vertex1, 
        Vector3 vertex2, 
        Vector3 vertex3, 
        Vector3 vertex4
    ) {
        int vertexIndex = _vertices.Count;
        _vertices.Add(vertex1);
        _vertices.Add(vertex2);
        _vertices.Add(vertex3);
        _vertices.Add(vertex4);
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 3);
    }

/// <summary>
///     Add hex data to the mesh corresponding to a particular set of hex indicies
///     represented as a collection by a Vector3.
/// </summary>
/// <param name="indices">
///     A Vector3 representing a collection of hex indices corresponding to the triangle.
/// </param>
/// <param name="weights1">
///     The weight of the first
/// </param>
/// <param name="weights2"></param>
/// <param name="weights3"></param>
    public void AddTriangleHexData(
        Vector3 indices, 
        Color weights1, 
        Color weights2, 
        Color weights3
    ) {
        _hexIndices.Add(indices);
        _hexIndices.Add(indices);
        _hexIndices.Add(indices);
        _hexWeights.Add(weights1);
        _hexWeights.Add(weights2);
        _hexWeights.Add(weights3);
    }

    public void AddTriangleHexData(Vector3 indices, Color weights) {
        AddTriangleHexData(indices, weights, weights, weights);
    }

    public void AddQuadHexData(
        Vector3 indices,
        Color weights1, 
        Color weights2, 
        Color weights3, 
        Color weights4
    ) {
        _hexIndices.Add(indices);
        _hexIndices.Add(indices);
        _hexIndices.Add(indices);
        _hexIndices.Add(indices);
        _hexWeights.Add(weights1);
        _hexWeights.Add(weights2);
        _hexWeights.Add(weights3);
        _hexWeights.Add(weights4);
    }

    public void AddQuadHexData(
        Vector3 indices, 
        Color weights1, 
        Color weights2
    ) {
        AddQuadHexData(indices, weights1, weights1, weights2, weights2);
    }

    public void AddQuadHexData(Vector3 indices, Color weights) {
        AddQuadHexData(indices, weights, weights, weights, weights);
    }

    public void AddTriangleUV(Vector2 uv1, Vector2 uv2, Vector3 uv3) {
        _uvs.Add(uv1);
        _uvs.Add(uv2);
        _uvs.Add(uv3);
    }

    public void AddQuadUV(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4) {
        _uvs.Add(uv1);
        _uvs.Add(uv2);
        _uvs.Add(uv3);
        _uvs.Add(uv4);
    }

    public void AddQuadUV(float uMin, float uMax, float vMin, float vMax) {
        _uvs.Add(new Vector2(uMin, vMin));
        _uvs.Add(new Vector2(uMax, vMin));
        _uvs.Add(new Vector2(uMin, vMax));
        _uvs.Add(new Vector2(uMax, vMax));
    }

    public void AddTriangleUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3) {
        _uv2s.Add(uv1);
        _uv2s.Add(uv2);
        _uv2s.Add(uv3);
    }

    public void AddQuadUV2(Vector2 uv1, Vector2 uv2, Vector3 uv3, Vector3 uv4) {
        _uv2s.Add(uv1);
        _uv2s.Add(uv2);
        _uv2s.Add(uv3);
        _uv2s.Add(uv4);
    }

    public void AddQuadUV2(float uMin, float uMax, float vMin, float vMax) {
        _uv2s.Add(new Vector2(uMin, vMin));
        _uv2s.Add(new Vector2(uMax, vMin));
        _uv2s.Add(new Vector2(uMin, vMax));
        _uv2s.Add(new Vector2(uMax, vMax));
    }

    public void Clear() {
        _hexMesh.Clear();
        _vertices = ListPool<Vector3>.Get();

        if (useHexData) {
            _hexWeights = ListPool<Color>.Get();
            _hexIndices = ListPool<Vector3>.Get();
        }

        if (useUVCoordinates) {
            _uvs = ListPool<Vector2>.Get();
        }

        if (useUV2Coordinates) {
            _uv2s = ListPool<Vector2>.Get();
        }

        _triangles = ListPool<int>.Get();
    }

    public void Apply() {
        _hexMesh.SetVertices(_vertices);
        ListPool<Vector3>.Add(_vertices);

        if (useHexData) {
            _hexMesh.SetColors(_hexWeights);
            ListPool<Color>.Add(_hexWeights);
            _hexMesh.SetUVs(2, _hexIndices);
            ListPool<Vector3>.Add(_hexIndices);
        }

        if (useUVCoordinates) {
            _hexMesh.SetUVs(0, _uvs);
            ListPool<Vector2>.Add(_uvs);
        }

        if (useUV2Coordinates) {
            _hexMesh.SetUVs(1, _uv2s);
            ListPool<Vector2>.Add(_uv2s);
        }

        _hexMesh.SetTriangles(_triangles, 0);
        ListPool<int>.Add(_triangles);
        _hexMesh.RecalculateNormals();

        if (useCollider) {
            meshCollider.sharedMesh = _hexMesh;
        }
    }
}
