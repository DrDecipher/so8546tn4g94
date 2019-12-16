using UnityEngine;


/// <summary>
/// Scriptable Object to define various fluid properties
/// </summary>
[CreateAssetMenu(fileName = "Fluid Definition", menuName = "SPH3/Fluid Definition", order = 1)]
public class FluidBase : ScriptableObject
{
    #region Public Variables
    /// <summary> Name of fluid </summary> 
    public string Name = "Undefined";

    /// <summary> Color of fluid </summary> 
    public Color Color = Color.blue;

    /// <summary> 
    /// Viscosity of fluid 
    /// Lower = less
    /// </summary
    public float Viscosity = 0.01f;

    /// <summary> Mass of individual fluid particle </summary>
    public float Mass = 1;

    #endregion


}
