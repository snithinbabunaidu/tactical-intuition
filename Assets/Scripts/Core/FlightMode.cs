namespace TacticalIntuition.Core
{
    /// <summary>
    /// Pilot-selectable flight modes, ordered roughly by increasing assistance.
    /// </summary>
    public enum FlightMode
    {
        /// <summary>Direct body-rate control, no self-leveling. Expert/FPV.</summary>
        Acro = 0,

        /// <summary>Sticks command lean angle; craft self-levels when released.</summary>
        Angle = 1,

        /// <summary>Angle + throttle stick commands climb rate; altitude locked at center stick.</summary>
        AltitudeHold = 2,

        /// <summary>Full brain: sticks command horizontal velocity, craft holds 3D position when released.</summary>
        PositionHold = 3,

        /// <summary>Autonomous: climb to safe altitude, fly home, descend and land.</summary>
        ReturnToLaunch = 4,

        /// <summary>Autonomous: descend in place and disarm on touchdown.</summary>
        AutoLand = 5
    }

    /// <summary>Broad airframe families supported by the vehicle framework.</summary>
    public enum VehicleClass
    {
        Multirotor = 0,
        FixedWing = 1,
        HybridVtol = 2
    }
}
