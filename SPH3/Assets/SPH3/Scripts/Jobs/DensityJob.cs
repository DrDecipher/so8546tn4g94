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
    /// Compute the density at each particle location
    /// </summary>
    [BurstCompile]
    public struct DensityJob : IJobParallelFor
    {
        public float H;
        public float Hx;
        public float P0; 
        public SphMath SMath;

        [ReadOnly]
        public NativeArray<Vector4> ProjectedPositions;

        /// <summary>
        /// Neighbor Storage
        /// </summary>
        [ReadOnly]
        public NativeArray2D<int> Neighbors2D;

        [NativeDisableParallelForRestriction]
        public NativeArray<Vector2> Densities;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> TotalJ;

        
        public void Execute(int i)
        {
            float density = 0;

            /// DEBUG ONLY
            int totalJ = 0;

            int nCount = Neighbors2D[i, 0];
            Vector3 iPosition = ProjectedPositions[i];

            int nIndex = 0;
            Vector3 nPosition = Vector3.zero;
            for (int n = 1; n < nCount + 1; n++)
            {
                nIndex = Neighbors2D[i, n];
                if (nIndex != i)
                {
                    nPosition = ProjectedPositions[nIndex];

                    float dist = math.distance(iPosition, nPosition);
                    if (dist < H)
                        totalJ += 1;

                    /// Normalized distance to H
                    float r = dist * Hx ;

                    density += SMath.Wpoly6opti(r);

                }
            }

            Densities[i] = new Vector2(density, (density / P0) - 1);

            /// DEBUG ONLY - REMOVE
            TotalJ[i] = totalJ;
        }
    }
}
/// <remarks>
/// Proof of faster Wpoly6opti vs Poly6 resulting in the same pressure ratio
/// </remarks>
/* 
Wpoly6
0 = 4757.903
21 = 8502.147
253 = 10660.47

Wpoly6opti
0 = 1.631961
21 = 2.916236
253 = 3.65654

Wpoly6/Wpoly6opti Ratio Identical
21 = 1.7869

*/



/*
Wpoly6
0 = 1374.132
21 = 2734.098
56 = 3484.915

Wpoly6 Ratio 
0 = 1
21 1.989
56 = 2.53


Wpoly6opti
0 = 2.3745
21= 4.72452
56 = 6.021932

Wpoly6opti Ratio
0 = 1
21 = 1.9896
56 = 2.5360

*/
