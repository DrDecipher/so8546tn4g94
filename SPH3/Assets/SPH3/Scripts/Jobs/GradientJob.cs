using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Job Related
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

using Extensions.NativeCollections;

namespace SPH3
{
    /// <summary>
    /// Compute the gradient at each particle locations based on neighbors
    /// within the smoothing kernel.
    /// </summary>
    [BurstCompile]
    public struct GradientJob : IJobParallelFor
    {
        public float H;
        public float Hx;

        public SphMath SMath;

        [ReadOnly]
        public NativeArray<Vector4> ProjectedPositions;

        [ReadOnly]
        public NativeArray2D<int> Neighbors2D;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> Gradients;

        public void Execute(int i)
        {
            Vector3 sumGradients = Vector3.zero;

            int nCount = Neighbors2D[i, 0];
            Vector3 iPosition = ProjectedPositions[i];

            int nIndex = 0;
            Vector3 nPosition = Vector3.zero;
            Vector3 tempGradient = Vector3.zero;

            for (int n = 1; n < nCount + 1; n++)
            {
                nIndex = Neighbors2D[i, n];
                if (nIndex != i)
                {
                    nPosition = ProjectedPositions[nIndex];
                    tempGradient += SMath.GradWspiky(iPosition, nPosition, H);
                }
            }
            Gradients[i] = tempGradient.normalized;
        }
    }
}
