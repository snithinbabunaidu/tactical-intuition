namespace TacticalIntuition.Core
{
    /// <summary>
    /// Normalized pilot stick state, produced once per frame by input and consumed
    /// by the flight mode logic. Sign conventions are fixed project-wide:
    ///   Roll     [-1..1]  positive = roll right (right stick right)
    ///   Pitch    [-1..1]  positive = nose down / fly forward (right stick forward)
    ///   Yaw      [-1..1]  positive = yaw clockwise viewed from above (left stick right)
    ///   Throttle [ 0..1]  raw stick; assisted modes treat 0.5 as "hold", above as climb.
    /// </summary>
    [System.Serializable]
    public struct PilotCommand
    {
        public float Roll;
        public float Pitch;
        public float Yaw;
        public float Throttle;

        public static PilotCommand Neutral => new PilotCommand { Roll = 0f, Pitch = 0f, Yaw = 0f, Throttle = 0f };
    }
}
