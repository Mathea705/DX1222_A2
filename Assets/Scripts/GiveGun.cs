using UnityEngine;

public class GiveGun : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FirstPersonController controller = other.GetComponent<FirstPersonController>();

                controller.EnableGun();
            
        }
    }
}
