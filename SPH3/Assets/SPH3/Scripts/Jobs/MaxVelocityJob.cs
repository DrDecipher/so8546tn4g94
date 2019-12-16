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
    /// Find the max velocity of system to subsample the frames, 
    /// ensuring proper collision detection.
    /// </summary>
    [BurstCompile]
    public struct MaxVelocityJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> Velocities;

        [NativeDisableParallelForRestriction]
        public NativeArray<float> VelocityPeek;


        public void Execute(int i)
        {
            if (VelocityPeek[0] < Velocities[i].magnitude)
                VelocityPeek[0] = Velocities[i].magnitude;
        }
    }
}