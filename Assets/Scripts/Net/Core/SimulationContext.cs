using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SimulationContext : ISimulationContext
{
    public int Frame { get; private set; }
    public float DeltaTime { get; private set; }

    public int BallOwnerId { get; private set; }
    public Vector2 BallPosition { get; private set; }
    public IReadOnlyList<CarrierSnapshot> Players => _players;
    private List<CarrierSnapshot> _players = new();

    public void BuildFrom(
        SimulationState state,
        int frame,
        float dt
    )
    {
        Frame = frame;
        DeltaTime = dt;

        BallPosition = state.Ball.Position;
        BallOwnerId = state.Ball.ownerId;

        _players.Clear();
        foreach (var p in state.Players)
        {
            _players.Add(new CarrierSnapshot(p));
        }
    }
}
