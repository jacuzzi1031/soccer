using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CommandBuffer
{
    private List<SimulationCommand> _commands = new();

    public void Enqueue(SimulationCommand cmd)
    {
        _commands.Add(cmd);
    }

    public IReadOnlyList<SimulationCommand> Consume()
    {
        var snapshot = _commands;
        _commands = new List<SimulationCommand>();
        return snapshot;
    }
    public IReadOnlyList<SimulationCommand> Dequeue()
    {
        var consumedCommands = new List<SimulationCommand>(_commands);
        _commands.Clear();
        return consumedCommands;
    }


    public void Clear()
    {
        _commands.Clear();
    }
}

