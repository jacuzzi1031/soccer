using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationFacade
{
    private readonly CommandBuffer _commandBuffer;
    public SimulationFacade(
        CommandBuffer commandBuffer)
    {
        _commandBuffer=commandBuffer;
    }

}
