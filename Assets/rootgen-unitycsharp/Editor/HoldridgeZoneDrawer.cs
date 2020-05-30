#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HoldridgeZone))]
public class HoldridgeZoneDrawer : PropertyDrawer {
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label
    ) {
        HoldridgeZone holdridgeZone = new HoldridgeZone(
            (LZone)property.FindPropertyRelative("lifeZone").intValue,
            (ABelt)property.FindPropertyRelative("altitudinalBelt").intValue,
            (LRegion)property.FindPropertyRelative("latitudinalRegion").intValue
        );

        position = EditorGUI.PrefixLabel(position, label);
        GUI.Label(position, holdridgeZone.ToString());
    }
}
#endif