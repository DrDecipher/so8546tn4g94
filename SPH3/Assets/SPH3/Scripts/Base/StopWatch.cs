using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SPH3
{
    /// <summary>
    /// Stop watch using System Time
    /// 
    /// Returns elapsed time in milliseconds
    /// </summary>
    public class StopWatch
    {
        public long StartTime;
        public long EndTime;
        public float Elapsed;

        public void Start()
        {
            StartTime = DateTime.Now.Ticks;
        }

        public float Check()
        {
            EndTime = DateTime.Now.Ticks;
            Elapsed = EndTime - StartTime;
            return (Elapsed / TimeSpan.TicksPerMillisecond);
        }
    }
}
