using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetection : MonoBehaviour
{
    public event Action<Collider2D> OnTriggered;
    public event Action<Collider2D> OnTriggerExit;
    public event Action<Collider2D> OnStay;

    private void OnTriggerEnter2D(Collider2D other)
    {
        OnTriggered?.Invoke(other);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerExit?.Invoke(other);
    }

    private void OnTriggerStay2D(Collider2D other) {
        OnStay?.Invoke(other);
    }

    public void EnableDetection(bool enable)
    {
        gameObject.SetActive(enable);
    }
}
