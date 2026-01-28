using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerResource
{
    public string fullName;
    public PlayerView.SkinColor skin;
    public Role role;
    public float speed;
    public float power;

    public PlayerResource(string fullName, PlayerView.SkinColor skin, Role role, float speed, float power)
    {
        this.fullName = fullName;
        this.skin = skin;
        this.role = role;
        this.speed = speed;
        this.power = power;
    }
}
