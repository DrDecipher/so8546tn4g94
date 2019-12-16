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
    /// Equations #7 though #11
    /// 
    /// Position Based Fluids 2014
    /// https://mmacklin.com/pbf_sig_preprint.pdf
    /// </summary>
    [BurstCompile]
    public struct LambdaJob : IJobParallelFor
    {
        public float H;
        public float Hx;

        public float P0;
        public float Eps;

        public SphMath SMath;

        [ReadOnly]
        public NativeArray<Vector4> ProjectedPositions;

        [ReadOnly]
        public NativeArray2D<int> Neighbors2D;

        [ReadOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> Densities;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float> Lambdas;

        public void Execute(int i)
        {
            float sumGradients = 0;
            Vector3 grad_ki = Vector3.zero;

            int nCount = Neighbors2D[i, 0];
            Vector3 iPosition = ProjectedPositions[i];

            int nIndex;
            Vector3 nPosition;
            Vector3 tempGradient;
            for (int n = 1; n < nCount + 1; n++)
            {
                nIndex = Neighbors2D[i, n];
                nPosition = ProjectedPositions[nIndex];
                tempGradient = SMath.GradWspiky(iPosition, nPosition, H);
                grad_ki += tempGradient;

                
                if (nIndex != i)
                {
                    sumGradients += tempGradient.x * tempGradient.x +
                                    tempGradient.y * tempGradient.y +
                                    tempGradient.z * tempGradient.z;
                }
                
            }
            
            sumGradients += grad_ki.x * grad_ki.x +
                            grad_ki.y * grad_ki.y +
                            grad_ki.z * grad_ki.z;
                            
            Lambdas[i] = -Densities[i].y / ((sumGradients / math.pow(P0, 2)) + Eps);
        }
    }
}
