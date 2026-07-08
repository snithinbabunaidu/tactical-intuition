using UnityEngine;

namespace TacticalIntuition.Core
{
    /// <summary>
    /// Frame conventions and small math helpers shared by all modules.
    /// Unity world: left-handed, +Y up, +Z = North, +X = East.
    /// NED (aviation): x = North, y = East, z = Down.
    /// </summary>
    public static class FrameUtil
    {
        /// <summary>Unity world vector → NED vector (North, East, Down).</summary>
        public static Vector3 UnityToNed(Vector3 u) => new Vector3(u.z, u.x, -u.y);

        /// <summary>NED vector (North, East, Down) → Unity world vector.</summary>
        public static Vector3 NedToUnity(Vector3 ned) => new Vector3(ned.y, -ned.z, ned.x);

        /// <summary>Compass heading in degrees [0..360) of a world-frame direction. 0 = North (+Z), 90 = East (+X).</summary>
        public static float HeadingDeg(Vector3 worldForward)
        {
            float deg = Mathf.Atan2(worldForward.x, worldForward.z) * Mathf.Rad2Deg;
            return deg < 0f ? deg + 360f : deg;
        }

        /// <summary>Wraps an angle to (-180, 180].</summary>
        public static float WrapDeg(float deg)
        {
            deg %= 360f;
            if (deg > 180f) deg -= 360f;
            if (deg <= -180f) deg += 360f;
            return deg;
        }

        /// <summary>Wraps an angle to (-π, π].</summary>
        public static float WrapRad(float rad)
        {
            const float twoPi = 2f * Mathf.PI;
            rad %= twoPi;
            if (rad > Mathf.PI) rad -= twoPi;
            if (rad <= -Mathf.PI) rad += twoPi;
            return rad;
        }

        /// <summary>
        /// Roll/pitch lean angles of an attitude, radians, sign-matched to
        /// PilotCommand: pitch positive = nose down, roll positive = right side down.
        /// </summary>
        public static void LeanAngles(Quaternion attitude, out float pitchRad, out float rollRad)
        {
            Vector3 fwd = attitude * Vector3.forward;
            Vector3 right = attitude * Vector3.right;
            pitchRad = Mathf.Asin(Mathf.Clamp(-fwd.y, -1f, 1f));
            rollRad = Mathf.Asin(Mathf.Clamp(-right.y, -1f, 1f));
        }

        /// <summary>Total tilt of body-up away from world-up, radians.</summary>
        public static float TiltRad(Quaternion attitude)
        {
            float upDot = Mathf.Clamp((attitude * Vector3.up).y, -1f, 1f);
            return Mathf.Acos(upDot);
        }

        /// <summary>Symmetric deadband then rescale so output still spans [-1, 1].</summary>
        public static float ApplyDeadband(float value, float deadband)
        {
            float a = Mathf.Abs(value);
            if (a <= deadband) return 0f;
            return Mathf.Sign(value) * (a - deadband) / (1f - deadband);
        }

        /// <summary>Classic RC expo: expo 0 = linear, 1 = fully cubic around center.</summary>
        public static float ApplyExpo(float value, float expo)
        {
            expo = Mathf.Clamp01(expo);
            return (1f - expo) * value + expo * value * value * value;
        }
    }
}
