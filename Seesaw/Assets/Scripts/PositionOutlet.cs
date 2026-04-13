using System.Collections.Generic;
using LSL4Unity.Utils;
using UnityEngine;

public class PositionOutlet : AFloatOutlet
{
    private const string BaseStreamName = "Unity.Position";
    private const string BaseStreamType = "Unity.Transform";

    public void Awake()
    {
        StreamName = BaseStreamName + "." + name;
        StreamType = BaseStreamType;
        moment = MomentForSampling.FixedUpdate;
    }

    public void Reset()
    {
        StreamName = BaseStreamName + "." + name;
        StreamType = BaseStreamType;
        moment = MomentForSampling.FixedUpdate;
    }

    public override List<string> ChannelNames
    {
        get { return new List<string> { "PosX", "PosY", "PosZ" }; }
    }

    protected override bool BuildSample()
    {
        var position = transform.position;
        sample[0] = position.x;
        sample[1] = position.y;
        sample[2] = position.z;
        return true;
    }
}
