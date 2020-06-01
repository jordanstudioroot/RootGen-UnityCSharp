#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomPropertyDrawer(typeof(HexRiverData))]
public class HexRiverDataDrawer : PropertyDrawer {
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    ) {
        HexRiverData hexRiverData = new HexRiverData(
            (HexDirections)property.FindPropertyRelative(
                "_incomingRiverDirection"
            ).intValue,
            (HexDirections)property.FindPropertyRelative(
                "_outgoingRiverDirection"
            ).intValue,
            property.FindPropertyRelative("HasIncomingRiver").boolValue,
            property.FindPropertyRelative("HasOutgoingRiver").boolValue
        );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, hexRiverData.ToString());
    }
}
#endif