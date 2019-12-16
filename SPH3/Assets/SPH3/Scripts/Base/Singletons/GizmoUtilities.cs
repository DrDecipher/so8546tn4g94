using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH3
{
    /// <summary>
    /// Gizmo drawing utility methods
    /// </summary>
    [ExecuteInEditMode]
    public class GizmoUtilities : Singleton<GizmoUtilities>
    {
        #region Vector Array Operations
        /// <summary>
        /// Vector Array Scaling
        /// </summary>
        /// <param name="_points"></param>
        /// <param name="_scale"></param>
        /// <returns></returns>
        public Vector3[] ScalePoints(Vector3[] _points, float _scale)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = _points[i] * _scale;
            }
            return _points;
        }
        public Vector3[] ScalePoints(Vector3[] _points, Vector2 _scale)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = Vector3.Scale(_points[i], _scale);
            }
            return _points;
        }
        public Vector3[] ScalePoints(Vector3[] _points, Vector3 _scale)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = Vector3.Scale(_points[i], _scale);
            }
            return _points;
        }

        /// <summary>
        /// Vector Array Offsetting
        /// </summary>
        /// <param name="_points"></param>
        /// <param name="_offset"></param>
        /// <returns></returns>
        public Vector3[] OffsetPoints(Vector3[] _points, Vector3 _offset)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                _points[i] = _points[i] + _offset;
            }
            return _points;
        }
        #endregion

        #region Shapes
        /// <summary>
        /// Four corners of a plane
        /// </summary>
        /// <returns></returns>
        private Vector3[] PlanePoints()
        {
            return new Vector3[]
            {
                new Vector3(-Units.HALF, -Units.HALF, 0),
                new Vector3(Units.HALF, -Units.HALF, 0),
                new Vector3(Units.HALF, Units.HALF, 0),
                new Vector3(-Units.HALF, Units.HALF, 0),
            };
        }

        /// <summary>
        /// Points on a circle
        /// </summary>
        /// <param name="_steps"></param>
        /// <returns></returns>
        private Vector3[] ArcPoints(int _steps, float _deg)
        {
            // Draw the circle.
            Vector3[] points = new Vector3[_steps+1];

            float dtheta = (float)(2 * Mathf.PI / _steps);
            float theta = 0;
            for (int i = 0; i < _steps+1; i++)
            {
                float x = (float)(1 * Mathf.Cos(theta));
                float y = (float)(1 * Mathf.Sin(theta));
                points[i] = new Vector3(x, y, 0);
                theta += dtheta * (_deg / 360);
            }
            return points;
        }
        #endregion

        #region Drawing
        /// <summary>
        /// Draw Axis gizmo
        /// </summary>
        /// <param name="_size"></param>
        public void DrawAxis(float _size)
        {
            /// X Axis
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(_size, 0, 0));

            /// Y Axis
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(0, _size, 0));

            /// Z Axis
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                Vector3.zero,
                new Vector3(0, 0, _size));
        }
        public void DrawAxis(float _size, Vector3 _offset)
        {
            /// X Axis
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                Vector3.zero + _offset,
                new Vector3(_size, 0, 0) + _offset);

            /// Y Axis
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                Vector3.zero + _offset,
                new Vector3(0, _size, 0) + _offset);

            /// Z Axis
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                Vector3.zero + _offset,
                new Vector3(0, 0, _size) + _offset);
        }

        /// <summary>
        /// Draws gizmo lines n to n+1
        /// </summary>
        /// <param name="_points"></param>
        /// <param name="_close"></param>
        public void DrawEdges(Vector3[] _points, bool _close)
        {
            /// Draw
            for (int i = 0; i < _points.Length - 1; i++)
                Gizmos.DrawLine(_points[i], _points[i + 1]);

            /// Close
            if (_close)
                Gizmos.DrawLine(_points[_points.Length - 1], _points[0]);
        }

        /// <summary>
        /// Draws Edges between two arrasy of corresponding points
        /// </summary>
        /// <param name="_pointsStart"></param>
        /// <param name="_pointsEnd"></param>
        public void DrawBridge(Vector3[] _pointsStart, Vector3[] _pointsEnd, int _by)
        {
            if (_pointsStart.Length == _pointsEnd.Length)
            {
                /// Draw
                for (int i = 0; i < _pointsStart.Length; i += _by)
                    Gizmos.DrawLine(_pointsStart[i], _pointsEnd[i]);
            }
            else
            {
                Debug.LogError("DrawBridge requires two arrays of the same length");
            }
        }

        /// <summary>
        /// Draw a plane gizmo edges
        /// </summary>
        /// <param name="_size"></param>
        /// <param name="_drawAxis"></param>
        public void DrawPlane(Vector2 _size, bool _drawAxis)
        {
            Vector3[] points = PlanePoints();
            points = ScalePoints(points, _size);
            DrawEdges(points, true);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_size.x + _size.y) * Units.ONE_TENTH);
        }

        /// <summary>
        /// Draw a plane gizmo edges
        /// </summary>
        /// <param name="_size"></param>
        /// <param name="_drawAxis"></param>
        public void DrawPlane(Vector2 _size, bool _drawAxis, Vector3 _offset)
        {
            Vector3[] points = PlanePoints();
            points = ScalePoints(points, _size);
            points = OffsetPoints(points, _offset);
            DrawEdges(points, true);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_size.x + _size.y) * Units.ONE_TENTH);
        }

        /// <summary>
        /// Draw a gizmo circle
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="_steps"></param>
        /// <param name="_drawAxis"></param>
        public void DrawCircle(float _radius, int _steps, bool _drawAxis)
        {
            Vector3[] points = ArcPoints(_steps, 360);

            points = ScalePoints(points, _radius);
            DrawEdges(points, false);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_radius + _radius + _radius) * Units.ONE_TENTH);
        }

        /// <summary>
        /// Draw a gizmo circle
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="_steps"></param>
        /// <param name="_drawAxis"></param>
        /// <param name="_offset"></param>
        public void DrawCircle(float _radius, int _steps, bool _drawAxis, Vector3 _offset)
        {
            Vector3[] points = ArcPoints(_steps, 360);
            points = ScalePoints(points, _radius);
            points = OffsetPoints(points, _offset);
            DrawEdges(points, false);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_radius + _radius + _radius) * Units.ONE_TENTH, _offset);
        }

        /// <summary>
        /// Draws and Ellipse
        /// </summary>
        /// <param name="_radius2"></param>
        /// <param name="_steps"></param>
        /// <param name="_drawAxis"></param>
        public void DrawEllipse(Vector2 _radius2, int _steps, bool _drawAxis)
        {
            Vector3[] points = ArcPoints(_steps, 270);
            points = ScalePoints(points, _radius2);
            DrawEdges(points, false);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_radius2.x + _radius2.y) * Units.ONE_TENTH);
        }
        public void DrawEllipse(Vector2 _radius2, int _steps, bool _drawAxis, Vector3 _offset)
        {
            Vector3[] points = ArcPoints(_steps, 270);
            points = ScalePoints(points, _radius2);
            points = OffsetPoints(points, _offset);
            DrawEdges(points, false);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_radius2.x + _radius2.y) * Units.ONE_TENTH, _offset);
        }

        /// <summary>
        /// Draws a Box gizmo with the axis at the center base
        /// </summary>
        /// <param name="_size"></param>
        /// <param name="_drawAxis"></param>
        public void DrawBox(Vector3 _size, bool _drawAxis)
        {
            Vector3[] pointsBase = PlanePoints();
            pointsBase = ScalePoints(pointsBase, _size);

            Vector3[] pointsTop = PlanePoints();
            pointsTop = ScalePoints(pointsTop, _size);
            pointsTop = OffsetPoints(pointsTop, new Vector3(0, 0, _size.z));

            DrawEdges(pointsBase, true);
            DrawEdges(pointsTop, true);
            DrawBridge(pointsBase, pointsTop, 1);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_size.x + _size.y + _size.z) * Units.ONE_TENTH);
        }

        /// <summary>
        /// Draws a Cylinder Gizmo with the pivot at the base
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="height"></param>
        /// <param name="_drawAxis"></param>
        public void DrawCylinder(float _radius, float height, bool _drawAxis)
        {
            Vector3[] pointsBase = ArcPoints(16, 360);
            pointsBase = ScalePoints(pointsBase, _radius);

            Vector3[] pointsTop = ArcPoints(16, 360);
            pointsTop = ScalePoints(pointsTop, _radius);
            pointsTop = OffsetPoints(pointsTop, new Vector3(0, 0, height));

            DrawEdges(pointsBase, true);
            DrawEdges(pointsTop, true);
            DrawBridge(pointsBase, pointsTop, 4);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_radius + _radius + _radius) * Units.ONE_TENTH);
        }

        /// <summary>
        /// Draw a sphere circle
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="_drawAxis"></param>
        public void DrawSphere(float _radius, bool _drawAxis)
        {
            Gizmos.DrawWireSphere(Vector3.zero, _radius);

            /// Draw Axis
            if (_drawAxis)
                DrawAxis((_radius + _radius + _radius) * Units.ONE_TENTH);
        }
        #endregion
    }
}
