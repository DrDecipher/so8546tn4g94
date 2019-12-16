using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SPH3
{
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class SPH_Collider : MonoBehaviour
    {
        #region Private Variables
        private MeshFilter meshFilter;
        #endregion

        #region Public Variables
        public Matrix4x4 TRS;
        public SPH_System SPHSystem;

        public Color color = Color.blue;
        public bool Active = true;

        public ColliderTypeEnum ColliderType = ColliderTypeEnum.Box;

        /// <summary>
        /// This is used for a container having the ability to pour 
        /// liquid in and/or have it spill out.
        /// </summary>
        public bool TopIsOpen = true;

        public bool Infinite = false;

        public Vector3 Size3dCm = new Vector3(10, 10, 10);
        public Vector2 Size2dCm = new Vector2(10, 10);
        public float RadiusCm = 10;
        public float HeightCm = 100;

        public Vector3 BoundsMin;
        public Vector3 BoundsMax;
        #endregion

        #region Unity Methods
        public void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();

            if (meshFilter.sharedMesh == null)
                meshFilter.sharedMesh = new Mesh();
        }

        public void Start()
        {
            TRS = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        }

        public void Update()
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
        }

        /// <summary>
        /// Draw the collider
        /// </summary>
        public void OnDrawGizmos()
        {
            Gizmos.matrix = TRS;
            Gizmos.color = color;

            switch (ColliderType)
            {
                case ColliderTypeEnum.Plane:
                    GizmoUtilities.Instance.DrawPlane(Units.Cm2M(Size2dCm), true);
                    break;
                case ColliderTypeEnum.Box:
                    GizmoUtilities.Instance.DrawBox(Units.Cm2M(Size3dCm), true);
                    break;
                case ColliderTypeEnum.Sphere:
                    GizmoUtilities.Instance.DrawSphere(Units.Cm2M(RadiusCm), true);
                    break;
                case ColliderTypeEnum.Cylinder:
                    GizmoUtilities.Instance.DrawCylinder(Units.Cm2M(RadiusCm), Units.Cm2M(HeightCm), true);
                    break;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Bounds calculation is expanded by the radius of the largest particle, to ensure accurate detection.
        /// 
        /// This is faster than doing a sphere collision with the bounding volume.
        /// </summary>
        public void CalculateBounds()
        {
            BoundsMin = Units.Cm2M(new Vector3(-RadiusCm, -RadiusCm, -SPHSystem.RadiusCm));
            BoundsMax = Units.Cm2M(new Vector3(RadiusCm, RadiusCm, HeightCm + SPHSystem.RadiusCm));
        }
        #endregion
    }
}
