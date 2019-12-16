using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH3
{

    public class Units
    {
        /// <summary>
        /// The static variablesa are used to emulate C++ style constants
        /// </summary>
        #region Constants
        public static float cm = 0.01f;
        public static float ONE_THIRD = 0.333333f;
        public static float HALF = 0.5f;
        public static float ONE_TENTH = 0.1f;
        public static float ONE_THOUSANDTH = 0.001f;
        public static float EPSILON = 0.0001f;
        public static float MIN_SIZE = 0.1f;

        public static Vector2 V2_TENTH = new Vector3(0.1f, 0.1f, 0.1f);
        public static Vector2 V2_THOUSANDTH = new Vector3(0.01f, 0.01f, 0.01f);
        public static Vector3 V3_TENTH = new Vector3(0.1f, 0.1f, 0.1f);
        public static Vector3 V3_THOUSANDTH = new Vector3(0.01f, 0.01f, 0.01f);
        #endregion


        #region Conversions
        public static float Cm2M(float _f)
        {
            return _f * Units.cm;
        }
        public static Vector2 Cm2M(Vector2 _v2)
        {
            return _v2 * Units.cm;
        }
        public static Vector3 Cm2M(Vector3 _v3)
        {
            return _v3 * Units.cm;
        }

        public static float M2Cm(float _f)
        {
            return _f / Units.cm;
        }
        public static Vector2 M2Cm(Vector2 _v2)
        {
            return _v2 / Units.cm;
        }
        public static Vector3 M2Cm(Vector3 _v3)
        {
            return _v3 / Units.cm;
        }
        #endregion

    }
}
