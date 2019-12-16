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
    /// This is the last job and it implements the
    /// Position Based Dynamics as described in:
    /// https://matthias-research.github.io/pages/publications/posBasedDyn.pdf
    /// 
    /// </summary>
    [BurstCompile]
    public struct UpdateSysJob : IJobParallelFor
    {
        public float ParticleRadius;
        public float OneDevidedByDelta;

        [ReadOnly]
        public NativeArray<Vector4> ProjectedPositions;

        [WriteOnly]
        public NativeArray<Vector3> Velocities;

        public NativeArray<Vector3> Positions;


        public void Execute(int i)
        {
            /// <ToDo>
            /// Add constant collision drag if distance to collision 
            /// is < particle radius.
            /// </ToDo>
            /// 

            /// Need to strip the w value that stores the hash bucket
            Vector3 PP3 = ProjectedPositions[i];

            /// Position Based Dynamics standard stability equation.
            Velocities[i] = ((PP3 - Positions[i]) * OneDevidedByDelta);
             
            Positions[i] = ProjectedPositions[i];
        }
    }

}
