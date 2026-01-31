using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISimulationSystem
{
    void Tick(ISimulationContext context);
    void Stop();

    void SetInputBuffer(InputBuffer inputBuffer) {
    }
}
