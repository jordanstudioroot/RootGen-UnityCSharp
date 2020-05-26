using UnityEngine;
using System.Collections.Generic;

public class HexShaderData : MonoBehaviour
{
    private Texture2D _hexTexture;
    private Color32[] _hexTextureData;
    private bool _needsVisibilityReset;

    private List<Hex> _transitioningHexes = new List<Hex>();

    private const float _transitionSpeed = 255f;

    public bool ImmediateMode { get; set; }
    public HexMap HexMap { get; set; }

    public void Initialize(int x, int z) {
        if (_hexTexture) {
            _hexTexture.Resize(x, z);
        }
        else {
            _hexTexture = new Texture2D (
                x, z, TextureFormat.RGBA32, false, true
            );

            _hexTexture.filterMode = FilterMode.Point;
            _hexTexture.wrapModeU = TextureWrapMode.Repeat;
            _hexTexture.wrapModeV = TextureWrapMode.Clamp;

            Shader.SetGlobalTexture("_hexData", _hexTexture);
        }
        
        Shader.SetGlobalVector(
            "_hexData_TexelSize",
            new Vector4(1f / x, 1f / z, x, z)
        );

        if (
            _hexTextureData == null ||
            _hexTextureData.Length != x * z
        ) {
            _hexTextureData = new Color32[x * z];
        }
        else {
            for (int i = 0; i < _hexTextureData.Length; i++)
            {
                _hexTextureData[i] = new Color32(0, 0, 0, 0);
            }
        }

        _transitioningHexes.Clear();
        enabled = true;
    }

    public void RefreshTerrain(Hex hex)
    {
        _hexTextureData[hex.Index].a = (byte)hex.TerrainType;
        enabled = true;
    }

    public void RefreshVisibility(Hex hex)
    {
        int index = hex.Index;

        if (ImmediateMode)
        {
            _hexTextureData[index].r = hex.IsVisible ? (byte)255 : (byte)0;
            _hexTextureData[index].g = hex.IsExplored ? (byte)255 : (byte)0;
        }
        else if (_hexTextureData[index].b != 255)
        {
            _hexTextureData[index].b = 255;
            _transitioningHexes.Add(hex);
        }

        enabled = true;
    }

    public void SetMapData (Hex hex, float data) {
		_hexTextureData[hex.Index].b =
			data < 0f ? (byte)0 : (data < 1f ? (byte)(data * 254f) : (byte)254);
		enabled = true;
	}

    public void ViewElevationChanged()
    {
        _needsVisibilityReset = true;
        enabled = true;
    }

    private void LateUpdate()
    {
        if (_needsVisibilityReset)
        {
            _needsVisibilityReset = false;
            HexMap.ResetVisibility();
        }

        int delta = (int)(Time.deltaTime * _transitionSpeed);

        if (delta == 0)
        {
            delta = 1;
        }

        for (int i = 0; i < _transitioningHexes.Count; i++)
        {
            if (!UpdateHexData(_transitioningHexes[i], delta))
            {
                _transitioningHexes[i--] =
                    _transitioningHexes[_transitioningHexes.Count - 1];
                _transitioningHexes.RemoveAt(_transitioningHexes.Count - 1);
            }
        }

        _hexTexture.SetPixels32(_hexTextureData);
        _hexTexture.Apply();
        enabled = _transitioningHexes.Count > 0;
    }

    private bool UpdateHexData(Hex hex, int delta)
    {
        int index = hex.Index;
        Color32 data = _hexTextureData[index];
        bool stillUpdating = false;

        if (hex.IsExplored && data.g < 255)
        {
            stillUpdating = true;
            int time = data.g + delta;
            data.g = time >= 255 ? (byte)255 : (byte)time;
        }

        if (hex.IsVisible && data.r < 255)
        {
            stillUpdating = true;
            int t = data.r + delta;
            data.r = t >= 255 ? (byte)255 : (byte)t;
        }
        else if (data.r > 0)
        {
            stillUpdating = true;
            int t = data.r - delta;
            data.r = t < 0 ? (byte)0 : (byte)t;
        }

        if (!stillUpdating)
        {
            data.b = 0;
        }

        _hexTextureData[index] = data;
        return stillUpdating;
    }  
}
