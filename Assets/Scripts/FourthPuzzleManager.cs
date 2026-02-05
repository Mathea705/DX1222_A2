using UnityEngine;
using Unity.Netcode;

public class FourthPuzzleManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] spikes1;
    [SerializeField] private GameObject[] spikes2;
    [SerializeField] private float downOffset = -1.5f;
    [SerializeField] private float lerpSpeed = 5f;

    private Vector3[] spikes1Start;
    private Vector3[] spikes2Start;
    private bool spikes1Down = false;
    private bool spikes2Down = false;

    void Start()
    {
        spikes1Start = new Vector3[spikes1.Length];
        spikes2Start = new Vector3[spikes2.Length];

        for (int i = 0; i < spikes1.Length; i++)
            spikes1Start[i] = spikes1[i].transform.position;
        for (int i = 0; i < spikes2.Length; i++)
            spikes2Start[i] = spikes2[i].transform.position;
    }

    void Update()
    {
        LerpSpikes(spikes1, spikes1Start, spikes1Down);
        LerpSpikes(spikes2, spikes2Start, spikes2Down);
    }

    void LerpSpikes(GameObject[] spikes, Vector3[] startPos, bool down)
    {
        for (int i = 0; i < spikes.Length; i++)
        {
            Vector3 target = down ? startPos[i] + new Vector3(0, downOffset, 0) : startPos[i];
            spikes[i].transform.position = Vector3.Lerp(spikes[i].transform.position, target, lerpSpeed * Time.deltaTime);
        }
    }
    public void PressPanel1()
    {
        LowerSpikes1ServerRpc();
    }

    public void PressPanel2()
    {
        LowerSpikes2ServerRpc();
    }

    [Rpc(SendTo.Server)]
    void LowerSpikes1ServerRpc()
    {

        spikes1Down = true;
        spikes2Down = false;

        SyncSpikes1DownClientRpc();
    }

    [Rpc(SendTo.Server)]
    void LowerSpikes2ServerRpc()
    {
    
        spikes1Down = false;
        spikes2Down = true;
        SyncSpikes2DownClientRpc();
    }

    [ClientRpc]
    void SyncSpikes1DownClientRpc()
    {
        if (IsServer) return;
        spikes1Down = true;
        spikes2Down = false;
    }

    [ClientRpc]
    void SyncSpikes2DownClientRpc()
    {
        if (IsServer) return;
        spikes1Down = false;
        spikes2Down = true;
    }
}
