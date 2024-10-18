using UnityEditor;
using UnityEngine;

namespace Kuuasema.Utils {

    [CustomPropertyDrawer(typeof(DisableInspectorAttribute), true)]
    public class DisableInspectorDrawer : PropertyDrawer {

        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }
    }



}