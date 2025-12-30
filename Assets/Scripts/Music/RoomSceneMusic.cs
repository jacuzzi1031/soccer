using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    void Start()
    {
        MusicManager.Instance.Play(
            MusicManager.Instance.Refs.ROOM
        );
    }
}
