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
    /// Equation #12
    /// 
    /// Position Based Fluids 2014
    /// https://mmacklin.com/pbf_sig_preprint.pdf
    /// </summary>
    [BurstCompile]
    public struct PositionCorrectionJob : IJobParallelFor
    {
        public float H;
        public float Hx;

        public float P0;
        public float Eps;

        public SphMath SMath;

        //[ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector4> ProjectedPositions;

        [ReadOnly]
        public NativeArray2D<int> Neighbors2D;

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> Densities;

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float> Lambdas;

        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> PositionCorrections;

        public void Execute(int i)
        {
            int nCount = Neighbors2D[i, 0];
            Vector3 iPosition = ProjectedPositions[i];

            Vector3 nDelta = Vector3.zero;

            int nIndex = 0;
            Vector3 nPosition = Vector3.zero;
            for (int n = 1; n < nCount + 1; n++)
            {
                nIndex = Neighbors2D[i, n];

                if (nIndex != i)
                {
                    nPosition = ProjectedPositions[nIndex];
                    nDelta += (Lambdas[i] + Lambdas[nIndex]) * SMath.GradWspiky(iPosition, nPosition, H);

                }
            }

            PositionCorrections[i] = (1.0f / P0) * nDelta;
            ProjectedPositions[i] -= new Vector4(PositionCorrections[i].x, PositionCorrections[i].y, PositionCorrections[i].z, ProjectedPositions[i].w)*2;
        }


    }
}