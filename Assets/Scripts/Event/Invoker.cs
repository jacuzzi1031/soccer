using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Invoker : MonoBehaviour
{
    public static Invoker Instance { get; private set; }

    public List<Action> DelegateList { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DelegateList = new List<Action>();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        DelegateList.Clear();
    }

    private void Update()
    {
        Execute();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void Execute()
    {
        if (DelegateList.Count == 0) return;
        for (int i = 0; i < DelegateList.Count; i++)
        {
            try
            {
                // Debug.Log("Invoker Execute!!!!!");
                DelegateList[i]?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        DelegateList.Clear();
    }
}