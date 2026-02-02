using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private int plateId;
    [SerializeField] private FirstPuzzleManager puzzleManager;
    [SerializeField] private Renderer plateRenderer;

    private int playersOnPlate = 0;
    private Color originalColor;
    private Color neonGreen = new Color(0.2f, 1f, 0.2f);

    void Start()
    {

            originalColor = plateRenderer.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playersOnPlate++;

        if (playersOnPlate == 1)
        {
            puzzleManager.PlateStateChanged(plateId, true);
                plateRenderer.material.color = neonGreen;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playersOnPlate--;

        if (playersOnPlate == 0)
        {
            puzzleManager.PlateStateChanged(plateId, false);
        
                plateRenderer.material.color = originalColor;
        }
    }
}
