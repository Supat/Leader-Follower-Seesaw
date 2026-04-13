using UnityEngine;

public class CylinderBehavior : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;

    void Start()
    {
        transform.position = player1.transform.position;
    }

    void FixedUpdate()
    {
        transform.position = player1.transform.position;
        transform.LookAt(player2.transform.position);
        transform.Rotate(90f, 0f, 0f, Space.Self);

        float playerDistance = Vector3.Distance(player2.transform.position, player1.transform.position);
        transform.localScale = new Vector3(transform.localScale.x, playerDistance / 2f, transform.localScale.z);
    }
}
