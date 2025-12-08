using UnityEngine;

public class PlayerStateData
{
    public GameInput.PlayerInputType InputType { get; private set; }
    public Vector2 ResetPosition { get; private set; }
    public Vector2 MoveDir { get; private set; }
    public Vector2 ShotDirection { get; private set; }
    public float ShotPower { get; private set; }

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


    public PlayerStateData SetInputType(GameInput.PlayerInputType inputType)
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
}