using UnityEngine;

namespace TacticalIntuition.Core
{
    /// <summary>
    /// Supplies the wind field. Implemented by WindField (FlightDynamics);
    /// configured by WeatherSystem (Environment); sampled by aerodynamics.
    /// </summary>
    public interface IWindProvider
    {
        /// <summary>World-frame wind velocity in m/s at a world position and sim time.</summary>
        Vector3 GetWind(Vector3 worldPosition, float time);
    }
}
