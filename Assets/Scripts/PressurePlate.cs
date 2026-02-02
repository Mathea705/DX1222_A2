using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private int plateId;
    [SerializeField] private FirstPuzzleManager puzzleManager;

    private int playersOnPlate = 0;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playersOnPlate++;

        if (playersOnPlate == 1)
            puzzleManager.PlateStateChanged(plateId, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playersOnPlate--;

        if (playersOnPlate == 0)
            puzzleManager.PlateStateChanged(plateId, false);
    }
}
