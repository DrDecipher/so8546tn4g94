using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SPH3
{
    /// <summary>
    /// DEBUG - This is a utility class to display various properties of the simulation
    /// for debugging and analysis.
    /// </summary>
    [RequireComponent(typeof(SPH_System))]
    [ExecuteInEditMode]
    public class SPH_Inspector : MonoBehaviour
    {
        #region Private Variables
        private SPH_System sphSystem;
        private ParticleContainer pContainer;
        private HashContainer hContainer;
        #endregion

        #region Public Variables
        public int ParticleID = 0;

        [Header("Data")]
        public float ParticleSize;
        public Vector3 ParticlePosition;
        public Vector3 ParticleVelocity;
        public float Density;
        public float Ci;

        public float Gx;
        public float Gy;
        public float Gz;

        public float Lambda;

        public float PCx;
        public float PCy;
        public float PCz;

        public int TotalN;
        public int TotalJ;

        [Header("Hashing")]
        public int Bucket;
        public int[] Neighbors;

        [Header("Collisions")]      
        public Vector4 ClosetPoint;

        [Header("Debug")]
        public bool Show = false;
        public Color Color = Color.yellow;
        #endregion

        #region Unity Methods
        private void Start()
        {
            sphSystem = GetComponent<SPH_System>();
            pContainer = sphSystem.pContainer;
            hContainer = sphSystem.hContainer;
            ParticlePosition = new Vector3(0, 0, 0);
        }

        private void OnDrawGizmos()
        {
            if (Show && sphSystem)
            {
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.color = Color;
                Gizmos.DrawWireSphere(ParticlePosition, Units.Cm2M(sphSystem.RadiusCm));
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Pull particle data from the system
        /// </summary>
        public void DoParticleUpdate()
        {
            if (pContainer != null)
            {

                if (ParticleID > pContainer.ParticleCount - 1)
                    ParticleID = pContainer.ParticleCount - 1;
                if (ParticleID < 0)
                    ParticleID = 0;
            }

            if (sphSystem != null &&
                pContainer != null &&
                pContainer.ParticleCount > 0
                )
            {
                /// <remarks>
                /// Make sure we have a valid particle
                /// </remarks>
                if (ParticleID > pContainer.ParticleCount - 1)
                    ParticleID = pContainer.ParticleCount - 1;

                ParticleSize = Units.Cm2M(sphSystem.RadiusCm);

                ParticlePosition = pContainer.PositionsNative[ParticleID];
                ParticleVelocity = pContainer.VelocitiesNative[ParticleID];

                Density = pContainer.DensitiesNative[ParticleID].x;
                Ci = pContainer.DensitiesNative[ParticleID].y;

                Gx = pContainer.GradientsNative[ParticleID].x;
                Gy = pContainer.GradientsNative[ParticleID].y;
                Gz = pContainer.GradientsNative[ParticleID].z;

                Lambda = pContainer.LambdasNative[ParticleID];

                PCx = pContainer.PositionCorrectionsNative[ParticleID].x;
                PCy = pContainer.PositionCorrectionsNative[ParticleID].y;
                PCz = pContainer.PositionCorrectionsNative[ParticleID].z;

                TotalN = hContainer.Neighbors2DNative[ParticleID, 0];
                TotalJ = pContainer.TotalJNative[ParticleID];

                Bucket = (int)pContainer.ProjectedPositionsNative[ParticleID].w;

                Neighbors = new int[hContainer.NeighborBinSize];
                for (int i = 0; i < hContainer.NeighborBinSize; i++)
                    Neighbors[i] = hContainer.Neighbors2DNative[ParticleID, i];
                             
                ClosetPoint = pContainer.CollisionsNative[ParticleID];
            }
        }
        #endregion
    }
}