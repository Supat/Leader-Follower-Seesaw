using LSL4Unity.Utils;
using UnityEngine;

[RequireComponent(typeof(ClientManager))]
public class ManagerInlet : AFloatInlet
{
    public void Reset()
    {
        StreamName = "Unity.GameManagerState";
    }

    protected override void OnStreamAvailable() { }

    protected override void Process(float[] newSample, double timestamp)
    {
        var manager = GetComponent<ClientManager>();

        if ((int)newSample[0] == 1)
            manager.PauseGame();
        else
            manager.UnpauseGame();

        manager.LeadingPlayerID = (int)newSample[1];
        manager.PlayScore = (int)newSample[2];
    }
}
