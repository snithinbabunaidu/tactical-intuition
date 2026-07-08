using System;

namespace TacticalIntuition.Core
{
    /// <summary>
    /// Project-wide event bus for loosely coupled systems (UI, audio, API,
    /// mission logic). Raise methods are provided so publishers never need
    /// null-checks; subscribers must unsubscribe in OnDisable/OnDestroy.
    /// </summary>
    public static class DroneEvents
    {
        public static event Action<IVehicle> VehicleSpawned;
        public static event Action<IVehicle> Armed;
        public static event Action<IVehicle> Disarmed;
        public static event Action<IVehicle, FlightMode> ModeChanged;

        /// <summary>Raised on hard impact; float = impact speed in m/s.</summary>
        public static event Action<IVehicle, float> Crashed;

        /// <summary>Raised when battery fraction crosses a 5% boundary; float = fraction [0..1].</summary>
        public static event Action<IVehicle, float> BatteryLevelChanged;

        public static void RaiseVehicleSpawned(IVehicle v) => VehicleSpawned?.Invoke(v);
        public static void RaiseArmed(IVehicle v) => Armed?.Invoke(v);
        public static void RaiseDisarmed(IVehicle v) => Disarmed?.Invoke(v);
        public static void RaiseModeChanged(IVehicle v, FlightMode m) => ModeChanged?.Invoke(v, m);
        public static void RaiseCrashed(IVehicle v, float impactSpeed) => Crashed?.Invoke(v, impactSpeed);
        public static void RaiseBatteryLevelChanged(IVehicle v, float fraction) => BatteryLevelChanged?.Invoke(v, fraction);
    }
}
