using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Job Related
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace SPH3
{
    /// <summary>
    /// Collision to Plane
    /// </summary>
    [BurstCompile]
    public struct PlaneCollideJob : IJobParallelFor
    {
        public Matrix4x4 TRS;
        public float ParticleRadius;

        public NativeArray<Vector4> ProjectedPositions;
        public NativeArray<Vector4> Collisions;

        public bool InfinitePlane;
        public Vector2 Min;
        public Vector2 Max;


        public void Execute(int i)
        {
            /// Change of basis to OBB
            Vector3 _point = TRS.inverse.MultiplyPoint(ProjectedPositions[i]);

            Vector3 closestPoint3 = Vector3.zero;

            if (!InfinitePlane)
            {
                bool insideX = Min.x < _point.x && _point.x < Max.x;
                bool insideY = Min.y < _point.y && _point.y < Max.y;

                bool pointInsideRectangle = insideX && insideY;

                if (!pointInsideRectangle)
                /// Outside
                {
                    /// <remarks> 
                    /// If we are outside then the closest point is 
                    /// clamped to the outer edges of the rectangle.
                    /// </remarks>
                    closestPoint3.x = math.max(Min.x, math.min(_point.x, Max.x));
                    closestPoint3.y = math.max(Min.y, math.min(_point.y, Max.y));
                }
                else
                /// Inside
                {
                    /// <remarks>
                    /// If we are inside the closest point is always given x, given y, z=0 
                    /// </remarks>
                    closestPoint3.x = _point.x;
                    closestPoint3.y = _point.y;
                }
            }
            else
            /// Infinite Plane
            {

                closestPoint3.x = _point.x;
                closestPoint3.y = _point.y;
            }

            /// Hand-off to V4 for return value
            Vector4 closestPoint4 = closestPoint3;
            
            /// Apply sign
            if (_point.z > 0)
                closestPoint4.w = Vector3.Distance(_point, closestPoint3);
            else
                closestPoint4.w = -Vector3.Distance(_point, closestPoint3);

            /// Stored regardless for comparison in the future.
            Collisions[i] = closestPoint4;

            /// Return valid position with particle radius offset,
            /// else do not modify.
            if (Mathf.Abs(closestPoint4.w) < ParticleRadius)
            {              
                ProjectedPositions[i] = TRS.MultiplyPoint(closestPoint3 + ((_point - closestPoint3).normalized * ParticleRadius));
            }
        }
    }
}