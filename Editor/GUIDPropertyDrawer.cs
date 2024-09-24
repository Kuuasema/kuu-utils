using System;
using UnityEditor;
using UnityEngine;

// based on: https://forum.unity.com/threads/cannot-serialize-a-guid-field-in-class.156862/

namespace Kuuasema.Utils {

    [CustomPropertyDrawer(typeof(GUID))]
    public class GUIDPropertyDrawer : PropertyDrawer {
    
    
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            if (Selection.objects.Length > 1) {
                EditorGUI.HelpBox(position, "GUID does not support multiple selection.", MessageType.Info);
                return;
            }

            // Start property draw
            EditorGUI.BeginProperty(position, label, property);
    
            // Get property
            SerializedProperty serializedPart1 = property.FindPropertyRelative("Part1");
            SerializedProperty serializedPart2 = property.FindPropertyRelative("Part2");
            SerializedProperty serializedPart3 = property.FindPropertyRelative("Part3");
            SerializedProperty serializedPart4 = property.FindPropertyRelative("Part4");
    
            // prefix
            position = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, position.height), GUIUtility.GetControlID(FocusType.Passive), label);
            
            // label
            label = new GUIContent(((GUID)property.boxedValue).ToString());
            EditorGUI.LabelField(new Rect(position.x, position.y, position.width - 100, position.height), label);
            position.x += position.width - 100;
            position.width = 100;

            // button
            if (GUI.Button(new Rect(position.x, position.y, position.width, position.height), "New")) {
                property.boxedValue = new GUID(Guid.NewGuid());
            }
    
            // End property
            EditorGUI.EndProperty();
        }
    
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}