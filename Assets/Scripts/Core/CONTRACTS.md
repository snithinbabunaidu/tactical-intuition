# Module Contracts — Tactical Intuition Drone Simulator

Binding conventions for every runtime module. The C# files in `Assets/Scripts/Core/`
are the source of truth for shared types; this file pins down everything the types
alone cannot express. **Any module that deviates from this sheet breaks integration.**

## Module map and ownership

| Folder (under `Assets/Scripts/`) | Namespace | Owns |
|---|---|---|
| `Core/` | `TacticalIntuition.Core` | Shared contracts (done — do not edit) |
| `FlightDynamics/` | `TacticalIntuition.FlightDynamics` | `QuadrotorDynamics`, `WindField`, `BatteryModel` |
| `Control/` | `TacticalIntuition.Control` | `Pid`, `MotorMixer`, `FlightModeManager`, `RateController`, `AttitudeController`, `VelocityController`, `PositionController`, `PilotInput` |
| `Sensors/` | `TacticalIntuition.Sensors` | `SensorBase`, `ImuSensor`, `GpsSensor`, `BarometerSensor`, `MagnetometerSensor`, `RangefinderSensor` |
| `Vehicles/` | `TacticalIntuition.Vehicles` | `DroneRuntime` (implements `IVehicle`), `DroneVisuals` |
| `Environment/` | `TacticalIntuition.Environment` | `TimeOfDaySystem`, `WeatherSystem`, `WeatherPreset` |
| `Cameras/` | `TacticalIntuition.Cameras` | `CameraRig` (Chase / FPV / Orbit / Pad modes) |
| `UI/` | `TacticalIntuition.UI` | `HudController`, `TelemetryReadout` (programmatic uGUI) |
| `API/` | `TacticalIntuition.Api` | `TelemetryServer` (TCP JSON), `MainThreadDispatcher` |
| `Editor/` | `TacticalIntuition.EditorTools` | Bootstrap, drone builder, terrain/scene builders |

One public class per file, file named after the class. XML doc comments on all
public members. No third-party dependencies. No per-frame heap allocations in
`FixedUpdate` paths (cache arrays and lists).

## Frames and sign conventions (memorize)

- Unity world: left-handed, **+Y up, +Z North, +X East**. NED conversions via `FrameUtil`.
- Body axes: **+Z forward = roll axis, +X right = pitch axis, +Y up = yaw axis**.
- `PilotCommand`: roll + = right side down; pitch + = nose down (fly forward);
  yaw + = clockwise from above; throttle raw [0..1].
- Angular velocity mapping (body frame, rad/s) — from stick signs to Unity axes:
  `targetRates = new Vector3(pitchCmd * maxPitchRate, yawCmd * maxYawRate, -rollCmd * maxRollRate)`
  (note the **negative** on roll: positive Unity Z-rotation banks left).
- Positive rotation about +Y appears clockwise viewed from above; a **clockwise
  prop therefore applies a negative (counter-clockwise) yaw reaction torque** on
  the body: `yawTorqueSign_i = MotorClockwise[i] ? -1f : +1f`.

## Physics interface (FlightDynamics ↔ Control)

`QuadrotorDynamics : MonoBehaviour, IPropulsion` — requires `Rigidbody` on same GameObject.

Per `FixedUpdate` (default execution order 0):
1. Advance each motor's first-order lag toward its commanded thrust:
   `thrustCmd_i = MaxThrustPerMotorN * pow(clamp01(throttle_i), ThrustCurveExponent)`,
   `thrust_i += (thrustCmd_i - thrust_i) * dt / max(dt, MotorTimeConstant)`.
2. Apply per-motor world force: `rb.AddForceAtPosition(transform.up * thrust_i, transform.TransformPoint(MotorLocalPositions[i]))`.
   (This produces roll/pitch torques automatically — do NOT add them again.)
3. Apply yaw reaction torque: `rb.AddTorque(transform.up * Σ(sign_i * TorqueToThrustRatio * thrust_i))`.
4. Drag: `vAir = rb.linearVelocity - wind`, transform to body frame, per-axis
   `F_i = -(LinearDrag * v_i + QuadraticDragBody_i * |v_i| * v_i)`, back to world, `AddForce`.
   Wind comes from `WindField.Instance` (may be null → zero wind).
5. When disarmed, thrust commands are forced to 0 (motors still spin down through the lag).

`WindField : MonoBehaviour, IWindProvider` — scene singleton exposed as
`public static WindField Instance` (set in OnEnable, cleared in OnDisable).
Composition: steady wind vector + gusts (slow sine bursts) + turbulence
(3D Perlin sampled at position/time, strength scales with mean wind speed).
Public fields settable by WeatherSystem: `MeanWind` (Vector3, m/s), `Turbulence` [0..1], `GustStrength` [0..1].

`BatteryModel : MonoBehaviour` — drains from `HoverCurrentA` scaled by
`(totalThrust / hoverThrust)²`; exposes `public float Fraction` [0..1] and
`public float VoltageV`; raises `DroneEvents.RaiseBatteryLevelChanged` on 5% steps.

## Control stack (Control module)

All controllers are plain C# classes (not MonoBehaviours) driven by
`FlightModeManager : MonoBehaviour` (execution order **-50**):

- `Pid` — standard PID with `PidGains`, derivative-on-measurement, integral clamp,
  output clamp, `Reset()`. Signature: `float Step(float error, float measurement, float dt)`.
- `RateController` — 3 PIDs on body-rate error → body torque demand (N·m, Vector3).
- `AttitudeController` — P on lean-angle error → rate setpoints; feeds RateController.
- `VelocityController` — PIDs on world-velocity error → desired accel; converts to
  lean angles (`pitch = atan2(aFwdBody, g)`, clamp to `MaxTiltDeg`) + thrust
  (`m*(g+az) / cos(tilt)`, clamped to `[0, 0.95 * MotorCount * MaxThrustPerMotorN]`).
- `PositionController` — P on position error → velocity setpoint (clamped).
- `MotorMixer` — static. Builds the 4×4 allocation matrix rows
  `[1,1,1,1; -z_i…; sign_i*c…; x_i…]` mapping motor thrusts → `(F, τx, τy, τz)`,
  inverts once (`Matrix4x4.inverse`), caches per config. `Allocate` returns
  normalized throttles: `throttle_i = pow(clamp01(T_i / MaxThrustPerMotorN), 1/ThrustCurveExponent)`.
- `FlightModeManager` public API (used by `DroneRuntime`, UI, API server):
  - `public FlightMode CurrentMode { get; }`
  - `public bool RequestMode(FlightMode mode)` — validates (RTL/AutoLand need armed craft)
  - `public void SetPilotOverride(PilotCommand cmd)` — external (API) input; newest wins for one FixedUpdate
  - Reads sticks from `PilotInput` component on the same GameObject each FixedUpdate,
    runs the cascade appropriate to the mode, calls `IPropulsion.SetThrottles`.
  - While disarmed: zero throttles; auto-disarm on touchdown in AutoLand/RTL final phase.
- `PilotInput : MonoBehaviour` — polls `Keyboard.current` / `Gamepad.current`
  (new Input System, low-level polling, no .inputactions asset). Exposes
  `public PilotCommand Command { get; }`, plus edge-triggered
  `public bool ConsumeArmToggle()`, `ConsumeModeCycle()`, `ConsumeCameraCycle()`, `ConsumeReset()`.
  Keys: WASD = pitch/roll, Q/E = yaw, Shift/Ctrl = throttle up/down (throttle is a
  persistent value moved by keys, gamepad left-stick-Y sets it absolutely),
  R = arm/disarm, M = mode cycle, C = camera, Backspace = reset. Deadband 0.05 +
  expo 0.3 via `FrameUtil`.

## Vehicle glue (Vehicles module)

`DroneRuntime : MonoBehaviour, IVehicle` — execution order **-100**. Requires
`Rigidbody`. On `Awake`: applies `MassKg`, `InertiaDiag` (set `rb.inertiaTensor`,
`rb.inertiaTensorRotation = Quaternion.identity`), `rb.useGravity = true`,
`rb.linearDamping = 0`, `rb.angularDamping = 0`, `rb.interpolation = Interpolate`,
`rb.collisionDetectionMode = ContinuousDynamic`, `rb.maxAngularVelocity = 25`.
Every FixedUpdate: refresh `TrueState` (AGL via downward raycast, layer mask
everything except own colliders — put drone on layer default, raycast from body
origin, `QueryTriggerInteraction.Ignore`, ignore own colliders by distance check or
start offset). Computes acceleration by differencing velocity. Handles
`OnCollisionEnter` → if `relativeVelocity.magnitude > 4` raise `RaiseCrashed` and
`Disarm()`. `Arm()` pre-checks: tilt < 20°, `BatteryFraction > 0.05`. On arm, store
`HomePosition`, raise event. `SetMode` delegates to `FlightModeManager.RequestMode`.

`DroneVisuals : MonoBehaviour` — spins visual prop transforms (assigned by the
editor drone builder) proportional to per-motor throttle from `IPropulsion.CurrentThrottles`,
opposite directions per `MotorClockwise`.

## Sensors module

`SensorBase : MonoBehaviour, ISensor` — finds `IVehicle` via `GetComponentInParent`.
In FixedUpdate accumulates time; when `1/UpdateRateHz` elapsed calls
`protected abstract void Sample(in StateSnapshot s, float dt)`. Gaussian noise via
Box–Muller (`NextGaussian(System.Random)` helper on the base class, seeded per sensor).

- `ImuSensor` — accel (body frame, includes gravity reaction, bias + noise), gyro (body rad/s, bias + noise). 200 Hz.
- `GpsSensor` — position (+ horizontal/vertical Gaussian error, latency queue ~100 ms), velocity. 10 Hz. Also exposes `LatLon` derived from a configurable origin (default 47.6414, -122.1401) using simple metres-per-degree conversion.
- `BarometerSensor` — altitude MSL = world Y + noise + slow random-walk drift. 50 Hz.
- `MagnetometerSensor` — heading degrees + noise. 50 Hz.
- `RangefinderSensor` — downward raycast, max 40 m, small proportional noise. 50 Hz.

## Environment module

- `TimeOfDaySystem` — drives a directional light ("Sun") rotation from
  `[Range(0,24)] public float TimeOfDay`, optional day speed multiplier; sets light
  temperature/intensity vs sun elevation. Must not require HDRP types beyond
  `HDAdditionalLightData` guarded with `#if` — plain Light API preferred.
- `WeatherSystem` — holds `WeatherPreset` (ScriptableObject: name, mean wind speed/
  direction, turbulence, gust, fog density, cloud opacity); applies wind to
  `WindField.Instance`, fog via an assigned HDRP `Volume` profile's `Fog` override
  (`UnityEngine.Rendering.HighDefinition.Fog`, use `meanFreePath`), null-safe if no volume assigned.

## Cameras / UI / API

- `CameraRig` — modes Chase (smoothed follow + look-at), FPV (hard-mounted, small
  angle uptilt), Orbit (mouse drag), Pad (fixed world point tracking drone). Cycled
  by `PilotInput.ConsumeCameraCycle()`. Target acquired from `DroneEvents.VehicleSpawned`.
- `HudController` — builds the entire canvas in code (`Screen Space - Overlay`,
  `CanvasScaler` 1920×1080 reference): armed state, mode, battery bar, altitude AGL/MSL,
  ground/air speed, heading tape (simple text), vertical speed, motor bars, wind
  readout, crosshair. Legacy `UnityEngine.UI.Text` with
  `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` — **no TextMeshPro**.
- `TelemetryServer` — TCP listener on 127.0.0.1:41451, newline-delimited JSON
  (`JsonUtility` for payloads). Commands: `get_state`, `arm`, `disarm`, `set_mode`,
  `takeoff {alt}`, `land`, `goto {x,y,z,speed}`, `set_velocity {vx,vy,vz,yaw_rate}`,
  `rc {roll,pitch,yaw,throttle}`. Socket work on a background thread; all vehicle
  access marshalled through `MainThreadDispatcher` (ConcurrentQueue drained in Update).
  Disabled by default (`enabled = false` until `StartServer()` or inspector toggle).

## Unity 6 API rules (build errors otherwise)

- `rb.linearVelocity` / `rb.linearDamping` / `rb.angularDamping` — **never**
  `rb.velocity`, `rb.drag`, `rb.angularDrag`.
- `Object.FindFirstObjectByType<T>()` / `FindObjectsByType<T>(FindObjectsSortMode.None)` —
  **never** `FindObjectOfType`.
- HDRP 17 namespaces: `UnityEngine.Rendering`, `UnityEngine.Rendering.HighDefinition`.
- Input System 1.11: `UnityEngine.InputSystem.Keyboard.current` etc.; project may
  have both input backends enabled.
- Target: Windows standalone, .NET Standard 2.1 profile — no `System.Text.Json`,
  use `JsonUtility`.

## Execution order summary

| Component | Order |
|---|---|
| `DroneRuntime` | -100 (`[DefaultExecutionOrder(-100)]`) |
| `FlightModeManager` | -50 |
| `QuadrotorDynamics`, `WindField` | 0 |
| Sensors | +10 |
| `BatteryModel` | +10 |
| UI / Cameras / API | Update/LateUpdate only |
