using LSL4Unity.Utils;
using UnityEngine;

public class PositionInlet : AFloatInlet
{
    private const string BaseStreamName = "Unity.Position";
    private const string BaseStreamType = "Unity.Transform";

    public void Awake()
    {
        StreamName = BaseStreamName + "." + name;
        StreamType = BaseStreamType;
    }

    void Reset()
    {
        StreamName = BaseStreamName + "." + name;
    }

    protected override void OnStreamAvailable() { }

    protected override void Process(float[] newSample, double timestamp)
    {
        transform.position = new Vector3(newSample[0], newSample[1], newSample[2]);
    }
}
