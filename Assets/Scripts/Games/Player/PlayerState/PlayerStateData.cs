using UnityEngine;

public class PlayerStateData
{
    public Vector2 HurtDirection { get; private set; }
    public Player PassTarget { get; private set; }
    public Vector2 ResetPosition { get; private set; }
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

    public PlayerStateData SetHurtDirection(Vector2 direction)
    {
        HurtDirection = direction;
        return this;
    }

    public PlayerStateData SetPassTarget(Player player)
    {
        PassTarget = player;
        return this;
    }

    public PlayerStateData SetResetPosition(Vector2 position)
    {
        ResetPosition = position;
        return this;
    }
}