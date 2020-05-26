#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(ClimateData))]
public class ClimateDataDrawer : PropertyDrawer {
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    ) {
        ClimateData cubeVector = new ClimateData(
            property.FindPropertyRelative("clouds").floatValue,
            property.FindPropertyRelative("moisture").floatValue,
            property.FindPropertyRelative("temperature").floatValue
        );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, cubeVector.ToString());
    }
}
#endif