using UnityEngine;

/// <summary>
/// Static helper class for enabling and disabling shader keywords
/// for the HexGrid.
/// </summary>
public static class HexGridShaderKeywords {
/// <summary>
/// Switches shader rendering between edit mode and game mode, disabling
/// and enabling features such as fog of war.
/// </summary>
    private const string EditModeKeyword = "HEX_MAP_EDIT_MODE";

/// <summary>
/// Switches the grid outline shader on and off.
/// </summary>
    private const string GridOnKeyword = "GRID_ON";

/// <summary>
/// Switches shader rendering between edit mode and game mode, disabling
/// and enabling features such as fog of war.
/// </summary>
    public static bool HexMapEditMode {
        set {
            if (value) {
                Shader.EnableKeyword(EditModeKeyword);
            }
            else {
                Shader.DisableKeyword(EditModeKeyword);
            }
        }
    }

/// <summary>
/// Switches the grid outline shader on and off.
/// </summary>
    public static bool GridOn {
        set {
            if (value) {
                Shader.EnableKeyword(GridOnKeyword);
            }
            else {
                Shader.DisableKeyword(GridOnKeyword);
            }
        }
    }
}