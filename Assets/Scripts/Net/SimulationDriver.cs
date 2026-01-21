using System.Collections.Generic;
using UnityEngine;

public enum SimulationState
{
    Idle,
    WaitingForStart,
    Running,
    Stopped
}

public class SimulationDriver : MonoBehaviour
{
    public static SimulationDriver Instance;

    public const float FRAME_DT = 1f / 60f;
    float accumulator;
    int currentFrame;

    SimulationState state = SimulationState.Idle;
    readonly List<ISimulationSystem> systems = new();

    public int CurrentFrame => currentFrame;
    public SimulationState State => state;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PrepareMatch(int startFrame)
    {
        systems.Clear();
        accumulator = 0f;
        currentFrame = startFrame;
        state = SimulationState.WaitingForStart;
    }

    public void SetSystems(List<ISimulationSystem> matchSystems)
    {
        systems.Clear();
        systems.AddRange(matchSystems);
    }

    public void StartSimulation()
    {
        state = SimulationState.Running;
    }

    public void StopMatch()
    {
        if (state == SimulationState.Stopped)
            return;

        state = SimulationState.Stopped;

        foreach (var system in systems)
            system.Stop();

        systems.Clear();
    }

    private void Update()
    {
        if (state != SimulationState.Running)
            return;

        accumulator += Time.deltaTime;

        while (accumulator >= FRAME_DT)
        {
            for (int i = 0; i < systems.Count; i++)
            {
                systems[i].Tick(currentFrame);
            }

            currentFrame++;
            accumulator -= FRAME_DT;
        }
    }
}


