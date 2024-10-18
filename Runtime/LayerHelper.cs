using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    public static class LayerHelper {
        public static readonly int DEFAULT_LAYER = LayerMask.NameToLayer("Default");
        public static readonly int TRANSPARENT_FX_LAYER = LayerMask.NameToLayer("TransparentFX");
        public static readonly int IGNORE_RAYCAST_LAYER = LayerMask.NameToLayer("Ignore Raycast");
        public static readonly int GROUND_LAYER = LayerMask.NameToLayer("Ground");
        public static readonly int WATER_LAYER = LayerMask.NameToLayer("Water");
        public static readonly int UI_LAYER = LayerMask.NameToLayer("UI");

        public static readonly int VISIBLE_LAYER = LayerMask.NameToLayer("Visible");
        public static readonly int VISION_BLOCKER_LAYER = LayerMask.NameToLayer("VisionBlocker");

        public static readonly int SELECTION_LAYER = LayerMask.NameToLayer("Selection");


        public static List<string> LayerNames { get; private set; } = new List<string>();
        public static Dictionary<string,int> LayerMap { get; private set;} = new Dictionary<string,int>();

        public static string GetLayerName(int layer) {
            return LayerMask.LayerToName(layer);
        }

        public static void RefreshLayerMap() {
            int layerCount = 0;
            for (int i = 0; i < 32; i++) {
                string layerName = LayerMask.LayerToName(i);
                if (layerName != "") {
                    if (LayerNames.Count > layerCount) {
                        if (LayerNames[layerCount] != layerName) {
                            LayerNames[layerCount] = layerName;
                        }
                    } else {
                        LayerNames.Add(layerName);
                    }
                    LayerMap[layerName] = i;
                    layerCount++;
                }
            }
        }
    }
}