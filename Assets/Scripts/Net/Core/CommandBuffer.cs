using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class CommandBuffer
{
    private List<SimulationCommand> _writeBuffer = new List<SimulationCommand>(32);
    private List<SimulationCommand> _readBuffer = new List<SimulationCommand>(32);

    public void Enqueue(SimulationCommand cmd)
    {
        _writeBuffer.Add(cmd);
    }
    
    public IReadOnlyList<SimulationCommand> Consume()
    {
        var temp = _readBuffer;
        _readBuffer = _writeBuffer;
        _writeBuffer = temp;
        
        _writeBuffer.Clear();

        return _readBuffer;
    }
    
    // private List<SimulationCommand> _commands = new();
    //
    // public void Enqueue(SimulationCommand cmd)
    // {
    //     _commands.Add(cmd);
    // }
    //
    // public IReadOnlyList<SimulationCommand> Consume()
    // {
    //     var snapshot = _commands;
    //     _commands = new List<SimulationCommand>();
    //     return snapshot;
    // }
    // public IReadOnlyList<SimulationCommand> Dequeue()
    // {
    //     var consumedCommands = new List<SimulationCommand>(_commands);
    //     _commands.Clear();
    //     return consumedCommands;
    // }
    //
    //
    // public void Clear()
    // {
    //     _commands.Clear();
    // }
}

