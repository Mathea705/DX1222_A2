using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameSceneManager : NetworkBehaviour
{

    [SerializeField] private GameObject playerPrefab;

    [SerializeField] private Transform[] spawnPoints;

    private Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
       
            SpawnAllPlayers();

        
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    void SpawnAllPlayers()
    {
        int spawnIndex = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayerForClient(client.ClientId, spawnIndex);
            spawnIndex++;
        }
    }

    void OnClientConnected(ulong clientId)
    {

        if (!spawnedPlayers.ContainsKey(clientId))
        {
            int spawnIndex = spawnedPlayers.Count;
            SpawnPlayerForClient(clientId, spawnIndex);
        }
    }

    void SpawnPlayerForClient(ulong clientId, int spawnIndex)
    {



        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int index = spawnIndex % spawnPoints.Length;
            spawnPosition = spawnPoints[index].position;
            spawnRotation = spawnPoints[index].rotation;
        }

 
        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        NetworkObject networkObject = playerInstance.GetComponent<NetworkObject>();

        if (networkObject != null)
        {
            networkObject.SpawnAsPlayerObject(clientId);
            spawnedPlayers[clientId] = playerInstance;

            FirstPersonController controller = playerInstance.GetComponent<FirstPersonController>();
            if (controller != null)
            {
                controller.SetPlayerIndex(spawnIndex);
            }
        }

    }


}
