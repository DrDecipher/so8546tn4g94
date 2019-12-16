using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH3
{

    /// <summary>
    /// Component to render a provided array of 
    /// positions on the GPOU as instanced geometry.
    /// </summary>
    public class RenderFluidAsInstancesIndirect : MonoBehaviour
    {
        #region Private Variables
        private int subMaterialIndex = 0;

        private int cachedInstanceCount = -1;
        private int cachedsubMaterialIndex = -1;

        private ComputeBuffer positionBuffer; // Position
        private ComputeBuffer propertiesBuffer; // Albedo
        private ComputeBuffer argsBuffer;
        private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        #endregion

        #region Public Variables
        public Mesh instanceMesh;
        public Material instanceMaterial;
        #endregion

        #region Unity Methods
        private void Start()
        {
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        }

        /// <summary>
        /// Release all memory
        /// </summary>
        private void OnDisable()
        {
            if (positionBuffer != null)
                positionBuffer.Release();
            positionBuffer = null;

            if (propertiesBuffer != null)
                propertiesBuffer.Release();
            propertiesBuffer = null;

            if (argsBuffer != null)
                argsBuffer.Release();
            argsBuffer = null;
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Send the buffers to the GPU
        /// </summary>
        /// <param name="_particleContainer"></param>
        /// <param name="_particleCount"></param>
        public void DrawFluid(ParticleContainer _particleContainer, int _particleCount)
        {
            UpdateBuffers(_particleContainer, _particleCount);
            // Render
            Graphics.DrawMeshInstancedIndirect(instanceMesh, subMaterialIndex, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
        }

        /// <summary>
        /// Populate Buffers
        /// </summary>
        /// <param name="_particleContainer"></param>
        /// <param name="_particleCount"></param>
        private void UpdateBuffers(ParticleContainer _particleContainer, int _particleCount)
        {
            // Ensure sub-mesh index is in range
            if (instanceMesh != null)
                subMaterialIndex = Mathf.Clamp(subMaterialIndex, 0, instanceMesh.subMeshCount - 1);

            // Positions
            if (positionBuffer != null)
                positionBuffer.Release();
            // Properties
            if (propertiesBuffer != null)
                propertiesBuffer.Release();

            positionBuffer = new ComputeBuffer(_particleCount, 16);
            Vector4[] positions = new Vector4[_particleCount];

            for (int i = 0; i < _particleCount; i++)
            {
                positions[i] = new Vector4(
                    _particleContainer.PositionsNative[i].x,
                    _particleContainer.PositionsNative[i].y,
                    _particleContainer.PositionsNative[i].z,
                    /// <remarks>
                    /// w = scale not radius for the GPU
                    /// </remarks>
                    _particleContainer.Radius * 2
                    );
            }
            positionBuffer.SetData(positions);

            instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

            // Indirect args
            if (instanceMesh != null)
            {
                args[0] = (uint)instanceMesh.GetIndexCount(subMaterialIndex);
                args[1] = (uint)_particleCount;
                args[2] = (uint)instanceMesh.GetIndexStart(subMaterialIndex);
                args[3] = (uint)instanceMesh.GetBaseVertex(subMaterialIndex);
            }
            else
            {
                args[0] = args[1] = args[2] = args[3] = 0;
            }
            argsBuffer.SetData(args);

            cachedInstanceCount = _particleCount;
            cachedsubMaterialIndex = subMaterialIndex;
        }
        #endregion

    }
}
