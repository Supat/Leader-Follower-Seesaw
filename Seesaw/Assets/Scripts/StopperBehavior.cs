using UnityEngine;

public class StopperBehavior : MonoBehaviour
{
    public GameObject player;
    public GameObject lookAtTarget;
    public bool invert;

    void Start()
    {
        transform.position = player.transform.position;
    }

    void Update()
    {
        transform.position = player.transform.position;
        transform.LookAt(lookAtTarget.transform.position);
        transform.Rotate(invert ? -90f : 90f, 0f, 0f, Space.Self);
        transform.localScale = new Vector3(transform.localScale.x, 1.0f, transform.localScale.z);
    }
}
