using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HexCell : MonoBehaviour
{
// FIELDS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private
    private static int MaxFeatureLevel {
        get {
            return 3;
        }
    }

// ~ Non-Static

// ~~ public
    public RectTransform uiRect;
    public HexGridChunk chunk;
    
// ~~ private
    private HexCoordinates _coordiantes;
    private int _elevation = int.MinValue;
    private int _waterLevel;
    private Color _color;
    private bool _hasIncomingRiver;
    private bool _hasOutgoingRiver;
    private bool _hasWalls;
    private bool _explored;
    private HexDirection _incomingRiver;
    private HexDirection _outgoingRiver;
    private int _urbanLevel;
    private int _farmLevel;
    private int _plantLevel;
    private int _specialIndex;
    private int _terrainTypeIndex;
    private int _distance;
    private int _visibility;
    private HexCell[] _neighbors;
    private bool[] _roads;

// CONSTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DESTRUCTORS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// DELEGATES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// EVENTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// ENUMS

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// INTERFACES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// PROPERTIES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ setter only

// ~~ getter only
    public Vector3 Position {
        get {
            return transform.localPosition;
        }
    }

    public bool HasIncomingRiver {
        get {
            return _hasIncomingRiver;
        }
    }

    public bool HasOutgoingRiver {
        get {
            return _hasOutgoingRiver;
        }
    }

    public HexDirection IncomingRiver {
        get {
            return _incomingRiver;
        }
    }

    public HexDirection OutgoingRiver {
        get {
            return _outgoingRiver;
        }
    }

    public bool HasRiver {
        get {
            return _hasIncomingRiver || _hasOutgoingRiver;
        }
    }

    public bool HasRoads {
        get {
            for (int i = 0; i < _roads.Length; i++) {
                if (_roads[i]) {
                    return true;
                }
            }
            return false;
        }
    }

    public bool HasRiverBeginOrEnd {
        get {
            return _hasIncomingRiver != _hasOutgoingRiver;
        }
    }

    public float StreamBedY {
        get {
            return
                (_elevation + HexagonPoint.streamBedElevationOffset) *
                HexagonPoint.elevationStep;
        }
    }

    public float RiverSurfaceY {
        get {
            return
                (_elevation + HexagonPoint.waterElevationOffset) *
                HexagonPoint.elevationStep;
        }
    }

    public float WaterSurfaceY {
        get {
            return
                (_waterLevel + HexagonPoint.waterElevationOffset) *
                HexagonPoint.elevationStep;
        }
    }

    public bool IsUnderwater {
        get {
            return _waterLevel > _elevation;
        }
    }

    public HexDirection RiverBeginOrEndDirection {
        get {
            return HasIncomingRiver ? IncomingRiver : OutgoingRiver;
        }
    }

    public bool IsSpecial {
        get { 
            return _specialIndex > 0; 
        }
    }

    public int SearchPriority {
        get { return _distance + SearchHeuristic; }
    }

    public int ViewElevation {
        get { 
            return _elevation >= _waterLevel ? 
                _elevation : _waterLevel;
        }
    }

    public bool IsVisible {
        get { 
            return _visibility > 0 && IsExplorable; 
        }
    }

    public HexCoordinates Coordinates {
        get {
            return _coordiantes;
        }

        set {
            _coordiantes = value;
        }
    }

    private Mesh GetInteractionMesh(float radius) {
        Mesh result = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        Vector3 center = this.transform.position;
        Vector3 offset = (Vector3.up * .2f);
        for (int i = 0; i < 6; i++) {
            int index = i * 3;
            verts.Add(center + offset);
            verts.Add(HexagonPoint.GetCorner(i, radius) + offset);
            verts.Add(HexagonPoint.GetCorner(i + 1, radius) + offset);

            tris.Add(index);
            tris.Add(index + 1);
            tris.Add(index + 2);
        }
        
        result.Clear();
        result.vertices = verts.ToArray();
        result.triangles = tris.ToArray();

        return result;
    }

    public void SetEnabledInteractionMesh(bool enabled, float outerRadius) {
        MeshCollider colldier;
        if (colldier = this.GetComponent<MeshCollider>()) {
            if (enabled) {
                colldier.enabled = true;
            }
            else {
                colldier.enabled = false;
            }
        }
        else {
            Mesh mesh = GetInteractionMesh(outerRadius);
            colldier = this.gameObject.AddComponent<MeshCollider>();
            colldier.sharedMesh = mesh;
            if (!enabled) {
                colldier.enabled = false;   
            }
        }
    }

    public int Elevation {
        get {
            return _elevation;
        }
    }

    public void SetElevation(int elevation, float cellOuterRadius) {
        if (_elevation == elevation) {
                return;
            }

            int originalViewElevation = ViewElevation;

            _elevation = elevation;

            if (ViewElevation != originalViewElevation) {
                ShaderData.ViewElevationChanged();
            }

            RefreshPosition(cellOuterRadius);
            ValidateRivers();

            for (int i = 0; i < _roads.Length; i++) {
                if (_roads[i] && GetElevationDifference((HexDirection)i) > 1) {
                    SetRoad(i, false);
                }
            }

            Refresh();
    }

    public int TerrainTypeIndex {
        get { 
            return _terrainTypeIndex; 
        }
        set {
            if (_terrainTypeIndex != value) {
                _terrainTypeIndex = value;
                ShaderData.RefreshTerrain(this);
            }
        }
    }

    public bool HasWalls {
        get { 
            return _hasWalls; 
        }

        set {
            if (_hasWalls != value) {
                _hasWalls = value;
                Refresh();
            }
        }
    }

    public int WaterLevel {
        get {
            return _waterLevel;
        }

        set {
            if (_waterLevel == value) {
                return;
            }

            int originalViewElevation = ViewElevation;

            _waterLevel = value;

            if (ViewElevation != originalViewElevation) {
                ShaderData.ViewElevationChanged();
            }

            ValidateRivers();
            Refresh();
        }
    }

    public int UrbanLevel {
        get { 
            return _urbanLevel;  
        }

        set {
            if (_urbanLevel != value) {
                _urbanLevel = Mathf.Clamp(value, 0, MaxFeatureLevel);
                RefreshSelfOnly();
            }
        }
    }

    public int FarmLevel {
        get {
            return _farmLevel;
        }

        set {
            if (_farmLevel != value) {
                _farmLevel = Mathf.Clamp(value, 0, MaxFeatureLevel);
                RefreshSelfOnly();
            }
        }
    }

    public int PlantLevel {
        get {
            return _plantLevel;
        }

        set {
            if (_plantLevel != value) {
                _plantLevel = Mathf.Clamp(value, 0, MaxFeatureLevel);
                RefreshSelfOnly();
            }
        }
    }

    public int SpecialIndex {
        get { 
            return _specialIndex; 
        }

        set {
            if (_specialIndex != value && !HasRiver)
            {
                _specialIndex = value;
                RemoveRoads();
                RefreshSelfOnly();
            }
        }
    }

    public int Distance {
        get { 
            return _distance; 
        }

        set {
            _distance = value;
        }
    }

    public bool IsExplored
    {
        get {
            return _explored && IsExplorable;
        }

        private set { 
            _explored = value; 
        }
    }

    public int SearchHeuristic { get; set; }
    public HexCell PathFrom { get; set; }
    public HexCell NextWithSamePriority { get; set; }
    public int SearchPhase { get; set; }
    public HexUnit Unit { get; set; }
    public CellShaderData ShaderData { get; set; }
    public int Index { get; set; }
    public int ColumnIndex { get; set; }
    public bool IsExplorable { get; set; }

    /// <summary>
    /// A new list composed of the neighbors of the cell.
    /// </summary>
    /// <value></value>
    public List<HexCell> Neighbors {
        get {
            List<HexCell> result = new List<HexCell>();

            for (int i = 0; i < 6; i++) {
                if (GetNeighbor((HexDirection)i)) {
                    result.Add(GetNeighbor((HexDirection)i));
                }
            }

            return result;
        }
    }

// ~~ private

// INDEXERS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// METHODS ~~~~~~~~~

// ~ Static

// ~~ public
    public static HexCell GetCell() {
        GameObject resultObj = new GameObject("Hex Cell");
        HexCell resultMono = resultObj.AddComponent<HexCell>();
        resultMono._roads = new bool[6];
        resultMono._neighbors = new HexCell[6];
        return resultMono;
    }

// ~~ private

// ~ Non-Static

// ~~ public
    public void ResetVisibility() {
        if (_visibility > 0)
        {
            _visibility = 0;
            ShaderData.RefreshVisibility(this);
        }
    }

    public HexCell GetNeighbor(HexDirection direction) {
        return _neighbors[(int)direction];
    }

/* Should consider adding edge checking for this method, but for now will only use it
* in the case where there is a neighbor.
*/
    public EdgeType GetEdgeType(HexDirection direction) {
        return HexagonPoint.GetEdgeType(Elevation, _neighbors[(int)direction].Elevation);
    }

    public void SetNeighborPair(HexDirection direction, HexCell cell) {
//Add the argument cell as neighbor at the specified direction.
        _neighbors[(int)direction] = cell;

/*Make the argument cell a neighbor of this cell using an extension method of
* the Direction class
*/

        cell._neighbors[(int)direction.Opposite()] = this;
    }

    public EdgeType GetEdgeType(HexCell otherCell) {
        return HexagonPoint.GetEdgeType(Elevation, otherCell.Elevation);
    }

    public bool HasRiverThroughEdge(HexDirection direction) {
        return
            _hasIncomingRiver && _incomingRiver == direction ||
            _hasOutgoingRiver && _outgoingRiver == direction;
    }

    public void SetOutgoingRiver(HexDirection direction) {
        // Outgoing river already present, return.
        if (_hasOutgoingRiver && _outgoingRiver == direction)
        {
            return;
        }

/* If neighbor does not exist, or is at a higher
* elevation, return as rivers do not flow uphill.
* Will need to add support for rivers flowing off
* the edge of a hex cell eventually to support
* the aesthetic of the Project Root.
*/
        HexCell neighbor = GetNeighbor(direction);
        if (!IsValidRiverDestination(neighbor))
        {
            return;
        }

/* Remove previous outgoing river if it exists, and
* remove incoming river it it overlaps with the
* position of the new outgoing river.
*/
        RemoveOutgoingRiver();
        if (HasIncomingRiver && IncomingRiver == direction)
        {
            RemoveIncomingRiver();
        }

        _hasOutgoingRiver = true;
        _outgoingRiver = direction;
        _specialIndex = 0;

/* Reset the incoming river of neighbor corresponding
* to the provided direction so it matches the outgoing
* river of this Cell.
*/
        neighbor.RemoveIncomingRiver();
        neighbor._hasIncomingRiver = true;
        neighbor._incomingRiver = direction.Opposite();
        neighbor._specialIndex = 0;

        SetRoad((int)direction, false);
    }

    public void RemoveOutgoingRiver() {
        if (!_hasOutgoingRiver)
        {
            return;
        }

        _hasOutgoingRiver = false;
        RefreshSelfOnly();

/* Can access private variable of neighbor because it is the same 
* class. While this is allowed, I dont care much for this approach
* as it is on the borderline of what I would consider sufficient
* information hiding.
*/
        HexCell neighbor = GetNeighbor(_outgoingRiver);
        neighbor._hasIncomingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver() {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void RemoveIncomingRiver() {
        if (!_hasIncomingRiver)
        {
            return;
        }

        _hasIncomingRiver = false;
        RefreshSelfOnly();

        HexCell neighbor = GetNeighbor(_incomingRiver);
        neighbor._hasOutgoingRiver = false;
        neighbor.RefreshSelfOnly();
    }

    public int GetElevationDifference(HexDirection direction) {
        int difference = Elevation - GetNeighbor(direction)._elevation;
        return difference >= 0 ? difference : -difference;
    }

    public void AddRoad(HexDirection direction) {
        if 
        (
            !_roads[(int)direction] && !HasRiverThroughEdge(direction) &&
            !IsSpecial && !GetNeighbor(direction).IsSpecial &&
            GetElevationDifference(direction) <= 1
        )
        {
            SetRoad((int)direction, true);
        }
    }

    public void RemoveRoads() {
        for (int i = 0; i < _neighbors.Length; i++)
        {
            if (_roads[i])
            {
                SetRoad(i, false);
            }
        }
    }

    public bool HasRoadThroughEdge(HexDirection direction) {
        return _roads[(int)direction];
    }

/* Most values are stored as integers, which will stay inside
* the range of 0-255. Only the first byte of each integer will be
* used, and therefore the other three bytes will always be zero.
*/
    public void Save(BinaryWriter writer) {
        writer.Write((byte)_terrainTypeIndex);
        writer.Write((byte)(_elevation + 127));
        writer.Write((byte)_waterLevel);
        writer.Write((byte)_urbanLevel);
        writer.Write((byte)_farmLevel);
        writer.Write((byte)_plantLevel);
        writer.Write((byte)_specialIndex);
        writer.Write(_hasWalls);

        if (_hasIncomingRiver)
        {
/* If an incoming river exists, add a check bit
* to the byte to indicate that a river exists.
*/
            writer.Write((byte)(_incomingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }
        
        if (_hasOutgoingRiver)
        {
            writer.Write((byte)(_outgoingRiver + 128));
        }
        else
        {
            writer.Write((byte)0);
        }

        int roadFlags = 0;
        for (int i = 0; i < _roads.Length; i++)
        {
            if (_roads[i])
            {
                roadFlags |= 1 << i;
            }
        }
        writer.Write((byte)roadFlags);
        writer.Write(IsExplored);
    }

/* Fields must be read back in the same order they were written above.
* Data structure could be made explicit with a Queue.
*/
    public void Load(
        BinaryReader reader,
        int header,
        float cellOuterRadius) {
        _terrainTypeIndex = reader.ReadByte();
        ShaderData.RefreshTerrain(this);
        _elevation = reader.ReadByte();

        if (header >= 4)
        {
            _elevation -= 127;
        }

        RefreshPosition(cellOuterRadius);
        _waterLevel = reader.ReadByte();
        _urbanLevel = reader.ReadByte();
        _farmLevel = reader.ReadByte();
        _plantLevel = reader.ReadByte();
        _specialIndex = reader.ReadByte();
        _hasWalls = reader.ReadBoolean();

/* Read back the river data by inspecting the check bit.
* If the check bit is present, convert the byte back to
* a meaningful value and the store it in
* incomingRiver/outgoingRiver.
*/
        byte riverData = reader.ReadByte();

        if (riverData >= 128)
        {
            _hasIncomingRiver = true;
            _incomingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            _hasIncomingRiver = false;
        }

        riverData = reader.ReadByte();
        if (riverData >= 128)
        {
            _hasOutgoingRiver = true;
            _outgoingRiver = (HexDirection)(riverData - 128);
        }
        else
        {
            _hasOutgoingRiver = false;
        }

        int roadFlags = reader.ReadByte();

        for (int i = 0; i < _roads.Length; i++)
        {
            _roads[i] = (roadFlags & (1 << i)) != 0;
        }

        IsExplored = header >= 3 ? reader.ReadBoolean() : false;
        ShaderData.RefreshVisibility(this);
    }

    public void DisableHighlight() {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.enabled = false;
    }

    public void EnableHighlight(Color color) {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.color = color;
        highlight.enabled = true;
    }

    public void IncreaseVisibility() {
        _visibility += 1;
        if (_visibility == 1)
        {
            IsExplored = true;
            ShaderData.RefreshVisibility(this);
        }
    }

    public void DecreaseVisibility() {
        _visibility -= 1;
        if (_visibility == 0)
        {
            ShaderData.RefreshVisibility(this);
        }
    }

    public void SetMapData(float data) {
        ShaderData.SetMapData(this, data);
    }

    public void SetLabel(string text) {
        UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }

// ~~ private
    private void Awake() {
        //SetEnabledInteractionMesh(true);
    }
    private void SetRoad(int index, bool state) {
        _roads[index] = state;
        _neighbors[index]._roads[(int)((HexDirection)index).Opposite()] = state;
        _neighbors[index].RefreshSelfOnly();
        RefreshSelfOnly();
    }

    private bool IsValidRiverDestination(HexCell neighbor) {
        return neighbor &&
        (
            _elevation >= neighbor._elevation || _waterLevel == neighbor._elevation
        );
    }

    private void Refresh() {
/* If chunk doesn't exist, cell has not been assigned yet and cannot be
* refreshed without a null reference exception.
*/        
        if (chunk)
        {
            chunk.Refresh();

/* If the chunk has neighbors in another chunk, refresh them as well to 
* avoid color / vertex seams.
*/
            for (int i = 0; i < _neighbors.Length; i++)
            {
                HexCell neighbor = _neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }

            if (Unit)
            {
                Unit.ValidateLocation();
            }
        }
    }

    private void ValidateRivers() {
        if 
        (
            _hasOutgoingRiver && 
            !IsValidRiverDestination(GetNeighbor(_outgoingRiver))
        )
        {
            RemoveOutgoingRiver();
        }

        if
        (
            _hasIncomingRiver &&
            !GetNeighbor(_incomingRiver).IsValidRiverDestination(this)
        )
        {
            RemoveIncomingRiver();
        }
    }

    private void RefreshSelfOnly() {
        chunk.Refresh();

        if (Unit)
        {
            Unit.ValidateLocation();
        }
    }

    private void RefreshPosition(float cellOuterRadius) {
        Vector3 position = transform.localPosition;
        position.y = _elevation * HexagonPoint.elevationStep;

        position.y +=
            (
                HexagonPoint.SampleNoise(
                    position,
                    cellOuterRadius
                ).y * 2f - 1f
            ) * HexagonPoint.elevationPerturbStrength;

        transform.localPosition = position;

/* Adjust the position of the cells UI elements
* when the elevation of the cell itself has changed.
* Because UI elements are laid down flat on the cell,
* the forward facing Z axis is adjusted instead of the
* upward facing Y axis.
*/
        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = -position.y;
        uiRect.localPosition = uiPosition;
    }

// STRUCTS ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private

// CLASSES ~~~~~~~~~~

// ~ Static

// ~~ public

// ~~ private

// ~ Non-Static

// ~~ public

// ~~ private
}
