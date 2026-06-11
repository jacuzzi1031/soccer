using Net.FixFloat;
using UnityEngine;

public class PlayerStateData
{
    public int InputType { get; private set; }
    public FixedVector2 ResetPosition { get; private set; }
    public FixedVector2 MoveDir { get; private set; }
    public FixedVector2 ShotDirection { get; private set; }
    public FixedFloat ShotPower { get; private set; }
    public bool isPowerShot { get; private set; }
    public  bool IsInstant { get; private set; }
    public  PlayerSim passTarget { get; private set; }
    

    public static PlayerStateData Build()
    {
        return new PlayerStateData();
    }

    public PlayerStateData SetShotDirection(FixedVector2 direction)
    {
        ShotDirection = direction;
        return this;
    }

    public PlayerStateData SetShotPower(FixedFloat power)
    {
        ShotPower = power;
        return this;
    }

    public PlayerStateData SetPowerShotSound(bool IsPowerShotSound) {
        isPowerShot = IsPowerShotSound;
        return this;
    }


    public PlayerStateData SetInputType(int inputType)
    {
        InputType = inputType;
        return this;
    }
    public PlayerStateData SetMoveDir(FixedVector2 moveDir)
    {
        MoveDir = moveDir;
        return this;
    }

    public PlayerStateData SetResetPosition(FixedVector2 position)
    {
        ResetPosition = position;
        return this;
    }

    public PlayerStateData SetIsInstant(bool isInstant) {
        IsInstant = isInstant;
        return this;
    }

    public PlayerStateData setPassTarget(PlayerSim PassTarget) {
        passTarget=PassTarget;
        return this;
    }
}