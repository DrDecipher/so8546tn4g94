using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// Job Related
using Unity.Collections;
using Unity.Jobs;


namespace SPH3
{
    /// <summary>
    /// Core SPH code
    /// 
    /// Implements the job system an individual jobs.
    /// 
    /// Will be modified to Queue Jobs in the future for speed after all math 
    /// and functionality has been verified.
    /// </summary>
    [ExecuteInEditMode]
    public class SPH_System : MonoBehaviour
    {
        #region Private Variables
        private StopWatch stopWatch;
        private Vector3[] handoffPositions;
        private RenderFluidAsInstancesIndirect renderInstancesComp;
        private bool firstLoop = true;

        /// <summary>
        /// Jobs Variables
        /// </summary>
        private List<JobHandle> JobHandles;
        
        private MaxVelocityJob m_MaxVelJob;
        private JobHandle m_MaxVelHandle;

        private ProjectJob m_ProjectJob;
        private JobHandle m_ProjectHandle;

        private HashToBucketsJob m_HashToBucketsJob;
        private JobHandle m_HashToBucketsHandle;

        private CollectNeighborsJob m_CollectNeighborsJob;
        private JobHandle m_CollectNeighborsHandle;

        private DensityJob m_DensityJob;
        private JobHandle m_DensityHandle;

        private GradientJob m_GradientJob;
        private JobHandle m_GradientHandle;

        private LambdaJob m_LambdaJob;
        private JobHandle m_LambdaHandle;

        private PositionCorrectionJob m_PositionCorrectionJob;
        private JobHandle m_PositionCorrectionHandle;

        private PlaneCollideJob m_PlaneCollideJob;
        private JobHandle m_PlaneCollideHandle;

        private UpdateSysJob m_UpdateSysJob;
        private JobHandle m_UpdateSysHandle;

        private SPH_Inspector inspector;
        #endregion

        #region Public Variables
        [Header("Components")]
        public SPH_Emitter[] Emitters;
        public SPH_Collider[] Colliders;
        

        public ParticleContainer pContainer;
        public HashContainer hContainer;

        [Header("Controls")]
        public bool Pause = false;
        public bool Step = false;


        [Header("Diagnostics")]
        public bool AutoBatch = true;
        public int BatchCount = 100;
        public float SimulationTime;
        public int ParticleCount;
        public float VelocityPeek = 0;
        public float Delta = 0;
        public float SubDelta = 0;

        [Header("Settings")]
        public int SolverIterations = 4;
        public float RestDenstiy;
        public float FluidEpsilon = 600;
        public float RadiusCm = 1;

        /// <remarks>
        /// *1 = 1 per bucket = 27 total
        /// *2 = 1-2 per bucket = 54 total
        /// *3 = 2-4 per bucket = 108 total
        /// </remarks>
        public float RadiusMult = 2.65f;

        public float VoxelSize = 2.65f;
        public float VoxelMult = 1.2f;
        public float HCm = 4;

        public float GravityMps = -9.8f;
        public float VelocityCap = 3.0f;
        public float Drag = 0.01f;
        public int SubframeSampling = 4;

        public bool DynamicSampling = false;
        private bool dynamicSampling = false;

        [Header("Debug")]
        public bool DebugOn = false;
        public bool ForceRepaint = true;
        public bool DrawOnGPU = true;      
        public bool DebugHashTableInsertion = false;


        [Header("Hashing")]
        public bool HashingOn = true;

        /// <remarks>
        /// DEBUG : Exposed for the Particle Inspector
        /// </remarks>
        public NativeArray<int> HashBucketCounterNative;
        #endregion

        #region Unity Methods
        private void Start()
        {
            stopWatch = new StopWatch();
            if (Application.isPlaying)
            {
                pContainer = new ParticleContainer();
                pContainer.Radius = Units.Cm2M(RadiusCm);

                pContainer.PopulateContiner();

                hContainer = new HashContainer();
                hContainer.PopulateContiner(pContainer.MaxParticleCount);

                VoxelSize = RadiusCm * RadiusMult;

                HCm = RadiusCm * VoxelMult;              
            }
            inspector = GetComponent<SPH_Inspector>();

        }

        /// <summary>
        /// In order to control the execution of all components 
        /// this is the one update function that invokes all related class method's.
        /// </summary>
        private void Update()
        {
            /// <remarks>
            /// Due to precision errors 0.4cm is the smallest
            /// particle size.  This can be worked around in the future by 
            /// either scaling the scene directly or in code.
            /// </remarks>
            if (RadiusCm < 0.4f)
                RadiusCm = 0.4f;

            /// <summary>
            /// Passing variable in Edit Mode
            /// </summary>
            if (Emitters.Length > 0)
            {
                for (int i = 0; i < Emitters.Length; i++)
                {
                    Emitters[i].SPHSystem = this;
                }
            }
            if (Colliders.Length > 0)
            {
                for (int i = 0; i < Colliders.Length; i++)
                {
                    Colliders[i].SPHSystem = this;
                }
            }

            /// <summary>
            /// Only simulate in Play Mode
            /// </summary>
            if (Application.isPlaying)
            {
                SimulationTime += Time.deltaTime;

                if (Time.deltaTime > 0 && !firstLoop )
                {
                    /// Since Last Frame
                    Delta = Time.deltaTime;

                    /// Get New Particles
                    /// Note: We will need to add sub-frame sampling 
                    /// to this method during constant emission.
                    DoEmission();

                    /// <summary>
                    /// To leave the main thread unobstructed we will use one 
                    /// less the number of available cores.
                    /// </summary>
                    if (AutoBatch)
                        BatchCount = ParticleCount / (System.Environment.ProcessorCount - 1);

                    /// Find Max Velocity
                    if (DebugOn)
                        stopWatch.Start();

                    stopWatch.Start();
                    m_MaxVelJob = new MaxVelocityJob()
                    {
                        Velocities = pContainer.VelocitiesNative,
                        VelocityPeek = pContainer.VelocityPeekNative
                    };
                    m_MaxVelHandle = m_MaxVelJob.Schedule(ParticleCount, BatchCount);
                    m_MaxVelHandle.Complete();

                    if (DebugOn)
                        Debug.Log("Velocity: " + stopWatch.Check());


                    /// <remarks>
                    /// We want to increase sample so that per iteration a particle can 
                    /// not move more than it's radius. Otherwise collisions may be missed.
                    /// 
                    /// A more robust version of this is possible as collision detection is refined.
                    /// </remarks>
                    VelocityPeek = pContainer.VelocityPeekNative[0];
                    pContainer.VelocityPeekNative[0] = 0;

                    if (Delta > 0 && dynamicSampling)
                    {
                        SubframeSampling = (int)(((VelocityPeek * Delta) / Units.Cm2M(RadiusCm)) * 1.3f);
                        /// <remarks>
                        /// We can't have less than one sample and we need some type of cap in case 
                        /// of continued acceleration by gravity.
                        /// </remarks>
                        if (SubframeSampling < 1)
                            SubframeSampling = 1;
                        //else if (SubframeSampling > 30)
                            //SubframeSampling = 30;
                    }

                    /// Delta of Each Sub-Sample
                    SubDelta = Delta / SubframeSampling;

                    /// <remarks>
                    /// Main Integration Loop
                    /// </remarks>
                    for (int i = 0; i < SubframeSampling; i++)
                    {

                        /// <remarks>
                        /// Projection Step
                        /// </remarks>
                        if (DebugOn)
                            stopWatch.Start();

                        m_ProjectJob = new ProjectJob()
                        {
                            Drag = 1 - Drag,
                            Delta = SubDelta,
                            Gravity = new Vector3(0, GravityMps, 0),
                            Positions = pContainer.PositionsNative,
                            ProjectedPositions = pContainer.ProjectedPositionsNative,
                            Velocities = pContainer.VelocitiesNative

                        };
                        m_ProjectHandle = m_ProjectJob.Schedule(ParticleCount, BatchCount);
                        m_ProjectHandle.Complete();

                        if (DebugOn)
                            Debug.Log("Project: " + stopWatch.Check());



                        if (HashingOn)
                        {
                            /// <summary>
                            /// The bin size is fixed and reused so we need to 
                            /// keep track of how many slots are in use per search. 
                            /// It is faster to re-allocate and dispose than zero out the values.
                            /// </summary>                       
                            HashBucketCounterNative = new NativeArray<int>(hContainer.TableSize, Allocator.TempJob);

                            /// <remarks>
                            /// We can not use statics or pass classes to the job system.
                            /// We must pass structs.
                            /// </remarks>
                            if (DebugOn)
                                stopWatch.Start();
                            m_HashToBucketsJob = new HashToBucketsJob()
                            {
                                HashParticle = new HashParticle()
                                {
                                    VoxelMult = 1.0f / Units.Cm2M(VoxelSize),
                                    WorldOffset = hContainer.WorldOffset,

                                    TableSize = hContainer.TableSize,

                                    Prime0 = hContainer.Prime0,
                                    Prime1 = hContainer.Prime1,
                                    Prime2 = hContainer.Prime2,
                                },

                                HashBinSize = hContainer.HashBinSize,

                                HashTable2D = hContainer.HashTable2DNative,
                                HashBucketCounter = HashBucketCounterNative,

                                ProjectedPositions = pContainer.ProjectedPositionsNative
                            };

                            m_HashToBucketsHandle = m_HashToBucketsJob.Schedule(ParticleCount, BatchCount);
                            m_HashToBucketsHandle.Complete();

                            if (DebugOn)
                                Debug.Log("Hash to buckets: " + stopWatch.Check());

                            
                            /// <remarks>
                            /// Neighbor Collection
                            /// </remarks>
                            if (DebugOn)
                                stopWatch.Start();
                            m_CollectNeighborsJob = new CollectNeighborsJob()
                            {
                                HashParticle = new HashParticle()
                                {
                                    VoxelMult = 1.0f / Units.Cm2M(VoxelSize),
                                    WorldOffset = hContainer.WorldOffset,

                                    TableSize = hContainer.TableSize,

                                    Prime0 = hContainer.Prime0,
                                    Prime1 = hContainer.Prime1,
                                    Prime2 = hContainer.Prime2,
                                },

                                ProjectedPositions = pContainer.ProjectedPositionsNative,

                                HashTable2D = hContainer.HashTable2DNative,
                                HashBucketCounter = HashBucketCounterNative,

                                Neighbors2D = hContainer.Neighbors2DNative

                            };

                            m_CollectNeighborsHandle = m_CollectNeighborsJob.Schedule(ParticleCount, BatchCount);
                            m_CollectNeighborsHandle.Complete();

                            if (DebugOn)
                                Debug.Log("Collect Neighbors: " + stopWatch.Check());
                            

                            /// Debugging hash table.
                            if (DebugHashTableInsertion)
                            {
                                for (int b = 0; b < HashBucketCounterNative.Length; b++)
                                {
                                    if (HashBucketCounterNative[b] > 0)
                                    {
                                        Debug.Log(HashBucketCounterNative[b]);
                                        for (int p = 0; p < HashBucketCounterNative[b]; p++)
                                        {
                                            Debug.Log(b + " " + hContainer.HashTable2DNative[b, p]);
                                        }
                                        
                                    }
                                }
                            }

                            /// <remarks>
                            /// Dispose of temporary job memory.
                            /// </remarks>
                            if (HashBucketCounterNative.IsCreated)
                                HashBucketCounterNative.Dispose();

                        }

                        /// <remarks>
                        /// Main Solver
                        /// </remarks>
                        for (int s = 0; s < SolverIterations; s++)
                        {
                            /// <remarks>
                            /// Density Calculation
                            /// </remarks>
                            if (DebugOn)
                                stopWatch.Start();

                            m_DensityJob = new DensityJob()
                            {
                                H = Units.Cm2M(HCm),
                                Hx = 1 / Units.Cm2M(HCm),
                                P0 = RestDenstiy,
                                SMath = new SphMath() { },
                                ProjectedPositions = pContainer.ProjectedPositionsNative,
                                Neighbors2D = hContainer.Neighbors2DNative,
                                Densities = pContainer.DensitiesNative,
                                TotalJ = pContainer.TotalJNative
                            };

                            m_DensityHandle = m_DensityJob.Schedule(ParticleCount, BatchCount);
                            m_DensityHandle.Complete();

                            if (DebugOn)
                                Debug.Log("Calculated Pressure: " + stopWatch.Check());


                            /// <remarks>
                            /// Lambda Calculation
                            /// </remarks>
                            if (DebugOn)
                                stopWatch.Start();

                            m_LambdaJob = new LambdaJob()
                            {
                                H = Units.Cm2M(HCm),
                                Hx = 1 / Units.Cm2M(HCm),
                                P0 = RestDenstiy,
                                Eps = FluidEpsilon,
                                SMath = new SphMath() { },
                                ProjectedPositions = pContainer.ProjectedPositionsNative,
                                Neighbors2D = hContainer.Neighbors2DNative,
                                Densities = pContainer.DensitiesNative,
                                Lambdas = pContainer.LambdasNative,
                            };

                            m_LambdaHandle = m_LambdaJob.Schedule(ParticleCount, BatchCount);
                            m_LambdaHandle.Complete();

                            if (DebugOn)
                                Debug.Log("Calculated Pressure: " + stopWatch.Check());

                            /// <remarks>
                            /// Position Correction Calculation
                            /// </remarks>
                            if (DebugOn)
                                stopWatch.Start();

                            m_PositionCorrectionJob = new PositionCorrectionJob()
                            {
                                H = Units.Cm2M(HCm),
                                Hx = 1 / Units.Cm2M(HCm),
                                P0 = RestDenstiy,
                                Eps = FluidEpsilon,
                                SMath = new SphMath() { },
                                ProjectedPositions = pContainer.ProjectedPositionsNative,
                                Neighbors2D = hContainer.Neighbors2DNative,
                                Densities = pContainer.DensitiesNative,
                                Lambdas = pContainer.LambdasNative,
                                PositionCorrections = pContainer.PositionCorrectionsNative
                            };

                            m_PositionCorrectionHandle = m_PositionCorrectionJob.Schedule(ParticleCount, BatchCount);
                            m_PositionCorrectionHandle.Complete();

                            if (DebugOn)
                                Debug.Log("Calculated Position Correction: " + stopWatch.Check());


                            /// <remarks>
                            /// Collisions
                            /// </remarks>
                            if (DebugOn)
                                stopWatch.Start();
                            for (int c = 0; c < Colliders.Length; c++)
                            {
                                /// Primitive Collisions
                                switch (Colliders[c].ColliderType)
                                {
                                    case ColliderTypeEnum.Plane:

                                        m_PlaneCollideJob = new PlaneCollideJob()
                                        {
                                            TRS = Colliders[c].TRS,
                                            ParticleRadius = Units.Cm2M(RadiusCm),

                                            ProjectedPositions = pContainer.ProjectedPositionsNative,
                                            Collisions = pContainer.CollisionsNative,

                                            InfinitePlane = Colliders[c].Infinite,
                                            Min = -Units.Cm2M(Colliders[c].Size2dCm) * Units.HALF,
                                            Max = Units.Cm2M(Colliders[c].Size2dCm) * Units.HALF
                                        };
                                        m_PlaneCollideHandle = m_PlaneCollideJob.Schedule(ParticleCount, BatchCount);
                                        m_PlaneCollideHandle.Complete();

                                        break;

                                    case ColliderTypeEnum.Box:
                                        /// TBA
                                        break;

                                    case ColliderTypeEnum.Sphere:
                                        /// TBA
                                        break;

                                    /// <remarks> 
                                    /// Needs debugging. Transformation matrix is off.
                                    /// </remarks>
                                    case ColliderTypeEnum.Cylinder:

                                        for (int cc = 0; cc < pContainer.ParticleCount; cc++)
                                        {
                                            Vector4 ClosesPoint = Vector4.zero;
                                            Vector3 ParticlePoint = Vector3.zero;
                                            CylinderCollide.CylinderCappedClosestPoint(Colliders[0].TRS.inverse.MultiplyPoint
                                            (
                                                pContainer.ProjectedPositionsNative[cc]), Units.Cm2M(Colliders[0].RadiusCm), Units.Cm2M(Colliders[0].HeightCm), Units.Cm2M(RadiusCm), ref ClosesPoint, ref ParticlePoint
                                            );

                                            if (Mathf.Abs(ClosesPoint.w) < Units.Cm2M(RadiusCm))
                                            {
                                                pContainer.ProjectedPositionsNative[cc] = Colliders[0].TRS.MultiplyPoint(ParticlePoint);

                                            }

                                            pContainer.CollisionsNative[cc] = ClosesPoint;
                                        }

                                        break;
                                }

                            }
                            if (DebugOn)
                                Debug.Log("Collisions: " + stopWatch.Check());
                        }

                        /// <remarks>
                        /// Update Positions and Velocities
                        /// </remarks>
                        if (DebugOn)
                            stopWatch.Start();
                        m_UpdateSysJob = new UpdateSysJob()
                        {
                            ParticleRadius = Units.Cm2M(RadiusCm),
                            OneDevidedByDelta = 1.0f / SubDelta,
                            Positions = pContainer.PositionsNative,
                            ProjectedPositions = pContainer.ProjectedPositionsNative,
                            Velocities = pContainer.VelocitiesNative
                        };
                        m_UpdateSysHandle = m_UpdateSysJob.Schedule(ParticleCount, BatchCount);
                        m_UpdateSysHandle.Complete();

                        if (DebugOn)
                            Debug.Log("Update: " + stopWatch.Check());


                    }

                    Step = false;
                }
                else
                    firstLoop = false;

                /// <remarks>
                /// Draw Particles
                // </remarks>
                if (DrawOnGPU)
                    DrawParticlesGPU();

                /// <summary>
                /// This is an implementation of our own Play/Pause/Step system 
                /// so we have a consistent time step in testing.
                /// </summary>
                if (!Pause || Step)
                {
                    Time.timeScale = 1;
                }
                else if (Time.timeScale != 0)
                {
                    Time.timeScale = 0;
                }
                dynamicSampling = DynamicSampling;

                if (ForceRepaint)
                {
                    EditorWindow view = EditorWindow.GetWindow<SceneView>();
                    view.Repaint();
                }
            }

            /// DEBUG ONLY
            inspector.DoParticleUpdate();
        }

        /// <summary>
        /// Clean Up Our Native Classes
        /// </summary>
        private void OnDestroy()
        {
            if (pContainer != null)
                pContainer.OnDestroy();
            
            if (hContainer != null)
                hContainer.OnDestroy();

        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Spawn New Particles
        /// </summary>
        private void DoEmission()
        {
            if (Emitters.Length > 0)
            {
                if (DebugOn)
                    stopWatch.Start();

                for (int i = 0; i < Emitters.Length; i++)
                {
                    Emitters[i].SPHSystem = this;
                    handoffPositions = Emitters[i].GetParticles();

                    /// <remarks>
                    /// If we have new particle add them to the container
                    /// </remarks>
                    if (handoffPositions.Length > 0)
                    {
                        pContainer.AddParticles(Emitters[i].Fluid, handoffPositions, Emitters[i].transform.forward * Emitters[i].VelocityMps);
                    }
                }
                if (DebugOn)
                    Debug.Log("Emitting: " + stopWatch.Check());
            }
            ParticleCount = pContainer.ParticleCount;
        }

        /// <summary>
        /// Draw particles as instances primitives on the GPU
        /// </summary>
        private void DrawParticlesGPU()
        {
            if (ParticleCount > 0)
            {
                renderInstancesComp = GetComponent<RenderFluidAsInstancesIndirect>();
                if (renderInstancesComp != null && renderInstancesComp.isActiveAndEnabled)
                {
                    renderInstancesComp.DrawFluid(pContainer, ParticleCount);
                }
            }
        }
        #endregion
    }
}
