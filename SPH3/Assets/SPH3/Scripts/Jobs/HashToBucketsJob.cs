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
    /// Hash the particles in the voxelized buckets for =fast neighborhood searches.
    /// https://matthias-research.github.io/pages/publications/tetraederCollision.pdf
    /// </summary>
    [BurstCompile]
    public struct HashToBucketsJob : IJobParallelFor
    {
        public HashParticle HashParticle;
        public int HashBinSize;

        [NativeDisableParallelForRestriction]
        public NativeArray2D<int> HashTable2D;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> HashBucketCounter;

        public NativeArray<Vector4> ProjectedPositions;


        public void Execute(int i)
        {
            int bucket = HashParticle.Hash(ProjectedPositions[i]);

            /// Keep track so we can immediately increment
            int bucketCounter = HashBucketCounter[bucket];

            ///<remarks>
            ///In the rare, but possible, circumstance that we have two threads writing 
            ///to the same bucket, we can mitigate this by incrementing the buckets count 
            ///first and writing to the bucket second.
            ///</remarks>
            ///
            /// Increase entries for the bucket.
            HashBucketCounter[bucket] += 1;

            /// Store the Bucket in the particle position for reuse.
            ProjectedPositions[i] = new Vector4(ProjectedPositions[i].x, ProjectedPositions[i].y, ProjectedPositions[i].z, bucket);

            /// Offset by the Bucket Size
            HashTable2D[bucket, bucketCounter] = i;
        }
    }
}
