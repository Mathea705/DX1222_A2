using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class FirstPuzzleManager : NetworkBehaviour
{
    [Header("Pressure Plates")]
    [SerializeField] private GameObject[] pressurePlates;

    [Header("Doors")]
    [SerializeField] private GameObject[] doors;
    [SerializeField] private Vector3 doorOpenOffset = new Vector3(0, 3f, 0);
    [SerializeField] private float doorMoveSpeed = 2f;

    private Dictionary<int, bool> plateStates = new Dictionary<int, bool>();

    private Vector3[] doorClosedPositions;
    private Vector3[] doorOpenPositions;
    private bool doorsMoving = false;

    public override void OnNetworkSpawn()
    {
        doorClosedPositions = new Vector3[doors.Length];
        doorOpenPositions = new Vector3[doors.Length];

        for (int i = 0; i < doors.Length; i++)
        {
            doorClosedPositions[i] = doors[i].transform.position;
            doorOpenPositions[i] = doors[i].transform.position + doorOpenOffset;
        }

        if (IsServer)
        {
            for (int i = 0; i < pressurePlates.Length; i++)
                plateStates[i] = false;
        }
    }


    public void PlateStateChanged(int plateId, bool isPressed)
    {
        if (!IsClient) return;
        PlateStateChangedServerRpc(plateId, isPressed);
    }

   [Rpc(SendTo.Server)]
    void PlateStateChangedServerRpc(int plateId, bool isPressed)
    {
        plateStates[plateId] = isPressed;
        CheckPuzzleState();
    }



    void CheckPuzzleState()
    {
        foreach (bool pressed in plateStates.Values)
        {
            if (!pressed)
                return;
        }

        PuzzleSolvedClientRpc();
    }


    [ClientRpc]
    void PuzzleSolvedClientRpc()
    {
        doorsMoving = true;
        // Debug.Log("solved.");
    }



    void Update()
    {
        if (!doorsMoving) return;

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
