namespace TacticalIntuition.Core
{
    /// <summary>
    /// Minimal contract for simulated sensors. Concrete sensors expose their own
    /// typed latest-reading properties; consumers discover them by concrete type.
    /// </summary>
    public interface ISensor
    {
        string SensorName { get; }

        /// <summary>Sampling rate in Hz; sensors decimate FixedUpdate to this rate.</summary>
        float UpdateRateHz { get; }

        /// <summary>Sim time of the most recent sample, seconds; negative before first sample.</summary>
        double LastSampleTime { get; }
    }
}
