using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SPH3
{
    [ExecuteInEditMode]
    /// <summary>
    /// A collection of MonoBehavior helper methods 
    /// </summary>
    public class UtilMono : Singleton<UtilMono>
    {

        /// Prevent non-singleton constructor use.
        protected UtilMono() { }

        #region ARRAYS
        public Vector4 AverageOfArrayValues(Vector4[] array)
        {
            Vector4 average = Vector4.zero;
            for (int i = 0; i < array.Length; i++)
                average += array[i];
            average /= array.Length;
            return average;
        }
        public Vector3 AverageOfArrayValues(Vector3[] array)
        {
            Vector3 average = Vector3.zero;
            for (int i = 0; i < array.Length; i++)
                average += array[i];
            average /= array.Length;
            return average;
        }
        public Vector2 AverageOfArrayValues(Vector2[] array)
        {
            Vector2 average = Vector2.zero;
            for (int i = 0; i < array.Length; i++)
                average += array[i];
            average /= array.Length;
            return average;
        }
        public int AverageOfArrayValues(int[] array)
        {
            int average = 0;
            for (int i = 0; i < array.Length; i++)
                average += array[i];
            average /= array.Length;
            return average;
        }
        public float AverageOfArrayValues(float[] array)
        {
            float average = 0;
            for (int i = 0; i < array.Length; i++)
                average += array[i];
            average /= array.Length;
            return average;
        }
        #endregion

        #region GEOMETRY
        public Vector3[] GenerateBoxVerts(float length, float width, float height)
        {
            Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
            Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
            Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
            Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

            Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
            Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
            Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
            Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

            Vector3[] vertices = new Vector3[]
            {
	        /// Bottom
	        p0, p1, p2, p3,
 
	        /// Left
	        p7, p4, p0, p3,
 
	        /// Front
	        p4, p5, p1, p0,
 
	        /// Back
	        p6, p7, p3, p2,
 
	        /// Right
	        p5, p6, p2, p1,
 
	        /// Top
	        p7, p6, p5, p4
            };
            return vertices;
        }
        #endregion

        #region MATERIALS
        /// <summary>
        /// Get all the materials in children
        /// </summary> 
        public Material[] CollectChildMaterials(GameObject _parent)
        {
            List<Material> materials = new List<Material>();

            Renderer[] children;
            children = _parent.GetComponentsInChildren<Renderer>();

            foreach (Renderer rend in children)
            {
                var mats = new Material[rend.sharedMaterials.Length];
                for (var j = 0; j < rend.sharedMaterials.Length; j++)
                {
                    Material mat = rend.sharedMaterials[j];
                    if (materials.Contains(mat) == false)
                        materials.Add(mat);
                }
            }
            return materials.ToArray();
        }

        public Material[] CollectChildMaterials(GameObject _parent, ref GameObject[] ourGameObjects)
        {
            // Temporary list for collecting
            List<GameObject> _gameObjects = new List<GameObject>();
            List<Material> _materials = new List<Material>();

            // Re3nderers in children
            Renderer[] children = _parent.GetComponentsInChildren<Renderer>();

            // For each child get all materials in renderer.
            // A renderer may have more than one material
            foreach (Renderer rend in children)
            {
                var mats = new Material[rend.sharedMaterials.Length];
                for (var j = 0; j < rend.sharedMaterials.Length; j++)
                {
                    Material mat = rend.sharedMaterials[j];
                    if (_materials.Contains(mat) == false)
                    {
                        _materials.Add(mat);
                        _gameObjects.Add(rend.gameObject);
                    }
                }
            }

            // Convert back to array before returning
            ourGameObjects = _gameObjects.ToArray();
            return _materials.ToArray();
        }

        /// <summary>
        /// Set color of material
        /// </summary> 
        public void SetMaterialColor(Material _mat, Color _color)
        {
            _mat.color = _color;
        }
        public void SetMaterialColor(Material[] _mats, Color _color)
        {
            for (int i = 0; i < _mats.Length; i++)
                SetMaterialColor(_mats[i], _color);
        }
        /// <summary>
        ///  Set color of Text Mesh Pro
        ///  </summary>
        public void SetTMPColor(TextMeshPro _mat, Color _color)
        {
            _mat.faceColor = _color;
        }
        public void SetTMPColor(TextMeshPro[] _mats, Color _color)
        {
            for (int i = 0; i < _mats.Length; i++)
                SetTMPColor(_mats[i], _color);
        }
        #endregion

        #region QUATERNIANS
        /// <summary>
        ///  Get an average (mean) from more than two quaternions (with two, slerp would be used).
        /// Note: this only works if all the quaternions are relatively close together.
        /// Usage:
        /// -Cumulative is an external Vector4 which holds all the added x y z and w components.
        /// -newRotation is the next rotation to be added to the average pool
        /// -firstRotation is the first quaternion of the array to be averaged
        /// -addAmount holds the total amount of quaternions which are currently added
        /// Returns the current average quaternion
        /// </summary>
        /// <param name="cumulative"></param>
        /// <param name="newRotation"></param>
        /// <param name="firstRotation"></param>
        /// <param name="addAmount"></param>
        /// <returns></returns>
        public static Quaternion AverageQuaternion(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation, int addAmount)
        {
            float w = 0.0f;
            float x = 0.0f;
            float y = 0.0f;
            float z = 0.0f;

            /// Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
            /// q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
            if (!AreQuaternionsClose(newRotation, firstRotation))
            {
                newRotation = InverseSignQuaternion(newRotation);
            }

            /// Average the values
            float addDet = 1f / (float)addAmount;
            cumulative.w += newRotation.w;
            w = cumulative.w * addDet;
            cumulative.x += newRotation.x;
            x = cumulative.x * addDet;
            cumulative.y += newRotation.y;
            y = cumulative.y * addDet;
            cumulative.z += newRotation.z;
            z = cumulative.z * addDet;

            /// Note: if speed is an issue, you can skip the normalization step
            return NormalizeQuaternion(x, y, z, w);
        }

        public static Quaternion NormalizeQuaternion(float x, float y, float z, float w)
        {
            float lengthD = 1.0f / (w * w + x * x + y * y + z * z);
            w *= lengthD;
            x *= lengthD;
            y *= lengthD;
            z *= lengthD;

            return new Quaternion(x, y, z, w);
        }

        /// <summary>
        /// Changes the sign of the quaternion components. 
        /// This is not the same as the inverse.
        /// </summary>
        public static Quaternion InverseSignQuaternion(Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, -q.w);
        }

        /// <summary>
        /// 
        /// </summary>Returns true if the two input quaternions are close to each other. 
        /// This can be used to check whether or not one of two quaternions which are supposed to
        /// be very similar but has its component signs reversed(q has the same rotation as -q)
        public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2)
        {
            float dot = Quaternion.Dot(q1, q2);

            if (dot < 0.0f)
            {
                return false;
            }

            else
            {
                return true;
            }
        }
        #endregion

        #region RAYCAST
        /// <summary>
        /// Simple Camera Ray Cast
        /// </summary>
        /// <returns></returns>
        public RaycastHit CameraRaycast()
        {
            Ray r = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            RaycastHit rch = new RaycastHit();
            Physics.Raycast(r, out rch);
            return rch;
        }
        #endregion


    }
}