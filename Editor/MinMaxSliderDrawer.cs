

using UnityEngine;
using UnityEditor;

namespace Kuuasema.Utils {

    [CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
    public class MinMaxSliderDrawer : PropertyDrawer {

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var minMaxAttribute = (MinMaxSliderAttribute)attribute;
            var propertyType = property.propertyType;
            Rect controlRect = EditorGUI.PrefixLabel(position, label);            
            if (propertyType == SerializedPropertyType.Vector2) { 

                EditorGUI.BeginChangeCheck();

                Vector2 vector = property.vector2Value;
                float minVal = vector.x;
                float maxVal = vector.y;

                const float FIELD_WIDTH = 60;
                Rect rect = controlRect;
                
                rect.width = FIELD_WIDTH;
                minVal = EditorGUI.FloatField(rect, minVal);

                rect.x += FIELD_WIDTH;
                rect.width = controlRect.width - FIELD_WIDTH * 2;
                EditorGUI.MinMaxSlider(rect, ref minVal, ref maxVal, minMaxAttribute.min, minMaxAttribute.max);

                rect.x += rect.width;
                rect.width = FIELD_WIDTH;
                maxVal = EditorGUI.FloatField(rect, maxVal);

                if (minVal < minMaxAttribute.min) {
                    minVal = minMaxAttribute.min;
                }

                if (maxVal > minMaxAttribute.max) {
                    maxVal = minMaxAttribute.max;
                }

                vector = new Vector2(minVal > maxVal ? maxVal : minVal, maxVal);

                if (EditorGUI.EndChangeCheck()) {
                    property.vector2Value = vector;
                }
            }
        }
    } 
}