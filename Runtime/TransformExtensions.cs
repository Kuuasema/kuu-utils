using UnityEngine;

namespace Kuuasema.Utils {
    public static class TransformExtensions {
        public static bool TryCountParentDistance(this Transform self, Transform parent, out int distance) {
            distance = -1;
            Transform current = self;
            while (current != null && current != parent) {
                current = current.parent;
                distance++;
            }
            if (current == parent) {
                return true;
            }
            return false;
        }
    }
}