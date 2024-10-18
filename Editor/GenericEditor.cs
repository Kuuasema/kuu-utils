using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System;
using System.Linq;
using UnityEditor.Overlays;
namespace Kuuasema.Utils {

    public class GenericEditor : UnityEditor.Editor {

        public enum TexColor {
            Black,
            DarkGrey,
            Grey,
            LightGrey,
            // White,
            ENUM_END
        }

        protected static Texture2D[] texColors;
        public static Texture2D[] TexColors => texColors;
        protected static GUIStyle dropBoxStyle;
        protected static GUIStyle centerLabelStyle;

        protected static GUIStyle moduleStyle;
        public static GUIStyle ModuleStyle => moduleStyle;

        protected static GUIStyle boldFoldout;
        public static GUIStyle BoldFoldout => boldFoldout;

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

            boldFoldout = new GUIStyle(EditorStyles.foldout);
            boldFoldout.fontStyle = FontStyle.Bold;
        }

        protected static Texture2D MakeTex(int width, int height, Color col) {
            Color[] pix = new Color[width * height];
            for( int i = 0; i < pix.Length; ++i )
            {
                pix[ i ] = col;
            }
            Texture2D result = new Texture2D( width, height );
            result.SetPixels( pix );
            result.Apply();
            return result;
        }

        protected static Texture2D MakeTex(int width, int height, Color col1, Color col2) {
            Color[] pix = new Color[width * height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    int i = x + y * width;
                    Color col = col1;
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                        col = col2;
                    }
                    pix[ i ] = col;
                }    
            }
            Texture2D result = new Texture2D( width, height );
            result.SetPixels( pix );
            result.Apply();
            return result;
        }

        public static bool DropArea(Rect dropRect, string text, out UnityEngine.Object[] dropped) {
            dropped = null;
            Event evt = Event.current;

            bool dropContains = dropRect.Contains (evt.mousePosition);

            GUI.Box (dropRect, "", dropBoxStyle);
            GUI.Label(dropRect, text, centerLabelStyle);

            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform: {

                    if (!dropContains) {
                        return false;
                    }
                    
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                
                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag ();
                        dropped = DragAndDrop.objectReferences;
                        return true;
                    }
                } break;
            }
            return false;
        }

        public static void BeginTitledSection(string title) {
            
            GUILayout.BeginVertical(GUIContent.none, GenericEditor.ModuleStyle);
            GUILayout.Label(title, EditorStyles.boldLabel); 
            EditorModule.DrawSeparator();
        }

        public static bool BeginToggleSection(string title, ref bool toggleState) {
            GUILayout.BeginVertical(GUIContent.none, GenericEditor.ModuleStyle);
            EditorGUI.indentLevel++;
            toggleState = EditorGUILayout.Foldout(toggleState, title, GenericEditor.BoldFoldout); 
            EditorGUI.indentLevel--;
            if (toggleState) {
                EditorModule.DrawSeparator();
            }
            return toggleState;
        }

        public static void EndTitledSection() => GUILayout.EndVertical();
        public static void EndToggleSection() => GUILayout.EndVertical();
        
    }

    public class GenericEditor<T> : GenericEditor where T : UnityEngine.Object {

        public T Target { get; protected set; }

        protected virtual bool ShowDefaultInspector { get => this._show; set => this._show = value ; }
        private bool _show;

        private Dictionary<System.Type,EditorModule> modules = new Dictionary<System.Type,EditorModule>();

        protected virtual bool UseSceneGUI => false;
        protected virtual bool HasRunningTasks => false;

        public bool IsEnabled { get; private set; }

        protected virtual void OnEnable() {
            if (this.HasRunningTasks && this.Target == target) return;
            this.Target = (T) this.target;
            this.IsEnabled = true;
            this.OnEnableModules();
            if (!this.HasRunningTasks) {
                if (this.UseSceneGUI) {
                    SceneView.duringSceneGui += this.OnSceneGUI;
                }
                EditorApplication.update += this.OnEditorUpdate;
            }
        }

        protected virtual void OnDisable() {
            if (this.HasRunningTasks) return;
            this.OnDisableModules();
            this.IsEnabled = false;
            if (this.UseSceneGUI) {
                SceneView.duringSceneGui -= this.OnSceneGUI;
            }
            EditorApplication.update -= this.OnEditorUpdate;
        }

        private double editorLastTime;

        protected virtual void OnEditorUpdate() {
            float delta = (float) (EditorApplication.timeSinceStartup - this.editorLastTime);
            foreach (EditorModule module in modules.Values) {
                module.OnEditorUpdate(delta);
            }
            this.editorLastTime = EditorApplication.timeSinceStartup;
        }

        protected virtual U InitModule<U>(ref U module) where U : EditorModule {
            if (module == null) {
                if (!this.TryGetModule<U>(out module)) {
                    System.Object[] args = new System.Object[] { this };
                    module = Activator.CreateInstance(typeof(U), args) as U;
                    this.AddModule(module);
                }
            } else {
                if (!this.TryGetModule<U>(out _)) {
                    this.AddModule(module);
                }
            }
            return module;
        }

        protected virtual void OnEnableModules() {
            foreach (EditorModule module in modules.Values) {
                module.EnableModule();
            }
        }

        protected virtual void OnDisableModules() {
            foreach (EditorModule module in modules.Values) {
                module.DisableModule();
            }
        }

        public bool HasModule<M>() where M : EditorModule {
            return modules.ContainsKey(typeof(M));
        }

        public M GetModule<M>() where M : EditorModule {
            if (modules.TryGetValue(typeof(M), out EditorModule module)) {
                return module as M;
            }
            return null;
        }

        public bool TryGetModule<M>(out M module) where M : EditorModule {
            module = null;
            if (modules.TryGetValue(typeof(M), out EditorModule _module)) {
                module = _module as M;
                return true;
            }
            return false;
        }

        protected void AddModule(EditorModule module) {
            System.Type type = module.GetType();
            if (!modules.ContainsKey(type)) {
                modules[type] = module;
                if (this.IsEnabled) {
                    module.EnableModule();
                }
            }
        }

        protected void RemoveModule(EditorModule module) {
            System.Type type = module.GetType();
            if (modules.ContainsKey(type)) {
                modules.Remove(type);
                if (this.IsEnabled) {
                    module.DisableModule();
                }
            }
        }

        public void UpdateModule<U>() where U : EditorModule {
            if (this.TryGetModule<U>(out U module)) {
                module.UpdateModule();
            }
        }

        public override void OnInspectorGUI() {
            GenericEditor.CreateStyles();
            this.Target = (T) this.target;
            this.serializedObject.Update();
            
            GUILayout.BeginVertical("Box");
            EditorGUI.BeginChangeCheck();
            this.ShowDefaultInspector = GUILayout.Toggle(this.ShowDefaultInspector, "Default Inspector");
            if (EditorGUI.EndChangeCheck()) {
                // toggled this frame, dont draw more
                GUILayout.EndVertical();
                return;
            }
            GUILayout.EndVertical();

            if (this.ShowDefaultInspector) {
                // EditorGUI.BeginChangeCheck();
                this.DrawDefaultInspector();
                // if (EditorGUI.EndChangeCheck()) {
                //     // ...
                // }
            } else {

                this.OnCustomInspectorGUI();

                this.serializedObject.ApplyModifiedProperties();
            }
        }

        public virtual void OnCustomInspectorGUI() {
            EditorGUILayout.HelpBox("Using the default Generic Editor GUI", MessageType.Info);
            this.OnModulesGUI();
        }

        public virtual void OnModulesGUI() {
            bool first = true;
            foreach (EditorModule module in modules.Values) {
                if (module == null) continue;
                if (!first) {
                    EditorModule.DrawSeparator(Color.black, 1, 2, 2);
                }
                module.OnEditorGUI();
                first = false;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("TypeSafety", "UNT0006:Incorrect message signature", Justification = "<Pending>")]
        public virtual void OnSceneGUI(SceneView sceneView) {
            Event e = Event.current;
            if (e.type == EventType.MouseUp) {
                Debug.Log($"MouseUp: {e.button}");
            }
            // if (e.type == EventType.Layout) {
            //     HandleUtility.AddDefaultControl( GUIUtility.GetControlID( GetHashCode(), FocusType.Passive ) );
            //     // return;
            // }
            foreach (EditorModule module in modules.Values) {
                module.OnSceneGUI(sceneView);
            }
        }
    }

    public class EditorModule {
        public virtual string Name => this.GetType().Name;
        public virtual void EnableModule() {

        }
        public virtual void DisableModule() {

        }
        public virtual void UpdateModule() {

        }
        public virtual void OnEditorUpdate(float delta) {

        }
        public virtual void OnEditorGUI() {

        }
        public virtual void OnSceneGUI(SceneView sceneView) {

        }

        public static void DrawSeparator(float height = 1, float marginTop = 1, float marginBottom = 1) {
            EditorModule.DrawSeparator(Color.gray, height, marginTop, marginBottom);
        }

        public static void DrawSeparator(Color color, float height, float marginTop, float marginBottom) {
            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(height + marginTop + marginBottom));
            rect.y += marginTop;
            rect.height = height;
            EditorGUI.DrawRect(rect, color);
        }

        public static void BeginHorizontalSpaced() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
        }

        public static void EndHorizontalSpaced() {
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }
    }

    public class GenericEditorModule<T,K> : EditorModule where K : GenericEditor {
        public T Module { get; set; }
        public K Editor { get; private set; }
        public virtual string ModuleTitle => typeof(T).Name;

        protected SerializedObject serializedModule;

        protected bool toggled;

        public GenericEditorModule(K editor) {
            this.Editor = editor;
        }
        public void SetEditor(K editor) {
            this.Editor = editor;
        }
        public void SetModule(T module) {
            this.Module = module;
            if (module != null) {
                UnityEngine.Object obj = module as UnityEngine.Object;
                if (obj != null) {
                    this.serializedModule = new SerializedObject(obj);
                } else {
                    this.serializedModule = null;
                }
            } else {
                this.serializedModule = null;
            }
            this.UpdateModule();
            
        }
        
        public override void OnEditorGUI() {
            this.BeginModuleGUI();
            this.EndModuleGUI();
        }

        public override void OnSceneGUI(SceneView sceneView) {

        }

        protected virtual void BeginModuleGUI() {
            this.BeginTitledSection(this.ModuleTitle);
        }

        protected virtual void EndModuleGUI() {
            this.EndTitledSection();
        }

        protected SerializedObject ActiveSerializedObject => this.serializedModule != null ? this.serializedModule : this.Editor.serializedObject;

        protected virtual void DrawPropertyField(SerializedObject serializedObject, SerializedProperty serializedProperty) {
            serializedObject.Update();
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedProperty, true);
            EditorGUI.indentLevel--;
            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawPropertyField(string propertyName) {
            SerializedProperty property = this.ActiveSerializedObject.FindProperty(propertyName);
            if (property != null) {
                this.DrawPropertyField(this.ActiveSerializedObject, property);
            } else {
                EditorGUILayout.HelpBox($"Property named '{propertyName}' not found in module {this.Name}", MessageType.Warning);
            }
        }

        protected virtual void DrawPropertyFields(params string[] propertyNames) {
            // this.ActiveSerializedObject.Update();
            foreach (string propertyName in propertyNames) {
                SerializedProperty property = this.ActiveSerializedObject.FindProperty(propertyName);
                if (property != null) {
                    this.DrawPropertyField(this.ActiveSerializedObject, property);
                } else {
                    EditorGUILayout.HelpBox($"Property named '{propertyName}' not found in module {this.Name}", MessageType.Warning);
                }
            }
            // this.ActiveSerializedObject.ApplyModifiedProperties();
        }

        protected virtual void BeginTitledSection(string title) {
            
            GUILayout.BeginVertical(GUIContent.none, GenericEditor.ModuleStyle);
            GUILayout.Label(title, EditorStyles.boldLabel); 
            EditorModule.DrawSeparator();
        }

        protected virtual bool BeginToggleSection(string title, ref bool toggleState) {
            GUILayout.BeginVertical(GUIContent.none, GenericEditor.ModuleStyle);
            EditorGUI.indentLevel++;
            toggleState = EditorGUILayout.Foldout(toggleState, title, GenericEditor.BoldFoldout); 
            EditorGUI.indentLevel--;
            if (toggleState) {
                EditorModule.DrawSeparator();
            }
            return toggleState;
        }

        protected virtual void EndTitledSection() {
            GUILayout.EndVertical();
        }

        protected virtual void EndToggleSection() => this.EndTitledSection();

        protected virtual void DrawLabeledDropdownMenu(Rect rect, string label, string menuLabel, GenericMenu menu) {
            Rect labelRect = rect;
            labelRect.width = 100;
            Rect dropdownRect = rect;
            dropdownRect.width -= 120;
            dropdownRect.x += 120;

            GUI.Label(labelRect, label);
            if (!EditorGUI.DropdownButton(dropdownRect, new GUIContent(menuLabel), FocusType.Passive)) {
                return;
            }
            menu.DropDown(dropdownRect);
        }

        protected static void DrawRectHandle(Rect rect, Color color0, Color color1, Vector3 position) {
            // color0.a = 1f;
            // color1.a = 1f;
            Vector3[] verts = new Vector3[4];
            verts[0] = position + new Vector3(rect.x, 0, rect.y);
            verts[1] = position + new Vector3(rect.x, 0, rect.y + rect.height);
            verts[2] = position + new Vector3(rect.x + rect.width, 0, rect.y + rect.height);
            verts[3] = position + new Vector3(rect.x + rect.width, 0, rect.y);
            
            Handles.DrawSolidRectangleWithOutline(verts, color0, color1);

            Handles.color = color1;
            for (int i = 0; i < 4; i++) {
                int idx0 = i;
                int idx1 = (i + 1) % 4;
                Handles.DrawLine(verts[idx0], verts[idx1], 1);
            }
        }


        const float APPLY_BUTTON_WIDTH = 50;
        const float REVERT_BUTTON_WIDTH = 60;
        const float APPLY_LABEL_WIDTH = 80;

        protected void DrawApplyablePropertyInt(string name, ref int change, ref int target) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            change = EditorGUILayout.IntField(GUIContent.none, change);
            EditorGUI.BeginDisabledGroup(change == target);
            if (GUILayout.Button("Apply", GUILayout.Width(APPLY_BUTTON_WIDTH))) {
                target = change;
                UnityEngine.Object unityObj = this.Module as UnityEngine.Object;
                if (unityObj != null) {
                    EditorUtility.SetDirty(unityObj);
                }
            }
            if (GUILayout.Button("Revert", GUILayout.Width(REVERT_BUTTON_WIDTH))) {
                change = target;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawApplyablePropertyFloat(string name, ref float change, ref float target) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            change = EditorGUILayout.FloatField(GUIContent.none, change);
            EditorGUI.BeginDisabledGroup(change == target);
            if (GUILayout.Button("Apply", GUILayout.Width(APPLY_BUTTON_WIDTH))) {
                target = change;
                UnityEngine.Object unityObj = this.Module as UnityEngine.Object;
                if (unityObj != null) {
                    EditorUtility.SetDirty(unityObj);
                }
            }
            if (GUILayout.Button("Revert", GUILayout.Width(REVERT_BUTTON_WIDTH))) {
                change = target;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        

        protected void DrawApplyablePropertyVector2(string name, ref Vector2 change, ref Vector2 target) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            change = EditorGUILayout.Vector2Field(GUIContent.none, change);
            EditorGUI.BeginDisabledGroup(change == target);
            if (GUILayout.Button("Apply", GUILayout.Width(APPLY_BUTTON_WIDTH))) {
                target = change;
                UnityEngine.Object unityObj = this.Module as UnityEngine.Object;
                if (unityObj != null) {
                    EditorUtility.SetDirty(unityObj);
                }
            }
            if (GUILayout.Button("Revert", GUILayout.Width(REVERT_BUTTON_WIDTH))) {
                change = target;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawApplyablePropertySlider(string name, float min, float max, ref float change, ref float target) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            change = EditorGUILayout.Slider(GUIContent.none, change, min, max);
            EditorGUI.BeginDisabledGroup(change == target);
            if (GUILayout.Button("Apply", GUILayout.Width(APPLY_BUTTON_WIDTH))) {
                target = change;
                UnityEngine.Object unityObj = this.Module as UnityEngine.Object;
                if (unityObj != null) {
                    EditorUtility.SetDirty(unityObj);
                }
            }
            if (GUILayout.Button("Revert", GUILayout.Width(REVERT_BUTTON_WIDTH))) {
                change = target;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawApplyablePropertyIntSlider(string name, int min, int max, ref int change, ref int target) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            change = EditorGUILayout.IntSlider(GUIContent.none, change, min, max);
            EditorGUI.BeginDisabledGroup(change == target);
            if (GUILayout.Button("Apply", GUILayout.Width(APPLY_BUTTON_WIDTH))) {
                target = change;
                UnityEngine.Object unityObj = this.Module as UnityEngine.Object;
                if (unityObj != null) {
                    EditorUtility.SetDirty(unityObj);
                }
            }
            if (GUILayout.Button("Revert", GUILayout.Width(REVERT_BUTTON_WIDTH))) {
                change = target;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawApplyablePropertyEnum<U>(string name, ref U change, ref U target) where U : Enum {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            change = (U) EditorGUILayout.EnumPopup(GUIContent.none, change);
            EditorGUI.BeginDisabledGroup((int)(object)change == (int)(object)target);
            if (GUILayout.Button("Apply", GUILayout.Width(APPLY_BUTTON_WIDTH))) {
                target = change;
                UnityEngine.Object unityObj = this.Module as UnityEngine.Object;
                if (unityObj != null) {
                    EditorUtility.SetDirty(unityObj);
                }
            }
            if (GUILayout.Button("Revert", GUILayout.Width(REVERT_BUTTON_WIDTH))) {
                change = target;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawApplyablePropertyFloatEnum<U>(string name, ref float changeFloat, ref float targetFloat, ref U change, ref U target) where U : Enum {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            changeFloat = EditorGUILayout.FloatField(GUIContent.none, changeFloat, GUILayout.Width(40));
            change = (U) EditorGUILayout.EnumPopup(GUIContent.none, change);
            EditorGUI.BeginDisabledGroup((changeFloat == targetFloat) || ((int)(object)change == (int)(object)target));
            if (GUILayout.Button("Apply", GUILayout.Width(APPLY_BUTTON_WIDTH))) {
                target = change;
                UnityEngine.Object unityObj = this.Module as UnityEngine.Object;
                if (unityObj != null) {
                    EditorUtility.SetDirty(unityObj);
                }
            }
            if (GUILayout.Button("Revert", GUILayout.Width(REVERT_BUTTON_WIDTH))) {
                change = target;
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawPropertyFloatEnum<U>(string name, ref float changeFloat, ref U changeEnum) where U : Enum {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            changeFloat = EditorGUILayout.FloatField(GUIContent.none, changeFloat, GUILayout.Width(40));
            changeEnum = (U) EditorGUILayout.EnumPopup(GUIContent.none, changeEnum);
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawPropertyFloatObject<U>(string name, ref float changeFloat, ref U changeObj) where U : UnityEngine.Object {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name, GUILayout.Width(APPLY_LABEL_WIDTH));
            changeFloat = EditorGUILayout.FloatField(GUIContent.none, changeFloat, GUILayout.Width(40));
            changeObj = (U) EditorGUILayout.ObjectField(changeObj, typeof(U), true);
            EditorGUILayout.EndHorizontal();
        }
    }

    public class GenericTreeView<T> : TreeView {

        public class ViewItem : TreeViewItem {
            public T Item { get; set; }
        }

        protected List<TreeViewItem> viewItems = new List<TreeViewItem>();
        public List<TreeViewItem> ViewItems => this.viewItems;
        protected ViewItem root;
        public ViewItem Root => this.root;

        public virtual float LineHeight => EditorGUIUtility.singleLineHeight;

        public Dictionary<int,int> DepthCount { get; private set; } = new Dictionary<int,int>();
        protected Dictionary<int,ViewItem> IdItemMap { get; private set; } = new Dictionary<int,ViewItem>();

        public GenericTreeView(TreeViewState treeViewState) : base(treeViewState) {
            this.rowHeight = this.LineHeight;
            this.Reload();
        }
            
        protected override TreeViewItem BuildRoot() {
            if (this.root == null) {
                this.root = this.AddItem(default(T), -1, "(Root)");
            }
            TreeView.SetupParentsAndChildrenFromDepths (this.root, this.viewItems);
            return this.root;
        }

        public void Clear() {
            this.viewItems.Clear();
            // this.root = null;
            this.DepthCount.Clear();
            this.IdItemMap.Clear();
        }

        public virtual ViewItem AddItem(T item, int depth = 0, string name = null) {
            int id = this.viewItems.Count;
            ViewItem viewItem = new ViewItem() {
                Item = item,
                id = id, 
                depth = depth, 
                displayName = string.IsNullOrEmpty(name) ? this.GetItemName(item) : name
            };
            int depthCount;
            this.DepthCount.TryGetValue(depth, out depthCount);
            this.DepthCount[depth] = depthCount + 1;
            this.IdItemMap[id] = viewItem;
            this.viewItems.Add(viewItem);
            return viewItem;
        }

        public virtual ViewItem GetItem(int id) {
            ViewItem item;
            this.IdItemMap.TryGetValue(id, out item);
            return item;
        }

        

        public int GetDepthCount(int depth) {
            int count;
            this.DepthCount.TryGetValue(depth, out count);
            return count;
        }

        protected virtual string GetItemName(T item) {
            return $"{item}";
        }

        protected override void RowGUI(TreeView.RowGUIArgs args) {
            base.RowGUI(args);
        }

        // protected override float GetCustomRowHeight(int row, TreeViewItem item) {
        //     return base.GetCustomRowHeight(row, item);
        // }

        private List<ViewItem> selectedItems = new List<ViewItem>();
        public List<ViewItem> SelectedItems => this.selectedItems;

        protected override void SelectionChanged(IList<int> selectedIds) {
            this.selectedItems.Clear();
            foreach (int id in selectedIds) {
                this.selectedItems.Add(this.IdItemMap[id]);
            }
            if (this.onItemSelectionchanged != null) {
                this.onItemSelectionchanged.Invoke(this.selectedItems);
            }
        }

        public delegate void OnItemSelectionChanged(List<ViewItem> selectedItems);
        public OnItemSelectionChanged onItemSelectionchanged;

        protected override void SingleClickedItem(int id) {
            if (this.IdItemMap.TryGetValue(id, out ViewItem item)) {
                this.OnItemClicked(item, false);   
            }
        }

        protected override void DoubleClickedItem(int id) {
            if (this.IdItemMap.TryGetValue(id, out ViewItem item)) {
                this.OnItemClicked(item, true);   
            }
        }

        private ViewItem lastClickedItem;

        protected virtual void OnItemClicked(ViewItem item, bool doubleClick) {
            if ((doubleClick || this.lastClickedItem == item) && this.IsSelected(item.id)) {
                if (this.IsExpanded(item.id)) {
                    this.SetExpanded(item.id, false);
                } else {
                    this.SetExpanded(item.id, true);
                    // Unity crashes everytime trying to use the FrameItem function, something wrong in own usage or bug in unity...
                    // if (item.children.Count > 0) {
                    //     int frameIndex = Mathf.Min(4, item.children.Count - 1);
                    //     this.FrameItem(item.children[frameIndex].id);
                    // }
                }
            }
            this.lastClickedItem = item;
        }

        public void ExpandToRoot(int id) {
            if (this.IdItemMap.TryGetValue(id, out ViewItem item)) {
                this.ExpandToRoot(item);   
            }
        }

        public void ExpandToRoot(ViewItem item) {
            if (item == this.root) {
                return;
            }
            if (!this.IsExpanded(item.id)) {
                this.SetExpanded(item.id, true);
            }
            if (item.parent != null) {
                this.ExpandToRoot(item.parent as ViewItem);
            }
        }
    }

    // [Overlay(typeof(SceneView), "LevelBuilderActionsModule.BuildActionsOverlay", "LevelBuilder", true, defaultLayout = Layout.VerticalToolbar)]
    public class GenericEditorOverlay : IMGUIOverlay {

        public int selectedSnap = -1;
        private GUIContent[] snapButtons = new GUIContent[]{ 
            EditorGUIUtility.IconContent("EditorSnapTarget"),
            EditorGUIUtility.IconContent("EditorSnapCollection"),
            EditorGUIUtility.IconContent("EditorSnapAsset")
        };

        public override void OnCreated() {
            base.OnCreated();
        }

        public override void OnGUI() {

            this.selectedSnap = GUILayout.Toolbar(this.selectedSnap, snapButtons, "button", GUILayout.Width(108), GUILayout.Height(32));

        }
    }

    public class GenericEditorToolbar {

        public int selectedIndex { get; private set; }= -1;
        public GUIContent[] buttons = System.Array.Empty<GUIContent>();

        public float width = 28;
        public float height = 28;

        public virtual void DrawToolbar(float width = -1, float height = -1) {
            if (this.buttons == null) {
                return;
            }
            if (width == -1) {
                width = this.width * this.buttons.Length;
            }
            if (height == -1) {
                height = this.height;
            }
            EditorGUI.BeginChangeCheck();
            int nextIndex = GUILayout.Toolbar(this.selectedIndex, buttons, "button", GUILayout.Width(width), GUILayout.Height(height));
            if (EditorGUI.EndChangeCheck()) {
                if (nextIndex == this.selectedIndex) {
                    nextIndex = -1;
                }
                this.Select(nextIndex);
                this.TriggerOnSelectionChanged();
            }
        }

        public virtual void Select(int index) {
            this.selectedIndex = index;
        }

        protected virtual void TriggerOnSelectionChanged() {
            if (this.onSelectedIndexChanged != null) {
                this.onSelectedIndexChanged(this.selectedIndex);
            }
        }

        public delegate void OnSelectedIndexChanged(int index);
        public OnSelectedIndexChanged onSelectedIndexChanged;
    }

    public class GenericEditorToolbar<T> : GenericEditorToolbar where T : Enum {

        public T Value { get; private set; } = (T) (object) -1;

        public override void Select(int index) {
            base.Select(index);
            this.Value = (T) (object) index;
        }

        public virtual void Select(T value) {
            base.Select((int) (object) value);
            this.Value = value;
        }

        protected override void TriggerOnSelectionChanged() {
            base.TriggerOnSelectionChanged();
            if (this.onSelectedValueChanged != null) {
                this.onSelectedValueChanged(this.Value);
            }
        }

        public delegate void OnSelectedValueChanged(T value);
        public OnSelectedValueChanged onSelectedValueChanged;
    }



    public class GenericPropertyDrawer<T> : PropertyDrawer {

        protected virtual IEnumerable<string> propertyNames => Enumerable.Empty<string>();
        protected virtual bool UseTitledContainer => false;
        protected virtual bool FoldoutInsideContainer => false;
        protected virtual float FoldoutInset => 0;
        protected virtual float LabelWidth => 0;
        protected virtual string GetTitle(SerializedProperty property) {
            if (property.propertyType == SerializedPropertyType.ManagedReference) {
                if (property.managedReferenceValue == null) {
                    return TextUtils.PadCapitalizedString(property.name);
                } else {
                    return TextUtils.PadCapitalizedString(property.managedReferenceValue.GetType().Name);
                }
            }
            return TextUtils.PadCapitalizedString(property.name);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            CreateStyles();

            EditorGUI.BeginProperty(position, label, property);

            Rect currentRect = position;

            if (this.UseTitledContainer && this.FoldoutInsideContainer) {
                this.DrawTitledFoldoutContainer(ref currentRect, property, this.GetTitle(property));
            } else {
                Rect foldoutRect = currentRect;
                foldoutRect.height = EditorGUIUtility.singleLineHeight;
                currentRect.height -= EditorGUIUtility.singleLineHeight;
                currentRect.y += EditorGUIUtility.singleLineHeight;
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            }

            if (property.isExpanded) {

                if (this.UseTitledContainer && !this.FoldoutInsideContainer) {
                    this.DrawTitledContainer(ref currentRect, this.GetTitle(property));
                }

                EditorGUI.indentLevel++;

                float originalLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = this.LabelWidth;

                foreach (string propertyName in this.propertyNames) {
                    this.TryDrawProperty(ref currentRect, property, propertyName);
                }

                this.DrawGUI(ref currentRect, property);

                EditorGUIUtility.labelWidth = originalLabelWidth;

                EditorGUI.indentLevel--;
            }
    
            EditorGUI.EndProperty();
        }

        protected virtual void DrawGUI(ref Rect currentRect, SerializedProperty property) {

        }

        protected virtual float GetDrawGUIHeight(SerializedProperty property) {
            return 0;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            float height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded) {
                if (this.UseTitledContainer) {
                    if (!this.FoldoutInsideContainer) {
                        height += EditorGUIUtility.singleLineHeight;
                    }
                    height += 4; // separator
                    height += 4; // margins
                }
                height += this.GetDrawGUIHeight(property);
                foreach (string propertyName in this.propertyNames) {
                    height += this.TryGetPropertyHeight(property, propertyName);
                    // height += 1; // padding
                }
            } else {
                if (this.UseTitledContainer && this.FoldoutInsideContainer) {
                    height += 4; // margins
                }
            }
            return height;
        }

        protected void TryDrawProperty(ref Rect currentRect, SerializedProperty propertyParent, string propertyName) {
            SerializedProperty serializedParameters = propertyParent.FindPropertyRelative(propertyName);
            if (serializedParameters != null) {
                float height = this.TryGetPropertyHeight(propertyParent, propertyName);

                currentRect.height = height - 1;

                GUIContent propertyLabel = new GUIContent(propertyName);
                EditorGUI.PropertyField(currentRect, serializedParameters, propertyLabel, true);

                currentRect.y += height;
            }
        }

        protected float TryGetPropertyHeight(SerializedProperty propertyParent, string propertyName) {
            float height = 0;
            SerializedProperty serializedProperty = propertyParent.FindPropertyRelative(propertyName);
            if (serializedProperty != null) {
                height = EditorGUI.GetPropertyHeight(serializedProperty, null, true);
                // if (serializedProperty.isExpanded) {
                //     height = EditorGUI.GetPropertyHeight(serializedProperty, null, true);
                // } else {
                //     height = EditorGUIUtility.singleLineHeight;
                // }
                height += 1; // padding
            }
            return height;
        }

        protected virtual void DrawTitledContainer(ref Rect currentRect, string title) {
            GUI.Box(currentRect, GUIContent.none, ContainerStyle);
            currentRect.x += 2;
            currentRect.y += 2;
            currentRect.width -= 4;
            currentRect.height -= 4;
            this.DrawLabel(ref currentRect, title, EditorStyles.boldLabel); 
            this.DrawSeparator(ref currentRect);
        }

        protected virtual void DrawTitledFoldoutContainer(ref Rect currentRect, SerializedProperty property, string title) {
            GUI.Box(currentRect, GUIContent.none, ContainerStyle);
            currentRect.x += 2;
            currentRect.y += 2;
            currentRect.width -= 4;
            currentRect.height -= 4;

            Rect foldoutRect = currentRect;
            foldoutRect.x += this.FoldoutInset;
            foldoutRect.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, new GUIContent(title));
            
            currentRect.y += EditorGUIUtility.singleLineHeight;

            if (property.isExpanded) {
                this.DrawSeparator(ref currentRect);
            }
        }

        

        protected virtual void DrawLabel(ref Rect currentRect, string label, GUIStyle style) {
            float height = currentRect.height;
            currentRect.height = EditorGUIUtility.singleLineHeight;
            GUI.Label(currentRect, label, style); 
            currentRect.y += EditorGUIUtility.singleLineHeight;
            currentRect.height = height - EditorGUIUtility.singleLineHeight;
        }

        protected virtual void DrawSeparator(ref Rect currentRect, float height = 1, float marginTop = 1, float marginBottom = 1) {
            this.DrawSeparator(ref currentRect, Color.gray, height, marginTop, marginBottom);
        }

        protected virtual void DrawSeparator(ref Rect currentRect, Color color, float height, float marginTop, float marginBottom) {
            Rect rect = currentRect;
            rect.height = height + marginTop + marginBottom;
            currentRect.y += rect.height + 1;
            currentRect.height -= rect.height;

            rect.y += marginTop;
            rect.height = height;
            EditorGUI.DrawRect(rect, color);   
        }

        protected static GUIStyle containerStyle;
        public static GUIStyle ContainerStyle => containerStyle;
        private static bool stylesCreated;

        protected static void CreateStyles() {
            if (stylesCreated) return;
            stylesCreated = true;
            GenericEditor.CreateStyles();
            containerStyle = new GUIStyle();//GUI.skin.window);
            containerStyle.border = new RectOffset(3, 3, 3, 3);
            containerStyle.normal.background = GenericEditor.TexColors[(int)GenericEditor.TexColor.Grey];
            containerStyle.contentOffset = Vector2.zero;
            containerStyle.padding = new RectOffset(4, 4, 4, 4);
        }

        public static void GetHorizontalSpaced(ref Rect currentRect, ref Rect[] rects, float partWidth, float padding, float minSpacing) {
            float width = currentRect.width;
            partWidth = Mathf.Min(partWidth, width / rects.Length);
            float space = Mathf.Max(minSpacing, width - partWidth * rects.Length);
            if (space + partWidth * rects.Length > currentRect.width) {
                partWidth = (currentRect.width - space) / rects.Length;
            }
            float x = space / 2;
            for (int i = 0; i < rects.Length; i++) {
                Rect r = currentRect;
                r.x += x;
                r.width = partWidth - padding;
                r.height = EditorGUIUtility.singleLineHeight;
                r.y += padding;
                x += partWidth;
                rects[i] = r;
            }
        }
        
    }
}