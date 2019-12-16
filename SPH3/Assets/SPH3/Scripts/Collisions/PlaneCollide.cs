using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH3
{
    public class PlaneCollide
    {
        /// <summary>
        /// I scoured the Internet an all queries gave either a signed distance w/o closest point or were designed for 
        /// ray intersects. None appear to be lest math than the following solution I've devised for this specialized purpose
        /// that takes into account the particles radius.
        /// 
        /// There were a few sphere plane collision options, but the math was heavier than compensating for the radius 
        /// after the fact with a simple vector offset.
        /// </summary>
        /// <param name="_point"></param>
        /// <param name="_size"></param>
        /// <param name="_particleRadius"></param>
        /// <param name="_closestPoint"></param>
        /// <param name="_validPosition"></param>
        public static void Collide(Matrix4x4 TRS, Vector3 _point, Vector2 _size, float _particleRadius, ref Vector4 _closestPoint, ref Vector3 _validPosition)
        {
            _point = TRS.inverse.MultiplyPoint(_point);
            Vector2 min = -_size * Units.HALF;
            Vector2 max = _size * Units.HALF;

            bool insideX = min.x < _point.x && _point.x < max.x;
            bool insideY = min.y < _point.y && _point.y < max.y;

            bool pointInsideRectangle = insideX && insideY;

            Vector3 closestPoint3 = Vector3.zero;

            if (!pointInsideRectangle)
            /// Outside
            {
                /// <remarks> 
                /// If we are outside then the closest point is 
                /// clamped to the outer edges of the rectangle.
                /// </remarks>
                closestPoint3.x = Mathf.Max(min.x, Mathf.Min(_point.x, max.x));
                closestPoint3.y = Mathf.Max(min.y, Mathf.Min(_point.y, max.y));
            }
            else
            /// Inside
            {
                /// <remarks>
                /// If we are inside the closest point is always given x, given y, z=0 
                /// </remarks>
                closestPoint3.x = _point.x;
                closestPoint3.y = _point.y;
            }

            /// Hand-off to V4 for return value
            Vector4 closestPoint4 = closestPoint3;

            /// Apply sign
            if (_point.z > 0)
                closestPoint4.w = Vector3.Distance(_point, closestPoint3);
            else
                closestPoint4.w = -Vector3.Distance(_point, closestPoint3);

            _closestPoint = closestPoint4;

            /// Return valid position with particle radius offset. 
            if (Mathf.Abs(_closestPoint.w) < _particleRadius)
                _validPosition = TRS.MultiplyPoint(
                    closestPoint3 + ((_point - closestPoint3).normalized * _particleRadius));
            else
                _validPosition = TRS.MultiplyPoint(_point);
        }
    }
}
