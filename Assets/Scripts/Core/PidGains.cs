using UnityEngine;

namespace TacticalIntuition.Core
{
    /// <summary>Serializable PID gain set with anti-windup and output clamping.</summary>
    [System.Serializable]
    public struct PidGains
    {
        public float Kp;
        public float Ki;
        public float Kd;

        /// <summary>Absolute clamp on the integral term's contribution to output.</summary>
        public float IntegralLimit;

        /// <summary>Absolute clamp on total output; 0 or negative = unclamped.</summary>
        public float OutputLimit;

        public PidGains(float kp, float ki, float kd, float integralLimit, float outputLimit)
        {
            Kp = kp;
            Ki = ki;
            Kd = kd;
            IntegralLimit = integralLimit;
            OutputLimit = outputLimit;
        }
    }
}
