using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH3
{
    /// <summary>
    /// DEBUG - Editor collision code test script
    /// </summary>
    [ExecuteInEditMode]
    public class CollisionTester : MonoBehaviour
    {
        #region Public Variables
        public SPH_System System;
        public float ParticleRadiusCm = 10;
        
        public GameObject TestPoint;
        public GameObject ClosestPoint;
        public GameObject NewParticlePosition;

        public SPH_Collider SPHCollider;

        public Vector4 ClosesPoint;
        public Vector3 NewParticlePoint;
        #endregion

        #region Unity Methods
        private void Update()
        {
            if (TestPoint && SPHCollider && NewParticlePosition && System)
            {
                ParticleRadiusCm = System.RadiusCm;
                NewParticlePosition.transform.localScale = Units.Cm2M(Vector3.one * ParticleRadiusCm * 2);


                Vector3 tranPosition = SPHCollider.TRS.inverse.MultiplyPoint(transform.position);


                switch (SPHCollider.ColliderType)
                {
                    case ColliderTypeEnum.Plane:
                        PlaneCollide.Collide(SPHCollider.TRS, transform.position, Units.Cm2M(SPHCollider.Size2dCm), Units.Cm2M(ParticleRadiusCm), ref ClosesPoint, ref NewParticlePoint);
                        ClosestPoint.transform.position = SPHCollider.TRS.MultiplyPoint(ClosesPoint);

                        if (Mathf.Abs(ClosesPoint.w) < Units.Cm2M(ParticleRadiusCm))
                            NewParticlePosition.transform.position = SPHCollider.TRS.MultiplyPoint(NewParticlePoint);
                        else
                            NewParticlePosition.transform.position = transform.position;

                        break;

                    case ColliderTypeEnum.Box:

                        break;

                    case ColliderTypeEnum.Sphere:
                        break;

                    case ColliderTypeEnum.Cylinder:
                        CylinderCollide.CylinderCappedClosestPoint(tranPosition, Units.Cm2M(SPHCollider.RadiusCm), Units.Cm2M(SPHCollider.HeightCm), Units.Cm2M(ParticleRadiusCm), ref ClosesPoint, ref NewParticlePoint);
                        ClosestPoint.transform.position = SPHCollider.TRS.MultiplyPoint(ClosesPoint);

                        if (Mathf.Abs(ClosesPoint.w) < Units.Cm2M(ParticleRadiusCm))
                        {
                            NewParticlePosition.transform.position = SPHCollider.TRS.MultiplyPoint(new Vector3(NewParticlePoint.x, NewParticlePoint.y, NewParticlePoint.z));
                        }
                        else
                        {
                            NewParticlePosition.transform.position = transform.position;
                        }
                        break;
                }
            }
        }
        #endregion
    }
}