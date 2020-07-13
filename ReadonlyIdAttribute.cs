using UnityEditor;
using UnityEngine;

public class ReadonlyInInspectorAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(ReadonlyInInspectorAttribute))]
public class ReadonlyInInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}

