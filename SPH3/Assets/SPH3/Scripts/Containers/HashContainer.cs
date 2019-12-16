using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Job System
using Unity.Collections;
using Extensions.NativeCollections;



namespace SPH3
{
    /// <summary>
    /// Hash system is an implementation based on:
    /// https://matthias-research.github.io/pages/publications/tetraederCollision.pdf
    /// </summary>
    public class HashContainer
    {
        public HashContainer()
        {
            /// <remarks>
            /// We need the Hash and Neighbor bins at least twice the possible number 
            /// that found in a single as we may have 2x-3x collisions. 
            /// 
            /// I've chosen 3x as a default.
            /// </remarks>
            HashBinSize = 12;
            NeighborBinSize = 162; ///(HashBinSize*27)

            TableSize = 999991;
            Prime0 = 73856093;
            Prime1 = 19349663;
            Prime2 = 83492791;

            /// Hash Properties
            WorldOffset = new Vector3Int(23869, 34471, 71807);
        }

        #region Public Variables
        public float H;

        public int TableSize;
        public int Prime0;
        public int Prime1;
        public int Prime2;

        /// <summary>
        /// Used to prevent hash mirroring around the origin.
        /// </summary>
        public Vector3 WorldOffset;


        /// <summary>
        /// We are going store the hash table 2D array as a 1D for speed.
        /// </summary>
        public int HashBinSize;

        public NativeArray2D<int> HashTable2DNative;

        /// <summary>
        /// Maximum number of neighbors
        /// This will be optimized in time. We do not 
        /// currently know the best size.
        /// </summary>
        public int NeighborBinSize;

        /// <summary>
        /// Neighbor collection bins
        /// We collect all the neighbors in one 
        /// pass before, processing the forces.
        /// 
        /// We are also using pooling here.
        /// 
        /// This is a flattened 2D array due to limitations of 
        /// the job system. Index of neighbors array = 
        /// (Particle Number - 1 ) * NeighborBinSize
        /// </summary>
        public NativeArray2D<int> Neighbors2DNative;
        #endregion

        #region Public Methods
        /// <summary>
        /// Allocate memory with defaults
        /// 
        /// Where the original paper hashed the particles and neighborhood voxels
        /// on the fly we are hashing the particles then hashing the buckets. The reduction in 
        /// calculations needed is orders of magnitude less. 
        /// 
        /// </summary>
        /// <param name="_count"></param>
        public void PopulateContiner(int _particleCount)
        {
            HashTable2DNative = new NativeArray2D<int>(TableSize, HashBinSize, Allocator.Persistent);

            /// <remarks> 
            /// Allocate memory for Neighbor bins 
            /// </remarks>
            Neighbors2DNative = new NativeArray2D<int>(_particleCount, NeighborBinSize, Allocator.Persistent);
        }

        /// <summary>
        /// Dispose of the memory in our native arrays.
        /// </summary>
        public void OnDestroy()
        {
            HashTable2DNative.Dispose();
            Neighbors2DNative.Dispose();
        }
        #endregion
    }
}
