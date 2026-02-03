using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Collections;

public class ThirdPuzzleManager : NetworkBehaviour
{
    private const int maxNum = 4;

    [Header("Player 1 Levers")]
    [SerializeField] private Lever[] levers1;

    [Header("Player 2 Levers")]
    [SerializeField] private Lever[] levers2;

    [Header("UI - Code Display")]
    [SerializeField] private Image[] code1Arrows;
    [SerializeField] private Image[] code2Arrows;
    [SerializeField] private Sprite upArrowSprite;
    [SerializeField] private Sprite downArrowSprite;

    [Header("Doors")]
    [SerializeField] private GameObject[] doors;
    [SerializeField] private Vector3 doorOpenOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float doorMoveSpeed = 2f;

    public string[] code1 = new string[maxNum];
    public string[] code2 = new string[maxNum];

    private NetworkVariable<FixedString64Bytes> networkCode1 = new NetworkVariable<FixedString64Bytes>();
    private NetworkVariable<FixedString64Bytes> networkCode2 = new NetworkVariable<FixedString64Bytes>();

    private bool player1Solved = false;
    private bool player2Solved = false;
    private bool puzzleComplete = false;
    private Vector3[] doorOpenPositions;

    public override void OnNetworkSpawn()
    {
        doorOpenPositions = new Vector3[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            doorOpenPositions[i] = doors[i].transform.position + doorOpenOffset;
        }

        // Subscribe to changes
        networkCode1.OnValueChanged += (old, newVal) => DisplayCode(code1Arrows, newVal.ToString());
        networkCode2.OnValueChanged += (old, newVal) => DisplayCode(code2Arrows, newVal.ToString());

        if (IsServer)
        {
            for (int i = 0; i < maxNum; i++)
            {
                code1[i] = GenerateState();
                code2[i] = GenerateState();
            }

            networkCode1.Value = string.Join(",", code1);
            networkCode2.Value = string.Join(",", code2);
        }

        
        bool isRed = NetworkManager.Singleton.LocalClientId == 0;
        Image[] myArrows = isRed ? code2Arrows : code1Arrows;
        Color arrowColor = isRed ? Color.blue : Color.red;

        foreach (Image arrow in myArrows)
        {
            arrow.color = arrowColor;
        }


        if (networkCode1.Value.Length > 0)
            DisplayCode(code1Arrows, networkCode1.Value.ToString());
        if (networkCode2.Value.Length > 0)
            DisplayCode(code2Arrows, networkCode2.Value.ToString());
    }

    void DisplayCode(Image[] arrows, string codeStr)
    {
        string[] states = codeStr.Split(',');
        for (int i = 0; i < arrows.Length && i < states.Length; i++)
        {
            arrows[i].sprite = states[i] == "Up" ? upArrowSprite : downArrowSprite;
        }
    }

    void Update()
    {
        if (puzzleComplete)
        {
            MoveDoors();
        }
    }

    public void OnLeverToggled(int playerSet, int leverIndex)
    {
        ToggleLeverServerRpc(playerSet, leverIndex);
    }

    [Rpc(SendTo.Server)]
    void ToggleLeverServerRpc(int playerSet, int leverIndex)
    {
        Lever[] targetLevers = playerSet == 1 ? levers1 : levers2;
        targetLevers[leverIndex].ToggleLever();

        ToggleLeverClientRpc(playerSet, leverIndex);

        CheckPuzzleCompletion();
    }

    [ClientRpc]
    void ToggleLeverClientRpc(int playerSet, int leverIndex)
    {
        if (IsServer) return;

        Lever[] targetLevers = playerSet == 1 ? levers1 : levers2;
        targetLevers[leverIndex].ToggleLever();
    }

    void CheckPuzzleCompletion()
    {
        player1Solved = CheckLeversMatch(levers1, code1);
        player2Solved = CheckLeversMatch(levers2, code2);

        if (player1Solved && player2Solved)
        {
            puzzleComplete = true;
            PuzzleSolvedClientRpc();
        }
    }

    bool CheckLeversMatch(Lever[] levers, string[] code)
    {
        for (int i = 0; i < levers.Length; i++)
        {
            if (levers[i].getState() != code[i])
            {
                return false;
            }
        }
        return true;
    }

    [ClientRpc]
    void PuzzleSolvedClientRpc()
    {
        puzzleComplete = true;

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

    string GenerateState()
    {
        return Random.Range(0, 2) == 0 ? "Up" : "Down";
    }
}
