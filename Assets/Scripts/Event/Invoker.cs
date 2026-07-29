using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Invoker : MonoBehaviour
{
    public static Invoker Instance { get; private set; }

    private readonly List<Action> _delegateList = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        _delegateList.Clear();
    }

    public void Enqueue(Action action)
    {
        _delegateList.Add(action);
    }

    private void Update()
    {
        Execute();
    }

    private void Execute()
    {
        if (_delegateList.Count == 0)
            return;

        for (int i = 0; i < _delegateList.Count; i++)
        {
            try
            {
                _delegateList[i]?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        _delegateList.Clear();
    }
}