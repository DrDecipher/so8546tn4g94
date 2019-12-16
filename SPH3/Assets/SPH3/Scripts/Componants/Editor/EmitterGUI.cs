using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace SPH3
{
    [CustomEditor(typeof(SPH_Emitter))]
    public class EmitterGUI : Editor
    {
        #region Private Variables
        /// <summary>
        /// Self Access Handle
        /// </summary>
        private SPH_Emitter _target;

        /// <summary>
        /// Fluid properties scriptable object
        /// </summary>
        private SerializedProperty m_FluidProperties;
        private SerializedProperty m_color;

        /// <summary>
        /// Time Foldout Properties
        /// </summary>
        private SavedBool m_ShowTime;
        private SerializedProperty m_StartTime;
        private SerializedProperty m_StopTime;
        private SerializedProperty m_Active;
        private SerializedProperty m_Trigger;

        /// <summary>
        /// Emission Foldout Properties
        /// </summary>
        protected SavedBool m_ShowEmission;
        private SerializedProperty m_ShapeType;
        private SerializedProperty m_Size3dCm;
        private SerializedProperty m_Size2dCm;
        private SerializedProperty m_RadiusCm;
        private SerializedProperty m_HeightCm;
        private SerializedProperty m_VelocityMps;
        private SerializedProperty m_Visualize;
        #endregion

        #region Unity Methods
        void OnEnable()
        {
            /// <remarks>Get Target Self</remarks>
            _target = (SPH_Emitter)target;

            m_FluidProperties = serializedObject.FindProperty("Fluid");
            m_color = serializedObject.FindProperty("color");
       
            /// <remarks>
            /// Get Time Property Handles
            /// </remarks>
            m_ShowTime = new SavedBool($"{target.GetType()}.ShowTime", true);
            m_StartTime = serializedObject.FindProperty("StartTime");
            m_StopTime = serializedObject.FindProperty("StopTime");
            m_Active = serializedObject.FindProperty("Active");
            m_Trigger = serializedObject.FindProperty("Trigger");

            /// <remarks>
            /// Get Emission Property Handles
            /// </remarks>
            m_ShowEmission = new SavedBool($"{target.GetType()}.ShowEmission", true);
            m_Size3dCm = serializedObject.FindProperty("Size3dCm");
            m_Size2dCm = serializedObject.FindProperty("Size2dCm");
            m_RadiusCm = serializedObject.FindProperty("RadiusCm");
            m_HeightCm = serializedObject.FindProperty("HeightCm");
            m_VelocityMps = serializedObject.FindProperty("VelocityMps");
            m_Visualize = serializedObject.FindProperty("Visualize");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            /// <remarks>
            /// Fluid Properties
            /// </remarks>
            EditorGUILayout.PropertyField(m_FluidProperties);
            _target.Color = EditorGUILayout.ColorField("Gizmo: ", _target.Color);


            ///  <remarks>
            ///  Foldout (Emission)
            ///  </remarks>           
            m_ShowTime.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowTime.value, Styles.Time);
            if (m_ShowTime.value)
            {
                DoTimeGUI();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            ///  <remarks>
            ///  Foldout (Shape)
            ///  </remarks>  
            m_ShowEmission.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShowEmission.value, Styles.Emission);
            if (m_ShowEmission.value)
            {                
                DoEmissionGUI();
            }
            /// <remarks>
            ///  Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            ///  </remarks>
            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// GUI Emission
        /// </summary>
        void DoTimeGUI()
        {         
            EditorGUI.indentLevel++;

            /// <remarks>
            /// Time Drop-down
            /// </remarks>
            _target.TimeType = (TimeTypeEnum)EditorGUILayout.EnumPopup("Time", _target.TimeType);

            switch (_target.TimeType)
            {
                /// <remarks>
                /// Draw Range
                /// </remarks>
                case TimeTypeEnum.Range:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_StartTime, new GUIContent("Start Time:"));
                    EditorGUILayout.LabelField(new GUIContent("Seconds"));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_StopTime, new GUIContent("Stop Time:"));
                    EditorGUILayout.LabelField(new GUIContent("Seconds"));
                    EditorGUILayout.EndHorizontal();
                    break;

                /// <remarks>
                /// Draw Animate
                /// </remarks>
                case TimeTypeEnum.Animate:
                    EditorGUILayout.PropertyField(m_Active);
                    break;

                /// <remarks>
                /// Draw Trigger
                /// </remarks>
                case TimeTypeEnum.Trigger:
                    EditorGUILayout.PropertyField(m_Trigger);
                    break;
            }

            EditorGUI.indentLevel--;
        }

        void DoEmissionGUI()
        {
            /// <remarks>
            /// Shape Drop-down
            /// </remarks>
            
            _target.ShapeType = (ShapeTypeEnum)EditorGUILayout.EnumPopup("Shape", _target.ShapeType);

            switch (_target.ShapeType)
            {
                /// <remarks>
                /// Draw Disk
                /// </remarks>
                case ShapeTypeEnum.Disk:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_RadiusCm, new GUIContent("Radius:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_VelocityMps, new GUIContent("Velocity:"));
                    EditorGUILayout.LabelField(new GUIContent("Meters p/s"));
                    EditorGUILayout.EndHorizontal();                  
                    break;

                /// <remarks>
                /// Draw Plane
                /// </remarks>
                case ShapeTypeEnum.Plane:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_Size2dCm, new GUIContent("Size:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"), GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_VelocityMps, new GUIContent("Velocity:"));
                    EditorGUILayout.LabelField(new GUIContent("Meters p/s"));
                    EditorGUILayout.EndHorizontal();
                    break;

                /// <remarks>
                /// Draw Sphere
                /// </remarks>
                case ShapeTypeEnum.Cylinder:
                    if (_target.TimeType != TimeTypeEnum.Trigger)
                    {

                        Debug.LogWarning("Sphere emitter may only use a triggered emission.");
                        _target.TimeType = TimeTypeEnum.Trigger;
                    }
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_RadiusCm, new GUIContent("Radius:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"), GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_HeightCm, new GUIContent("Height:"));
                    EditorGUILayout.LabelField(new GUIContent("Cm"), GUILayout.MaxWidth(40));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_VelocityMps, new GUIContent("Velocity:"));
                    EditorGUILayout.LabelField(new GUIContent("Meters p/s"));
                    EditorGUILayout.EndHorizontal();
                    break;

                /// <remarks>
                /// Draw Box
                /// </remarks>
                case ShapeTypeEnum.Box:
                    if (_target.TimeType != TimeTypeEnum.Trigger)
                    {
                        Debug.LogWarning("Box emitter may only use a triggered emission.");
                        _target.TimeType = TimeTypeEnum.Trigger;
                    }

                    EditorGUILayout.PropertyField(m_Size3dCm, new GUIContent("Size:"));

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(m_VelocityMps, new GUIContent("Velocity:"));
                    EditorGUILayout.LabelField(new GUIContent("Meters p/s"));
                    EditorGUILayout.EndHorizontal();
                    break;


            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_Visualize, new GUIContent("Visualize:"));
            EditorGUILayout.EndHorizontal();
            
        }
        #endregion
    }
}
