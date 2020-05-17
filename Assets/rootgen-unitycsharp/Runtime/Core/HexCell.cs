using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HexCell : MonoBehaviour, IHexPoint {
    private static int MaxFeatureLevel {
        get {
            return 3;
        }
    }
    public RectTransform uiRect;
    public HexGridChunk chunk;
    private int _visibility;

    public Vector3 Position {
        get {
            return transform.localPosition;
        }
    }

    public float StreamBedY {
        get {
            return
                (Elevation + HexagonPoint.streamBedElevationOffset) *
                HexagonPoint.elevationStep;
        }
    }

    public float RiverSurfaceY {
        get {
            return
                (Elevation + HexagonPoint.waterElevationOffset) *
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
            return WaterLevel > Elevation;
        }
    }

    public bool IsSpecial {
        get { 
            return SpecialIndex > 0; 
        }
    }

    public int ViewElevation {
        get { 
            return Elevation >= WaterLevel ? 
                Elevation : WaterLevel;
        }
    }

    public bool IsVisible {
        get { 
            return _visibility > 0 && IsExplorable; 
        }
    }

    public CubeVector HexCoordinates { get; set; }

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

    public int Elevation { get; set; }

    public void SetElevation(
        int elevation,
        float cellOuterRadius,
        int wrapSize
    ) {
        if (Elevation == elevation) {
            return;
        }

        int originalViewElevation = ViewElevation;

        Elevation = elevation;

        if (ViewElevation != originalViewElevation) {
            ShaderData.ViewElevationChanged();
        }

        RefreshPosition(
            cellOuterRadius,
            wrapSize
        );
    }

    public int TerrainTypeIndex { get; set; }
    public bool HasWalls { get; set; }
    public int WaterLevel { get; set; }
    public int UrbanLevel { get; set; }
    public int FarmLevel { get; set; }
    public int PlantLevel { get; set; }
    public int SpecialIndex { get; set; }
    public bool IsExplored { get; set; }
    public int SearchHeuristic { get; set; }
    public HexCell PathFrom { get; set; }
    public HexCell NextWithSamePriority { get; set; }
    public int SearchPhase { get; set; }
    public HexUnit Unit { get; set; }
    public CellShaderData ShaderData { get; set; }
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
    public static HexCell Instantiate() {
        GameObject resultObj = new GameObject("HexCell");
        HexCell resultMono = resultObj.AddComponent<HexCell>();
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

    public ElevationEdgeTypes GetEdgeType(HexCell otherCell) {
        return HexagonPoint.GetEdgeType(Elevation, otherCell.Elevation);
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
///     Set the map data visualization value for this hex cell cell,
///     and enable the associated shader.
///     TODO: This method has too many cross cutting concerns. Should
///           separate data and rendering concerns.
/// </summary>
/// <param name="data">
///     The value representing the magnitude of the data point
///     being represented.
/// </param>
    public void SetAndEnableMapVisualizationShaderData(float data) {
        ShaderData.SetAndEnableMapVisualizationShaderData(this, data);
    }

/// <summary>
///     Set the text label for this HexCell.
/// 
///     TODO: This method has too many cross cutting concerns. Should
///           separate data and rendering concerns.
/// </summary>
/// <param name="text">
///     The text that the HexCell should display when the HexCell ui is visible.
/// </param>
    public void SetLabel(string text) {
        UnityEngine.UI.Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }

    public override string ToString() {
        return HexCoordinates.ToString();
    }

    private void Awake() {
        //SetEnabledInteractionMesh(true);
        Elevation = int.MinValue;
    }

    private bool IsValidRiverDestination(HexCell neighbor) {
        return neighbor &&
        (
            Elevation >= neighbor.Elevation || WaterLevel == neighbor.Elevation
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
        float cellOuterRadius,
        int wrapSize
    ) {
        Vector3 position = transform.localPosition;
        position.y = Elevation * HexagonPoint.elevationStep;

        position.y +=
            (
                HexagonPoint.SampleNoise(
                    position,
                    cellOuterRadius,
                    wrapSize
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
}
