using System.Collections.Generic;
using UnityEngine;

namespace Kuuasema.Utils {
    public static class MathUtils {

        public static float SqrDistance(Vector3 a, Vector3 b) {  
            return (a-b).sqrMagnitude;  
        }

        public static bool LineIntersect(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, out Vector3 intersect, float maxDistance = float.MaxValue) {

            intersect = Vector3.zero;

            Vector3 rayA = p2 - p1;
            Vector3 rayB = p3 - p4;

            Vector3 cross0 = Vector3.Cross(rayA.normalized, rayB.normalized);
            Vector3 cross1 = Vector3.Cross(rayA.normalized, cross0.normalized);
            Plane plane = new Plane(cross1.normalized, p1);

            Ray ray = new Ray(p4, rayB.normalized);
            float rayDistance = rayB.magnitude;
            float enter;
            if (plane.Raycast(ray, out enter)) {
                if (enter < rayDistance) {
                    intersect = ray.GetPoint(enter);

                    Vector3 planePoint = ClosestPointOnLine(intersect, p1, p2);
                    if ((planePoint - intersect).magnitude < maxDistance) {
                        return true;
                    }
                }
            }
            ray = new Ray(p3, -rayB.normalized);
            if (plane.Raycast(ray, out enter)) {
                if (enter < rayDistance) {
                    intersect = ray.GetPoint(enter);
                    
                    Vector3 planePoint = ClosestPointOnLine(intersect, p1, p2);
                    if ((planePoint - intersect).magnitude < maxDistance) {
                        return true;
                    }
                }
            }
            return false;
        }

        public static Vector3 ClosestPointOnLine(Vector3 point, Vector3 line0, Vector3 line1) {
            Vector3 line_direction = line1 - line0;
            float line_length = line_direction.magnitude;
            line_direction.Normalize();
            float project_length = Mathf.Clamp(Vector3.Dot(point - line0, line_direction), 0f, line_length);
            return line0 + line_direction * project_length;
        }

        public static bool PointInPolygon(Vector3 point, List<Vector3> polygon) {
            
            int polygonLength = polygon.Count, i = 0;
            bool inside = false;
            float pointX = point.x, pointZ = point.z;
            float startX, startZ, endX, endZ;
            Vector3 endPoint = polygon[polygonLength - 1];           
            endX = endPoint.x; 
            endZ = endPoint.z;
            while (i < polygonLength) {
                startX = endX;
                startZ = endZ;
                endPoint = polygon[i++];
                endX = endPoint.x;
                endZ = endPoint.z;
                inside ^= ( endZ > pointZ ^ startZ > pointZ ) /* ? pointY inside [startY;endY] segment ? */
                            && /* if so, test if it is under the segment */
                            ( (pointX - endX) < (pointZ - endZ) * (startX - endX) / (startZ - endZ) ) ;
            }
            return inside;
        }
    }
}