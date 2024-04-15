
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IMovement), true)]
public class IMovementDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.ObjectField(position, property, label);
        EditorGUI.EndProperty();
    }
}

