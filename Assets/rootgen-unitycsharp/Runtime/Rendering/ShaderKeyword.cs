using UnityEngine;
public static class ShaderKeyword {
    private const string EditModeKeyword = "HEX_MAP_EDIT_MODE";
    private const string GridOnKeyword = "GRID_ON";

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