using UnityEngine;
using Unity.Netcode;
using TMPro;


public class SecondPuzzleManager : NetworkBehaviour
{

    private NetworkVariable<int> correctCode = new NetworkVariable<int>();

    [SerializeField] private GameObject keypadCanvas;
    [SerializeField] private TextMeshProUGUI codeDisplayText;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [SerializeField] private TextMeshProUGUI wallCode;

    // [SerializeField] private BoxCollider triggerZone;

    [Header("Doors")]
    [SerializeField] private GameObject[] doors;
    [SerializeField] private Vector3 doorOpenOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float doorMoveSpeed = 2f;

    private string currentInput = "";
    private bool isInteracting = false;
    private bool puzzleSolved = false;
    private Vector3[] doorOpenPositions;

    public override void OnNetworkSpawn()
    {
        doorOpenPositions = new Vector3[doors.Length];
        for (int i = 0; i < doors.Length; i++) {
            doorOpenPositions[i] = doors[i].transform.position + doorOpenOffset;
        }

        keypadCanvas.SetActive(false);

        correctCode.OnValueChanged += (old, newVal) => wallCode.text = newVal.ToString();

        if (IsServer)
        {
            GenerateRandomCode();
            wallCode.text = correctCode.Value.ToString();
        }
    }

    void GenerateRandomCode()
    {
        correctCode.Value = Random.Range(1000, 10000);
    }

    void Update()
    {
        if (puzzleSolved)
        {
            MoveDoors();
            return;
        }

    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         OpenKeypad();
    //     }
    // }

    public void OnCloseButtonClicked()
    {
         CloseKeypad();
    }

    public void OpenKeypad()
    {
        isInteracting = true;
        keypadCanvas.SetActive(true);
        currentInput = "";
        UpdateDisplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;


            feedbackText.text = "";
    }

    public void CloseKeypad()
    {
        isInteracting = false;
        keypadCanvas.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UpdateDisplay()
    {

            codeDisplayText.text = currentInput;
    }


    public void OnNumberPressed(int number)
    {
        if (currentInput.Length < 4)
        {
            currentInput += number.ToString();
            UpdateDisplay();
        }
    }

    public void OnDeletePressed()
    {
        if (currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
            UpdateDisplay();
        }
    }

    public void OnSubmitPressed()
    {
        if (string.IsNullOrEmpty(currentInput)) return;
        SubmitCodeServerRpc(currentInput);
    }

    [Rpc(SendTo.Server)]
    void SubmitCodeServerRpc(string code)
    {
        bool isCorrect = code == correctCode.Value.ToString();
        CodeResultClientRpc(isCorrect);
    }

    [ClientRpc]
    void CodeResultClientRpc(bool success)
    {
        if (success)
        {
            puzzleSolved = true;
            feedbackText.text = "ACCESS GRANTED";
            feedbackText.color = Color.green;
            Invoke(nameof(CloseKeypad), 1f);
        }
        else
        {
            feedbackText.text = "ACCESS DENIED";
            feedbackText.color = Color.red;
            currentInput = "";
            UpdateDisplay();
            Invoke(nameof(ClearFeedback), 1f);
        }
    }

    void ClearFeedback()
    {
        feedbackText.text = "";
    }

    void MoveDoors()
    {

        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].transform.position = Vector3.MoveTowards(
                doors[i].transform.position,
                doorOpenPositions[i],
                doorMoveSpeed * Time.deltaTime
            );
        }
    }
}
