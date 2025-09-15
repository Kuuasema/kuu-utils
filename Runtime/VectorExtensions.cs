using UnityEngine;

namespace Kuuasema.Utils {
    public static class VectorExtensions
    {
        public static float PlanarDistance(this Vector3 self, Vector3 other)
        {
            Vector3 a = self;
            Vector3 b = other;
            a.y = 0;
            b.y = 0;
            return (a - b).magnitude;
        }
        public static float SqrDistance(this Vector3 self, Vector3 other)
        {
            return (self - other).sqrMagnitude;
        }
        public static float SqrPlanarDistance(this Vector3 self, Vector3 other)
        {
            Vector3 a = self;
            Vector3 b = other;
            a.y = 0;
            b.y = 0;
            return (a - b).sqrMagnitude;
        }
        public static float PlanarAngle(this Vector3 self, Vector3 other)
        {

            Vector3 a = self;
            Vector3 b = other;
            a.y = 0;
            b.y = 0;
            return Vector3.Angle(a, b);
        }
        public static float SignedPlanarAngle(this Vector3 self, Vector3 other)
        {

            Vector3 a = self;
            Vector3 b = other;
            a.y = 0;
            b.y = 0;
            return Vector3.SignedAngle(a, b, Vector3.up);
        }

        public static bool IsDistanceCloser(this Vector3 self, Vector3 other, float distance) {
            float sqrDistance = distance * distance;
            return SqrDistance(self,other) < distance;
        }

        public static bool IsPlanarDistanceCloser(this Vector3 self, Vector3 other, float distance) {
            float sqrDistance = distance * distance;
            return SqrPlanarDistance(self,other) < sqrDistance;
        }
        public static bool IsPlanarAngleWithin(this Vector3 self, Vector3 other, float tolerance)
        {
            return PlanarAngle(self,other) <= tolerance;
        }
    }
}