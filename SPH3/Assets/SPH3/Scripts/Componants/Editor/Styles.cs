using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SPH3
{
    /// <summary>
    /// Styles for drawing foldouts
    /// </summary>
    class Styles : Editor
    {
        public static readonly GUIContent Time = EditorGUIUtility.TrTextContent("Time");
        public static readonly GUIContent Emission = EditorGUIUtility.TrTextContent("Emission");
    }
}