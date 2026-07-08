using UnityEngine;

namespace TacticalIntuition.Core
{
    /// <summary>
    /// Ground-truth kinematic state of a vehicle for one physics step.
    /// World frame is the Unity frame: left-handed, +Y up, +Z treated as North,
    /// +X treated as East. Body frame: +Z forward (roll axis), +X right (pitch
    /// axis), +Y up (yaw axis). See FrameUtil for NED conversions.
    /// </summary>
    [System.Serializable]
    public struct StateSnapshot
    {
        /// <summary>Seconds since simulation start (Time.fixedTimeAsDouble).</summary>
        public double Time;

        /// <summary>World position in metres.</summary>
        public Vector3 Position;

        /// <summary>World-frame velocity in m/s.</summary>
        public Vector3 Velocity;

        /// <summary>World-frame acceleration in m/s^2, gravity included.</summary>
        public Vector3 Acceleration;

        /// <summary>Body-to-world rotation.</summary>
        public Quaternion Attitude;

        /// <summary>Angular velocity in rad/s expressed in the body frame.</summary>
        public Vector3 AngularVelocityBody;

        /// <summary>Metres above ground from a downward raycast; negative if unknown.</summary>
        public float AltitudeAgl;

        /// <summary>Compass heading in degrees [0..360), 0 = North (+Z), 90 = East (+X).</summary>
        public float HeadingDeg => FrameUtil.HeadingDeg(Attitude * Vector3.forward);
    }
}
