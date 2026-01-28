using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISimulationSystem
{
    void Tick(int frame);
    void Stop();

    void SetInputBuffer(InputBuffer inputBuffer) {
    }
}
