using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH3
{
    /// <summary>
    /// Custom class to find the closes point on a Capped Cylinder
    /// Returns a Vector4: x, y, z, w = signed distance
    /// </summary>
    public class CylinderCollide
    {
        #region Private Variables
        private static float signedDistance;
        private static Vector3 closestPointPos;
        
        private static Vector3 pointXY;
        private static Vector3 radialOffset;

        private static Vector4 TopCP;
        private static Vector3 TopParticle;

        private static Vector4 BottomCP;
        private static Vector3 BottomParticle;

        private static Vector4 RoundCP;
        private static Vector3 RoundParticle;
        #endregion

        #region Public Methods
        /// <summary>
        /// Find the closest point and new valid 
        /// particle position based on it's radius.
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_radius"></param>
        /// <param name="_height"></param>
        /// <param name="_particleRadius"></param>
        /// <param name="_closestPoint"></param>
        /// <param name="_particlePosition"></param>
        public static void CylinderCappedClosestPoint
        (
            Vector3 _point,
            float _radius,
            float _height,
            float _particleRadius,
            ref Vector4 _closestPoint,
            ref Vector3 _particlePosition
        )
        {

            /// Below
            if (_point.z < 0)
            {
                ///Status = "Below Base";
                ClosestPointBase(_point, _radius, _height, _particleRadius, ref BottomCP, ref BottomParticle);
                _closestPoint = BottomCP;
                _particlePosition = BottomParticle;
                return;
            }
            /// Above
            else if (_point.z > _height)
            {
                ///Status = "Above Top";
                ClosestPointCap(_point, _radius, _height, _particleRadius, ref TopCP, ref TopParticle);
                _closestPoint = TopCP;
                _particlePosition = TopParticle;
                return;
            }
            /// <remarks>
            /// If the point's distance on z from and end is greater than 
            /// the radius of the cylinder the point must be closest to the round.
            /// </remarks>
            else if (_point.z > _radius && _point.z < _height - _radius)
            {
                ///Status = "Closest point to round.";
                ClosestPointRound(_point, _radius, _height, _particleRadius, ref RoundCP, ref RoundParticle);
                _closestPoint = RoundCP;
                _particlePosition = RoundParticle;
                return;
            }
            /// <remarks>
            /// If we have made it this far the point may be close to the top or bottom and the round
            /// </remarks>
            else if (_point.z < _height * Units.HALF)
            {
                /// Bottom or Round
                ClosestPointRound(_point, _radius, _height, _particleRadius, ref RoundCP, ref RoundParticle);
                ClosestPointBase(_point, _radius, _height, _particleRadius, ref BottomCP, ref BottomParticle);
                BottomCP.w *= -1;
                if (Mathf.Abs(BottomCP.w) < Mathf.Abs(RoundCP.w))
                {
                    _closestPoint = BottomCP;
                    _particlePosition = BottomParticle;
                    if (_closestPoint.w < 0)
                        InforceCornersTopBottom(_radius, _particleRadius, _closestPoint, ref _particlePosition);
                }
                else
                {
                    _closestPoint = RoundCP;
                    _particlePosition = RoundParticle;

                    /// We are on the inside
                    if (_closestPoint.w < 0)
                        InforceCornersRound(_height, _particleRadius, _closestPoint, ref _particlePosition);
                }
            }
            else
            {
                /// Top or Round
                ClosestPointRound(_point, _radius, _height, _particleRadius, ref RoundCP, ref RoundParticle);
                ClosestPointCap(_point, _radius, _height, _particleRadius, ref TopCP, ref TopParticle);
                TopCP.w *= -1;

                if (Mathf.Abs(TopCP.w) < Mathf.Abs(RoundCP.w))
                {
                    _closestPoint = TopCP;
                    _particlePosition = TopParticle;
                    if (_closestPoint.w < 0)
                        InforceCornersTopBottom(_radius, _particleRadius, _closestPoint, ref _particlePosition);
                }
                else
                {
                    _closestPoint = RoundCP;
                    _particlePosition = RoundParticle;

                    /// We are on the inside
                    if (_closestPoint.w < 0)
                        InforceCornersRound(_height, _particleRadius, _closestPoint, ref _particlePosition);
                }
            }
        }

        public static void ClosestPointTopBottom
        (
            Vector3 _point, 
            float _radius, 
            float _height, 
            float _particleRadius, 
            float z,
            ref Vector4 _closestPoint,
            ref Vector3 _particlePosition
        )
            
        {
            pointXY = new Vector2(_point.x, _point.y);

            /// <remarks>
            /// If we are within the radius we simply move to the z plane.
            /// </remarks>
            if (pointXY.magnitude < _radius)
            {
                /// Bring point to z = 0
                closestPointPos = new Vector3(_point.x, _point.y, z);            
            }
            /// <remarks>
            /// Outside the radius the closest point is the edge of the cylinder
            /// </remarks>
            else
            {
                /// Bring point to max radius
                radialOffset = pointXY.normalized * _radius;

                /// Bring point to z = 0
                closestPointPos = new Vector3(radialOffset.x, radialOffset.y, z);
            }
            _closestPoint = new Vector4(closestPointPos.x, closestPointPos.y, closestPointPos.z, Vector3.Distance(_point, closestPointPos));

            /// Offset the point in the direction of the closest point by it's radius
            Vector3 closestVector = (_point - closestPointPos).normalized * _particleRadius;
            _particlePosition = closestPointPos + closestVector;           
        }

        /// <summary>
        /// Closest point to the to the cap of the cylinder
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_radius"></param>
        /// <param name="_height"></param>
        /// <returns>
        /// Vector4: x, y, z, w = signed distance
        /// </returns>
        public static void ClosestPointBase
        (
            Vector3 _point, 
            float _radius, 
            float _height, 
            float _particleRadius,
            ref Vector4 _closestPoint,
            ref Vector3 _particlePosition
        )
        {
            ClosestPointTopBottom(_point, _radius, _height, _particleRadius, 0, ref _closestPoint, ref _particlePosition);
        }

        /// <summary>
        /// Closest point to the base of the cylinder
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_radius"></param>
        /// <param name="_height"></param>
        /// <returns>
        /// Vector4: x, y, z, w = signed distance
        /// </returns>
        public static void ClosestPointCap
        (
            Vector3 _point,
            float _radius,
            float _height,
            float _particleRadius,
            ref Vector4 _closestPoint,
            ref Vector3 _particlePosition
        )
        {
            ClosestPointTopBottom(_point, _radius, _height, _particleRadius, _height, ref _closestPoint, ref _particlePosition);
        }

        /// <summary>
        /// Finds the closest point to the round of the cylinder
        /// Since we have to calculate the direction to the center we 
        /// get the particle offset for very little cost.
        /// 
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_radius"></param>
        /// <param name="_height"></param>
        /// <param name="_particleRadius"></param>
        /// <param name="_closestpoint"></param>
        /// <param name="_particlePosition"></param>
        /// <returns>
        /// Return are by reference:
        /// _closestpoint V4 w/ .w being signed distance
        /// _particlePosition V3 includes the particle offset radius
        /// </returns>
        public static void ClosestPointRound(
            Vector3 _point, 
            float _radius, 
            float _height, 
            float _particleRadius,
            ref Vector4 _closestpoint,
            ref Vector3 _particlePosition
            )
        {
            /// Closest point in 2D
            pointXY = new Vector3(_point.x, _point.y, 0);
            closestPointPos = pointXY.normalized * _radius;

            /// Vertical Offset
            closestPointPos = new Vector3(closestPointPos.x, closestPointPos.y, _point.z);

            if (pointXY.magnitude < _radius)
            {         
                signedDistance = -Vector3.Distance(_point, _closestpoint);

                /// Offset of particle radius
                _particlePosition = closestPointPos - (pointXY.normalized * _particleRadius);
            }
            else
            {
                signedDistance = Vector3.Distance(_point, closestPointPos);

                /// Offset of particle radius
                _particlePosition = closestPointPos + (pointXY.normalized * _particleRadius);
            }
            _closestpoint = new Vector4(closestPointPos.x, closestPointPos.y, closestPointPos.z, signedDistance);
        }

        /// <summary>
        /// Special case of corner enforcement if closest point is to the top/bottom
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="_particleRadius"></param>
        /// <param name="_closestPoint"></param>
        /// <param name="_particlePosition"></param>
        public static void InforceCornersTopBottom(float _radius, float _particleRadius, Vector4 _closestPoint, ref Vector3 _particlePosition)
        {
            pointXY = new Vector3(_closestPoint.x, _closestPoint.y, 0);

            if (pointXY.magnitude > _radius - _particleRadius)
            {
                pointXY = new Vector3(_particlePosition.x, _particlePosition.y, 0);
                pointXY = pointXY.normalized * (_radius - _particleRadius);

                _particlePosition.x = pointXY.x;
                _particlePosition.y = pointXY.y;
            }

        }

        /// <summary>
        /// Special case of corner enforcement if closest point is to the round of the cylinder
        /// </summary>
        /// <param name="_height"></param>
        /// <param name="_particleRadius"></param>
        /// <param name="_closestPoint"></param>
        /// <param name="_particlePosition"></param>
        public static void InforceCornersRound(float _height, float _particleRadius, Vector4 _closestPoint, ref Vector3 _particlePosition )
        {
            if (_closestPoint.z > _height - _particleRadius)
                _particlePosition.z = _height - _particleRadius;
            else if (_closestPoint.z < _particleRadius)
                _particlePosition.z = _particleRadius;     
        }
        #endregion
    }

}
