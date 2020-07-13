using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ReadonlyIdAttribute : PropertyAttribute { }


[CustomPropertyDrawer(typeof(ReadonlyIdAttribute))]
public class ReadonlyIdDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var root = new VisualElement();

        // Create property fields.
        var id = property.FindPropertyRelative("unit");
        var idLbl = new Label(id.stringValue);

        root.Add(idLbl);

        return root;
    }
}