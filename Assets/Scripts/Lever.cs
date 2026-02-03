using UnityEngine;

public class Lever : MonoBehaviour
{

    private bool up;

    [SerializeField] private GameObject handle;

    private Vector3 originalPosition;
    private Vector3 offsetPosition;
    private Vector3 targetPosition;

    [SerializeField] private ThirdPuzzleManager manager;
    [SerializeField] private int playerSet;
    [SerializeField] private int leverIndex;

    void Start()
    {
        originalPosition = handle.transform.position;
        offsetPosition = handle.transform.position + new Vector3(0, 0.7f, 0);
        targetPosition = originalPosition;
    }

    void Update()
    {
        handle.transform.position = Vector3.Lerp(handle.transform.position, targetPosition, Time.deltaTime * 2.0f);
    }

    void OnMouseDown()
    {
        manager.OnLeverToggled(playerSet, leverIndex);
    }

    public void ToggleLever()
    {
        up = !up;
        targetPosition = up ? offsetPosition : originalPosition;
    }

    public string getState()
    {
        if (up)
        {
            return "Up";
        }
        else
        {
            return "Down";
        }
    }
}
