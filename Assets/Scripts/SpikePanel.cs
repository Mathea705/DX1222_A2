using UnityEngine;

public class SpikePanel : MonoBehaviour
{
    [SerializeField] private FourthPuzzleManager manager;
    [SerializeField] private int panelId;

    void OnMouseDown()
    {
        if (panelId == 1) {
            manager.PressPanel1();
        }
        else {
             manager.PressPanel2();
        }
    }
}
