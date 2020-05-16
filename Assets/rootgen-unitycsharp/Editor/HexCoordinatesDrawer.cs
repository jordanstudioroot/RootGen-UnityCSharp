#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HexVector))]
public class HexCoordinatesDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    ) {
        HexVector coordinates = new HexVector
            (
                property.FindPropertyRelative("x").intValue,
                property.FindPropertyRelative("z").intValue,
                property.FindPropertyRelative("wrapSize").intValue
            );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, coordinates.ToString());
    }
}
#endif