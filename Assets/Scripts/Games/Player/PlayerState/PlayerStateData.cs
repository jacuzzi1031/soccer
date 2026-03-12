using UnityEngine;

public class PlayerStateData
{
    public int InputType { get; private set; }
    public Vector2 ResetPosition { get; private set; }
    public Vector2 MoveDir { get; private set; }
    public Vector2 ShotDirection { get; private set; }
    public float ShotPower { get; private set; }
    public bool isPowerShot { get; private set; }
    public  bool IsInstant { get; private set; }
    public  PlayerSim passTarget { get; private set; }
    

    public static PlayerStateData Build()
    {
        return new PlayerStateData();
    }

    public PlayerStateData SetShotDirection(Vector2 direction)
    {
        ShotDirection = direction;
        return this;
    }

    public PlayerStateData SetShotPower(float power)
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
    public PlayerStateData SetMoveDir(Vector2 moveDir)
    {
        MoveDir = moveDir;
        return this;
    }

    public PlayerStateData SetResetPosition(Vector2 position)
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