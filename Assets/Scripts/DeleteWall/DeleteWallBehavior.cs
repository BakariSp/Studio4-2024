using UnityEngine;

public class DeleteWallBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Deletable"))
        {
            Destroy(other.gameObject);
        }
    }
} 