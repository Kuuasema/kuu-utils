using UnityEngine;

namespace Kuuasema.Utils {
    public static class GameObjectExtensions {

        public static bool TrySetActive(this GameObject self, bool active) {
            if (self.activeSelf == active) {
                return false;
            }
            self.SetActive(active);
            return true;
        }

        public static GameObject CreateChild(this GameObject self, string name) {
            GameObject obj = new GameObject(name);
            obj.transform.parent = self.transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            return obj;
        }

        public static bool HasParent(this GameObject self, GameObject other, int allowDepth = -1) {
            if (allowDepth == 0) {
                Debug.LogError("Cannot check parent depth with zero depth allowed.");
                return false;
            }
            Transform parentTransform = self.transform.parent;
            Transform targetTransform = other.transform;
            while (allowDepth != 0 && parentTransform != null && parentTransform != targetTransform) {
                parentTransform = parentTransform.parent;
                allowDepth--;
            }
            return parentTransform == targetTransform;
        }
    }
}