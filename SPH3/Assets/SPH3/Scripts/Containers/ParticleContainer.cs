using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Job System
using Unity.Collections;

namespace SPH3
{
    /// <summary>
    /// The New Job System only operates on the NativeArray class.
    /// 
    /// This container holds and manages all the memory of the particle 
    /// system not related to hashing.
    /// </summary>
    public class ParticleContainer
    {
      
        /// <summary>
        /// Initialize Class Defaults
        /// Defaults:
        ///     MaxParticleCount = 100000;
        ///     ParticleCount = 0;
        ///     NeighborBinSize = 200;
        /// </summary>
        public ParticleContainer()
        {
            MaxParticleCount = 100000;
            ParticleCount = 0;
        }

        #region Public Variables
        /// <summary>
        /// Stored in Meters
        /// </summary>
        public float Radius;

        /// <remarks>
        /// Due to limitations in the job system's data types, 
        /// we will have our Particles are not made of a
        /// a struct or class, as we would usually, but separated by components 
        /// to enable processing of shared memory.
        /// Looks ugly, but magnitudes faster.
        /// </remarks>

        /// <summary>
        /// Maximum number of particles
        /// </summary>
        public int MaxParticleCount;

        /// <summary>
        /// Current particle count
        /// To tracking pooling.
        /// </summary>
        public int ParticleCount;

        /// <summary>
        /// Contains: 
        /// x,y,z = Position in world space
        /// </summary>
        public NativeArray<Vector3> PositionsNative;

        /// <summary>
        /// Contains: 
        /// x,y,z = Position in world space
        /// w = hash bucket value of coordinates.
        /// </summary>
        public NativeArray<Vector4> ProjectedPositionsNative;

        /// <summary>
        /// Velocity of particle
        /// </summary>
        public NativeArray<Vector3> VelocitiesNative;

        /// <summary>2
        /// Contains: 
        ///     Mass,
        ///     Viscosity,
        /// </summary>
        public NativeArray<Vector2> PropertiesNative;

        /// <summary>
        /// We'll store the last collision or possible collision
        /// to determine if we have crossed the barrier of an object.
        ///  
        /// A collision has occurred if we have the and collision object 
        /// flagged and we have flipped signs in distance.
        /// x = Collider index
        /// y = signed distance
        /// </summary>
        public NativeArray<Vector4> CollisionsNative;
   
        /// <summary>
        /// Used to receive the Magnitude of the largest Velocity of the system.
        /// 
        /// This is a bit of a hack but the only way in the current 
        /// Job System to return a value processed by the job.
        /// https://unitycodemonkey.com/video.php?v=YBrCR9rUOaA
        /// </summary>
        public NativeArray<float> VelocityPeekNative;

        /// <summary>
        /// We calculate the pressure per iteration and store it here.
        /// .x = pressure
        /// .y = Ci (Density Differential)
        /// </summary>
        public NativeArray<Vector2> DensitiesNative;

        /// <summary>
        /// DEBUG ONLY - values to track how may particle are within out smoothing kernel.
        /// </summary>
        public NativeArray<int> TotalJNative;

        /// <summary>
        /// We will pre-process all the gradient at each particle location for later use
        /// </summary>
        public NativeArray<Vector3> GradientsNative;

        /// <summary>
        /// Lambda is the strength of the density constraint.
        /// </summary>
        public NativeArray<float> LambdasNative;

        public NativeArray<Vector3> PositionCorrectionsNative;
        #endregion

        #region Public Methods
        /// <summary>
        /// Allocate Memory with defaults
        /// </summary>
        /// <param name="_count"></param>
        public void PopulateContiner()
        {
            /// <remarks> 
            /// Allocate memory for particles 
            /// </remarks>
            PositionsNative = new NativeArray<Vector3>(MaxParticleCount, Allocator.Persistent);
            ProjectedPositionsNative = new NativeArray<Vector4>(MaxParticleCount, Allocator.Persistent);

            VelocitiesNative = new NativeArray<Vector3>(MaxParticleCount, Allocator.Persistent);
            PropertiesNative = new NativeArray<Vector2>(MaxParticleCount, Allocator.Persistent);

            CollisionsNative = new NativeArray<Vector4>(MaxParticleCount, Allocator.Persistent);

            VelocityPeekNative = new NativeArray<float>(1, Allocator.Persistent);
            VelocityPeekNative[0] = 0;

            DensitiesNative = new NativeArray<Vector2>(MaxParticleCount, Allocator.Persistent);
            TotalJNative = new NativeArray<int>(MaxParticleCount, Allocator.Persistent);

            GradientsNative = new NativeArray<Vector3>(MaxParticleCount, Allocator.Persistent);
            LambdasNative = new NativeArray<float>(MaxParticleCount, Allocator.Persistent);

            PositionCorrectionsNative = new NativeArray<Vector3>(MaxParticleCount, Allocator.Persistent);

        }

        /// <summary>
        /// Initialize memory with non-default values
        /// </summary>
        /// <param name="_maxParticleCount"></param>
        /// <param name="_neighborBinSize"></param>
        public void PopulateContiner(int _maxParticleCount)
        {
            MaxParticleCount = _maxParticleCount;
            PopulateContiner();
        }

        /// <summary>
        /// Adding a particle to the system pool
        /// </summary>
        /// <param name="_fluidProperties"></param>
        /// <param name="_position"></param>
        /// <param name="_velocity"></param>
        public void AddParticle(FluidBase _fluidProperties, Vector3 _position, Vector3 _velocity)
        {
            if (ParticleCount < MaxParticleCount)
            {
                PositionsNative[ParticleCount] = _position;
                VelocitiesNative[ParticleCount] = _velocity;
                PropertiesNative[ParticleCount] = new Vector2(_fluidProperties.Mass, _fluidProperties.Viscosity);

                ParticleCount += 1;
            }
        }
        /// <summary>
        /// Add an array of particles
        /// </summary>
        /// <param name="_fluidProperties"></param>
        /// <param name="_positions"></param>
        /// <param name="_velocity"></param>
        public void AddParticles(FluidBase _fluidProperties, Vector3[] _positions, Vector3 _velocity)
        {
            for (int i = 0; i < _positions.Length; i++)
            {
                AddParticle(_fluidProperties, _positions[i], _velocity);
            }
        }
        /// <summary>
        /// To delete a particle we simply shuffle it with the last 
        /// particle in the array and lower the count reducing the length.
        /// </summary>
        /// <param name="_particleNumber"></param>
        public void DeleteParticle(int _particleNumber)
        {
            if (ParticleCount > 0)
            {
                PositionsNative[ParticleCount] = PositionsNative[ParticleCount - 1];
                VelocitiesNative[ParticleCount] = VelocitiesNative[ParticleCount - 1];
                PropertiesNative[ParticleCount] = PropertiesNative[ParticleCount - 1];
                ParticleCount -= 1;
            }
        }

        /// <summary>
        /// Dispose of the memory in our native arrays.
        /// </summary>
        public void OnDestroy()
        {
            PositionsNative.Dispose();
            ProjectedPositionsNative.Dispose();
            CollisionsNative.Dispose();

            VelocitiesNative.Dispose();           
            VelocityPeekNative.Dispose();

            PropertiesNative.Dispose();

            DensitiesNative.Dispose();
            TotalJNative.Dispose();

            GradientsNative.Dispose();
            LambdasNative.Dispose();

            PositionCorrectionsNative.Dispose();
        }
        #endregion
    }

}
