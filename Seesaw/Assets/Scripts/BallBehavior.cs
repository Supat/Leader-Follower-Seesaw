using UnityEngine;

public class BallBehavior : MonoBehaviour
{
    private bool isHit = false;
    public bool IsHit
    {
        get { return isHit; }
        set { isHit = value; }
    }

    private void OnTriggerEnter(Collider other)
    {
        isHit = true;
    }
}
