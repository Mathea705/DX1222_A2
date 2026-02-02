using UnityEngine;
using Unity.Netcode;

public class KeypadManager : MonoBehaviour
{
    [SerializeField] private SecondPuzzleManager puzzleManager;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj.IsOwner)
        {
            puzzleManager.OpenKeypad();
        }
    }
}
