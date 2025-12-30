using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneMusic : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance.Play(
            MusicManager.Instance.Refs.GAMEPLAY
        );
    }
}
