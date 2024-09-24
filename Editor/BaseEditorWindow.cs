using UnityEngine;
using UnityEditor;
namespace Kuuasema.Utils {

    public static class BaseGUI {
        public enum TexColor {
            Black,
            DarkGrey,
            Grey,
            LightGrey,
            // White,
            ENUM_END
        }

        public static Texture2D[] texColors;
        public static Texture2D[] TexColors => texColors;
        public static GUIStyle dropBoxStyle;
        public static GUIStyle centerLabelStyle;

        public static GUIStyle moduleStyle;
        public static GUIStyle ModuleStyle { get { if (!stylesCreated) { CreateStyles(); } return moduleStyle; } }
        private static bool stylesCreated;

        public static void CreateStyles() {
            if (stylesCreated) return;
            stylesCreated = true;

            texColors = new Texture2D[(int)TexColor.ENUM_END];
            int max = (int)TexColor.ENUM_END;
            for (int i = 0; i < max; i++) {
                texColors[i] = MakeTex(9, 9, Color.Lerp(Color.black, Color.gray, (float) i / (float) (max-1)), Color.Lerp(Color.gray, Color.black, (float) i / (float) (max-1)));
            }

        
            centerLabelStyle = new GUIStyle(GUI.skin.label);
            centerLabelStyle.alignment = TextAnchor.MiddleCenter;
            centerLabelStyle.fontStyle = FontStyle.Bold;
            centerLabelStyle.normal.textColor = Color.black;
            centerLabelStyle.hover.textColor = Color.white;
        
    
            dropBoxStyle = new GUIStyle(GUI.skin.box);
            dropBoxStyle.normal.background = texColors[(int)TexColor.LightGrey];
            dropBoxStyle.hover.background = texColors[(int)TexColor.Black];


            moduleStyle = new GUIStyle(GUI.skin.window);
            moduleStyle.normal.background = texColors[(int)TexColor.LightGrey];
            moduleStyle.contentOffset = Vector2.zero;
            moduleStyle.padding = new RectOffset(4, 4, 4, 4);
        }
        private static Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i) {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        private static Texture2D MakeTex(int width, int height, Color col1, Color col2) {
            Color[] pix = new Color[width * height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int i = x + y * width;
                    Color col = col1;
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                        col = col2;
                    }
                    pix[i] = col;
                }    
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        public static void BeginTitledSection(string title) {
            GUILayout.BeginVertical(GUIContent.none, BaseGUI.ModuleStyle);
            GUILayout.Label(title, EditorStyles.boldLabel); 
            EditorModule.DrawSeparator();
        }

        public static void EndTitledSection() {
            GUILayout.EndVertical();
        }
        
        public static void DrawPropertyField(SerializedObject serializedObject, SerializedProperty serializedProperty) {
            serializedObject.Update();
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedProperty, true);
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        public static void DrawPropertyField(SerializedObject serializedObject, string propertyName) {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null) {
                DrawPropertyField(serializedObject, property);
            } else {
                EditorGUILayout.HelpBox($"Property named '{propertyName}' not found in {serializedObject}", MessageType.Warning);
            }
        }

        public static void DrawPropertyFields(SerializedObject serializedObject, params string[] propertyNames) {
            foreach (string propertyName in propertyNames) {
                SerializedProperty property = serializedObject.FindProperty(propertyName);
                if (property != null) {
                    DrawPropertyField(serializedObject, property);
                } else {
                    EditorGUILayout.HelpBox($"Property named '{propertyName}' not found in {serializedObject}", MessageType.Warning);
                }
            }
        }
    }

    public class BaseEditorWindow : EditorWindow {

        protected static T OpenWindow<T>(string title = null) where T : BaseEditorWindow {
            T window = GetWindow<T>();
            if (string.IsNullOrWhiteSpace(title)) {
                title = TextUtils.PadCapitalizedString(typeof(T).Name);
            }
            window.titleContent = new GUIContent(title);
            return window;
        }

        protected virtual void OnEnable() {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        protected virtual void OnDisable() {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        protected virtual void CreateGUI() {
            
        }

        protected virtual void OnGUI() {
            
        }

        protected virtual void OnSceneGUI(SceneView sceneView) {

        }

        protected virtual void OnSelectionChange() {
            this.Repaint();
        }
    }
}