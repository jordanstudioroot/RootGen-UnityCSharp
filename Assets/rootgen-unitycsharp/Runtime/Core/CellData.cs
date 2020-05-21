using UnityEngine;

public struct HexData {
    public CubeVector Coordinates { get; set; }
    public int Index { get; set; }
    public int ColumnIndex { get; set; }
    public HexShaderData HexShaderData { get; set; }
    public bool IsExplorable { get; set; }
    public Vector3 WorldPosition { get; set; }
    public HexMeshChunk HexGridChunk { get; set; }
    
}
