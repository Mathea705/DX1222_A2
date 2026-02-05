using UnityEngine;

public class FourthExit : MonoBehaviour
{
    [SerializeField] private GameObject[] doors;
    [SerializeField] private float openOffset = 3.0f;
    [SerializeField] private float lerpSpeed = 5f;

    private Vector3[] doorStartPositions;
    private bool doorsOpen = false;

    void Start()
    {
        doorStartPositions = new Vector3[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            doorStartPositions[i] = doors[i].transform.position;
        }
    }

    void Update()
    {
        if (!doorsOpen) return;

        for (int i = 0; i < doors.Length; i++)
        {
            Vector3 target = doorStartPositions[i] + new Vector3(0, openOffset, 0);
            doors[i].transform.position = Vector3.Lerp(doors[i].transform.position, target, lerpSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            doorsOpen = true;
        }
    }
}
