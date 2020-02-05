#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

public class TextureArrayWizard : ScriptableWizard
{
    public Texture2D[] textures;

    [MenuItem("Assets/Create/TextureArray")]
    private static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<TextureArrayWizard>
        (
            "CreateTextureArray", "Create"
        );
    }

    private void OnWizardCreate()
    {
        if (textures.Length == 0)
        {
            return;
        }

        string path = EditorUtility.SaveFilePanelInProject
        (
            "Save Texture Array", 
            "Texture Array", 
            "asset", 
            "Save Texture Array"
        );

        if (path.Length == 0)
        {
            return;
        }

        Texture2D texture = textures[0];
        Texture2DArray textureArray = new Texture2DArray
        (
            texture.width, texture.height, textures.Length, texture.format, texture.mipmapCount > 1
        );

        textureArray.anisoLevel = texture.anisoLevel;
        textureArray.filterMode = texture.filterMode;
        textureArray.wrapMode = texture.wrapMode;

        for (int i = 0; i < textures.Length; i++)
        {
            for (int m = 0; m < texture.mipmapCount; m++)
            {
                Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
            }
        }

        AssetDatabase.CreateAsset(textureArray, path);
    }
}
#endif