using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetection : MonoBehaviour
{
    public event Action<Collider2D> OnTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggered?.Invoke(other);
    }
    public void EnableDetection(bool enable)
    {
        gameObject.SetActive(enable);
    }
}
