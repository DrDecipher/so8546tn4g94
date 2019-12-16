using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SPH3
{
    /// <summary>
    /// GUI Utilities - Custom Editors
    /// </summary>
    public class GUIUtilities : Singleton<GUIUtilities>
    {
        /// <summary>
        /// Prevent Scaling of Emitter
        /// </summary>
        public void LockScale(Transform _transform)
        {
            _transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Prevent Negative Numbers in the Size Input
        /// </summary>     
        public void EnforcePositiveSize(ref float Size)
        {
            if (Size < Units.MIN_SIZE)
            {
                Debug.LogWarning(this + " !!!Emitter may not have negative values!!!");
                Size = Units.MIN_SIZE;
            }
        }
        public void EnforcePositiveSize(ref Vector2 Size)
        {
            EnforcePositiveSize(ref Size.x);
            EnforcePositiveSize(ref Size.y);
        }
        public void EnforcePositiveSize(ref Vector3 Size)
        {
            EnforcePositiveSize(ref Size.x);
            EnforcePositiveSize(ref Size.y);
            EnforcePositiveSize(ref Size.z);
        }
    }
}