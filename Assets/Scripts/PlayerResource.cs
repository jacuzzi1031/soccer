using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResource
{
    public string fullName;
    public Player.SkinColor skin;
    public Player.Role role;
    public float speed;
    public float power;

    public PlayerResource(string fullName, Player.SkinColor skin, Player.Role role, float speed, float power)
    {
        this.fullName = fullName;
        this.skin = skin;
        this.role = role;
        this.speed = speed;
        this.power = power;
    }
}
