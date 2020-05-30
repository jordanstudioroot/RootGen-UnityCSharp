using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Hex : MonoBehaviour, IHexPoint {
    [SerializeField]
    private ClimateData _climateData;
    [SerializeField]
    private HoldridgeZone _holdridgeZone;
    public ClimateData ClimateData { 
        get {
            return _climateData;
        } 
        
        set {
            _climateData = value;
        } 
    }

    public HoldridgeZone HoldrigeZone {
        get {
            return _holdridgeZone;
        }

        set {
            _holdridgeZone = value;
        }
    }

    [SerializeField]
    private CubeVector _cubeCoordinates;

    private static int MaxFeatureLevel {
        get {
            return 3;
        }
    }
    public RectTransform uiRect;
    public MapMeshChunk chunk;
    private int _visibility;

    public Vector3 Position {
        get {
            return transform.localPosition;
        }
    }

    public float StreamBedY {
        get {
            return
                (elevation + HexagonPoint.streamBedElevationOffset) *
                HexagonPoint.elevationStep;
        }
    }

    public float RiverSurfaceY {
        get {
            return
                (elevation + HexagonPoint.waterElevationOffset) *
                HexagonPoint.elevationStep;
        }
    }

    public float WaterSurfaceY {
        get {
            return
                (WaterLevel + HexagonPoint.waterElevationOffset) *
                HexagonPoint.elevationStep;
        }
    }

    public bool IsUnderwater {
        get {
            return WaterLevel > elevation;
        }
    }

    public bool IsSpecial {
        get { 
            return SpecialIndex > 0; 
        }
    }

    public int ViewElevation {
        get { 
            return elevation >= WaterLevel ? 
                elevation : WaterLevel;
        }
    }

    public bool IsVisible {
        get { 
            return _visibility > 0 && IsExplorable; 
        }
    }

    public CubeVector CubeCoordinates {
        get {
            return _cubeCoordinates;
        } 
        
        private set {
            _cubeCoordinates = value;
        } 
    }

    private Mesh GetInteractionMesh(float radius) {
        Mesh result = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Vector3 center = this.transform.position;
        Vector3 offset = (Vector3.up * .2f);

        for (int i = 0; i < 6; i++) {
            int index = i * 3;
            vertices.Add(center + offset);
            vertices.Add(HexagonPoint.GetCorner(i, radius) + offset);
            vertices.Add(HexagonPoint.GetCorner(i + 1, radius) + offset);

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }
        
        result.Clear();
        result.vertices = vertices.ToArray();
        result.triangles = triangles.ToArray();

        return result;
    }

    public void InteractionMeshEnabled(bool enabled, float outerRadius) {
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

    public int elevation;

    public void SetElevation(
        int elevation,
        float hexOuterRadius,
        int wrapSize
    ) {
        if (this.elevation == elevation) {
            return;
        }

        int originalViewElevation = ViewElevation;

        this.elevation = elevation;

        if (ViewElevation != originalViewElevation) {
            ShaderData.ViewElevationChanged();
        }

        RefreshPosition(
            hexOuterRadius,
            wrapSize
        );
    }

    public bool HasWalls { get; set; }
    public int WaterLevel { get; set; }
    public int UrbanLevel { get; set; }
    public int FarmLevel { get; set; }
    
    public int PlantLevel { 
        get {
            return 3 - (int)(
                3 * 
                ((float)_holdridgeZone.altitudinalBelt / 5f)
            );
        }
    }

    public int SpecialIndex { get; set; }
    public bool IsExplored { get; set; }
    public int SearchPhase { get; set; }
    public HexUnit Unit { get; set; }
    public HexMapShaderData ShaderData { get; set; }
    public int Index { get; set; }
    public int ColumnIndex { get; set; }
    public bool IsExplorable { get; set; }

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
    public static Hex Instantiate(
        int offsetX,
        int offsetZ,
        int wrapSize
    ) {
        GameObject resultObj = new GameObject("Hex");
        Hex resultMono = resultObj.AddComponent<Hex>();

        resultMono.CubeCoordinates = CubeVector.FromOffsetCoordinates(
            offsetX,
            offsetZ,
            wrapSize
        );

        return resultMono;
    }

// ~~ private

// ~ Non-Static

// ~~ public
    public void ResetVisibility() {
        if (_visibility > 0) {
            _visibility = 0;
            ShaderData.RefreshVisibility(this);
        }
    }

    public ElevationEdgeTypes GetEdgeType(Hex otherHex) {
        return HexagonPoint.GetEdgeType(elevation, otherHex.elevation);
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

/// <summary>
///     Set the map data visualization value for this hex,
///     and enable the associated shader.
///     TODO: This method has too many cross cutting concerns. Should
///           separate data and rendering concerns.
/// </summary>
/// <param name="data">
///     The value representing the magnitude of the data point
///     being represented.
/// </param>
    public void SetMapData(float data) {
        ShaderData.SetMapData(this, data);
    }

/// <summary>
///     Set the text label for this hex.
/// 
///     TODO: This method has too many cross cutting concerns. Should
///           separate data and rendering concerns.
/// </summary>
/// <param name="text">
///     The text that the hex should display when the hex ui is visible.
/// </param>
    public void SetLabel(string text) {
        UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }

    public override string ToString() {
        return 
            "[X: " +
                CubeCoordinates.X.ToString() + ", Y:" +
                CubeCoordinates.Y.ToString() + ", Z:" +
                CubeCoordinates.Z.ToString() +
            "]";
    }

    private void Awake() {
        //SetEnabledInteractionMesh(true);
        elevation = int.MinValue;
    }

    private bool IsValidRiverDestination(Hex neighbor) {
        return neighbor &&
        (
            elevation >= neighbor.elevation || WaterLevel == neighbor.elevation
        );
    }

    private void RefreshAttachedChunk() {
        chunk.Refresh();

        if (Unit)
        {
            Unit.ValidateLocation();
        }
    }

    private void RefreshPosition(
        float hexOuterRadius,
        int wrapSize
    ) {
        Vector3 position = transform.localPosition;
        position.y = elevation * HexagonPoint.elevationStep;

        position.y +=
            (
                HexagonPoint.SampleNoise(
                    position,
                    hexOuterRadius,
                    wrapSize
                ).y * 2f - 1f
            ) * HexagonPoint.elevationPerturbStrength;

        transform.localPosition = position;

/* Adjust the position of the hex UI elements
* when the elevation of the hex itself has changed.
* Because UI elements are laid down flat on the hex,
* the forward facing Z axis is adjusted instead of the
* upward facing Y axis.
*/
        Vector3 uiPosition = uiRect.localPosition;
        uiPosition.z = -position.y;
        uiRect.localPosition = uiPosition;
    }
}
