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
    /// Collect all of the particles in the neighboring 26 voxels
    /// </summary>
    //[BurstCompile]
    public struct CollectNeighborsJob : IJobParallelFor
    {
        /// <summary>
        /// Helper methods
        /// </summary>
        public HashParticle HashParticle;

        /// <summary>
        /// .w Contains our hash bucket
        /// </summary>
        [ReadOnly]
        public NativeArray<Vector4> ProjectedPositions;

        /// <summary>
        ///  Hash Storage
        /// </summary>
        [ReadOnly]
        public NativeArray2D<int> HashTable2D;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> HashBucketCounter;

        /// <summary>
        /// Neighbor Storage
        /// </summary>
        [NativeDisableParallelForRestriction]
        public NativeArray2D<int> Neighbors2D;


        //[BurstCompile]
        public void Execute(int i)
        {
            int bucket = (int)ProjectedPositions[i].w;
            Vector3 pos = ProjectedPositions[i];

            /// Reset neighbor counter which 
            /// is the first index(0) of the array
            Neighbors2D[i, 0] = 0;

            /// Bucket 0
            for (int p = 0; p < HashBucketCounter[bucket]; p++)
            {
                /// Skip the first index counter
                int placeHere = Neighbors2D[i, 0] + 1;

                int vert = HashTable2D[bucket, p];
                Neighbors2D[i, placeHere] = vert;

                /// Increment the counter
                Neighbors2D[i, 0] += 1;
            }

            
            /// 26 Surrounding Buckets
            int[] otherBuckets = HashParticle.GetNeighborBuckets(pos);
            for (int b = 0; b < 26; b++)
            {
                // Neighbor bucket has particles
                int nBucket = otherBuckets[b];
                if (HashBucketCounter[nBucket] > 0)
                {
                    /// Bucket nth
                    for (int p = 0; p < HashBucketCounter[nBucket]; p++)
                    {
                        int placeHere = Neighbors2D[i, 0] + 1;

                        int vert = HashTable2D[nBucket, p];
                        Neighbors2D[i, placeHere] = vert;

                        Neighbors2D[i, 0] += 1;
                    }
                }
            }                      
        }
    }
}


