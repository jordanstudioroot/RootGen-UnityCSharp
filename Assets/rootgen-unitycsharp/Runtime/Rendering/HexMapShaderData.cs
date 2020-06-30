using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the shader data for a hex map.
/// </summary>
public class HexMapShaderData : MonoBehaviour {
    private Texture2D _hexTexture;
    private Color32[] _hexTerrainData;
    private bool _needsVisibilityReset;

    private List<Hex> _transitioningHexes = new List<Hex>();

    private const float _transitionSpeed = 255f;

    public bool ImmediateMode { get; set; }
    public HexMap HexMap { get; set; }

    public void Initialize(
        int hexMapColumns,
        int hexMapRows
    ) {

        // Initialize the texture for the map, or create a new texture if
        // none exists.
        if (_hexTexture) {
            _hexTexture.Resize(
                hexMapColumns,
                hexMapRows
            );
        }
        else {
            _hexTexture = new Texture2D(
                hexMapColumns,
                hexMapRows,
                TextureFormat.RGBA32,
                false,
                true
            );

            _hexTexture.filterMode = FilterMode.Point;
            _hexTexture.wrapModeU = TextureWrapMode.Repeat;
            _hexTexture.wrapModeV = TextureWrapMode.Clamp;
            
            // Create a global property for all shaders, referencing the
            // hex map's texture.
            Shader.SetGlobalTexture("_hexData", _hexTexture);
        }
        
        // Create a global property for all shaders, referencing the texel
        // size of the hex map given the number of columns and rows in the
        // hex map.
        Shader.SetGlobalVector(
            "_hexData_TexelSize",
            new Vector4(
                1f / hexMapColumns,
                1f / hexMapRows,
                hexMapColumns,
                hexMapRows
            )
        );

        // If the texture data is null or does not match the size of the
        // map.
        if (
            _hexTerrainData == null ||
            _hexTerrainData.Length != hexMapColumns * hexMapRows
        ) {
            // Create a new texture data array.
            _hexTerrainData = new Color32[hexMapColumns * hexMapRows];
        }
        else {
            for (int i = 0; i < _hexTerrainData.Length; i++) {
                // Else reinitialize the array with all black.
                _hexTerrainData[i] = new Color32(0, 0, 0, 0);
            }
        }

        // Clear any hexes which are in the middle of a fog of war
        // transition.
        _transitioningHexes.Clear();

        // Enable this component.
        enabled = true;
    }

    /// <summary>
    /// Update the global texture for the hex map with the terrain type
    /// of the hex.
    /// </summary>
    /// <param name="hex">
    /// A hex to update the global hex map texture with.
    /// </param>
    public void RefreshTerrain(Hex hex) {
        _hexTerrainData[hex.Index].a = (byte)hex.HoldrigeZone.lifeZone;
        enabled = true;
    }

    /// <summary>
    /// Refresh the fog of war for the specified hex.
    /// </summary>
    /// <param name="hex"></param>
    public void RefreshVisibility(Hex hex) {
        int index = hex.Index;

        // If the fog of war transition is set to immediate mode...
        if (ImmediateMode) {

            // ...instantly set the r and g channels to white (visible) or
            // black (not visible) in the global texture data for the hex
            // map.
            _hexTerrainData[index].r =
                hex.IsVisible ? (byte)255 :
                (byte)0;

            _hexTerrainData[index].g = hex.IsExplored ?
                (byte)255 :
                (byte)0;
        }
        else if (_hexTerrainData[index].b != 255) {
            // .. else set the b channel to 255 and add it to the list of
            // transitioning hexes.
            _hexTerrainData[index].b = 255;
            _transitioningHexes.Add(hex);
        }

        enabled = true;
    }

    /// <summary>
    /// Sets the b channel of the terrain color data for the provided hex
    /// to a magnitude which reflects the magnitude of the provided data.
    /// </summary>
    /// <param name="hex">
    /// The hex for which the color data should be changed.
    /// </param>
    /// <param name="data">
    /// A value representing the magnitude of that the b channel should be
    /// set to, clamped betwen 0 and 254.
    /// </param>
    public void SetMapData (Hex hex, float data) {
        // Set the b channel of the global texture data for the hex..
		_hexTerrainData[hex.Index].b =
			data < 0f ? 
                // ...to 0 black if the data value is negative...
                (byte)0 :
                (data < 1f ?
                    // ...to to a some value between 0 and 245 if the data
                    // ...value is less than 1...
                    (byte)(data * 254f) :
                    // ...to 254 if the data value is greater than 1.
                    (byte)254);

		enabled = true;
	}

    /// <summary>
    /// A psuedo event to be triggered when the view elevation of a cell
    /// changes. Updates the visibility for the hex map.
    /// </summary>
    public void ViewElevationChanged() {
        _needsVisibilityReset = true;
        enabled = true;
    }

    private void LateUpdate() {
        if (_needsVisibilityReset) {
            _needsVisibilityReset = false;
            // TODO: This creates a circular dependency,
            // HexMapShaderData -> HexMap -> Hex -> HexMapShaderData
            // Resets the visibility of all hexes in the hex map
            // to 0 and calls refresh visibility to update the
            // terrain color data with the new visibility.
            HexMap.ResetVisibility();
        }

        // Get the current delta based on time and fog of war transition
        // speed.
        int delta = (int)(Time.deltaTime * _transitionSpeed);

        // Loop the delta around if it gets to 0.
        if (delta == 0) {
            delta = 1;
        }

        // For each hex transitioning to visible.
        for (int i = 0; i < _transitioningHexes.Count; i++) {
            // If the hex is not transitioning to being visible
            // or not visible.
            if (!UpdateHexData(_transitioningHexes[i], delta)) {
                // Remove the hex from the list of transitioning hexes and
                // set the end of the array to be the next hex to transition.
                _transitioningHexes[i--] =
                    _transitioningHexes[_transitioningHexes.Count - 1];
                _transitioningHexes.RemoveAt(_transitioningHexes.Count - 1);
            }
        }

        // Update the global texture data for the hex map with
        // the current terrain data
        _hexTexture.SetPixels32(_hexTerrainData);
        _hexTexture.Apply();
        enabled = _transitioningHexes.Count > 0;
    }

    private bool UpdateHexData(Hex hex, int delta) {
        int index = hex.Index;
        // Get the texture color data for this hex.
        Color32 data = _hexTerrainData[index];
        bool stillUpdating = false;

        // If the hex is explored and the g channel is less than 255
        if (hex.IsExplored && data.g < 255) {
            // The hex is still transitioning
            stillUpdating = true;
            // Set the g channel to the g value for the current time
            int timeG = data.g + delta;
            data.g =
                timeG >= 255 ?
                    (byte)255 : (byte)timeG;
        }

        if (hex.IsVisible && data.r < 255) {
            // If the hex is visible and the r channel is less than 255,
            // do the same as above.
            stillUpdating = true;
            int timeRVisible = data.r + delta;
            data.r =
                timeRVisible >= 255 ?
                    (byte)255 : (byte)timeRVisible;
        }
        else if (data.r > 0) {
            // If the hex is not visible and the r cahnnel is not negative,
            // the hex is transitioning to not visible.
            stillUpdating = true;
            int timeRNotVisible = data.r - delta;
            data.r =
                timeRNotVisible < 0 ?
                    (byte)0 : (byte)timeRNotVisible;
        }

        if (!stillUpdating) {
            // If the hex is neither explored, visible, or transitioning to
            // not being visible, set the b channel of the texture color 
            // data to 0.
            data.b = 0;
        }

        // Update the texture color data for the hex with the new color data
        // and return the status of whether the hex is still updating its
        // visibility. 
        _hexTerrainData[index] = data;
        return stillUpdating;
    }  
}
