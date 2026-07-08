using UnityEngine;

namespace TacticalIntuition.Core
{
    /// <summary>
    /// The single façade for one aircraft. Implemented by DroneRuntime
    /// (Vehicles module); consumed by control, sensors, UI, cameras and API.
    /// </summary>
    public interface IVehicle
    {
        string VehicleName { get; }

        VehicleConfig Config { get; }

        Rigidbody Body { get; }

        /// <summary>True state refreshed at the start of every FixedUpdate (execution order -100).</summary>
        StateSnapshot TrueState { get; }

        bool IsArmed { get; }

        FlightMode Mode { get; }

        /// <summary>World position captured at arm time; RTL target.</summary>
        Vector3 HomePosition { get; }

        /// <summary>Remaining battery as fraction [0..1].</summary>
        float BatteryFraction { get; }

        /// <summary>Arms motors if pre-arm checks pass (level attitude, throttle low). Returns success.</summary>
        bool Arm();

        void Disarm();

        /// <summary>Requests a mode change; may be rejected (e.g. RTL with no home). Returns success.</summary>
        bool SetMode(FlightMode mode);
    }
}
