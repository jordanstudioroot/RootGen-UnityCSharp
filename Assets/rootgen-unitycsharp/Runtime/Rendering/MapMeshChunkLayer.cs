using RootCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A monobehaviour facade for a mesh layer of a mesh chunk, providing
/// interfaces for adding and updating triangle vertices and uv coordinates
/// assocaited with a given layer of a mesh chunk.
/// </summary>

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapMeshChunkLayer : MonoBehaviour {

/// <summary>
/// Boolean value representing whether the mesh collider is used.
/// </summary>
    private bool _useCollider;

/// <summary>
/// Boolean value representing whether the hexData is used in rendering.
/// </summary>
    private bool _useHexData;

/// <summary>
/// Boolean value representing whether the UV coordinates are used in rendering.
/// </summary>
    private bool _useUVCoordinates;
/// <summary>
/// Boolean value representing whether the UV2 coordinates are used in rendering.
/// </summary>
    private bool _useUV2Coordinates;

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
/// A list of Color values used as weights to blend textures between hexes. 
/// </summary>
    [NonSerialized] private List<Color> _textureWeights;
    
/// <summary>
/// A list of Vector3s used to map the hex positions to the UV map.
/// </summary>
    [NonSerialized] private List<Vector3> _uv3s;

    private Mesh _mesh;
    private MeshCollider _meshCollider;
    

    protected void Awake() {
        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Mesh";
    }

    public static MapMeshChunkLayer CreateEmpty(
        Material material,
        bool useCollider, 
        bool useHexData, 
        bool useUVCoordinates, 
        bool useUV2Coordinates
    ) {
        GameObject resultObj = new GameObject("Map Mesh Chunk Layer");
        MapMeshChunkLayer resultMono = resultObj.AddComponent<MapMeshChunkLayer>();
        resultMono.GetComponent<MeshRenderer>().material = material;
        resultMono._useCollider = useCollider;

        if (useCollider)
            resultMono._meshCollider =
                resultObj.AddComponent<MeshCollider>();

        resultMono._useHexData = useHexData;
        resultMono._useUVCoordinates = useUVCoordinates;
        resultMono._useUV2Coordinates = useUV2Coordinates;

        return resultMono;
    }

/// <summary>
/// Add a triangle.
/// </summary>
/// <param name="firstCorner">
/// The first corner of the triangle to be drawn, counter-clockwise.
/// </param>
/// <param name="secondCorner">
/// The second corner of the triangle to be drawn, counter-clockwise.
/// </param>
/// <param name="thirdCorner">
/// The third corner of the triangle to be drawn, counter-clockwise.
/// </param>
    public void AddTriangleUnperturbed(
        Vector3 firstCorner, 
        Vector3 secondCorner, 
        Vector3 thirdCorner
    ) {
        int vertexIndex = _vertices.Count;

        _vertices.Add(firstCorner);
        _vertices.Add(secondCorner);
        _vertices.Add(thirdCorner);

        // 3rd
        // |  \
        // |   \
        // |    \
        // |     \
        // 1st -- 2nd
        
        // First corner
        _triangles.Add(vertexIndex);
        
        // Second corner
        _triangles.Add(vertexIndex + 1);
        
        // Third corner
        _triangles.Add(vertexIndex + 2);
    }

    /// <summary>
    /// Add and perturb the vertices of a triangle.
    /// </summary>
    /// <param name="firstCorner">
    /// The first corner of the triangle to be drawn.
    /// </param>
    /// <param name="secondCorner">
    /// The second corner of the triangle to be drawn.
    /// </param>
    /// <param name="thirdCorner">
    /// The third corner of the triangle to be drawn.
    /// </param>
    /// <param name="hexOuterRadius">
    /// The outer radius (distance from the center to a given corner) of the
    /// hex which the triangle is a part of.
    /// </param>
    /// <param name="wrapOffsetX">
    /// The offset required to wrap the map along the longitudinal axis.
    /// This should be equal to the longitudinal size of the plane which 
    /// the hex being drawn is a part of.
    /// </param>
    public void AddTrianglePerturbed(
        Vector3 firstCorner, 
        Vector3 secondCorner, 
        Vector3 thirdCorner,
        float hexOuterRadius,
        int wrapOffsetX
    ) {
        int vertexIndex = _vertices.Count;

        _vertices.Add(
            HexagonPoint.Perturb(
                firstCorner,
                hexOuterRadius,
                wrapOffsetX
            )
        );

        _vertices.Add(
            HexagonPoint.Perturb(
                secondCorner,
                hexOuterRadius,
                wrapOffsetX
            )
        );
        
        _vertices.Add(
            HexagonPoint.Perturb(
                thirdCorner,
                hexOuterRadius,
                wrapOffsetX
            )
        );
        
        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
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

        // 3rd -- 4th  1: 1st -> 3rd -> 2nd
        //  |\     |
        //  | \  2 |   2: 2nd -> 3rd -> 4th
        //  |  \   |
        //  | 1 \  |
        //  |    \ |
        // 1st -- 2nd

        // Triangle 1
        // Vertex 1
        _triangles.Add(vertexIndex);
        
        // Vertex 3
        _triangles.Add(vertexIndex + 2);

        // Vertex 2
        _triangles.Add(vertexIndex + 1);
        
        // Triangle2

        // Vertex 2
        _triangles.Add(vertexIndex + 1);
        
        // Vertex 3
        _triangles.Add(vertexIndex + 2);
        
        // Vertex 4
        _triangles.Add(vertexIndex + 3);
    }

    /// <summary>
    /// Add a quad with perturbed corners.
    /// </summary>
    /// <param name="firstCorner">
    /// The first corner of the quad to be drawn, counter-clockwise.
    /// </param>
    /// <param name="secondCorner">
    /// The second corner of the quad to be drawn, counter-clockwise.
    /// </param>
    /// <param name="thirdCorner">
    /// The third corner of the quad to be drawn, counter-clockwise.
    /// </param>
    /// <param name="fourthCorner">
    /// The fourth corner of the quad to be drawn, counter-clockwise.
    /// </param>
    /// <param name="hexOuterRadius">
    /// The  outer radius (distance from the center to a given corner) of
    /// the hex which the quad is a part of.
    /// </param>
    /// <param name="wrapOffsetX">
    /// The offset required to wrap the map along the longitudinal axis.
    /// This should be equal to the longitudinal size of the plane which
    /// the hex being drawn is a part of.
    /// </param>
    public void AddQuadPerturbed(
        Vector3 firstCorner, 
        Vector3 secondCorner, 
        Vector3 thirdCorner, 
        Vector3 fourthCorner,
        float hexOuterRadius,
        int wrapOffsetX
    ) {

        int vertexIndex = _vertices.Count;
        _vertices.Add(
            HexagonPoint.Perturb(
                firstCorner,
                hexOuterRadius,
                wrapOffsetX
            )
        );

        _vertices.Add(
            HexagonPoint.Perturb(
                secondCorner,
                hexOuterRadius,
                wrapOffsetX
            )
        );
        
        _vertices.Add(
            HexagonPoint.Perturb(
                thirdCorner,
                hexOuterRadius,
                wrapOffsetX
            )
        );

        _vertices.Add(
            HexagonPoint.Perturb(
                fourthCorner,
                hexOuterRadius,
                wrapOffsetX
            )
        );

        _triangles.Add(vertexIndex);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 1);       
        _triangles.Add(vertexIndex + 1);
        _triangles.Add(vertexIndex + 2);
        _triangles.Add(vertexIndex + 3);
    }

/// <summary>
/// Add hex data to the mesh corresponding to a particular set of hex
/// indicies represented as a collection by a Vector3.
/// </summary>
/// <param name="terrainTypes">
/// A Vector3 containing the terrain type of each vertex which composes
/// the triangle, where the terrain type is represented as an integer.
/// </param>
/// <param name="firstTextureWeight">
/// 
/// </param>
/// <param name="secondTextureWeight">
/// The weight of the terrain texture for the second corner.
/// </param>
/// <param name="thirdTextureWeight">
/// The weight of the terrain texture for the third corner.
/// </param>
    public void AddTriangleHexData(
        Vector3 terrainTypes, 
        Color firstTextureWeight, 
        Color secondTextureWeight, 
        Color thirdTextureWeight
    ) {
        _uv3s.Add(terrainTypes);
        _uv3s.Add(terrainTypes);
        _uv3s.Add(terrainTypes);
        
        _textureWeights.Add(firstTextureWeight);
        _textureWeights.Add(secondTextureWeight);
        _textureWeights.Add(thirdTextureWeight);
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
        _uv3s.Add(indices);
        _uv3s.Add(indices);
        _uv3s.Add(indices);
        _uv3s.Add(indices);
        _textureWeights.Add(weights1);
        _textureWeights.Add(weights2);
        _textureWeights.Add(weights3);
        _textureWeights.Add(weights4);
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

    /// <summary>
    /// Clears the triangles, vuvs, and vertex colors of the mesh,
    /// filling them with empty vertices.
    /// </summary>
    public void Clear() {
        _mesh.Clear();
        _vertices = ListPool<Vector3>.Get();

        if (_useHexData) {
            _textureWeights = ListPool<Color>.Get();
            _uv3s = ListPool<Vector3>.Get();
        }

        if (_useUVCoordinates) {
            _uvs = ListPool<Vector2>.Get();
        }

        if (_useUV2Coordinates) {
            _uv2s = ListPool<Vector2>.Get();
        }

        _triangles = ListPool<int>.Get();
    }

    /// <summary>
    /// Updates the mesh with changes to the triangles, uvs, and vertex
    /// colors.
    /// </summary>
    public void Draw() {
        _mesh.SetVertices(_vertices);
        ListPool<Vector3>.Add(_vertices);

        if (_useHexData) {
            // Set the mesh color data to represent texture weights.
            _mesh.SetColors(_textureWeights);
            ListPool<Color>.Add(_textureWeights);

            // Set the uv3 coordinates to represent cell indices.
            _mesh.SetUVs(2, _uv3s);
            ListPool<Vector3>.Add(_uv3s);
        }

        if (_useUVCoordinates) {
            _mesh.SetUVs(0, _uvs);
            ListPool<Vector2>.Add(_uvs);
        }

        if (_useUV2Coordinates) {
            _mesh.SetUVs(1, _uv2s);
            ListPool<Vector2>.Add(_uv2s);
        }

        _mesh.SetTriangles(_triangles, 0);
        ListPool<int>.Add(_triangles);
        _mesh.RecalculateNormals();

        if (_useCollider) {
            _meshCollider.sharedMesh = _mesh;
        }
    }
}
