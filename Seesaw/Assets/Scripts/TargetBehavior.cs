using UnityEngine;

public class TargetBehavior : MonoBehaviour
{
    public Material greenMaterial;
    public Material redMaterial;

    private bool isHit = false;
    public bool IsHit
    {
        get { return isHit; }
        set { isHit = value; }
    }

    void Start()
    {
        GetComponent<MeshRenderer>().material = redMaterial;
    }

    private void OnTriggerEnter(Collider other)
    {
        isHit = true;
    }

    private void OnTriggerExit(Collider other)
    {
        isHit = false;
    }
}
