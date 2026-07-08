using UnityEngine;

namespace TacticalIntuition.Core
{
    /// <summary>
    /// Designer-tunable description of one airframe (mass properties, motor
    /// layout, limits, control gains). All quantities SI unless noted. Defaults
    /// describe the TI-X4, a 1.5 kg X-configuration quadcopter.
    /// </summary>
    [CreateAssetMenu(fileName = "VehicleConfig", menuName = "Tactical Intuition/Vehicle Config")]
    public class VehicleConfig : ScriptableObject
    {
        [Header("Identity")]
        public string DisplayName = "TI-X4";
        public VehicleClass Class = VehicleClass.Multirotor;

        [Header("Airframe")]
        [Tooltip("Total takeoff mass, kg")]
        public float MassKg = 1.5f;

        [Tooltip("Principal moments of inertia about body axes (x=pitch, y=yaw, z=roll), kg·m²")]
        public Vector3 InertiaDiag = new Vector3(0.03f, 0.05f, 0.03f);

        [Header("Motors (order: FR, FL, RL, RR for quads)")]
        [Tooltip("Motor hub positions in body frame, metres. +X right, +Z forward.")]
        public Vector3[] MotorLocalPositions =
        {
            new Vector3( 0.16f, 0f,  0.16f),   // 0 front-right
            new Vector3(-0.16f, 0f,  0.16f),   // 1 front-left
            new Vector3(-0.16f, 0f, -0.16f),   // 2 rear-left
            new Vector3( 0.16f, 0f, -0.16f)    // 3 rear-right
        };

        [Tooltip("True = prop spins clockwise viewed from above. Standard props-in X: FR ccw, FL cw, RL ccw, RR cw.")]
        public bool[] MotorClockwise = { false, true, false, true };

        [Tooltip("Maximum static thrust per motor, Newtons")]
        public float MaxThrustPerMotorN = 11f;

        [Tooltip("First-order motor response time constant, seconds")]
        public float MotorTimeConstant = 0.05f;

        [Tooltip("Thrust = MaxThrust * throttle^exponent. 2 ≈ realistic prop, 1 = linear")]
        public float ThrustCurveExponent = 2f;

        [Tooltip("Yaw reaction torque per Newton of thrust, metres (k_M / k_F)")]
        public float TorqueToThrustRatio = 0.016f;

        [Header("Aerodynamic drag")]
        [Tooltip("Linear drag coefficient, N per (m/s), isotropic")]
        public float LinearDrag = 0.02f;

        [Tooltip("Quadratic drag per body axis, N per (m/s)², x=lateral y=vertical z=longitudinal")]
        public Vector3 QuadraticDragBody = new Vector3(0.08f, 0.12f, 0.08f);

        [Header("Envelope limits")]
        public float MaxTiltDeg = 35f;
        public float MaxRollPitchRateDegS = 360f;
        public float MaxYawRateDegS = 180f;
        public float MaxClimbRateMs = 6f;
        public float MaxDescentRateMs = 3.5f;
        public float MaxHorizontalSpeedMs = 15f;

        [Header("RTL / auto-land")]
        [Tooltip("Altitude AGL the craft climbs to before flying home, metres")]
        public float RtlAltitudeM = 30f;
        public float AutoLandSpeedMs = 1.0f;

        [Header("Battery")]
        public float BatteryCapacitymAh = 5000f;
        public float BatteryNominalVoltage = 14.8f;
        [Tooltip("Current draw at hover, Amps; scales with square of total thrust fraction")]
        public float HoverCurrentA = 15f;

        [Header("Control gains — body rate loops (output N·m)")]
        public PidGains RateRollGains = new PidGains(0.6f, 0.2f, 0.02f, 0.2f, 4f);
        public PidGains RatePitchGains = new PidGains(0.6f, 0.2f, 0.02f, 0.2f, 4f);
        public PidGains RateYawGains = new PidGains(0.8f, 0.1f, 0f, 0.3f, 2f);

        [Header("Control gains — attitude P loop (angle err rad → rate sp rad/s)")]
        public PidGains AngleGains = new PidGains(8f, 0f, 0f, 0f, 6f);

        [Header("Control gains — velocity loops (vel err m/s → accel m/s²)")]
        public PidGains VelocityXYGains = new PidGains(2.5f, 0.5f, 0f, 2f, 8f);
        public PidGains VelocityZGains = new PidGains(4f, 1f, 0f, 3f, 8f);

        [Header("Control gains — position P loop (pos err m → vel sp m/s)")]
        public PidGains PositionGains = new PidGains(1.2f, 0f, 0f, 0f, 15f);

        public int MotorCount => MotorLocalPositions != null ? MotorLocalPositions.Length : 0;

        /// <summary>Weight in Newtons at standard gravity.</summary>
        public float WeightN => MassKg * 9.81f;

        /// <summary>Throttle fraction that produces exactly hover thrust on all motors.</summary>
        public float HoverThrottle
        {
            get
            {
                float perMotor = WeightN / Mathf.Max(1, MotorCount);
                float frac = Mathf.Clamp01(perMotor / Mathf.Max(0.001f, MaxThrustPerMotorN));
                return Mathf.Pow(frac, 1f / Mathf.Max(0.1f, ThrustCurveExponent));
            }
        }

        /// <summary>Basic consistency checks; returns null when valid, else a message.</summary>
        public string Validate()
        {
            if (MotorCount < 3) return "Need at least 3 motors for a multirotor.";
            if (MotorClockwise == null || MotorClockwise.Length != MotorCount)
                return "MotorClockwise length must match MotorLocalPositions.";
            if (MaxThrustPerMotorN * MotorCount <= WeightN)
                return "Total max thrust must exceed weight or the craft cannot hover.";
            return null;
        }
    }
}
