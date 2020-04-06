using UnityEngine;

public class CellData : ScriptableObject {
    public HexCoordinates _coordiantes;
    public int _elevation = int.MinValue;
    public int _waterLevel;
    public bool _hasIncomingRiver;
    public bool _hasOutgoingRiver;
    public bool _hasWalls;
    public bool _explored;
    public HexDirection _incomingRiver;
    public HexDirection _outgoingRiver;
    public int _urbanLevel;
    public int _farmLevel;
    public int _plantLevel;
    public int _specialIndex;
    public int _terrainTypeIndex;
    public int _distance;
    public int _visibility;
    public HexCell[] _neighbors;
    public bool[] _roads;
}