using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISimulationSystem
{   
    
    void Tick(SimulationContext context);
    void Stop();
}
