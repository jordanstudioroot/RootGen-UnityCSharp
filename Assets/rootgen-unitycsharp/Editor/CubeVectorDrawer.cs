#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(CubeVector))]
public class CubeVectorDrawer : PropertyDrawer {
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    ) {
        CubeVector cubeVector = new CubeVector(
            property.FindPropertyRelative("_x").intValue,
            property.FindPropertyRelative("_z").intValue
        );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, cubeVector.ToString());
    }
}
#endif