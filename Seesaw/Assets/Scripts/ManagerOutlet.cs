using System.Collections.Generic;
using LSL4Unity.Utils;
using UnityEngine;

[RequireComponent(typeof(ServerManager))]
public class ManagerOutlet : AFloatOutlet
{
    public void Reset()
    {
        StreamName = "Unity.GameManagerState";
        StreamType = "Unity.Tuple";
        moment = MomentForSampling.FixedUpdate;
    }

    public override List<string> ChannelNames
    {
        get { return new List<string> { "IsPause", "LeadingPlayerID", "PlayScore" }; }
    }

    protected override bool BuildSample()
    {
        var manager = GetComponent<ServerManager>();
        sample[0] = manager.PauseState;
        sample[1] = manager.LeadingPlayerID;
        sample[2] = manager.PlayScore;
        return true;
    }
}
