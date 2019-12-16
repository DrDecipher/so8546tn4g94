using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Job Related
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;

namespace SPH3
{
    /// <summary>
    /// Initial Euler projection based on velocity
    /// </summary>
    [BurstCompile]
    public struct ProjectJob : IJobParallelFor
    {
        public float Delta;
        public Vector3 Gravity;
        public float Drag;

        [ReadOnly]
        public NativeArray<Vector3> Positions;

        [WriteOnly]
        public NativeArray<Vector4> ProjectedPositions;

        public NativeArray<Vector3> Velocities;


        public void Execute(int i)
        {
            /// <remarks>
            /// Apply drag to prior velocity before gravity integration.
            /// </remarks>
            Velocities[i] *= Drag;

            /// <remarks>
            /// The terminal velocity of a rain drop is around 9m/s;
            /// The effect of drag can be simply modeled with the following.
            /// </remarks>
            Velocities[i] += Gravity * Mathf.Pow((1.0f - (Velocities[i].magnitude / 9.0f)), 2)  * Delta;

            ProjectedPositions[i] = Positions[i] + (Velocities[i] * Delta);   
        }
    }
}