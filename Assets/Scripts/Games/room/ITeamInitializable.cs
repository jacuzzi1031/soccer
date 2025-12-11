using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITeamInitializable
{
    void OnTeamsReady(OnSquadsReadyEvent e);
}
