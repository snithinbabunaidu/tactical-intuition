namespace TacticalIntuition.Core
{
    /// <summary>
    /// Turns normalized throttle commands into forces on the airframe.
    /// Implemented by QuadrotorDynamics (FlightDynamics module); driven by the
    /// motor mixer (Control module) every FixedUpdate.
    /// </summary>
    public interface IPropulsion
    {
        int MotorCount { get; }

        /// <summary>
        /// Commands all motors for this physics step. Values are clamped to
        /// [0..1]; array length must equal MotorCount. The implementation applies
        /// its own motor spin-up lag before thrust is produced.
        /// </summary>
        void SetThrottles(float[] throttles);

        /// <summary>Last commanded throttles (normalized), for HUD/telemetry.</summary>
        float[] CurrentThrottles { get; }

        /// <summary>Total thrust currently produced by all motors, Newtons.</summary>
        float TotalThrustNewtons { get; }
    }
}
