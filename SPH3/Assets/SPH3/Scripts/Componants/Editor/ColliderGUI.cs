using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace SPH3
{
    [CustomEditor(typeof(SPH_Collider))]
    public class ColliderGUI : Editor
    {
        #region Private Variables
        /// <summary>
        /// Self Access Handle
        /// </summary>
        private SPH_Collider _target;

        private SerializedProperty m_color;
        private SerializedProperty m_Active;

        /// <summary>
        /// Emission Foldout Properties
        /// </summary>
        private SavedBool m_ShowProperties;

        private SerializedProperty m_TopIsOpen;
        private SerializedProperty m_Infinite;

        private SerializedProperty m_Size3dCm;
        private SerializedProperty m_Size2dCm;
        private SerializedProperty m_RadiusCm;
        private SerializedProperty m_HeightCm;
        #endregion

        #region Unity Methods
        void OnEnable()
        {
            /// <remarks>Get Target Self</remarks>
            _target = (SPH_Collider)target;

            /// <remarks>
            /// Get Property Handles
            /// </remarks>
            m_color = serializedObject.FindProperty("color");
            m_Active = serializedObject.FindProperty("Active");

            m_ShowProperties = new SavedBool($"{target.GetType()}.m_ShowProperties", true);

            m_TopIsOpen = serializedObject.FindProperty("TopIsOpen");
            m_Infinite = serializedObject.FindProperty("Infinite");

            m_Size3dCm = serializedObject.FindProperty("Size3dCm");
            m_Size2dCm = serializedObject.FindProperty("Size2dCm");
            m_RadiusCm = serializedObject.FindProperty("RadiusCm");
            m_HeightCm = serializedObject.FindProperty("HeightCm");
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            /// <remarks>
            /// Emitter Properties
            /// </remarks>
            _target.color = EditorGUILayout.ColorField("Gizmo:", _target.color);
            EditorGUILayout.PropertyField(m_Active);


            ///  <remarks>
            ///  Foldout (Properties)
            ///  </remarks>           
            m_ShowProperties.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowProperties.value, Styles.Time);
            if (m_ShowProperties.value)
            {
                DoPropertiesGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            /// <remarks>
            ///  Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            ///  </remarks>
            serializedObject.ApplyModifiedProperties();
            if (GUI.changed)
            {
                _target.CalculateBounds();
                EditorUtility.SetDirty(target);
            }
       
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// GUI Emission
        /// </summary>
        void DoPropertiesGUI()
        {
            /// <remarks>
            /// Collider Type Drop-down
            /// </remarks>
            _target.ColliderType = (ColliderTypeEnum)EditorGUILayout.EnumPopup("Shape", _target.ColliderType);

            switch (_target.ColliderType)
            {
                /// <remarks>
                /// Draw Plane
                /// </remarks>
                case ColliderTypeEnum.Plane:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_Size2dCm, new GUIContent("Size:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"), GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(m_Infinite, new GUIContent("Is Infinite:"));
                    break;

                /// <remarks>
                /// Draw Box
                /// </remarks>
                case ColliderTypeEnum.Box:                  
                    EditorGUILayout.PropertyField(m_Size3dCm, new GUIContent("Size:"));
                    EditorGUILayout.PropertyField(m_TopIsOpen, new GUIContent("Top is open:"));


                    break;

                /// <remarks>
                /// Draw Sphere
                /// </remarks>
                case ColliderTypeEnum.Sphere:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_RadiusCm, new GUIContent("Radius:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"), GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();
                    break;

                /// <remarks>
                /// Draw Cylinder
                /// </remarks>
                case ColliderTypeEnum.Cylinder:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_RadiusCm, new GUIContent("Radius:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"), GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_HeightCm, new GUIContent("Height:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"), GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.PropertyField(m_TopIsOpen, new GUIContent("Top is open:"));
                    break;
            }
        }
        #endregion
    }
}