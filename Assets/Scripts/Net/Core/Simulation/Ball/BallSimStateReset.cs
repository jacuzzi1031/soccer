
    using Net.FixFloat;
    using UnityEngine;

    public class BallSimStateReset:BallSimState{
        public override void OnEnter() {
            ballSim.Position = ballSim.spawnPosition;
            ballSim.Velocity = FixedVector2.Zero;
            //heightVelocity必须设为零，否则可能导致开球MoveVertical进而球速受影响
            ballSim.HeightVelocity=FixedFloat.Zero;
            ballSim.Height=FixedFloat.Zero;
        }

        public override void _Update()
        {
            MoveHorizontal();
        }
    }
