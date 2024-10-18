using UnityEngine;

namespace Kuuasema.Utils {
    public static class VectorExtensions {
        public static bool IsDistanceCloser(this Vector3 self, Vector3 other, float distance) {
            float sqrDistance = distance * distance;
            float sqrMagnitude = (self - other).sqrMagnitude;
            return sqrMagnitude < sqrDistance;
        }

        public static bool IsPlanarDistanceCloser(this Vector3 self, Vector3 other, float distance) {
            float sqrDistance = distance * distance;
            Vector3 a = self;
            Vector3 b = other;
            a.y = 0;
            b.y = 0;
            float sqrMagnitude = (a - b).sqrMagnitude;
            return sqrMagnitude < sqrDistance;
        }
    }
}