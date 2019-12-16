using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SPH3
{
    /// <summary>
    /// Multipurpose emitter for the SPH system
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [ExecuteInEditMode]
    public class SPH_Emitter : MonoBehaviour
    {
        #region Private Variables
        private bool hasBeenWarned = false;
        private MeshFilter meshFilter;
        #endregion

        #region Public Variables
        public Matrix4x4 TRS;
        /// <remarks> Fluid System </remarks>
        public SPH_System SPHSystem;

        /// <remarks> Fluid Properties </remarks>
        public FluidBase Fluid;
        public Color Color = Color.yellow;
        
        /// <remarks>
        /// Time Properties
        /// </remarks>
        public TimeTypeEnum TimeType = TimeTypeEnum.Animate;
        public float StartTime = 0;  // For Spawn
        public float StopTime = 2;   // For Spawn
        public bool Active = true;   // For Spawn
        public bool Trigger = false; // For At_Once

        /// <remarks>
        /// Emission Properties
        /// </remarks>      
        public ShapeTypeEnum ShapeType = ShapeTypeEnum.Disk;
        public Vector3  Size3dCm = new Vector3(10, 10, 10);
        public Vector2  Size2dCm = new Vector2(10, 10);
        public float    RadiusCm = 10;
        public float    HeightCm = 100;
        public float    VelocityMps = 0f;
        public bool     Visualize = false;

        #endregion

        #region Unity Methods
        /// <summary>
        /// Make sure we have a display mesh
        /// </summary>
        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
                meshFilter.sharedMesh = new Mesh();
        }

        /// <summary>
        /// Start Up
        /// </summary>
        private void Start()
        {
            hasBeenWarned = false;

            if (!Application.isPlaying)
                TRS = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

        }

        /// <summary>
        /// Per frame local
        /// </summary>
        private void Update()
        {
            /// <remarks>
            /// Housekeeping Goes Here...
            /// </remarks>   
            GUIUtilities.Instance.LockScale(transform);
            GUIUtilities.Instance.EnforcePositiveSize(ref Size3dCm);
            GUIUtilities.Instance.EnforcePositiveSize(ref Size2dCm);
            GUIUtilities.Instance.EnforcePositiveSize(ref RadiusCm);
            GUIUtilities.Instance.EnforcePositiveSize(ref HeightCm);
            TRS = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);


            if (Visualize && !Application.isPlaying && SPHSystem)
                PreviewParticles();
            else
                meshFilter.sharedMesh = null;
        }

        /// <summary>
        /// Draw the gizmos of the selected shape type
        /// </summary>
        private void OnDrawGizmos()
        {
            Gizmos.matrix = TRS;
            Gizmos.color = Color;

            switch (ShapeType)
            {
                case ShapeTypeEnum.Disk:
                    GizmoUtilities.Instance.DrawCircle(Units.Cm2M(RadiusCm), 20, true);
                    break;
                case ShapeTypeEnum.Plane:
                    GizmoUtilities.Instance.DrawPlane(
                        Units.Cm2M(Size2dCm), true);
                    break;
                case ShapeTypeEnum.Box:
                    GizmoUtilities.Instance.DrawBox(
                        Units.Cm2M(Size3dCm), true);
                    break;
                case ShapeTypeEnum.Cylinder:
                    GizmoUtilities.Instance.DrawCylinder(Units.Cm2M(RadiusCm), Units.Cm2M(HeightCm), true);
                    break;
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Used by the system to get particles, if any, from the emitter.
        /// </summary>
        public Vector3[] GetParticles()
        {
            /// <remarks>
            /// Emission Goes Here...
            /// </remarks> 
            if (CheckFluid() && Application.isPlaying)
            {
                TRS = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                switch (TimeType)
                {
                    case TimeTypeEnum.Animate:
                        if (Active)
                        {
                            return (SpawnParticles(true, false));
                        }
                        break;

                    case TimeTypeEnum.Range:
                        if (SPHSystem.SimulationTime > StartTime && SPHSystem.SimulationTime < StopTime)
                        {
                            return (SpawnParticles(true, false));
                        }                           
                        break;

                    case TimeTypeEnum.Trigger:
                        if (Trigger)
                        {
                            Trigger = false;
                            return (SpawnParticles(true, false));
                            
                        }
                        break;
                }
                
            }
            return (new Vector3[0]);
        }


        #endregion

        #region Private Methods
        /// <summary>
        /// Create a particle array based on the emitter parameters
        /// </summary>
        /// <param name="_worldSpace"></param>
        /// <param name="_preview"></param>
        /// <returns></returns>
        private Vector3[] SpawnParticles(bool _worldSpace, bool _preview)
        {
            Vector3[] positions = new Vector3[0];

            switch (ShapeType)
            {
                case ShapeTypeEnum.Disk:
                    positions = SpawnLayer(Units.Cm2M(RadiusCm), Units.Cm2M(SPHSystem.RadiusCm), true, _worldSpace);
                    break;
                case ShapeTypeEnum.Plane:
                    positions = SpawnLayer(Units.Cm2M(Size2dCm.x), Units.Cm2M(Size2dCm.y), Units.Cm2M(SPHSystem.RadiusCm), false, _worldSpace);
                    break;
                case ShapeTypeEnum.Box:
                    positions = SpawnLayers(Units.Cm2M(Size3dCm.x), Units.Cm2M(Size3dCm.y), Units.Cm2M(Size3dCm.z), Units.Cm2M(SPHSystem.RadiusCm), false, _worldSpace, _preview);
                    break;
                case ShapeTypeEnum.Cylinder:
                    positions = SpawnLayers(Units.Cm2M(RadiusCm), Units.Cm2M(HeightCm), Units.Cm2M(SPHSystem.RadiusCm), true, _worldSpace, _preview);
                    break;
            }
            return positions;
        }


        /// <summary>
        /// Display the emitter output in Edit Mode
        /// </summary>
        private void PreviewParticles()
        {
            Mesh m = new Mesh();
            Vector3[] verticies = SpawnParticles(false, true);

            m.vertices = verticies;
            m.SetIndices(BuildIndicies(verticies.Length), MeshTopology.Points, 0);
            meshFilter.sharedMesh = m;
            m.RecalculateBounds();
        }

        /// <remarks>
        /// See if we are in we have 
        /// a definition and only warn one time.
        /// 
        /// This will properly warn again upon entering play 
        /// mode if not corrected.
        /// </remarks>
        private bool CheckFluid()
        {
            if (!Fluid)
            {
                if (!hasBeenWarned)
                {
                    Debug.LogWarning(this + " You must supply a fluid definition.");
                    hasBeenWarned = true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Spawn a layer of particle positions.
        /// 
        /// Input is expected to be in meters.
        /// </summary>
        /// <param name="_width"></param>
        /// <param name="_length"></param>
        /// <param name="_particleSize"></param>
        /// <param name="_radialCull"></param>
        /// <returns></returns>
        private Vector3[] SpawnLayer(float _width, float _length, float _particleRadius, bool _radialCull, bool _worldSpace)
        {           
            List<Vector3> positions = new List<Vector3>();
            int widthCount;
            int lengthCount;


            if (ShapeType == ShapeTypeEnum.Plane || ShapeType == ShapeTypeEnum.Box)
            {
                widthCount = (int)(_width / (_particleRadius * 2) + Units.ONE_THOUSANDTH);
                lengthCount = (int)(_length / (_particleRadius * 2) + Units.ONE_THOUSANDTH);
            }
            else
            {
                widthCount = (int)(_width / (_particleRadius * 2));
                lengthCount = (int)(_length / (_particleRadius * 2));
            }


            for (int w = 0; w < widthCount; w++)
            {
                for (int h = 0; h < lengthCount; h++)
                {
                    float x = w * _particleRadius * 2;
                    float y = h * _particleRadius * 2;

                    /// Offset by half a particle
                    x += _particleRadius;
                    y += _particleRadius;

                    /// Offset by half of size to center particle around (0,0,0)
                    x -= (widthCount * _particleRadius);
                    y -= (lengthCount * _particleRadius); 

                    Vector3 pos = new Vector3(x, y, 0);
                    /// Disk or Tube
                    if (_radialCull)
                    {
                        if (Vector3.Distance(pos, Vector3.zero) < _width * Units.HALF)
                        {
                            positions.Add(pos);
                        }
                    }
                    else
                    {
                        positions.Add(pos);
                    }
                }
            }
            return positions.ToArray();
        }

        /// <summary>
        /// Override for Disk
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="_particleSize"></param>
        /// <param name="_radialCull"></param>
        /// <returns></returns>
        private Vector3[] SpawnLayer(float _radius, float _particleSize, bool _radialCull, bool _worldSpace)
        {
            return SpawnLayer(_radius * 2, _radius * 2, _particleSize, _radialCull, _worldSpace);
        }

        /// <summary>
        /// Spawn a volume of layers
        /// </summary>
        /// <param name="_width"></param>
        /// <param name="_length"></param>
        /// <param name="_height"></param>
        /// <param name="_particleSize"></param>
        /// <param name="_radialCull"></param>
        /// <returns></returns>
        private Vector3[] SpawnLayers(float _width, float _length, float _height, float _particleRadius, bool _radialCull, bool _worldSpace, bool _preview)
        {

            int heightCount = (int)(_height / (_particleRadius*2) + Units.ONE_THOUSANDTH) ;
            Vector3[] baseLayer = SpawnLayer(_width, _length, _particleRadius, _radialCull, _worldSpace);
            Vector3[] allLayers = new Vector3[baseLayer.Length * heightCount];
            Vector3 offsetPosition;
            int copyCounter = 0;
            for (int h = 0; h < heightCount; h++)
            {
                for (int p = 0; p < baseLayer.Length; p++)
                {
                    offsetPosition = baseLayer[p] + new Vector3(0, 0, (h * _particleRadius * 2) + _particleRadius);

                    if (_preview)
                        allLayers[copyCounter] = offsetPosition;
                    else
                        allLayers[copyCounter] = TRS.MultiplyPoint(offsetPosition);

                    copyCounter++;
                }
            }
            return allLayers;
        }

        /// <summary>
        /// Override for Cylinder
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="_height"></param>
        /// <param name="_particleSize"></param>
        /// <param name="_radialCull"></param>
        /// <returns></returns>
        private Vector3[] SpawnLayers(float _radius, float _height, float _particleRadius, bool _radialCull, bool _worldSpace, bool _preview)
        {
            return SpawnLayers(_radius * 2, _radius * 2, _height, _particleRadius, _radialCull, _worldSpace, _preview);
        }

        /// <summary>
        /// For the preview display we need a the particles as 
        /// an indice array for the mesh.
        /// </summary>
        /// <param name="_count"></param>
        /// <returns></returns>
        public int[] BuildIndicies(int _count)
        {
            int[] indicies = new int[_count];
            for (int i = 0; i < _count; i++)
            {
                indicies[i] = i;
            }
            return indicies;
        }
        #endregion
    }
}
