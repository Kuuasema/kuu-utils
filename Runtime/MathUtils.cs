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



        // ///////// BEZIER MATH BASED ON : https://stackoverflow.com/questions/2742610/closest-point-on-a-cubic-bezier-curve
        // /** Find the ~closest point on a Bézier curve to a point you supply.
        // * out    : A vector to modify to be the point on the curve
        // * curve  : Array of vectors representing control points for a Bézier curve
        // * pt     : The point (vector) you want to find out to be near
        // * tmps   : Array of temporary vectors (reduces memory allocations)
        // * returns: The parameter t representing the location of `out`
        // */
        // public struct BezierCurve {
        //     public Vector3 p0;
        //     public Vector3 p1;
        //     public Vector3 p2;
        //     public Vector3 p4;
        // }
        // private static Vector3[] bezierTempVectors = new Vector3[8];
        // public static float BezierClosestPoint(out Vector3 closest, BezierCurve curve, Vector3 point) {
        //     int mindex = 0;
        //     int scans = 25;
        //     // let mindex, scans=25; // More scans -> better chance of being correct
        //     Vector3 vec = curve.p0;
        //     // const vec=vmath['w' in curve[0]?'vec4':'z' in curve[0]?'vec3':'vec2'];
        //     float min = float.MaxValue;
        //     for (int i = scans++;i>=0;i--) {
        //         // for (let min=Infinity, i=scans+1;i--;) {
                
        //         float d2 = SqrDistance(point, BezierPoint(out _, curve, i / scans));
            
            
        //         // let d2 = vec.squaredDistance(pt, bézierPoint(out, curve, i/scans, tmps));
        //         if (d2 < min) { 
        //             min = d2; 
        //             mindex = i;
        //         }
        //     }
        //     let t0 = Math.max((mindex-1)/scans,0);
        //     let t1 = Math.min((mindex+1)/scans,1);
        //     let d2ForT = t => vec.squaredDistance(pt, bézierPoint(out,curve,t,tmps));
        //     return localMinimum(t0, t1, d2ForT, 1e-4);
        // }

        // /** Find a minimum point for a bounded function. May be a local minimum.
        // * minX   : the smallest input value
        // * maxX   : the largest input value
        // * ƒ      : a function that returns a value `y` given an `x`
        // * ε      : how close in `x` the bounds must be before returning
        // * returns: the `x` value that produces the smallest `y`
        // */
        // function localMinimum(minX, maxX, ƒ, ε) {
        //     if (ε===undefined) ε=1e-10;
        //     let m=minX, n=maxX, k;
        //     while ((n-m)>ε) {
        //         k = (n+m)/2;
        //         if (ƒ(k-ε)<ƒ(k+ε)) n=k;
        //         else               m=k;
        //     }
        //     return k;
        // }

        // /** Calculate a point along a Bézier segment for a given parameter.
        // * out    : A vector to modify to be the point on the curve
        // * curve  : Array of vectors representing control points for a Bézier curve
        // * t      : Parameter [0,1] for how far along the curve the point should be
        // * tmps   : Array of temporary vectors (reduces memory allocations)
        // * returns: out (the vector that was modified)
        // */
        // float BezierPoint(out Vector3 oncurve, BezierCurve curve, float t) {
        //     if (curve.length<2) console.error('At least 2 control points are required');
        //     const vec=vmath['w' in curve[0]?'vec4':'z' in curve[0]?'vec3':'vec2'];
        //     if (!tmps) tmps = curve.map( pt=>vec.clone(pt) );
        //     else tmps.forEach( (pt,i)=>{ vec.copy(pt,curve[i]) } );
        //     for (var degree=curve.length-1;degree--;) {
        //         for (var i=0;i<=degree;++i) vec.lerp(tmps[i],tmps[i],tmps[i+1],t);
        //     }
        //     return vec.copy(out,tmps[0]);
        // }
    }
}