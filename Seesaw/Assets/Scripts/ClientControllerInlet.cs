using LSL4Unity.Utils;
using UnityEngine;

public class ClientControllerInlet : AFloatInlet, PlayerController
{
    private static bool isFreeze = false;
    public bool IsFreeze
    {
        get { return isFreeze; }
        set { isFreeze = value; }
    }

    private float perturbation = 1.0f;
    public float Perturbation
    {
        get { return perturbation; }
        set { perturbation = value; }
    }

    public void Reset()
    {
        StreamName = "Unity.ClientPlayerDistance";
    }

    protected override void OnStreamAvailable() { }

    protected override void Process(float[] newSample, double timestamp)
    {
        if (!isFreeze)
            transform.position = new Vector3(transform.position.x, transform.position.y + (newSample[0] * perturbation), transform.position.z);
    }
}
