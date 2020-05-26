#if(UNITY_EDITOR)
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(CubeVector))]
public class BiomeDrawer : PropertyDrawer {
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    ) {
        Biome biome = new Biome(
            (Terrains)property.FindPropertyRelative("Terrain").intValue,
            property.FindPropertyRelative("plant").intValue
        );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, biome.ToString());
    }
}
#endif