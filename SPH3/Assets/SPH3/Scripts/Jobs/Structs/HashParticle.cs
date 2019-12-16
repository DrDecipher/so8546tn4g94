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
    /// Various Methods we need for Hash Related Jobs
    /// 
    /// We store these in this struct to make it Job System and Burst compatible.
    /// </summary>
    //[BurstCompile]
    public struct HashParticle
    {
        #region Public Variables
        public float VoxelMult;
        public Vector3 WorldOffset;

        public int TableSize;
        
        public int Prime0;
        public int Prime1;
        public int Prime2;
        #endregion

        #region Public Methods
        /// <summary>
        /// Hash position
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_offset"></param>
        /// <param name="_voxMult"></param>
        /// <param name="_prime0"></param>
        /// <param name="_prime1"></param>
        /// <param name="_prime2"></param>
        /// <param name="_tableSize"></param>
        /// <returns></returns>
        public int Hash(Vector3 _pos)
        {
            Vector3 voxPos = new Vector3
            (
             (int)((_pos.x + WorldOffset.x) * VoxelMult),
             (int)((_pos.y + WorldOffset.y) * VoxelMult),
             (int)((_pos.z + WorldOffset.z) * VoxelMult)
            );

            int xor_1 = ((int)voxPos.x * Prime0) ^ ((int)voxPos.y * Prime1);
            int xor_2 = xor_1 ^ ((int)voxPos.z * Prime2);

            return (math.abs(xor_2 % (TableSize - 1)) + 1);
        }

        /// <summary>
        /// Get the neighboring 26 buckets.
        /// </summary>
        /// <param name="_pos"></param>
        /// <param name="_offset"></param>
        /// <param name="_voxMult"></param>
        /// <param name="_prime0"></param>
        /// <param name="_prime1"></param>
        /// <param name="_prime2"></param>
        /// <param name="_tableSize"></param>
        /// <returns></returns>
        public int[] GetNeighborBuckets(Vector3 _pos)
        {        
            Vector3[] searchArray = ThreeCubedSearch();
            int[] neighbors = new int[searchArray.Length];
            for (int i = 0; i < searchArray.Length; i++)
            {
                neighbors[i] = Hash(_pos + (searchArray[i] / VoxelMult));
            }
            return neighbors;
        }

        /// <summary>
        /// Pre-calculated search off sets.
        /// Does not include 0,0,0
        /// </summary>
        /// <returns></returns>
        public Vector3[] ThreeCubedSearch()
        {
            return new Vector3[26]
            {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1),
            new Vector3(1, 0, 1),
            new Vector3(1, 0, -1),
            new Vector3(-1, 0, 1),
            new Vector3(-1, 0, -1),

            new Vector3(0, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(-1, -1, 0),
            new Vector3(0, -1, 1),
            new Vector3(0, -1, -1),
            new Vector3(1, -1, 1),
            new Vector3(1, -1, -1),
            new Vector3(-1, -1, 1),
            new Vector3(-1, -1, -1),

            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(0, 1, -1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, -1),
            new Vector3(-1, 1, 1),
            new Vector3(-1, 1, -1)
            };
        }
        #endregion
    }
}
