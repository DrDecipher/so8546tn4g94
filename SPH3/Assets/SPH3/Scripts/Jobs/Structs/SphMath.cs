using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Job Related
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

using Extensions.NativeCollections;


namespace SPH3
{
    /// <summary>
    /// These kernels are based on the paper from, Matthias Muller in 2003
    /// https://matthias-research.github.io/pages/publications/sca03.pdf
    /// 
    /// I have updated them to newer variants as well as optimized/reduced some of the math 
    /// by normalizing to the smoothing kernel.
    /// </summary>
    [BurstCompile]
    public struct SphMath
    {

        public float Density(Vector3 _pos, Vector3[] _neighbors, float _h)
        {
            float p = 0;

            for (int i = 0; i < _neighbors.Length; i++)
                p += Wpoly6(math.distance(_pos, _neighbors[i]), _h);

            return p;

        }

        //  Design for Particle-Based Fluid Simulation for Interactive Applications
        //  http://matthias-mueller-fischer.ch/publications/sca03.pdf (20)
        //  The Poly6 kernel is used for density estimation
        public float Wpoly6(float _dist, float _h)
        {
            if (_dist >= _h)
                return 0;

            double left = 315.0f / (64.0f * math.PI * math.pow(_h, 9));
            double right = math.pow((_h * _h) - (_dist * _dist), 3);

            return (float)(left * right);
        }

        /// <summary>
        /// This is a optimized version of the above code where the 
        /// distance has been normalized by h to 1.
        /// 
        /// </summary>
        /// <param name="_dist"></param>
        /// <returns></returns>
        public float Wpoly6opti(float _dist)
        {
            if (_dist >= 1)
                return 0;

            double left = 315.0f / (64.0f * math.PI);
            double right = math.pow(1 - (_dist * _dist), 3);

            return (float)(left * right);
        }

        /// <summary>
        /// For gradient calculation we use the following Spiky Kernel
        /// 
        /// This is an updated version of the kernel from 2014.
        /// </summary>
        /// <param name="_Pi"></param>
        /// <param name="_Pj"></param>
        /// <param name="_h"></param>
        /// <returns></returns>
        public Vector3 GradWspiky(Vector3 _Pi, Vector3 _Pj, float _h)
        {
            Vector3 r = _Pi - _Pj;
            float rLen = r.magnitude;

            if (rLen > _h || rLen < 0.0001f)
            {
                return Vector3.zero;
            }

            double left = 45.0f / math.pow(math.PI * _h, 6);
            double right = math.pow(1 - rLen, 2);

            return r * (float)(left * right);
        }

    }
}
