using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Kuuasema.Utils
{
    public static class NavigationUtils
    { 
        public static float PathLenght(NavMeshPath path)
        {
            float o = 0f;

            for (int i = 1; i < path.corners.Length; i++)
            {
                o += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return o;
        }

        public static Vector3 PartialPath(NavMeshPath path, float t) 
        {
            Vector3 o = Vector3.zero;
            float lenght = PathLenght(path) * t;
            float totalLenght = 0;
            for (int i = 1; i < path.corners.Length; i++)
            {
                float dist = Vector3.Distance(path.corners[i - 1], path.corners[i]);
                if (totalLenght + dist > lenght)
                {
                    o = Vector3.Lerp(path.corners[i - 1], path.corners[i], (lenght - totalLenght) / dist);
                    break;
                }
                totalLenght += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return o;
        }

        public static Vector3 DirectionAt(NavMeshPath path, float t)
        {
            Vector3 o = Vector3.zero;
            float lenght = PathLenght(path) * t;
            float totalLenght = 0;
            for (int i = 1; i < path.corners.Length; i++)
            {
                o = (path.corners[i] - path.corners[i - 1]).normalized;
                float dist = Vector3.Distance(path.corners[i - 1], path.corners[i]);
                if (totalLenght + dist > lenght) break;
                totalLenght += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return o;
        }
    }
}
